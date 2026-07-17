using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class LockpickingHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private SpriteGui _spriteGui;

	private ServerData.Chest _chest;

	private bool _win;

	private ActionD _winCallback;

	private ActionD _loseCallback;

	private int[] _iconIdxs = new int[27];

	private bool _showAllActive;

	private LockpickingCell _onHold;

	private LockpickingCell _rightCell1;

	private LockpickingCell _rightCell2;

	private LockpickingCell _wrongCell1;

	private LockpickingCell _wrongCell2;

	private string _timerFormat;

	private string _helpBottomFormat;

	private readonly Vector3 _lootIconPlace = new Vector3(77f, 0f, 0f);

	private int _uniqueId;

	private Stack<GameObject> _lootButtons = new Stack<GameObject>();

	private int _startLockpicksCount;

	public float ShowTime = 5f;

	public Vector2[] SmallCoords;

	public Vector2[] MediumCoords;

	public Vector2[] LargeCoords;

	public Vector3 HellCoord = new Vector3(0f, 1000f, 0f);

	public float FadeInOutTick = 0.3f;

	public Transform LockBg;

	public Transform ChestBg;

	public LockpickingCell[] Cells;

	public SpriteText Title;

	public SpriteText Messages;

	public SpriteText LockpickCount;

	public SpriteText HelpBottom;

	public GameObject LootProto;

	public AnimationCurve InCurve;

	public AnimationCurve InCurveLean;

	public AnimationCurve OutCurve;

	public AnimationCurve OutToZero;

	public GameObject ButtonContinue;

	public GameObject LeftRoot;

	public GameObject RightRoot;

	public Transform RightMask;

	public float RightRootWidth = 330f;

	private int UniqueId => _uniqueId++;

	private LockpickingCell OnHold
	{
		get
		{
			return _onHold;
		}
		set
		{
			if (!(_onHold == value))
			{
				if (_onHold != null)
				{
					_onHold.SetClear();
				}
				if (value != null)
				{
					value.SetOrange();
					PostMessage(ServerData.PhrasesE.LockpickingFindAMatch, FontManager.ColorE.LockMessageGold);
				}
				_onHold = value;
			}
		}
	}

	private LockpickingCell RightCell1
	{
		get
		{
			return _rightCell1;
		}
		set
		{
			if (value == null)
			{
				if (_rightCell1 != null)
				{
					_rightCell1.SetClear();
				}
			}
			else
			{
				value.SetGreen();
				value.OpenForever();
			}
			_rightCell1 = value;
		}
	}

	private LockpickingCell RightCell2
	{
		get
		{
			return _rightCell2;
		}
		set
		{
			if (value == null)
			{
				if (_rightCell2 != null)
				{
					_rightCell2.SetClear();
				}
			}
			else
			{
				value.SetGreen();
				value.OpenForever();
			}
			_rightCell2 = value;
		}
	}

	private LockpickingCell WrongCell1
	{
		get
		{
			return _wrongCell1;
		}
		set
		{
			if (value == null)
			{
				if (_wrongCell1 != null)
				{
					_wrongCell1.SetClear();
				}
			}
			else
			{
				value.SetRed();
			}
			_wrongCell1 = value;
		}
	}

	private LockpickingCell WrongCell2
	{
		get
		{
			return _wrongCell2;
		}
		set
		{
			if (value == null)
			{
				if (_wrongCell2 != null)
				{
					_wrongCell2.SetClear();
				}
			}
			else
			{
				value.SetRed();
			}
			_wrongCell2 = value;
		}
	}

	public void MakeGame(ServerData.Chest chest, ActionD winCallback, ActionD loseCallback)
	{
		_chest = chest;
		_winCallback = winCallback;
		_loseCallback = loseCallback;
		_win = false;
		_startLockpicksCount = SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount;
		_helpBottomFormat = HelpBottom.Text_;
		HelpBottom.Phrase_ = ServerData.PhrasesE.Custom;
		HelpBottom.Text_ = string.Format(_helpBottomFormat, 5, 10);
		_timerFormat = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.LockpickingSecBeforeGame);
		Animation component = LockBg.GetComponent<Animation>();
		component.Stop();
		LockBg.gameObject.SampleAnimation(component.clip, 0f);
		RightMask.localPosition = new Vector3(3000f, RightMask.localPosition.y, RightMask.localPosition.z);
		ButtonContinue.SetActiveRecursivelyMk1(setActive: false);
		StopAllCoroutines();
		while (_lootButtons.Count > 0)
		{
			GameObject obj = _lootButtons.Pop();
			UnityEngine.Object.Destroy(obj);
		}
		switch (chest.Type)
		{
		case 1:
			SmallCoords.ShuffleInPlace();
			DoMakeGame(SmallCoords);
			Title.Phrase_ = ServerData.PhrasesE.LockpickingDifficultyLow;
			break;
		case 2:
			MediumCoords.ShuffleInPlace();
			DoMakeGame(MediumCoords);
			Title.Phrase_ = ServerData.PhrasesE.LockpickingDifficultyMedium;
			break;
		case 3:
			LargeCoords.ShuffleInPlace();
			DoMakeGame(LargeCoords);
			Title.Phrase_ = ServerData.PhrasesE.LockpickingDifficultyHigh;
			break;
		default:
			throw new ArgumentOutOfRangeException("chest");
		}
		if (SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount > 0)
		{
			UseLockpicks(0);
			ShowAll();
		}
		else
		{
			UseLockpicks(0);
		}
		SetupLockpickCount();
	}

	private void Awake()
	{
		_spriteGui = base.transform.GetSpriteGui();
		for (int i = 0; i < _iconIdxs.Length; i++)
		{
			_iconIdxs[i] = i + 1;
		}
		_iconIdxs.ShuffleInPlace();
		LeftRoot.transform.localPosition = new Vector3((int)(((float)Camera2D.ScreenWidth - RightRootWidth) / 2f), LeftRoot.transform.localPosition.y, LeftRoot.transform.localPosition.z);
		ClearMessage();
	}

	private void OnEnable()
	{
		_spriteGui.Release += ProcessButtons;
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.MoneyType.TypeE, string>.AddListener(Globals.MsgPlayerFundsChanged, OnLockPickCount));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
		_spriteGui.Release -= ProcessButtons;
	}

	private void SetupLockpickCount()
	{
		ServerData.MoneyType.TypeE type = ServerData.MoneyType.TypeE.Key;
		string text_ = type.GetPlayerFundsCount().ToString();
		LockpickCount.Text_ = text_;
	}

	private void OnLockPickCount(ServerData.MoneyType.TypeE typeE, string reason)
	{
		if (typeE == ServerData.MoneyType.TypeE.Key)
		{
			SetupLockpickCount();
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void ProcessButtons(SpriteButton button)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.ChestMiniGame)
		{
			return;
		}
		if (button.name == "_close_button")
		{
			_loseCallback();
		}
		else if (button.name == "chest_minigame_continue")
		{
			_loseCallback();
		}
		else
		{
			if (_showAllActive || _win)
			{
				return;
			}
			if (button.name == "show_cells" && SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount >= 10)
			{
				UseLockpicks(10);
				ShowAll();
				return;
			}
			LockpickingCell lockpickingCell = button as LockpickingCell;
			if (!(lockpickingCell != null))
			{
				return;
			}
			if (RightCell1 != null || RightCell2 != null)
			{
				RightCell1 = null;
				RightCell2 = null;
			}
			if (OnHold == null)
			{
				if (lockpickingCell == WrongCell1)
				{
					WrongCell1 = null;
					WrongCell2.Hide();
					WrongCell2 = null;
				}
				else if (lockpickingCell == WrongCell2)
				{
					WrongCell2 = null;
					WrongCell1.Hide();
					WrongCell1 = null;
				}
				else
				{
					if (WrongCell1 != null)
					{
						WrongCell1.Hide();
						WrongCell1 = null;
					}
					if (WrongCell2 != null)
					{
						WrongCell2.Hide();
						WrongCell2 = null;
					}
					lockpickingCell.Show();
				}
				OnHold = lockpickingCell;
			}
			else if (!(lockpickingCell == OnHold))
			{
				if (lockpickingCell.CellIdx == OnHold.CellIdx)
				{
					LockpickingCell onHold = OnHold;
					OnHold = null;
					RightCell1 = onHold;
					RightCell2 = lockpickingCell;
					PostMessage(ServerData.PhrasesE.LockpickingMatchFound, FontManager.ColorE.CompareGreen);
					SingletonT<SoundManager>.I.PlaySuccessSound();
					CheckWinCondition();
				}
				else
				{
					UseLockpicks(1);
					LockpickingCell onHold2 = OnHold;
					OnHold = null;
					SingletonT<SoundManager>.I.PlayFailSound();
					WrongCell1 = lockpickingCell;
					WrongCell2 = onHold2;
					lockpickingCell.Show();
				}
			}
		}
	}

	private void DoMakeGame(Vector2[] coords)
	{
		LockBg.gameObject.active = true;
		ChestBg.gameObject.active = false;
		_iconIdxs.ShuffleInPlace();
		GoToHell();
		int num = coords.Length / 2;
		List<int> randomIconIndices = new List<int>(_chest.PicturesCount);
		Utils.Random(_iconIdxs, (int p) => 1, _chest.PicturesCount, allowDuplicates: false, delegate(int number, int index)
		{
			randomIconIndices.Add(_iconIdxs[index]);
		});
		int num2 = 0;
		for (int num3 = 0; num3 < num; num3++)
		{
			int cell = randomIconIndices[num2];
			num2++;
			num2 %= randomIconIndices.Count;
			LockpickingCell lockpickingCell = Cells[num3];
			LockpickingCell lockpickingCell2 = Cells[num + num3];
			lockpickingCell.State = LockpickingCell.StateE.Hidden;
			lockpickingCell.State = LockpickingCell.StateE.Hidden;
			lockpickingCell.SetCell(cell);
			lockpickingCell2.SetCell(cell);
			lockpickingCell.transform.localPosition = coords[num3];
			lockpickingCell2.transform.localPosition = coords[num + num3];
		}
	}

	private void GoToHell()
	{
		LockpickingCell[] cells = Cells;
		foreach (LockpickingCell lockpickingCell in cells)
		{
			lockpickingCell.transform.localPosition = HellCoord;
			lockpickingCell.SetCell(0);
			lockpickingCell.State = LockpickingCell.StateE.Inactive;
		}
	}

	private void UseLockpicks(int count)
	{
		if (Globals.DebugKeysInfinite)
		{
			SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount += count + 1;
		}
		if (SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount <= 0)
		{
			SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount = 0;
			LoseGame();
			return;
		}
		SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount -= count;
		if (count == 1)
		{
			PostMessage(ServerData.PhrasesE.LockpickingWrongMatch, FontManager.ColorE.CompareRed);
		}
		else
		{
			ClearMessage();
		}
	}

	private void PostMessage(ServerData.PhrasesE phrase, FontManager.ColorE color)
	{
		Messages.gameObject.SetActiveRecursivelyMk1(setActive: true);
		Messages.Phrase_ = phrase;
		Messages.NamedColorE_ = color;
	}

	private void PostMessage(string fmt, int value, FontManager.ColorE color)
	{
		Messages.gameObject.SetActiveRecursivelyMk1(setActive: true);
		Messages.Phrase_ = ServerData.PhrasesE.Custom;
		Messages.Text_ = string.Format(fmt, value);
		Messages.NamedColorE_ = color;
	}

	private void ClearMessage()
	{
		Messages.gameObject.SetActiveRecursivelyMk1(setActive: false);
		Messages.Phrase_ = ServerData.PhrasesE.Custom;
		Messages.Text_ = string.Empty;
	}

	private void ShowAll()
	{
		StartCoroutine("ShowAllCoro");
	}

	private IEnumerator ShowAllCoro()
	{
		OnHold = null;
		WrongCell1 = null;
		WrongCell2 = null;
		RightCell1 = null;
		RightCell2 = null;
		_showAllActive = true;
		int count = 0;
		LockpickingCell[] cells = Cells;
		foreach (LockpickingCell lockCell in cells)
		{
			if (lockCell.State == LockpickingCell.StateE.Hidden)
			{
				lockCell.Show();
				count++;
			}
		}
		int minCount = count / 4;
		ClearMessage();
		yield return null;
		float start = Time.time;
		int maxSec = ShowTime.RoundToInt();
		PostMessage(_timerFormat, maxSec, FontManager.ColorE.LockMessageGold);
		while (Time.time < start + ShowTime)
		{
			float dt = Time.time - start;
			int left = (ShowTime - dt).CeilToInt();
			if (left < maxSec)
			{
				maxSec = left;
				PostMessage(_timerFormat, left, FontManager.ColorE.LockMessageGold);
			}
			yield return null;
		}
		ClearMessage();
		int loop = 0;
		while (count > 0)
		{
			loop++;
			LockpickingCell[] cells2 = Cells;
			foreach (LockpickingCell lockCell2 in cells2)
			{
				if (lockCell2.State == LockpickingCell.StateE.Showed && (count <= minCount || loop >= 4 || 3.Dice0()))
				{
					lockCell2.Hide();
					count--;
				}
			}
			yield return new WaitForSeconds(FadeInOutTick);
			yield return null;
		}
		LockpickingCell[] cells3 = Cells;
		foreach (LockpickingCell lockpickingCell in cells3)
		{
			if (lockpickingCell.State == LockpickingCell.StateE.Showed)
			{
				lockpickingCell.Hide();
			}
		}
		_showAllActive = false;
	}

	private void LoseGame()
	{
		RightMask.localPosition = new Vector3(512f, RightMask.localPosition.y, RightMask.localPosition.z);
		GoToHell();
		if (Globals.IsDebugBuild)
		{
			Debug.Log("[YOU LOSE!]");
		}
		PostMessage(ServerData.PhrasesE.LockpickingNoMoreLockpicks, FontManager.ColorE.CompareRed);
		ButtonContinue.SetActiveRecursivelyMk1(setActive: true);
		StartCoroutine("WaitForever");
	}

	private IEnumerator WaitForever()
	{
		while (true)
		{
			yield return null;
		}
	}

	private void CheckWinCondition()
	{
		bool flag = true;
		LockpickingCell[] cells = Cells;
		foreach (LockpickingCell lockpickingCell in cells)
		{
			if (lockpickingCell.State == LockpickingCell.StateE.Hidden || lockpickingCell.State == LockpickingCell.StateE.Showed)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			_win = true;
			Messenger<int, int>.Invoke(Globals.Msg_ChestOpened, _chest.Type, _startLockpicksCount - SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount);
			Messenger.Invoke(Globals.Msg_GotoChestGameFromLocation, _chest.Type);
			StartCoroutine("YouWin");
		}
	}

	private IEnumerator YouWin()
	{
		SingletonT<SoundManager>.I.PlaySuccessSound();
		RightMask.localPosition = new Vector3(512f, RightMask.localPosition.y, RightMask.localPosition.z);
		yield return new WaitForSeconds(1f);
		PostMessage(ServerData.PhrasesE.LockpickingPrize, FontManager.ColorE.CompareGreen);
		LockpickingCell[] cells = Cells;
		foreach (LockpickingCell lockCell in cells)
		{
			if (lockCell.State == LockpickingCell.StateE.Opened)
			{
				lockCell.CloseForever(1f);
			}
		}
		yield return new WaitForSeconds(1f);
		GoToHell();
		Animation animation = LockBg.GetComponent<Animation>();
		GetComponent<Animation>()Play();
		float animationTime = 0f;
		ChestBg.localScale = default(Vector3);
		ChestBg.SetAlpha(0.7f);
		ChestBg.gameObject.active = true;
		while (animationTime < GetComponent<Animation>()clip.length)
		{
			Vector3 scale = Vector3.Lerp(Vector3.zero, Vector3.one, animationTime / GetComponent<Animation>()clip.length);
			animationTime += Time.deltaTime;
			ChestBg.localScale = scale;
			yield return null;
		}
		ChestBg.localScale = Vector3.one;
		LockBg.gameObject.active = false;
		if (_chest != null && _chest.Bonus != null)
		{
			List<ServerData.Bonus.DropElement> loot = _chest.Bonus.GetRandomDrop();
			StartCoroutine("ShowLoot", loot);
		}
		yield return new WaitForSeconds(1f);
		_winCallback();
	}

	private IEnumerator ShowLoot(List<ServerData.Bonus.DropElement> loot)
	{
		int maxItemsPerRow = 3;
		int rowsCount = loot.Count / maxItemsPerRow;
		rowsCount += ((loot.Count % maxItemsPerRow > 0) ? 1 : 0);
		int itemSize = 200;
		int index = 0;
		for (int y = 0; y < rowsCount; y++)
		{
			int remains = loot.Count - y * maxItemsPerRow;
			remains = ((remains <= maxItemsPerRow) ? remains : maxItemsPerRow);
			for (int x = 0; x < remains; x++)
			{
				index = y * maxItemsPerRow + x;
				GameObject button = (GameObject)UnityEngine.Object.Instantiate(LootProto);
				ExtraItemPreview chestButton = button.GetComponentInChildren<ExtraItemPreview>();
				button.transform.parent = LeftRoot.transform;
				chestButton.SetLoot(loot[index]);
				chestButton.SetSelected();
				ServerData.Bonus.DropElement oneLoot = loot[index];
				if (oneLoot.Item.IsMoney())
				{
					oneLoot.Item.GetMoneyTypeFromItem().ChangePlayerFundsCount(oneLoot.Count);
				}
				else
				{
					SingletonT<ServerData>.I.AddToBag(oneLoot);
				}
				Messenger<ServerData.Item>.Invoke(Globals.MsgItemFoundInDrop, oneLoot.Item);
				button.name = $"lockpicking_button_{SpriteGui.UniqueId:00}";
				chestButton.Init();
				int offsetX = (int)((float)(-remains * itemSize) / 2f);
				int offsetY = rowsCount * itemSize / 2;
				button.transform.localPosition = new Vector3(offsetX + x * itemSize, offsetY - y * itemSize, -50f);
				_lootButtons.Push(button);
			}
		}
		ButtonContinue.SetActiveRecursivelyMk1(setActive: true);
		yield return null;
	}
}
