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

    private void EnsureTex()
    {
        if (_tex != null) return;
        if (!_loggedOn)
        {
            _loggedOn = true;
            Debug.Log("[BattleUI] startup on " + gameObject.name);
        }
        _tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        _tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        _tex.Apply();
    }

    private void OnGUI()
    {
        var cc = CombatController.Instance;
        if (cc == null) return;

        EnsureTex();

        float w = Screen.width, h = Screen.height;
        float s = Mathf.Max(0.5f, Mathf.Min(w / 1920f, h / 1080f));

        // DIAGNOSTIC: draw a huge red rectangle in the center to confirm
        // GUI.DrawTexture works at all with our texture.
        GUI.color = new Color(1f, 0f, 0f, 0.5f);
        GUI.DrawTexture(new Rect(w/2 - 200, h/2 - 200, 400, 400), _tex);
        GUI.color = Color.white;

        DrawHPBars(cc, s, w, h);
        DrawBanner(cc, s, w, h);
        DrawButtons(cc, s, w, h);
    }

    // ---- HP bars ----
    private void DrawHPBars(CombatController cc, float s, float w, float h)
    {
        float barW = 480f * s, barH = 30f * s, y = 60f * s;

        // Player (left)
        GUI.Label(new Rect(50 * s, y - 28 * s, 300 * s, 22 * s),
                  "YOU  " + Mathf.CeilToInt(cc.PlayerHP) + "/100",
                  Style(new Color(0.4f, 0.95f, 0.55f, 1f), s));
        Bar(50 * s, y, barW, barH, cc.PlayerHP / 100f,
            cc.PlayerHP > 35
                ? new Color(0.25f, 0.85f, 0.35f)
                : new Color(0.95f, 0.3f, 0.2f));

        // Enemy (right)
        float ex = w - 50 * s - barW;
        GUI.Label(new Rect(ex, y - 28 * s, barW, 22 * s),
                  "ENEMY  " + Mathf.CeilToInt(cc.EnemyHP) + "/100",
                  Style(new Color(1f, 0.6f, 0.5f, 1f), s));
        Bar(ex, y, barW, barH, cc.EnemyHP / 100f,
            cc.EnemyHP > 35
                ? new Color(0.85f, 0.38f, 0.28f)
                : new Color(0.95f, 0.85f, 0.25f));
    }

    private void Bar(float x, float y, float w, float h, float frac, Color fill)
    {
        // backing
        GUI.color = new Color(0.05f, 0.05f, 0.08f, 1f);
        GUI.DrawTexture(new Rect(x, y, w, h), _tex);
        // fill
        GUI.color = fill;
        GUI.DrawTexture(new Rect(x, y, w * Mathf.Clamp01(frac), h), _tex);
        // thin border
        GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        float t = 3f;
        GUI.DrawTexture(new Rect(x, y, w, t), _tex);
        GUI.DrawTexture(new Rect(x, y + h - t, w, t), _tex);
        GUI.DrawTexture(new Rect(x, y, t, h), _tex);
        GUI.DrawTexture(new Rect(x + w - t, y, t, h), _tex);
        GUI.color = Color.white;
    }

    private GUIStyle Style(Color c, float s)
    {
        var st = new GUIStyle(GUI.skin.label);
        st.fontSize = Mathf.RoundToInt(20 * s);
        st.alignment = TextAnchor.MiddleLeft;
        st.normal.textColor = c;
        return st;
    }

    // ---- Banner ----
    private void DrawBanner(CombatController cc, float s, float w, float h)
    {
        string msg = cc.BattleOver
            ? (cc.Victory ? "VICTORY" : "DEFEAT")
            : cc.Phase;
        float cw = 700f * s, by = 120f * s;
        var st = new GUIStyle(GUI.skin.label);
        st.fontSize = Mathf.RoundToInt(56 * s);
        st.fontStyle = FontStyle.Bold;
        st.alignment = TextAnchor.MiddleCenter;
        st.normal.textColor = Color.white;

        // black backing
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(w / 2 - cw / 2, by - 10f * s, cw, 84f * s), _tex);
        GUI.color = cc.BattleOver
            ? (cc.Victory ? new Color(0.2f, 0.95f, 0.4f) : new Color(1f, 0.35f, 0.3f))
            : Color.white;
        GUI.Label(new Rect(w / 2 - cw / 2, by, cw, 70f * s), msg, st);
        GUI.color = Color.white;
    }

    // ---- Buttons ----
    private void DrawButtons(CombatController cc, float s, float w, float h)
    {
        if (cc.BattleOver)
        {
            float bw = 360f * s, bbh = 84f * s;
            if (Btn(w / 2 - bw / 2, 460f * s, bw, bbh,
                    cc.Victory ? "NEXT ARENA" : "RETRY",
                    new Color(0.9f, 0.75f, 0.35f),
                    new Color(0.12f, 0.08f, 0.03f, 1f), s, 34))
                cc.AdvanceArena();
        }
        else if (!cc.Busy && cc.IsReady)
        {
            float bw = 220f * s, bh = 78f * s, gap = 26f * s;
            float y = h - bh - 48f * s;
            float total = 4f * bw + 3f * gap;
            float x0 = (w - total) / 2f;

            if (Btn(x0,          y, bw, bh, "ATTACK", new Color(0.85f, 0.32f, 0.26f), Color.white, s, 30)) cc.PlayerAttack();
            if (Btn(x0 + bw + gap, y, bw, bh, "MAGIC",  new Color(0.34f, 0.5f, 0.92f), Color.white, s, 30)) cc.PlayerMagic();
            if (Btn(x0 + 2f*(bw+gap), y, bw, bh, "BLOCK",  new Color(0.52f, 0.54f, 0.6f),  new Color(0.05f, 0.05f, 0.05f), s, 30)) cc.PlayerBlock();
            if (Btn(x0 + 3f*(bw+gap), y, bw, bh, "DODGE",  new Color(0.3f, 0.75f, 0.4f),   Color.white, s, 30)) cc.PlayerDodge();
        }
    }

    private bool Btn(float x, float y, float bw, float bh,
                     string label, Color bg, Color fg, float s, int fontSize)
    {
        Color push = GUI.color;
        GUI.color = bg;
        GUI.DrawTexture(new Rect(x, y, bw, bh), _tex);
        GUI.color = push;

        var st = new GUIStyle(GUI.skin.label);
        st.fontSize = Mathf.RoundToInt(fontSize * s);
        st.fontStyle = FontStyle.Bold;
        st.alignment = TextAnchor.MiddleCenter;
        st.normal.textColor = fg;
        GUI.Label(new Rect(x, y, bw, bh), label, st);

        Event e = Event.current;
        return e.type == EventType.MouseDown
            && new Rect(x, y, bw, bh).Contains(e.mousePosition);
    }
}