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
        // Our local Resources are plaintext JSON (not encrypted admin format)
        // UseJsonAdmin=true chooses JSON path over protobuf
        // UseEncryptedJsonAdmin=false chooses plaintext over encrypted/compressed
        Globals.UseJsonAdmin = true;
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
