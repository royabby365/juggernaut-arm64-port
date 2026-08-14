using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class BagInventory : MonoBehaviour
{
	private const int MinCols = 4;

	private const int VisibleCols = 3;

	private const int BagSlotsPitch = 128;

	private const int Rows = 3;

	private const float MaxSpeed = 4000f;

	private const float DefaultFriction = 4000f;

	private const float UnpenetrateTime = 0.4f;

	private const float LongestTime = 2f;

	private CompositeDisposable _subscriptions;

	public SpriteButton EmptyFilterMessage;

	public GameObject ItemButtonProto;

	public GameObject BagSlotProto;

	public Puppet PuppetScript;

	public Transform BagSlotsRoot;

	public Transform LeftClipObject;

	public Transform RightClipObject;

	public Collider SliceCollider;

	private readonly List<BagItemSlot> _bagSlots = new List<BagItemSlot>();

	private readonly List<BagItemButton> _allBagItems = new List<BagItemButton>();

	private readonly List<BagItemButton> _puppetItems = new List<BagItemButton>();

	private List<BagItemButton> _inventoryItems = new List<BagItemButton>();

	private int _cols;

	private bool _upgradeState;

	public AnimationCurve DecelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

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

	private bool _needRefresh;

	private bool _bagScroll;

	private float _xMax;

	public static BagInventory Instance { get; set; }

	private void Awake()
	{
		_maxScrollLoc = BagSlotsRoot.localPosition.x.RoundToInt();
		Instance = this;
		EmptyFilterMessage.Init();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger.AddListener(Globals.MsgBagNeedRefresh, OnBagNeedRefresh));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnChangeGui));
		SpriteGui spriteGui = (_gui = base.transform.GetSpriteGui());
		spriteGui.Release += StarsUpgradeButton;
		spriteGui.Release += OnBagItemButtonRelease;
		spriteGui.MoveBegin += GuiOnDragBegin;
		spriteGui.MoveEnd += GuiOnDragEnd;
		spriteGui.Move += GuiOnDrag;
	}

	private void StarsUpgradeButton(SpriteButton spriteButton)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (spriteButton.name == "_star_upgrade_info")
		{
			ActivateUpgrade(activate: false);
		}
		else
		{
			if (!(spriteButton.name == "stars_upgrade_button"))
			{
				return;
			}
			if (spriteButton.Selected)
			{
				ActivateUpgrade(activate: false);
				return;
			}
			ActivateUpgrade(activate: true);
			if (_upgradeState && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.BagStats)
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BagItems);
			}
		}
	}

	internal void ActivateUpgrade(bool activate)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		int num = 0;
		PuppetSlot[] puppetSlots = PuppetScript.PuppetSlots;
		foreach (PuppetSlot puppetSlot in puppetSlots)
		{
			BagItemButton putOnItemFromSlot = GetPutOnItemFromSlot(puppetSlot.Slot);
			if (puppetSlot.ActivateUpgrade(putOnItemFromSlot, activate))
			{
				num++;
			}
		}
		_upgradeState = activate && num > 0 && SingletonT<ServerData>.I.PlayerParams.StarsCount > 0;
		if (_upgradeState)
		{
			PuppetScript.ActivateUpgrades.SetSelected();
			EmptyFilterMessage.transform.localScale = Vector3.one;
			EmptyFilterMessage.SetActive();
		}
		else
		{
			PuppetScript.ActivateUpgrades.SetUnselected();
			EmptyFilterMessage.transform.localScale = Vector3.zero;
			EmptyFilterMessage.SetInactive();
		}
		if (activate && !_upgradeState)
		{
			if (num <= 0)
			{
				Messenger<ServerData.PhrasesE, ServerData.PhrasesE, ServerData.PhrasesE, Action>.Invoke(Globals.MsgPopup2ButtonYesHandler, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, ServerData.PhrasesE.NoEpicItems, delegate
				{
					Messenger.Invoke(Globals.MsgShopEpicsInsufficient);
					HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
				});
			}
			else
			{
				Messenger<ServerData.PhrasesE, ServerData.PhrasesE, ServerData.PhrasesE, Action>.Invoke(Globals.MsgPopup2ButtonYesHandler, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, ServerData.PhrasesE.NeedMoreStars, delegate
				{
					Messenger.Invoke(Globals.MsgShopStarsInsufficient);
					HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
				});
				ActivateUpgrade(activate: false);
			}
			PuppetScript.ActivateUpgrades.SetUnselected();
		}
		ShowHideAllItems();
	}

	private void OnChangeGui(GuiRoot.GuiType from, GuiRoot.GuiType to)
	{
		if (to == GuiRoot.GuiType.BagItems)
		{
			ScrollToBegin();
		}
		switch (to)
		{
		case GuiRoot.GuiType.BagStats:
			ActivateUpgrade(activate: false);
			break;
		case GuiRoot.GuiType.BagItems:
			if (from != to && from != GuiRoot.GuiType.BagStats)
			{
				ActivateUpgrade(activate: false);
			}
			break;
		}
	}

	private void OnBagNeedRefresh()
	{
		_needRefresh = true;
	}

	private void GuiOnDrag(Vector3 begin, Vector3 end)
	{
		if (_bagScroll && !(_gui == null) && _gui.CheckCollider(SliceCollider, begin) && !_upgradeState)
		{
			float x = (end - begin).x;
			x *= Camera2D.Scale;
			_scrollSpeed = x / Time.deltaTime;
			Vector3 localPosition = BagSlotsRoot.localPosition;
			BagSlotsRoot.localPosition = new Vector3(localPosition.x + (float)x.RoundToInt(), localPosition.y, localPosition.z);
			ClipInventory();
		}
	}

	private void ScrollToBegin()
	{
		_bagScroll = false;
		Vector3 localPosition = BagSlotsRoot.localPosition;
		_startTime = Time.time;
		_startPos = localPosition;
		_deceleration = true;
		_stopPos = new Vector3(_maxScrollLoc, localPosition.y, localPosition.z);
		_stopTime = _startTime + 0.4f;
	}

	private void GuiOnDragEnd(Vector3 vector3)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.BagItems)
		{
			return;
		}
		_bagScroll = false;
		Vector3 localPosition = BagSlotsRoot.localPosition;
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
			float num3 = a / 4000f;
			float num4 = Mathf.Round(a * num3 - 4000f * num3 * num3 / 2f);
			if (num4 > num2)
			{
				num3 /= num4 / num2;
				num4 = num2;
			}
			num3 = Mathf.Max(num3, 0.4f);
			_stopTime = _startTime + num3;
			float x = num * num4;
			_stopPos = localPosition + new Vector3(x, 0f, 0f);
			if (Globals.IsDebugBuild)
			{
				Debug.Log("=== before correction: {0}".Fmt(_stopPos));
			}
			float x2 = _stopPos.x;
			float num5 = x2;
			x2 /= 128f;
			x2 -= 0.5f;
			x2 = (int)x2;
			x2 *= 128f;
			_stopPos += new Vector3(x2 - num5, 0f, 0f);
		}
		else
		{
			_stopPos = BagSlotsRoot.localPosition;
			if (Globals.IsDebugBuild)
			{
				Debug.Log("=== before correction: {0}".Fmt(_stopPos));
			}
			_stopTime = _startTime + 0.4f;
			float x3 = _stopPos.x;
			float num6 = x3;
			x3 /= 128f;
			x3 -= 0.5f;
			x3 = (int)x3;
			x3 *= 128f;
			_stopPos += new Vector3(x3 - num6, 0f, 0f);
		}
		_scrollSpeed = 0f;
		ClipInventory();
	}

	private void GuiOnDragBegin(Vector3 begin)
	{
		if (!(_gui == null) && _gui.CheckCollider(SliceCollider, begin))
		{
			_bagScroll = true;
			_deceleration = false;
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
				BagSlotsRoot.localPosition = Vector3.Lerp(_startPos, _stopPos, DecelerationCurve.Evaluate(num2 / num));
			}
			else
			{
				BagSlotsRoot.localPosition = _stopPos;
				_deceleration = false;
			}
			ClipInventory();
		}
	}

	private void UpdateMinMax()
	{
		if (_cols <= 3)
		{
			_minScrollLoc = _maxScrollLoc;
		}
		else
		{
			_minScrollLoc = -(_cols - 3) * 128 + _maxScrollLoc;
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		RearrangeBag();
	}

	private bool ButtonExists(ServerData.Item item)
	{
		foreach (BagItemButton allBagItem in _allBagItems)
		{
			if (item == allBagItem.item)
			{
				return true;
			}
		}
		return false;
	}

	private bool ButtonNotExists(BagItemButton bibutton)
	{
		return SingletonT<ServerData>.I.FindInBag((ServerData.Item item) => item == bibutton.item) == null;
	}

	private void LateUpdate()
	{
		if (!_needRefresh)
		{
			return;
		}
		_needRefresh = false;
		List<ServerData.Item> toAddButton = new List<ServerData.Item>();
		SingletonT<ServerData>.I.ForeachInBag(delegate(ServerData.Item item)
		{
			if (!ButtonExists(item))
			{
				toAddButton.Add(item);
			}
		});
		List<BagItemButton> list = new List<BagItemButton>();
		foreach (BagItemButton allBagItem in _allBagItems)
		{
			if (ButtonNotExists(allBagItem))
			{
				list.Add(allBagItem);
			}
		}
		foreach (BagItemButton item in list)
		{
			_allBagItems.Remove(item);
			item.UnregisterMe();
			item.Eliminate();
		}
		foreach (ServerData.Item item2 in toAddButton)
		{
			AddItemToBag(item2);
		}
		RearrangeBag();
		Messenger.Invoke(Globals.MsgBagRefreshFinished);
	}

	internal BagItemButton GetPutOnItemFromSlot(ServerData.Slot.TypeE slot)
	{
		foreach (BagItemButton allBagItem in _allBagItems)
		{
			if (allBagItem.item.PutOn && allBagItem.item.Slot.SlotId == slot)
			{
				return allBagItem;
			}
		}
		return null;
	}

	private void PlaceToPers(BagItemButton itemButton)
	{
		PuppetSlot puppetSlot = GetPuppetSlot(itemButton);
		puppetSlot.TurnBgOff(off: true);
		itemButton.transform.position = puppetSlot.transform.position;
		itemButton.transform.parent = puppetSlot.transform;
		itemButton.transform.localPosition -= 50f * Vector3.forward;
		itemButton.item.PutArmorOn();
		ShopMk1.RecommendationsNeedRegeneration = true;
	}

	internal void RemoveFromPers(BagItemButton itemButton, bool showStatChanging)
	{
		if (!(itemButton == null))
		{
			PuppetSlot puppetSlot = GetPuppetSlot(itemButton);
			puppetSlot.TurnBgOff(off: false);
			itemButton.SetActive();
			itemButton.item.PutOn = false;
			if (showStatChanging)
			{
				System.Tuple<string, bool, string, bool> changeStatsDigits = Extensions.GetChangeStatsDigits(null, itemButton.item);
				Messenger.Invoke(Globals.MsgPlayerItemsChanged, changeStatsDigits);
			}
			itemButton.item.RemoveArmor();
			ShopMk1.RecommendationsNeedRegeneration = true;
		}
	}

	private PuppetSlot GetPuppetSlot(BagItemButton itemButton)
	{
		return Array.Find(PuppetScript.PuppetSlots, (PuppetSlot s) => s.Slot == itemButton.item.Slot.SlotId);
	}

	private void AddItemToBag(ServerData.Item newItem)
	{
		if (newItem.ElixirType == ServerData.Item.ElixirTypeE.None)
		{
			BagItemButton bagItemButton = Utils.Instaniate<BagItemButton>(ItemButtonProto);
			bagItemButton.transform.parent = BagSlotsRoot;
			bagItemButton.name = "bag_item_" + SpriteGui.UniqueId;
			bagItemButton.item = newItem;
			bagItemButton.Refresh();
			_allBagItems.Add(bagItemButton);
			bagItemButton.Init();
			bagItemButton.SetActive();
			bagItemButton.transform.GoToHell();
		}
	}

	internal void RearrangeBag()
	{
		_puppetItems.Clear();
		_inventoryItems = new List<BagItemButton>();
		foreach (BagItemButton allBagItem in _allBagItems)
		{
			if (allBagItem.item.PutOn)
			{
				_puppetItems.Add(allBagItem);
			}
			else
			{
				_inventoryItems.Add(allBagItem);
			}
		}
		foreach (BagItemButton puppetItem in _puppetItems)
		{
			puppetItem.RemoveNew();
			PlaceToPers(puppetItem);
		}
		ShowHideAllItems();
		_inventoryItems = _inventoryItems.OrderByDescending((BagItemButton item) => item.item.CreateTime).ToList();
		AddColsToInventory(Mathf.Max(12, _inventoryItems.Count()));
		ClipInventory();
		Messenger.Invoke(Globals.MsgPlayerSkillChanged);
	}

	internal void ShowHideAllItems()
	{
		foreach (BagItemButton allBagItem in _allBagItems)
		{
			if (_upgradeState && allBagItem.item.PutOn && allBagItem.item.MaxStars <= 0)
			{
				allBagItem.Hide();
			}
			else
			{
				allBagItem.Unhide();
			}
		}
	}

	private void ClipInventory()
	{
		float leftX = LeftClipObject.transform.position.x - 4f;
		float rightX = RightClipObject.transform.position.x - 6f;
		int num = 0;
		foreach (BagItemSlot bagSlot in _bagSlots)
		{
			bagSlot.TurnBgOff(off: false);
		}
		foreach (BagItemSlot bagSlot2 in _bagSlots)
		{
			bagSlot2.ClipWorld(leftX, rightX);
		}
		foreach (BagItemButton inventoryItem in _inventoryItems)
		{
			BagItemSlot bagItemSlot = _bagSlots[num++];
			bagItemSlot.TurnBgOff(off: true);
			bagItemSlot.SetItemColor(inventoryItem.item);
			inventoryItem.transform.parent = bagItemSlot.transform.parent;
			inventoryItem.transform.localPosition = bagItemSlot.transform.localPosition + new Vector3(4f, -4f, -100f);
			inventoryItem.ClipWorld(leftX, rightX);
		}
	}

	private void AddColsToInventory(int toCount)
	{
		int count = _bagSlots.Count;
		int num = toCount - count;
		if (num <= 0)
		{
			return;
		}
		int num2 = num / 3 + ((num % 3 > 0) ? 1 : 0);
		for (int i = 0; i < 3 * num2; i++)
		{
			BagItemSlot bagItemSlot = Utils.Instaniate<BagItemSlot>(BagSlotProto);
			bagItemSlot.transform.parent = BagSlotsRoot;
			bagItemSlot.transform.SetLayerRecursively(BagSlotsRoot);
			int num3 = i % 3;
			int num4 = i / 3 + count / 3;
			bagItemSlot.name = $"inventory_slot_{num4:00}x{num3:00}";
			int num5 = 128 * num4;
			int num6 = -128 * num3;
			if ((float)num5 > _xMax)
			{
				_xMax = num5;
			}
			bagItemSlot.transform.localPosition = new Vector3(num5, num6, BagSlotsRoot.localPosition.z);
			_bagSlots.Add(bagItemSlot);
		}
		_cols = _bagSlots.Count / 3;
		UpdateMinMax();
	}

	private void OnBagItemButtonRelease(SpriteButton spriteButton)
	{
		if (_upgradeState)
		{
			return;
		}
		BagItemButton bagItemButton = spriteButton as BagItemButton;
		if (!(bagItemButton == null))
		{
			if (!bagItemButton.item.PutOn)
			{
				bagItemButton.RemoveNew();
				ComparePopup(bagItemButton.item);
			}
			else
			{
				RemovePopup(bagItemButton);
			}
		}
	}

	private void RemovePopup(BagItemButton itemButton)
	{
		if (itemButton.item.IsItemSet())
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogError("Removing Set! {0}".Fmt(itemButton));
			}
		}
		else
		{
			Messenger<BagItemButton>.Invoke(Globals.MsgPlayerRemoveItem, itemButton);
		}
	}

	private void ComparePopup(ServerData.Item item)
	{
		if (item.IsItemSet())
		{
			PutonSet(item);
		}
		else
		{
			Messenger<ServerData.Item>.Invoke(Globals.MsgPlayerCompareItem, item);
		}
	}

	private void RemoveBeforeSet(HashSet<ServerData.ShopGood> set)
	{
		ServerData.Slot.TypeE[] slots = set.Select((ServerData.ShopGood shopGood) => shopGood.Item.Slot.SlotId).ToArray();
		IEnumerable<BagItemButton> enumerable = _allBagItems.Where((BagItemButton bib) => bib.item.PutOn && slots.Contains(bib.item.Slot.SlotId));
		foreach (BagItemButton item in enumerable)
		{
			RemoveFromPers(item, showStatChanging: false);
		}
	}

	private void PutonSet(ServerData.Item item)
	{
		if (!item.IsItemSet())
		{
			Debug.LogError("[FUCKUP -- MUST BE A SET]");
			return;
		}
		if (item.Slot.SlotId == ServerData.Slot.TypeE.RareSet)
		{
			RemoveBeforeSet(ShopMk1.RareSet);
			foreach (ServerData.ShopGood item2 in ShopMk1.RareSet)
			{
				ServerData.ShopGood shopGood = item2.MakeRealItem(forShop: true);
				shopGood.Item.SetPutonTrue();
				SingletonT<ServerData>.I.AddToBag(shopGood.Item);
			}
		}
		else if (item.Slot.SlotId == ServerData.Slot.TypeE.EpicSet)
		{
			RemoveBeforeSet(ShopMk1.EpicSet);
			foreach (ServerData.ShopGood item3 in ShopMk1.EpicSet)
			{
				ServerData.ShopGood shopGood2 = item3.MakeRealItem(forShop: true);
				shopGood2.Item.SetPutonTrue();
				SingletonT<ServerData>.I.AddToBag(shopGood2.Item);
			}
		}
		else if (item.Slot.SlotId == ServerData.Slot.TypeE.HalfEpicSet)
		{
			RemoveBeforeSet(ShopMk1.HalfEpicSet);
			foreach (ServerData.ShopGood item4 in ShopMk1.HalfEpicSet)
			{
				ServerData.ShopGood shopGood3 = item4.MakeRealItem(forShop: true);
				shopGood3.Item.SetPutonTrue();
				SingletonT<ServerData>.I.AddToBag(shopGood3.Item);
			}
		}
		else if (item.Slot.SlotId == ServerData.Slot.TypeE.AlchemySet)
		{
			foreach (ServerData.ShopGood item5 in ShopMk1.AlchemySet)
			{
				ServerData.ShopGood shopGood4 = item5.MakeRealItem(forShop: true);
				switch (shopGood4.Item.ElixirType)
				{
				case ServerData.Item.ElixirTypeE.Heal:
					shopGood4.Item.RealItemsCount = 60;
					break;
				case ServerData.Item.ElixirTypeE.Critical:
					shopGood4.Item.RealItemsCount = 50;
					break;
				case ServerData.Item.ElixirTypeE.Poison:
					shopGood4.Item.RealItemsCount = 30;
					break;
				}
				SingletonT<ServerData>.I.AddToBag(shopGood4.Item);
			}
		}
		SingletonT<ServerData>.I.RemoveFromBag(item);
		Messenger.Invoke(Globals.MsgBagNeedRefresh);
		StartCoroutine(RefreshOneTime());
	}

	private IEnumerator RefreshOneTime()
	{
		yield return new WaitForSeconds(3f);
		RearrangeBag();
	}
}
