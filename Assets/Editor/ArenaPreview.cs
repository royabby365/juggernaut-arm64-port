// Juggernaut - Arena + Character scene preview (editor batchmode).
// Assembles the baked first-arena geometry (Assets/Models/__baked_arena) and the
// baked bind-pose blue warrior (Assets/Models/__baked), textures everything with
// the extracted _ds/atlas PNGs, and renders a screenshot for visual verification.
//
// Usage (batchmode, under xvfb - nographics renders blank):
//   Unity -batchmode -quit -projectPath . -executeMethod ArenaPreview.Render \
//        -previewOut /tmp/arena_preview.png
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ArenaPreview
{
    public static void Render()
    {
        string outPath = "/tmp/arena_preview.png";
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-previewOut") outPath = args[i + 1];
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera - close-up of character (verify assembly detail)
        var camGo = new GameObject("Arena Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
        cam.fieldOfView = 40f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 500f;
        cam.transform.position = new Vector3(4.2f, 2.2f, -4.6f);
        cam.transform.LookAt(new Vector3(0f, 1.1f, 0f));
        camGo.AddComponent<AudioListener>();

        // Lighting
        var key = new GameObject("Key Light");
        var keyL = key.AddComponent<Light>();
        keyL.type = LightType.Directional; keyL.intensity = 1.4f;
        key.transform.rotation = Quaternion.Euler(45f, -40f, 0f);

        var fill = new GameObject("Fill Light");
        var fillL = fill.AddComponent<Light>();
        fillL.type = LightType.Directional; fillL.intensity = 0.55f;
        fill.transform.rotation = Quaternion.Euler(25f, 140f, 0f);

        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");

        // ---- Arena geometry ----
        var arenaRoot = new GameObject("Arena_1_Colosseum");
        int arenaParts = BuildParts(arenaRoot.transform, "Assets/Models/__baked_arena_1", "Assets/Resources/__textures", shader, skipNames: null);

        // ---- Character at arena center, facing camera (+z toward viewer) ----
        var charRoot = new GameObject("Blue_Warrior_Pve1");
        charRoot.transform.position = new Vector3(0f, 0f, 0f);
        charRoot.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // A-pose faces +z; turn to face camera
        int charParts = BuildParts(charRoot.transform, "Assets/Models/__baked", "Assets/Resources/__textures", shader,
            skipNames: new HashSet<string> { "helm", "4_torso" });

        Debug.Log($"[ArenaPreview] arena={arenaParts} character={charParts}");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/ArenaPreview.unity");

        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        var tex = RenderToTexture(cam, 1280, 720);
        if (tex != null)
        {
            var png = ImageConversion.EncodeToPNG(tex);
            File.WriteAllBytes(outPath, png);
            Debug.Log($"[ArenaPreview] Wrote {outPath} ({png.Length} bytes)");
        }
        else
        {
            Debug.LogError("[ArenaPreview] render failed");
        }
    }

    private static int BuildParts(Transform parent, string objDir, string texDir, Shader shader, HashSet<string> skipNames)
    {
        string[] files = Directory.GetFiles(objDir, "*.obj");
        Array.Sort(files);
        int loaded = 0;
        foreach (string objPath in files)
        {
            string name = Path.GetFileNameWithoutExtension(objPath);
            if (skipNames != null && skipNames.Contains(name)) continue;

            AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(objPath) as ModelImporter;
            if (importer != null)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.SaveAndReimport();
            }
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            if (mesh == null) continue;

            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();

            // MTL map_Kd line tells us the exact texture path
            Texture2D tex = null;
            string mtlPath = Path.ChangeExtension(objPath, ".mtl");
            if (File.Exists(mtlPath))
            {
                foreach (var line in File.ReadAllLines(mtlPath))
                {
                    if (line.StartsWith("map_Kd "))
                    {
                        string rel = line.Substring(7).Trim();
                        // MTL written with texDir-relative path like ../__textures/01_tile.png
                        string tp = Path.GetFullPath(Path.Combine(objDir, rel));
                        if (File.Exists(tp))
                        {
                            // Convert to project-relative asset path
                            string projRel = ToAssetPath(tp);
                            if (projRel != null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>(projRel);
                        }
                        break;
                    }
                }
            }
            if (tex == null)
            {
                string[] cands = { name + "_ds", name };
                foreach (var cand in cands)
                {
                    string tp = texDir + "/" + cand + ".png";
                    if (File.Exists(tp)) { tex = AssetDatabase.LoadAssetAtPath<Texture2D>(tp); if (tex != null) break; }
                }
            }
            // Arena scene convention: base geometry has null materials in the
            // bundle; map by mesh role so the floor/walls aren't flat white.
            if (tex == null)
            {
                string roleTex = null;
                if (name == "Plane001") roleTex = "01_tile";         // base floor
                else if (name == "Plane002") roleTex = "arena_01_bg_02"; // side wall
                else if (name == "center") roleTex = "arena_08_floor_decals";
                else if (name == "background") roleTex = "arena_01_bg_01";
                if (roleTex != null)
                {
                    string tp = texDir + "/" + roleTex + ".png";
                    if (File.Exists(tp)) tex = AssetDatabase.LoadAssetAtPath<Texture2D>(tp);
                }
            }
            var mat = new Material(shader != null ? shader : Shader.Find("Standard"));
            if (tex != null)
            {
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (shader != null && shader.name == "Standard" && mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                // Repeat the tile texture across large base planes
                if (name == "Plane001" || name == "floor" || name == "Plane003")
                {
                    if (mat.HasProperty("_MainTex_ST")) mat.SetTextureScale("_MainTex", new Vector2(12f, 12f));
                    if (mat.HasProperty("_BaseMap_ST")) mat.SetTextureScale("_BaseMap", new Vector2(12f, 12f));
                }
            }
            mr.sharedMaterial = mat;
            loaded++;
        }
        return loaded;
    }

    private static string ToAssetPath(string fullPath)
    {
        string proj = Path.GetFullPath(Directory.GetCurrentDirectory());
        fullPath = Path.GetFullPath(fullPath);
        if (fullPath.StartsWith(proj))
            return fullPath.Substring(proj.Length).TrimStart('/', '\\');
        return null;
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
