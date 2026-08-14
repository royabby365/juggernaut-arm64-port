using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime-only boot text renderer. Sneaks past the Editor assembly not
/// referencing UnityEngine.UI.dll by living in a runtime assembly instead.
/// Attached by BuildScript during scene generation.
/// </summary>
public class BootTextInitializer : MonoBehaviour
{
    public string BootText = "Juggernaut\narm64 port build\n(boot scene - game content TBD)";
    public int FontSize = 48;

    private void Start()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var textGo = new GameObject("BootText");
        textGo.transform.SetParent(transform, false);
        var text = textGo.AddComponent<Text>();
        text.text = BootText;
        text.fontSize = FontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        // Font with realistic fallback chain
        var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.Load<Font>("BootFont");
        if (font != null) text.font = font;
        else Debug.LogWarning("[BootTextInitializer] No font available – text may render as blocks");
    }
}