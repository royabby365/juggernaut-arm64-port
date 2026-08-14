using System;
using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class SocialPoster : MonoBehaviour
{
	public enum MessageType
	{
		Rating,
		Achievement
	}

	private const string TWITTER_KEY = "LastTwitterTime";

	private const string FACEBOOK_KEY = "LastFacebookTime";

	private const float COOLDOWN_SAVE_PERIOD = 60f;

	private CompositeDisposable _subscriptions;

	private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();

	private string _twitterKey = "LastTwitterTime";

	private string _facebookKey = "LastFacebookTime";

	private float _saveTime;

	private bool _isLoaded;

	public SocialButtonMk1 ButtonFacebook;

	public SocialButtonMk1 ButtonTwitter;

	public Sprite ProgressBarTwitter;

	public Sprite ProgressBarFacebook;

	public AnimationCurve ButtonCooldown;

	private void Start()
	{
		ButtonFacebook.gameObject.SetActiveRecursivelyMk1(setActive: false);
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += ProcessButtons;
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger.AddListener(Globals.MsgTwitterNoInternet, delegate
		{
			Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowAlert, ServerData.PhrasesE.AlertNoInternet);
		}));
		_subscriptions.Add(Messenger<string>.AddListener(Globals.MsgTwitterPostSucceded, delegate(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("SOCIAL POSTER: MsgTwitterPostSucceded " + ButtonTwitter.MessageType);
			}
			int achievmentId = GetAchievmentId(message);
			GiveMoneyForSocialPost(achievmentId);
			if (achievmentId > -1)
			{
				if (ButtonTwitter.MessageType == MessageType.Achievement)
				{
					ButtonTwitter.gameObject.SetActiveRecursively(state: false);
				}
			}
			else if (ButtonTwitter.MessageType == MessageType.Rating)
			{
				_cooldowns[_twitterKey] = 0f;
				PlayerPrefs.SetFloat(_twitterKey, _cooldowns[_twitterKey]);
				PlayerPrefs.Save();
			}
			Metrics.OnPostToTwitterDone();
		}));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgTwitterCanceled, delegate
		{
		}));
		_subscriptions.Add(Messenger<string>.AddListener(Globals.MsgFacebookPostSucceded, delegate(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("SOCIAL POSTER: MsgFacebookPostSucceded " + ButtonFacebook.MessageType);
			}
			int achievmentId = GetAchievmentId(message);
			GiveMoneyForSocialPost(achievmentId);
			if (achievmentId > -1)
			{
				if (ButtonFacebook.MessageType == MessageType.Achievement)
				{
					ButtonFacebook.gameObject.SetActiveRecursively(state: false);
				}
			}
			else if (ButtonFacebook.MessageType == MessageType.Rating)
			{
				_cooldowns[_facebookKey] = 0f;
				PlayerPrefs.SetFloat(_facebookKey, _cooldowns[_facebookKey]);
				PlayerPrefs.Save();
			}
			Metrics.OnPostToFacebookDone();
		}));
		_subscriptions.Add(Messenger<string>.AddListener(Globals.MsgFacebookNotPosted, delegate(string text)
		{
			string[] array = text.Split(new string[1] { UnityApi._xcodeSplitter }, StringSplitOptions.None);
			if (array.Length > 1 && int.TryParse(array[array.Length - 1], out var result) && result == 341)
			{
				Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowAlert, ServerData.PhrasesE.RatingSocialMessageFacebook341Error);
			}
			else
			{
				Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowAlert, ServerData.PhrasesE.RatingSocialMessageFacebookNotPosted);
			}
		}));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, delegate(GuiRoot.GuiType old, GuiRoot.GuiType @new)
		{
			if (@new == GuiRoot.GuiType.Achievments && ButtonFacebook.MessageType == MessageType.Achievement)
			{
				ButtonTwitter.gameObject.SetActiveRecursively(state: true);
			}
		}));
		_subscriptions.Add(Messenger<int>.AddListener(Globals.MsgNewSaveSlotIndex, delegate(int slotIndex)
		{
			Utils.LogForce("LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL", slotIndex);
			_twitterKey = "LastTwitterTime" + slotIndex;
			_facebookKey = "LastFacebookTime" + slotIndex;
			int num = (SingletonT<ServerData>.I.GameSettings.RatingSocialCulldown + 1) * 60;
			_cooldowns = new Dictionary<string, float>
			{
				{ _twitterKey, num },
				{ _facebookKey, num }
			};
			if (ButtonFacebook.MessageType == MessageType.Rating)
			{
				if (PlayerPrefs.HasKey(_twitterKey))
				{
					_cooldowns[_twitterKey] = PlayerPrefs.GetFloat(_twitterKey);
				}
				if (PlayerPrefs.HasKey(_facebookKey))
				{
					_cooldowns[_facebookKey] = PlayerPrefs.GetFloat(_facebookKey);
				}
				Utils.LogForce("LLLLLL", _cooldowns[_twitterKey], _cooldowns[_facebookKey]);
				CheckProgress(_twitterKey, ButtonTwitter, ProgressBarTwitter);
				CheckProgress(_facebookKey, ButtonFacebook, ProgressBarFacebook);
			}
			if (!UnityApi.IsTwitterEnabled())
			{
				Utils.Log("TWITTER.SetInactive");
				ButtonTwitter.SetInactive();
				ButtonTwitter.gameObject.SetActiveRecursivelyMk1(setActive: false);
			}
			_isLoaded = true;
		}));
	}

	private int GetAchievmentId(string message)
	{
		string[] array = message.Split(new string[1] { UnityApi._xcodeSplitter }, StringSplitOptions.None);
		if (!int.TryParse(array[array.Length - 1], out var result))
		{
			result = -1;
		}
		Utils.LogForce("SOCIAL POSTER GetAchievmentId: string=" + array[array.Length - 1] + " int=" + result);
		return result;
	}

	private void GiveMoneyForSocialPost(int achievmentId)
	{
		if (achievmentId > -1)
		{
			if (ButtonFacebook.MessageType == MessageType.Achievement)
			{
				SingletonT<ServerData>.I.PlayerParams.MoneyGoldCount += SingletonT<ServerData>.I.GameSettings.AchievmentSharingMoneyBonus;
			}
		}
		else if (ButtonFacebook.MessageType == MessageType.Rating)
		{
			SingletonT<ServerData>.I.PlayerParams.MoneySkullsCount += SingletonT<ServerData>.I.GameSettings.RatingSharingMoneyBonus;
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Update()
	{
		if (!_isLoaded)
		{
			return;
		}
		CheckProgress(_twitterKey, ButtonTwitter, ProgressBarTwitter);
		if (!UnityApi.IsTwitterEnabled())
		{
			ButtonTwitter.SetInactive();
			ButtonTwitter.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		CheckProgress(_facebookKey, ButtonFacebook, ProgressBarFacebook);
		if (_saveTime > 60f && ButtonFacebook.MessageType != MessageType.Achievement)
		{
			foreach (KeyValuePair<string, float> cooldown in _cooldowns)
			{
				PlayerPrefs.SetFloat(cooldown.Key, cooldown.Value);
			}
			PlayerPrefs.Save();
			_saveTime = 0f;
		}
		_saveTime += Time.deltaTime;
	}

	private void CheckProgress(string key, SocialButtonMk1 button, Sprite progressBar)
	{
		if ((!(button != ButtonFacebook) || !(button != ButtonTwitter)) && !string.IsNullOrEmpty(key) && !(button == null) && !(progressBar == null) && button.MessageType != MessageType.Achievement)
		{
			float num = _cooldowns[key];
			num += Time.deltaTime;
			_cooldowns[key] = num;
			float num2 = ((!Globals.DebugIgnoreSocialPostPeriod) ? (SingletonT<ServerData>.I.GameSettings.RatingSocialCulldown * 60) : 60);
			if (num >= num2)
			{
				progressBar.ClipVertical(1f);
				button.SetActive();
				return;
			}
			float time = num / num2;
			float fraction = ButtonCooldown.Evaluate(time);
			progressBar.ClipVertical(fraction);
			button.SetInactive();
		}
	}

	private void ProcessButtons(SpriteButton spriteButton)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (spriteButton == ButtonTwitter)
		{
			switch (ButtonTwitter.MessageType)
			{
			case MessageType.Rating:
				UnityApi.TwitterRatingSubmit(Utils.PutArg0(SingletonT<ServerData>.I.GetPhrase(ButtonTwitter.SocialMessage), MainMenu.GameEvents.AllFinishedAchivsPoints.ToString()));
				Messenger.Invoke(Globals.MsgRatingSocialMessageClick);
				if (Globals.DebugUseLocalSocialPosting)
				{
					Messenger.Invoke(Globals.MsgTwitterPostSucceded, "_twitterPostSucceded");
				}
				break;
			case MessageType.Achievement:
				UnityApi.TwitterSubmit(HudMk1.Instance.GetComponentInChildren<AchievmentsHud>().Achievement);
				if (Globals.DebugUseLocalSocialPosting)
				{
					Messenger.Invoke(Globals.MsgTwitterPostSucceded, "_twitterPostSucceded" + UnityApi._xcodeSplitter + 0);
				}
				break;
			}
		}
		else
		{
			if (!(spriteButton == ButtonFacebook))
			{
				return;
			}
			switch (ButtonFacebook.MessageType)
			{
			case MessageType.Rating:
				UnityApi.FacebookRatingSubmit(Utils.PutArg0(SingletonT<ServerData>.I.GetPhrase(ButtonFacebook.SocialMessage), MainMenu.GameEvents.AllFinishedAchivsPoints.ToString()));
				Messenger.Invoke(Globals.MsgRatingSocialMessageClick);
				if (Globals.DebugUseLocalSocialPosting)
				{
					Messenger.Invoke(Globals.MsgFacebookPostSucceded, "_facebookPostSucceded");
				}
				break;
			case MessageType.Achievement:
				UnityApi.FacebookSubmit(HudMk1.Instance.GetComponentInChildren<AchievmentsHud>().Achievement);
				if (Globals.DebugUseLocalSocialPosting)
				{
					Messenger.Invoke(Globals.MsgFacebookPostSucceded, "_facebookPostSucceded" + UnityApi._xcodeSplitter + 0);
				}
				break;
			}
		}
	}

	public static void ResetCulldown(int slotId)
	{
		int num = (SingletonT<ServerData>.I.GameSettings.RatingSocialCulldown + 1) * 60;
		string key = "LastTwitterTime" + slotId;
		if (PlayerPrefs.HasKey(key))
		{
			PlayerPrefs.SetFloat(key, num);
		}
		string key2 = "LastFacebookTime" + slotId;
		if (PlayerPrefs.HasKey(key2))
		{
			PlayerPrefs.SetFloat(key2, num);
		}
	}
}
