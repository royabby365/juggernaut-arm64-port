using UnityEngine;

public class AppLoader : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EarlyBootstrap()
    {
        Debug.Log("[AppLoader] EarlyBootstrap: Setting debug flags BEFORE scene load");
        // Ensure AtlasManager does not hang looking for non-existent bundles if unconfigured
        Globals.DebugDoNotLoadAtlases = true;
        Globals.DebugStartMenuSimple = true;
        // Our local Resources are unencrypted JSON, not the encrypted/compressed admin format
        Globals.UseJsonAdmin = false;
        Globals.UseEncryptedJsonAdmin = false;
        Debug.Log($"[AppLoader] EarlyBootstrap: UseJsonAdmin={Globals.UseJsonAdmin}, UseEncryptedJsonAdmin={Globals.UseEncryptedJsonAdmin}");
    }

    private void Start()
    {
        Debug.Log("[AppLoader] Initializing Juggernaut boot...");

        var mainMenuGo = new GameObject("MainMenuHost");
        mainMenuGo.AddComponent<MainMenu>();
        // Add SaveLoadProtobuf component required by CreateServerData for protobuf path
        mainMenuGo.AddComponent<SaveLoadProtobuf>();
    }
}
