using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gesture;
using Gesture.CustomGestures;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class HudMk1 : SpriteGui
{
	public class GuiDesc
	{
		public GuiRoot.GuiType Type;

		public ITuple Args;

		public GuiDesc(GuiRoot.GuiType type, ITuple args)
		{
			Type = type;
			Args = args;
		}
	}

	private class GuiInitialization
	{
		private string ResourceName;

		private InitializeGui IniFunc;

		private bool PreCreate;

		private bool DoNotDestroy;

		private GuiRoot.GuiType[] GuiTypes;

		private GameObject Prefab;

		private GameObject Clone;

		private GuiRoot Root;

		public GuiInitialization(string resourceName, InitializeGui iniFunc, bool preCreate, bool doNotDestroy, params GuiRoot.GuiType[] guiTypes)
		{
			ResourceName = resourceName;
			IniFunc = iniFunc;
			PreCreate = preCreate;
			DoNotDestroy = doNotDestroy;
			GuiTypes = guiTypes;
		}

		public bool Match(GuiRoot.GuiType guiType)
		{
			return GuiTypes.Contains(guiType);
		}

		public void EnsureInitialized(bool preCreateOnly, List<GuiRoot> guis, Transform parent, ITuple args, ActionD onInit)
		{
			if (preCreateOnly && !PreCreate)
			{
				onInit();
				return;
			}
			if (!string.IsNullOrEmpty(ResourceName) && Prefab == null)
			{
				Prefab = Util.Resource<GameObject>(ResourceName);
			}
			if (Prefab != null && Clone == null)
			{
				GuiRoot.CurrentInstantiationParent = parent;
				Clone = (GameObject)UnityEngine.Object.Instantiate(Prefab);
				GuiRoot.CurrentInstantiationParent = null;
			}
			if (Clone != null && Root == null)
			{
				Root = Clone.GetComponent<GuiRoot>();
				if (Root != null)
				{
					guis.Add(Root);
				}
			}
			if (Clone != null)
			{
				SpriteButton[] componentsInChildren = Clone.GetComponentsInChildren<SpriteButton>(includeInactive: true);
				foreach (SpriteButton spriteButton in componentsInChildren)
				{
					if (!spriteButton.name.StartsWith("mob_icon_"))
					{
						spriteButton.SetActive();
					}
					else
					{
						spriteButton.SetInactive();
					}
				}
				if (IniFunc != null)
				{
					IniFunc(args, onInit);
				}
				else
				{
					onInit();
				}
			}
			else
			{
				onInit();
			}
		}

		public void Destroy(List<GuiRoot> guis)
		{
			if (DoNotDestroy)
			{
				return;
			}
			if (Root != null)
			{
				guis.Remove(Root);
				Utils.Destroy(Root);
				Root = null;
			}
			if (Clone != null)
			{
				SpriteButton[] componentsInChildren = Clone.GetComponentsInChildren<SpriteButton>(includeInactive: true);
				foreach (SpriteButton spriteButton in componentsInChildren)
				{
					spriteButton.UnregisterMe();
				}
			}
			Utils.Destroy(ref Clone);
		}
	}

	private delegate void InitializeGui(ITuple args, ActionD onInit);

	private CompositeDisposable _subscriptions;

	public static HudMk1 Instance;

	private readonly List<GuiRoot> _guis = new List<GuiRoot>();

	private GuiDesc _currentGui = new GuiDesc(GuiRoot.GuiType.None, null);

	private static readonly Stack<GuiDesc> Modals = new Stack<GuiDesc>();

	private Transform _fightButton;

	private GuiDesc _layer0 = new GuiDesc(GuiRoot.GuiType.None, null);

	private GuiDesc _layer1 = new GuiDesc(GuiRoot.GuiType.None, null);

	private GuiDesc _layer2 = new GuiDesc(GuiRoot.GuiType.None, null);

	private Recognizer _recognizer;

	public SpriteText ChapterText;

	public FightScreenMobIcon[] MobIcons;

	public SpriteText MobText;

	private bool _allowSelectMob;

	private bool _inTransition;

	private readonly Queue<Action> _transitions = new Queue<Action>();

	private List<GuiInitialization> _guiInitializations = new List<GuiInitialization>();

	private readonly GuiRoot.GuiType[] _types = (GuiRoot.GuiType[])Enum.GetValues(typeof(GuiRoot.GuiType));

	private readonly List<Vector3> _gesturepoints = new List<Vector3>();

	private bool _gesture;

	private int _activeIndex;

	private AreaData _area;

	public bool IsGuiStable => !_inTransition && _transitions.Count <= 0;

	internal GuiDesc CurrentGui
	{
		get
		{
			return _currentGui;
		}
		private set
		{
			_currentGui = value;
		}
	}

	internal bool IsLoadingPlayerStats { get; private set; }

	public bool AllowSelectMob
	{
		get
		{
			return _allowSelectMob;
		}
		set
		{
			if (_allowSelectMob == value)
			{
				return;
			}
			_allowSelectMob = value;
			foreach (SpriteButton value2 in _buttons.Values)
			{
				if (value2.name.StartsWith("mob_icon_"))
				{
					if (value)
					{
						value2.SetActive();
					}
					else
					{
						value2.SetInactive();
					}
				}
			}
		}
	}

	public void ShowFightButton(bool show)
	{
		if (_fightButton == null)
		{
			FightScreenFightButton componentInChildren = GetComponentInChildren<FightScreenFightButton>();
			if (componentInChildren != null)
			{
				_fightButton = componentInChildren.transform.parent;
			}
		}
		if (_fightButton != null)
		{
			if (show)
			{
				_fightButton.localPosition = new Vector3(-4f, -50f, -50f);
			}
			else
			{
				_fightButton.GoToHell();
			}
		}
	}

	private void InitTouchscreenCasts()
	{
		_recognizer = new Recognizer();
		_recognizer.AddGesture(new Lightning());
		_recognizer.AddGesture(new Dark());
		_recognizer.AddGesture(new Fire());
		_recognizer.AddGesture(new Ice());
	}

	private void MemoryToLog(string text)
	{
	}

	private void EnsureGuiInitializedRec(GuiDesc gui, IEnumerator<GuiInitialization> iniEnum, ActionD onInit)
	{
		if (!iniEnum.MoveNext())
		{
			onInit();
			return;
		}
		GuiInitialization current = iniEnum.Current;
		if (gui == null || current.Match(gui.Type))
		{
			bool preCreateOnly = gui == null;
			List<GuiRoot> guis = _guis;
			Transform parent = base.transform;
			ITuple args;
			if (gui == null)
			{
				ITuple tuple = null;
				args = tuple;
			}
			else
			{
				args = gui.Args;
			}
			current.EnsureInitialized(preCreateOnly, guis, parent, args, delegate
			{
				EnsureGuiInitializedRec(gui, iniEnum, onInit);
			});
		}
		else
		{
			EnsureGuiInitializedRec(gui, iniEnum, onInit);
		}
	}

	public void EnsureGuiInitialized(GuiDesc gui, ActionD onInit)
	{
		EnsureGuiInitializedRec(gui, _guiInitializations.GetEnumerator(), onInit);
	}

	public void DestroyGui(GuiDesc gui)
	{
		foreach (GuiInitialization guiInitialization in _guiInitializations)
		{
			if (gui == null || guiInitialization.Match(gui.Type))
			{
				guiInitialization.Destroy(_guis);
			}
		}
	}

	public void DestroyExceptGui(GuiDesc gui)
	{
		foreach (GuiInitialization guiInitialization in _guiInitializations)
		{
			if (gui == null || !guiInitialization.Match(gui.Type))
			{
				guiInitialization.Destroy(_guis);
			}
		}
	}

	private void RegisterInitializationRules()
	{
		bool preCreate = false;
		bool doNotDestroy = false;
		_guiInitializations.Add(new GuiInitialization("gui/match3_start_screen", Initialize_Match3StartScreen, preCreate, doNotDestroy, GuiRoot.GuiType.Match3StartScreen));
		_guiInitializations.Add(new GuiInitialization("gui/match3_minigame", Initialize_Match3Minigame, preCreate, doNotDestroy, GuiRoot.GuiType.Match3));
		_guiInitializations.Add(new GuiInitialization("gui/options_support", null, preCreate, doNotDestroy, GuiRoot.GuiType.SupportPopup));
		_guiInitializations.Add(new GuiInitialization("gui/options", null, preCreate, doNotDestroy, GuiRoot.GuiType.Options));
		_guiInitializations.Add(new GuiInitialization("gui/options_right_top", null, preCreate, doNotDestroy, GuiRoot.GuiType.Options));
		_guiInitializations.Add(new GuiInitialization("gui/_choose_char", Initialize_ChooseChar, preCreate, doNotDestroy, GuiRoot.GuiType.ChooseChar));
		_guiInitializations.Add(new GuiInitialization("gui/start_menu_bottom", null, true, doNotDestroy, GuiRoot.GuiType.StartMenu));
		_guiInitializations.Add(new GuiInitialization("gui/start_menu_fb", Initialize_StartMenu, true, doNotDestroy, GuiRoot.GuiType.StartMenu));
		_guiInitializations.Add(new GuiInitialization("gui/start_menu_top", null, true, doNotDestroy, GuiRoot.GuiType.StartMenu));
		_guiInitializations.Add(new GuiInitialization("gui/magic_book_center", Initialize_MagicBook, preCreate, doNotDestroy, GuiRoot.GuiType.MagicBook));
		_guiInitializations.Add(new GuiInitialization("gui/sovereighn_info", null, preCreate, doNotDestroy, GuiRoot.GuiType.SovereighnInfo));
		_guiInitializations.Add(new GuiInitialization("gui/chapter_bonus", Initialize_ChapterInfo, preCreate, doNotDestroy, GuiRoot.GuiType.ChapterInfo));
		_guiInitializations.Add(new GuiInitialization("gui/achievments", Initialize_Achievments, preCreate, doNotDestroy, GuiRoot.GuiType.Achievments));
		_guiInitializations.Add(new GuiInitialization("gui/achievements_scroll", Initialize_AchievsmentsScroll, preCreate, doNotDestroy, GuiRoot.GuiType.AchievementsScroll));
		_guiInitializations.Add(new GuiInitialization("gui/extra_chapter_progress", null, preCreate, doNotDestroy, GuiRoot.GuiType.ExtraChapterInfo));
		_guiInitializations.Add(new GuiInitialization("gui/extra_chapter_popup_common", Initialize_ExtraChapterInfo, preCreate, doNotDestroy, GuiRoot.GuiType.ExtraChapterInfo));
		_guiInitializations.Add(new GuiInitialization("gui/extra_chapter_congratulations", null, preCreate, doNotDestroy, GuiRoot.GuiType.ExtraChapterCongratulations));
		_guiInitializations.Add(new GuiInitialization("gui/main_map_top", null, preCreate, doNotDestroy, GuiRoot.GuiType.MainMap));
		_guiInitializations.Add(new GuiInitialization("gui/main_map", Initialize_MainMap, preCreate, doNotDestroy, GuiRoot.GuiType.MainMap));
		_guiInitializations.Add(new GuiInitialization("gui/got_level_center_center", null, preCreate, doNotDestroy, GuiRoot.GuiType.GotLevelScreen));
		_guiInitializations.Add(new GuiInitialization("gui/got_level_center_top", Initialize_GotLevelBanner, preCreate, doNotDestroy, GuiRoot.GuiType.GotLevelScreen));
		_guiInitializations.Add(new GuiInitialization("gui/got_level_items_center_bottom", null, preCreate, doNotDestroy, GuiRoot.GuiType.GotLevelNewItems));
		_guiInitializations.Add(new GuiInitialization("gui/got_level_items_center_top", Initialize_GotLevelBanner, preCreate, doNotDestroy, GuiRoot.GuiType.GotLevelNewItems));
		_guiInitializations.Add(new GuiInitialization("gui/got_level_items_top_left", Initialize_GotLevelNewItems, preCreate, doNotDestroy, GuiRoot.GuiType.GotLevelNewItems));
		_guiInitializations.Add(new GuiInitialization("gui/location_bottom", null, preCreate, doNotDestroy, GuiRoot.GuiType.Location));
		_guiInitializations.Add(new GuiInitialization("gui/location_top", null, preCreate, doNotDestroy, GuiRoot.GuiType.Location));
		_guiInitializations.Add(new GuiInitialization("gui/location", Initialize_Location, preCreate, doNotDestroy, GuiRoot.GuiType.Location));
		_guiInitializations.Add(new GuiInitialization("gui/chest_minigame_bg", null, preCreate, doNotDestroy, GuiRoot.GuiType.ChestMiniGame));
		_guiInitializations.Add(new GuiInitialization("gui/chest_minigame_right", null, preCreate, doNotDestroy, GuiRoot.GuiType.ChestMiniGame));
		_guiInitializations.Add(new GuiInitialization("gui/chest_minigame_left", Initialize_ChestMiniGame, preCreate, doNotDestroy, GuiRoot.GuiType.ChestMiniGame));
		_guiInitializations.Add(new GuiInitialization("gui/final", null, preCreate, doNotDestroy, GuiRoot.GuiType.Final));
		_guiInitializations.Add(new GuiInitialization("gui/_advertising", null, preCreate, doNotDestroy, GuiRoot.GuiType.Advertising));
	}

	private void Initialize_StartMenu(ITuple args, ActionD onInit)
	{
		MainMenuGameClubButtonMk1 componentInChildren = GetComponentInChildren<MainMenuGameClubButtonMk1>();
		if (componentInChildren != null && !UnityApi.UseGameClub())
		{
			componentInChildren.gameObject.SetActive(false);
		}
		MainMenuFbButtonMk1 componentInChildren2 = GetComponentInChildren<MainMenuFbButtonMk1>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.Init();
		}
		onInit();
	}

	private void Initialize_Match3StartScreen(ITuple args, ActionD onInit)
	{
		Match3StartScreenHud componentInChildren = GetComponentInChildren<Match3StartScreenHud>();
		componentInChildren.Init(((System.Tuple<string>)args).Item1);
		onInit();
	}

	private void Initialize_Match3Minigame(ITuple args, ActionD onInit)
	{
		Match3Hud componentInChildren = GetComponentInChildren<Match3Hud>();
		componentInChildren.Init(((System.Tuple<ServerData.Mine>)args).Item1);
		onInit();
	}

	private void Initialize_ChooseChar(ITuple args, ActionD onInit)
	{
		GetComponentInChildren<ChooseCharHud>().Show();
		onInit();
	}

	private void Initialize_MagicBook(ITuple args, ActionD onInit)
	{
		MagicBookHud componentInChildren = Instance.GetComponentInChildren<MagicBookHud>();
		componentInChildren.Init();
		onInit();
	}

	private void Initialize_ChapterInfo(ITuple args, ActionD onInit)
	{
		ChapterBonusHud componentInChildren = GetComponentInChildren<ChapterBonusHud>();
		componentInChildren.Init(delegate
		{
			onInit();
		});
	}

	private void Initialize_Achievments(ITuple args, ActionD onInit)
	{
		AchievmentsHud componentInChildren = Instance.GetComponentInChildren<AchievmentsHud>();
		SocialPoster componentInChildren2 = componentInChildren.gameObject.GetComponentInChildren<SocialPoster>();
		if (componentInChildren2 != null || UnityApi.UseOK())
		{
			componentInChildren2.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		componentInChildren.Init(((System.Tuple<GameEvents.Event>)args).Item1);
		onInit();
	}

	private void Initialize_AchievsmentsScroll(ITuple args, ActionD onInit)
	{
		AchievementScroll componentInChildren = GetComponentInChildren<AchievementScroll>();
		SocialPoster componentInChildren2 = componentInChildren.gameObject.GetComponentInChildren<SocialPoster>();
		if (componentInChildren2 != null || UnityApi.UseOK())
		{
			componentInChildren2.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		componentInChildren.Init(GameObject.FindWithTag("_viewport_01").GetComponent<Camera>());
		onInit();
	}

	private void Initialize_ExtraChapterInfo(ITuple args, ActionD onInit)
	{
		ExtraChapterHud componentInChildren = GetComponentInChildren<ExtraChapterHud>();
		ServerData.Location item = ((System.Tuple<ServerData.Location, int>)args).Item1;
		int item2 = ((System.Tuple<ServerData.Location, int>)args).Item2;
		componentInChildren.IconsRoot = GameObject.FindWithTag("extra_chapter_progress_icons").transform;
		componentInChildren.OpenButton = GameObject.FindWithTag("extra_chapter_progress_button");
		componentInChildren.TextOpenButton = componentInChildren.OpenButton.GetComponentInChildren<SpriteText>();
		GameObject gameObject = GameObject.FindWithTag("extra_chapter_progress_mission");
		componentInChildren.TextOpenCondition = gameObject.GetComponentInChildren<SpriteText>();
		componentInChildren.Init(item, item2);
		onInit();
	}

	private void Initialize_MainMap(ITuple args, ActionD onInit)
	{
		MainMapHud componentInChildren = GetComponentInChildren<MainMapHud>();
		componentInChildren.ProgressBar = GameObject.FindWithTag("main_map_progress_root");
		componentInChildren.RefreshLocationButtons();
		FogOfWar componentInChildren2 = componentInChildren.gameObject.GetComponentInChildren<FogOfWar>();
		componentInChildren2.DarkAndRefresh();
		onInit();
	}

	private void Initialize_GotLevelBanner(ITuple args, ActionD onInit)
	{
		GotLevelBanner componentInChildren = GetComponentInChildren<GotLevelBanner>();
		componentInChildren.Init();
		onInit();
	}

	private void Initialize_GotLevelNewItems(ITuple args, ActionD onInit)
	{
		GotLevelItems componentInChildren = GetComponentInChildren<GotLevelItems>();
		componentInChildren.Init();
		onInit();
	}

	private void Initialize_Location(ITuple args, ActionD onInit)
	{
		LocationHud componentInChildren = GetComponentInChildren<LocationHud>();
		ServerData.Location location = null;
		if (args != null)
		{
			location = ((System.Tuple<ServerData.Location>)args).Item1;
		}
		if (location == null)
		{
			location = AreaData.Current.Location;
		}
		componentInChildren.MobIcons = GameObject.FindWithTag("location_mob_icons");
		GameObject gameObject = GameObject.FindWithTag("location_population");
		componentInChildren.PopulationCount = gameObject.GetComponentInChildren<SpriteText>();
		Transform transform = gameObject.transform.FindChildByName("arrow", includeInactive: true);
		componentInChildren.Arrow = transform.GetComponent<Sprite>();
		GameObject gameObject2 = GameObject.FindWithTag("location_msg_root");
		componentInChildren.Messages = gameObject2.GetComponentInChildren<SpriteText>();
		GameObject gameObject3 = GameObject.FindWithTag("location_scarab");
		componentInChildren.ScarabCount = gameObject3.GetComponentInChildren<SpriteText>();
		componentInChildren.ScarabCountIcon = GameObject.FindWithTag("location_scarab_icon");
		GameObject gameObject4 = GameObject.FindWithTag("location_bottom_root");
		componentInChildren.ButtonDig = gameObject4.transform.FindChildByName("button_dig_chest", includeInactive: true).gameObject;
		componentInChildren.ButtonFindChest = gameObject4.transform.FindChildByName("button_find_chest", includeInactive: true).gameObject;
		componentInChildren.ButtonCancelDig = gameObject4.transform.FindChildByName("button_cancel_dig_chest", includeInactive: true).gameObject;
		GameObject gameObject5 = GameObject.FindWithTag("location_money");
		componentInChildren.MoneyCount = gameObject5.GetComponentInChildren<SpriteText>();
		GameObject gameObject6 = GameObject.FindWithTag("location_chest");
		componentInChildren.ChestCount = gameObject6.GetComponentInChildren<SpriteText>();
		componentInChildren.ChestIconHolder = GameObject.FindWithTag("location_chest_icon").transform;
		componentInChildren.InitLocation(location, onInit);
	}

	private void Initialize_ChestMiniGame(ITuple args, ActionD onInit)
	{
		System.Tuple<ServerData.Chest, ActionD, ActionD> tuple = (System.Tuple<ServerData.Chest, ActionD, ActionD>)args;
		ServerData.Chest item = tuple.Item1;
		ActionD item2 = tuple.Item2;
		ActionD item3 = tuple.Item3;
		LockpickingHud componentInChildren = GetComponentInChildren<LockpickingHud>();
		GameObject gameObject = GameObject.FindWithTag("chest_minigame_title");
		componentInChildren.Title = gameObject.GetComponentInChildren<SpriteText>();
		GameObject gameObject2 = GameObject.FindWithTag("chest_minigame_keys");
		componentInChildren.LockpickCount = gameObject2.GetComponentInChildren<SpriteText>();
		GameObject gameObject3 = GameObject.FindWithTag("chest_minigame_help");
		componentInChildren.HelpBottom = gameObject3.GetComponentInChildren<SpriteText>();
		componentInChildren.RightRoot = GameObject.FindWithTag("chest_minigame_right_root");
		componentInChildren.RightMask = GameObject.FindWithTag("chest_minigame_bg").transform;
		componentInChildren.MakeGame(item, item2, item3);
		onInit();
	}

	public void ChangeGuiTo(GuiRoot.GuiType type)
	{
		ChangeGuiTo(type, null);
	}

	public void ChangeGuiTo(GuiRoot.GuiType type, ITuple args)
	{
		ChangeGuiTo(new GuiDesc(type, args));
	}

	public void ChangeGuiTo(GuiDesc switchTo)
	{
		if (CurrentGui.Type == GuiRoot.GuiType.Location && switchTo.Type == GuiRoot.GuiType.Location)
		{
			return;
		}
		Utils.Log("]]]]================ ChangeGuiTo ===================[[[[", CurrentGui.Type, switchTo.Type);
		if ((switchTo.Type == GuiRoot.GuiType.MainMap || switchTo.Type == GuiRoot.GuiType.Location) && !Globals.MainMenu.OpenConditionsEmpty())
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("=======>> ACTION ENQ: {0} <<=================".Fmt("ACHIEVEMENTS"));
			}
			_transitions.Enqueue(delegate
			{
				Globals.MainMenu.StartShowAfterFightScreens();
			});
		}
		if (_inTransition)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("=======>> ACTION ENQ: {0} <<=================".Fmt(switchTo.Type));
			}
			_transitions.Enqueue(delegate
			{
				ChangeGuiTo(switchTo);
			});
			return;
		}
		if ((switchTo.Type == GuiRoot.GuiType.BattleHud || switchTo.Type == GuiRoot.GuiType.EnemyTurn) && Globals.Enemy != null && Globals.Enemy.Health <= 0)
		{
			switchTo = new GuiDesc(GuiRoot.GuiType.EnemyDefeated, null);
		}
		MemoryToLog("before ChangeGuiCoro");
		_inTransition = true;
		EnsureGuiInitialized(switchTo, delegate
		{
			StartCoroutine("ChangeGuiCoro", switchTo);
		});
	}

	public void AddOrRemoveGui(bool add, GuiRoot.GuiType what)
	{
		float num = 0.1f;
		foreach (GuiRoot gui in _guis)
		{
			float b = gui.OnOff(add, what);
			num = Mathf.Max(num, b);
		}
		SpriteGui.BlockReleaseUntil(Time.time + num);
	}

	internal void InitLocationView(int activeIndex, AreaData areaData)
	{
		_area = areaData;
		for (int i = areaData.Mobs.Length; i < 10; i++)
		{
			if (MobIcons[i] != null)
			{
				MobIcons[i].SetState(FightScreenMobIcon.State.OutOfGui);
			}
		}
		SetSelectedMob(activeIndex, loadMob: false);
		ChapterText.Text_ = areaData.Location.Title;
	}

	private void Awake()
	{
		RegisterInitializationRules();
		InitGui();
		if (Instance != null && Globals.IsDebugBuild)
		{
			Debug.LogWarning("=============== HudMk1 NOT a singleton! =================");
		}
		Instance = this;
		Debug.Log("INSTANCE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_area = null;
		_subscriptions.Dispose();
	}

	private void Start()
	{
		Globals.MemDebugPrint("HUD_MK1 Start Begin");
		foreach (SpriteButton value in _buttons.Values)
		{
			if (!value.name.StartsWith("mob_icon_"))
			{
				value.SetActive();
			}
			else
			{
				value.SetInactive();
			}
		}
		base.Release += ProcessButtons;
		base.Move += ProcessGestures;
		base.MoveBegin += OnGestureBegin;
		base.MoveEnd += OnGestureEnd;
		InitTouchscreenCasts();
		Globals.MemDebugPrint("HUD_MK1 Start End");
	}

	private void ProcessGestures(Vector3 arg1, Vector3 arg2)
	{
		if (_gesture)
		{
			_gesturepoints.Add(arg2);
		}
	}

	private void OnGestureBegin(Vector3 begin)
	{
		_gesture = CurrentGui.Type == GuiRoot.GuiType.CastMagic;
		if (_gesture)
		{
			_gesturepoints.Clear();
			_gesturepoints.Add(begin);
		}
	}

	private void OnGestureEnd(Vector3 endpoint)
	{
		if (!_gesture)
		{
			return;
		}
		_gesturepoints.Add(endpoint);
		if (_gesturepoints.Count > 10 && _gesturepoints.Count < 600)
		{
			string[] array = _recognizer.Recognize(_gesturepoints).ToArray();
			if (array.Length >= 1)
			{
				Messenger<string[]>.Invoke(Globals.MsgGuiBattle_CastGesture, array);
			}
		}
	}

	private void Update()
	{
		ProcessRayCast();
		if (!_inTransition && _transitions.Count > 0)
		{
			Action action = _transitions.Dequeue();
			if (Globals.IsDebugBuild)
			{
				Debug.Log("=======>> ACTION DQ");
			}
			action();
		}
	}

	private void SetSelectedMob(int i, bool loadMob)
	{
	}

	private void ChangeunselectedMobIconsView()
	{
		for (int i = 0; i < _area.Mobs.Length; i++)
		{
			AreaData.MobData mobData = _area.Mobs[i];
			FightScreenMobIcon fightScreenMobIcon = MobIcons[i];
			if (mobData.IsBoss)
			{
				fightScreenMobIcon.SetMeBoss();
			}
			fightScreenMobIcon.SetIcon(Path.GetFileNameWithoutExtension(mobData.ServerInfo.Picture));
			if (i != _activeIndex)
			{
				fightScreenMobIcon.SetState((i >= _activeIndex) ? FightScreenMobIcon.State.Inactive : FightScreenMobIcon.State.Skull);
			}
		}
	}

	private void ProcessButtons(SpriteButton button)
	{
		string text = button.name;
		if (!string.IsNullOrEmpty(button.ClickSound))
		{
			SingletonT<SoundManager>.I.PlayGlobalSound(button.ClickSound);
		}
		else if (!text.StartsWith("lock_cell_") && !text.StartsWith("l_money_"))
		{
			if (text.Contains("_catch_all_") || text == "_alert_continue")
			{
				SingletonT<SoundManager>.I.PlayGlobalSound("click_close_popup");
			}
			else
			{
				SingletonT<SoundManager>.I.PlaySoundClickPopup();
			}
		}
		Messenger.Invoke(Globals.MsgGuiButtonPressed, text);
	}

	private void PlayKaChing(ServerData.MoneyType.TypeE moneyType, string reason)
	{
		if (moneyType == ServerData.MoneyType.TypeE.Gold || moneyType == ServerData.MoneyType.TypeE.Diamond)
		{
			SingletonT<SoundManager>.I.PlaySoundMoneyPay();
		}
	}

	private void InitGui()
	{
		foreach (Transform item in base.transform)
		{
			GuiRoot component = item.GetComponent<GuiRoot>();
			if (!(component == null))
			{
				_guis.Add(component);
			}
		}
	}

	public float GetMaxTime(GuiRoot.State phase, GuiRoot.GuiType switchTo)
	{
		return 1f;
	}

	internal void InitPlayerStats()
	{
		IsLoadingPlayerStats = true;
		SingletonT<ServerData>.I.RegenShopGoods();
		foreach (object value in Enum.GetValues(typeof(ServerData.MoneyType.TypeE)))
		{
			Messenger.Invoke(Globals.MsgPlayerFundsChanged, (ServerData.MoneyType.TypeE)(int)value, string.Empty);
		}
		Messenger.Invoke(Globals.MsgPlayerLevelChanged, SingletonT<ServerData>.I.PlayerParams.Level - 1, SingletonT<ServerData>.I.PlayerParams.Level, "InitPlayerStats");
		Messenger.Invoke(Globals.MsgPlayerExpChanged, 100 - SingletonT<ServerData>.I.GetPlayerExperiencePercentToNextLevel());
		Messenger<int, int>.Invoke(Globals.MsgPlayerHealthChanged, SingletonT<ServerData>.I.PlayerParams.HP, SingletonT<ServerData>.I.PlayerParams.HP);
		Messenger.Invoke(Globals.MsgPlayerSkillChanged);
		Messenger.Invoke(Globals.MsgPlayerSkillPointsChanged);
		Messenger.Invoke(Globals.MsgBagNeedRefresh);
		Messenger.Invoke(Globals.MsgNewPersInited);
		SingletonT<ServerData>.I.RefreshElixirsCount();
		_subscriptions.Add(Messenger<ServerData.MoneyType.TypeE, string>.AddListener(Globals.MsgPlayerFundsChanged, PlayKaChing));
		SingletonT<ServerData>.I.UpdateLiveShamansCount();
		IsLoadingPlayerStats = false;
		if (Globals.DebugRenderersStage > Globals.DebugRenderers.DoNothing)
		{
			BroadcastMessage("TurnOffRenderer", SendMessageOptions.DontRequireReceiver);
		}
		SingletonT<ServerData>.I.UpdateLiveShamansCount();
	}

	public bool PopModal()
	{
		if (GuiRoot.ModalTypes.Contains(CurrentGui.Type) && Modals.Count > 0)
		{
			GuiDesc guiDesc = Modals.Peek();
			if (Globals.IsDebugBuild)
			{
				Debug.Log(" <-------- PEEK: {0}".Fmt(guiDesc));
			}
			ChangeGuiTo(guiDesc);
			return true;
		}
		ChangeGuiTo(_layer0);
		return false;
	}

	private IEnumerator ChangeGuiCoro(GuiDesc switchTo)
	{
		GuiDesc saveGui = CurrentGui;
		int oldlayer = GuiRoot.GetLayer(saveGui.Type);
		int newLayer = GuiRoot.GetLayer(switchTo.Type);
		if (newLayer <= 0)
		{
			_layer0 = switchTo;
		}
		else if (newLayer == 1)
		{
			_layer1 = switchTo;
		}
		else
		{
			_layer2 = switchTo;
		}
		if (GuiRoot.ModalTypes.Contains(saveGui.Type) && GuiRoot.ModalTypes.Contains(switchTo.Type))
		{
			if (Modals.Count > 0 && Modals.Peek().Type == switchTo.Type)
			{
				GuiDesc p = Modals.Pop();
				if (Globals.IsDebugBuild)
				{
					Debug.Log("-------> POP: {0}".Fmt(p.Type));
				}
			}
			else if (saveGui.Type != switchTo.Type && ((GuiRoot.BagTypes.Contains(saveGui.Type) && !GuiRoot.BagTypes.Contains(switchTo.Type)) || !GuiRoot.BagTypes.Contains(saveGui.Type)))
			{
				if (Globals.IsDebugBuild)
				{
					Debug.Log("-------> PUSH: {0}".Fmt(saveGui.Type));
				}
				Modals.Push(saveGui);
			}
		}
		else if (GuiRoot.ModalTypes.Contains(saveGui.Type) && GuiRoot.ModalTypes.Contains(switchTo.Type))
		{
		}
		MemoryToLog("ChangeGuiCoro(1) " + switchTo.Type);
		Messenger.Invoke(Globals.MsgGuiSwitchToBefore, saveGui.Type, switchTo.Type);
		MemoryToLog("ChangeGuiCoro(2) " + switchTo.Type);
		bool _needTransition = GuiRoot.NeedTransition.Contains(switchTo.Type) && GuiRoot.NeedTransition.Contains(saveGui.Type);
		float? transitionOut = ChangeToOff(switchTo.Type, oldlayer, newLayer, _needTransition);
		SpriteGui.BlockReleaseUntil(Time.time + (transitionOut ?? 0.1f));
		MemoryToLog("ChangeGuiCoro(3) " + switchTo.Type);
		yield return new WaitForSeconds(transitionOut ?? 0.1f);
		MemoryToLog("ChangeGuiCoro(4) " + switchTo.Type);
		Messenger.Invoke(Globals.MsgGuiSwitchToPre, saveGui.Type, switchTo.Type);
		MemoryToLog("ChangeGuiCoro(5) " + switchTo.Type);
		float? transitionIn = ChangeToOn(switchTo.Type, _needTransition);
		SpriteGui.BlockReleaseUntil(Time.time + (transitionIn ?? 0.1f));
		MemoryToLog("ChangeGuiCoro(6) " + switchTo.Type);
		yield return new WaitForSeconds(transitionIn ?? 0.1f);
		MemoryToLog("ChangeGuiCoro(7) " + switchTo.Type);
		if (_needTransition)
		{
			foreach (GuiRoot guiRoot in _guis)
			{
				if (!guiRoot.Hide)
				{
					guiRoot.Remove(saveGui.Type);
				}
				guiRoot.MoveOnLayer(switchTo.Type);
			}
		}
		MemoryToLog("ChangeGuiCoro(8) " + switchTo.Type);
		CurrentGui = switchTo;
		_inTransition = false;
		MemoryToLog("ChangeGuiCoro(9) " + switchTo.Type);
		if (!GuiRoot.CancelableTypes.Contains(switchTo.Type))
		{
			DestroyExceptGui(switchTo);
		}
		Messenger.Invoke(Globals.MsgGuiSwitchToPost, saveGui.Type, switchTo.Type);
		MemoryToLog("ChangeGuiCoro(10) " + switchTo.Type);
	}

	private float? ChangeToOff(GuiRoot.GuiType @new, int layerOld, int layerNew, bool transition)
	{
		if (!transition && (layerNew < layerOld || (layerOld <= 0 && layerNew <= 0) || GuiRoot.BagTypes.Contains(@new)))
		{
			return _guis.Select((GuiRoot root) => root.ChangeState(GuiRoot.State.Off, @new, transition: false)).Max();
		}
		return (from root in _guis
			where root.Hide
			select root.ChangeState(GuiRoot.State.Off, @new, transition: false)).Max();
	}

	private float? ChangeToOn(GuiRoot.GuiType @new, bool transition)
	{
		return _guis.Select((GuiRoot root) => root.ChangeState(GuiRoot.State.On, @new, transition)).Max();
	}

	public override void HideHud()
	{
		base.HideHud();
		ChangeGuiTo(GuiRoot.GuiType.None);
	}

	public override void UnhideHud()
	{
		base.UnhideHud();
		ChangeGuiTo(GuiRoot.GuiType.Fight);
	}

	protected override void ProcessRayCast()
	{
		if (_camera2d == null)
		{
			InitCamera2D();
		}
		if (_camera2d != null && !base.IsLocked && !_hidden && _camera2d.enabled)
		{
			bool mouseButtonDown = Input.GetMouseButtonDown(0);
			bool mouseButton = Input.GetMouseButton(0);
			bool mouseButtonUp = Input.GetMouseButtonUp(0);
			if (mouseButtonUp || mouseButton || mouseButtonDown)
			{
				Vector3 mousePosition = Input.mousePosition;
				DoProcessInput(mouseButtonDown, mouseButtonUp, mouseButton, mousePosition);
			}
		}
	}

	private void DoProcessInput(bool singleDown, bool up, bool down, Vector3 pos)
	{
		bool flag = false;
		if (Globals.IsDebugInput)
		{
		}
		if (up || down)
		{
			SpriteButton button = null;
			Ray ray = _camera2d.ScreenPointToRay(pos);
			RaycastHit[] array = Physics.RaycastAll(ray, SpriteGui._colliderDistance, _mask);
			if (Globals.IsDebugInput)
			{
			}
			Array.Sort(array, (RaycastHit left, RaycastHit right) => left.distance.CompareTo(right.distance));
			RaycastHit[] array2 = array;
			foreach (RaycastHit raycastHit in array2)
			{
				SpriteButton button2 = GetButton(raycastHit.transform.name);
				if (Globals.IsDebugInput)
				{
				}
				if (button2 != null && button2.Active)
				{
					button = button2;
					if (down)
					{
						flag = true;
					}
					break;
				}
			}
			ActiveState activeState = _activeState;
			_activeState = ChangeActiveState(down, button, pos);
			if (!Globals.IsDebugInput)
			{
			}
		}
		if (flag || !singleDown || !(Globals.Battle != null))
		{
			return;
		}
		Battle battle = Globals.Battle;
		Ray ray2 = battle.BubbleCam.ScreenPointToRay(pos);
		RaycastHit[] array3 = Physics.RaycastAll(ray2, 10000f, 2560);
		bool flag2 = false;
		RaycastHit[] array4 = array3;
		foreach (RaycastHit raycastHit2 in array4)
		{
			GameObject gameObject = raycastHit2.collider.gameObject;
			BubbleFatality component = gameObject.GetComponent<BubbleFatality>();
			if (component != null)
			{
				component.Die();
				SingletonT<SoundManager>.I.PlayGlobalSound("click_resurrection");
				SingletonT<ServerData>.I.PlayerParams.ResurrectionSpheresCount++;
				continue;
			}
			BubbleMagicGame2 component2 = gameObject.GetComponent<BubbleMagicGame2>();
			if (component2 != null && !flag2)
			{
				BubblesMagicGame2 bubbles = Globals.Battle._bubbles2;
				if (bubbles != null && bubbles._magicGame2List != null && bubbles._magicGame2List.Remove(component2))
				{
					flag2 = true;
					component2.Destroy();
					SingletonT<SoundManager>.I.PlayGlobalSound("click_blackball");
					Messenger.Invoke(Globals.MsgFatalityModeSlicesChanged, bubbles._count - bubbles._magicGame2List.Count, bubbles._count);
					if (bubbles._magicGame2List.Count == 0)
					{
						bubbles._magicGame2List = null;
						Messenger.Invoke(Globals.Msg_MagicGame_Finished, "weak", arg2: true);
					}
					continue;
				}
			}
			Bubble component3 = gameObject.GetComponent<Bubble>();
			if (component3 != null)
			{
				if (component3.Type == Bubble.TypeE.Mana && !Globals.ForceDontClickManaBubbles)
				{
					component3.Destroy();
					Bubbles._unlockHudBubblesTime = 0.15f;
					Messenger<bool>.Invoke(Globals.MsgGuiBattle_SetMagicBarVisible, arg1: false);
					Globals.Player.Mana++;
					SingletonT<SoundManager>.I.PlayGlobalSound("click_manaball");
					Messenger.Invoke(Globals.MsgFightManaBallClicked);
				}
				else if (component3.Type == Bubble.TypeE.Rage && !Globals.ForceDontClickRageBubbles)
				{
					component3.Destroy();
					Bubbles._unlockHudBubblesTime = 0.15f;
					Messenger<bool>.Invoke(Globals.MsgGuiBattle_SetMagicBarVisible, arg1: false);
					SingletonT<SoundManager>.I.PlayGlobalSound("click_rageball");
					SingletonT<ServerData>.I.PlayerParams.RageSpheresCount++;
					Messenger.Invoke(Globals.MsgFightRageBallClicked);
				}
			}
			else if (gameObject.layer == 11 && !Globals.ForceDontClickViewSector)
			{
				if (gameObject.name == "green3")
				{
					Messenger.Invoke(Globals.MsgFightViewSectorClicked, 3);
				}
				else if (gameObject.name == "green2")
				{
					Messenger.Invoke(Globals.MsgFightViewSectorClicked, 2);
				}
				else if (gameObject.name == "green1")
				{
					Messenger.Invoke(Globals.MsgFightViewSectorClicked, 1);
				}
			}
		}
	}
}
