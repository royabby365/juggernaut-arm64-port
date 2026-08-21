using UnityEngine;

/// <summary>
/// Minimal but polished START MENU for the offline (DebugNoBundles) port.
/// The full NGUI menu needs prefabs that can only be extracted from the
/// original asset bundles, so this OnGUI skeleton is the shipped entry point:
///   NEW GAME  -> GoToFromStartMenuToMainMap()  (into the arena/battle)
///   CONTINUE  -> GoToFromStartMenuToMainMap()  (DebugNoBundles: same as new)
///   QUIT      -> exit
/// Uses bright gold-on-dark styling so the buttons are discoverable against the
/// dark boot background, and a dimming scrim for a polished title-screen look.
/// IL2CPP-safe: no Shader.Find, no CreatePrimitive; GUI primitives only.
/// </summary>
internal class StartMenuSimple : MonoBehaviour
{
	private GUIStyle _titleStyle;
	private GUIStyle _subStyle;
	private GUIStyle _buttonStyle;
	private GUIStyle _hintStyle;
	private GUIStyle _footerStyle;
	private Texture2D _scrim;
	private bool _initialized;

	private void EnsureStyles()
	{
		if (_initialized) return;

		float s = Mathf.Max(0.5f, Mathf.Min(Screen.width / 1920f, Screen.height / 1080f));

		// 1x1 white texture used as a tintable backdrop (scrim behind title + as
		// the button fill base — tinted via GUI.backgroundColor).
		_scrim = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		_scrim.SetPixels(new Color[] {
			Color.white, Color.white,
			Color.white, Color.white
		});
		_scrim.Apply();

		// Title — large gold JUGGERNAUT
		_titleStyle = new GUIStyle(GUI.skin.label);
		_titleStyle.fontSize = Mathf.RoundToInt(86 * s);
		_titleStyle.fontStyle = FontStyle.Bold;
		_titleStyle.normal.textColor = new Color(0.95f, 0.82f, 0.42f, 1f);
		_titleStyle.alignment = TextAnchor.MiddleCenter;

		// Subtitle
		_subStyle = new GUIStyle(GUI.skin.label);
		_subStyle.fontSize = Mathf.RoundToInt(24 * s);
		_subStyle.normal.textColor = new Color(0.75f, 0.72f, 0.64f, 1f);
		_subStyle.alignment = TextAnchor.MiddleCenter;

		// Buttons — dark text on solid gold fill for contrast. The project's
		// default GUI.skin.button has NO baked background, so GUI.backgroundColor
		// tinting alone yields an invisible button. Assign the solid _scrim as the
		// explicit background; MenuButton() tints it gold via GUI.backgroundColor.
		_buttonStyle = new GUIStyle(GUI.skin.button);
		_buttonStyle.fontSize = Mathf.RoundToInt(34 * s);
		_buttonStyle.fontStyle = FontStyle.Bold;
		_buttonStyle.alignment = TextAnchor.MiddleCenter;
		if (_scrim != null)
		{
			_buttonStyle.normal.background = _scrim;
			_buttonStyle.hover.background = _scrim;
			_buttonStyle.active.background = _scrim;
		}
		_buttonStyle.normal.textColor = new Color(0.08f, 0.06f, 0.03f, 1f);
		_buttonStyle.hover.textColor = new Color(0.02f, 0.02f, 0.01f, 1f);
		_buttonStyle.active.textColor = new Color(0.0f, 0.0f, 0.0f, 1f);

		// Footer hint
		_footerStyle = new GUIStyle(GUI.skin.label);
		_footerStyle.fontSize = Mathf.RoundToInt(16 * s);
		_footerStyle.normal.textColor = new Color(0.55f, 0.55f, 0.5f, 1f);
		_footerStyle.alignment = TextAnchor.MiddleCenter;

		_initialized = true;
	}

	private void OnGUI()
	{
		EnsureStyles();

		float screenW = Screen.width;
		float screenH = Screen.height;
		float s = Mathf.Max(0.5f, Mathf.Min(screenW / 1920f, screenH / 1080f));

		// ---- Full-screen dimming scrim so the menu reads as a title screen ----
		GUI.color = new Color(0.02f, 0.02f, 0.045f, 0.86f);
		GUI.DrawTexture(new Rect(0, 0, screenW, screenH), _scrim);
		GUI.color = Color.white;

		// ---- Vertical layout block centered ----
		float menuW = Mathf.Min(640f, screenW * 0.74f);
		float blockX = (screenW - menuW) / 2f;
		float blockY = screenH * 0.16f;

		// Title with a subtle dark backing plaque
		GUI.DrawTexture(new Rect(blockX - 60f, blockY - 40f, menuW + 120f, 190f),
		                _scrim, ScaleMode.StretchToFill);
		GUI.color = new Color(0, 0, 0, 0.35f);          // dim plaque
		GUI.DrawTexture(new Rect(blockX - 60f, blockY - 40f, menuW + 120f, 190f),
		                _scrim, ScaleMode.StretchToFill);
		GUI.color = Color.white;
		GUI.Label(new Rect(blockX, blockY, menuW, 88f), "JUGGERNAUT", _titleStyle);
		GUI.Label(new Rect(blockX, blockY + 92f, menuW, 34f),
		          "Revenge of Sovering — Android arm64 port", _subStyle);

		// ---- Buttons ----
		float btnY = blockY + 170f;
		float btnH = 78f;
		float gap = 22f;

		// NEW GAME (DebugNoBundles == CONTINUE — avoids the player-select/intro
		//   screens that hang without server data)
		if (MenuButton(blockX, btnY, menuW, btnH, "NEW GAME"))
		{
			var menu = GameObject.Find(Globals.LocationGameObjectMainMenu)?.GetComponent<MainMenu>();
			if (menu != null)
			{
				if (Globals.DebugNoBundles) menu.GoToFromStartMenuToMainMap();
				else menu.StartNewGame();
			}
			else Debug.LogWarning("[StartMenuSimple] MainMenu not found, cannot start new game.");
		}
		btnY += btnH + gap;

		// CONTINUE
		if (MenuButton(blockX, btnY, menuW, btnH, "CONTINUE"))
		{
			var menu = GameObject.Find(Globals.LocationGameObjectMainMenu)?.GetComponent<MainMenu>();
			if (menu != null) menu.GoToFromStartMenuToMainMap();
			else Debug.LogWarning("[StartMenuSimple] MainMenu not found, cannot continue.");
		}
		btnY += btnH + gap;

		// QUIT
		if (MenuButton(blockX, btnY, menuW, btnH, "QUIT"))
		{
			var menu = GameObject.Find(Globals.LocationGameObjectMainMenu)?.GetComponent<MainMenu>();
			if (menu != null) menu.ExitGame();
			else Application.Quit();
		}

		// Footer
		GUI.Label(new Rect(blockX, screenH - 54f, menuW, 28f),
		          "v2.4.19 — offline turn-based battle build " + Application.version, _footerStyle);
	}

	/// <summary>Bright gold OnGUI button, scaled to the landscape base.</summary>
	private bool MenuButton(float x, float y, float w, float h, string label)
	{
		var push = GUI.backgroundColor;
		GUI.backgroundColor = new Color(0.88f, 0.72f, 0.38f, 1f); // gold
		GUI.color = Color.white;
		bool clicked = GUI.Button(new Rect(x, y, w, h), label, _buttonStyle);
		GUI.backgroundColor = push;
		GUI.color = Color.white;
		return clicked;
	}
}