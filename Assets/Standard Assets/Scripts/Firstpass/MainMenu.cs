using System;
using System.Collections.Generic;
using System.Text;
using Common;
using UnityEngine;
using Yarx;
using Yarx.Collections;

internal class MainMenu : MonoBehaviour
{
	internal enum EventTypeE
	{
		ExtraChapterCongrats,
		ChapterDone,
		NewLevel,
		ComboOpened,
		MagicOpened,
		RageOpened,
		ExtraChapter,
		Achievment,
		XCodeRating
	}

	public class BuildAndroid
	{
		public static string MODEL { get; set; }

		public static string MANUFACTURER { get; set; }

		static BuildAndroid()
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build");
				MANUFACTURER = androidJavaClass.GetStatic<string>("MANUFACTURER");
				MODEL = androidJavaClass.GetStatic<string>("MODEL");
			}
		}
	}

	internal enum DestroyAllE
	{
		None,
		GoToMainMap,
		GoToLocation,
		DestroyEnemy,
		Restart,
		GoToMainMapFromStartMenu,
		DebugDestroyAll
	}

	public static readonly int SovereighnMobId = 492;

	public static readonly int UnaLocationId = 1182;

	public static readonly int SwampLocationId = 287;

	public Material MaterialBlood;

	public Battle Battle;

	public GameObject PrefabGuiBattleHud;

	public GameObject TutorialArrowPrefab;

	public GameObject TutorialCastMagicPrefab;

	public GameObject TutorialFatalityPrefab;

	public GameObject TutorialSliceAttackPrefab;

	private CompositeDisposable _listeners;

	private static Tutorials _tutorials = new Tutorials();

	private static GameEvents _gameEvents = ((!Globals.UseGameEvents) ? null : new GameEvents());

	private float _mobsRespCooldown;

	private float _metrics24Time = 60f;

	private static int Counter = 0;

	private int _mycounter;

	private PlayerState _playerState = new PlayerState();

	internal DestroyAllE _gotoFromBattle;

	internal GuiRoot.GuiType _lastLocationOrMap = GuiRoot.GuiType.None;

	internal GuiRoot.GuiType _lastLocationOrMapOrZachistka = GuiRoot.GuiType.None;

	internal GuiRoot.GuiType _lastMapOrZachistkaOrFight = GuiRoot.GuiType.None;

	internal List<Yarx.Collections.Tuple<EventTypeE, int>> _openConditionProgressChanged = new List<Yarx.Collections.Tuple<EventTypeE, int>>();

	private static bool SocialAuthProcessed = false;

	private Dictionary<string, Vector3> _storedPos = new Dictionary<string, Vector3>();

	private int _saveIndex = -1;

	private HudMk1 __hud;

	private static bool BuggyDeviceListInitialized = false;

	private static bool InverseScreenOrientation = false;

	private float timeA = 60f;

	private float timeB = 60f;

	private DestroyAllE __destroyAll;

	private StartMenu _start_menu2d;

	private bool _lastLoadedSceneIsSame;

	private GameEvents.Event _lastShownAchievment;

	private HudMk1.GuiDesc _lastShownAchievmentScreen = new HudMk1.GuiDesc(GuiRoot.GuiType.None, null);

	internal static string IOSCallAdmanLoadBannersDidFinishedLastCalledText = null;

	internal SocialAspect Social { get; private set; }

	public static GameEvents GameEvents => _gameEvents;

	public static Tutorials Tutorials => _tutorials;

	private bool IsInFight
	{
		get
		{
			GuiRoot.GuiType type = _hud.CurrentGui.Type;
			if (type == GuiRoot.GuiType.Fight || type == GuiRoot.GuiType.EnemyTurn || type == GuiRoot.GuiType.CastMagic || type == GuiRoot.GuiType.Execution || type == GuiRoot.GuiType.BattleHud || type == GuiRoot.GuiType.StrongMagicMiniGame || type == GuiRoot.GuiType.WeakMagicMiniGame)
			{
				return true;
			}
			return false;
		}
	}

	internal int SaveSlotIndex
	{
		get
		{
			return _saveIndex;
		}
		set
		{
			if (_saveIndex != value)
			{
				Utils.LogForce("SAVEINDEX!!!!", _saveIndex, "->", value);
				_saveIndex = value;
				if (value >= 0)
				{
					Messenger.Invoke(Globals.MsgNewSaveSlotIndex, value);
				}
			}
		}
	}

	internal HudMk1 _hud
	{
		get
		{
			return __hud;
		}
		set
		{
			__hud = value;
		}
	}

	internal DestroyAllE _destroyAll
	{
		get
		{
			return __destroyAll;
		}
		set
		{
			__destroyAll = value;
		}
	}

	public MainMenu()
	{
		Counter++;
		_mycounter = Counter;
	}

	internal bool TryStartTutorial(string name)
	{
		if (_tutorials != null)
		{
			return _tutorials.TryStartTutorial(name);
		}
		return false;
	}

	public override string ToString()
	{
		return base.ToString() + "_" + _mycounter;
	}

	private void PrintObjectsNames()
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		UnityEngine.Object[] array = UnityEngine.Object.FindSceneObjectsOfType(typeof(GameObject));
		foreach (UnityEngine.Object obj in array)
		{
			stringBuilder.Append(obj.name);
			stringBuilder.Append(", ");
		}
		Utils.Log(stringBuilder.ToString());
	}

	public bool OpenConditionsEmpty()
	{
		return _openConditionProgressChanged.Count == 0;
	}

	private void Awake()
	{
		Globals.MainMenu = this;
		UnityApi.SetMetricController(GetComponent<MetricController>());
		UniqueData.LoadSlots();
		if (SingletonT<ServerData>.I.LoadState == ServerData.StateE.None && _listeners == null)
		{
			_listeners = new CompositeDisposable();
			_listeners.Add(Messenger.AddListener(Globals.MsgGoToMainMenu, delegate
			{
				Battle.LeaveMode();
				Utils.Log("_destroyAll=", _gotoFromBattle);
				_destroyAll = ((_gotoFromBattle == DestroyAllE.None) ? DestroyAllE.GoToMainMap : _gotoFromBattle);
			}));
			_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
			_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPost, OnMsgGuiSwitchToPost));
			_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnMsgGuiSwitchToPre));
			_listeners.Add(Messenger<string>.AddListener(Globals.MsgGuiButtonPressed, ProcessButtons));
			_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnSwitchGui));
			_listeners.Add(Messenger.AddListener(Globals.MsgGameScreenChanged, OnMsgGameScreenChanged));
			_listeners.Add(Messenger.AddListener(Globals.MsgSelectScreenDataLoaded, delegate
			{
			}));
			_listeners.Add(Messenger<int>.AddListener(Globals.Msg_LocationOpenConditionProgressChanged, delegate(int locationId)
			{
				bool flag = false;
				foreach (Yarx.Collections.Tuple<EventTypeE, int> item in _openConditionProgressChanged)
				{
					if (item.Item1 == EventTypeE.ExtraChapter && item.Item2 == locationId)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					AddOpenCondition(EventTypeE.ExtraChapter, locationId);
				}
			}));
			_listeners.Add(Messenger<GameEvents.Event, string>.AddListener(Globals.MsgGameEventProgressChanged, delegate(GameEvents.Event @event, string reason)
			{
				if (!(reason != "ProgressChanged") && @event.Achievement != null && @event.Progress == @event.MaxProgress)
				{
					bool flag = false;
					foreach (Yarx.Collections.Tuple<EventTypeE, int> item2 in _openConditionProgressChanged)
					{
						if (item2.Item1 == EventTypeE.Achievment && item2.Item2 == @event.Achievement.Id)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						AddOpenCondition(EventTypeE.Achievment, @event.Achievement.Id);
					}
				}
			}));
			_listeners.Add(Messenger.AddListener(Globals.MsgFightStarted, delegate
			{
				_openConditionProgressChanged.Clear();
			}));
			_listeners.Add(Messenger.AddListener(Globals.MsgGuiExitExtraChapter, OnMsgGuiExitAchievmentOrExtraChapter));
			_listeners.Add(Messenger.AddListener(Globals.MsgGuiExitAchievments, OnMsgGuiExitAchievmentOrExtraChapter));
			_listeners.Add(Messenger.AddListener(Globals.MsgGuiExitSkill, OnMsgGuiExitAchievmentOrExtraChapter));
			_listeners.Add(Messenger<int, int, string>.AddListener(Globals.MsgPlayerLevelChanged, OnMsgPlayerLevelChanged));
			_listeners.Add(Messenger.AddListener(Globals.MsgFightStarted, OnMsgFightStarted));
			_listeners.Add(Messenger.AddListener(Globals.Msg_StartIntro_Finished, ShowSelectScreen));
			_listeners.Add(Messenger<ServerData.Location>.AddListener(Globals.MsgZachistkaDone, OnMsgZachistkaDone));
		}
		SingletonT<ServerData>.I.LoadFromImage(this, delegate
		{
			SingletonT<AtlasManager>.I.PreloadAtlases();
			Globals.ShowLoadingScreen(delegate
			{
				UnityApi.Init();
				if (SingletonT<ServerData>.I._phrases.Count > 0)
				{
					UnityApi.SetSubtitles();
				}
				UnityApi.HideStartLoading();
				Social = UnityApi.CreateSocialAspect(this);
				Social.ProcessAuthentication(delegate
				{
					Globals.RefreshLoadingScreen(delegate
					{
						SingletonT<SoundManager>.I.ForcePlayMenuMusic(null);
						SingletonT<ResourcesManager>.I.UnloadUnusedAssets(this, delegate
						{
							Globals.RefreshLoadingScreen(delegate
							{
								AwakeLoadData();
							});
						});
					});
				});
			});
		});
	}

	public void PlayMovieCallbackHandler(string message)
	{
		UnityApi.OnPlayMovieFinish();
	}

	public void OpenFeintCallbackHandler(string message)
	{
	}

	public void IDreamSkyCallbackHandler(string message)
	{
		IDreamSky.Proxy.ReceiveResponse(message);
	}

	public void GameClubCallbackHandler(string message)
	{
		GameClub.Proxy.ReceiveResponse(message);
	}

	public void OKCallbackHandler(string message)
	{
		OK.Proxy.ReceiveResponse(message);
	}

	private void AwakeLoadData()
	{
		if (Globals.DebugDontLoadConfig)
		{
			ServerDataLoaded();
			return;
		}
		if (SingletonT<ServerData>.I.LoadState == ServerData.StateE.Loaded)
		{
			ServerDataLoaded();
		}
		else
		{
			_listeners.Add(Messenger.AddListener(Globals.MsgInternalGameSaveLoaded, ServerDataLoaded));
		}
		SingletonT<ServerData>.I.LoadData(this);
	}

	private void OnMsgFightStarted()
	{
		if (!SingletonT<ServerData>.I.IsComboOpened)
		{
			HideHud("combos");
		}
		else
		{
			UnhideHud("combos");
		}
		if (!SingletonT<ServerData>.I.IsMagicOpened)
		{
			HideHud("mana_bar");
		}
		else
		{
			UnhideHud("mana_bar");
		}
		if (!SingletonT<ServerData>.I.IsRageOpened)
		{
			HideHud("rage_bar");
		}
		else
		{
			UnhideHud("rage_bar");
		}
	}

	private void HideHud(string name)
	{
		if (!(HudMk1.Instance == null))
		{
			Transform transform = HudMk1.Instance.transform.FindChildByName(name, includeInactive: true);
			if (transform != null && !_storedPos.ContainsKey(name))
			{
				_storedPos[name] = transform.transform.localPosition;
				Utils.Log("**** TUTORIAL STORE POS", name, transform.transform.position);
			}
			transform.transform.GoToHell();
		}
	}

	private void UnhideHud(string name)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		Vector3 value = default(Vector3);
		if (_storedPos.TryGetValue(name, out value))
		{
			Transform transform = HudMk1.Instance.transform.FindChildByName(name, includeInactive: true);
			Utils.Log("***** TUTORIAL RESTORE POS", name, value, transform);
			if (transform != null)
			{
				transform.transform.localPosition = value;
			}
		}
	}

	private void ShowAdvertising()
	{
		if ((UnityApi.GetLanguage() == "ru" && SingletonT<ServerData>.I.GameSettings.ShowEvolutionPromoInRu) || SingletonT<ServerData>.I.GameSettings.ShowEvolutionPromoInWorld)
		{
			int value = PlayerPrefs.GetInt("EVO_AD_COUNTER", 0);
			if (value++ < Globals.AD_COUNTER_LIMINT)
			{
				PlayerPrefs.SetInt("EVO_AD_COUNTER", value);
				_hud.ChangeGuiTo(GuiRoot.GuiType.Advertising);
			}
		}
	}

	private void OnMsgPlayerLevelChanged(int old, int @new, string reason)
	{
		if (reason != "AddPlayerExperience")
		{
			return;
		}
		if (old < SingletonT<ServerData>.I.GameSettings.SkillComboLevel && @new >= SingletonT<ServerData>.I.GameSettings.SkillComboLevel)
		{
			AddOpenCondition(EventTypeE.ComboOpened, 0);
		}
		if (old < SingletonT<ServerData>.I.GameSettings.SkillMagicLevel && @new >= SingletonT<ServerData>.I.GameSettings.SkillMagicLevel)
		{
			AddOpenCondition(EventTypeE.MagicOpened, 0);
			SetMagicButtonActive(state: true);
		}
		if (old < SingletonT<ServerData>.I.GameSettings.SkillRageLevel && @new >= SingletonT<ServerData>.I.GameSettings.SkillRageLevel)
		{
			AddOpenCondition(EventTypeE.RageOpened, 0);
		}
		List<ServerData.ShopGood> diffList = GotLevelItems.GetDiffList(old, @new);
		if (diffList.Count > 0)
		{
			ShopMk1.RecommendationsNeedRegeneration = true;
			if (!SingletonT<ServerData>.I.IsLoading && !HudMk1.Instance.IsLoadingPlayerStats)
			{
				Messenger.Invoke(Globals.MsgShopRecommendationsChanged);
			}
		}
	}

	private void SetMagicButtonActive(bool state)
	{
		if (!(HudMk1.Instance == null))
		{
			Transform transform = HudMk1.Instance.transform.FindChildByName("global_nav_3");
			SidebarButton componentInChildren = transform.GetComponentInChildren<SidebarButton>();
			if (state)
			{
				componentInChildren.SetActive();
			}
			else
			{
				componentInChildren.SetInactive();
			}
		}
	}

	private void OnMsgGuiSwitchToPost(GuiRoot.GuiType oldGui, GuiRoot.GuiType newGui)
	{
		if (newGui == GuiRoot.GuiType.MainMap || newGui == GuiRoot.GuiType.Location)
		{
			SetCameraActive(v: false);
			SingletonT<SoundManager>.I.PlayLocationMusic(null);
		}
		if (newGui == GuiRoot.GuiType.StartMenu)
		{
			Globals.HideLoadingScreen();
		}
		if (newGui == GuiRoot.GuiType.StartMenu && oldGui != GuiRoot.GuiType.Advertising)
		{
			ShowAdvertising();
		}
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType oldGui, GuiRoot.GuiType newGui)
	{
		if (newGui == GuiRoot.GuiType.FightOnLocation || newGui == GuiRoot.GuiType.Fight || newGui == GuiRoot.GuiType.BagItems || newGui == GuiRoot.GuiType.BagStats)
		{
			SetCameraActive(v: true);
		}
	}

	private void SetCameraActive(bool v)
	{
		GameObject gameObject = GameObject.Find("camera");
		if (gameObject != null)
		{
			gameObject.GetComponent<Camera>().enabled = v;
		}
	}

	private void OnMsgGuiSwitchToPre(GuiRoot.GuiType oldGui, GuiRoot.GuiType newGui)
	{
		if (newGui == GuiRoot.GuiType.MainMap && Globals.BuildType == Globals.BuildTypeE.InnerRelease)
		{
			Globals.HideDebugButtons();
		}
		if (oldGui == GuiRoot.GuiType.MainMap || oldGui == GuiRoot.GuiType.Location)
		{
			_lastLocationOrMap = oldGui;
		}
		if (oldGui == GuiRoot.GuiType.MainMap || oldGui == GuiRoot.GuiType.Fight || oldGui == GuiRoot.GuiType.FightOnLocation)
		{
			_lastMapOrZachistkaOrFight = oldGui;
		}
		if (oldGui == GuiRoot.GuiType.MainMap || oldGui == GuiRoot.GuiType.Location || oldGui == GuiRoot.GuiType.Fight)
		{
			_lastLocationOrMapOrZachistka = oldGui;
			if (oldGui == GuiRoot.GuiType.Fight)
			{
				Globals.Nav0GoToZachistka = false;
			}
		}
		if (newGui == GuiRoot.GuiType.Options || oldGui == GuiRoot.GuiType.Bank || oldGui == GuiRoot.GuiType.MainMap || oldGui == GuiRoot.GuiType.Shop || oldGui == GuiRoot.GuiType.MagicBook || oldGui == GuiRoot.GuiType.Location || oldGui == GuiRoot.GuiType.BagItems || oldGui == GuiRoot.GuiType.BagStats)
		{
			SaveGame();
		}
	}

	private void OnDestroy()
	{
		Utils.Dispose(ref _listeners);
	}

	private void OnEnable()
	{
		UnityApi.Enable();
	}

	private void OnDisable()
	{
		UnityApi.Disable();
	}

	private void ProcessButtons(string buttonName)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (buttonName.Contains("buy_money"))
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Bank);
			return;
		}
		if (buttonName.Contains("buy_diamonds"))
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Bank);
			return;
		}
		switch (buttonName)
		{
		case "global_nav_0":
			if (Globals.IsDebugBuild)
			{
				Debug.Log("------------ {0}".Fmt(buttonName));
			}
			Globals.Nav0GoToZachistka = false;
			if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap || HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Location)
			{
				ServerData.Location lastGameLocation = SingletonT<ServerData>.I.GetLastGameLocation();
				if (lastGameLocation != null)
				{
					_gotoFromBattle = DestroyAllE.None;
					Globals.Nav0GoToZachistka = true;
					AreaData.MakeCurrent(lastGameLocation, zachistka: true);
					GoToBattleFromMap();
				}
				else
				{
					GoToMainMap();
				}
			}
			else if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.FightOnLocation)
			{
				Utils.Log("nav_0 GuiRoot.GuiType.Location");
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Location);
			}
			else
			{
				GoToMainMap();
			}
			return;
		case "global_nav_1":
			if (SingletonT<ServerData>.I.PlayerParams != null && SingletonT<ServerData>.I.PlayerParams._skillPoints > 0)
			{
				_hud.ChangeGuiTo(GuiRoot.GuiType.BagStats);
			}
			else
			{
				_hud.ChangeGuiTo(GuiRoot.GuiType.BagItems);
			}
			return;
		case "global_nav_2":
			_hud.ChangeGuiTo(GuiRoot.GuiType.Shop);
			return;
		case "global_nav_3":
			_hud.ChangeGuiTo(GuiRoot.GuiType.MagicBook);
			return;
		case "global_nav_4":
			_hud.ChangeGuiTo(GuiRoot.GuiType.Options);
			return;
		case "_close_button":
		case "button_match3_restart":
			if (GuiRoot.ModalTypes.Contains(_hud.CurrentGui.Type))
			{
				_hud.PopModal();
			}
			else if (_hud.CurrentGui.Type != GuiRoot.GuiType.ChestMiniGame && _hud.CurrentGui.Type != GuiRoot.GuiType.Location)
			{
				_hud.ChangeGuiTo(Globals.MainMenu._lastLocationOrMapOrZachistka);
			}
			return;
		case "_alert_continue":
		case "_full_screen_catch_all_button":
			HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.GlobalAlertPopup);
			return;
		case "_achievement_button":
			_hud.ChangeGuiTo(GuiRoot.GuiType.AchievementsScroll);
			return;
		case "_free_crystal":
		case "_free_crystal_2":
			Metrics.OnTapJoy();
			UnityApi.GoToTapjoy();
			return;
		case "_tech_support":
			UnityApi.ShowSupport();
			return;
		}
		if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.SovereighnInfo)
		{
			if (buttonName == "button_sovereighn_info_continue")
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.MainMap);
			}
		}
		else if (buttonName == "evolution_ad" && _hud.CurrentGui.Type == GuiRoot.GuiType.Advertising)
		{
			Application.OpenURL(SingletonT<ServerData>.I.GameSettings.UrlEvolutionAndroid);
			_hud.PopModal();
		}
	}

	private void OnSwitchGui(GuiRoot.GuiType old, GuiRoot.GuiType current)
	{
		if (old != current)
		{
			if (old == GuiRoot.GuiType.Fight)
			{
			}
			if (current == GuiRoot.GuiType.Fight)
			{
			}
			if (current == GuiRoot.GuiType.Shop && ShopMk1.RecommendationsNeedRegeneration)
			{
				ShopMk1.RecommendationsNeedRegeneration = false;
				Messenger.Invoke(Globals.MsgShopGoodsReseted);
			}
			SidebarButton.ChangeMapToSwords(current == GuiRoot.GuiType.Location || current == GuiRoot.GuiType.MainMap);
			SetMagicButtonActive(SingletonT<ServerData>.I.IsMagicOpened);
		}
	}

	private void ShowSelectScreen()
	{
		Globals.ShowLoadingScreen(delegate
		{
			_hud.ChangeGuiTo(GuiRoot.GuiType.ChooseChar);
		});
	}

	private void LoadPlayerModel(Action onLoad)
	{
		if (Globals.Player != null || Globals.DebugDontLoadPlayer)
		{
			onLoad();
			return;
		}
		string id = ((SingletonT<ServerData>.I.PlayerServerPersData == null) ? "1" : SingletonT<ServerData>.I.PlayerServerPersData.ModelId);
		SingletonT<ResourcesManager>.I.CreatePerson(this, id, delegate(string _, GameObject PlayerGameObject)
		{
			PlayerGameObject.name = Globals.PlayerName;
			PlayerGameObject.SetActiveRecursivelyMk1(setActive: false);
			Globals.PlayerGameObject = PlayerGameObject;
			AfterPersonLoaded(onLoad);
		});
	}

	private void OnMsgGameScreenChanged()
	{
		if (Globals.GameScreen != Globals.GameScreenE.StartMenu)
		{
			if (Globals.GameScreen == Globals.GameScreenE.SelectPlayer)
			{
				SingletonT<SoundManager>.I.PlayMenuMusic(null);
				return;
			}
			SingletonT<SoundManager>.I.PlayLocationMusic(null);
			Globals.HideLoadingScreen();
		}
	}

	private void ServerDataLoaded()
	{
		Utils.LogForce("I_OnLoadEnd", this);
		if (_hud == null)
		{
			CreateHud();
			if (_gameEvents != null)
			{
				_gameEvents.Reset();
			}
			if (Social != null)
			{
				Social.SyncWithExternal();
			}
			SingletonT<SoundManager>.I.CacheGlobalSounds(this);
			_hud.EnsureGuiInitialized(null, delegate
			{
				_hud.ChangeGuiTo(GuiRoot.GuiType.StartMenu);
			});
		}
	}

	private void AfterPersonLoaded(Action onLoad)
	{
		PersonArmor personArmor = Globals.PlayerGameObject.GetComponent<PersonArmor>();
		string modelId = ((SingletonT<ServerData>.I.PlayerServerPersData == null) ? "1" : SingletonT<ServerData>.I.PlayerServerPersData.ModelId);
		personArmor.PutAllPlayerArmor(modelId, null, noWeapon: false, delegate
		{
			SingletonT<ResourcesManager>.I.LoadAnimations(Globals.DebugDontLoadAnimation, modelId, this, personArmor, delegate(string _)
			{
				FromAssetBundleAnimations fromAssetBundleAnimations = Globals.PlayerGameObject.AddComponent<FromAssetBundleAnimations>();
				fromAssetBundleAnimations.AnimationsAssetBundlePath = _;
				Utils.PrecacheAnimations(Globals.PlayerGameObject);
				OnMsgGameScreenChanged();
				SingletonT<SoundManager>.I.CacheGlobalSounds(this);
				SingletonT<SoundManager>.I.CacheSounds(this, modelId);
				onLoad();
			});
		});
	}

	private void OnApplicationPause(bool pause)
	{
		if (Globals.IsDebugInput)
		{
			Debug.Log($"+++ OnApplicationPause, pause: {pause}");
		}
		if (pause)
		{
			SpriteGui.DontReleaseButtons = true;
		}
		else
		{
			SpriteGui.DontReleaseButtons = false;
		}
	}

	internal void SaveGame()
	{
		if (Globals.IgnoreSaveGame || SaveSlotIndex < 0 || SingletonT<ServerData>.I.PlayerServerPersData == null)
		{
			return;
		}
		Utils.LogForce("**************SAVEGAME");
		if (!(GetComponent<SaveLoadProtobuf>() is ISaveLoad<PlayerState> saveLoad))
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogWarning("CANNOT FIND SaveLoadProtobuf COMPONENT");
			}
			return;
		}
		SingletonT<ServerData>.I.PlayerParams.BonusStarsForOldSave = 100;
		PlayerState playerState = new PlayerState();
		playerState.Version = 3;
		playerState.Inventory = SingletonT<ServerData>.I.BagAsArray();
		playerState.PlayerParams = SingletonT<ServerData>.I.PlayerParams;
		playerState.Locations = new List<ServerData.Location>(SingletonT<ServerData>.I._locations.Values);
		playerState.Spells = SingletonT<ServerData>.I.MySpells;
		playerState.PlayerPersDataId = ((SingletonT<ServerData>.I.PlayerServerPersData != null) ? SingletonT<ServerData>.I.PlayerServerPersData.Id : 0);
		playerState.LastLoadedSceneServerId = Globals.LastLoadedSceneServerId;
		playerState.TutorialsOn = Tutorials.Enabled;
		playerState.TutorialsState = Tutorials.InnerState;
		playerState.IsSovereighnFinalPlayed = Globals.IsSovereighnFinalPlayed;
		playerState.IsUnaIntrolPlayed = Globals.IsUnaIntroPlayed;
		playerState.IsSwampIntroPlayed = Globals.IsSwampIntroPlayed;
		playerState.BuyedMines = ServerData.BuyedMines;
		playerState.IsFinalScreenShowed = Globals.IsFinalScreenShowed;
		playerState.SaveTime = DateTime.Now.Ticks;
		PlayerState playerState2 = playerState;
		if (_gameEvents != null)
		{
			_gameEvents.SaveProgress();
		}
		Utils.Log("[Save]", playerState2.Version, playerState2.Inventory.Length, playerState2.PlayerPersDataId, playerState2.PlayerParams._mana, playerState2.PlayerParams._rageSpheresCount);
		saveLoad.Save(SaveSlotIndex, playerState2);
		Utils.Log("Save saved successed...");
	}

	internal PlayerState TryLoadSave(int i)
	{
		ISaveLoad<PlayerState> saveLoad = Globals.MainMenu.GetComponent<SaveLoadProtobuf>() as ISaveLoad<PlayerState>;
		return saveLoad.Load(i);
	}

	internal void LoadGame(int index)
	{
		if (Globals.DebugDontLoadSave)
		{
			return;
		}
		ISaveLoad<PlayerState> saveLoad = GetComponent<SaveLoadProtobuf>() as ISaveLoad<PlayerState>;
		_playerState = saveLoad.Load(index);
		Globals.LastLoadGameSuccessed = _playerState != null;
		if (Globals.LastLoadGameSuccessed)
		{
			Utils.Log("GAME LOADED", _playerState.Version, _playerState, _playerState.PlayerParams._rageSpheresCount, _playerState.PlayerParams._mana);
		}
		else
		{
			Utils.Log("GAME LOAD FAILED");
			Tutorials.Enabled = true;
		}
		SingletonT<ServerData>.I.LoadFrom(_playerState);
		if (_playerState != null)
		{
			ServerData.BuyedMines = _playerState.BuyedMines;
			if (ServerData.BuyedMines == null)
			{
				ServerData.BuyedMines = new List<int>();
			}
			Globals.IsSovereighnFinalPlayed = _playerState.IsSovereighnFinalPlayed;
			Globals.IsFinalScreenShowed = _playerState.IsFinalScreenShowed;
			Globals.LastLoadedSceneServerId = _playerState.LastLoadedSceneServerId;
			Globals.IsUnaIntroPlayed = _playerState.IsUnaIntrolPlayed;
			Globals.IsSwampIntroPlayed = _playerState.IsSwampIntroPlayed;
			Tutorials.Enabled = _playerState.TutorialsOn;
			Tutorials.InnerState = _playerState.TutorialsState;
		}
		if (SingletonT<ServerData>.I.GameSettings != null)
		{
			_mobsRespCooldown = SingletonT<ServerData>.I.GameSettings.MobsRespCooldown;
		}
		else
		{
			_mobsRespCooldown = 180f;
		}
	}

	private void OnApplicationQuit()
	{
	}

	private void Start()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		androidJavaObject.Call("registerOrLoginUserWithId", SystemInfo.deviceUniqueIdentifier);
		SingletonT<Fxs>.I.ToString();
#if UNITY_IOS
		iOS.DeviceGeneration generation = iOS.Device.generation;
		if (generation == iOS.DeviceGeneration.iPhone3G || generation == iOS.DeviceGeneration.iPhone3GS || generation == iOS.DeviceGeneration.iPhone4 || generation == iOS.DeviceGeneration.iPhone4S)
		{
			Application.targetFrameRate = 30;
		}
		else
		{
			Application.targetFrameRate = Globals.DefaultFPS;
		}
		if (iOS.Device.generation == iOS.DeviceGeneration.iPodTouch3Gen || iOS.Device.generation == iOS.DeviceGeneration.iPodTouch4Gen || iOS.Device.generation == iOS.DeviceGeneration.iPhone3GS || iOS.Device.generation == iOS.DeviceGeneration.iPad1Gen)
		{
			QualitySettings.masterTextureLimit = ((!Globals.DebugMinMapMax) ? 1 : 4);
		}
		if (iOS.Device.generation == iOS.DeviceGeneration.iPad2Gen)
		{
		}
#endif
		ShowStartMenu();
	}

	private void PreloadPersAtlases()
	{
		ServerData.PersData playerServerPersData = SingletonT<ServerData>.I.PlayerServerPersData;
		int setNumber = playerServerPersData?.Class ?? 1;
		if (playerServerPersData == null && Globals.IsDebugBuild)
		{
			Debug.LogError("[]========================[ ServerData.I.PlayerPersData == null ]=========================[]");
		}
		SingletonT<AtlasManager>.I.PreloadSetAtlases(setNumber);
	}

	private void CreateHud()
	{
		if (!(_hud != null) && !Globals.IgnoreHud)
		{
			_hud = (HudMk1)UnityEngine.Object.FindObjectOfType(typeof(HudMk1));
			if (_hud == null)
			{
				_hud = ((GameObject)UnityEngine.Object.Instantiate(Util.Resource<UnityEngine.Object>("_hud_mk1"))).GetComponent<HudMk1>();
				_hud.Release += mainMenu2d_Click;
				_hud.Release += MainMap_Click;
			}
			_hud.transform.parent = base.transform;
		}
	}

	private void DestroyHud()
	{
		if (!(_hud == null))
		{
		}
	}

	internal void DestroyEnemy()
	{
		Enemy enemy = Globals.Enemy;
		if (!(enemy == null))
		{
			Globals.Enemy = null;
			UnityEngine.Object.Destroy(enemy.transform.root.gameObject);
		}
	}

	private void DestroyAllForce()
	{
		Utils.Log("DestroyAllForce");
		GameObject gameObject = null;
		if (Battle != null)
		{
			if (Battle.SelectGui != null)
			{
				if (Battle.SelectGui.Hud != null)
				{
					gameObject = Battle.SelectGui.Hud.gameObject;
					Battle.SelectGui.Hud = null;
					UnityEngine.Object.Destroy(gameObject);
				}
				gameObject = Battle.SelectGui.gameObject;
				Battle.SelectGui = null;
				UnityEngine.Object.Destroy(gameObject);
			}
			gameObject = Battle.BattleGui.gameObject;
			Battle.BattleGui = null;
			UnityEngine.Object.Destroy(gameObject);
			Battle.FullSector = null;
		}
		SingletonT<SoundManager>.I.OnNewEnemy(Globals.Player, Globals.Enemy);
		Globals.Enemy = null;
		Globals.Player = null;
		Battle = null;
		DestroyAll(null);
	}

	private void EnsureCorrectScreenOrientation()
	{
		if (!BuggyDeviceListInitialized)
		{
			InverseScreenOrientation = BuildAndroid.MANUFACTURER == "Amazon" && (BuildAndroid.MODEL == "WFJWI" || BuildAndroid.MODEL == "WFJWA" || BuildAndroid.MODEL == "KFTT");
			BuggyDeviceListInitialized = true;
		}
		if (InverseScreenOrientation)
		{
			if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
			{
				Screen.orientation = ScreenOrientation.LandscapeRight;
			}
			else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
			{
				Screen.orientation = ScreenOrientation.LandscapeLeft;
			}
		}
	}

	private void Update()
	{
		try
		{
			if (SingletonT<ServerData>.I != null)
			{
				SingletonT<ServerData>.I.Update(Time.deltaTime);
			}
			if (_tutorials != null)
			{
				_tutorials.Update();
			}
			if (_destroyAll != DestroyAllE.None)
			{
				if (_destroyAll == DestroyAllE.DebugDestroyAll)
				{
					_destroyAll = DestroyAllE.None;
					DestroyAllForce();
				}
				else if (_destroyAll == DestroyAllE.GoToMainMapFromStartMenu)
				{
					_destroyAll = DestroyAllE.None;
					DestroyAllForce();
					LoadPlayerModel(delegate
					{
						AreaData.MakeCurrent(SingletonT<ServerData>.I.GetLastGameLocation(), zachistka: true);
						GoToBattleFromMap();
					});
				}
				else
				{
					Utils.Log("_destroyAll=", _destroyAll);
					DestroyAllForce();
					Utils.Log("_destroyAll2=", _destroyAll);
					DestroyAllE destroyAll = _destroyAll;
					_destroyAll = DestroyAllE.None;
					switch (destroyAll)
					{
					case DestroyAllE.GoToMainMap:
						GoToMainMap();
						break;
					case DestroyAllE.GoToLocation:
						GoToLocation(AreaData.Current.Location);
						break;
					}
				}
			}
			SingletonT<ServerData>.I.UpdateAllLocationsLogic();
			UpdateBotsRespawn();
			SingletonT<TimeEventsManager>.I.Update(Time.deltaTime);
			if (Application.platform == RuntimePlatform.Android && (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Menu)))
			{
				NavigateBack();
			}
		}
		catch (Exception ex)
		{
			Debug.Log("SOMEEXINUPDATE " + ex.Message);
		}
		EnsureCorrectScreenOrientation();
	}

	private void UpdateBotsRespawn()
	{
		if (Globals.GameDataLoaded)
		{
			if (_mobsRespCooldown < 0f)
			{
				int num = 0;
				List<LocationLogic> list = new List<LocationLogic>();
				foreach (KeyValuePair<int, ServerData.Location> location in SingletonT<ServerData>.I._locations)
				{
					if (location.Value.Logic.IsOpened)
					{
						num += location.Value.Logic._mobs.Count;
						if (location.Value.Logic._mobs.Count < location.Value.RespawnMax)
						{
							list.Add(location.Value.Logic);
						}
					}
				}
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				ServerData.MobRespProb[] mobsRespawnProbs = SingletonT<ServerData>.I.GameSettings.MobsRespawnProbs;
				for (int i = 0; i < mobsRespawnProbs.Length; i++)
				{
					ServerData.MobRespProb mobRespProb = mobsRespawnProbs[i];
					if (mobRespProb.Count == num)
					{
						num2 = mobRespProb.Prob;
						num3 = num;
						break;
					}
					if (num3 < mobRespProb.Count)
					{
						num3 = mobRespProb.Count;
						num4 = mobRespProb.Prob;
					}
				}
				if (num > num3)
				{
					num2 = num4;
				}
				if (list.Count > 0 && UnityEngine.Random.Range(0, 100) < num2)
				{
					LocationLogic locationLogic = list[UnityEngine.Random.Range(0, list.Count)];
					locationLogic.RespawnMob();
				}
				_mobsRespCooldown = SingletonT<ServerData>.I.GameSettings.MobsRespCooldown;
			}
			else
			{
				timeA -= Time.deltaTime;
				if (timeA < 0f)
				{
					timeA = 60f;
				}
				_mobsRespCooldown -= Time.deltaTime;
			}
		}
		else
		{
			timeB -= Time.deltaTime;
			if (timeB < 0f)
			{
				timeB = 60f;
			}
		}
	}

	private void DestroyAll(string dontDestoryEnemyWithName)
	{
		Transform transform = ((!(Globals.PlayerGameObject != null)) ? null : Globals.PlayerGameObject.transform.root);
		int num = ((AreaData.Current == null) ? (-1) : AreaData.Current.Location.MapModel);
		UnityEngine.Object[] array = UnityEngine.Object.FindSceneObjectsOfType(typeof(GameObject));
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = (GameObject)array[i];
			if (!(gameObject == base.gameObject))
			{
				Transform root = gameObject.transform.root;
				if ((!(root == transform) || _destroyAll == DestroyAllE.Restart) && !(root.name == "FPS") && !(root.name == Globals.LocationGameObjectMainMenu) && (_destroyAll != DestroyAllE.GoToLocation || !(root.name == Globals.LocationGameObjectLocationName)) && (num != SingletonT<ResourcesManager>.I.LastLoadedSceneIndex || !(root.name == Globals.LocationGameObjectSceneGeomName)) && (dontDestoryEnemyWithName == null || !(root.name == dontDestoryEnemyWithName)) && gameObject.transform.parent == null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}
		_lastLoadedSceneIsSame = false;
		if (_destroyAll == DestroyAllE.Restart)
		{
			_destroyAll = DestroyAllE.None;
			SingletonT<AtlasManager>.I.Clear();
			Globals.MainMenu = null;
			Globals.Battle = null;
			Globals.Player = null;
			Globals.Enemy = null;
			if (Globals.PlayerGameObject != null)
			{
				GameObject playerGameObject = Globals.PlayerGameObject;
				Globals.PlayerGameObject = null;
				UnityEngine.Object.Destroy(playerGameObject);
			}
			Globals.Reload(this);
		}
	}

	internal void GoToMainMap()
	{
		SingletonT<Fxs>.I.CleanFxCache();
		Globals.GameScreen = Globals.GameScreenE.Map;
		_hud.ChangeGuiTo(GuiRoot.GuiType.MainMap);
		Globals.HideLoadingScreen();
	}

	internal void GoToOpenedLocation(ServerData.Location location)
	{
		_gotoFromBattle = DestroyAllE.None;
		AreaData.MakeCurrent(location, zachistka: true);
		GoToBattleFromMap();
	}

	private void MainMap_Click(SpriteButton button)
	{
		if (HudMk1.Instance == null || _hud.CurrentGui.Type != GuiRoot.GuiType.MainMap)
		{
			return;
		}
		ServerData.Location location = null;
		if (button.name == "button_reset")
		{
			NewGame();
		}
		else if (button.name == "button_save")
		{
			SaveGame();
		}
		else if (button.name == "button_load_config")
		{
			SingletonT<ServerData>.I.TryLoadRemoteData(this);
		}
		else if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap && (IsLocationButton(button, MainMapHud.ZachistkaId + "/", ref location) || IsLocationButton(button, MainMapHud.BossId + "/", ref location) || IsLocationButton(button, MainMapHud.AltId + "/", ref location)))
		{
			if (IsLocationButton(button, MainMapHud.BossId + "/", ref location))
			{
				if (location.Logic.OpenCondition == null || (location.Logic.OpenCondition != null && location.Logic.OpenCondition.Progress >= location.Logic.OpenCondition.MaxProgress) || location.Logic.OpenCondition.Done)
				{
					if (location.Id == UnaLocationId && (!Globals.IsUnaIntroPlayed || Globals.ForceVideoPlayback))
					{
						SpriteGui.DontReleaseButtons = true;
						UnityApi.PlayMovie("4", delegate
						{
							SpriteGui.DontReleaseButtons = false;
							Globals.IsUnaIntroPlayed = true;
							GoToOpenedLocation(location);
						});
					}
					else if (location.Id == SwampLocationId && (!Globals.IsSwampIntroPlayed || Globals.ForceVideoPlayback))
					{
						SpriteGui.DontReleaseButtons = true;
						UnityApi.PlayMovie("5", delegate
						{
							SpriteGui.DontReleaseButtons = false;
							Globals.IsSwampIntroPlayed = true;
							GoToOpenedLocation(location);
						});
					}
					else
					{
						GoToOpenedLocation(location);
					}
				}
				else
				{
					_hud.ChangeGuiTo(new HudMk1.GuiDesc(GuiRoot.GuiType.ExtraChapterInfo, Yarx.Collections.Tuple.Create(location, location.Logic.OpenCondition.Progress)));
				}
			}
			else
			{
				_gotoFromBattle = DestroyAllE.None;
				AreaData.MakeCurrent(location, zachistka: true);
				GoToBattleFromMap();
			}
		}
		else if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap && IsLocationButton(button, MainMapHud.MoneyId + "/", ref location))
		{
			GoToLocation(location);
		}
		else if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap && IsLocationButton(button, MainMapHud.ChestId + "/", ref location))
		{
			GoToLocation(location);
		}
		else if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap && IsLocationButton(button, MainMapHud.MobId + "/", ref location))
		{
			GoToLocation(location);
		}
		else if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap && IsLocationButton(button, MainMapHud.FlagId + "/", ref location))
		{
			GoToLocation(location);
		}
		else if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap && IsLocationButton(button, MainMapHud.CaveId + "/", ref location))
		{
			GoToMatch3(location);
		}
	}

	private void GoToMatch3(ServerData.Location location)
	{
		if (!(HudMk1.Instance == null))
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Match3StartScreen, Yarx.Collections.Tuple.Create(location.CaveName));
		}
	}

	private void RemoveMainMenu2d()
	{
	}

	private void ShowStartMenu()
	{
		Globals.GameScreen = Globals.GameScreenE.StartMenu;
		if (Globals.DebugStartMenuSimple)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = Globals.LocationGameObjectStartMenuName;
			gameObject.AddComponent<StartMenuSimple>();
		}
	}

	private void GoToPlayerSelectMenu()
	{
		Globals.GameScreen = Globals.GameScreenE.SelectPlayer;
		Globals.ShowLoadingScreen(delegate
		{
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.1f, delegate
			{
				if (!Globals.ShowIntro)
				{
					ShowSelectScreen();
				}
				else
				{
					SingletonT<ResourcesManager>.I.UnloadUnusedAssets(this, delegate
					{
						GameObject original = Util.Resource<GameObject>("intro/prefabs/_intro");
						GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(original);
						gameObject.name = Globals.LocationGameObjectStartIntroName;
						gameObject.GetComponent<Camera>().enabled = true;
						IntroScreen component = gameObject.GetComponent<IntroScreen>();
						Globals.HideLoadingScreen();
					});
				}
			});
		});
	}

	public void GoToStartMenu()
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (TutorialFullScreenInfo.IsShowDialog)
		{
			TutorialFullScreenInfo componentInChildren = HudMk1.Instance.GetComponentInChildren<TutorialFullScreenInfo>();
			componentInChildren.RemoveTutorial();
		}
		Globals.ShowLoadingScreen(delegate
		{
			SingletonT<ResourcesManager>.I.UnloadUnusedAssets(this, delegate
			{
				UnityApi.SendMainMenuToXCode();
				Restart();
			});
		});
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void NavigateBack()
	{
		if (Globals.GameScreen == Globals.GameScreenE.StartMenu)
		{
			ExitGame();
		}
		else if (Globals.InFight)
		{
			if (!TutorialFullScreenInfo.IsShowDialog)
			{
				Globals.TogglePauseResume();
			}
		}
		else
		{
			GoToStartMenu();
		}
	}

	public void ChooseActivePlayerClicked()
	{
		_hud.ChangeGuiTo(GuiRoot.GuiType.None);
		NewGame();
		if (SingletonT<ServerData>.I.PlayerServerPersData.Bonus != null)
		{
			ServerData.Item item = SingletonT<ServerData>.I.PlayerServerPersData.Bonus.MakeRealItem(forShop: false, 1);
			item.PutOn = false;
			SingletonT<ServerData>.I.AddToBag(item);
		}
		if (Globals.MyDebug)
		{
			SingletonT<ServerData>.I.PlayerParams.AddMoney(ServerData.MoneyType.TypeE.Diamond, 100000);
			SingletonT<ServerData>.I.PlayerParams.AddMoney(ServerData.MoneyType.TypeE.Gold, 10000000);
			SingletonT<ServerData>.I.PlayerParams.AddMoney(ServerData.MoneyType.TypeE.Key, 10000);
			SingletonT<ServerData>.I.PlayerParams.AddMoney(ServerData.MoneyType.TypeE.Skull, 10000);
			SingletonT<ServerData>.I.PlayerParams.AddMoney(ServerData.MoneyType.TypeE.Scarab, 1000);
			SingletonT<ServerData>.I.PlayerParams.AddMoney(ServerData.MoneyType.TypeE.Star, 1000);
			SingletonT<ServerData>.I._locations[282].OpenAfter = SingletonT<ServerData>.I.GetLocationByServerId(52);
			SingletonT<ServerData>.I._locations[283].OpenAfter = SingletonT<ServerData>.I.GetLocationByServerId(52);
			SingletonT<ServerData>.I._locations[286].OpenAfter = SingletonT<ServerData>.I.GetLocationByServerId(52);
			SingletonT<ServerData>.I._locations[1182].OpenAfter = SingletonT<ServerData>.I.GetLocationByServerId(52);
			SingletonT<ServerData>.I._locations[1862].OpenAfter = SingletonT<ServerData>.I.GetLocationByServerId(52);
		}
		SaveGame();
		UnityApi.SendNewGameToXCode((SaveSlotIndex + 1).ToString());
		Globals.ShowLoadingScreen(delegate
		{
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.2f, delegate
			{
				SingletonT<ResourcesManager>.I.UnloadUnusedAssets(this, delegate
				{
					LoadPlayerModel(delegate
					{
						AreaData.MakeCurrent(SingletonT<ServerData>.I.GetStartGameLocation(), zachistka: true);
						GoToBattleFromMap(delegate
						{
							Utils.Log("**** START PLAY SELECTED VIDEO");
							SpriteGui.DontReleaseButtons = true;
							UnityApi.PlayMovie("2", delegate
							{
								SpriteGui.DontReleaseButtons = false;
								Utils.Log("**** STOP PLAY SELECTED VIDEO");
								if (!SingletonT<ServerData>.I.IsMagicOpened)
								{
									SetMagicButtonActive(state: false);
								}
							});
						});
					});
				});
			});
		});
	}

	internal void GoToFromStartMenuToMainMap()
	{
		Globals.ShowLoadingScreen(delegate
		{
			_destroyAll = DestroyAllE.GoToMainMapFromStartMenu;
		});
	}

	internal void LoadGameThenContinue(int index)
	{
		Globals.PlayerGameObject = null;
		Globals.ShowLoadingScreen(delegate
		{
			LoadGame(index);
			SaveSlotIndex = index;
			UnityApi.SendContinueToXCode((SaveSlotIndex + 1).ToString());
			Globals.GameDataLoaded = true;
			PreloadPersAtlases();
			if (Social != null)
			{
				Social.SyncWithExternal();
			}
			_destroyAll = DestroyAllE.GoToMainMapFromStartMenu;
			Utils.FindObjectOfTypeNoThrow<HudMk1>().InitPlayerStats();
			Messenger.Invoke(Globals.MsgLoadThenContinueGame);
		});
	}

	private void mainMenu2d_Click(SpriteButton button)
	{
		if ((Globals.GameScreen == Globals.GameScreenE.StartMenu || Globals.GameScreen == Globals.GameScreenE.SelectPlayer) && (!(button.name == "choose_active_char") || Globals.GameScreen != Globals.GameScreenE.SelectPlayer))
		{
			if (button.name == "_alert_ok")
			{
				_start_menu2d.HideAlert();
				StartNewGame();
			}
			else if (button.name == "_alert_cancel")
			{
				_start_menu2d.HideAlert();
			}
		}
	}

	internal void StartNewGame()
	{
		if (Social != null)
		{
			Social.SyncWithExternal();
		}
		GoToPlayerSelectMenu();
		Globals.GameDataLoaded = true;
		if (SingletonT<ServerData>.I.GameSettings != null)
		{
			_mobsRespCooldown = SingletonT<ServerData>.I.GameSettings.MobsRespCooldown;
		}
		else
		{
			_mobsRespCooldown = 180f;
		}
	}

	internal void GoToLocation(ServerData.Location location)
	{
		AreaData.MakeCurrent(location, zachistka: false);
		Globals.GameScreen = Globals.GameScreenE.Location;
		Utils.Log("**********GOTOLOCATION", location);
		_hud.ChangeGuiTo(new HudMk1.GuiDesc(GuiRoot.GuiType.Location, Yarx.Collections.Tuple.Create(location)));
	}

	private bool IsLocationButton(SpriteButton button, string prefix, ref ServerData.Location location)
	{
		if (button.name.StartsWith(prefix))
		{
			int result = 0;
			if (int.TryParse(button.name.Substring(prefix.Length), out result))
			{
				ServerData.Location locationByServerId = SingletonT<ServerData>.I.GetLocationByServerId(result);
				if (locationByServerId != null)
				{
					location = locationByServerId;
				}
			}
		}
		return location != null;
	}

	private void NewGame()
	{
		_lastShownAchievment = null;
		if (_gameEvents != null)
		{
			_gameEvents.Reset();
		}
		_tutorials.OnNewGame();
		Globals.Nav0GoToZachistka = false;
		Globals.IsSovereighnFinalPlayed = false;
		Globals.IsUnaIntroPlayed = false;
		_openConditionProgressChanged.Clear();
		if (_playerState != null)
		{
			_playerState = null;
		}
		SingletonT<ServerData>.I.NewGame();
		Metrics.OnNewGame();
		if (Globals.PlayerGameObject != null)
		{
			Globals.PlayerGameObject.GetComponent<PersonArmor>().PutAllPlayerArmor();
		}
		PreloadPersAtlases();
		Utils.FindObjectOfTypeNoThrow<HudMk1>().InitPlayerStats();
		Messenger.Invoke(Globals.MsgNewGame);
	}

	private Camera CreateBattleCamera()
	{
		GameObject gameObject = SingletonT<ResourcesManager>.I.CreateSceneObject("__battle_camera");
		gameObject.name = Globals.LocationGameObjectBattleCamera;
		return gameObject.transform.FindChildByName("camera_upper").GetComponent<Camera>();
	}

	private void GoToBattleFromLocation(ServerData.BotInfo bot)
	{
		GoToBattleFromLocation(bot, -1);
	}

	private void GoToBattleFromLocation(ServerData.BotInfo bot, int addLevel)
	{
		_gotoFromBattle = DestroyAllE.GoToLocation;
		BattleResultsHud.ResetFightResultsStats();
		LocationLogic._chapterBonus = null;
		Globals.GameScreen = Globals.GameScreenE.Battle;
		Globals.ShowLoadingScreen(delegate
		{
			DestroyAll(Globals.EnemyName(bot.Id.ToString()));
			Battle battle = SingletonT<ResourcesManager>.I.CreateSceneObject<Battle>("__battle");
			battle.BubbleCam = CreateBattleCamera();
			battle.BattleGui = battle.GetComponent<BattleGui>();
			battle.SelectGui = battle.GetComponent<SelectGui>();
			_hud.ChangeGuiTo(GuiRoot.GuiType.FightOnLocation);
			Battle = battle;
			LoadScene(AreaData.Current.Location, delegate(string landPath, GameObject _)
			{
				battle._landPrefab = _;
				battle._landPrefabPath = landPath;
				ServerData.Settings gameSettings = SingletonT<ServerData>.I.GameSettings;
				int num = 5;
				if (addLevel < 0)
				{
					num = bot.Level;
				}
				else
				{
					Debug.Break();
					num = SingletonT<ServerData>.I.PlayerParams.Level + addLevel;
				}
				Utils.Log("**** FIGHTFROMLOCATION", addLevel, num);
				if (num <= 0)
				{
					num = 1;
				}
				AreaData.MobData mobData = new AreaData.MobData(bot, num, isBoss: false, "FromLocation", fromLocation: true);
				battle.DoStart(mobData);
				battle.ChangeEnemy(mobData, delegate
				{
					battle.EnterMode();
					Globals.Player.RemoveBerserk();
					battle.GetComponent<Battle>().SetupEnemyOnScene();
					Globals.HideLoadingScreen();
					FightOnLocationHud componentInChildren = _hud.GetComponentInChildren<FightOnLocationHud>();
					componentInChildren.Init(mobData);
				});
			});
		});
	}

	private void GoToBattleFromMap()
	{
		GoToBattleFromMap(null);
	}

	private void GoToBattleFromMap(ActionD action)
	{
		int locationProgress = SingletonT<ServerData>.I.GetLocationProgress(AreaData.Current.Location);
		AreaData.MobData mob = null;
		if (locationProgress < AreaData.Current.Mobs.Length)
		{
			mob = AreaData.Current.Mobs[locationProgress];
		}
		string enemyName = ((mob != null) ? Globals.EnemyName(mob.ServerInfo.Id.ToString()) : string.Empty);
		if (mob != null && mob.ServerInfo.Id == SovereighnMobId && (!Globals.IsSovereighnFinalPlayed || Globals.ForceVideoPlayback))
		{
			SpriteGui.DontReleaseButtons = true;
			UnityApi.PlayMovie("3", delegate
			{
				SpriteGui.DontReleaseButtons = false;
				Globals.IsSovereighnFinalPlayed = true;
				GoToBattleFromMap(action, mob, enemyName);
			});
		}
		else
		{
			GoToBattleFromMap(action, mob, enemyName);
		}
	}

	private void GoToBattleFromMap(ActionD action, AreaData.MobData mob, string enemyName)
	{
		Utils.Log("**** GoToBattleFromMap", mob, enemyName);
		Globals.GameScreen = Globals.GameScreenE.Battle;
		RemoveMainMenu2d();
		DestroyAll(enemyName);
		Battle battle = SingletonT<ResourcesManager>.I.CreateSceneObject<Battle>("__battle");
		battle.BubbleCam = CreateBattleCamera();
		battle.BattleGui = battle.GetComponent<BattleGui>();
		battle.SelectGui = battle.GetComponent<SelectGui>();
		if (!Globals.IgnoreHud)
		{
			_hud.ChangeGuiTo(GuiRoot.GuiType.Fight);
		}
		else
		{
			GuiRoot.GuiType arg = GuiRoot.GuiType.None;
			GuiRoot.GuiType arg2 = GuiRoot.GuiType.Fight;
			Messenger.Invoke(Globals.MsgGuiSwitchToBefore, arg, arg2);
			Messenger.Invoke(Globals.MsgGuiSwitchToPre, arg, arg2);
			Messenger.Invoke(Globals.MsgGuiSwitchToPost, arg, arg2);
		}
		Battle = battle;
		LoadScene(AreaData.Current.Location, delegate(string landPath, GameObject _)
		{
			battle._landPrefab = _;
			battle._landPrefabPath = landPath;
			battle.DoStart(mob);
			battle.EnterMode();
			if (Globals.Player != null)
			{
				Globals.Player.RemoveBerserk();
			}
			if (action != null)
			{
				action();
			}
		});
	}

	private void LoadScene(ServerData.Location location, ActionD<string, GameObject> action)
	{
		int index = location.MapModel;
		Globals.LastLoadedSceneServerId = location.Id;
		if (index == SingletonT<ResourcesManager>.I.LastLoadedSceneIndex)
		{
			GameObject gameObject = GameObject.Find(Globals.LocationGameObjectSceneGeomName);
			if (gameObject != null)
			{
				_lastLoadedSceneIsSame = true;
				action(null, null);
				return;
			}
		}
		_lastLoadedSceneIsSame = false;
		Globals.ShowLoadingScreen(delegate
		{
			SingletonT<ResourcesManager>.I.LoadScene(this, index, action);
		});
	}

	private void SortOpenConditions()
	{
		_openConditionProgressChanged.Sort((Yarx.Collections.Tuple<EventTypeE, int> x, Yarx.Collections.Tuple<EventTypeE, int> y) => x.Item1.CompareTo(y.Item1));
	}

	internal void StartShowAfterFightScreens()
	{
		SortOpenConditions();
		OnMsgGuiExitAchievmentOrExtraChapter();
	}

	internal void AddOpenCondition(EventTypeE type, int value)
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log(string.Concat("++++++++++++++ADD type ", type, " value ", value));
		}
		_openConditionProgressChanged.Add(Yarx.Collections.Tuple.Create(type, value));
	}

	internal void OnMsgGuiExitAchievmentOrExtraChapter()
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (_lastShownAchievment != null)
		{
			Utils.Log("*********** 1");
			_lastShownAchievment = null;
			_hud.ChangeGuiTo(_lastShownAchievmentScreen);
		}
		else if (Battle.BattleGui != null)
		{
			Utils.Log("*********** 2", _openConditionProgressChanged.Count);
			if (_openConditionProgressChanged.Count > 0)
			{
				Utils.Log("*********** 3");
				SortOpenConditions();
				Yarx.Collections.Tuple<EventTypeE, int> tuple = _openConditionProgressChanged[0];
				_openConditionProgressChanged.RemoveAt(0);
				switch (tuple.Item1)
				{
				case EventTypeE.ExtraChapterCongrats:
					_hud.ChangeGuiTo(GuiRoot.GuiType.ExtraChapterCongratulations);
					break;
				case EventTypeE.ExtraChapter:
				{
					ServerData.Location locationByServerId = SingletonT<ServerData>.I.GetLocationByServerId(tuple.Item2);
					if (locationByServerId != null)
					{
						_hud.ChangeGuiTo(new HudMk1.GuiDesc(GuiRoot.GuiType.ExtraChapterInfo, Yarx.Collections.Tuple.Create(locationByServerId, locationByServerId.Logic.OpenCondition.Progress - 1)));
					}
					break;
				}
				case EventTypeE.Achievment:
					ShowAchivmentScreen(GameEvents.GetEventByAchievmentId(tuple.Item2), fromFight: true);
					break;
				case EventTypeE.ComboOpened:
				{
					SkillBonusHud componentInChildren = HudMk1.Instance.GetComponentInChildren<SkillBonusHud>();
					componentInChildren.Init(SkillBonusHud.SkillBonusTypeE.Combo);
					_hud.ChangeGuiTo(GuiRoot.GuiType.SkillBonus);
					break;
				}
				case EventTypeE.MagicOpened:
				{
					SkillBonusHud componentInChildren3 = HudMk1.Instance.GetComponentInChildren<SkillBonusHud>();
					componentInChildren3.Init(SkillBonusHud.SkillBonusTypeE.Magic);
					_hud.ChangeGuiTo(GuiRoot.GuiType.SkillBonus);
					break;
				}
				case EventTypeE.RageOpened:
				{
					SkillBonusHud componentInChildren2 = HudMk1.Instance.GetComponentInChildren<SkillBonusHud>();
					componentInChildren2.Init(SkillBonusHud.SkillBonusTypeE.Rage);
					_hud.ChangeGuiTo(GuiRoot.GuiType.SkillBonus);
					break;
				}
				case EventTypeE.XCodeRating:
					UnityApi.RateApp();
					OnMsgGuiExitAchievmentOrExtraChapter();
					break;
				case EventTypeE.NewLevel:
					if (SingletonT<ServerData>.I.PlayerParams.Level == Globals.LevelAtStartFight)
					{
						Messenger.Invoke(Globals.MsgFightResult_Continue);
					}
					else if (GotLevelItems.GetDiffList(Globals.LevelAtStartFight, SingletonT<ServerData>.I.PlayerParams.Level).Count > 0)
					{
						SingletonT<SoundManager>.I.PlayGlobalSound("Jug_level_up");
						HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.GotLevelNewItems);
					}
					else
					{
						SingletonT<SoundManager>.I.PlayGlobalSound("Jug_level_up");
						HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.GotLevelScreen);
					}
					break;
				case EventTypeE.ChapterDone:
					_hud.ChangeGuiTo(GuiRoot.GuiType.ChapterInfo);
					break;
				}
			}
			else
			{
				Utils.Log("*********** 4");
				Messenger.Invoke(Globals.MsgFightResult_Continue);
			}
		}
		else
		{
			Utils.Log("*********** 5");
			Messenger.Invoke(Globals.MsgFightResult_Continue);
		}
	}

	private void ShowAchivmentScreen(GameEvents.Event achievment, bool fromFight)
	{
		if (!(HudMk1.Instance == null) && achievment != null)
		{
			_lastShownAchievment = ((!fromFight) ? achievment : null);
			_lastShownAchievmentScreen = HudMk1.Instance.CurrentGui;
			_hud.ChangeGuiTo(new HudMk1.GuiDesc(GuiRoot.GuiType.Achievments, Yarx.Collections.Tuple.Create(achievment)));
		}
	}

	internal void StartFightWith(ServerData.BotInfo botInfo)
	{
		GoToBattleFromLocation(botInfo);
	}

	internal void StartFightWith(ServerData.BotInfo botInfo, int addLevel)
	{
		GoToBattleFromLocation(botInfo, addLevel);
	}

	public void AdmanCallbackHandler(string text)
	{
		Debug.Log("adman responce " + text);
		IOSCallAdmanLoadBannersDidFinishedLastCalledText = text;
		Utils.LogForce("AdmanCallbackHandler", text);
		Debug.Log("Adman invoke");
		Messenger<bool>.Invoke("MyComEvent", text == "true");
	}

	internal void CallPaymentSuccessful(string paymentText)
	{
		if (paymentText == "BankBuyDoneFail")
		{
			Messenger.Invoke(Globals.MsgBankBuyDoneFail);
			return;
		}
		string[] array = paymentText.Split(new string[1] { "|||" }, StringSplitOptions.None);
		if (array.Length == 2 && !string.IsNullOrEmpty(array[0]) && !string.IsNullOrEmpty(array[1]))
		{
			string key = array[0];
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("ru.mail.games.juggernaut.purchase2", "purchase_1");
			dictionary.Add("ru.mail.games.juggernaut.purchase3", "purchase_2");
			dictionary.Add("ru.mail.games.juggernaut.purchase4", "purchase_3");
			dictionary.Add("ru.mail.games.juggernaut.purchase5", "purchase_4");
			dictionary.Add("ru.mail.games.juggernaut.purchase6", "purchase_5");
			dictionary.Add("ru.mail.games.juggernaut.purchase1", "purchase_6");
			dictionary.Add("ru.mail.games.juggernaut.purchase7", "purchase_7");
			dictionary.Add("ru.mail.games.juggernaut.purchase9", "purchase_9");
			string id = dictionary[key];
			string itemData = ((array[1] == null) ? "fake" : array[1]);
			SingletonT<ServerData>.I.BankBuySuccess(id);
			Messenger.Invoke(Globals.MsgBankBuyDoneSuccessful);
			MarketBillingPlugin.ClosePurchase(itemData);
		}
	}

	public void IOSCall(string message)
	{
		Utils.LogForce("IOSCall", message);
		if (message.Contains("applicationWillResignActive"))
		{
			Utils.LogForce("RECEIVE applicationWillResignActive", Social);
			Metrics.TrySendDayMetrics();
			if (Social != null)
			{
				Social.SyncWithExternal();
			}
		}
		else
		{
			if (message.Contains("applicationDidReceiveMemoryWarning"))
			{
				return;
			}
			switch (message)
			{
			case "closeAchievWindow":
				SpriteGui.DontReleaseButtons = false;
				return;
			case "_SendGameStateToXCode":
				UnityApi.SendGameStateToXCode();
				return;
			case "_SendAchievToXCode":
				UnityApi.SendAchievToXCode();
				return;
			case "BankBuyDoneFail":
				Messenger.Invoke(Globals.MsgBankBuyDoneFail);
				return;
			}
			if (message.StartsWith("GiveMoney gold "))
			{
				SingletonT<ServerData>.I.GiveMoney(ServerData.MoneyType.TypeE.Gold, message.Substring("GiveMoney gold ".Length));
				return;
			}
			if (message.StartsWith("GiveMoney diamand "))
			{
				SingletonT<ServerData>.I.GiveMoney(ServerData.MoneyType.TypeE.Diamond, message.Substring("GiveMoney diamand ".Length));
				return;
			}
			if (message.StartsWith("_twitterPostSucceded"))
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log("MAIN MENU: " + message);
				}
				Messenger.Invoke(Globals.MsgTwitterPostSucceded, message);
				return;
			}
			if (message == "_twitterNoInternet")
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log("MAIN MENU: _twitterNoInternet");
				}
				Messenger.Invoke(Globals.MsgTwitterNoInternet);
				return;
			}
			if (message == "_twitterCanceled")
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log("MAIN MENU: _twitterCanceled");
				}
				Messenger.Invoke(Globals.MsgTwitterCanceled);
				return;
			}
			if (message.StartsWith("_facebookPostSucceded"))
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log("MAIN MENU: " + message);
				}
				Messenger.Invoke(Globals.MsgFacebookPostSucceded, message);
				return;
			}
			if (message.StartsWith("_facebookNotPosted"))
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log("MAIN MENU: " + message);
				}
				Messenger.Invoke(Globals.MsgFacebookNotPosted, message);
				return;
			}
			switch (message)
			{
			case "_fakeTwitter":
			{
				if (SingletonT<ServerData>.I._achievements.Count <= 0)
				{
					break;
				}
				using Dictionary<int, ServerData.Achievement>.ValueCollection.Enumerator enumerator = SingletonT<ServerData>.I._achievements.Values.GetEnumerator();
				if (enumerator.MoveNext())
				{
					ServerData.Achievement current = enumerator.Current;
					UnityApi.TwitterSubmit(current);
				}
				break;
			}
			case "_fakeFacebook":
			{
				if (SingletonT<ServerData>.I._achievements.Count <= 0)
				{
					break;
				}
				using Dictionary<int, ServerData.Achievement>.ValueCollection.Enumerator enumerator2 = SingletonT<ServerData>.I._achievements.Values.GetEnumerator();
				if (enumerator2.MoveNext())
				{
					ServerData.Achievement current2 = enumerator2.Current;
					UnityApi.FacebookSubmit(current2);
				}
				break;
			}
			case "resumeUnityAfterVideo":
				SpriteGui.DontReleaseButtons = false;
				SingletonT<SoundManager>.I.SetCurrentMusicVolume(SingletonT<ServerData>.I.GameSettings.MusicVolume);
				break;
			default:
				if (Globals.IsDebugBuild)
				{
					Utils.Log($"![IOS]   {message}");
				}
				break;
			}
		}
	}

	internal void GoToChestGame(ServerData.Location location, LocationLogic.ChestOnLocation chest)
	{
		Utils.Log("GoToChestGame", Globals.GameScreen);
		SingletonT<SoundManager>.I.PlayChestSound();
		ActionD item = delegate
		{
			if (location.Logic.ChestsOnLocation.Remove(chest))
			{
				Messenger<LocationLogic, LocationLogic.ChestOnLocation>.Invoke(Globals.Msg_ChestOnLocationRemoved, location.Logic, chest);
				if (location.Logic.ChestsOnLocation.Count == 0)
				{
					Messenger.Invoke(Globals.MsgAllChestsUnlocked);
				}
				SaveGame();
			}
		};
		ActionD item2 = delegate
		{
			GoToLocationFromChestGame();
		};
		Yarx.Collections.Tuple<ServerData.Chest, ActionD, ActionD> args = Yarx.Collections.Tuple.Create(chest.Chest, item, item2);
		_hud.ChangeGuiTo(new HudMk1.GuiDesc(GuiRoot.GuiType.ChestMiniGame, args));
	}

	internal void GoToLocationFromChestGame()
	{
		Globals.GameScreen = Globals.GameScreenE.Location;
		GameObject gameObject = GameObject.Find(Globals.LocationGameObjectChestGameName);
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject.gameObject);
		}
		Utils.Log("GoToLocationFromChestGame");
		_hud.ChangeGuiTo(GuiRoot.GuiType.Location);
	}

	internal void Restart()
	{
		if (!(HudMk1.Instance == null))
		{
			SaveSlotIndex = -1;
			if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.ChooseChar)
			{
				_hud.GetComponentInChildren<ChooseCharHud>().DestroyPersons();
			}
			_hud.ChangeGuiTo(GuiRoot.GuiType.None);
			StopAllCoroutines();
			_destroyAll = DestroyAllE.Restart;
		}
	}

	private void OnMsgZachistkaDone(ServerData.Location location)
	{
		if (SingletonT<ServerData>.I.GetStartGameLocation() != null && location.Id != SingletonT<ServerData>.I.GetStartGameLocation().Id)
		{
			AddOpenCondition(EventTypeE.XCodeRating, 0);
		}
	}
}
