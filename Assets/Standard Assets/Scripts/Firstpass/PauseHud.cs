using UnityEngine;

public class PauseHud : MonoBehaviour
{
	public PauseButton ButtonPause;

	public PauseButton ButtonResume;

	public PauseButton ButtonRestart;

	public PauseButton ButtonExit;

	private void Start()
	{
		ButtonPause.Click += ButtonPause_Click;
		ButtonResume.Click += ButtonResume_Click;
		ButtonRestart.Click += ButtonRestart_Click;
		ButtonExit.Click += ButtonExit_Click;
	}

	private void ButtonPause_Click(PauseButton obj)
	{
		if (!Globals.IsPaused)
		{
			Utils.LogForce("PAUSE");
			Time.timeScale = 0f;
			SpriteGui.DontReleaseButtons = true;
			Globals.IsPaused = true;
			if (HudMk1.Instance != null)
			{
				HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.Pause);
			}
		}
	}

	private void ButtonExit_Click(PauseButton obj)
	{
		SpriteGui.DontReleaseButtons = false;
		Time.timeScale = Globals.DefaultTimeScale;
		Globals.IsPaused = false;
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.Pause);
		}
		Globals.Battle.BreakBattle();
	}

	private void ButtonRestart_Click(PauseButton obj)
	{
		SpriteGui.DontReleaseButtons = false;
		Time.timeScale = Globals.DefaultTimeScale;
		Globals.IsPaused = false;
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.Pause);
		}
		Globals.Battle.RestartBattleWithSameEnemy(defeated: false);
	}

	private void ButtonResume_Click(PauseButton obj)
	{
		SpriteGui.DontReleaseButtons = false;
		Time.timeScale = Globals.DefaultTimeScale;
		Globals.IsPaused = false;
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.Pause);
		}
	}
}
