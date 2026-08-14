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
using UnityEngine.UI;
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
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64; // dual-arch test build for x86_64 emulator
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.bundleVersion = "2.4.3";
        PlayerSettings.productName = "Juggernaut";
        PlayerSettings.Android.bundleVersionCode = 1;

        // ---- 2. Guarantee a scene to pack (Unity requires >= 1 scene)
        if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0)
        {
            Debug.Log("[BuildScript] No scenes in EditorBuildSettings - generating boot splash scene.");
            string sceneDir = "Assets/Scenes";
            Directory.CreateDirectory(sceneDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.10f);
            camGo.AddComponent<AudioListener>();

            var splashGo = new GameObject("BootSplashText");
            var canvas = splashGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Add the text as a child of the Canvas so it renders through the
            // uGUI pipeline (no dynamic-font-atlas / GUI/Text Shader issues).
            var textGo = new GameObject("BootText");
            textGo.transform.SetParent(splashGo.transform, false);
            var text = textGo.AddComponent<Text>();
            text.text = "Juggernaut\narm64 port build\n(boot scene - game content TBD)";
            text.fontSize = 48;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            // Assign a font — fallback chain ensures glyphs render (not blocks).
            var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/BootFont.ttf");
            if (font != null)
            {
                text.font = font;
            }
            else
            {
                Debug.LogWarning("[BuildScript] No font available for boot splash text; if missing, glyphs will be blocks");
            }
            // UI Text on Canvas needs no MeshRenderer/Shader.Find —
            // Text generates its own vertex geometry and uses the font's own
            // internal material, avoiding the dynamic-atlas-less-gui-block problem.

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