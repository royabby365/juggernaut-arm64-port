using UnityEngine;

internal class StartMenuSimple : MonoBehaviour
{
	private GUIStyle _titleStyle;
	private GUIStyle _buttonStyle;
	private GUIStyle _hintStyle;
	private bool _initialized;

	private void EnsureStyles()
	{
		if (_initialized) return;
		_titleStyle = new GUIStyle(GUI.skin.label);
		_titleStyle.fontSize = 48;
		_titleStyle.fontStyle = FontStyle.Bold;
		_titleStyle.normal.textColor = new Color(0.95f, 0.85f, 0.55f, 1f);
		_titleStyle.alignment = TextAnchor.MiddleCenter;

		_buttonStyle = new GUIStyle(GUI.skin.button);
		_buttonStyle.fontSize = 28;
		_buttonStyle.fontStyle = FontStyle.Bold;

		_hintStyle = new GUIStyle(GUI.skin.label);
		_hintStyle.fontSize = 16;
		_hintStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);
		_hintStyle.alignment = TextAnchor.MiddleCenter;
		_initialized = true;
	}

	private void OnGUI()
	{
		EnsureStyles();

		// Center the menu block on the screen
		float screenW = Screen.width;
		float screenH = Screen.height;
		float menuW = Mathf.Min(520f, screenW * 0.7f);
		float blockX = (screenW - menuW) / 2f;
		float blockY = screenH * 0.18f;

		// Title
		GUI.Label(new Rect(blockX, blockY, menuW, 70f), "JUGGERNAUT", _titleStyle);
		GUI.Label(new Rect(blockX, blockY + 70f, menuW, 30f), "Android port", _hintStyle);

		float btnY = blockY + 130f;
		float btnH = 70f;
		float gap = 18f;

		// NEW GAME (in DebugNoBundles mode, same as CONTINUE — skip player-select/intro which hangs on missing server data)
		if (GUI.Button(new Rect(blockX, btnY, menuW, btnH), "NEW GAME", _buttonStyle))
		{
			var menu = GameObject.Find("__main_menu")?.GetComponent<MainMenu>();
			if (menu != null)
			{
				if (Globals.DebugNoBundles)
				{
					menu.GoToFromStartMenuToMainMap();
				}
				else
				{
					menu.StartNewGame();
				}
			}
			else
			{
				Debug.LogWarning("[StartMenuSimple] __main_menu not found, cannot start new game.");
			}
		}
		btnY += btnH + gap;

		// CONTINUE (loads the last saved game)
		if (GUI.Button(new Rect(blockX, btnY, menuW, btnH), "CONTINUE", _buttonStyle))
		{
			var menu = GameObject.Find("__main_menu")?.GetComponent<MainMenu>();
			if (menu != null)
			{
				menu.GoToFromStartMenuToMainMap();
			}
			else
			{
				Debug.LogWarning("[StartMenuSimple] __main_menu not found, cannot continue.");
			}
		}
		btnY += btnH + gap;

		// QUIT
		if (GUI.Button(new Rect(blockX, btnY, menuW, btnH), "QUIT", _buttonStyle))
		{
			var menu = GameObject.Find("__main_menu")?.GetComponent<MainMenu>();
			if (menu != null)
			{
				menu.ExitGame();
			}
			else
			{
				Application.Quit();
			}
		}

		// Footer hint
		GUI.Label(
			new Rect(blockX, screenH - 50f, menuW, 30f),
			"v2.4.3 - build " + Application.version,
			_hintStyle);
	}
}
