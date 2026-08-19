// Juggernaut arm64 port - bundle loader bridge.
//
// The original Unity 4.x game loaded AssetBundles from inside the APK with
// "jar:file://" URLs. Unity 2021's WWW class no longer reads those URLs
// reliably - it returns truncated data, so every load fails with
// "The 'Memory' file is not a valid AssetBundle" / "Failed to decompress
// data for the AssetBundle 'Memory'" (Memory is Unity's internal name for
// LoadFromMemory, not a real file) and the game falls back to placeholders.
//
// Fix: read bundles out of the APK ourselves via the Android AssetManager
// (the platform API for exactly this) into Application.persistentDataPath,
// then hand WWW a plain "file://" URL that Unity 2021 handles perfectly.
// Extraction happens lazily per bundle on first request and is cached in
// memory, so a running session only pays the I/O cost once per file.
//
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class JugBundles
{
    private static string _cacheDir;
    private static readonly Dictionary<string, string> _urlCache = new Dictionary<string, string>();

    private static string CacheDir
    {
        get
        {
            if (_cacheDir == null)
                _cacheDir = Path.Combine(Application.persistentDataPath, "bundles");
            return _cacheDir;
        }
    }

    /// <summary>
    /// Returns a loadable URL for the bundle named <paramref name="name"/>
    /// (e.g. "characters/1/1", "scenes/1", "resources/locations/1181_2k").
    /// The ".unity3d" suffix is appended automatically, matching the old
    /// GetAssetBundlePath contract.
    /// </summary>
    public static string LocalUrl(string name)
    {
        string key = name + ".unity3d";
        string url;
        if (_urlCache.TryGetValue(key, out url))
            return url;

        // Placeholder mode (DebugNoBundles): the shipped bundles are Unity 4.x
        // content that Unity 2021 cannot load anyway, and the game code falls
        // back to code-generated placeholders on load failure. Skip extraction
        // entirely to avoid pointless I/O on every missing bundle.
        if (Globals.DebugNoBundles)
        {
            url = "jar:file://" + Application.dataPath + "!/assets/android/" + key;
            _urlCache[key] = url;
            return url;
        }

        // Path of the bundle inside the APK: assets/android/<name>.unity3d.
        // AssetManager.open() takes paths relative to the assets/ root.
        string assetPath = "android/" + key;
        string localFile = Path.Combine(CacheDir, key.Replace('/', Path.DirectorySeparatorChar));

        try
        {
            if (!File.Exists(localFile))
            {
                byte[] data = ReadAsset(assetPath);
                if (data != null && data.Length > 0)
                {
                    string dir = Path.GetDirectoryName(localFile);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);
                    File.WriteAllBytes(localFile, data);
                    Debug.Log("[JugBundles] extracted " + key + " (" + data.Length + " bytes)");
                }
                else
                {
                    // Asset missing from APK - fall back to the legacy URL so
                    // downstream error handling behaves exactly as before.
                    Debug.LogWarning("[JugBundles] asset not readable: " + assetPath);
                    url = "jar:file://" + Application.dataPath + "!/assets/android/" + key;
                    _urlCache[key] = url;
                    return url;
                }
            }
            url = "file://" + localFile;
            _urlCache[key] = url;
            return url;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[JugBundles] LocalUrl failed for " + key + ": " + ex);
            url = "jar:file://" + Application.dataPath + "!/assets/android/" + key;
            _urlCache[key] = url;
            return url;
        }
    }

    private static byte[] ReadAsset(string assetPath)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity == null)
                {
                    Debug.LogWarning("[JugBundles] currentActivity is null");
                    return null;
                }
                using (AndroidJavaObject assets = activity.Call<AndroidJavaObject>("getAssets"))
                using (AndroidJavaObject stream = assets.Call<AndroidJavaObject>("open", assetPath))
                {
                    // API 33+: InputStream.readAllBytes() - return-side marshaling
                    // only, avoids the broken byte[] argument conversion.
                    try
                    {
                        byte[] all = stream.Call<byte[]>("readAllBytes");
                        if (all != null)
                            return all;
                    }
                    catch (Exception ex1)
                    {
                        Debug.LogWarning("[JugBundles] readAllBytes unavailable: " + ex1.Message);
                    }
                    // Fallback: read loop with sbyte[] (byte[] args are obsolete
                    // and unreliable in IL2CPP JNI marshaling).
                    sbyte[] sbuf = new sbyte[65536];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int n;
                        while ((n = stream.Call<int>("read", sbuf)) > 0)
                        {
                            byte[] chunk = new byte[n];
                            Buffer.BlockCopy(sbuf, 0, chunk, 0, n);
                            ms.Write(chunk, 0, n);
                        }
                        return ms.ToArray();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // FileNotFound is expected for bundles that don't exist.
            Debug.LogWarning("[JugBundles] ReadAsset failed for " + assetPath + ": " + ex.Message);
            return null;
        }
#else
        // Editor / non-Android: read straight from the StreamingAssets folder.
        try
        {
            string rel = assetPath.StartsWith("android/") ? assetPath.Substring("android/".Length) : assetPath;
            string p = Path.Combine(Application.streamingAssetsPath, "android", rel);
            if (File.Exists(p))
                return File.ReadAllBytes(p);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[JugBundles] ReadAsset(editor) failed for " + assetPath + ": " + ex.Message);
        }
        return null;
#endif
    }
}
