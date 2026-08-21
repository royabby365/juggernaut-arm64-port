using UnityEngine;

/// <summary>
/// Draws the combat UI (HP bars + action buttons + VICTORY/DEFEAT banner)
/// via OnGUI on the ARENA ROOT itself, which lives in the active scene
/// with a camera —- unlike the DontDestroyOnLoad CombatController singleton
/// whose OnGUI is never called after scene transitions.
///
/// Reads state from CombatController.Instance; drives PlayerAttack/Magic/etc.
/// Added by ArenaBuilder.Build and destroyed with the arena on rebuild.
/// </summary>
public class BattleUI : MonoBehaviour
{
    private Texture2D _tex;
    private bool _loggedOn;

    private void Awake()
    {
        _tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        _tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        _tex.Apply();
        _tex.hideFlags = HideFlags.HideAndDontSave;
        Debug.Log("[BattleUI] Awake on " + gameObject.name);
    }

    private void EnsureTex()
    {
        // Already created in Awake — no-op guard against destroy/recreate edge cases.
    }

    private void OnGUI()
    {
        var cc = CombatController.Instance;
        if (cc == null) return;

        float w = Screen.width, h = Screen.height;
        float s = Mathf.Max(0.5f, Mathf.Min(w / 1920f, h / 1080f));

        // Draw UI using GUI.Box with backgroundColor + a solid-style box.
        // The game's default GUI.skin.box has no baked background, so we override
        // it with our texture for all draws.
        var boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = _tex;
        var labelStyle = new GUIStyle(GUI.skin.label);

        // ---- HP bars (solid boxes) ----
        float barW = 480f * s, barH = 30f * s, y = 60f * s;

        // Player bar
        labelStyle.fontSize = Mathf.RoundToInt(20 * s);
        labelStyle.normal.textColor = new Color(0.4f, 0.95f, 0.55f);
        GUI.Label(new Rect(50 * s, y - 28 * s, 300 * s, 22 * s),
                  "YOU " + Mathf.CeilToInt(cc.PlayerHP) + "/100", labelStyle);
        GUI.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        GUI.Box(new Rect(50 * s, y, barW, barH), "", boxStyle);
        GUI.backgroundColor = cc.PlayerHP > 35 ? new Color(0.25f, 0.85f, 0.35f) : new Color(0.95f, 0.3f, 0.2f);
        GUI.Box(new Rect(50 * s, y, barW * Mathf.Clamp01(cc.PlayerHP / 100f), barH), "", boxStyle);

        // Enemy bar
        float ex = w - 50 * s - barW;
        labelStyle.alignment = TextAnchor.MiddleRight;
        labelStyle.normal.textColor = new Color(1f, 0.6f, 0.5f);
        GUI.Label(new Rect(ex, y - 28 * s, barW, 22 * s),
                  "ENEMY " + Mathf.CeilToInt(cc.EnemyHP) + "/100", labelStyle);
        GUI.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        GUI.Box(new Rect(ex, y, barW, barH), "", boxStyle);
        GUI.backgroundColor = cc.EnemyHP > 35 ? new Color(0.85f, 0.38f, 0.28f) : new Color(0.95f, 0.85f, 0.25f);
        GUI.Box(new Rect(ex, y, barW * Mathf.Clamp01(cc.EnemyHP / 100f), barH), "", boxStyle);
        GUI.backgroundColor = Color.white;

        // ---- Banner ----
        labelStyle.fontSize = Mathf.RoundToInt(56 * s);
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        string msg = cc.BattleOver
            ? (cc.Victory ? "VICTORY" : "DEFEAT")
            : cc.Phase;
        float cw = 700f * s, by = 120f * s;
        // Black backing
        GUI.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
        GUI.Box(new Rect(w / 2 - cw / 2, by - 10f * s, cw, 84f * s), "", boxStyle);
        // Text
        labelStyle.normal.textColor = cc.BattleOver
            ? (cc.Victory ? new Color(0.2f, 0.95f, 0.4f) : new Color(1f, 0.35f, 0.3f))
            : Color.white;
        GUI.Label(new Rect(w / 2 - cw / 2, by, cw, 70f * s), msg, labelStyle);

        // ---- Action buttons ----
        var btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.normal.background = _tex;
        btnStyle.fontSize = Mathf.RoundToInt(30 * s);
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;

        if (cc.BattleOver)
        {
            float bw = 360f * s, bh = 84f * s;
            btnStyle.fontSize = Mathf.RoundToInt(34 * s);
            btnStyle.normal.textColor = new Color(0.12f, 0.08f, 0.03f);
            GUI.backgroundColor = new Color(0.9f, 0.75f, 0.35f);
            if (GUI.Button(new Rect(w / 2 - bw / 2, 460f * s, bw, bh),
                           cc.Victory ? "NEXT ARENA" : "RETRY", btnStyle))
                cc.AdvanceArena();
        }
        else if (!cc.Busy && cc.IsReady)
        {
            float bw = 220f * s, bh = 78f * s, gap = 26f * s;
            float btnY = h - bh - 48f * s;
            float total = 4f * bw + 3f * gap;
            float x0 = (w - total) / 2f;

            btnStyle.normal.textColor = Color.white;
            GUI.backgroundColor = new Color(0.85f, 0.32f, 0.26f);
            if (GUI.Button(new Rect(x0, btnY, bw, bh), "ATTACK", btnStyle)) cc.PlayerAttack();

            GUI.backgroundColor = new Color(0.34f, 0.5f, 0.92f);
            if (GUI.Button(new Rect(x0 + bw + gap, btnY, bw, bh), "MAGIC", btnStyle)) cc.PlayerMagic();

            GUI.backgroundColor = new Color(0.52f, 0.54f, 0.6f);
            btnStyle.normal.textColor = new Color(0.05f, 0.05f, 0.05f);
            if (GUI.Button(new Rect(x0 + 2f * (bw + gap), btnY, bw, bh), "BLOCK", btnStyle)) cc.PlayerBlock();

            GUI.backgroundColor = new Color(0.3f, 0.75f, 0.4f);
            btnStyle.normal.textColor = Color.white;
            if (GUI.Button(new Rect(x0 + 3f * (bw + gap), btnY, bw, bh), "DODGE", btnStyle)) cc.PlayerDodge();
        }

        GUI.backgroundColor = Color.white;
    }
}