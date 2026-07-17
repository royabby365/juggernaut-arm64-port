using System;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using UnityEngine;

public class UnityApi
{
	private static Dictionary<string, string> PhrasesEx = null;

	private static string _OpenFeintAppId = null;

	private static string _ServerSignatureSalt = null;

	private static string PROD_FACEBOOK_APP_ID = "238316452939601";

	private static string DEV_FACEBOOK_APP_ID = "309384282477678";

	private static bool Initialized = false;

	private static bool PresentationInitialized = false;

	private static bool _UseIDreamSkyValue = false;

	private static bool _UseIDreamSkyInitialized = false;

	private static bool _UseGameClubValue = false;

	private static bool _UseGameClubInitialized = false;

	private static MetricController MetricController;

	private static float _TimeScale = 0f;

	private static float _AudioVolume = 0f;

	private static ActionD _OnPlayMovieFinish = null;

	private static int _isTwitterEnabled = 0;

	private static readonly string[] supportKeys = new string[12]
	{
		"supportXCodeText", "supportXCodeEMail", "supportXCodeTheme", "supportXCodeSend", "supportXCodeClose", "supportXCodeError", "supportXCodeEnterEmail", "supportXCodeEnterSubject", "supportXCodeEnterDesc", "supportXCodeAlertClose",
		"supportXCodeInvEmail", "supportXCodePosted"
	};

	public static readonly string _xcodeSplitter = "-*|";

	private static string localization = null;

	private static bool InitMyComAdmanCalled = false;

	internal static bool IsDevelopersBuild
	{
		get
		{
			try
			{
				string text = _GetBuildType();
				return text == "developers";
			}
			catch (Exception ex)
			{
				Utils.Log("IsDevelopersBuild FAILED", ex.Message);
			}
			return true;
		}
	}

	internal static bool IsReleaseBuild
	{
		get
		{
			try
			{
				return isRelease();
			}
			catch (Exception ex)
			{
				Utils.Log("IsReleaseBuild FAILED", ex.Message);
			}
			return false;
		}
	}

	public static float CharWidth
	{
		get
		{
			switch (GetLanguage())
			{
			case "ko":
				return 1.4f;
			case "zh":
			case "cn":
				return 1.8f;
			case "jp":
				return 1.8f;
			default:
				return 1f;
			}
		}
	}

	private static void _AndroidInit()
	{
		JavaVM.AttachCurrentThread();
	}

	private static void _OpenFeintInit()
	{
	}

	private static void _IDreamSkyInit()
	{
		IDreamSky.Init("__main_menu", "IDreamSkyCallbackHandler");
	}

	private static void _GameClubInit()
	{
		GameClub.Init("__main_menu", "GameClubCallbackHandler");
	}

	private static void _OKInit()
	{
		OK.Init("__main_menu", "OKCallbackHandler");
	}

	private static void _MarketBillingInit()
	{
		MarketBillingPlugin.SetCallbackHandler("__main_menu", "CallPaymentSuccessful");
		MarketBillingPlugin.RestoreTransaction();
	}

	private static void _AdmanInit()
	{
		GetCurrentActivity().Call("setAdmanCallback", "__main_menu", "AdmanCallbackHandler");
	}

	private static AndroidJavaObject GetCurrentActivity()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		return androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
	}

	private static void _PlayMovieInit()
	{
		GetCurrentActivity().Call("setVideoCallback", "__main_menu", "PlayMovieCallbackHandler");
	}

	private static void _OurGames()
	{
		Application.OpenURL("https://play.google.com/store/apps/developer?id=Mail.Ru+Group");
	}

	private static void _FacebookEnable()
	{
		FacebookPlugin.Enable();
	}

	private static void _FacebookDisable()
	{
		FacebookPlugin.Disable();
	}

	private static void _FacebookSubmit(string description)
	{
		FacebookPlugin.Submit(description);
	}

	private static void _PostScreenshot()
	{
		GameObject.Find("FacebookManager").GetComponent<ScreenshotManager>().TakeScreenshot();
	}

	private static bool _IsTwitterEnabled()
	{
		return TwitterPlugin.IsTwitterEnabled();
	}

	private static void _TwitterEnable()
	{
		TwitterPlugin.Enable();
	}

	private static void _TwitterDisable()
	{
		TwitterPlugin.Disable();
	}

	private static void _TwitterSubmit(string description)
	{
		TwitterPlugin.Submit(description);
	}

	private static void LoadPhrasesEx()
	{
		if (PhrasesEx != null)
		{
			return;
		}
		SingletonT<ResourcesManager>.I.LoadText("phrases_ex.json", delegate(string text)
		{
			Dictionary<string, object> t = (Dictionary<string, object>)Json.Deserialize(text);
			Dictionary<string, object> dictionary = JsonUtils.JsonGetHashtable(GetLanguage(), t);
			PhrasesEx = new Dictionary<string, string>();
			foreach (string key in dictionary.Keys)
			{
				PhrasesEx.Add(key, (string)dictionary[key]);
			}
		}, delegate(string error)
		{
			Utils.Log("Failed to load phrases_ex.json: " + error);
		});
	}

	private static string TranslateCrystals()
	{
		LoadPhrasesEx();
		string value = string.Empty;
		if (PhrasesEx == null || !PhrasesEx.TryGetValue("N_crystals", out value))
		{
			return "crystals";
		}
		return value;
	}

	private static string TranslateGold()
	{
		LoadPhrasesEx();
		string value = string.Empty;
		if (PhrasesEx == null || !PhrasesEx.TryGetValue("N_gold", out value))
		{
			return "gold";
		}
		return value;
	}

	private static void _OpenFeintBankBuy(ServerData.BankItem bankItem)
	{
		string slotId = UniqueData.SlotId;
		string slotUUID = UniqueData.SlotUUIDs[UniqueData.SlotId];
		int num = bankItem.Count + bankItem.Bonus;
		string empty = string.Empty;
		empty = ((bankItem.CountType.Type != ServerData.MoneyType.TypeE.Diamond) ? $"{num} {TranslateGold()}" : $"{num} {TranslateCrystals()}");
		double price = Globals.OpenFeintBankRealToCoins(bankItem.Real);
		Utils.Log("_OpenFeintBankBuy entered, slotId: " + slotId + ", slotUUID: " + slotUUID + ", purchaseId: " + bankItem.PurchaseId + ", purchaseName: " + empty + ", price: " + price);
		OpenFeint.DoPayment(bankItem.PurchaseId, empty, price, 1, slotId, slotUUID, delegate
		{
			string text = $"BankBuyDoneSuccessful {bankItem.PurchaseId}|{slotId}|{slotUUID}";
			Utils.Log("_OpenFeintBankBuy succeeded: ", text);
			Globals.MainMenu.IOSCall(text);
			Globals.MainMenu.SaveGame();
		}, delegate(Exception ex)
		{
			Utils.Log("_OpenFeintBankBuy failed: ", ex.Message);
			Globals.MainMenu.IOSCall("BankBuyDoneFail");
		});
	}

	private static bool _GetUseOpenFeintPurchases()
	{
		return GetOpenFeintAppId() == Globals.NewOpenFeintAppId;
	}

	private static string _GetOpenFeintAppId()
	{
		if (_OpenFeintAppId == null)
		{
			_OpenFeintAppId = string.Empty;
			_OpenFeintAppId = GetCurrentActivity().Call<string>("getOpenFeintAppId", new object[0]);
		}
		return _OpenFeintAppId;
	}

	private static void _IDreamSkyBankBuy(ServerData.BankItem bankItem)
	{
		string slotId = UniqueData.SlotId;
		string slotUUID = UniqueData.SlotUUIDs[UniqueData.SlotId];
		int num = bankItem.Count + bankItem.Bonus;
		string empty = string.Empty;
		empty = ((bankItem.CountType.Type != ServerData.MoneyType.TypeE.Diamond) ? $"{num} {TranslateGold()}" : $"{num} {TranslateCrystals()}");
		double price = Globals.IDreamSkyBankRealToCoins(bankItem.Real);
		Utils.Log("_IDreamSkyBankBuy entered, slotId: " + slotId + ", slotUUID: " + slotUUID + ", purchaseId: " + bankItem.PurchaseId + ", purchaseName: " + empty + ", price: " + price);
		IDreamSky.DoPayment(bankItem.PurchaseId, empty, price, 1, slotId, slotUUID, delegate
		{
			string text = $"BankBuyDoneSuccessful {bankItem.PurchaseId}|{slotId}|{slotUUID}";
			Utils.Log("_IDreamSkyBankBuy succeeded: ", text);
			Globals.MainMenu.IOSCall(text);
			Globals.MainMenu.SaveGame();
		}, delegate(Exception ex)
		{
			Utils.Log("_IDreamSkyBankBuy failed: ", ex.Message);
			Globals.MainMenu.IOSCall("BankBuyDoneFail");
		});
	}

	private static void _GameClubBankBuy(ServerData.BankItem bankItem)
	{
		string slotId = UniqueData.SlotId;
		string slotUUID = UniqueData.SlotUUIDs[UniqueData.SlotId];
		int num = bankItem.Count + bankItem.Bonus;
		string empty = string.Empty;
		empty = ((bankItem.CountType.Type != ServerData.MoneyType.TypeE.Diamond) ? $"{num} {TranslateGold()}" : $"{num} {TranslateCrystals()}");
		double price = Globals.GameClubBankRealToCoins(bankItem.Real);
		Utils.Log("_GameClubBankBuy entered, slotId: " + slotId + ", slotUUID: " + slotUUID + ", purchaseId: " + bankItem.PurchaseId + ", purchaseName: " + empty + ", price: " + price);
		GameClub.DoPayment(bankItem.PurchaseId, empty, price, 1, slotId, slotUUID, delegate
		{
			string text = $"BankBuyDoneSuccessful {bankItem.PurchaseId}|{slotId}|{slotUUID}";
			Utils.Log("_GameClubBankBuy succeeded: ", text);
			Globals.MainMenu.IOSCall(text);
			Globals.MainMenu.SaveGame();
		}, delegate(Exception ex)
		{
			Utils.Log("_GameClubBankBuy failed: ", ex.Message);
			Globals.MainMenu.IOSCall("BankBuyDoneFail");
			Globals.HideLoadingScreen();
		});
	}

	private static string _GetServerSignatureSalt()
	{
		if (_ServerSignatureSalt == null)
		{
			_ServerSignatureSalt = GetCurrentActivity().Call<string>("getServerSignatureSalt", new object[0]);
		}
		return _ServerSignatureSalt;
	}

	private static void _BankBuy(string purchaseId)
	{
		MarketBillingPlugin.RequestPurchase(purchaseId);
	}

	private static void _SendMainMenuToXCode()
	{
		UniqueData.InGame = false;
	}

	private static void _SendNewGameToXCode(string id)
	{
		UniqueData.InGame = true;
		UniqueData.SlotId = id;
		UniqueData.SlotUUIDs[id] = Guid.NewGuid().ToString();
		UniqueData.SaveSlots();
		Utils.Log("UniqueData.SlotId: ", UniqueData.SlotId);
		Utils.Log("UniqueData.SlotUUIDs[UniqueData.SlotId]: ", UniqueData.SlotUUIDs[UniqueData.SlotId]);
		SendGameStateToXCode();
		SendAchievToXCode();
		MetricController.GetDataAboutMoney();
		TapjoyPlugin.GetTapPoints();
	}

	private static void _SendContinueToXCode(string id)
	{
		UniqueData.InGame = true;
		UniqueData.SlotId = id;
		Utils.Log("UniqueData.SlotId: ", UniqueData.SlotId);
		Utils.Log("UniqueData.SlotUUIDs[UniqueData.SlotId]: ", UniqueData.SlotUUIDs[UniqueData.SlotId]);
		SendGameStateToXCode();
		SendAchievToXCode();
		MetricController.GetDataAboutMoney();
		TapjoyPlugin.GetTapPoints();
	}

	private static void _ShowSupport(string text)
	{
		if (!(HudMk1.Instance == null))
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.SupportPopup);
		}
	}

	private static void _GoToTapjoy()
	{
		TapjoyPlugin.ShowOffers();
	}

	private static void _PlayMovie(string videoId, string lang)
	{
		GetCurrentActivity().Call("playVideo", videoId);
	}

	private static string _GetBuildType()
	{
		return string.Empty;
	}

	private static bool isRelease()
	{
		return true;
	}

	private static void _RateApp()
	{
	}

	private static void _Init()
	{
		if (!Initialized)
		{
			_AndroidInit();
			if (UseOK())
			{
				_OKInit();
			}
			if (_UseIDreamSky())
			{
				_IDreamSkyInit();
			}
			else if (_UseGameClub())
			{
				_GameClubInit();
			}
			else
			{
				_OpenFeintInit();
			}
			_MarketBillingInit();
			_AdmanInit();
			_PlayMovieInit();
			FacebookPlugin.Init(PROD_FACEBOOK_APP_ID);
			Initialized = true;
		}
	}

	private static void _NotifyPresentationInitialized()
	{
		if (!PresentationInitialized)
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			androidJavaObject.Call("onPresentationInitialized");
			PresentationInitialized = true;
		}
	}

	private static void _Enable()
	{
		_TwitterEnable();
		_FacebookEnable();
	}

	private static void _Disable()
	{
		_TwitterDisable();
		_FacebookDisable();
	}

	private static bool _UseIDreamSky()
	{
		if (!_UseIDreamSkyInitialized)
		{
			try
			{
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				_UseIDreamSkyValue = androidJavaObject.Call<bool>("useIDreamSky", new object[0]);
			}
			catch (Exception)
			{
				_UseIDreamSkyValue = false;
			}
			Debug.Log("_UseIDreamSky: " + _UseIDreamSkyValue);
			_UseIDreamSkyInitialized = true;
		}
		return _UseIDreamSkyValue;
	}

	private static bool _UseGameClub()
	{
		if (!_UseGameClubInitialized)
		{
			try
			{
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				_UseGameClubValue = androidJavaObject.Call<bool>("useGameClub", new object[0]);
			}
			catch (Exception)
			{
				_UseGameClubValue = false;
			}
			Debug.Log("_UseGameClub: " + _UseGameClubValue);
			_UseGameClubInitialized = true;
		}
		return _UseGameClubValue;
	}

	private static SocialAspect _CreateSocialAspect(MonoBehaviour behaviour)
	{
		if (_UseIDreamSky())
		{
			return new IDreamSkyAndroidSocialAspect(behaviour);
		}
		if (_UseGameClub())
		{
			return new GameClubAndroidSocialAspect(behaviour);
		}
		return new OpenFeintAndroidSocialAspect(behaviour);
	}

	private static void _AFPresentNotifications()
	{
		AppsFirePlugin.ShowNotifications();
	}

	private static int _GetNumberOfPendingNotifications()
	{
		return AppsFirePlugin.GetNumberOfPendingNotifications();
	}

	private static string _GetServerUrl()
	{
		if (_UseIDreamSky())
		{
			return "http://jugpadcn.ext.terrhq.ru/";
		}
		if (_UseGameClub())
		{
			return "http://jugpadkr.ext.terrhq.ru/";
		}
		if (IsReleaseBuild)
		{
			return "http://jugpad.ext.terrhq.ru/";
		}
		return "http://jptest.ext.terrhq.ru/";
	}

	private static void _HideStartLoading()
	{
		_NotifyPresentationInitialized();
	}

	private static string _GetLanguage()
	{
		JavaVM.AttachCurrentThread();
		return GetCurrentActivity().Call<string>("getLanguage", new object[0]);
	}

	private static void setSubtitles(string subtitles)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		androidJavaObject.Call("setVideoSubtitles", subtitles);
	}

	private static void _AcquireLoadingWakeLock()
	{
		GetCurrentActivity().Call("acquireLoadingWakeLock");
	}

	private static void _ReleaseLoadingWakeLock()
	{
		GetCurrentActivity().Call("releaseLoadingWakeLock");
	}

	internal static void SetSubtitles()
	{
		try
		{
			string text = "|||";
			Dictionary<int, ServerData.Subtitle> subtitles = SingletonT<ServerData>.I._subtitles;
			StringBuilder stringBuilder = new StringBuilder(subtitles.Count * 128);
			foreach (ServerData.Subtitle value in subtitles.Values)
			{
				stringBuilder.Append("{0}{1}{2}{3}{4}{5}{6}{7}".Fmt(value.Video, text, value.StartTime, text, value.EndTime, text, value.Text, text));
			}
			setSubtitles(stringBuilder.ToString());
		}
		catch (Exception ex)
		{
			Utils.Log("setSubtitles", ex.Message);
		}
	}

	internal static string GetServerUrl()
	{
		return _GetServerUrl();
	}

	internal static int GetNumberOfPendingNotifications()
	{
		try
		{
			return _GetNumberOfPendingNotifications();
		}
		catch (Exception ex)
		{
			Utils.Log("_GetNumberOfPendingNotifications FAILED", ex.Message);
		}
		return 0;
	}

	internal static void AFPresentNotifications()
	{
		try
		{
			_AFPresentNotifications();
		}
		catch (Exception ex)
		{
			Utils.Log("_AFPresentNotifications FAILED", ex.Message);
		}
	}

	private static int _GetNewMessage()
	{
		return 0;
	}

	private static void _OpenMessage()
	{
	}

	private static void _ActionComplete(string action)
	{
	}

	internal static bool RateApp()
	{
		Utils.LogForce("RATEAPPin");
		try
		{
			_RateApp();
			Utils.LogForce("RATEAPP ok");
			return true;
		}
		catch (Exception ex)
		{
			Utils.LogForce("RateApp FAILED", ex.Message);
		}
		return false;
	}

	public static bool UseSingleApk()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		return androidJavaObject.Call<bool>("useSingleApk", new object[0]);
	}

	public static string GetMainObbPath()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		return androidJavaObject.Call<string>("getMainObbPath", new object[0]);
	}

	public static string GetPackageName()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		return androidJavaObject.Call<string>("getPackageName", new object[0]);
	}

	public static int GetPackageVersionCode()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		return androidJavaObject.Call<int>("getPackageVersionCode", new object[0]);
	}

	public static void OnPlayMovieFinish()
	{
		Time.timeScale = _TimeScale;
		AudioListener.volume = _AudioVolume;
		if (_OnPlayMovieFinish != null)
		{
			_OnPlayMovieFinish();
			_OnPlayMovieFinish = null;
		}
	}

	public static void PlayMovie(string name, ActionD onFinish)
	{
		_OnPlayMovieFinish = onFinish;
		_TimeScale = Time.timeScale;
		_AudioVolume = AudioListener.volume;
		Time.timeScale = 0f;
		AudioListener.volume = 0f;
		try
		{
			_PlayMovie(name, GetLanguage());
		}
		catch (Exception ex)
		{
			Utils.Log("_PlayMovie FAILED", name, ex.Message);
		}
	}

	public static void AcquireLoadingWakeLock()
	{
		_AcquireLoadingWakeLock();
	}

	public static void ReleaseLoadingWakeLock()
	{
		_ReleaseLoadingWakeLock();
	}

	public static void Init()
	{
		Debug.Log("UnityApi.Init");
		_Init();
	}

	public static void Enable()
	{
		Debug.Log("UnityApi.Enable");
		_Enable();
	}

	public static void Disable()
	{
		Debug.Log("UnityApi.Disable");
		_Disable();
	}

	public static SocialAspect CreateSocialAspect(MonoBehaviour behaviour)
	{
		return _CreateSocialAspect(behaviour);
	}

	internal static bool IsTwitterEnabled()
	{
		return true;
	}

	internal static void GoToTapjoy()
	{
		try
		{
			_GoToTapjoy();
		}
		catch (Exception ex)
		{
			Utils.Log("GoToTapjoy FAILED", ex.Message);
		}
	}

	internal static void ShowSupport()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder(supportKeys.Length * 60);
			string[] array = supportKeys;
			foreach (string text in array)
			{
				string text2 = SingletonT<ServerData>.I.GetPhrase(text);
				if (text2 == null)
				{
					text2 = string.Empty;
				}
				stringBuilder.Append(text);
				stringBuilder.Append("=");
				stringBuilder.Append(text2);
				stringBuilder.Append(_xcodeSplitter);
			}
			_ShowSupport(stringBuilder.ToString());
		}
		catch (Exception ex)
		{
			Utils.Log("ShowSupport FAILED", ex.Message);
		}
	}

	internal static void HideStartLoading()
	{
		try
		{
			_HideStartLoading();
		}
		catch (Exception ex)
		{
			Utils.Log("HideStartLoading FAILED", ex.Message);
		}
	}

	internal static void SendMainMenuToXCode()
	{
		try
		{
			_SendMainMenuToXCode();
		}
		catch (Exception ex)
		{
			Utils.Log("SendMainMenuToXCode FAILED", ex.Message);
		}
	}

	internal static void SendContinueToXCode(string id)
	{
		try
		{
			_SendContinueToXCode(id);
		}
		catch (Exception ex)
		{
			Utils.Log("SendContinueToXCode FAILED", ex.Message);
		}
	}

	internal static void OpenLeaderboard(string text)
	{
		try
		{
		}
		catch (Exception ex)
		{
			Utils.Log("OpenLeaderboard FAILED", text, ex.Message);
		}
	}

	internal static string GetServerSignatureSalt()
	{
		try
		{
			return _GetServerSignatureSalt();
		}
		catch (Exception ex)
		{
			Utils.Log("GetServerSignatureSalt FAILED", ex.Message);
			return string.Empty;
		}
	}

	internal static string GetOpenFeintAppId()
	{
		try
		{
			return _GetOpenFeintAppId();
		}
		catch (Exception ex)
		{
			Utils.Log("GetOpenFeintAppId FAILED", ex.Message);
			return string.Empty;
		}
	}

	internal static bool GetUseOpenFeintPurchases()
	{
		try
		{
			return _GetUseOpenFeintPurchases();
		}
		catch (Exception ex)
		{
			Utils.Log("GetUseOpenFeintPurchases FAILED", ex.Message);
			return false;
		}
	}

	internal static void BankBuy(ServerData.BankItem bankItem)
	{
		try
		{
			if (_UseIDreamSky())
			{
				_IDreamSkyBankBuy(bankItem);
			}
			else if (_UseGameClub())
			{
				_GameClubBankBuy(bankItem);
			}
			else if (GetUseOpenFeintPurchases())
			{
				_OpenFeintBankBuy(bankItem);
			}
			else
			{
				_BankBuy(bankItem.PurchaseId);
			}
		}
		catch (Exception ex)
		{
			Utils.Log("BunkBuy FAILED", bankItem.PurchaseId, ex.Message);
		}
	}

	private static string SocialMessage(ServerData.Achievement achiv, ServerData.PhrasesE phrase)
	{
		string phrase2 = SingletonT<ServerData>.I.GetPhrase(phrase);
		return phrase2.Fmt(achiv.Title) + _xcodeSplitter + achiv.Id;
	}

	internal static void FacebookSubmit(ServerData.Achievement achiv)
	{
		if (achiv == null)
		{
			return;
		}
		try
		{
			Metrics.OnPostToSocialClicked();
			string text = Utils.PutArg0(SingletonT<ServerData>.I.GetPhrase("SocialAchivMessageFacebook1"), achiv.Title);
			string text2 = Utils.PutArg0(SingletonT<ServerData>.I.GetPhrase("SocialAchivMessageFacebook2"), achiv.Title);
			if (text == null)
			{
				text = string.Empty;
			}
			if (text2 == null)
			{
				text2 = string.Empty;
			}
			string text3 = "{1}{0}{2}{0}{3}".Fmt(_xcodeSplitter, text, text2, SocialMessage(achiv, ServerData.PhrasesE.SocialAchivMessageFacebook));
			Utils.LogForce("FACEBOOK", text3);
			_FacebookSubmit(text3);
		}
		catch (Exception ex)
		{
			Utils.LogForce("_FacebookSubmit FAILED", ex.Message, ex.StackTrace);
		}
	}

	internal static void FacebookRatingSubmit(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			return;
		}
		try
		{
			Metrics.OnPostToSocialClicked();
			string phrase = SingletonT<ServerData>.I.GetPhrase("SocialAchivMessageFacebook1");
			string text = "{1}{0}{2}{0}{3}".Fmt(_xcodeSplitter, phrase, message, _xcodeSplitter + -1);
			Utils.Log("FACEBOOK", text);
			_FacebookSubmit(text);
		}
		catch (Exception ex)
		{
			Utils.Log("_FacebookSubmit FAILED", ex.Message);
		}
	}

	internal static void TwitterSubmit(ServerData.Achievement achiv)
	{
		if (achiv == null)
		{
			return;
		}
		try
		{
			Metrics.OnPostToSocialClicked();
			_TwitterSubmit(SocialMessage(achiv, ServerData.PhrasesE.SocialAchivMessageTwitter));
		}
		catch (Exception ex)
		{
			Utils.Log("_TwitterSubmit FAILED", ex.Message);
		}
	}

	internal static void TwitterRatingSubmit(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			return;
		}
		try
		{
			Metrics.OnPostToSocialClicked();
			Utils.Log("TWITTER", message);
			_TwitterSubmit(message + _xcodeSplitter + -1);
		}
		catch (Exception ex)
		{
			Utils.Log("_TwitterSubmit FAILED", ex.Message);
		}
	}

	internal static void SendGameStateToXCode()
	{
		try
		{
			int areaId = 0;
			int mobIndex = 0;
			int gold = 0;
			int diamonds = 0;
			int level = 0;
			bool flag = true;
			ServerData.PlayerParamsData playerParams = SingletonT<ServerData>.I.PlayerParams;
			if (playerParams != null)
			{
				gold = playerParams.MoneyGoldCount;
				diamonds = playerParams.MoneyDiamondCount;
				level = playerParams.Level;
			}
			else
			{
				flag = false;
			}
			ServerData.Location progressLocation = SingletonT<ServerData>.I.GetProgressLocation();
			int raceId = ((SingletonT<ServerData>.I.PlayerServerPersData != null) ? (raceId = SingletonT<ServerData>.I.PlayerServerPersData.Id) : 0);
			if (progressLocation != null)
			{
				areaId = progressLocation.Id;
				mobIndex = SingletonT<ServerData>.I.GetLocationProgress(progressLocation);
			}
			else
			{
				flag = false;
			}
			if (flag)
			{
				MetricController.AddGameState(level, gold, diamonds, areaId, mobIndex, raceId, SingletonT<ServerData>.I.GetAllExp());
			}
		}
		catch (Exception ex)
		{
			Utils.LogForce("SendGameStateToXCode FAILED", ex.Message);
		}
	}

	internal static void SendAchievToXCode()
	{
		MetricController.AddAchivStateAndSend(MainMenu.GameEvents.Events);
	}

	internal static void PostScreenshot()
	{
		try
		{
			_PostScreenshot();
		}
		catch (Exception ex)
		{
			Utils.Log("PostScreenshot FAILED", ex.Message);
		}
	}

	internal static void SendNewGameToXCode(string id)
	{
		try
		{
			_SendNewGameToXCode(id);
		}
		catch (Exception ex)
		{
			Utils.Log("SendNewGameToXCode FAILED", ex.Message);
		}
	}

	public static int GetNewMessage()
	{
		return 0;
	}

	public static void OpenMessage()
	{
	}

	public static void ActionComplete(string action)
	{
	}

	public static void AddMetric(int typeInt, int valueInt, int levelInt, int objectInt)
	{
		Utils.Log("*** ADDMETRIC", typeInt, valueInt, levelInt, objectInt);
		MetricController.AddMetric(typeInt, valueInt, levelInt, objectInt);
	}

	public static void OurGames()
	{
		if (Application.platform != RuntimePlatform.OSXEditor)
		{
			_OurGames();
		}
	}

	public static float GetGameVersion()
	{
		return 2.4f;
	}

	public static int GetUserMemory()
	{
		try
		{
			return 0;
		}
		catch (Exception)
		{
			return 0;
		}
	}

	public static int GetMonoHeap()
	{
		try
		{
			return 0;
		}
		catch (Exception)
		{
			return 0;
		}
	}

	public static int GetMonoUsedHeap()
	{
		try
		{
			return 0;
		}
		catch (Exception)
		{
			return 0;
		}
	}

	private static string _UnityGetLanguage()
	{
		if (_UseIDreamSky())
		{
			return "cn";
		}
		if (_UseGameClub())
		{
			return "ko";
		}
		if (Application.systemLanguage == SystemLanguage.Russian)
		{
			return "ru";
		}
		if (Application.systemLanguage == SystemLanguage.German)
		{
			return "de";
		}
		if (Application.systemLanguage == SystemLanguage.Spanish)
		{
			return "es";
		}
		if (Application.systemLanguage == SystemLanguage.French)
		{
			return "fr";
		}
		if (Application.systemLanguage == SystemLanguage.Polish)
		{
			return "pl";
		}
		if (Application.systemLanguage == SystemLanguage.Turkish)
		{
			return "tr";
		}
		if (Application.systemLanguage == SystemLanguage.Italian)
		{
			return "it";
		}
		if (Application.systemLanguage == SystemLanguage.Japanese)
		{
			return "ja";
		}
		if (Application.systemLanguage == SystemLanguage.Korean)
		{
			return "ko";
		}
		if (Application.systemLanguage == SystemLanguage.Chinese)
		{
			return "cn";
		}
		return "en";
	}

	public static string GetLanguage()
	{
		if (localization == null)
		{
			localization = _UnityGetLanguage();
		}
		return localization;
	}

	public static bool UseIDreamSky()
	{
		return _UseIDreamSky();
	}

	public static bool UseGameClub()
	{
		return _UseGameClub();
	}

	public static bool UseOK()
	{
		return false;
	}

	public static bool ShowScreenshotButton()
	{
		return !UseGameClub() && !UseOK();
	}

	public static bool NeedPurchaseConfirmation()
	{
		return _UseGameClub();
	}

	private static string TryGetPurchaseConfirmationPhrase(ServerData.BankItem bankItem)
	{
		string value = string.Empty;
		if (bankItem.CountType.Type == ServerData.MoneyType.TypeE.Diamond && PhrasesEx.TryGetValue("PurchaseConfirmationCrystal", out value))
		{
			return value;
		}
		if (bankItem.CountType.Type == ServerData.MoneyType.TypeE.Gold && PhrasesEx.TryGetValue("PurchaseConfirmationGold", out value))
		{
			return value;
		}
		return string.Empty;
	}

	public static string TranslatePurchaseConfirmation(ServerData.BankItem bankItem)
	{
		LoadPhrasesEx();
		if (PhrasesEx == null)
		{
			return string.Empty;
		}
		string text = TryGetPurchaseConfirmationPhrase(bankItem);
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		return string.Format(text, bankItem.Count + bankItem.Bonus, bankItem.Real);
	}

	public static string GetPath()
	{
		return Application.persistentDataPath;
	}

	public static void SetMetricController(MetricController metrics)
	{
		MetricController = metrics;
	}

	public static void GotoGameCenter()
	{
		try
		{
		}
		catch (Exception)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("!!!!! NO _GotoGameCenter");
			}
		}
	}

	private static void initAdman(string localizeName)
	{
		GetCurrentActivity().Call("initAdman", localizeName);
	}

	private static void admanStartShow()
	{
		GetCurrentActivity().Call("admanStartShow");
	}

	private static void admanShow()
	{
		GetCurrentActivity().Call("admanShow");
	}

	private static void admanStopShow()
	{
		GetCurrentActivity().Call("admanStopShow");
	}

	private static int getAdmanStatus()
	{
		return GetCurrentActivity().Call<int>("getAdmanStatus", new object[0]);
	}

	private static void completeAdManLazyInit()
	{
		GetCurrentActivity().Call("completeAdManLazyInit");
	}

	internal static void InitMyComAdman()
	{
		if (!InitMyComAdmanCalled)
		{
			InitMyComAdmanCalled = true;
			string text = "More Games!";
			text = SingletonT<ServerData>.I.GetPhrase("MyComShowWindow") + "|||" + SingletonT<ServerData>.I.GetPhrase("MyComBack");
			initAdman(text);
		}
	}

	internal static void MyComAdmanShow()
	{
		admanShow();
	}

	internal static void MyComAdmanStart()
	{
		admanStartShow();
	}

	internal static void MyComAdmanStop()
	{
		admanStopShow();
	}
}
