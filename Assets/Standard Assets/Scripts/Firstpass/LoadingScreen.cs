using System.Collections;
using UnityEngine;

/// <summary>
/// Full-screen LOADING overlay drawn via OnGUI on its own DontDestroyOnLoad
/// GO (same survival trick as BattleUI — OnGUI on scene GOs is unreliable in
/// this project). Shown before heavy synchronous work (arena build, rig spawn)
/// and hidden once the caller is done. Callers must defer the heavy work by
/// one frame (yield return null) after Show() so the loading frame renders.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    private static LoadingScreen _instance;
    public static LoadingScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("__loading_screen");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<LoadingScreen>();
            }
            return _instance;
        }
    }

    public static bool Active = false;

    private Texture2D _tex;
    private float _t;

    public static void Show()
    {
        Active = true;
        Instance._t = 0f;
    }

    public static void Hide()
    {
        Active = false;
    }

    private void Awake()
    {
        _tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        _tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        _tex.Apply();
        _tex.hideFlags = HideFlags.HideAndDontSave;
    }

    private void Update()
    {
        if (Active) _t += Time.deltaTime;
    }

    private void OnGUI()
    {
        if (!Active) return;

        float w = Screen.width, h = Screen.height;

        // Full-screen dark backdrop (covers the blue/grey standby).
        GUI.color = new Color(0.04f, 0.05f, 0.08f, 1f);
        GUI.DrawTexture(new Rect(0, 0, w, h), _tex);
        GUI.color = Color.white;

        var box = new GUIStyle(GUI.skin.box);
        box.normal.background = _tex;

        // Title
        var title = new GUIStyle(GUI.skin.label);
        title.fontSize = Mathf.RoundToInt(Mathf.Min(w, h) * 0.075f);
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.normal.textColor = new Color(0.95f, 0.82f, 0.42f);
        GUI.Label(new Rect(0, h * 0.34f, w, 80f), "JUGGERNAUT", title);

        // Spinner dots (pure IMGUI — no assets needed)
        int dots = 3 + ((int)(_t * 2f) % 3);
        string dotsStr = "LOADING";
        for (int i = 0; i < dots; i++) dotsStr += ".";
        var sub = new GUIStyle(GUI.skin.label);
        sub.fontSize = Mathf.RoundToInt(Mathf.Min(w, h) * 0.035f);
        sub.alignment = TextAnchor.MiddleCenter;
        sub.normal.textColor = new Color(0.75f, 0.75f, 0.7f);
        GUI.Label(new Rect(0, h * 0.46f, w, 50f), dotsStr, sub);

        // Gold progress bar (animated)
        float barW = Mathf.Min(500f, w * 0.6f), barH = 14f;
        float bx = (w - barW) / 2f, by = h * 0.55f;
        float frac = 0.15f + 0.7f * Mathf.Abs(Mathf.Sin(_t * 0.6f));
        GUI.color = new Color(0.1f, 0.1f, 0.12f, 1f);
        GUI.DrawTexture(new Rect(bx, by, barW, barH), _tex);
        GUI.color = new Color(0.85f, 0.7f, 0.35f, 1f);
        GUI.DrawTexture(new Rect(bx, by, barW * frac, barH), _tex);
        GUI.color = Color.white;
    }
}