using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Render the skinned warrior to verify skeletal rig + idle animation.</summary>
public static class SkinnedPreview
{
    public static void Render()
    {
        string outPath = "/tmp/skinned_preview.png";
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-previewOut") outPath = args[i + 1];

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Cam");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
        cam.transform.position = new Vector3(2.2f, 1.1f, -2.4f);
        cam.transform.LookAt(new Vector3(0f, 1.1f, 0f));
        cam.fieldOfView = 40f;
        camGo.AddComponent<AudioListener>();

        var lightGo = new GameObject("Key");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

        var built = SkinnedRigBuilder.Build("__anim/warrior_rig", "__anim/warrior_clips", "Warrior_Skinned");
        if (built != null)
        {
            built.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            Debug.Log("[SkinnedPreview] built OK");
        }
        else
        {
            Debug.LogWarning("[SkinnedPreview] build null");
        }

        // Advance a few frames so the clip player samples real bone motion
        for (int i = 0; i < 30; i++)
        {
            foreach (var p in Resources.FindObjectsOfTypeAll<LegacyClipPlayer>())
                if (p != null && p.isActiveAndEnabled)
                    p.playOnStart = true;
        }

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SkinnedPreview.unity");

        var rt = new RenderTexture(1024, 768, 24);
        var old = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1024, 768, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1024, 768), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = old;
        File.WriteAllBytes(outPath, ImageConversion.EncodeToPNG(tex));
        Debug.Log($"[SkinnedPreview] Wrote {outPath}");
    }
}
