// Quick diagnostic: dump the contents of an AssetBundle so we can see
// (a) whether Unity 2021 can load the Unity 5.5 bundles at all, and
// (b) what asset names are actually inside them (the game looks for
//     "2_iOS", "1", etc.).
using System;
using UnityEditor;
using UnityEngine;

public static class BundleInspector
{
    public static void DumpBundle()
    {
        string rel = Environment.GetEnvironmentVariable("BUNDLE_REL") ?? "scenes/2_iOS.unity3d";
        string path = "Assets/StreamingAssets/android/" + rel;
        Debug.Log("[BundleInspector] trying to load: " + path);

        AssetBundle ab = null;
        try
        {
            ab = AssetBundle.LoadFromFile(path);
        }
        catch (Exception ex)
        {
            Debug.LogError("[BundleInspector] LoadFromFile threw: " + ex);
            return;
        }

        if (ab == null)
        {
            Debug.LogError("[BundleInspector] LoadFromFile returned NULL (unloadable by Unity 2021)");
            return;
        }

        Debug.Log("[BundleInspector] LOAD OK! bundle name=" + ab.name);
        Debug.Log("[BundleInspector] mainAsset=" + (ab.mainAsset != null ? ab.mainAsset.name + " (" + ab.mainAsset.GetType().Name + ")" : "NULL"));

        string[] names = ab.GetAllAssetNames();
        Debug.Log("[BundleInspector] asset count=" + names.Length);
        foreach (string n in names)
        {
            try
            {
                var obj = ab.LoadAsset(n);
                Debug.Log("[BundleInspector]   ASSET: " + n + " -> " + (obj != null ? obj.GetType().Name : "NULL"));
            }
            catch (Exception ex)
            {
                Debug.LogError("[BundleInspector]   ASSET: " + n + " -> LOAD ERROR: " + ex.Message);
            }
        }

        ab.Unload(false);
        Debug.Log("[BundleInspector] DONE");
    }

    public static void IconTest()
    {
        var legacy = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AppIcon/juggernaut_icon.png");
        Debug.Log("[IconTest] legacy loaded: " + (legacy != null ? legacy.name + " " + legacy.width + "x" + legacy.height : "NULL"));
        if (legacy == null) return;

        var legacyIcons = PlayerSettings.GetPlatformIcons(UnityEditor.Build.NamedBuildTarget.Android,
            UnityEditor.Android.AndroidPlatformIconKind.Legacy);
        Debug.Log("[IconTest] legacy slots: " + (legacyIcons != null ? legacyIcons.Length : 0));
        foreach (var slot in legacyIcons)
            slot.SetTextures(new[] { legacy });
        PlayerSettings.SetPlatformIcons(UnityEditor.Build.NamedBuildTarget.Android,
            UnityEditor.Android.AndroidPlatformIconKind.Legacy, legacyIcons);
        Debug.Log("[IconTest] legacy set OK");

        var fg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AppIcon/juggernaut_adaptive_fg.png");
        var bg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AppIcon/juggernaut_adaptive_bg.png");
        Debug.Log("[IconTest] fg=" + (fg != null ? fg.width + "x" + fg.height : "NULL") + " bg=" + (bg != null ? bg.width + "x" + bg.height : "NULL"));
        var adaptive = PlayerSettings.GetPlatformIcons(UnityEditor.Build.NamedBuildTarget.Android,
            UnityEditor.Android.AndroidPlatformIconKind.Adaptive);
        Debug.Log("[IconTest] adaptive slots: " + (adaptive != null ? adaptive.Length : 0));
        if (adaptive != null && adaptive.Length >= 2 && fg != null && bg != null)
        {
            try
            {
                adaptive[0].SetTextures(new[] { fg, fg });
                Debug.Log("[IconTest] adaptive fg set OK");
            }
            catch (Exception e)
            {
                Debug.LogError("[IconTest] adaptive fg FAILED: " + e);
            }
            try
            {
                adaptive[1].SetTextures(new[] { bg, bg });
                Debug.Log("[IconTest] adaptive bg set OK");
            }
            catch (Exception e)
            {
                Debug.LogError("[IconTest] adaptive bg FAILED: " + e);
            }
            PlayerSettings.SetPlatformIcons(UnityEditor.Build.NamedBuildTarget.Android,
                UnityEditor.Android.AndroidPlatformIconKind.Adaptive, adaptive);
            Debug.Log("[IconTest] adaptive set OK (fg=" + fg.name + " bg=" + bg.name + ")");
        }
        Debug.Log("[IconTest] DONE");
    }
}
