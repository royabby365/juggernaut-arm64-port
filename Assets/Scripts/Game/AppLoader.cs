using UnityEngine;

public class AppLoader : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("[AppLoader] Initializing Juggernaut boot...");

        // Ensure AtlasManager does not hang looking for non-existent bundles if unconfigured
        Globals.DebugDoNotLoadAtlases = true;
        Globals.DebugStartMenuSimple = true;
        // Our local Resources are unencrypted JSON, not the encrypted/compressed admin format
        Globals.UseEncryptedJsonAdmin = false;

        var mainMenuGo = new GameObject("MainMenuHost");
        mainMenuGo.AddComponent<MainMenu>();
    }
}
