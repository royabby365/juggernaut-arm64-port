using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class Lockpicking : SpriteGui
{
	private CompositeDisposable _subscriptions;

	public string assetPrefix = "lockpicking";

	public string defaultLoc = "en";

	public float ShowTime = 5f;

	public SpriteText Title;

	public SpriteText LockpickCount;

	public SpriteText HelpTop;

	public SpriteText HelpBottom;

	public SpriteText Messages;

	public Transform MessagesBg;

	public Transform MessageIco;

	public LockCell[] Cells;

	public Vector2[] SmallCoords;

	public Vector2[] MediumCoords;

	public Vector2[] LargeCoords;

	public Vector3 HellCoord = new Vector3(0f, 1000f, 0f);

	public float FadeInOutTick = 0.3f;

	public AnimationCurve InCurve;

	public AnimationCurve InCurveLean;

	public AnimationCurve OutCurve;

	public AnimationCurve OutToZero;

	public GameObject lootProto;

	public GameObject itemButtonProto;

	public Transform lootLeftTop;

	private readonly Vector3 lootIconPlace = new Vector3(77f, 0f, 0f);

	public Transform LockBg;

	public Transform ChestBg;

	private string _helpBottomFormat;

	private readonly int[] _iconIdxs = new int[27];

	private bool _showAllActive;

	private bool _win;

	private Action _winCallback;

	private Action _loseCallback;

	private LockCell _onHold;

	private LockCell _wrongCell1;

	private LockCell _wrongCell2;

	private LockCell _rightCell1;

	private LockCell _rightCell2;

	private ServerData.Chest _chest;

	private string _timerFormat;

	private LockCell OnHold
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
					base.transform.root.GetComponent<Lockpicking>().PostMessage(ServerData.PhrasesE.LockpickingFindAMatch, FontManager.ColorE.LockMessageGold);
					MessageIco.GetComponent<MeshFilter>().mesh.SetUV(value.GetUvRect());
				}
				_onHold = value;
			}
		}
	}

	private LockCell RightCell1
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

	private LockCell RightCell2
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

	private LockCell WrongCell1
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

	private LockCell WrongCell2
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

	public void MakeGame(ServerData.Chest chest, Action winCallback, Action loseCallback)
	{
		_winCallback = winCallback;
		_loseCallback = loseCallback;
		_chest = chest;
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
	}

	private void Awake()
	{
		string language = UnityApi.GetLanguage();
		if (language != defaultLoc)
		{
		}
		for (int i = 0; i < _iconIdxs.Length; i++)
		{
			_iconIdxs[i] = i + 1;
		}
		_iconIdxs.ShuffleInPlace();
		ClearMessage();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
		StopAllCoroutines();
	}

	private void Start()
	{
		Init();
		foreach (SpriteButton value in _buttons.Values)
		{
			value.SetActive();
		}
		RegenerateAtlas();
		base.Release += ProcessButtons;
		_helpBottomFormat = HelpBottom.Text_;
		HelpBottom.Phrase_ = ServerData.PhrasesE.Custom;
		HelpBottom.Text_ = string.Format(_helpBottomFormat, 5, 10);
		_timerFormat = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.LockpickingSecBeforeGame);
	}

	private void Update()
	{
		ProcessRayCast();
	}

	private void LateUpdate()
	{
	}

	private IEnumerator YouWin()
	{
		SingletonT<SoundManager>.I.PlaySuccessSound();
		yield return new WaitForSeconds(1f);
		PostMessage(ServerData.PhrasesE.LockpickingPrize, FontManager.ColorE.CompareGreen);
		LockBg.GetComponent<Animation>().Play();
		yield return new WaitForSeconds(2f);
		LockBg.gameObject.active = false;
		ChestBg.SetAlpha(0.7f);
		ChestBg.gameObject.active = true;
		LockCell[] cells = Cells;
		foreach (LockCell lockCell in cells)
		{
			if (lockCell.State == LockCell.StateE.Opened)
			{
				lockCell.CloseForever(1f);
			}
		}
		yield return new WaitForSeconds(1f);
		GoToHell();
		if (_chest != null && _chest.Bonus != null)
		{
			List<ServerData.Bonus.DropElement> loot = _chest.Bonus.GetRandomDrop();
			foreach (ServerData.Bonus.DropElement c in loot)
			{
				SingletonT<ServerData>.I.AddToBag(c);
			}
			StartCoroutine("ShowLoot", loot);
		}
		yield return new WaitForSeconds(1f);
		_winCallback();
	}

	private IEnumerator ShowLoot(List<ServerData.Bonus.DropElement> loot)
	{
		for (int index = 0; index < loot.Count; index++)
		{
			ServerData.Bonus.DropElement dropElement = loot[index];
			int _lootCount = dropElement.Count;
			if (!dropElement.IsItem)
			{
				GameObject lootItem = (GameObject)UnityEngine.Object.Instantiate(lootProto);
				Transform icotr = lootItem.transform.FindChildByName("resource_ico", includeInactive: true);
				if (icotr != null)
				{
					icotr.GetComponent<MeshRenderer>().material.mainTexture = SingletonT<ResourcesManager>.I.LoadItemIcon(dropElement);
				}
				Transform badgetr = lootItem.transform.FindChildByName("count_bg", includeInactive: true);
				badgetr.gameObject.SetActiveRecursivelyMk1(_lootCount != 1);
				if (badgetr.gameObject.active)
				{
					Transform counttr = badgetr.FindChildByName("count", includeInactive: true);
					SpriteText count = counttr.GetComponent<SpriteText>();
					count.Text_ = _lootCount.ToString();
				}
				lootItem.transform.parent = lootLeftTop;
				lootItem.transform.SetLayerRecursively(lootLeftTop);
				lootItem.transform.localPosition = index * lootIconPlace;
			}
			else
			{
				GameObject lootItem = (GameObject)UnityEngine.Object.Instantiate(itemButtonProto);
				InventoryItemButton itemButton = lootItem.transform.Find("bag_item").GetComponent<InventoryItemButton>();
				itemButton.name = "chest_loot_" + SpriteGui.UniqueId;
				lootItem.transform.parent = lootLeftTop;
				lootItem.transform.SetLayerRecursively(lootLeftTop);
				lootItem.transform.localPosition = index * lootIconPlace;
				itemButton.shopItem = dropElement.Item;
				itemButton.renderer.material.mainTexture = SingletonT<ResourcesManager>.I.LoadItemIcon(dropElement.Item);
				itemButton.RemoveNew();
				itemButton.Init();
				itemButton.SetActive();
				yield return new WaitForSeconds(0.5f);
			}
		}
		yield return null;
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
		LockCell[] cells = Cells;
		foreach (LockCell lockCell in cells)
		{
			if (lockCell.State == LockCell.StateE.Hidden)
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
			LockCell[] cells2 = Cells;
			foreach (LockCell lockCell2 in cells2)
			{
				if (lockCell2.State == LockCell.StateE.Showed && (count <= minCount || loop >= 4 || 3.Dice0()))
				{
					lockCell2.Hide();
					count--;
				}
			}
			yield return new WaitForSeconds(FadeInOutTick);
			yield return null;
		}
		_showAllActive = false;
	}

	private void Init()
	{
		LockCell[] cells = Cells;
		foreach (LockCell lockCell in cells)
		{
			lockCell.Init(-10, -10);
		}
	}

	private void GoToHell()
	{
		LockCell[] cells = Cells;
		foreach (LockCell lockCell in cells)
		{
			lockCell.transform.localPosition = HellCoord;
			lockCell.SetCell(0);
			lockCell.State = LockCell.StateE.Inactive;
		}
	}

	private void ClearMessage()
	{
		Messages.Phrase_ = ServerData.PhrasesE.Custom;
		Messages.Text_ = string.Empty;
		MessageIco.gameObject.active = false;
		MessagesBg.gameObject.active = false;
	}

	private void PostMessage(string fmt, int value, FontManager.ColorE color)
	{
		Messages.Phrase_ = ServerData.PhrasesE.Custom;
		Messages.Text_ = string.Format(fmt, value);
		Messages.NamedColorE_ = color;
		MessageIco.gameObject.active = false;
		MessagesBg.gameObject.active = true;
	}

	private void PostMessage(string message, FontManager.ColorE color)
	{
		Messages.Phrase_ = ServerData.PhrasesE.Custom;
		Messages.Text_ = message;
		Messages.NamedColorE_ = color;
		MessageIco.gameObject.active = false;
		MessagesBg.gameObject.active = true;
	}

	private void PostMessage(ServerData.PhrasesE phrase, FontManager.ColorE color)
	{
		Messages.Phrase_ = phrase;
		Messages.NamedColorE_ = color;
		MessageIco.gameObject.active = false;
		MessagesBg.gameObject.active = true;
	}

	private void ShowAll()
	{
		StartCoroutine("ShowAllCoro");
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
			LockCell lockCell = Cells[num3];
			LockCell lockCell2 = Cells[num + num3];
			lockCell.State = LockCell.StateE.Hidden;
			lockCell.State = LockCell.StateE.Hidden;
			lockCell.SetCell(cell);
			lockCell2.SetCell(cell);
			lockCell.transform.localPosition = coords[num3];
			lockCell2.transform.localPosition = coords[num + num3];
		}
	}

	private void CheckWinCondition()
	{
		bool flag = true;
		LockCell[] cells = Cells;
		foreach (LockCell lockCell in cells)
		{
			if (lockCell.State == LockCell.StateE.Hidden || lockCell.State == LockCell.StateE.Showed)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			_win = true;
			StartCoroutine("YouWin");
		}
	}

	private void LoseGame()
	{
		GoToHell();
		if (Globals.IsDebugBuild)
		{
			Debug.Log("[YOU LOSE!]");
		}
		PostMessage(ServerData.PhrasesE.LockpickingNoMoreLockpicks, FontManager.ColorE.CompareRed);
		StartCoroutine("WaitForever");
	}

	private IEnumerator WaitForever()
	{
		while (true)
		{
			yield return null;
		}
	}

	private void ProcessButtons(SpriteButton button)
	{
		if (button.name == "close_button")
		{
			_loseCallback();
		}
		else
		{
			if (_showAllActive || _win)
			{
				return;
			}
			if (button.name == "show_cells")
			{
				UseLockpicks(10);
				ShowAll();
				return;
			}
			LockCell lockCell = button as LockCell;
			if (!(lockCell != null))
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
				if (lockCell == WrongCell1)
				{
					WrongCell1 = null;
					WrongCell2.Hide();
					WrongCell2 = null;
				}
				else if (lockCell == WrongCell2)
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
					lockCell.Show();
				}
				OnHold = lockCell;
			}
			else if (!(lockCell == OnHold))
			{
				if (lockCell.CellIdx == OnHold.CellIdx)
				{
					LockCell onHold = OnHold;
					OnHold = null;
					RightCell1 = onHold;
					RightCell2 = lockCell;
					PostMessage(ServerData.PhrasesE.LockpickingMatchFound, FontManager.ColorE.CompareGreen);
					SingletonT<SoundManager>.I.PlaySuccessSound();
					CheckWinCondition();
				}
				else
				{
					UseLockpicks(1);
					LockCell onHold2 = OnHold;
					OnHold = null;
					SingletonT<SoundManager>.I.PlayFailSound();
					WrongCell1 = lockCell;
					WrongCell2 = onHold2;
					lockCell.Show();
				}
			}
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
			LockpickCount.Text_ = SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount.ToString();
			LoseGame();
			return;
		}
		SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount -= count;
		LockpickCount.Text_ = SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount.ToString();
		if (count == 1)
		{
			base.transform.root.GetComponent<Lockpicking>().PostMessage(ServerData.PhrasesE.LockpickingWrongMatch, FontManager.ColorE.CompareRed);
		}
		else
		{
			base.transform.root.GetComponent<Lockpicking>().ClearMessage();
		}
	}
}
