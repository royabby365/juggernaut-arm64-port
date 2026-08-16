using UnityEngine;

public class AppLoader : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EarlyBootstrap()
    {
        // Ensure AtlasManager does not hang looking for non-existent bundles if unconfigured
        Globals.DebugDoNotLoadAtlases = true;
        Globals.DebugStartMenuSimple = true;
        // Our local Resources are unencrypted JSON, not the encrypted/compressed admin format
        Globals.UseEncryptedJsonAdmin = false;
    }

    private void Start()
    {
        Debug.Log("[AppLoader] Initializing Juggernaut boot...");

        var mainMenuGo = new GameObject("MainMenuHost");
        mainMenuGo.AddComponent<MainMenu>();
    }
}
