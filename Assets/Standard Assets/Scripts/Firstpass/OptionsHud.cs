using UnityEngine;
using Yarx;

public class OptionsHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Slider SliderMusicVolume;

	public Slider SliderSoundsVolume;

	public ToggleSlider ToggleTutorials;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<string>.AddListener(Globals.MsgGuiButtonPressed, ProcessButtons));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
		SliderMusicVolume.ValueChanged += SliderMusicVolume_ValueChanged;
		SliderSoundsVolume.ValueChanged += SliderSoundsVolume_ValueChanged;
		ToggleTutorials.ValueChanged += ToggleTutorials_ValueChanged;
	}

	private void OnDisable()
	{
		SliderMusicVolume.ValueChanged -= SliderMusicVolume_ValueChanged;
		SliderSoundsVolume.ValueChanged -= SliderSoundsVolume_ValueChanged;
		ToggleTutorials.ValueChanged -= ToggleTutorials_ValueChanged;
		_subscriptions.Dispose();
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.Options)
		{
			if (SingletonT<ServerData>.I.GameSettings != null)
			{
				SliderMusicVolume.Value = SingletonT<ServerData>.I.GameSettings.MusicVolume;
				SliderSoundsVolume.Value = SingletonT<ServerData>.I.GameSettings.SoundsVolume;
			}
			ToggleTutorials.Value = MainMenu.Tutorials.Enabled;
		}
	}

	private void Start()
	{
		if (SingletonT<ServerData>.I.GameSettings != null)
		{
			SliderMusicVolume.Value = SingletonT<ServerData>.I.GameSettings.MusicVolume;
			SliderSoundsVolume.Value = SingletonT<ServerData>.I.GameSettings.SoundsVolume;
		}
		ToggleTutorials.Value = MainMenu.Tutorials.Enabled;
	}

	private void ProcessButtons(string buttonName)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Options)
		{
			return;
		}
		switch (buttonName)
		{
		case "button_options_restart":
			Globals.ShowLoadingScreen(delegate
			{
				UnityApi.SendMainMenuToXCode();
				Globals.MainMenu.Restart();
			});
			break;
		case "button_options_tizer_page":
			Application.OpenURL(SingletonT<ServerData>.I.GameSettings.TeaserPage);
			break;
		case "button_options_faq_page":
			Application.OpenURL(SingletonT<ServerData>.I.GameSettings.FaqPage);
			break;
		}
	}

	private void SliderMusicVolume_ValueChanged(float value)
	{
		SingletonT<SoundManager>.I.SetMusicVolume(value);
	}

	private void SliderSoundsVolume_ValueChanged(float value)
	{
		SingletonT<ServerData>.I.GameSettings.SoundsVolume = value;
	}

	private void ToggleTutorials_ValueChanged(bool value)
	{
		MainMenu.Tutorials.Enabled = value;
	}
}
