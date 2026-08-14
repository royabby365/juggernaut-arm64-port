using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class Match3Hud : MonoBehaviour
{
	private enum StateE
	{
		WaitUserInput,
		RemovingBlockChain,
		AddingNewBlocks,
		LootGrabbing
	}

	public enum DifficultyE
	{
		Easy,
		Normal
	}

	private const int FIELD_WIDTH = 7;

	private const int FIELD_HEIGHT = 7;

	private const int CELL_WIDTH = 91;

	private const int CELL_HEIGHT = 91;

	private const float BLOCKS_REMOVING_TIME = 0.2f;

	private const float BLOCKS_ADDING_TIME = 0.2f;

	private const float LOOT_GRABBING_TIME = 0.2f;

	private const int MAX_LOOT_COUNT = 3;

	private const float LOOT_FX_TIME = 1.4f;

	private const float BLOCKER_TRANSITION_TIME = 0.4f;

	private const int NORMAL_MULTIPLIER = 10;

	public const string RECORD_KEY = "Match3Record";

	public const float TICK_TIME = 5f;

	public const float TICK_SOUND_PERIOD = 1f;

	private CompositeDisposable _listeners;

	private Stack<Match3Block> _cachedBlocks = new Stack<Match3Block>();

	private Stack<Match3Arrow> _cachedArrows = new Stack<Match3Arrow>();

	private Match3Block[] _matrix = new Match3Block[49];

	private List<Match3Block> _currentBlockChain = new List<Match3Block>();

	private Stack<Match3Arrow> _currentArrowChain = new Stack<Match3Arrow>();

	private List<Match3Block> _fallingBlocks = new List<Match3Block>();

	private StateE _state;

	private float _removingTimer;

	private float _addingTimer;

	private float _lootTimer;

	private List<Match3Block> _loot = new List<Match3Block>(3);

	private int _lootColumn;

	private string[] _fallingSounds = new string[3] { "block_falling_01", "block_falling_02", "block_falling_03" };

	private string[] _destroyingSounds = new string[10] { "block_destroying_1_3", "block_destroying_1_3", "block_destroying_1_3", "block_destroying_1_3", "block_destroying_4", "block_destroying_5_6", "block_destroying_5_6", "block_destroying_7_8", "block_destroying_7_8", "block_destroying_9" };

	private float _mainTimer;

	private int _mainTimerMax;

	private System.Tuple<ServerData.MoneyType.TypeE, int, string> _keysCost;

	private float _tickTimer;

	private float _gameTime;

	private int _lockpicksUsed;

	private int _countGold;

	private int _countCrystall;

	private int _countSkull;

	private int _countStar;

	private int _countScarab;

	private int _countPoints;

	private bool _isBlockedByTimer;

	private List<System.Tuple<int, int, float>> _points = new List<System.Tuple<int, int, float>>
	{
		new System.Tuple<int, int, float>(4, 1, 1f),
		new System.Tuple<int, int, float>(5, 2, 1.2f),
		new System.Tuple<int, int, float>(6, 4, 1.5f),
		new System.Tuple<int, int, float>(7, 7, 1.7f),
		new System.Tuple<int, int, float>(8, 11, 1.9f),
		new System.Tuple<int, int, float>(9, 16, 2.1f),
		new System.Tuple<int, int, float>(10, 25, 2.3f)
	};

	private Transform _closeButton;

	private float _closeButtonDepth;

	private ServerData.Mine _mine;

	public GameObject BlockPrefab;

	public GameObject ArrowPrefab;

	public GameObject[] BlockDestroyingFxPrefabs;

	public SpriteText CountGold;

	public SpriteText CountCrystall;

	public SpriteText CountSkull;

	public SpriteText CountStar;

	public SpriteText CountScarab;

	public SpriteText CountKeys;

	public SpriteText CountTime;

	public Match3ProgressBar ProgressBar;

	public SpriteText HelpText;

	public Sprite IconGold;

	public Sprite IconCrystall;

	public Sprite IconSkull;

	public Sprite IconStar;

	public Sprite IconScarab;

	public GameObject TrailPrefab;

	public Transform Blocker;

	public SpriteText PointsCounter;

	public SpriteText RecordCounter;

	public AnimationCurve TextAnimationCurve;

	public Transform GamefieldRoot;

	public SpriteText ResultsPointsCounter;

	public SpriteText ResultsRecordCounter;

	public SpriteText UseKeysButtonText;

	private Match3Block this[int index]
	{
		get
		{
			Match3Block match3Block = _matrix[index];
			if (match3Block != null)
			{
				match3Block.X = index % 7;
				match3Block.Y = index / 7;
			}
			return match3Block;
		}
		set
		{
			if (value != null)
			{
				value.X = index % 7;
				value.Y = index / 7;
			}
			_matrix[index] = value;
		}
	}

	public int Length => _matrix.Length;

	public DifficultyE Difficulty => _mine.Difficulty;

	internal ServerData.Mine Mine
	{
		get
		{
			return _mine;
		}
		set
		{
			_mine = value;
			_keysCost = _mine.ContinuePrice.GetPrice();
		}
	}

	public void Init(ServerData.Mine mine)
	{
		Mine = mine;
		SetupPicksCount();
	}

	private void OnEnable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.MoveBegin += Instance_MoveBegin;
			HudMk1.Instance.Move += Instance_Move;
			HudMk1.Instance.MoveEnd += Instance_MoveEnd;
			HudMk1.Instance.Release += Instance_Release;
			HudMk1.Instance.MoveTouch += Instance_MoveTouch;
		}
		_listeners = new CompositeDisposable();
		_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
		_listeners.Add(Messenger<ServerData.MoneyType.TypeE, string>.AddListener(Globals.MsgPlayerFundsChanged, LockPicksCount));
	}

	private void OnDisable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.MoveBegin -= Instance_MoveBegin;
			HudMk1.Instance.Move -= Instance_Move;
			HudMk1.Instance.MoveEnd -= Instance_MoveEnd;
			HudMk1.Instance.Release -= Instance_Release;
			HudMk1.Instance.MoveTouch -= Instance_MoveTouch;
		}
		_listeners.Dispose();
	}

	private void LockPicksCount(ServerData.MoneyType.TypeE typeE, string reason)
	{
		if (typeE == ServerData.MoneyType.TypeE.Key)
		{
			SetupPicksCount();
		}
	}

	private void SetupPicksCount()
	{
		ServerData.MoneyType.TypeE type = ServerData.MoneyType.TypeE.Key;
		string text_ = type.GetPlayerFundsCount().ToString();
		CountKeys.Text_ = text_;
	}

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			_closeButton = HudMk1.Instance.transform.FindChildByName("close_button");
			_closeButtonDepth = _closeButton.localPosition.z;
		}
	}

	private void Instance_Release(SpriteButton obj)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Match3)
		{
			return;
		}
		if (obj.name == "button_match3_use_keys")
		{
			if (SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount >= _keysCost.Item2)
			{
				_keysCost.Item1.ChangePlayerFundsCount(-_keysCost.Item2);
				Globals.MainMenu.SaveGame();
				_lockpicksUsed += _keysCost.Item2;
				_mainTimer = _mainTimerMax;
				if (_isBlockedByTimer)
				{
					_isBlockedByTimer = false;
					StartCoroutine(ShowBlock(showOrHide: false));
				}
			}
			else
			{
				Messenger<ServerData.HintCodesE>.Invoke(Globals.MsgShowHint, ServerData.HintCodesE.vzlom);
			}
		}
		else if (obj.name == "_close_button")
		{
			Match3ReportToMR();
		}
		else if (obj.name == "button_match3_restart")
		{
			Match3ReportToMR();
		}
		else if (_currentBlockChain.Count == 1)
		{
			FinishChain();
		}
	}

	private void Match3ReportToMR()
	{
		float arg = Time.realtimeSinceStartup - _gameTime;
		int lockpicksUsed = _lockpicksUsed;
		Messenger<int, float, int>.Invoke(Globals.MsgMineGameStats, _mine.Id, arg, lockpicksUsed);
	}

	private void Update()
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Match3)
		{
			switch (_state)
			{
			case StateE.WaitUserInput:
				UpdateUserInput();
				break;
			case StateE.RemovingBlockChain:
				UpdateRemovingBlocks();
				break;
			case StateE.AddingNewBlocks:
				UpdateAddingNewBlocks();
				break;
			case StateE.LootGrabbing:
				UpdateLootGrabbing();
				break;
			}
		}
	}

	private void UpdateUserInput()
	{
		_mainTimer -= Time.deltaTime;
		ProgressBar.SetIndicator(_mainTimer.RoundToInt(), _mainTimerMax);
		if (_mainTimer < 5f && _mainTimer >= 0f)
		{
			if (_tickTimer <= 0f)
			{
				SingletonT<SoundManager>.I.PlayGlobalSound("tick8");
				_tickTimer = 1f;
			}
			_tickTimer -= Time.deltaTime;
		}
		if (_mainTimer < 0f)
		{
			if (_currentBlockChain.Count > 0)
			{
				FinishChain();
			}
			if (!_isBlockedByTimer)
			{
				_isBlockedByTimer = true;
				StartCoroutine(ShowBlock(showOrHide: true));
			}
		}
	}

	private IEnumerator ShowBlock(bool showOrHide)
	{
		if (showOrHide)
		{
			SingletonT<SoundManager>.I.PlayGlobalSound("mine_result");
		}
		float time = 0f;
		Vector3 source;
		Vector3 dest;
		if (showOrHide)
		{
			source = new Vector3(GetBlockerHidePos(), 0f, Blocker.localPosition.z);
			dest = new Vector3(GetBlockerShowPos(), 0f, Blocker.localPosition.z);
			int record = PlayerPrefs.GetInt("Match3Record");
			ResultsRecordCounter.Text_ = record.ToString();
			ResultsPointsCounter.Text_ = _countPoints.ToString();
			_closeButton.localPosition = new Vector3(_closeButton.localPosition.x, _closeButton.localPosition.y, Blocker.parent.parent.localPosition.z - 250f);
		}
		else
		{
			source = new Vector3(GetBlockerShowPos(), 0f, Blocker.localPosition.z);
			dest = new Vector3(GetBlockerHidePos(), 0f, Blocker.localPosition.z);
			_closeButton.localPosition = new Vector3(_closeButton.localPosition.x, _closeButton.localPosition.y, _closeButtonDepth);
		}
		Blocker.localPosition = source;
		List<GameObject> countersVisible = new List<GameObject>();
		List<GameObject> countersHidden = new List<GameObject>();
		countersVisible.Add(CountGold.transform.parent.gameObject);
		if (_countCrystall > 0)
		{
			countersVisible.Add(CountCrystall.transform.parent.gameObject);
		}
		else
		{
			countersHidden.Add(CountCrystall.transform.parent.gameObject);
		}
		if (_countSkull > 0)
		{
			countersVisible.Add(CountSkull.transform.parent.gameObject);
		}
		else
		{
			countersHidden.Add(CountSkull.transform.parent.gameObject);
		}
		if (_countStar > 0)
		{
			countersVisible.Add(CountStar.transform.parent.gameObject);
		}
		else
		{
			countersHidden.Add(CountStar.transform.parent.gameObject);
		}
		if (_countScarab > 0)
		{
			countersVisible.Add(CountScarab.transform.parent.gameObject);
		}
		else
		{
			countersHidden.Add(CountScarab.transform.parent.gameObject);
		}
		foreach (GameObject item in countersHidden)
		{
			item.SetActiveRecursivelyMk1(setActive: false);
		}
		int offset = -(countersVisible.Count * 175) / 2;
		for (int i = 0; i < countersVisible.Count; i++)
		{
			GameObject item2 = countersVisible[i];
			item2.transform.localPosition = new Vector3(offset + i * 175, item2.transform.localPosition.y, item2.transform.localPosition.z);
			item2.SetActiveRecursivelyMk1(setActive: true);
		}
		while (time < 0.4f)
		{
			Blocker.localPosition = Vector3.Lerp(source, dest, time / 0.4f);
			time += Time.deltaTime;
			yield return null;
		}
		Blocker.localPosition = dest;
	}

	private int GetBlockerHidePos()
	{
		return -Camera2D.ScreenWidth;
	}

	private int GetBlockerShowPos()
	{
		return 0;
	}

	private void UpdateLootGrabbing()
	{
		if (_lootTimer > 0f)
		{
			foreach (Match3Block item in _loot)
			{
				if (item.Y == 0)
				{
					item.gameObject.transform.localPosition = Vector3.Lerp(item.StartPos, GameFieldCoordsToScreenPoint(item.X, item.Y), 1f - _lootTimer / 0.2f);
				}
			}
			_lootTimer -= Time.deltaTime;
			return;
		}
		for (int i = 0; i < _loot.Count; i++)
		{
			Match3Block match3Block = _loot[i];
			if (match3Block.Y == 0)
			{
				this[match3Block.Y * 7 + match3Block.X] = null;
				RemoveBlock(match3Block);
				switch (match3Block.Loot.Item.ElixirType)
				{
				case ServerData.Item.ElixirTypeE.Skull:
					_countSkull += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.MoneySkullsCount += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.Match3SkullMined += match3Block.Loot.Count;
					break;
				case ServerData.Item.ElixirTypeE.Scarab:
					_countScarab += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.MoneyScarabCount += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.Match3ScarabMined += match3Block.Loot.Count;
					break;
				case ServerData.Item.ElixirTypeE.Gold:
					_countGold += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.MoneyGoldCount += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.Match3GoldMined += match3Block.Loot.Count;
					break;
				case ServerData.Item.ElixirTypeE.Diamond:
					_countCrystall += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.MoneyDiamondCount += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.Match3CrystallMined += match3Block.Loot.Count;
					break;
				case ServerData.Item.ElixirTypeE.Star:
					_countStar += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.MoneyStarsCount += match3Block.Loot.Count;
					SingletonT<ServerData>.I.PlayerParams.Match3StarMined += match3Block.Loot.Count;
					break;
				}
				StartCoroutine(AnimateLootGrabbing(match3Block));
				_loot.RemoveAt(i);
				i--;
				Messenger.Invoke(Globals.MsgObtainInMine);
				Globals.MainMenu.SaveGame();
			}
		}
		SingletonT<SoundManager>.I.PlayGlobalSound("star");
		MakeCorrectStacks();
		_addingTimer = 0.2f;
		_state = StateE.AddingNewBlocks;
		foreach (Match3Block item2 in _loot)
		{
			if (item2.Y == 0)
			{
				_lootTimer = 0.2f;
				_state = StateE.LootGrabbing;
				break;
			}
		}
	}

	private IEnumerator AnimateLootGrabbing(Match3Block loot)
	{
		float time = 0f;
		Vector3 sourcePos = loot.transform.position + new Vector3(0f, 0f, -150f);
		GameObject icon = null;
		switch (loot.Loot.Item.ElixirType)
		{
		case ServerData.Item.ElixirTypeE.Skull:
			icon = IconSkull.gameObject;
			break;
		case ServerData.Item.ElixirTypeE.Scarab:
			icon = IconScarab.gameObject;
			break;
		case ServerData.Item.ElixirTypeE.Gold:
			icon = IconGold.gameObject;
			break;
		case ServerData.Item.ElixirTypeE.Diamond:
			icon = IconCrystall.gameObject;
			break;
		case ServerData.Item.ElixirTypeE.Star:
			icon = IconStar.gameObject;
			break;
		}
		Vector3 destPos = sourcePos + new Vector3(0f, Camera2D.ScreenHeight + 100, 0f);
		GameObject fxGO = (GameObject)Object.Instantiate(icon);
		fxGO.transform.position = sourcePos;
		GameObject fx = (GameObject)Object.Instantiate(TrailPrefab);
		fx.transform.parent = fxGO.transform;
		fx.transform.localPosition = new Vector3(0f, 0f, 25f);
		fxGO.transform.SetLayerRecursively(base.transform);
		for (; time < 1.4f; time += Time.deltaTime)
		{
			fxGO.transform.position = Vector3.Lerp(sourcePos, destPos, time / 1.4f);
			yield return null;
		}
		fxGO.transform.position = destPos;
		UpdateCounters();
		yield return new WaitForSeconds(0.8f);
		Object.Destroy(fxGO);
	}

	private void UpdateCounters()
	{
		CountGold.Text_ = _countGold.ToString();
		CountCrystall.Text_ = _countCrystall.ToString();
		CountSkull.Text_ = _countSkull.ToString();
		CountStar.Text_ = _countStar.ToString();
		CountScarab.Text_ = _countScarab.ToString();
	}

	private void UpdateRemovingBlocks()
	{
		if (_removingTimer > 0f)
		{
			if (_removingTimer == 0.2f)
			{
				foreach (Match3Block item in _currentBlockChain)
				{
					item.IsSelected = false;
					RemoveBlock(item);
					if (BlockDestroyingFxPrefabs != null && BlockDestroyingFxPrefabs.Length > 0)
					{
						GameObject gameObject = (GameObject)Object.Instantiate(BlockDestroyingFxPrefabs[Random.Range(0, BlockDestroyingFxPrefabs.Length)]);
						Transform transform = gameObject.transform.FindChildByName("fx_details_01");
						ParticleSystemRenderer component = transform.GetComponent<ParticleSystemRenderer>();
						Color32 color = new Color32(17, 78, 122, byte.MaxValue);
						switch (item.Type)
						{
						case Match3Block.TypeE.Red:
							color = new Color32(172, 8, 1, byte.MaxValue);
							break;
						case Match3Block.TypeE.Blue:
							color = new Color32(17, 78, 122, byte.MaxValue);
							break;
						case Match3Block.TypeE.Violet:
							color = new Color32(109, 40, 142, byte.MaxValue);
							break;
						}
						component.material.SetColor("_TintColor", color);
						Suicidal suicidal = gameObject.AddComponent<Suicidal>();
						suicidal.SuicideTime = 4f;
						gameObject.transform.parent = GamefieldRoot;
						gameObject.transform.localPosition = item.transform.localPosition + new Vector3(0f, 0f, -150f);
					}
				}
				SingletonT<SoundManager>.I.PlayGlobalSound(GetDestroyingSound(_currentBlockChain.Count));
			}
			_removingTimer -= Time.deltaTime;
			return;
		}
		_lootColumn = _currentBlockChain[Random.Range(0, _currentBlockChain.Count)].X;
		while (_currentBlockChain.Count > 0)
		{
			Match3Block match3Block = _currentBlockChain.Pop();
			this[7 * match3Block.Y + match3Block.X] = null;
		}
		MakeCorrectStacks();
		_addingTimer = 0.2f;
		_state = StateE.AddingNewBlocks;
		foreach (Match3Block item2 in _loot)
		{
			if (item2.Y == 0)
			{
				_lootTimer = 0.2f;
				_state = StateE.LootGrabbing;
				break;
			}
		}
	}

	private void MakeCorrectStacks()
	{
		for (int i = 0; i < 7; i++)
		{
			int num = -1;
			for (int j = 0; j < 7; j++)
			{
				int index = j * 7 + i;
				Match3Block match3Block = this[index];
				if (match3Block != null)
				{
					if (match3Block.Y - num > 1)
					{
						this[index] = null;
						match3Block.StartPos = match3Block.transform.localPosition;
						match3Block.Y = num + 1;
						this[match3Block.Y * 7 + match3Block.X] = match3Block;
						_fallingBlocks.Add(match3Block);
					}
					num = match3Block.Y;
				}
			}
		}
	}

	private void UpdateAddingNewBlocks()
	{
		if (_addingTimer > 0f)
		{
			if (_addingTimer == 0.2f)
			{
				GenerateNewBlocks();
			}
			foreach (Match3Block fallingBlock in _fallingBlocks)
			{
				fallingBlock.gameObject.transform.localPosition = Vector3.Lerp(fallingBlock.StartPos, GameFieldCoordsToScreenPoint(fallingBlock.X, fallingBlock.Y), 1f - _addingTimer / 0.2f);
			}
			_addingTimer -= Time.deltaTime;
			return;
		}
		foreach (Match3Block fallingBlock2 in _fallingBlocks)
		{
			fallingBlock2.gameObject.transform.localPosition = GameFieldCoordsToScreenPoint(fallingBlock2.X, fallingBlock2.Y) + new Vector3(0f, 0f, -50f);
		}
		SingletonT<SoundManager>.I.PlayGlobalSound(GetRandomFallingSound());
		_fallingBlocks.Clear();
		_state = StateE.WaitUserInput;
	}

	private void GenerateNewBlocks()
	{
		bool flag = _loot.Count == 0 || Random.Range(0, 100) < ((Difficulty != DifficultyE.Normal) ? SingletonT<ServerData>.I.GameSettings.Match3BonusProbEasy : SingletonT<ServerData>.I.GameSettings.Match3BonusProbNormal);
		for (int i = 0; i < 7; i++)
		{
			int num = 6;
			while (num >= 0)
			{
				int index = num * 7 + i;
				Match3Block match3Block = this[index];
				if (match3Block == null)
				{
					match3Block = GetFreeBlock();
					match3Block.X = i;
					match3Block.Y = num;
					this[match3Block.Y * 7 + match3Block.X] = match3Block;
					match3Block.transform.localPosition = GameFieldCoordsToScreenPoint(match3Block.X, match3Block.Y) + new Vector3(0f, Camera2D.ScreenHeight / 2, -50f);
					match3Block.StartPos = match3Block.transform.localPosition;
					match3Block.gameObject.SetActiveRecursivelyMk1(setActive: true);
					match3Block.IsSelected = false;
					if (_loot.Count < 3 && num == 6 && i == _lootColumn && flag)
					{
						ServerData.Bonus.DropElement randomLoot = GetRandomLoot(_mine.SecondBonus);
						match3Block.Init(Match3Block.TypeE.Loot, randomLoot, new Vector2(91f, 91f));
						_loot.Add(match3Block);
					}
					else
					{
						match3Block.Init((Match3Block.TypeE)Random.Range(0, (Difficulty != DifficultyE.Easy) ? 3 : 2), null, new Vector2(91f, 91f));
					}
					_fallingBlocks.Add(match3Block);
					num--;
					continue;
				}
				break;
			}
		}
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.Match3 && old == GuiRoot.GuiType.Match3StartScreen)
		{
			Reset();
		}
	}

	private void Reset()
	{
		_gameTime = Time.realtimeSinceStartup;
		_lockpicksUsed = 0;
		_countGold = 0;
		_countCrystall = 0;
		_countSkull = 0;
		_countStar = 0;
		_countScarab = 0;
		_countPoints = 0;
		UpdateCounters();
		PointsCounter.Text_ = _countPoints.ToString();
		if (PlayerPrefs.HasKey("Match3Record"))
		{
			RecordCounter.Text_ = PlayerPrefs.GetInt("Match3Record").ToString();
		}
		else
		{
			PlayerPrefs.SetInt("Match3Record", 0);
			PlayerPrefs.Save();
		}
		_isBlockedByTimer = false;
		Blocker.localPosition = new Vector3(GetBlockerHidePos(), 0f, Blocker.localPosition.z);
		for (int i = 0; i < Length; i++)
		{
			Match3Block match3Block = this[i];
			if (match3Block != null)
			{
				match3Block.gameObject.SetActiveRecursivelyMk1(setActive: false);
				_cachedBlocks.Push(match3Block);
				this[i] = null;
			}
		}
		int num = Random.Range(0, 7);
		_loot.Clear();
		for (int j = 0; j < 7; j++)
		{
			for (int k = 0; k < 7; k++)
			{
				int index = j * 7 + k;
				Match3Block freeBlock = GetFreeBlock();
				freeBlock.gameObject.SetActiveRecursivelyMk1(setActive: true);
				freeBlock.X = k;
				freeBlock.Y = j;
				freeBlock.gameObject.transform.localPosition = GameFieldCoordsToScreenPoint(freeBlock.X, freeBlock.Y) + new Vector3(0f, 0f, -50f);
				if (j == 6 && k == num && _loot.Count < 3)
				{
					ServerData.Bonus.DropElement randomLoot = GetRandomLoot(_mine.FirstBonus);
					freeBlock.Init(Match3Block.TypeE.Loot, randomLoot, new Vector2(91f, 91f));
					_loot.Add(freeBlock);
				}
				else
				{
					freeBlock.Init((Match3Block.TypeE)Random.Range(0, (Difficulty != DifficultyE.Easy) ? 3 : 2), null, new Vector2(91f, 91f));
				}
				this[index] = freeBlock;
				freeBlock.IsSelected = false;
			}
		}
		switch (Difficulty)
		{
		case DifficultyE.Easy:
			_mainTimerMax = SingletonT<ServerData>.I.GameSettings.Match3TimerEasy;
			break;
		case DifficultyE.Normal:
			_mainTimerMax = SingletonT<ServerData>.I.GameSettings.Match3TimerNormal;
			break;
		}
		string phrase = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.Match3HelpBottom);
		HelpText.Phrase_ = ServerData.PhrasesE.Custom;
		HelpText.Text_ = phrase.Fmt(_keysCost.Item2);
		phrase = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.Match3ButtonUseKeys);
		UseKeysButtonText.Phrase_ = ServerData.PhrasesE.Custom;
		UseKeysButtonText.Text_ = phrase.Fmt("{0} {1}".Fmt(_keysCost.Item2, _keysCost.Item3));
		CountTime.Text_ = _mainTimerMax.ToString();
		_mainTimer = _mainTimerMax;
	}

	private ServerData.Bonus.DropElement GetRandomLoot(ServerData.Bonus bonus)
	{
		ServerData.Bonus.DropElement result = null;
		Utils.Random(bonus.Drop, (ServerData.Bonus.DropElement loot) => loot.Probability, 1, allowDuplicates: false, delegate(int i, int j)
		{
			result = bonus.Drop[j].MakeDrop();
		});
		return result;
	}

	private Match3Block GetFreeBlock()
	{
		if (_cachedBlocks.Count > 0)
		{
			return _cachedBlocks.Pop();
		}
		GameObject gameObject = (GameObject)Object.Instantiate(BlockPrefab);
		gameObject.transform.parent = GamefieldRoot;
		Match3Block component = gameObject.GetComponent<Match3Block>();
		component.Icon.gameObject.SetActiveRecursivelyMk1(setActive: false);
		component.Count.gameObject.SetActiveRecursivelyMk1(setActive: false);
		component.IsSelected = false;
		return component;
	}

	private Match3Arrow GetFreeArrow()
	{
		if (_cachedArrows.Count > 0)
		{
			return _cachedArrows.Pop();
		}
		GameObject gameObject = (GameObject)Object.Instantiate(ArrowPrefab);
		gameObject.transform.parent = GamefieldRoot;
		return gameObject.GetComponent<Match3Arrow>();
	}

	private void Instance_MoveBegin(Vector3 obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Match3 && _state == StateE.WaitUserInput && !(_mainTimer < 0f))
		{
		}
	}

	private void Instance_MoveEnd(Vector3 obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Match3 && _state == StateE.WaitUserInput && !(_mainTimer < 0f))
		{
			FinishChain();
		}
	}

	private void FinishChain()
	{
		if (_currentBlockChain.Count > 2)
		{
			_state = StateE.RemovingBlockChain;
			_removingTimer = 0.2f;
			int num = 0;
			float scale = 1f;
			foreach (System.Tuple<int, int, float> point in _points)
			{
				if (_currentBlockChain.Count >= point.Item1)
				{
					num = point.Item2;
					scale = point.Item3;
					continue;
				}
				break;
			}
			if (num > 0)
			{
				num *= ((Difficulty != DifficultyE.Normal) ? 1 : 10);
				_countPoints += num;
				int num2 = (PlayerPrefs.HasKey("Match3Record") ? PlayerPrefs.GetInt("Match3Record") : 0);
				if (num2 < _countPoints)
				{
					PlayerPrefs.SetInt("Match3Record", _countPoints);
					RecordCounter.Text_ = _countPoints.ToString();
					Messenger<int>.Invoke(Globals.MsgMatch3NewRecord, _countPoints);
				}
				PointsCounter.Text_ = _countPoints.ToString();
				if (Difficulty == DifficultyE.Normal)
				{
					Messenger<int>.Invoke(Globals.MsgMatch3ChainDestroyed, _currentBlockChain.Count);
				}
				StartCoroutine(ShowPoints(num, _currentBlockChain.Peek().transform.position, scale));
			}
		}
		else
		{
			while (_currentBlockChain.Count > 0)
			{
				Match3Block match3Block = _currentBlockChain.Pop();
				match3Block.IsSelected = false;
			}
		}
		while (_currentArrowChain.Count > 0)
		{
			Match3Arrow arrow = _currentArrowChain.Pop();
			RemoveArrow(arrow);
		}
	}

	private IEnumerator ShowPoints(int points, Vector3 pos, float scale)
	{
		float time = 0f;
		GameObject textGO = CreatePointsText(points);
		textGO.transform.position = pos + new Vector3(0f, 0f, -250f);
		float curveLength = TextAnimationCurve.keys[TextAnimationCurve.keys.Length - 1].time;
		while (time < curveLength && !(textGO == null))
		{
			float currentScale = scale * TextAnimationCurve.Evaluate(time);
			textGO.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
			time += Time.deltaTime;
			yield return null;
		}
		if (textGO != null)
		{
			textGO.transform.localScale = Vector3.zero;
		}
	}

	private GameObject CreatePointsText(int points)
	{
		GameObject gameObject = new GameObject();
		SpriteText spriteText = gameObject.AddComponent<SpriteText>();
		spriteText.FontFamily = FontManager.FontFamilyE.Tahoma;
		spriteText.NamedColorE = FontManager.ColorE.BagMoney;
		spriteText.Anchor = TextAnchor.MiddleCenter;
		spriteText.Outline = true;
		spriteText.Bold = true;
		spriteText.PixSize_ = 34;
		spriteText.Text_ = "{0}{1}".Fmt('\u001f', points);
		Suicidal suicidal = gameObject.AddComponent<Suicidal>();
		suicidal.SuicideTime = TextAnimationCurve.keys[TextAnimationCurve.keys.Length - 1].time + 0.1f;
		gameObject.transform.SetLayerRecursively(base.transform);
		return gameObject;
	}

	private static float GetWorldScaleX()
	{
		return (float)Camera2D.ScreenWidth / (float)Screen.width;
	}

	private static float GetWorldScaleY()
	{
		return (float)Camera2D.ScreenHeight / (float)Screen.height;
	}

	private void Instance_MoveTouch(Vector3 arg1)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Match3 && _state == StateE.WaitUserInput && !(_mainTimer < 0f) && !TutorialFullScreenInfo.IsShowDialog)
		{
			arg1.x *= Camera2D.Scale * GetWorldScaleX();
			arg1.y *= Camera2D.Scale * GetWorldScaleY();
			Match3Block blockFromField = GetBlockFromField(arg1);
			if (blockFromField != null && _currentBlockChain.Count == 0 && blockFromField.Type != Match3Block.TypeE.Loot)
			{
				_currentBlockChain.Push(blockFromField);
				blockFromField.IsSelected = true;
			}
		}
	}

	private void Instance_Move(Vector3 arg1, Vector3 arg2)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Match3 || _state != StateE.WaitUserInput || _mainTimer < 0f)
		{
			return;
		}
		arg1.x *= Camera2D.Scale * GetWorldScaleX();
		arg1.y *= Camera2D.Scale * GetWorldScaleY();
		arg2.x *= Camera2D.Scale * GetWorldScaleX();
		arg2.y *= Camera2D.Scale * GetWorldScaleY();
		Match3Block blockFromField = GetBlockFromField(arg2);
		if (!(blockFromField != null))
		{
			return;
		}
		if (_currentBlockChain.Count == 0)
		{
			if (blockFromField.Type != Match3Block.TypeE.Loot)
			{
				_currentBlockChain.Push(blockFromField);
				blockFromField.IsSelected = true;
			}
			return;
		}
		Match3Block match3Block = _currentBlockChain.Peek();
		if (!(blockFromField != match3Block) || blockFromField.Type != match3Block.Type)
		{
			return;
		}
		if (_currentBlockChain.Count > 1 && _currentBlockChain[_currentBlockChain.Count - 2] == blockFromField)
		{
			match3Block.IsSelected = false;
			_currentBlockChain.Pop();
			Match3Arrow arrow = _currentArrowChain.Pop();
			RemoveArrow(arrow);
			return;
		}
		bool flag = false;
		Match3Arrow.DirectionE directionE = Match3Arrow.DirectionE.E;
		if (blockFromField.X == match3Block.X && Mathf.Abs(blockFromField.Y - match3Block.Y) == 1)
		{
			flag = true;
			directionE = ((blockFromField.Y <= match3Block.Y) ? Match3Arrow.DirectionE.S : Match3Arrow.DirectionE.N);
		}
		else if (blockFromField.Y == match3Block.Y && Mathf.Abs(blockFromField.X - match3Block.X) == 1)
		{
			flag = true;
			directionE = ((blockFromField.X <= match3Block.X) ? Match3Arrow.DirectionE.W : Match3Arrow.DirectionE.E);
		}
		if (flag && !_currentBlockChain.Contains(blockFromField))
		{
			_currentBlockChain.Push(blockFromField);
			blockFromField.IsSelected = true;
			Match3Arrow freeArrow = GetFreeArrow();
			_currentArrowChain.Push(freeArrow);
			freeArrow.Direction = directionE;
			int num = 637;
			int num2 = (Camera2D.ScreenHeight - num) / 2;
			switch (directionE)
			{
			case Match3Arrow.DirectionE.N:
				freeArrow.transform.localPosition = GameFieldCoordsToScreenPoint(blockFromField.X, blockFromField.Y) + new Vector3(0f, -45f, -200f);
				break;
			case Match3Arrow.DirectionE.S:
				freeArrow.transform.localPosition = GameFieldCoordsToScreenPoint(blockFromField.X, blockFromField.Y) + new Vector3(0f, 45f, -200f);
				break;
			case Match3Arrow.DirectionE.E:
				freeArrow.transform.localPosition = GameFieldCoordsToScreenPoint(blockFromField.X, blockFromField.Y) + new Vector3(-45f, 0f, -200f);
				break;
			case Match3Arrow.DirectionE.W:
				freeArrow.transform.localPosition = GameFieldCoordsToScreenPoint(blockFromField.X, blockFromField.Y) + new Vector3(45f, 0f, -200f);
				break;
			}
			freeArrow.gameObject.SetActiveRecursivelyMk1(setActive: true);
		}
	}

	private void RemoveArrow(Match3Arrow arrow)
	{
		arrow.gameObject.SetActiveRecursivelyMk1(setActive: false);
		_cachedArrows.Push(arrow);
	}

	private void RemoveBlock(Match3Block block)
	{
		block.gameObject.SetActiveRecursivelyMk1(setActive: false);
		_cachedBlocks.Push(block);
	}

	private Match3Block GetBlockFromField(Vector3 screenPoint)
	{
		ScreenPointToGameFieldCoords(screenPoint, out var x, out var y);
		if (x > -1 && y > -1)
		{
			Match3Block match3Block = this[y * 7 + x];
			match3Block.X = x;
			match3Block.Y = y;
			return match3Block;
		}
		return null;
	}

	private void ScreenPointToGameFieldCoords(Vector3 screenPoint, out int x, out int y)
	{
		x = -1;
		y = -1;
		int num = 637;
		int num2 = (int)GamefieldRoot.localPosition.x + Camera2D.ScreenWidth / 2;
		if (!(screenPoint.x < (float)num2) && !(screenPoint.x >= (float)(num2 + num)))
		{
			int num3 = 637;
			int num4 = (Camera2D.ScreenHeight - num3) / 2;
			if (!(screenPoint.y < (float)num4) && !(screenPoint.y >= (float)(num4 + num3)))
			{
				x = (int)((screenPoint.x - (float)num2) / 91f);
				y = (int)((screenPoint.y - (float)num4) / 91f);
			}
		}
	}

	private Vector3 GameFieldCoordsToScreenPoint(int x, int y)
	{
		int num = 637;
		int num2 = (Camera2D.ScreenHeight - num) / 2;
		return new Vector3(x * 91 + 45, -num / 2 + y * 91 + 45, 0f);
	}

	private string GetRandomFallingSound()
	{
		return _fallingSounds[Random.Range(0, _fallingSounds.Length)];
	}

	private string GetDestroyingSound(int chainLength)
	{
		if (chainLength >= _destroyingSounds.Length)
		{
			chainLength = _destroyingSounds.Length - 1;
		}
		return _destroyingSounds[chainLength];
	}
}
