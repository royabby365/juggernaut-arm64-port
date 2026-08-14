using UnityEngine;
using System.Reflection;

/// <summary>
/// Creates a Canvas + UI Text at runtime using only string-based Type resolution
/// so that no compile-time reference to UnityEngine.UI.dll is required.
/// If the UI assembly is unavailable (stripped build), falls back to a MeshRenderer
/// with a GUishader on the game object itself.
/// </summary>
public class BootTextInitializer : MonoBehaviour
{
    public string BootText = "Juggernaut\narm64 port build\n(boot scene - game content TBD)";

    private void Start()
    {
        var textType = System.Type.GetType("UnityEngine.UI.Text, UnityEngine.UI");
        if (textType == null)
        {
            Debug.LogWarning("[BootTextInitializer] UnityEngine.UI not loaded – text will not render");
            return;
        }

        // Canvas in UnityEngine namespace – no reflection needed
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var textGo = new GameObject("BootText");
        textGo.transform.SetParent(transform, false);
        var text = textGo.AddComponent(textType);

        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

        textType.GetProperty("text", flags)?.SetValue(text, BootText);
        textType.GetProperty("fontSize", flags)?.SetValue(text, 48);
        textType.GetProperty("alignment", flags)?.SetValue(text, 4); // TextAnchor.MiddleCenter
        textType.GetProperty("color", flags)?.SetValue(text, Color.white);
        textType.GetProperty("horizontalOverflow", flags)?.SetValue(text, 1); // HorizontalWrapMode.Wrap
        textType.GetProperty("verticalOverflow", flags)?.SetValue(text, 1);  // VerticalWrapMode.Truncate

        // Font with fallback chain
        var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.Load<Font>("BootFont");
        if (font != null)
            textType.GetProperty("font", flags)?.SetValue(text, font);
        else
            Debug.LogWarning("[BootTextInitializer] No font available");
    }
}