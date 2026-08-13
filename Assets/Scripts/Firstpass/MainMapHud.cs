using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class MainMapHud : MonoBehaviour
{
	public static readonly string MobId = "mm_monster";

	public static readonly string MoneyId = "mm_money";

	public static readonly string CaveId = "mm_cave";

	public static readonly string ChestId = "mm_chest";

	public static readonly string ZachistkaId = "mm_zhachistka";

	public static readonly string FlagId = "mm_flag";

	public static readonly string AltId = "mm_alternative";

	public static readonly string BossId = "mm_mini_boss";

	private readonly float _inertiaDuration = 0.5f;

	private readonly Vector2 _mainMapSize = new Vector2(2048f, 1280f);

	private readonly Stack<GameObject> _buttons = new Stack<GameObject>();

	private CompositeDisposable _listeners;

	private float _startTime;

	private Vector2 _startSpeed;

	private Vector2 _currentSpeed;

	private Vector2 _savedSpeed;

	private bool _isMoving;

	public GameObject SoveringIcon;

	public GameObject ProgressBar;

	public GameObject MainMapQuad;

	public GameObject ZachistkaButtonProto;

	public GameObject MoneyButtonProto;

	public GameObject ChestButtonProto;

	public GameObject MobButtonProto;

	public GameObject FlagButtonProto;

	public GameObject BossButtonProto;

	public GameObject AltButtonProto;

	public GameObject LockedButtonProto;

	public GameObject MineButtonProto;

	public Sprite ArrowNorth;

	public Sprite ArrowSouth;

	public Sprite ArrowEast;

	public Sprite ArrowWest;

	private SpriteGui _spriteGui;

	private void OnEnable()
	{
		_spriteGui.MoveBegin += LocationHud_MoveBegin;
		_spriteGui.Move += LocationHud_Move;
		_spriteGui.MoveEnd += LocationHud_MoveEnd;
		_spriteGui.Release += LocationHud_Release;
		_listeners = new CompositeDisposable();
		_listeners.Add(Messenger.AddListener(Globals.MsgNewSaveSlotIndex, RefreshLocationButtons));
		_listeners.Add(Messenger<ServerData.Location, int>.AddListener(Globals.MsgLocationMobsAdded, MsgLocationMobsAddedHandler));
		_listeners.Add(Messenger<ServerData.Location>.AddListener(Globals.MsgLocationMobsRemoved, MsgLocationMobsRemovedHandler));
		_listeners.Add(Messenger<ServerData.Location, int, int>.AddListener(Globals.MsgLocationMoneyChanged, delegate(ServerData.Location _1, int _2, int _3)
		{
			MsgLocationMobsAddedHandler(_1, _3);
		}));
		_listeners.Add(Messenger<ServerData.Location, int>.AddListener(Globals.MsgLocationPopulationChanged, MsgLocationMobsAddedHandler));
		_listeners.Add(Messenger<LocationLogic, LocationLogic.ChestOnLocation>.AddListener(Globals.Msg_ChestOnLocationRemoved, MsgLocationChestRemoved));
		_listeners.Add(Messenger<LocationLogic>.AddListener(Globals.Msg_ChestOnLocationAdded, MsgLocationChestChanged));
		_listeners.Add(Messenger<LocationLogic>.AddListener(Globals.Msg_ChestOnLocationWasFound, MsgLocationChestChanged));
		_listeners.Add(Messenger<ServerData.Location>.AddListener(Globals.MsgZachistkaProgressChanged, MsgZachistkaProgressChangedHandler));
		_listeners.Add(Messenger<ServerData.Location, OpenCondition.DoneReasonE>.AddListener(Globals.Msg_LocationOpenConditionDone, MsgLocationOpenConditionDoneHandler));
		_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
		if (Globals.BuildType == Globals.BuildTypeE.InnerRelease)
		{
			Globals.HideDebugButtons();
		}
	}

	private void OnDisable()
	{
		_spriteGui.MoveBegin -= LocationHud_MoveBegin;
		_spriteGui.Move -= LocationHud_Move;
		_spriteGui.MoveEnd -= LocationHud_MoveEnd;
		_spriteGui.Release -= LocationHud_Release;
		_listeners.Dispose();
	}

	private void Awake()
	{
		_spriteGui = base.transform.GetSpriteGui();
		if (Globals.BuildType != Globals.BuildTypeE.InnerRelease)
		{
			Utils.DestroyComponentThenAddNew<MainMapOnGui>(base.gameObject);
		}
		if (MainMapQuad == null)
		{
			MainMapQuad = GameObject.Find("main_map_quad");
			if (MainMapQuad == null && Globals.IsDebugBuild)
			{
				Debug.LogError("main_map_quad is missing");
			}
		}
		ArrowEast.transform.localPosition = new Vector3(Camera2D.ScreenWidth / 2 - 100, 0f, 0f);
		ArrowWest.transform.localPosition = new Vector3(-Camera2D.ScreenWidth / 2, 0f, 0f);
		ArrowNorth.transform.localPosition = new Vector3(0f, Camera2D.ScreenHeight / 2 - 126, 0f);
		ArrowSouth.transform.localPosition = new Vector3(0f, -Camera2D.ScreenHeight / 2, 0f);
	}

	private void LateUpdate()
	{
		if (HudMk1.Instance == null || !(Globals.MainMenu != null) || !(HudMk1.Instance != null) || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.MainMap)
		{
			return;
		}
		Vector3 localPosition = MainMapQuad.transform.localPosition;
		localPosition += new Vector3(_currentSpeed.x, _currentSpeed.y, 0f) * Time.deltaTime;
		_currentSpeed = _savedSpeed;
		if (_isMoving)
		{
			_currentSpeed = default(Vector2);
		}
		else
		{
			float magnitude = _currentSpeed.magnitude;
			if (magnitude > 0.1f)
			{
				float num = Time.time - _startTime;
				_currentSpeed = Vector2.Lerp(_startSpeed, Vector2.zero, num / _inertiaDuration);
			}
			else
			{
				_currentSpeed = default(Vector2);
			}
			_savedSpeed = _currentSpeed;
		}
		Vector2 mainMapSize = _mainMapSize;
		float x = mainMapSize.x - (float)Camera2D.ScreenWidth;
		Vector2 mainMapSize2 = _mainMapSize;
		Vector2 vector = new Vector2(x, mainMapSize2.y - (float)Camera2D.ScreenHeight);
		Vector2 vector2 = new Vector2(Camera2D.ScreenWidth / 2, Camera2D.ScreenHeight / 2);
		float num2 = 0f - vector2.x;
		if (localPosition.x >= num2)
		{
			localPosition.x = num2;
			_savedSpeed.x = 0f;
			ArrowWest.Tint_ = new Color(ArrowWest.Tint.r, ArrowWest.Tint.g, ArrowWest.Tint.b, 0f);
		}
		else
		{
			ArrowWest.Tint_ = new Color(ArrowWest.Tint.r, ArrowWest.Tint.g, ArrowWest.Tint.b, (num2 - localPosition.x) / vector.x);
		}
		float num3 = 0f - vector2.x;
		Vector2 mainMapSize3 = _mainMapSize;
		num2 = num3 - mainMapSize3.x + (float)Camera2D.ScreenWidth;
		if (localPosition.x <= num2)
		{
			localPosition.x = num2;
			_savedSpeed.x = 0f;
			ArrowEast.Tint_ = new Color(ArrowEast.Tint.r, ArrowEast.Tint.g, ArrowEast.Tint.b, 0f);
		}
		else
		{
			ArrowEast.Tint_ = new Color(ArrowEast.Tint.r, ArrowEast.Tint.g, ArrowEast.Tint.b, (localPosition.x - num2) / vector.x);
		}
		num2 = vector2.y;
		if (localPosition.y <= num2)
		{
			localPosition.y = num2;
			_savedSpeed.y = 0f;
			ArrowNorth.Tint_ = new Color(ArrowNorth.Tint.r, ArrowNorth.Tint.g, ArrowNorth.Tint.b, 0f);
		}
		else
		{
			ArrowNorth.Tint_ = new Color(ArrowNorth.Tint.r, ArrowNorth.Tint.g, ArrowNorth.Tint.b, (localPosition.y - num2) / vector.y);
		}
		float y = vector2.y;
		Vector2 mainMapSize4 = _mainMapSize;
		num2 = y + mainMapSize4.y - (float)Camera2D.ScreenHeight;
		if (localPosition.y >= num2)
		{
			localPosition.y = num2;
			_savedSpeed.y = 0f;
			ArrowSouth.Tint_ = new Color(ArrowSouth.Tint.r, ArrowSouth.Tint.g, ArrowSouth.Tint.b, 0f);
		}
		else
		{
			ArrowSouth.Tint_ = new Color(ArrowSouth.Tint.r, ArrowSouth.Tint.g, ArrowSouth.Tint.b, (num2 - localPosition.y) / vector.y);
		}
		localPosition.x = Mathf.Round(localPosition.x);
		localPosition.y = Mathf.Round(localPosition.y);
		MainMapQuad.transform.localPosition = localPosition;
	}

	public void DestroyLocationButtons()
	{
		while (_buttons.Count > 0)
		{
			GameObject gameObject = _buttons.Pop();
			SpriteButton componentInChildren = gameObject.GetComponentInChildren<SpriteButton>();
			componentInChildren.UnregisterMe();
			gameObject.transform.Eliminate();
		}
	}

	public void RefreshLocationButtons()
	{
		DestroyLocationButtons();
		foreach (ServerData.Location value in SingletonT<ServerData>.I._locations.Values)
		{
			int id = value.Id;
			if (value.IsShowMobs)
			{
				ShowMob(id, value.IconMobsCoord);
			}
			if (value.IsShowMoney)
			{
				ShowMoney(id, value.IconMoneyCoord);
			}
			if (value.IsShowChests)
			{
				Vector2 iconChestCoord = value.IconChestCoord;
				if (value.Id == 283)
				{
					iconChestCoord.x -= 120f;
				}
				ShowChests(id, iconChestCoord);
			}
			if (value.IsOpened && value.IsCave)
			{
				ShowCave(id, value.CaveGlobalCoord);
			}
			if (value.IsZachistkaOpened || Globals.ForceShowAllLocationsOnMap)
			{
				if (value.IsMiniBoss)
				{
					if (value.Logic.OpenCondition != null && value.Logic.OpenCondition.Done)
					{
						ShowBoss(id, value.IconZachistkaCoord);
					}
					else
					{
						ShowLocked(id, value.IconZachistkaCoord);
					}
				}
				else if (value.IsAltPath)
				{
					if (value.Logic.OpenCondition != null && value.Logic.OpenCondition.Done)
					{
						ShowAlt(id, value.IconZachistkaCoord);
					}
					else
					{
						ShowLocked(id, value.IconZachistkaCoord);
					}
				}
				else
				{
					ShowZachistka(id, value.IconZachistkaCoord);
				}
			}
			if ((value.IsOpened && !value.IsZachistkaOpened && !value.IsShowMoney && !value.IsShowMobs) || Globals.ForceShowAllLocations)
			{
				ShowFlag(id, value.IconZachistkaCoord);
			}
		}
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (old == GuiRoot.GuiType.MainMap)
		{
			DestroyLocationButtons();
		}
		if (@new != GuiRoot.GuiType.MainMap)
		{
			return;
		}
		if (SingletonT<ServerData>.I.GameProgress() == 100)
		{
			if (ProgressBar != null)
			{
				ProgressBar.SetActiveRecursivelyMk1(setActive: false);
			}
		}
		else if (ProgressBar != null)
		{
			ProgressBar.SetActiveRecursivelyMk1(setActive: true);
		}
		bool flag = true;
		if (SoveringIcon != null)
		{
			foreach (KeyValuePair<int, ServerData.Location> location in SingletonT<ServerData>.I._locations)
			{
				ServerData.Location.BotLocationInfo[] bots = location.Value.Bots;
				foreach (ServerData.Location.BotLocationInfo botLocationInfo in bots)
				{
					if (botLocationInfo.Bot.Id == MainMenu.SovereighnMobId && location.Value.Logic.ZachistkaMobsKilled >= location.Value.Bots.Length)
					{
						flag = false;
					}
				}
			}
			SoveringIcon.SetActiveRecursivelyMk1(flag);
		}
		Utils.Log("MainMapHud.OnMsgGuiSwitchToBefore: ", flag, Globals.IsFinalScreenShowed);
		if (!flag && !Globals.IsFinalScreenShowed)
		{
			Globals.IsFinalScreenShowed = true;
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Final);
		}
	}

	private void LocationHud_Move(Vector3 arg1, Vector3 arg2)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			_currentSpeed = arg2 - arg1;
			_currentSpeed *= Camera2D.Scale;
			_currentSpeed *= 1f / Time.deltaTime;
			_savedSpeed = _currentSpeed;
			_startSpeed = _currentSpeed;
			_startTime = Time.time;
		}
	}

	private void LocationHud_MoveBegin(Vector3 obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			_isMoving = true;
		}
	}

	private void LocationHud_MoveEnd(Vector3 obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			_isMoving = false;
		}
	}

	private void LocationHud_Release(SpriteButton obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap && obj.name == "boss_cell")
		{
			SingletonT<SoundManager>.I.PlayGlobalSound("sovering_threat");
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.SovereighnInfo);
		}
	}

	private void ShowZachistka(int id, Vector2 iconZachistkaCoord)
	{
		MessUpWithButton(id, iconZachistkaCoord, ZachistkaButtonProto, ZachistkaId);
	}

	private void ShowFlag(int id, Vector2 iconZachistkaCoord)
	{
		MessUpWithButton(id, iconZachistkaCoord, FlagButtonProto, FlagId);
	}

	private void ShowMoney(int id, Vector2 iconMoneyCoord)
	{
		MessUpWithButton(id, iconMoneyCoord, MoneyButtonProto, MoneyId);
	}

	private void ShowCave(int id, Vector2 iconCaveCoord)
	{
		MessUpWithButton(id, iconCaveCoord, MineButtonProto, CaveId);
	}

	private void ShowChests(int id, Vector2 iconChestCoord)
	{
		MessUpWithButton(id, iconChestCoord, ChestButtonProto, ChestId);
	}

	private void ShowMob(int id, Vector2 iconMobsCoord)
	{
		MessUpWithButton(id, iconMobsCoord, MobButtonProto, MobId);
	}

	private void ShowBoss(int id, Vector2 iconBossCoord)
	{
		MessUpWithButton(id, iconBossCoord, BossButtonProto, BossId);
	}

	private void ShowLocked(int id, Vector2 iconBossCoord)
	{
		MessUpWithButton(id, iconBossCoord, LockedButtonProto, BossId);
	}

	private void ShowAlt(int id, Vector2 altPathCoord)
	{
		MessUpWithButton(id, altPathCoord, AltButtonProto, AltId);
	}

	private void MessUpWithButton(int id, Vector2 coord, GameObject proto, string sid)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(proto);
		gameObject.transform.parent = MainMapQuad.transform;
		gameObject.transform.SetLayerRecursively(base.transform);
		coord.y = 0f - coord.y;
		gameObject.transform.localPosition = coord.ToVector3(-100f);
		MapButton componentInChildren = gameObject.GetComponentInChildren<MapButton>();
		componentInChildren.name = $"{sid}/{id}";
		if (componentInChildren.GetComponent<Animation>() != null)
		{
			componentInChildren.GetComponent<Animation>()["Take 001"].time = componentInChildren.GetComponent<Animation>().clip.length * Random.value;
		}
		else if (componentInChildren.CustomAnimation != null)
		{
			componentInChildren.CustomAnimation["Take 001"].time = componentInChildren.CustomAnimation.clip.length * Random.value;
		}
		componentInChildren.Init();
		componentInChildren.SetActive();
		_buttons.Push(gameObject);
	}

	private void MsgLocationMobsRemovedHandler(ServerData.Location location)
	{
		if (Globals.GameDataLoaded && HudMk1.Instance != null && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			RefreshLocationButtons();
		}
	}

	private void MsgLocationMobsAddedHandler(ServerData.Location location, int index)
	{
		if (Globals.GameDataLoaded && HudMk1.Instance != null && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			RefreshLocationButtons();
		}
	}

	private void MsgLocationChestRemoved(LocationLogic logic, LocationLogic.ChestOnLocation chest)
	{
		if (Globals.GameDataLoaded && HudMk1.Instance != null && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			RefreshLocationButtons();
		}
	}

	private void MsgLocationChestChanged(LocationLogic logic)
	{
		if (Globals.GameDataLoaded && HudMk1.Instance != null && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			RefreshLocationButtons();
		}
	}

	private void MsgZachistkaProgressChangedHandler(ServerData.Location location)
	{
		if (Globals.GameDataLoaded && HudMk1.Instance != null && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			RefreshLocationButtons();
		}
	}

	private void MsgLocationOpenConditionDoneHandler(ServerData.Location location, OpenCondition.DoneReasonE reason)
	{
		if (Globals.GameDataLoaded && HudMk1.Instance != null && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			RefreshLocationButtons();
		}
	}
}
