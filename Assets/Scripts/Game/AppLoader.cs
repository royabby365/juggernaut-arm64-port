using UnityEngine;

public class AppLoader : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EarlyBootstrap()
    {
        Debug.Log("[AppLoader] EarlyBootstrap: Setting debug flags BEFORE scene load");
        // UI textures exported to Resources/__textures/
        // Runtime atlas fallback creates Atlas components on demand
        Globals.DebugDoNotLoadAtlases = false;
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

        var mainMenuGo = new GameObject(Globals.LocationGameObjectMainMenu);
        mainMenuGo.AddComponent<MainMenu>();
        // Add SaveLoadProtobuf component required by CreateServerData for protobuf path
        mainMenuGo.AddComponent<SaveLoadProtobuf>();

        // Hide the diagnostic BootSplash text once game UI is ready
        Invoke(nameof(HideBootSplash), 1.5f);
    }

    private void HideBootSplash()
    {
        var splash = GameObject.Find("BootSplashText");
        if (splash != null)
        {
            Debug.Log("[AppLoader] Hiding BootSplash diagnostic text");
            splash.SetActive(false);
        }
    }
}
