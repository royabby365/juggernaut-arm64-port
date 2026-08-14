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
                        splashGo.transform.position = new Vector3(0f, 0f, 5f);
                        // Render boot text as a Sprite on a SpriteRenderer. This avoids
                        // Shader.Find (usually null in headless) and UnityEngine.UI (stripped
                        // from IL2CPP). Font.GetCharacterInfo reads glyph pixels from the
                        // font's texture atlas and copies them onto a new Texture2D.
                        var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        if (font == null) font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/BootFont.ttf");
                        if (font != null)
                        {
                            string bootText = "Juggernaut\narm64 port build\n(boot scene - game content TBD)";
                            font.RequestCharactersInTexture(bootText, 48, FontStyle.Normal);
                            int fontSize = 48;
                            int lineH = 0;
                            string[] lines = bootText.Split('\n');
                            // Measure dimensions
                            foreach (var line in lines)
                                foreach (char c in line)
                                {
                                    CharacterInfo ci;
                                    if (font.GetCharacterInfo(c, out ci, fontSize))
                                        lineH = Mathf.Max(lineH, ci.glyphHeight);
                                }
                            int maxW = 1;
                            foreach (var line in lines)
                            {
                                int lw = 0;
                                foreach (char c in line)
                                {
                                    CharacterInfo ci;
                                    if (font.GetCharacterInfo(c, out ci, fontSize))
                                        lw += ci.advance;
                                }
                                maxW = Mathf.Max(maxW, lw);
                            }
                            int texW = Mathf.Max(maxW, 1);
                            int texH = Mathf.Max(lineH * lines.Length, 1);
                            var tex = new Texture2D(texW, texH, TextureFormat.ARGB32, false);
                            // Transparent background
                            var clr = new Color32(255, 255, 255, 0);
                            var pix = new Color32[texW * texH];
                            for (int i = 0; i < pix.Length; i++) pix[i] = clr;
                            tex.SetPixels32(pix);
                            // Draw each glyph onto the texture
                            int yOff = texH - lineH;
                            var fontTex = font.material.mainTexture as Texture2D;
                            foreach (var line in lines)
                            {
                                int xOff = 0;
                                foreach (char c in line)
                                {
                                    CharacterInfo ci;
                                    if (font.GetCharacterInfo(c, out ci, fontSize) && fontTex != null)
                                    {
                                        var gr = ci.glyphRect;
                                        var glyphPix = fontTex.GetPixels32(gr.x, gr.y, gr.width, gr.height);
                                        // Clamp to tex bounds
                                        int destX = Mathf.Clamp(xOff + (int)gr.x, 0, texW - 1);
                                        int destY = Mathf.Clamp(yOff + (int)gr.y, 0, texH - 1);
                                        int copyW = Mathf.Min(gr.width, texW - destX);
                                        int copyH = Mathf.Min(gr.height, texH - destY);
                                        for (int gy = 0; gy < copyH; gy++)
                                            for (int gx = 0; gx < copyW; gx++)
                                            {
                                                int si = gy * gr.width + gx;
                                                int di = (destY + gy) * texW + (destX + gx);
                                                if (glyphPix[si].a > 0)
                                                    pix[di] = glyphPix[si];
                                            }
                                        xOff += ci.advance;
                                    }
                                }
                                yOff -= lineH;
                            }
                            tex.SetPixels32(pix);
                            tex.Apply();
                            tex.filterMode = FilterMode.Bilinear;
                            var sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH),
                                new Vector2(0.5f, 0.5f), 100f);
                            var sr = splashGo.AddComponent<SpriteRenderer>();
                            sr.sprite = sprite;
                            // Tint white (default material is always-included Sprites-Default)
                            sr.color = Color.white;
                        }
                        else
                                                {
                                                    Debug.LogWarning("[BuildScript] No font – boot splash text will not render");
                                                }

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