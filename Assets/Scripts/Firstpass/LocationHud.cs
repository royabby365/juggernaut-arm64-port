using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class LocationHud : MonoBehaviour
{
	private class ChestZone
	{
		internal LocationLogic.ChestOnLocation Chest;

		internal int MoneId = -1;
	}

	private const int CURSOR_Z = -580;

	private const int STATIC_CURSOR = 0;

	private const int ANIMATED_CURSOR = 1;

	private const int MONEY_PILES_MIN = 4;

	public int ChestZoneWidth = 80;

	public int MoneyZoneWidth = 50;

	private static int _uniqueId;

	private readonly string _moneyId = "l_money_";

	private readonly string _chestId = "l_chest_";

	private readonly Vector2 _locationSize = new Vector2(1400f, 800f);

	private readonly float _inertiaDuration = 0.5f;

	private readonly Dictionary<Rect, ChestZone> _chestZones = new Dictionary<Rect, ChestZone>();

	private readonly Dictionary<Rect, ChestZone> _moneyZones = new Dictionary<Rect, ChestZone>();

	private readonly List<LocationChest> _chests = new List<LocationChest>();

	private readonly List<LocationMoney> _moneyPiles = new List<LocationMoney>();

	private readonly Dictionary<LocationMoney, GameObject> _animatedMoneyButtons = new Dictionary<LocationMoney, GameObject>();

	private readonly Dictionary<LocationChest, GameObject> _animatedChestButtons = new Dictionary<LocationChest, GameObject>();

	private GameObject _animatedCaveIconGO;

	private MapButton _animatedCaveButton;

	private CompositeDisposable _listeners;

	private float _startTime;

	private Vector2 _startSpeed;

	private Vector2 _currentSpeed;

	private Vector2 _savedSpeed;

	private bool _isMoving;

	private Material _quadMaterial;

	private SpriteGui _spriteGui;

	internal ServerData.Location _location;

	private FightScreenMobIcon[] _enemies;

	private Sprite _fightButton;

	private int _selectedBot;

	private bool _isScarabActive;

	private Vector3 _cursorPos;

	private GameObject[] _cursors;

	private int _currentCursor;

	private GameObject _scarabSearchEffect;

	private float _scarabRadius = 150f;

	private AudioSource _flySound;

	private GameObject _chestIcon;

	private int _scrollBorder = 150;

	private GameObject _highlightPrefab;

	private GameObject _highlightChests;

	public GameObject LocationQuad;

	public GameObject MobIcons;

	public GameObject[] ChestProtos;

	public SpriteText PopulationCount;

	public GameObject TakemoneyAnim;

	public GameObject AnimatedMoneyButtonPrefab;

	public GameObject AnimatedChestButtonPrefab;

	public GameObject AnimatedMineButtonPrefab;

	public SpriteText Messages;

	public Sprite Arrow;

	public SpriteText ScarabCount;

	public GameObject ButtonDig;

	public GameObject ButtonFindChest;

	public GameObject ButtonCancelDig;

	public GameObject ScarabCursorPrefab;

	public GameObject ScarabCursorAnimatedPrefab;

	public GameObject ScarabCountIcon;

	public GameObject ScarabTrail;

	public GameObject ScarabSearchEffectPrefab;

	public GameObject ScarabOnChestEffect;

	public GameObject ScarabHideEffectPrefab;

	public GameObject ScarabRoot;

	public AnimationCurve AlphaCurve;

	public int ScrollBorderSpeed = 5;

	public Sprite ArrowEast;

	public Sprite ArrowWest;

	public SpriteText MoneyCount;

	public SpriteText ChestCount;

	public Transform ChestIconHolder;

	public static int UniqueId => _uniqueId++;

	private void Awake()
	{
		_spriteGui = base.transform.GetSpriteGui();
		if (LocationQuad == null)
		{
			LocationQuad = GameObject.Find("location_quad");
			if (LocationQuad == null && Globals.IsDebugBuild)
			{
				Debug.LogError("Location quad is missing");
			}
		}
		MeshRenderer meshRenderer = (MeshRenderer)LocationQuad.renderer;
		_quadMaterial = meshRenderer.material;
		_cursors = new GameObject[2];
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ScarabCursorPrefab);
		gameObject.transform.parent = ScarabRoot.transform;
		gameObject.transform.localPosition = default(Vector3);
		gameObject.SetActiveRecursivelyMk1(setActive: false);
		gameObject.transform.SetLayerRecursively(base.transform);
		_cursors[0] = gameObject;
		GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(ScarabCursorAnimatedPrefab);
		gameObject2.transform.parent = ScarabRoot.transform;
		gameObject2.transform.localPosition = default(Vector3);
		gameObject2.SetActiveRecursivelyMk1(setActive: false);
		gameObject2.transform.SetLayerRecursively(base.transform);
		_cursors[1] = gameObject2;
		_scarabSearchEffect = (GameObject)UnityEngine.Object.Instantiate(ScarabSearchEffectPrefab);
		_scarabSearchEffect.transform.parent = ScarabRoot.transform;
		_scarabSearchEffect.transform.localPosition = default(Vector3);
		_scarabSearchEffect.transform.SetLayerRecursively(base.transform);
		_currentCursor = 0;
		ScarabTrail.SetActiveRecursivelyMk1(setActive: false);
		ArrowEast.transform.localPosition = new Vector3(-100f, Camera2D.ScreenHeight / 2, 0f);
		ArrowWest.transform.localPosition = new Vector3(-Camera2D.ScreenWidth, Camera2D.ScreenHeight / 2, 0f);
		_highlightPrefab = Util.Resource<GameObject>("_z_prefabs/combo_button_highlight");
	}

	private void OnEnable()
	{
		_listeners = new CompositeDisposable();
		_listeners.Add(Messenger<ServerData.Location, int, int>.AddListener(Globals.MsgLocationMoneyChanged, MoneyChanged));
		_listeners.Add(Messenger<ServerData.Location, int, bool>.AddListener(Globals.MsgLocationPopulationChanged, PopulationChanged));
		_listeners.Add(Messenger<LocationLogic>.AddListener(Globals.Msg_ChestOnLocationAdded, PlaceChestsOnLocation));
		_listeners.Add(Messenger<LocationLogic, LocationLogic.ChestOnLocation>.AddListener(Globals.Msg_ChestOnLocationRemoved, RemoveChestFromLocation));
		_listeners.Add(Messenger<LocationLogic>.AddListener(Globals.Msg_ExitFromLocation, LocationCleanup));
		_listeners.Add(Messenger<ServerData.MoneyType.TypeE, string>.AddListener(Globals.MsgPlayerFundsChanged, FundsCountChanged));
		_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, GuiSwitchBeforeHandler));
		_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPost, GuiSwitchPostHandler));
		_listeners.Add(Messenger<LocationLogic>.AddListener(Globals.Msg_ChestOnLocationWasFound, UseOneScarab));
		_listeners.Add(Messenger<ServerData.Location, int>.AddListener(Globals.MsgLocationMobsAdded, delegate(ServerData.Location location, int __)
		{
			if (_location != null && !location.Equals(_location) && _location.Id == location.Id)
			{
				Utils.Log("????? MsgLocationMobsAdded", location.Id);
			}
			if (_location != null && _location.Id == location.Id)
			{
				RefreshMobs(_location);
			}
		}));
		_listeners.Add(Messenger<ServerData.Location>.AddListener(Globals.MsgLocationMobsRemoved, delegate(ServerData.Location location)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("LOCATION: PersonDieHandler " + location);
			}
			if (_location != null && _location != location && _location.Id == location.Id)
			{
				Utils.LogForce("!!!!!! MsgLocationMobsRemoved", location.Id);
			}
			if (_location != null && _location.Id == location.Id)
			{
				if (Globals.IsDebugBuild)
				{
					Debug.Log("LOCATION: PersonDieHandler");
				}
				RefreshMobs(_location);
			}
		}));
		_listeners.Add(Messenger<float>.AddListener(Globals.MsgSoundsVolumeChanged, delegate(float volume)
		{
			if (_flySound != null)
			{
				_flySound.volume = volume;
			}
		}));
		_listeners.Add(Messenger.AddListener(Globals.MsgAttackElf, delegate
		{
			ServerData.BotInfo elf = GetElf();
			if (elf != null)
			{
				AreaData.Current.Location.Logic.MobInFight = _location.Logic._mobs.IndexOf(elf);
				Globals.MainMenu.StartFightWith(elf, _location.Logic.ChestsOnLocation[0].Chest.ElfLevelDifference);
			}
		}));
		_spriteGui.MoveBegin += LocationHud_MoveBegin;
		_spriteGui.Move += LocationHud_Move;
		_spriteGui.MoveEnd += LocationHud_MoveEnd;
		_spriteGui.Release += LocationHud_Release;
		GameObject gameObject = new GameObject();
		gameObject.name = "scarab_fly_sound_holder";
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = default(Vector3);
		if (SingletonT<ServerData>.I.GameSettings != null)
		{
			_flySound = gameObject.AddComponent<AudioSource>();
			_flySound.volume = SingletonT<ServerData>.I.GameSettings.SoundsVolume;
			_flySound.loop = true;
		}
	}

	private void OnDisable()
	{
		_spriteGui.MoveBegin -= LocationHud_MoveBegin;
		_spriteGui.Move -= LocationHud_Move;
		_spriteGui.MoveEnd -= LocationHud_MoveEnd;
		_spriteGui.Release -= LocationHud_Release;
		_listeners.Dispose();
		if (_flySound != null)
		{
			_flySound.Eliminate();
		}
	}

	private void UseOneScarab(LocationLogic locationLogic)
	{
		ServerData.MoneyType.TypeE.Scarab.ChangePlayerFundsCount(-1);
	}

	private void LateUpdate()
	{
		if (HudMk1.Instance == null || !(Globals.MainMenu != null) || !(HudMk1.Instance != null) || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Location)
		{
			return;
		}
		if (_isScarabActive)
		{
			Vector3 cursorPos = Vector3.Lerp(ScarabRoot.transform.localPosition, _cursorPos, 0.5f * Time.deltaTime);
			ScarabRoot.transform.localPosition = cursorPos;
			float num = float.MaxValue;
			LocationChest locationChest = null;
			foreach (LocationChest chest in _chests)
			{
				if (!chest.ChestonLocation.WasFound)
				{
					float num2 = Vector2.Distance(ScarabRoot.transform.localPosition, chest.transform.localPosition + new Vector3(chest.cellW / 2f, (0f - chest.cellH) / 2f));
					if (num > num2)
					{
						locationChest = chest;
						num = num2;
					}
				}
			}
			if (locationChest == null)
			{
				_scarabSearchEffect.transform.localScale = default(Vector3);
			}
			else if (num > _scarabRadius)
			{
				_scarabSearchEffect.transform.localScale = default(Vector3);
			}
			else
			{
				float num3 = 1f - num / _scarabRadius;
				_scarabSearchEffect.transform.localScale = new Vector3(num3, num3, num3);
			}
			int num4 = FindChest(ref cursorPos);
			if (num4 != -1)
			{
				if (_currentCursor != 1)
				{
					SwapCursors();
					GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ScarabOnChestEffect);
					gameObject.transform.parent = ScarabRoot.transform;
					gameObject.transform.localPosition = default(Vector3);
				}
				float num5 = Mathf.Lerp(0.9f, 1.05f, Mathf.Sin(Time.realtimeSinceStartup * 5f) * 0.5f + 0.5f);
				ButtonDig.transform.localScale = new Vector3(num5, num5, num5);
				NewGuiButtonMk1 componentInChildren = ButtonDig.GetComponentInChildren<NewGuiButtonMk1>();
				componentInChildren.SetActive();
			}
			else
			{
				if (_currentCursor != 0)
				{
					SwapCursors();
				}
				ButtonDig.transform.localScale = Vector3.one;
				NewGuiButtonMk1 componentInChildren2 = ButtonDig.GetComponentInChildren<NewGuiButtonMk1>();
				componentInChildren2.SetInactive();
			}
			return;
		}
		Vector3 localPosition = LocationQuad.transform.localPosition;
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
				float num6 = Time.time - _startTime;
				_currentSpeed = Vector2.Lerp(_startSpeed, Vector2.zero, num6 / _inertiaDuration);
			}
			else
			{
				_currentSpeed = default(Vector2);
			}
			_savedSpeed = _currentSpeed;
		}
		localPosition = Limit(localPosition);
		LocationQuad.transform.localPosition = localPosition;
	}

	private int FindChest(ref Vector3 cursorPos)
	{
		int num = -1;
		for (int i = 0; i < _chests.Count; i++)
		{
			LocationChest locationChest = _chests[i];
			if (!locationChest.ChestonLocation.WasFound && num == -1)
			{
				Rect rect = new Rect(locationChest.transform.localPosition.x, locationChest.transform.localPosition.y - locationChest.cellH, locationChest.cellW, locationChest.cellH);
				if (rect.Contains(cursorPos))
				{
					num = i;
					break;
				}
			}
		}
		return num;
	}

	private void SwapCursors()
	{
		_cursors[_currentCursor].SetActiveRecursivelyMk1(setActive: false);
		_currentCursor = 1 - _currentCursor;
		_cursors[_currentCursor].SetActiveRecursivelyMk1(setActive: true);
	}

	public void InitLocation(ServerData.Location location, ActionD onInit)
	{
		if (_location != null && location.Id == location.Id)
		{
			onInit();
			return;
		}
		if (MobIcons != null)
		{
			_enemies = MobIcons.GetComponentsInChildren<FightScreenMobIcon>();
			_enemies = _enemies.OrderByDescending((FightScreenMobIcon e) => e.transform.localPosition.x).ToArray();
			Transform transform = MobIcons.transform.FindChildByName("attack_button", includeInactive: true);
			_fightButton = transform.GetComponent<Sprite>();
			FightScreenMobIcon[] enemies = _enemies;
			foreach (FightScreenMobIcon fightScreenMobIcon in enemies)
			{
				fightScreenMobIcon.SetState(FightScreenMobIcon.State.OutOfGui);
			}
			_fightButton.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		if (_location != null)
		{
			Utils.LogForce("InitLocation cleanup", _location);
		}
		_location = location;
		Utils.LogForce("InitLocation texture load", location);
		LoadLocationTexture($"{location.Id}_2k", delegate
		{
			try
			{
				Utils.LogForce("InitLocation setup", location);
				SetupLocation();
				onInit();
			}
			catch (Exception ex)
			{
				Utils.Log("INITLOCATION FAILED", ex.Message, ex.StackTrace);
				throw;
			}
		});
	}

	private void SetupLocation()
	{
		if (_animatedCaveIconGO != null)
		{
			UnityEngine.Object.Destroy(_animatedCaveIconGO);
			_animatedCaveButton = null;
		}
		if (_location.IsCave)
		{
			Debug.Log("Create");
			_animatedCaveIconGO = (GameObject)UnityEngine.Object.Instantiate(AnimatedMineButtonPrefab);
			_animatedCaveIconGO.transform.parent = LocationQuad.transform;
			_animatedCaveIconGO.transform.SetLayerRecursively(base.gameObject.transform);
			_animatedCaveIconGO.transform.localPosition = new Vector3(_location.CaveLocationCoord.x, _location.CaveLocationCoord.y, -50f);
			_animatedCaveButton = _animatedCaveIconGO.GetComponentInChildren<MapButton>();
			_animatedCaveButton.gameObject.name = "cave_" + UniqueId;
			_animatedCaveButton.Init();
			_animatedCaveButton.SetActive();
		}
		string path = $"locations/Zones/{_location.Id}-zones";
		GameObject gameObject = Util.Resource<GameObject>(path);
		if (gameObject != null)
		{
			SerializedRectangles component = gameObject.transform.GetComponent<SerializedRectangles>();
			UpdateZones(component);
			foreach (KeyValuePair<Rect, ChestZone> chestZone in _chestZones)
			{
				foreach (LocationLogic.ChestOnLocation item in _location.Logic.ChestsOnLocation)
				{
					if (item.X >= 0 && item.Y >= 0)
					{
						Vector2 point = new Vector2(item.X, item.Y);
						if (chestZone.Key.Contains(point))
						{
							item.RectOnLocation = chestZone.Key;
							chestZone.Value.Chest = item;
							break;
						}
					}
					else
					{
						item.RectOnLocation = default(Rect);
					}
				}
			}
			if (Globals.IsDebugBuild)
			{
				Debug.Log($"LOCATION: {_chestZones.Count} ZONES LOADED");
			}
		}
		else if (Globals.IsDebugBuild)
		{
			Debug.Log("[FUCKUP -- ZONES]");
		}
		if (_location != null)
		{
			MoneyChanged(_location, _location.Logic.Money, _location.Logic.Money);
			PopulationChanged(_location, _location.Logic.Population, grow: true);
			PlaceChestsOnLocation(_location.Logic);
			RefreshMobs(_location);
		}
		MoneyCount.Text_ = SingletonT<ServerData>.I.PlayerParams.MoneyGoldCount.ToString();
		ScarabCount.Text_ = SingletonT<ServerData>.I.PlayerParams.MoneyScarabCount.ToString();
		GameObject[] cursors = _cursors;
		foreach (GameObject gameObject2 in cursors)
		{
			gameObject2.SetActiveRecursivelyMk1(setActive: false);
		}
		ButtonDig.SetActiveRecursivelyMk1(setActive: false);
		ButtonCancelDig.SetActiveRecursivelyMk1(setActive: false);
		ButtonFindChest.SetActiveRecursivelyMk1(setActive: true);
		_isScarabActive = false;
		HideTrail();
		if (_chestIcon != null)
		{
			UnityEngine.Object.Destroy(_chestIcon);
		}
		GameObject original = ChestProtos[int.Parse(_location.Chests[0].Chest.Index) - 1];
		_chestIcon = (GameObject)UnityEngine.Object.Instantiate(original);
		_chestIcon.transform.parent = ChestIconHolder;
		_chestIcon.transform.localPosition = default(Vector3);
		LocationChest componentInChildren = _chestIcon.GetComponentInChildren<LocationChest>();
		componentInChildren.Animate = false;
		componentInChildren.SetInactive();
		_chestIcon.transform.SetLayerRecursively(base.transform);
	}

	private void FundsCountChanged(ServerData.MoneyType.TypeE moneyType, string reason)
	{
		switch (moneyType)
		{
		case ServerData.MoneyType.TypeE.Gold:
			MoneyCount.Text_ = SingletonT<ServerData>.I.PlayerParams.MoneyGoldCount.ToString();
			break;
		case ServerData.MoneyType.TypeE.Scarab:
			ScarabCount.Text_ = SingletonT<ServerData>.I.PlayerParams.MoneyScarabCount.ToString();
			break;
		}
	}

	private void GuiSwitchPostHandler(GuiRoot.GuiType oldGui, GuiRoot.GuiType newGui)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Location)
		{
			return;
		}
		if (_moneyPiles.Count > 0)
		{
			Messenger.Invoke(Globals.MsgLocationMoneyPilesAdded);
		}
		if (_location.Logic._mobs.Count > 0)
		{
			Messenger.Invoke(Globals.MsgLocationMobAttack);
		}
		if (_location.Logic.ChestsOnLocation.Count <= 0)
		{
			return;
		}
		int num = 0;
		foreach (LocationLogic.ChestOnLocation item in _location.Logic.ChestsOnLocation)
		{
			if (!item.WasFound)
			{
				num++;
			}
		}
		if (num > 0)
		{
			_highlightChests = (GameObject)UnityEngine.Object.Instantiate(_highlightPrefab);
			_highlightChests.transform.parent = ChestIconHolder;
			_highlightChests.transform.localPosition = new Vector3(0f, -20f, 50f);
			StartCoroutine(ShowHighlight(_highlightChests));
		}
	}

	private void GuiSwitchBeforeHandler(GuiRoot.GuiType oldGui, GuiRoot.GuiType newGui)
	{
		if (oldGui == GuiRoot.GuiType.Location)
		{
			LocationQuad.transform.localPosition = default(Vector3);
		}
		if (newGui != GuiRoot.GuiType.Location && _isScarabActive)
		{
			ButtonDig.SetActiveRecursivelyMk1(setActive: false);
			ButtonCancelDig.SetActiveRecursivelyMk1(setActive: false);
			ButtonFindChest.SetActiveRecursivelyMk1(setActive: true);
			Dig(isDigging: false);
		}
	}

	private void UpdateZones(SerializedRectangles sr)
	{
		Rect[] smallRectangles = sr.SmallRectangles;
		foreach (Rect key in smallRectangles)
		{
			_moneyZones[key] = new ChestZone();
		}
		Rect[] bigRectangles = sr.BigRectangles;
		foreach (Rect key2 in bigRectangles)
		{
			_chestZones[key2] = new ChestZone();
		}
	}

	private Vector3 Limit(Vector3 currentPos)
	{
		Vector2 locationSize = _locationSize;
		float num = locationSize.x - (float)Camera2D.ScreenWidth;
		float num2 = -Camera2D.ScreenWidth;
		if (currentPos.x >= num2)
		{
			currentPos.x = num2;
			_savedSpeed.x = 0f;
			ArrowWest.Tint_ = new Color(ArrowWest.Tint.r, ArrowWest.Tint.g, ArrowWest.Tint.b, 0f);
		}
		else
		{
			ArrowWest.Tint_ = new Color(ArrowWest.Tint.r, ArrowWest.Tint.g, ArrowWest.Tint.b, (num2 - currentPos.x) / num);
		}
		Vector2 locationSize2 = _locationSize;
		num2 = 0f - locationSize2.x;
		if (currentPos.x <= num2)
		{
			currentPos.x = num2;
			_savedSpeed.x = 0f;
			ArrowEast.Tint_ = new Color(ArrowEast.Tint.r, ArrowEast.Tint.g, ArrowEast.Tint.b, 0f);
		}
		else
		{
			ArrowEast.Tint_ = new Color(ArrowEast.Tint.r, ArrowEast.Tint.g, ArrowEast.Tint.b, (currentPos.x - num2) / num);
		}
		num2 = Camera2D.ScreenHeight;
		if (currentPos.y < num2)
		{
			currentPos.y = num2;
			_savedSpeed.y = 0f;
		}
		Vector2 locationSize3 = _locationSize;
		num2 = locationSize3.y;
		if (currentPos.y > num2)
		{
			currentPos.y = num2;
			_savedSpeed.y = 0f;
		}
		return currentPos;
	}

	private void LoadLocationTexture(string textureName, ActionD onLoad)
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log("LOCATION: TEXTURE NAME: " + textureName);
		}
		string text = "resources/locations/" + textureName;
		SingletonT<ResourcesManager>.I.GetAssetBundleAsync(Globals.MainMenu, ResourcesManager.GetAssetBundlePath(text), delegate(string _, ResourcesManager.AssetBundleData ab, float time)
		{
			SetLocationTexture((Texture2D)ab.Bundle.Load(textureName));
			SingletonT<ResourcesManager>.I.RemoveAssetBundleNoActions(ab);
			onLoad();
		}, delegate(string _, string errorMessage)
		{
			Utils.LogForce("LocationHud.LoadLocationTexture", errorMessage);
			onLoad();
		});
	}

	private void SetLocationTexture(Texture2D tex)
	{
		if (_quadMaterial != null)
		{
			Texture mainTexture = _quadMaterial.mainTexture;
			_quadMaterial.mainTexture = tex;
			_quadMaterial.mainTextureScale = new Vector2(1f, 2f);
			if (mainTexture != tex && mainTexture != null)
			{
				Debug.Log("----------UnloadTexture " + mainTexture.name);
				Resources.UnloadAsset(mainTexture);
			}
		}
	}

	private void LocationHud_Move(Vector3 arg1, Vector3 arg2)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Location)
		{
			return;
		}
		arg1 *= Camera2D.Scale * GetWorldScaleX();
		arg2 *= Camera2D.Scale * GetWorldScaleY();
		_currentSpeed = arg2 - arg1;
		_currentSpeed *= 1f / Time.deltaTime;
		_savedSpeed = _currentSpeed;
		_startSpeed = _currentSpeed;
		_startTime = Time.time;
		if (_isScarabActive)
		{
			if (arg2.x < (float)_scrollBorder)
			{
				LocationQuad.transform.localPosition = new Vector3(LocationQuad.transform.localPosition.x + (float)ScrollBorderSpeed, LocationQuad.transform.localPosition.y, LocationQuad.transform.localPosition.z);
				LocationQuad.transform.localPosition = Limit(LocationQuad.transform.localPosition);
			}
			if (arg2.x > (float)(Camera2D.ScreenWidth - _scrollBorder))
			{
				LocationQuad.transform.localPosition = new Vector3(LocationQuad.transform.localPosition.x - (float)ScrollBorderSpeed, LocationQuad.transform.localPosition.y, LocationQuad.transform.localPosition.z);
				LocationQuad.transform.localPosition = Limit(LocationQuad.transform.localPosition);
			}
			if (arg2.y < (float)_scrollBorder)
			{
				LocationQuad.transform.localPosition = new Vector3(LocationQuad.transform.localPosition.x, LocationQuad.transform.localPosition.y + (float)ScrollBorderSpeed, LocationQuad.transform.localPosition.z);
				LocationQuad.transform.localPosition = Limit(LocationQuad.transform.localPosition);
			}
			if (arg2.y > (float)(Camera2D.ScreenHeight - _scrollBorder))
			{
				LocationQuad.transform.localPosition = new Vector3(LocationQuad.transform.localPosition.x, LocationQuad.transform.localPosition.y - (float)ScrollBorderSpeed, LocationQuad.transform.localPosition.z);
				LocationQuad.transform.localPosition = Limit(LocationQuad.transform.localPosition);
			}
			_cursorPos = new Vector3(arg2.x - LocationQuad.transform.localPosition.x - (float)Camera2D.ScreenWidth, arg2.y - (float)Camera2D.ScreenHeight - LocationQuad.transform.localPosition.y + (float)Camera2D.ScreenHeight, -580f);
		}
	}

	private static float GetWorldScaleX()
	{
		return (float)Camera2D.ScreenWidth / (float)Screen.width;
	}

	private static float GetWorldScaleY()
	{
		return (float)Camera2D.ScreenHeight / (float)Screen.height;
	}

	private void LocationHud_MoveBegin(Vector3 obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Location)
		{
			_isMoving = true;
			obj *= Camera2D.Scale;
			if (_isScarabActive)
			{
				_cursorPos = new Vector3(GetWorldScaleX() * obj.x - LocationQuad.transform.localPosition.x - (float)Camera2D.ScreenWidth, GetWorldScaleY() * obj.y - (float)Camera2D.ScreenHeight - LocationQuad.transform.localPosition.y + (float)Camera2D.ScreenHeight, -580f);
			}
		}
	}

	private void LocationHud_MoveEnd(Vector3 obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Location)
		{
			_isMoving = false;
			obj *= Camera2D.Scale;
			if (_isScarabActive)
			{
				_cursorPos = new Vector3(GetWorldScaleX() * obj.x - LocationQuad.transform.localPosition.x - (float)Camera2D.ScreenWidth, GetWorldScaleY() * obj.y - (float)Camera2D.ScreenHeight - LocationQuad.transform.localPosition.y + (float)Camera2D.ScreenHeight, -580f);
			}
		}
	}

	private void LocationHud_Release(SpriteButton obj)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Location)
		{
			return;
		}
		if (obj == _animatedCaveButton)
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Match3StartScreen, Tuple.Create(_location.CaveName));
		}
		LocationMoney locationMoney = obj as LocationMoney;
		if (locationMoney != null)
		{
			StartTakeMoneyAnimation(obj, locationMoney.moneyCount.ToString());
			RemoveMoneyPileFromLocation(locationMoney);
			SingletonT<ServerData>.I.TakeMoneyFromLocation(_location, locationMoney.moneyCount);
			Messenger.Invoke(Globals.MsgLocationMoneyPileClicked);
			return;
		}
		if (obj.name.StartsWith(_moneyId))
		{
			LocationMoney componentInChildren = obj.transform.parent.parent.parent.GetComponentInChildren<LocationMoney>();
			StartTakeMoneyAnimation(obj, componentInChildren.moneyCount.ToString());
			RemoveMoneyPileFromLocation(componentInChildren);
			SingletonT<ServerData>.I.TakeMoneyFromLocation(_location, componentInChildren.moneyCount);
			Messenger.Invoke(Globals.MsgLocationMoneyPileClicked);
			return;
		}
		LocationChest chest = obj as LocationChest;
		ProcessChestButton(chest);
		if (obj.name.StartsWith(_chestId))
		{
			chest = obj.transform.parent.parent.parent.parent.GetComponentInChildren<LocationChest>();
			ProcessChestButton(chest);
		}
		FightScreenMobIcon fightScreenMobIcon = obj as FightScreenMobIcon;
		if (fightScreenMobIcon != null)
		{
			_selectedBot = _enemies.IndexOf(fightScreenMobIcon);
			SetFightButtonActive(_selectedBot);
			return;
		}
		if (obj.name == "_close_button")
		{
			if (HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Location)
			{
				return;
			}
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.MainMap);
			Globals.GameScreen = Globals.GameScreenE.Map;
			LocationQuad.transform.localPosition = new Vector3(-Camera2D.ScreenWidth, Camera2D.ScreenHeight, LocationQuad.transform.localPosition.z);
			if (_isScarabActive)
			{
				HideTrail();
				if (_flySound != null)
				{
					_flySound.Stop();
				}
			}
			return;
		}
		if (obj.name == "global_nav_3")
		{
			LocationQuad.transform.localPosition = new Vector3(-Camera2D.ScreenWidth, Camera2D.ScreenHeight, LocationQuad.transform.localPosition.z);
		}
		Sprite component = obj.gameObject.GetComponent<Sprite>();
		if (component != null && component == _fightButton)
		{
			if (AreaData.Current == null)
			{
				Utils.Log("AAAA AreaData.Current == null");
			}
			if (AreaData.Current.Location == null)
			{
				Utils.Log("AAAA AreaData.Current.Location == null");
			}
			if (AreaData.Current.Location.Logic == null)
			{
				Utils.Log("AAAA AreaData.Current.Location.Logic == null");
			}
			if (AreaData.Current.Location.Logic._mobs == null)
			{
				Utils.Log("AAAA AreaData.Current.Location.Logic._mobs == null");
			}
			Utils.LogForce("AAAA ", AreaData.Current.Location, AreaData.Current.Location.Logic, _selectedBot);
			AreaData.Current.Location.Logic.MobInFight = _selectedBot;
			if (_location.Logic.ChestsOnLocation.Count > 0)
			{
				int elfModel = _location.Logic.ChestsOnLocation[0].Chest.ElfModel;
				if (AreaData.Current.Location.Logic._mobs[_selectedBot].Id == SingletonT<ServerData>.I.GameSettings.Elfs[elfModel].Bot.Id)
				{
					Globals.MainMenu.StartFightWith(AreaData.Current.Location.Logic._mobs[_selectedBot], _location.Logic.ChestsOnLocation[0].Chest.ElfLevelDifference);
				}
				else
				{
					Globals.MainMenu.StartFightWith(AreaData.Current.Location.Logic._mobs[_selectedBot]);
				}
			}
			else
			{
				Globals.MainMenu.StartFightWith(AreaData.Current.Location.Logic._mobs[_selectedBot]);
			}
			if (_isScarabActive)
			{
				HideTrail();
				if (_flySound != null)
				{
					_flySound.Stop();
				}
			}
			return;
		}
		if (obj.name == "find_chest")
		{
			if (GetElf() != null)
			{
				Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowElf, ServerData.PhrasesE.ElfPhrase2);
			}
			else if (SingletonT<ServerData>.I.PlayerParams.MoneyScarabCount > 0)
			{
				Globals.MainMenu.TryStartTutorial(Globals.TutorialFindChest);
				_isScarabActive = true;
				ButtonFindChest.SetActiveRecursivelyMk1(setActive: false);
				ButtonDig.SetActiveRecursivelyMk1(setActive: true);
				ButtonCancelDig.SetActiveRecursivelyMk1(setActive: true);
				StartFind();
			}
			else
			{
				Messenger<ServerData.PhrasesE, ServerData.PhrasesE, ServerData.PhrasesE, Action>.Invoke(Globals.MsgPopup2ButtonYesHandler, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, ServerData.PhrasesE.ScarabInsufficient, delegate
				{
					Messenger.Invoke(Globals.MsgShopScarabInsufficient);
					HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
				});
			}
		}
		if (obj.name == "dig_chest")
		{
			_isScarabActive = false;
			ButtonDig.SetActiveRecursivelyMk1(setActive: false);
			ButtonCancelDig.SetActiveRecursivelyMk1(setActive: false);
			ButtonFindChest.SetActiveRecursivelyMk1(setActive: true);
			Dig(isDigging: true);
		}
		if (obj.name == "cancel_dig_chest")
		{
			_isScarabActive = false;
			ButtonDig.SetActiveRecursivelyMk1(setActive: false);
			ButtonCancelDig.SetActiveRecursivelyMk1(setActive: false);
			ButtonFindChest.SetActiveRecursivelyMk1(setActive: true);
			Dig(isDigging: false);
		}
	}

	private void ProcessChestButton(LocationChest chest)
	{
		if (!(chest != null))
		{
			return;
		}
		if (Debug.isDebugBuild)
		{
			Debug.Log("LOCATION_HID: ChestClick - chest.ElfState=" + chest.ChestonLocation.ElfState);
		}
		if (GetElf() == null)
		{
			switch (chest.ChestonLocation.ElfState)
			{
			case LocationLogic.ChestOnLocation.ElfStateE.NotGeneratedYet:
				if (Debug.isDebugBuild)
				{
					Debug.LogWarning("Need to generate elf");
				}
				break;
			case LocationLogic.ChestOnLocation.ElfStateE.GeneratedFalse:
				Globals.MainMenu.GoToChestGame(_location, chest.ChestonLocation);
				break;
			case LocationLogic.ChestOnLocation.ElfStateE.GeneratedTrue:
				Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowElf, ServerData.PhrasesE.ElfPhrase1);
				break;
			case LocationLogic.ChestOnLocation.ElfStateE.Killed:
				Globals.MainMenu.GoToChestGame(_location, chest.ChestonLocation);
				break;
			}
		}
		else
		{
			Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowElf, ServerData.PhrasesE.ElfPhrase1);
		}
	}

	private void StartFind()
	{
		if (_flySound != null)
		{
			_flySound.clip = SingletonT<SoundManager>.I.GetSound(null, "bug_fly2");
			_flySound.Play();
		}
		ScarabRoot.transform.localPosition = ScarabRoot.transform.parent.InverseTransformPoint(ScarabCountIcon.transform.position);
		GameObject gameObject = _cursors[_currentCursor];
		gameObject.SetActiveRecursivelyMk1(setActive: true);
		ScarabTrail.SetActiveRecursivelyMk1(setActive: true);
		_scarabSearchEffect.SetActiveRecursivelyMk1(setActive: true);
		_cursorPos = new Vector3((float)(Camera2D.ScreenWidth / 2) - LocationQuad.transform.localPosition.x - (float)Camera2D.ScreenWidth, (float)(Camera2D.ScreenHeight / 2 - Camera2D.ScreenHeight) - LocationQuad.transform.localPosition.y + (float)Camera2D.ScreenHeight, -580f);
	}

	private void Dig(bool isDigging)
	{
		_isScarabActive = false;
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ScarabHideEffectPrefab);
		gameObject.transform.parent = ScarabRoot.transform;
		gameObject.transform.localPosition = default(Vector3);
		if (isDigging)
		{
			Metrics.OnScarabUsed();
			Vector3 cursorPos = ScarabRoot.transform.localPosition;
			int num = FindChest(ref cursorPos);
			if (num != -1)
			{
				LocationChest locationChest = _chests[num];
				locationChest.ChestonLocation.WasFound = true;
				locationChest.gameObject.SetActiveRecursivelyMk1(setActive: true);
				GameObject gameObject2 = _animatedChestButtons[locationChest];
				gameObject2.SetActiveRecursivelyMk1(setActive: true);
				InitChestMapButton(gameObject2);
				StartCoroutine(SpawnObject(locationChest.gameObject, 0.5f));
				SwapCursors();
				Messenger<LocationLogic>.Invoke(Globals.Msg_ChestOnLocationWasFound, _location.Logic);
				int num2 = 0;
				int num3 = 0;
				foreach (LocationChest chest in _chests)
				{
					if (chest.ChestonLocation != null && chest.ChestonLocation.WasFound)
					{
						num2++;
					}
				}
				foreach (KeyValuePair<int, ServerData.Location> location in SingletonT<ServerData>.I._locations)
				{
					foreach (LocationLogic.ChestOnLocation item in location.Value.Logic.ChestsOnLocation)
					{
						if (item.WasFound)
						{
							num3++;
						}
					}
				}
				if (num2 > 0)
				{
					Messenger.Invoke(Globals.Msg_LocationChestsWasFoundChanged, num2, num3);
				}
				UpdateChestCount();
				GenerateElf(locationChest.ChestonLocation);
				Globals.MainMenu.SaveGame();
			}
		}
		GameObject[] cursors = _cursors;
		foreach (GameObject gameObject3 in cursors)
		{
			gameObject3.SetActiveRecursivelyMk1(setActive: false);
		}
		HideTrail();
		_flySound.Stop();
	}

	private void GenerateElf(LocationLogic.ChestOnLocation chestOnLocation)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("LOCATION_HUD GenerateElf");
		}
		if (chestOnLocation.ElfState == LocationLogic.ChestOnLocation.ElfStateE.NotGeneratedYet)
		{
			if (UnityEngine.Random.Range(0, 100) < chestOnLocation.Chest.ElfProbability || Globals.DebugAlwaysGenerateElf)
			{
				chestOnLocation.ElfState = LocationLogic.ChestOnLocation.ElfStateE.GeneratedTrue;
			}
			else
			{
				chestOnLocation.ElfState = LocationLogic.ChestOnLocation.ElfStateE.GeneratedFalse;
			}
		}
		if (chestOnLocation.ElfState == LocationLogic.ChestOnLocation.ElfStateE.GeneratedTrue && GetElf() == null && _location != null)
		{
			if (_location.Logic._mobs.Count >= 3)
			{
				_location.Logic._mobs[0] = SingletonT<ServerData>.I.GameSettings.Elfs[chestOnLocation.Chest.ElfModel].Bot;
			}
			else
			{
				_location.Logic._mobs.Insert(0, SingletonT<ServerData>.I.GameSettings.Elfs[chestOnLocation.Chest.ElfModel].Bot);
			}
			Messenger<ServerData.Location, int>.Invoke(Globals.MsgLocationMobsAdded, _location, 0);
		}
	}

	private ServerData.BotInfo GetElf()
	{
		if (_location != null)
		{
			return _location.Logic.GetElf();
		}
		return null;
	}

	private void StartTakeMoneyAnimation(SpriteButton button, string count)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(TakemoneyAnim);
		gameObject.transform.position = new Vector3(100 * UniqueId, 0f, 0f);
		Camera componentInChildren = gameObject.GetComponentInChildren<Camera>();
		Vector3 vector = HudMk1.Instance.camera.WorldToScreenPoint(button.transform.position);
		float num = 100f / (float)Camera2D.ScreenHeight;
		float num2 = num * componentInChildren.aspect;
		componentInChildren.rect = new Rect(vector.x / (float)Screen.width - num2 / 2f, vector.y / (float)Screen.height - num / 2f, num2, num);
		float num3 = float.MinValue;
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera camera in allCameras)
		{
			if (GetComponent<Camera>()depth > num3)
			{
				num3 = GetComponent<Camera>()depth;
			}
		}
		componentInChildren.depth = num3 + 1f;
		Animation componentInChildren2 = gameObject.GetComponentInChildren<Animation>();
		StartCoroutine(KillMoneyAnimation(gameObject, vector, componentInChildren2.clip.length, count));
	}

	private IEnumerator KillMoneyAnimation(GameObject anim, Vector2 textPos, float animationLength, string count)
	{
		GameObject textGO = new GameObject();
		SpriteText text = textGO.AddComponent<SpriteText>();
		text.Anchor_ = TextAnchor.MiddleCenter;
		text.Bold_ = true;
		text.PixSize_ = 26;
		text.Text_ = "+" + count;
		text.transform.parent = LocationQuad.transform;
		text.NamedColorE_ = FontManager.ColorE.BagMoney;
		textPos.x *= (float)Camera2D.ScreenWidth / (float)Screen.width;
		textPos.y *= (float)Camera2D.ScreenHeight / (float)Screen.height;
		text.transform.localPosition = new Vector3((float)(int)textPos.x - LocationQuad.transform.localPosition.x - (float)Camera2D.ScreenWidth, (float)((int)textPos.y - Camera2D.ScreenHeight) - LocationQuad.transform.localPosition.y + (float)Camera2D.ScreenHeight + 30f, -550f);
		text.transform.localScale = Vector3.one * 2f;
		textGO.layer = base.gameObject.layer;
		float time = 0f;
		float alphaCurveLength = AlphaCurve.keys[AlphaCurve.keys.Length - 1].time;
		while (time < animationLength)
		{
			textGO.transform.localPosition = new Vector3(textGO.transform.localPosition.x, textGO.transform.localPosition.y + 100f * Time.deltaTime, textGO.transform.localPosition.z);
			text.TextAlpha_ = AlphaCurve.Evaluate(time / animationLength * alphaCurveLength);
			time += Time.deltaTime;
			yield return null;
		}
		UnityEngine.Object.Destroy(anim);
		UnityEngine.Object.Destroy(textGO);
	}

	private void RemoveMoneyPileFromLocation(LocationMoney pile)
	{
		int num = Mathf.Abs(pile.gameObject.GetInstanceID());
		foreach (KeyValuePair<Rect, ChestZone> moneyZone in _moneyZones)
		{
			if (moneyZone.Value.MoneId == num)
			{
				_moneyZones[moneyZone.Key].MoneId = -1;
				break;
			}
		}
		_moneyPiles.Remove(pile);
		pile.UnregisterMe();
		pile.Eliminate();
		GameObject gameObject = _animatedMoneyButtons[pile];
		MapButton componentInChildren = gameObject.GetComponentInChildren<MapButton>();
		componentInChildren.UnregisterMe();
		componentInChildren.Eliminate();
		_animatedMoneyButtons.Remove(pile);
	}

	private void MoneyChanged(ServerData.Location location, int wasMoney, int money)
	{
		if (_location == null || _location.Id != location.Id)
		{
			return;
		}
		if (Globals.IsDebugBuild)
		{
			Debug.Log("LOCATION_HUD: MoneyChanged - " + money);
		}
		if (money > 0 && _location != null && location.Id == _location.Id)
		{
			if (location.MoneyMax <= 0)
			{
				Utils.Log("location.MoneyMax <= 0");
			}
			else
			{
				PlaceMoneyOnLocation(money);
			}
		}
	}

	private void PopulationChanged(ServerData.Location location, int count, bool grow)
	{
		if (_location != null && _location.Id == location.Id)
		{
			if (count == location.PopulationMax && location.Logic._mobs.Count == 0)
			{
				Arrow.gameObject.SetActiveRecursivelyMk1(setActive: false);
				Messages.Text_ = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.LocationPopulationMax);
			}
			else
			{
				Arrow.gameObject.SetActiveRecursivelyMk1(setActive: true);
			}
			PopulationCount.Text_ = count.ToString();
			if (Globals.IsDebugBuild)
			{
				Debug.Log("LOCATION_HUD: PopulationChanged - " + count);
			}
		}
	}

	private void PlaceChestsOnLocation(LocationLogic locationLogic)
	{
		if (_location == null || _location.Id != locationLogic.MyLocation.Id)
		{
			return;
		}
		if (Globals.IsDebugBuild)
		{
			Debug.Log("LOCATION_HUD: PlaceChestsOnLocation - chests count: " + locationLogic.ChestsOnLocation.Count);
		}
		foreach (LocationLogic.ChestOnLocation item in locationLogic.ChestsOnLocation)
		{
			GameObject gameObject = ChestProtos[int.Parse(item.Chest.Index) - 1];
			int x = item.X;
			int y = item.Y;
			if (x < 0 || y < 0)
			{
				float cellW = gameObject.GetComponent<LocationChest>().cellW;
				float cellH = gameObject.GetComponent<LocationChest>().cellH;
				Rect randomFreeChestZone = GetRandomFreeChestZone();
				if (randomFreeChestZone == default(Rect))
				{
					if (Globals.IsDebugBuild)
					{
						Debug.Log("[Fuckup! -- Cannot place more chests.]");
					}
					if (Globals.IsDebugBuild)
					{
						Debug.Log(_location.Logic.ChestsOnLocation.Count + " " + _chestZones.Count);
					}
				}
				item.RectOnLocation = randomFreeChestZone;
				_chestZones[randomFreeChestZone].Chest = item;
				x = (int)randomFreeChestZone.x;
				y = (int)randomFreeChestZone.y;
				item.X = x;
				item.Y = y;
			}
			MessWithChestsOnLocationButtons(item);
			if (item.WasFound && (item.ElfState == LocationLogic.ChestOnLocation.ElfStateE.NotGeneratedYet || item.ElfState == LocationLogic.ChestOnLocation.ElfStateE.GeneratedTrue))
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(string.Concat("GenerateElf elfstate=", item.ElfState, " wasfound=", item.WasFound));
				}
				GenerateElf(item);
			}
		}
		UpdateChestCount();
	}

	private void UpdateChestCount()
	{
		int num = 0;
		foreach (LocationLogic.ChestOnLocation item in _location.Logic.ChestsOnLocation)
		{
			if (!item.WasFound)
			{
				num++;
			}
		}
		ChestCount.Text_ = num.ToString();
	}

	private Rect GetRandomFreeChestZone()
	{
		List<Rect> list = new List<Rect>();
		foreach (KeyValuePair<Rect, ChestZone> chestZone in _chestZones)
		{
			if (chestZone.Value.Chest == null)
			{
				list.Add(chestZone.Key);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return default(Rect);
	}

	private void RemoveChestFromLocation(LocationLogic locationLogic, LocationLogic.ChestOnLocation chest)
	{
		if (_location == null || locationLogic.MyLocation.Id != _location.Id)
		{
			return;
		}
		if (Globals.IsDebugBuild)
		{
			Debug.Log("LOCATION_HUD: RemoveChestFromLocation - chest: " + chest);
		}
		KeyValuePair<Rect, ChestZone> item;
		foreach (KeyValuePair<Rect, ChestZone> chestZone in _chestZones)
		{
			item = chestZone;
			if (item.Value.Chest == chest)
			{
				LocationChest locationChest = _chests.Find((LocationChest ch) => item.Value.Chest.InstanceId == ch.ChestonLocation.InstanceId);
				if (locationChest != null)
				{
					GameObject gameObject = _animatedChestButtons[locationChest];
					_animatedChestButtons.Remove(locationChest);
					MapButton componentInChildren = gameObject.GetComponentInChildren<MapButton>();
					componentInChildren.UnregisterMe();
					UnityEngine.Object.Destroy(gameObject);
					locationChest.UnregisterMe();
					locationChest.Eliminate();
					item.Value.Chest.X = -1;
					item.Value.Chest.Y = -1;
					item.Value.Chest.RectOnLocation = default(Rect);
					item.Value.Chest = null;
				}
			}
		}
		UpdateChestCount();
	}

	private void LocationCleanup(LocationLogic locationLogic)
	{
		CleanupChests();
		CleanupMoneyPiles();
		_chestZones.Clear();
		_moneyZones.Clear();
		_selectedBot = -1;
		SetFightButtonInactive();
	}

	private void CleanupChests()
	{
		foreach (LocationChest chest in _chests)
		{
			if (chest != null)
			{
				GameObject gameObject = _animatedChestButtons[chest];
				gameObject.SetActiveRecursivelyMk1(setActive: true);
				MapButton componentInChildren = gameObject.GetComponentInChildren<MapButton>();
				componentInChildren.UnregisterMe();
				UnityEngine.Object.Destroy(gameObject);
				chest.UnregisterMe();
				chest.Eliminate();
			}
		}
		_animatedChestButtons.Clear();
		_chests.Clear();
	}

	private void CleanupMoneyPiles()
	{
		foreach (LocationMoney moneyPile in _moneyPiles)
		{
			GameObject gameObject = _animatedMoneyButtons[moneyPile];
			MapButton componentInChildren = gameObject.GetComponentInChildren<MapButton>();
			componentInChildren.UnregisterMe();
			componentInChildren.Eliminate();
			moneyPile.UnregisterMe();
			moneyPile.Eliminate();
		}
		_moneyPiles.Clear();
		_animatedMoneyButtons.Clear();
	}

	private void RefreshMobs(ServerData.Location location)
	{
		if (_location == null || location.Id != _location.Id)
		{
			return;
		}
		Utils.LogForce("RefreshMobs", location, _location);
		if (Globals.IsDebugBuild)
		{
			Debug.Log("LOCATION_HUD: RefreshMobs - Mobs count: " + location.Logic._mobs.Count);
		}
		if (_selectedBot > -1 && (location.Logic._mobs.Count == 0 || _selectedBot >= location.Logic._mobs.Count))
		{
			_selectedBot = -1;
			SetFightButtonInactive();
		}
		for (int i = 0; i < _enemies.Length; i++)
		{
			FightScreenMobIcon fightScreenMobIcon = _enemies[i];
			fightScreenMobIcon.SetState(FightScreenMobIcon.State.OutOfGui);
		}
		if (location.Logic._mobs.Count > 0)
		{
			if (_selectedBot < 0)
			{
				_selectedBot = 0;
			}
			Arrow.SpriteName_ = "less";
			Messages.Phrase_ = ServerData.PhrasesE.Custom;
			Messages.NamedColorE_ = FontManager.ColorE.CompareRed;
			Messages.Text_ = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.LocationPopulationAtacked);
			Messenger.Invoke(Globals.MsgLocationMobAttack);
		}
		else
		{
			Arrow.SpriteName_ = "more";
			Messages.Phrase_ = ServerData.PhrasesE.Custom;
			Messages.NamedColorE_ = FontManager.ColorE.ItemGreen;
			if (location.Logic.Population == location.PopulationMax && location.Logic._mobs.Count == 0)
			{
				Arrow.gameObject.SetActiveRecursivelyMk1(setActive: false);
				Messages.Text_ = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.LocationPopulationMax);
			}
			else
			{
				Arrow.gameObject.SetActiveRecursivelyMk1(setActive: true);
				Messages.Text_ = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.LocationPopulationPeace);
			}
		}
		int num = Mathf.Min(_enemies.Length, location.Logic._mobs.Count);
		for (int j = 0; j < num; j++)
		{
			FightScreenMobIcon fightScreenMobIcon2 = _enemies[j];
			ServerData.BotInfo botInfo = location.Logic._mobs[j];
			fightScreenMobIcon2.SetIcon(Path.GetFileNameWithoutExtension(botInfo.Picture));
			fightScreenMobIcon2.SetActive();
			fightScreenMobIcon2.SetState(FightScreenMobIcon.State.Inactive);
		}
		if (_selectedBot > -1)
		{
			SetFightButtonActive(_selectedBot);
		}
	}

	private void PlaceMoneyOnLocation(int newTotal)
	{
		float value = (float)newTotal / (float)_location.MoneyMax;
		value = Mathf.Clamp01(value);
		int num = 0;
		foreach (LocationMoney moneyPile in _moneyPiles)
		{
			num += moneyPile.moneyCount;
		}
		int num2 = newTotal - num;
		if (_moneyPiles.Count < 4)
		{
			if (num2 <= 4 - _moneyPiles.Count)
			{
				AddLocationMoneyButtons(num2);
			}
			else
			{
				AddLocationMoneyButtons(4 - _moneyPiles.Count);
				if (_moneyPiles.Count < _moneyZones.Count)
				{
					float num3 = (float)_moneyPiles.Count / (float)_moneyZones.Count;
					if (num3 < value)
					{
						int num4 = Mathf.RoundToInt((float)_moneyZones.Count * value);
						int num5 = num4 - _moneyPiles.Count;
						if (num5 > 0)
						{
							AddLocationMoneyButtons(num5);
						}
					}
				}
			}
		}
		else if (_moneyPiles.Count < _moneyZones.Count)
		{
			float num6 = (float)_moneyPiles.Count / (float)_moneyZones.Count;
			if (num6 < value)
			{
				int num7 = Mathf.RoundToInt((float)_moneyZones.Count * value);
				int num8 = num7 - _moneyPiles.Count;
				if (num8 > 0)
				{
					AddLocationMoneyButtons(num8);
				}
			}
		}
		if (Globals.IsDebugBuild)
		{
			Debug.Log($"NewTotal: {newTotal} ToDistribute: {num2} _moneyPiles.Count: {_moneyPiles.Count}");
		}
		int num9 = 0;
		foreach (LocationMoney moneyPile2 in _moneyPiles)
		{
			moneyPile2.moneyCount = 0;
		}
		for (int num10 = newTotal; num10 > 0; num10--)
		{
			LocationMoney locationMoney = _moneyPiles[num9];
			locationMoney.moneyCount++;
			num9++;
			if (num9 >= _moneyPiles.Count)
			{
				num9 = 0;
			}
		}
		foreach (LocationMoney moneyPile3 in _moneyPiles)
		{
			moneyPile3.SetMoneyCount(moneyPile3.moneyCount);
		}
	}

	private void InitChestMapButton(GameObject animatedButton)
	{
		MapButton componentInChildren = animatedButton.GetComponentInChildren<MapButton>();
		if (componentInChildren != null)
		{
			componentInChildren.name = _chestId + UniqueId;
			if (componentInChildren.animation != null)
			{
				componentInChildren.animation["Take 001"].time = componentInChildren.animation.clip.length * UnityEngine.Random.value;
			}
			else if (componentInChildren.CustomAnimation != null)
			{
				componentInChildren.CustomAnimation["Take 001"].time = componentInChildren.CustomAnimation.clip.length * UnityEngine.Random.value;
			}
			componentInChildren.Init();
			componentInChildren.SetActive();
		}
	}

	private void MessWithChestsOnLocationButtons(LocationLogic.ChestOnLocation chestOnLocation)
	{
		ServerData.Chest chest = chestOnLocation.Chest;
		GameObject original = ChestProtos[int.Parse(chest.Index) - 1];
		int x = chestOnLocation.X;
		int y = chestOnLocation.Y;
		if (!_chests.Exists((LocationChest ch) => ch.ChestonLocation.InstanceId == chestOnLocation.InstanceId))
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(original);
			gameObject.name = "chest_" + chestOnLocation.InstanceId;
			LocationChest component = gameObject.transform.GetComponent<LocationChest>();
			component.ChestonLocation = chestOnLocation;
			gameObject.transform.parent = LocationQuad.transform;
			gameObject.transform.SetLayerRecursively(base.gameObject.transform);
			component.Init();
			component.SetActive();
			component.gameObject.SetActiveRecursivelyMk1(chestOnLocation.WasFound);
			if (Globals.IsDebugBuild)
			{
				Debug.Log($"[CHEST BUTTON INITED]@[x:{x} y:{y}] -- {component.name}");
			}
			_chests.Add(component);
			Vector3 vector = new Vector3(x + ChestZoneWidth / 2, y);
			gameObject.transform.localPosition = new Vector3(vector.x, 0f - vector.y, -50f);
			if (AnimatedChestButtonPrefab != null)
			{
				GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(AnimatedChestButtonPrefab);
				_animatedChestButtons.Add(component, gameObject2);
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.SetLayerRecursively(base.gameObject.transform);
				gameObject2.transform.localPosition = new Vector3(0f, 0f, -100f);
				InitChestMapButton(gameObject2);
				gameObject2.SetActiveRecursivelyMk1(chestOnLocation.WasFound);
			}
		}
	}

	private void AddLocationMoneyButtons(int maxNumberOfPiles)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		int num = maxNumberOfPiles - _moneyPiles.Count;
		if (num <= 0)
		{
			return;
		}
		GameObject gameObject = ChestProtos[5];
		if (!(gameObject != null))
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			Rect randomFreeMoneyZone = GetRandomFreeMoneyZone();
			float x = randomFreeMoneyZone.x;
			float y = randomFreeMoneyZone.y;
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(gameObject);
			_moneyZones[randomFreeMoneyZone].MoneId = Mathf.Abs(gameObject2.GetInstanceID());
			gameObject2.name = "money_" + UniqueId;
			gameObject2.transform.parent = LocationQuad.transform;
			gameObject2.transform.SetLayerRecursively(base.gameObject.transform);
			LocationMoney component = gameObject2.GetComponent<LocationMoney>();
			component.Init();
			component.SetActive();
			if (Globals.IsDebugBuild)
			{
				Debug.Log($"[MONEY BUTTON INITED]@[x:{x} y:{y}] -- {component.name}");
			}
			_moneyPiles.Add(component);
			Vector3 vector = new Vector3(x + (float)(MoneyZoneWidth / 2), y);
			gameObject2.transform.localPosition = new Vector3(vector.x, 0f - vector.y, -50f);
			if (AnimatedMoneyButtonPrefab != null)
			{
				GameObject gameObject3 = (GameObject)UnityEngine.Object.Instantiate(AnimatedMoneyButtonPrefab);
				_animatedMoneyButtons.Add(component, gameObject3);
				gameObject3.transform.parent = gameObject2.transform;
				gameObject3.transform.SetLayerRecursively(base.gameObject.transform);
				gameObject3.transform.localPosition = new Vector3(0f, 0f, -50f);
				MapButton componentInChildren = gameObject3.GetComponentInChildren<MapButton>();
				componentInChildren.name = _moneyId + UniqueId;
				if (componentInChildren.animation != null)
				{
					componentInChildren.animation["Take 001"].time = componentInChildren.animation.clip.length * UnityEngine.Random.value;
				}
				else if (componentInChildren.CustomAnimation != null)
				{
					componentInChildren.CustomAnimation["Take 001"].time = componentInChildren.CustomAnimation.clip.length * UnityEngine.Random.value;
				}
				componentInChildren.Init();
				componentInChildren.SetActive();
			}
			StartCoroutine(SpawnObject(gameObject2, 0.5f));
		}
		if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Location)
		{
			Messenger.Invoke(Globals.MsgLocationMoneyPilesAdded);
		}
	}

	private Rect GetRandomFreeMoneyZone()
	{
		List<Rect> list = new List<Rect>();
		foreach (KeyValuePair<Rect, ChestZone> moneyZone in _moneyZones)
		{
			if (moneyZone.Value.MoneId < 0)
			{
				list.Add(moneyZone.Key);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return default(Rect);
	}

	private IEnumerator SpawnObject(GameObject go, float time)
	{
		Vector3 zero = Vector3.zero;
		Vector3 one = Vector3.one;
		go.transform.localScale = zero;
		float currentTime = 0f;
		while (go != null && currentTime < time)
		{
			go.transform.localScale = Vector3.Lerp(zero, one, currentTime / time);
			currentTime += Time.deltaTime;
			yield return null;
		}
		if (go != null)
		{
			go.transform.localScale = one;
		}
	}

	private void SetFightButtonActive(int enemyIndex)
	{
		Vector3 vector = new Vector3((float)_fightButton.Width / 2f, (float)(-_fightButton.Height) / 2f);
		FightScreenMobIcon fightScreenMobIcon = _enemies[enemyIndex];
		float x = fightScreenMobIcon.transform.localPosition.x;
		_fightButton.transform.localPosition = new Vector3(x, -85f, 0f) + vector;
		_fightButton.gameObject.SetActiveRecursivelyMk1(setActive: true);
		for (int i = 0; i < _location.Logic._mobs.Count; i++)
		{
			if (i == enemyIndex)
			{
				_enemies[i].SetState(FightScreenMobIcon.State.Active);
			}
			else
			{
				_enemies[i].SetState(FightScreenMobIcon.State.Inactive);
			}
		}
	}

	private void SetFightButtonInactive()
	{
		_fightButton.gameObject.SetActiveRecursivelyMk1(setActive: false);
	}

	private void HideTrail()
	{
		ParticleEmitter[] componentsInChildren = ScarabTrail.GetComponentsInChildren<ParticleEmitter>();
		ParticleEmitter[] array = componentsInChildren;
		foreach (ParticleEmitter particleEmitter in array)
		{
			particleEmitter.ClearParticles();
		}
		ScarabTrail.SetActiveRecursivelyMk1(setActive: false);
		_scarabSearchEffect.SetActiveRecursivelyMk1(setActive: false);
	}

	private IEnumerator ShowHighlight(GameObject highlight)
	{
		float time = 0f;
		Vector3 scale = Vector3.one * 100f;
		while (time < 0.5f)
		{
			highlight.transform.localScale = Vector3.Lerp(Vector3.zero, scale, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		highlight.transform.localScale = scale;
		yield return new WaitForSeconds(2f);
		time = 0f;
		while (time < 0.5f)
		{
			highlight.transform.localScale = Vector3.Lerp(scale, Vector3.zero, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		highlight.transform.localScale = Vector3.zero;
		UnityEngine.Object.Destroy(highlight);
	}
}
