using UnityEngine;

/// <summary>
/// Draws the combat UI (HP bars + action buttons + VICTORY/DEFEAT banner)
/// via OnGUI on its own DontDestroyOnLoad/HideAndDontSave GO, because
/// OnGUI doesn't render on scene-root GOs in this project's transition pipeline.
///
/// Uses GUI.Box/GUI.Button with _tex (2x2 white) as their background and
/// GUI.backgroundColor for tinting — the project's default skin has no baked
/// backgrounds, so without this pattern fills are invisible.
///
/// Reads state from CombatController.Instance.
/// </summary>
public class BattleUI : MonoBehaviour
{
    private Texture2D _tex;

    private void Awake()
    {
        _tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        _tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        _tex.Apply();
        _tex.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnGUI()
    {
        var cc = CombatController.Instance;
        if (cc == null) return;

        float w = Screen.width, h = Screen.height;
        float s = Mathf.Max(0.5f, Mathf.Min(w / 1920f, h / 1080f));

        // Styles with the solid texture as background (IL2CPP-safe, no missing skin).
        var box = new GUIStyle(GUI.skin.box);
        box.normal.background = _tex;
        var btn = new GUIStyle(GUI.skin.button);
        btn.normal.background = _tex;
        btn.fontSize = Mathf.RoundToInt(30 * s);
        btn.fontStyle = FontStyle.Bold;
        btn.alignment = TextAnchor.MiddleCenter;
        var lbl = new GUIStyle(GUI.skin.label);

        // ---- HP bars ----
        float barW = 480f * s, barH = 30f * s, by = 60f * s;

        lbl.fontSize = Mathf.RoundToInt(20 * s);
        // Player (left)
        lbl.normal.textColor = new Color(0.4f, 0.95f, 0.55f);
        lbl.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(50 * s, by - 28 * s, 300 * s, 22 * s),
                  "YOU " + Mathf.CeilToInt(cc.PlayerHP) + "/100", lbl);
        GUI.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        GUI.Box(new Rect(50 * s, by, barW, barH), "", box);
        GUI.backgroundColor = cc.PlayerHP > 35
            ? new Color(0.25f, 0.85f, 0.35f)
            : new Color(0.95f, 0.3f, 0.2f);
        GUI.Box(new Rect(50 * s, by, barW * Mathf.Clamp01(cc.PlayerHP / 100f), barH), "", box);

        // Enemy (right)
        float ex = w - 50 * s - barW;
        lbl.normal.textColor = new Color(1f, 0.6f, 0.5f);
        lbl.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(ex, by - 28 * s, barW, 22 * s),
                  "ENEMY " + Mathf.CeilToInt(cc.EnemyHP) + "/100", lbl);
        GUI.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        GUI.Box(new Rect(ex, by, barW, barH), "", box);
        GUI.backgroundColor = cc.EnemyHP > 35
            ? new Color(0.85f, 0.38f, 0.28f)
            : new Color(0.95f, 0.85f, 0.25f);
        GUI.Box(new Rect(ex, by, barW * Mathf.Clamp01(cc.EnemyHP / 100f), barH), "", box);
        GUI.backgroundColor = Color.white;

        // ---- Banner ----
        lbl.fontSize = Mathf.RoundToInt(56 * s);
        lbl.fontStyle = FontStyle.Bold;
        lbl.alignment = TextAnchor.MiddleCenter;
        string msg = cc.BattleOver
            ? (cc.Victory ? "VICTORY" : "DEFEAT")
            : cc.Phase;
        float cw = 700f * s, bannerY = 120f * s;
        // Black backing
        GUI.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
        GUI.Box(new Rect(w / 2 - cw / 2, bannerY - 10f * s, cw, 84f * s), "", box);
        // Text
        lbl.normal.textColor = cc.BattleOver
            ? (cc.Victory ? new Color(0.2f, 0.95f, 0.4f) : new Color(1f, 0.35f, 0.3f))
            : Color.white;
        GUI.Label(new Rect(w / 2 - cw / 2, bannerY, cw, 70f * s), msg, lbl);

        // ---- Action buttons ----
        if (cc.BattleOver)
        {
            float bw = 360f * s, bbh = 84f * s;
            btn.fontSize = Mathf.RoundToInt(34 * s);
            btn.normal.textColor = new Color(0.12f, 0.08f, 0.03f);
            GUI.backgroundColor = new Color(0.9f, 0.75f, 0.35f);
            if (GUI.Button(new Rect(w / 2 - bw / 2, 460f * s, bw, bbh),
                           cc.Victory ? "NEXT ARENA" : "RETRY", btn))
                cc.AdvanceArena();
        }
        else if (!cc.Busy && cc.IsReady)
        {
            float bw = 220f * s, bh = 78f * s, gap = 26f * s;
            float btnY = h - bh - 48f * s;
            float total = 4f * bw + 3f * gap;
            float x0 = (w - total) / 2f;

            btn.normal.textColor = Color.white;
            GUI.backgroundColor = new Color(0.85f, 0.32f, 0.26f);
            if (GUI.Button(new Rect(x0, btnY, bw, bh), "ATTACK", btn)) cc.PlayerAttack();

            GUI.backgroundColor = new Color(0.34f, 0.5f, 0.92f);
            if (GUI.Button(new Rect(x0 + bw + gap, btnY, bw, bh), "MAGIC", btn)) cc.PlayerMagic();

            GUI.backgroundColor = new Color(0.52f, 0.54f, 0.6f);
            btn.normal.textColor = new Color(0.05f, 0.05f, 0.05f);
            if (GUI.Button(new Rect(x0 + 2f * (bw + gap), btnY, bw, bh), "BLOCK", btn)) cc.PlayerBlock();

            GUI.backgroundColor = new Color(0.3f, 0.75f, 0.4f);
            btn.normal.textColor = Color.white;
            if (GUI.Button(new Rect(x0 + 3f * (bw + gap), btnY, bw, bh), "DODGE", btn)) cc.PlayerDodge();
        }

        GUI.backgroundColor = Color.white;
    }
}