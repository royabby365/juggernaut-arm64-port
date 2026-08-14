// Juggernaut arm64 port - headless build entry for GameCI / Unity batchmode.
// Invoked by game-ci/unity-builder with buildMethod: BuildScript.BuildPlayer
//
// Forces IL2CPP + arm64-v8a, guarantees a playable boot scene (the original
// Unity 4.x scene lives inside Assets/GameData/mainData and cannot be read
// directly by Unity 2021 - we generate a lightweight splash scene so the
// pipeline produces a real, installable APK end to end).

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildScript
{
    public static void BuildPlayer()
    {
        string outputPath = "build/juggernaut-arm64.apk";

        // ---- Platform/backend: arm64-v8a + IL2CPP (this is the actual
        // ---- "64-bit" switch; Mono is 32-bit only on modern Unity Android)
        EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.Build.NamedBuildTarget.Android, BuildTarget.Android);
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64; // Dual-arch for local emulator testing
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.bundleVersion = "2.4.3";
        PlayerSettings.productName = "Juggernaut";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        // ---- 2. Guarantee a scene to pack (unconditionally regenerate
        // to ensure latest font / TextMesh configuration is baked in)
        {
            Debug.Log("[BuildScript] Generating clean boot splash scene.");
            string sceneDir = "Assets/Scenes";
            Directory.CreateDirectory(sceneDir);
            string scenePath = sceneDir + "/BootSplash.unity";
            if (File.Exists(scenePath)) File.Delete(scenePath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.10f);
            camGo.AddComponent<AudioListener>();

            var splashGo = new GameObject("BootSplashText");
            splashGo.transform.position = new Vector3(0f, 0f, 5f);
            var text = splashGo.AddComponent<TextMesh>();
            text.alignment = TextAlignment.Center;
            text.anchor = TextAnchor.MiddleCenter;
            text.text = "Juggernaut\narm64 port build\n(boot scene - game content TBD)";
            text.fontSize = 32;
            text.characterSize = 0.04f;
            var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/BootFont.ttf");
            if (font != null) text.font = font;
            // CRITICAL: do NOT override MeshRenderer.sharedMaterial. TextMesh
            // auto-assigns the font's default material (always-available Font
            // shader). Overriding it with Shader.Find(...) can produce a
            // null-shader material in headless builds, killing all text rendering.

            EditorSceneManager.SaveScene(scene, sceneDir + "/BootSplash.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(sceneDir + "/BootSplash.unity", true) };
        }

        // ---- 3. Build
        Directory.CreateDirectory("build");
        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
            target = BuildTarget.Android,
            locationPathName = Path.GetFullPath(outputPath),
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log("[BootScript] Build result: " + summary.result + " size=" + summary.totalSize + " errs=" + summary.totalErrors);
        if (summary.result != BuildResult.Succeeded)
        {
            foreach (var step in report.steps)
            {
                Debug.LogError($"[BootScript] step {step.name} depth={step.depth} msgs={step.messages.Length}");
                foreach (var msg in step.messages)
                    Debug.LogError($"[BootScript]   {msg.type}: {msg.content}");
            }
            throw new Exception("Build FAILED with " + summary.totalErrors + " errors");
        }
        Debug.Log("[BootScript] APK written to " + Path.GetFullPath(outputPath));
    }
}