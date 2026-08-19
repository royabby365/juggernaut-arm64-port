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
}
