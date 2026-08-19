// Juggernaut - Character preview renderer (editor batchmode).
// Assembles the baked bind-pose character parts from Assets/Models/__baked,
// textures them with the extracted _ds PNGs, and renders a screenshot so we
// can visually verify the extracted meshes/textures WITHOUT building an APK.
//
// Usage (batchmode):
//   Unity -batchmode -quit -projectPath . -executeMethod CharacterPreview.Render \
//        -previewOut /tmp/char_preview.png
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CharacterPreview
{
    public static void Render()
    {
        string outPath = "/tmp/char_preview.png";
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-previewOut") outPath = args[i + 1];
        }

        string bakedDir = "Assets/Models/__baked";
        string texDir = "Assets/Resources/__textures";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGo = new GameObject("Preview Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.12f, 0.16f);
        cam.transform.position = new Vector3(2.4f, 1.0f, -2.6f);
        cam.transform.rotation = Quaternion.LookRotation(new Vector3(-0.55f, -0.05f, 0.83f));
        cam.fieldOfView = 40f;
        camGo.AddComponent<AudioListener>();

        // Light
        var lightGo = new GameObject("Key Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        var fillGo = new GameObject("Fill Light");
        var fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.4f;
        fillGo.transform.rotation = Quaternion.Euler(30f, 150f, 0f);

        // Find a shader that works headless with Standard materials.
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");

        // Assemble character root
        var root = new GameObject("Juggernaut_BindPose_BlueWarPve1");
        root.transform.position = Vector3.zero;

        string[] objFiles = Directory.GetFiles(bakedDir, "*.obj");
        Array.Sort(objFiles);
        int loaded = 0, skipped = 0;
        foreach (string objPath in objFiles)
        {
            // The redundant 'helm.obj' (mesh at wrong origin) is skipped:
            // GO19 'helm' bakes to origin; GO97 'blue_war_m_pve_1_helm' is the
            // correctly-positioned helm part.
            string name = Path.GetFileNameWithoutExtension(objPath);
            if (name == "helm" || name == "4_torso") { skipped++; continue; }

            // Import mesh via Unity's OBJ importer
            AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(objPath) as ModelImporter;
            if (importer != null)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.SaveAndReimport();
            }
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            if (mesh == null) { skipped++; continue; }

            var go = new GameObject(name);
            go.transform.SetParent(root.transform);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();

            // Material: use matching _ds texture
            var mat = new Material(shader != null ? shader : Shader.Find("Standard"));
            Texture2D tex = null;
            string texName = name + "_ds";
            string texPath = texDir + "/" + texName + ".png";
            if (File.Exists(texPath)) tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null && mat.HasProperty("_MainTex"))
            {
                mat.mainTexture = tex;
                mat.SetTexture("_MainTex", tex);
            }
            // Standard shader: set albedo from texture (Unlit/Texture uses _MainTex directly)
            if (shader != null && shader.name == "Standard" && tex != null && mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", tex);
            mr.sharedMaterial = mat;
            loaded++;
        }
        Debug.Log($"[CharacterPreview] loaded={loaded} skipped={skipped} objs={objFiles.Length}");

        // Framing: compute bounds and adjust camera
        var bounds = new Bounds(Vector3.zero, Vector3.one);
        bool any = false;
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            var b = mf.sharedMesh.bounds;
            var center = mf.transform.TransformPoint(b.center);
            var size = Vector3.Scale(b.size, mf.transform.lossyScale);
            if (!any) { bounds = new Bounds(center, size); any = true; }
            else bounds.Encapsulate(new Bounds(center, size));
        }
        if (any)
        {
            float r = bounds.extents.magnitude;
            cam.transform.position = bounds.center + new Vector3(1.3f, 0.35f, -1.5f) * r * 1.9f;
            cam.transform.LookAt(bounds.center + new Vector3(0f, 0.25f, 0f));
        }

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/CharacterPreview.unity");

        // Render
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        var camTex = RenderToTexture(cam, 1024, 768);
        if (camTex != null)
        {
            var png = ImageConversion.EncodeToPNG(camTex);
            File.WriteAllBytes(outPath, png);
            Debug.Log($"[CharacterPreview] Wrote {outPath} ({png.Length} bytes)");
        }
        else
        {
            // Fallback: force a frame then capture
            cam.Render();
            ScreenCapture.CaptureScreenshot(outPath);
            Debug.Log("[CharacterPreview] fallback capture issued");
        }
    }

    private static Texture2D RenderToTexture(Camera cam, int w, int h)
    {
        var rt = new RenderTexture(w, h, 24);
        var old = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        cam.targetTexture = old;
        rt.Release();
        return tex;
    }
}
