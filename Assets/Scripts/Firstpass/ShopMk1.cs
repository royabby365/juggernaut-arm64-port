using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class ShopMk1 : MonoBehaviour
{
	private enum Pointers
	{
		Pointer01,
		Pointer02,
		Pointer03,
		Pointer04,
		Pointer05
	}

	private const string Division = "_shop_division_";

	private const int ShopSlotsPitch = 300;

	private const int _pointer1 = 0;

	private const float MaxSpeed = 4000f;

	private const float DefaultFriction = 2500f;

	private const float UnpenetrateTime = 0.4f;

	private const float LongestTime = 2f;

	private const int RecomendedAbsolutePointerShift = 1000000;

	private CompositeDisposable _subscriptions;

	public static bool RecommendationsNeedRegeneration;

	public GameObject ShopItemProto;

	public Transform ItemsRoot;

	public Collider SliceCollider;

	public SpriteButton[] Divisions;

	public Transform RightVertical;

	public static readonly HashSet<ServerData.ShopGood> RareSet = new HashSet<ServerData.ShopGood>();

	public static readonly HashSet<ServerData.ShopGood> EpicSet = new HashSet<ServerData.ShopGood>();

	public static readonly HashSet<ServerData.ShopGood> HalfEpicSet = new HashSet<ServerData.ShopGood>();

	public static readonly HashSet<ServerData.ShopGood> AlchemySet = new HashSet<ServerData.ShopGood>();

	public static readonly HashSet<ServerData.ShopGood> LegendarySet = new HashSet<ServerData.ShopGood>();

	public static readonly HashSet<int> LegendaryRingIds = new HashSet<int> { 331, 339, 325, 348, 342, 323, 345, 317 };

	private readonly List<ShopItemMk1> _shopList = new List<ShopItemMk1>();

	private readonly List<ShopItemMk1> _recomendedList = new List<ShopItemMk1>();

	private int _pointer2;

	private int _pointer3;

	private int _pointer4;

	private int _pointer5;

	public AnimationCurve DecelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private int _minScrollLoc;

	private int _maxScrollLoc;

	private float _scrollSpeed;

	private Vector3 _scrollBegin;

	private bool _deceleration;

	private float _startTime;

	private float _stopTime;

	private Vector3 _startPos;

	private Vector3 _stopPos;

	private SpriteGui _gui;

	private bool _doNotUpdateSwitches;

	private Action _changePositionToPointer;

	private readonly HashSet<ShopItemMk1> _toFilterFromShopList = new HashSet<ShopItemMk1>();

	private void ProcessButtons(SpriteButton button)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Shop)
		{
			return;
		}
		if (button.name.Contains("epic_item_info_"))
		{
			Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowAlert, ServerData.PhrasesE.EpicItemInfo);
		}
		else if (button.name.Contains("_shop_division_"))
		{
			SpriteButton[] divisions = Divisions;
			foreach (SpriteButton spriteButton in divisions)
			{
				spriteButton.SetUnselected();
			}
			button.SetSelected();
			if (button.name.Contains("1"))
			{
				ChangePositionToPointer(Pointers.Pointer01, 0);
			}
			else if (button.name.Contains("2"))
			{
				ChangePositionToPointer(Pointers.Pointer02, 0);
			}
			else if (button.name.Contains("3"))
			{
				ChangePositionToPointer(Pointers.Pointer03, 0);
			}
			else if (button.name.Contains("4"))
			{
				ChangePositionToPointer(Pointers.Pointer04, 0);
			}
			else if (button.name.Contains("5"))
			{
				ChangePositionToPointer(Pointers.Pointer05, 0);
			}
			Messenger<string>.Invoke(Globals.MsgShopFilterChanged, button.name);
		}
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger.AddListener(Globals.MsgShopGoodsReseted, OnShopGoodsReseted));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgShopGoodsNeedStateRefresh, delegate
		{
			RefreshShopGoodItems(needRewind: false);
		}));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgBagRefreshFinished, OnBagRefreshFinished));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgInsufficientFunds, OnInsufficientFunds));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgShopRecommendationsChanged, delegate
		{
			ChangePositionToPointer(Pointers.Pointer01, 0);
		}));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgShopScarabInsufficient, OnLowElixirType(ServerData.Item.ElixirTypeE.Scarab)));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgShopSkullInsufficient, OnLowElixirType(ServerData.Item.ElixirTypeE.Skull)));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgShopStarsInsufficient, OnLowElixirType(ServerData.Item.ElixirTypeE.Star)));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgShopEpicsInsufficient, OnLowEpicArmor));
		_subscriptions.Add(Messenger<int>.AddListener(Globals.MsgShopChangePointer, OnChangeShopAbsolutePointer));
		_subscriptions.Add(Messenger<int>.AddListener(Globals.MsgShopChangeFilter, delegate(int filter)
		{
			switch (filter)
			{
			case 1:
				ChangePositionToPointer(Pointers.Pointer01, 0);
				break;
			case 2:
				ChangePositionToPointer(Pointers.Pointer02, 0);
				break;
			case 3:
				ChangePositionToPointer(Pointers.Pointer03, 0);
				break;
			case 4:
				ChangePositionToPointer(Pointers.Pointer04, 0);
				break;
			case 5:
				ChangePositionToPointer(Pointers.Pointer05, 0);
				break;
			case 10:
				ChangePositionToPointer(Pointers.Pointer02, -3);
				break;
			default:
				ChangePositionToPointer(Pointers.Pointer01, 0);
				break;
			}
		}));
		_gui = base.transform.GetSpriteGui();
		_gui.MoveBegin += GuiOnMoveBegin;
		_gui.MoveEnd += GuiOnMoveEnd;
		_gui.Move += GuiOnMove;
	}

	private Callback OnLowElixirType(ServerData.Item.ElixirTypeE etype)
	{
		return delegate
		{
			foreach (ShopItemMk1 shop in _shopList)
			{
				if (shop.ShopGood.Item.ElixirType == etype)
				{
					ChangePositionToPointer(Pointers.Pointer02, shop.AbsolutePointer);
					return;
				}
			}
			ChangePositionToPointer(Pointers.Pointer01, 0);
		};
	}

	private void OnChangeShopAbsolutePointer(int i)
	{
		if (i >= 1000000)
		{
			ChangePositionToPointer(Pointers.Pointer01, i - 1000000);
		}
		else
		{
			ChangePositionToPointer(Pointers.Pointer02, i);
		}
	}

	private void OnLowEpicArmor()
	{
		foreach (ShopItemMk1 shop in _shopList)
		{
			if (shop.ShopGood.IsEpicShopGood())
			{
				ChangePositionToPointer(Pointers.Pointer02, shop.AbsolutePointer);
				break;
			}
		}
	}

	private void OnInsufficientFunds()
	{
		if (!(HudMk1.Instance == null))
		{
			Messenger<ServerData.PhrasesE, ServerData.PhrasesE, ServerData.PhrasesE, Action>.Invoke(Globals.MsgPopup2ButtonYesHandler, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, ServerData.PhrasesE.InsufficientFunds, delegate
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Bank);
			});
		}
	}

	private void OnBagRefreshFinished()
	{
		foreach (ShopItemMk1 recomended in _recomendedList)
		{
			recomended.UpdateCompare();
		}
		foreach (ShopItemMk1 shop in _shopList)
		{
			shop.UpdateCompare();
		}
	}

	private void GuiOnMove(Vector3 begin, Vector3 end)
	{
		if (!(_gui == null) && _gui.CheckCollider(SliceCollider, begin))
		{
			float x = (end - begin).x;
			x *= Camera2D.Scale;
			_scrollSpeed = x / Time.deltaTime;
			Vector3 localPosition = ItemsRoot.localPosition;
			ItemsRoot.localPosition = new Vector3(localPosition.x + (float)x.RoundToInt(), localPosition.y, localPosition.z);
			UpdateSwitchSelectors();
		}
	}

	private void ChangePositionToPointer(Pointers pe, int offset)
	{
		_changePositionToPointer = delegate
		{
			int num = offset;
			int num2 = -(pe switch
			{
				Pointers.Pointer01 => num, 
				Pointers.Pointer02 => num + _pointer2, 
				Pointers.Pointer03 => num + _pointer3, 
				Pointers.Pointer04 => num + _pointer4, 
				Pointers.Pointer05 => num + _pointer5, 
				_ => throw new ArgumentOutOfRangeException("pe"), 
			}) * 300;
			_startTime = Time.time;
			Vector3 localPosition = ItemsRoot.localPosition;
			_startPos = localPosition;
			_stopPos = new Vector3(num2, localPosition.y, localPosition.z);
			float a = 0.4f * Mathf.Max(1f, Mathf.Abs((_stopPos - _startPos).x / (float)Camera2D.ScreenWidth));
			_stopTime = Time.time + Mathf.Min(a, 2f);
			_deceleration = true;
			_doNotUpdateSwitches = true;
		};
		_changePositionToPointer();
	}

	private void DebugPointers()
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log(" == @{5} 1:{0} 2:{1} 3:{2} 4:{3} 5:{4}==".Fmt(0, _pointer2, _pointer3, _pointer4, _pointer5, Time.frameCount));
		}
		if (Globals.IsDebugBuild)
		{
			Debug.Log("======== min:{0} max:{1} speed:{2} start:{3} stop:{4}".Fmt(_minScrollLoc, _maxScrollLoc, _scrollSpeed, _startPos, _stopPos));
		}
	}

	private void GuiOnMoveEnd(Vector3 vector3)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Shop)
		{
			return;
		}
		Vector3 localPosition = ItemsRoot.localPosition;
		_startTime = Time.time;
		_startPos = localPosition;
		_deceleration = true;
		if (localPosition.x < (float)_minScrollLoc)
		{
			_stopPos = new Vector3(_minScrollLoc, localPosition.y, localPosition.z);
			_stopTime = _startTime + 0.4f;
		}
		else if (localPosition.x > (float)_maxScrollLoc)
		{
			_stopPos = new Vector3(_maxScrollLoc, localPosition.y, localPosition.z);
			_stopTime = _startTime + 0.4f;
		}
		else if (!_scrollSpeed.Eqv(0f))
		{
			float a = Mathf.Abs(_scrollSpeed);
			float num = Mathf.Sign(_scrollSpeed);
			a = Mathf.Min(a, 4000f);
			float num2 = ((!(num < 0f)) ? Mathf.Abs((float)_maxScrollLoc - localPosition.x) : Mathf.Abs(localPosition.x - (float)_minScrollLoc));
			float num3 = a / 2500f;
			float num4 = Mathf.Round(a * num3 - 2500f * num3 * num3 / 2f);
			if (num4 > num2)
			{
				num3 /= num4 / num2;
				num4 = num2;
			}
			_stopTime = _startTime + num3;
			_stopPos = localPosition + new Vector3(num * num4, 0f, 0f);
		}
		else
		{
			_deceleration = false;
		}
		_scrollSpeed = 0f;
	}

	private void GuiOnMoveBegin(Vector3 begin)
	{
		if (!(_gui == null) && _gui.CheckCollider(SliceCollider, begin))
		{
			_deceleration = false;
			_doNotUpdateSwitches = false;
		}
	}

	private void Update()
	{
		if (_deceleration)
		{
			float time = Time.time;
			if (time <= _stopTime)
			{
				float num = _stopTime - _startTime;
				float num2 = time - _startTime;
				ItemsRoot.localPosition = Vector3.Lerp(_startPos, _stopPos, DecelerationCurve.Evaluate(num2 / num));
			}
			else
			{
				ItemsRoot.localPosition = _stopPos;
				_deceleration = false;
				_doNotUpdateSwitches = false;
			}
			if (!_doNotUpdateSwitches)
			{
				UpdateSwitchSelectors();
			}
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		SpriteGui spriteGui = base.transform.GetSpriteGui();
		spriteGui.Release += ProcessButtons;
		SpriteButton[] divisions = Divisions;
		foreach (SpriteButton spriteButton in divisions)
		{
			spriteButton.SetUnselected();
		}
	}

	private void UpdateSwitchSelectors()
	{
		int num = Mathf.Abs(ItemsRoot.localPosition.x / 300f).RoundToInt();
		SpriteButton[] divisions = Divisions;
		foreach (SpriteButton spriteButton in divisions)
		{
			spriteButton.SetUnselected();
		}
		if (num >= _pointer5)
		{
			Divisions[4].SetSelected();
		}
		else if (num >= _pointer4)
		{
			Divisions[3].SetSelected();
		}
		else if (num >= _pointer3)
		{
			Divisions[2].SetSelected();
		}
		else if (num >= _pointer2)
		{
			Divisions[1].SetSelected();
		}
		else
		{
			Divisions[0].SetSelected();
		}
	}

	private static bool OtherGoods(ShopItemMk1 shopItemMk1)
	{
		int num = ServerData.ShopGoodPriority(shopItemMk1.ShopGood);
		return num >= 3000000 && num < 4000000;
	}

	private static bool Premium(ShopItemMk1 shopItemMk1)
	{
		int num = ServerData.ShopGoodPriority(shopItemMk1.ShopGood);
		return num >= 4000000 && num < 100000000;
	}

	private bool Relic(ShopItemMk1 shopItemMk1)
	{
		return ServerData.ShopGoodPriority(shopItemMk1.ShopGood) >= 100000000;
	}

	private bool Recommended(ServerData.ShopGood shopGood)
	{
		if (shopGood.Relict)
		{
			return false;
		}
		if (ServerData.RecommendedOnly(shopGood))
		{
			return true;
		}
		int level = SingletonT<ServerData>.I.PlayerParams.Level;
		Tuple<ServerData.MoneyType.TypeE, int, string> itemBuyPrice = shopGood.GetItemBuyPrice();
		if (itemBuyPrice.Item1 == ServerData.MoneyType.TypeE.Diamond)
		{
			return false;
		}
		return shopGood.LevelMin <= level && shopGood.LevelMax >= level && !shopGood.PlayerHasBetter() && !shopGood.PlayerHasIt();
	}

	private void OnShopGoodsReseted()
	{
		List<ServerData.ShopGood> list = SingletonT<ServerData>.I.InShopGoods.ToList();
		list.Sort((ServerData.ShopGood l, ServerData.ShopGood r) => ServerData.ShopGoodPriority(l).CompareTo(ServerData.ShopGoodPriority(r)));
		List<ServerData.ShopGood> list2 = list.Where((ServerData.ShopGood x) => !ServerData.RecommendedOnly(x)).ToList();
		bool needRewind = false;
		_toFilterFromShopList.Clear();
		if (_shopList.Count != list2.Count)
		{
			needRewind = true;
			CleanShopList();
			RareSet.Clear();
			EpicSet.Clear();
			HalfEpicSet.Clear();
			AlchemySet.Clear();
			LegendarySet.Clear();
			foreach (ServerData.ShopGood item in list2)
			{
				AddOneShopGoodItem(_shopList, item);
				PossiblyAddToSet(item);
			}
		}
		List<ServerData.ShopGood> list3 = list.Where(Recommended).ToList();
		_pointer2 = list3.Count;
		HashSet<ServerData.ShopGood> hashSet = new HashSet<ServerData.ShopGood>(_recomendedList.Select((ShopItemMk1 x) => x.ShopGood));
		HashSet<ServerData.ShopGood> hashSet2 = new HashSet<ServerData.ShopGood>(list3);
		bool flag = hashSet.SetEquals(hashSet2);
		if (flag && Globals.IsDebugBuild)
		{
			Debug.Log("======================== SKIP RECOMMENDED REGENERATION ====================");
		}
		if (!flag)
		{
			CleanRecommended();
			foreach (ServerData.ShopGood item2 in list3)
			{
				AddOneShopGoodItem(_recomendedList, item2);
			}
		}
		ShopItemMk1 recomShopItem;
		foreach (ShopItemMk1 recomended in _recomendedList)
		{
			recomShopItem = recomended;
			ShopItemMk1 shopItemMk = _shopList.Find((ShopItemMk1 al) => al.ShopGood.GetPrice(ServerData.MoneyType.TypeE.Diamond) > 0 && al.ShopGood.Item.Id == recomShopItem.ShopGood.Item.Id);
			if (shopItemMk != null)
			{
				recomShopItem.AddAlternateShopItem(shopItemMk);
				_toFilterFromShopList.Add(shopItemMk);
			}
		}
		ShopItemMk1 shopGood;
		foreach (ShopItemMk1 shop in _shopList)
		{
			shopGood = shop;
			if (shopGood.ShopGood.GetPrice(ServerData.MoneyType.TypeE.Diamond) <= 0)
			{
				ShopItemMk1 shopItemMk2 = _shopList.Find((ShopItemMk1 al) => al.ShopGood.GetPrice(ServerData.MoneyType.TypeE.Diamond) > 0 && al.ShopGood.Item.Id == shopGood.ShopGood.Item.Id);
				if (shopItemMk2 != null)
				{
					shopGood.AddAlternateShopItem(shopItemMk2);
					_toFilterFromShopList.Add(shopItemMk2);
				}
			}
		}
		RefreshShopGoodItems(needRewind);
		if (_changePositionToPointer != null)
		{
			_changePositionToPointer();
			_changePositionToPointer = null;
		}
	}

	private static void PossiblyAddToSet(ServerData.ShopGood shopGood)
	{
		if (shopGood.Item.IsArmor)
		{
			if (shopGood.Item.Set == 2)
			{
				RareSet.Add(shopGood);
			}
			else if (shopGood.Item.Set == 3 && shopGood.GetPrice(ServerData.MoneyType.TypeE.Diamond) > 0)
			{
				EpicSet.Add(shopGood);
				ServerData.Slot.TypeE slotId = shopGood.Item.Slot.SlotId;
				if (slotId == ServerData.Slot.TypeE.Boots || slotId == ServerData.Slot.TypeE.Helm || slotId == ServerData.Slot.TypeE.Pelvis || slotId == ServerData.Slot.TypeE.HandLeft)
				{
					HalfEpicSet.Add(shopGood);
				}
			}
			else if (shopGood.Item.Set == 4)
			{
				LegendarySet.Add(shopGood);
			}
		}
		if (shopGood.Item.IsBattleElixir())
		{
			AlchemySet.Add(shopGood);
		}
	}

	private void RefreshShopGoodItems(bool needRewind)
	{
		int num = 0;
		foreach (ShopItemMk1 recomended in _recomendedList)
		{
			int num2 = num * 300;
			recomended.RefreshState(1000000 + num);
			num++;
			recomended.transform.localPosition = Vector3.zero;
			recomended.transform.localPosition += num2 * Vector3.right;
		}
		_pointer2 = _recomendedList.Count;
		_pointer3 = -1;
		_pointer4 = -1;
		_pointer5 = -1;
		int num3 = 0;
		foreach (ShopItemMk1 shop in _shopList)
		{
			if (_toFilterFromShopList.Contains(shop))
			{
				shop.transform.GoToHell();
				shop.gameObject.SetActiveRecursively(state: false);
				continue;
			}
			int num4 = num * 300;
			shop.RefreshState(num3);
			num++;
			num3++;
			shop.transform.localPosition = Vector3.zero;
			shop.transform.localPosition += num4 * Vector3.right;
			if (_pointer3 < 0 && OtherGoods(shop))
			{
				_pointer3 = num - 1;
			}
			else if (_pointer4 < 0 && Premium(shop))
			{
				_pointer4 = num - 1;
			}
			else if (_pointer5 < 0 && Relic(shop))
			{
				_pointer5 = num - 1;
			}
		}
		UpdateMinMax(num);
		if (needRewind)
		{
			ChangePositionToPointer(Pointers.Pointer01, 0);
		}
		RightVertical.localPosition = new Vector3(num * 300, 0f, 0f);
	}

	private void UpdateMinMax(int count)
	{
		_minScrollLoc = -count * 300 + Camera2D.ScreenWidth;
		_maxScrollLoc = 0;
	}

	private void AddOneShopGoodItem(List<ShopItemMk1> toList, ServerData.ShopGood inShopGood)
	{
		ShopItemMk1 shopItemMk = Utils.Instaniate<ShopItemMk1>(ShopItemProto);
		toList.Add(shopItemMk);
		shopItemMk.transform.parent = base.transform;
		shopItemMk.transform.SetLayerRecursively(base.transform);
		shopItemMk.ShopGood = inShopGood;
	}

	private void CleanShopList()
	{
		foreach (ShopItemMk1 shop in _shopList)
		{
			EleminateShopItem(shop);
		}
		_shopList.Clear();
	}

	private void CleanRecommended()
	{
		foreach (ShopItemMk1 recomended in _recomendedList)
		{
			EleminateShopItem(recomended);
		}
		_recomendedList.Clear();
	}

	private static void EleminateShopItem(ShopItemMk1 shopItemMk1)
	{
		shopItemMk1.ButtonBuy.UnregisterMe();
		shopItemMk1.ButtonBuyForGold.UnregisterMe();
		shopItemMk1.ButtonBuyForDiamonds.UnregisterMe();
		if (shopItemMk1.EpicInfoButton != null)
		{
			shopItemMk1.EpicInfoButton.UnregisterMe();
		}
		shopItemMk1.Eliminate();
	}

	public ShopItemMk1 GetRandomRecommended()
	{
		List<ShopItemMk1> list = new List<ShopItemMk1>();
		int num = 0;
		foreach (ShopItemMk1 recomended in _recomendedList)
		{
			if (recomended.ShopGood.Item.IsArmorOrWeapon)
			{
				if (!recomended.ShopGood.PlayerHasIt() && !recomended.ShopGood.PlayerHasBetter())
				{
					list.Add(recomended);
				}
				else
				{
					num++;
				}
				continue;
			}
			break;
		}
		if (list.Count > 0)
		{
			return _recomendedList[UnityEngine.Random.Range(0, list.Count)];
		}
		return _recomendedList[UnityEngine.Random.Range(num, _recomendedList.Count)];
	}

	public ShopItemMk1 GetRandomPremium()
	{
		List<ShopItemMk1> list = new List<ShopItemMk1>();
		foreach (ShopItemMk1 shop in _shopList)
		{
			ServerData.ShopGood shopGood = shop.ShopGood;
			if (shopGood.Item.IsItemSet() && !shopGood.PlayerHasIt())
			{
				list.Add(shop);
			}
		}
		if (list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			return list[index];
		}
		int num = 0;
		foreach (ShopItemMk1 recomended in _recomendedList)
		{
			if (recomended.ShopGood.Item.IsBattleElixir())
			{
				break;
			}
			num++;
		}
		int index2 = UnityEngine.Random.Range(num, _recomendedList.Count);
		return _recomendedList[index2];
	}
}
