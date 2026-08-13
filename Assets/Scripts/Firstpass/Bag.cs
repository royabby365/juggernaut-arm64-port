using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bag : SpriteGui, IViewportFrame
{
	public Transform filterEmpty;

	public InventoryScrollButton scrollLeft;

	public InventoryScrollButton scrollRight;

	public Rect inventoryFrame = new Rect(26f, 60f, 380f, 412f);

	public PersSlot[] slots;

	private readonly List<InventoryItemButton> _items = new List<InventoryItemButton>();

	public BackPackItem[] backpackSlots;

	public StatLabel Life;

	public StatLabel Anger;

	public StatLabel Strength;

	public StatLabel Mana;

	public SpriteText pointToLabel;

	public Transform popupLeftTop;

	public Vector3 popupInactivePos = new Vector3(1024f, 0f, -500f);

	public SpriteText goldLabel;

	public SpriteText diamondLabel;

	public SpriteText keysLabel;

	public SpriteText persLevel;

	public SpriteText bagLabel;

	public SpriteText healthCount;

	public SpriteText superCount;

	public SpriteText poisonCount;

	public SpriteText ForceOfFireStat;

	public SpriteText ForceOfIceStat;

	public SpriteText ForceOfLightningStat;

	public SpriteText ForceOfDarkStat;

	private string _bagNameUnfiltered;

	public GameObject ItemPrototype;

	public GameObject ItemsNode;

	public Transform progressExp;

	public Transform progressRage;

	private GameObject _pers;

	private PersonArmor _personArmor;

	private int _toAddLife;

	private int _toAddAnger;

	private int _toAddStrength;

	private int _toAddMana;

	private bool _popupIsOn;

	private InventoryItemButton _old;

	private InventoryItemButton _new;

	private float _cameraRotation;

	private List<InventoryItemButton> _itemsForBag = new List<InventoryItemButton>();

	private readonly List<InventoryItemButton> _itemsForPers = new List<InventoryItemButton>();

	private bool _filterOn;

	private PersSlot _activeFilter;

	private int _scrollLeft;

	private ITouchscreen _touchscreen;

	private CameraPersInBag _cameraInBag;

	private GameObject Pers
	{
		get
		{
			if (_pers == null)
			{
				_pers = Globals.PlayerGameObject;
			}
			return _pers;
		}
	}

	private PersonArmor PersonArmor
	{
		get
		{
			if (Pers != null && _personArmor == null)
			{
				_personArmor = _pers.GetComponent<PersonArmor>();
			}
			return _personArmor;
		}
	}

	public void SetHealthCount(int count)
	{
		healthCount.Text_ = count.ToString();
	}

	public void SetCriticalCount(int count)
	{
		superCount.Text_ = count.ToString();
	}

	public void SetPoisonCount(int count)
	{
		poisonCount.Text_ = count.ToString();
	}

	public void SetPersentageExp(int percent)
	{
		FightResultPbar component = progressExp.GetComponent<FightResultPbar>();
		component.SetPecentage(percent);
	}

	public void SetPersentageAnger(int percent)
	{
		FightResultPbar component = progressRage.GetComponent<FightResultPbar>();
		component.SetPecentage(percent);
	}

	public void SetPersLevel(int lvl)
	{
		persLevel.GetComponent<SpriteText>().Text_ = lvl.ToString();
	}

	private void PopupCompare()
	{
		Rect rect = ((!(_old == null)) ? popupLeftTop.GetComponent<ElasticPopup>().Compare(_old, _new) : popupLeftTop.GetComponent<ElasticPopup>().Compare(_new));
		popupLeftTop.localPosition = new Vector3((1024 - rect.width.RoundToInt()) / 2, (-768 + rect.height.RoundToInt()) / 2, -500f);
		_popupIsOn = true;
	}

	private void Start()
	{
		SetPersentageExp(100 - SingletonT<ServerData>.I.GetPlayerExperiencePercentToNextLevel());
		SetPersentageAnger(SingletonT<ServerData>.I.PlayerParams.Anger);
		SetPersLevel(SingletonT<ServerData>.I.PlayerParams.Level);
		_bagNameUnfiltered = bagLabel.Text_;
		foreach (SpriteButton value in _buttons.Values)
		{
			value.SetActive();
		}
		base.Release += ProcessButtons;
		base.LongPress += ProcessLongPressIntervals;
		_touchscreen = Globals.CreateTouchscreen();
		_touchscreen.OnTouchMove += Touchscreen_OnTouchMove;
		_touchscreen.OnZoom += Touchscreen_OnZoom;
		_cameraInBag = (CameraPersInBag)UnityEngine.Object.FindObjectOfType(typeof(CameraPersInBag));
		_cameraInBag.player = Pers;
		ArmorData[] componentsInChildren = Pers.GetComponentsInChildren<ArmorData>(includeInactive: true);
		foreach (ArmorData armorData in componentsInChildren)
		{
			armorData.gameObject.SetActive(true);
			Utils.SetAllRenderersActive(armorData, value: true);
		}
		Transform transform = Pers.transform.FindChildByName("head", includeInactive: true);
		if (transform != null)
		{
			Utils.SetAllRenderersActive(transform, value: true);
		}
		Setup();
		if (Globals.IsDebugBuild)
		{
			Debug.Log("[BAG START]");
		}
		RegenerateAtlas();
		Globals.HideLoadingScreen();
	}

	private void Touchscreen_OnZoom(float offset, Vector2 startPos, Vector2 startPos1, Vector2 startPos2)
	{
		_cameraInBag.InputZoom = offset;
		_cameraInBag.InputZoomStartPos = new Vector2(startPos.x, (float)Camera2D.ScreenHeight - startPos.y);
		_cameraInBag.InputZoomStartPos1 = new Vector2(startPos1.x, (float)Camera2D.ScreenHeight - startPos1.y);
		_cameraInBag.InputZoomStartPos1 = new Vector2(startPos2.x, (float)Camera2D.ScreenHeight - startPos2.y);
	}

	private void Touchscreen_OnTouchMove(Vector2 offset, Vector2 pos)
	{
		_cameraInBag.InputOffset = offset;
		_cameraInBag.InputPos = new Vector2(pos.x, (float)Camera2D.ScreenHeight - pos.y);
	}

	private void Update()
	{
		ProcessRayCast();
		if (_touchscreen != null)
		{
			_touchscreen.Update();
		}
		if (Camera.main != null && Pers != null && _cameraRotation.Eqv(0f))
		{
			Transform transform = Camera.main.transform;
			if (transform != null)
			{
				_cameraRotation = 0f;
			}
		}
	}

	private void ProcessLongPressIntervals(SpriteButton button)
	{
		PersSlot persSlot = button as PersSlot;
		if (persSlot == null)
		{
			return;
		}
		ServerData.Slot.TypeE slot = persSlot.Slot;
		foreach (InventoryItemButton item in _items)
		{
			if (item.shopItem.Slot.SlotId == slot && item.shopItem.PutOn)
			{
				RemoveFromPers(item);
				RefreshBag();
				break;
			}
		}
	}

	private void ProcessButtons(SpriteButton button)
	{
		switch (button.name)
		{
		case "+life":
			if (!_popupIsOn)
			{
				Life.AddOne();
				UpdatePlusesMinuses();
			}
			break;
		case "-life":
			if (!_popupIsOn)
			{
				Life.MinusOne();
				UpdatePlusesMinuses();
			}
			break;
		case "+mana":
			if (!_popupIsOn)
			{
				Mana.AddOne();
				UpdatePlusesMinuses();
			}
			break;
		case "-mana":
			if (!_popupIsOn)
			{
				Mana.MinusOne();
				UpdatePlusesMinuses();
			}
			break;
		case "+anger":
			if (!_popupIsOn)
			{
				Anger.AddOne();
				UpdatePlusesMinuses();
			}
			break;
		case "-anger":
			if (!_popupIsOn)
			{
				Anger.MinusOne();
				UpdatePlusesMinuses();
			}
			break;
		case "+strength":
			if (!_popupIsOn)
			{
				Strength.AddOne();
				UpdatePlusesMinuses();
			}
			break;
		case "-strength":
			if (!_popupIsOn)
			{
				Strength.MinusOne();
				UpdatePlusesMinuses();
			}
			break;
		case "popup_puton":
			PopUpPuton();
			break;
		case "popup_sell":
			PopUpSell();
			break;
		case "popup_cancel":
			PopUpCancel();
			break;
		case "scroll_left":
			_scrollLeft--;
			RefreshBag();
			break;
		case "scroll_right":
			_scrollLeft++;
			RefreshBag();
			break;
		case "close_btn_sp":
			CommitStatChanges();
			break;
		default:
		{
			if (_popupIsOn)
			{
				break;
			}
			InventoryItemButton inventoryItemButton = button as InventoryItemButton;
			PersSlot persSlot = button as PersSlot;
			if (persSlot != null)
			{
				if (_filterOn && persSlot != _activeFilter)
				{
					UnfilterItems();
					_activeFilter = persSlot;
					FilterItems();
				}
				else if (_filterOn && persSlot == _activeFilter)
				{
					UnfilterItems();
				}
				else if (!_filterOn)
				{
					_activeFilter = persSlot;
					FilterItems();
				}
				else if (Globals.IsDebugBuild)
				{
					Debug.LogError($"cannot be here: on:{_filterOn} active:{_activeFilter.name} this:{persSlot.name}");
				}
				RefreshBag();
			}
			else if (inventoryItemButton != null)
			{
				inventoryItemButton.RemoveNew();
				GoToPers(inventoryItemButton);
				RefreshBag();
			}
			break;
		}
		}
		UpdateStats();
	}

	private void OnEnable()
	{
		Messenger<int>.AddListener(Globals.MsgPlayerAngerChanged, MsgPlayerAngerChangedHandler);
	}

	private void MsgPlayerAngerChangedHandler(int anger)
	{
		SetPersentageAnger(anger);
	}

	private void OnDisable()
	{
		Messenger<int>.RemoveListener(Globals.MsgPlayerAngerChanged, MsgPlayerAngerChangedHandler);
	}

	private void RefreshBag()
	{
		_itemsForBag.Clear();
		_itemsForPers.Clear();
		BackPackItem[] array = backpackSlots;
		foreach (BackPackItem backPackItem in array)
		{
			backPackItem.SetOff();
		}
		foreach (InventoryItemButton item in _items)
		{
			if (item.shopItem.ElixirType == ServerData.Item.ElixirTypeE.None)
			{
				if (item.shopItem.PutOn)
				{
					_itemsForPers.Add(item);
				}
				else if (_filterOn && item.shopItem.Slot.SlotId != _activeFilter.Slot)
				{
					GoToHell(item);
				}
				else
				{
					_itemsForBag.Add(item);
				}
			}
		}
		if (_itemsForBag.Count <= backpackSlots.Length)
		{
			_scrollLeft = 0;
			scrollLeft.SetInactive();
			scrollRight.SetInactive();
		}
		else
		{
			int num = (_itemsForBag.Count - backpackSlots.Length) / 2 + (_itemsForBag.Count - backpackSlots.Length) % 2;
			if (_scrollLeft < 0)
			{
				_scrollLeft = 0;
			}
			if (_scrollLeft > num)
			{
				_scrollLeft = num;
			}
			if (Globals.IsDebugBuild)
			{
				Debug.Log($"max:{num} left:{_scrollLeft}");
			}
			if (_scrollLeft == 0)
			{
				scrollLeft.SetInactive();
				scrollRight.SetActive();
			}
			else if (_scrollLeft < num)
			{
				scrollLeft.SetActive();
				scrollRight.SetActive();
			}
			else
			{
				scrollLeft.SetActive();
				scrollRight.SetInactive();
			}
		}
		int num2 = 2 * _scrollLeft;
		for (int j = 0; j < num2; j++)
		{
			GoToHell(_itemsForBag[j]);
		}
		_itemsForBag = _itemsForBag.OrderByDescending((InventoryItemButton it) => it.shopItem.CreateTime).ToList();
		for (int num3 = num2; num3 < _itemsForBag.Count; num3++)
		{
			if (num3 < backpackSlots.Length + num2)
			{
				GoToBag(_itemsForBag[num3], num3 - num2);
			}
			else
			{
				GoToHell(_itemsForBag[num3]);
			}
		}
		foreach (InventoryItemButton itemsForPer in _itemsForPers)
		{
			itemsForPer.RemoveNew();
			PlaceToPers(itemsForPer);
		}
		if (_filterOn && _itemsForBag.Count == 0)
		{
			filterEmpty.gameObject.SetActiveRecursivelyMk1(setActive: true);
		}
		else
		{
			filterEmpty.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		UpdateAdds();
		Messenger.Invoke(Globals.MsgPlayerBagRearrange);
		Messenger.Invoke(Globals.MsgPlayerSkillChanged);
	}

	private static void GoToHell(InventoryItemButton it)
	{
		float x = it.transform.position.x;
		float y = it.transform.position.y;
		float z = it.transform.position.z;
		it.transform.position = new Vector3(x + 2500f, y, z);
		it.Hide();
	}

	private static string SlotToString(ServerData.Slot.TypeE slid)
	{
		using (IEnumerator<ServerData.Slot> enumerator = SingletonT<ServerData>.I.Slots.Where((ServerData.Slot sl) => sl.SlotId == slid).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				ServerData.Slot current = enumerator.Current;
				return current.Title;
			}
		}
		return "???";
	}

	private void UpdateStats()
	{
		Life.SetStat(SingletonT<ServerData>.I.PlayerParams.HP);
		Mana.SetStat(SingletonT<ServerData>.I.PlayerParams.Magic);
		Anger.SetStat(SingletonT<ServerData>.I.PlayerParams.Rage);
		Strength.SetStat(SingletonT<ServerData>.I.PlayerParams.Strength);
		pointToLabel.Text_ = SingletonT<ServerData>.I.PlayerParams.SkillPoints.ToString();
		ServerData.PlayerParamsData playerParams = SingletonT<ServerData>.I.PlayerParams;
		goldLabel.Text_ = playerParams.MoneyGoldCount.ToString();
		diamondLabel.Text_ = playerParams.MoneyDiamondCount.ToString();
		keysLabel.Text_ = playerParams.MoneyKeysCount.ToString();
	}

	private void GoToBag(InventoryItemButton item, int index)
	{
		item.Unhide();
		item.SetActive();
		BackPackItem backPackItem = backpackSlots[index];
		backPackItem.SetOn();
		item.transform.position = backPackItem.transform.position;
		item.transform.parent = backPackItem.transform;
		item.transform.localPosition += new Vector3(11f, -11f, -100f);
	}

	private void RemoveFromPers(InventoryItemButton item)
	{
		if (!(item == null))
		{
			PersSlot persSlot = Array.Find(slots, (PersSlot s) => s.Slot == item.shopItem.Slot.SlotId);
			persSlot.EnableLongPress(enable: false);
			item.shopItem.PutOn = false;
			item.SetActive();
			RemoveArmor(item);
			Messenger.Invoke(Globals.MsgPlayerBagRearrange);
		}
	}

	private void GoToPers(InventoryItemButton item)
	{
		PersSlot slot = Array.Find(slots, (PersSlot s) => s.Slot == item.shopItem.Slot.SlotId);
		List<InventoryItemButton> list = (from it in _items
			where it != item
			where it.shopItem.PutOn && it.shopItem.Slot.SlotId == slot.Slot
			select it).ToList();
		_old = ((list.Count <= 0) ? null : list[0]);
		_new = item;
		PopupCompare();
	}

	private void PlaceToPers(InventoryItemButton item)
	{
		PersSlot persSlot = Array.Find(slots, (PersSlot s) => s.Slot == item.shopItem.Slot.SlotId);
		persSlot.EnableLongPress(enable: true);
		item.transform.position = persSlot.transform.position;
		item.transform.parent = persSlot.transform;
		item.transform.localPosition += new Vector3(4f, -5f, -100f);
		item.SetInactive();
		item.Unhide();
		PutArmorOn(item);
	}

	private void UpdateAdds()
	{
		int life = 0;
		int anger = 0;
		int mana = 0;
		int strength = 0;
		SingletonT<ServerData>.I.ForeachInBag(delegate(ServerData.Item it)
		{
			if (it.PutOn)
			{
				life += it.GetSkill(ServerData.Skill.TypeE.Vitality, 0);
				anger += it.GetSkill(ServerData.Skill.TypeE.Rage, 0);
				strength += it.GetSkill(ServerData.Skill.TypeE.Strength, 0);
				mana += it.GetSkill(ServerData.Skill.TypeE.Magic, 0);
			}
		});
		Life.AddToStat(life);
		Anger.AddToStat(anger);
		Mana.AddToStat(mana);
		Strength.AddToStat(strength);
		UpdateStats();
	}

	private void UpdatePlusesMinuses()
	{
		Life.SetPlusMinus();
		Anger.SetPlusMinus();
		Mana.SetPlusMinus();
		Strength.SetPlusMinus();
	}

	public void CommitStatChanges()
	{
		Life.Commit();
		Anger.Commit();
		Mana.Commit();
		Strength.Commit();
	}

	private void PopUpCancel()
	{
		PopUpCleanup();
	}

	private void PopUpPuton()
	{
		InventoryItemButton inventoryItemButton = _new;
		InventoryItemButton old = _old;
		PopUpCleanup();
		inventoryItemButton.shopItem.PutOn = true;
		RemoveFromPers(old);
		RefreshBag();
	}

	private void PopUpSell()
	{
		InventoryItemButton inventoryItemButton = _new;
		PopUpCleanup();
		SingletonT<ServerData>.I.SellItem(inventoryItemButton.shopItem);
		RemoveItemForever(inventoryItemButton);
		UpdateStats();
		RefreshBag();
		Messenger.Invoke(Globals.MsgPlayerBagChanged);
	}

	private void RemoveItemForever(InventoryItemButton item)
	{
		GoToHell(item);
		_items.Remove(item);
		_itemsForBag.Remove(item);
		_itemsForPers.Remove(item);
		UnregisterButton(item);
		UnityEngine.Object.Destroy(item.gameObject);
	}

	private void PopUpCleanup()
	{
		_old = null;
		_new = null;
		_popupIsOn = false;
		popupLeftTop.transform.GetComponent<ElasticPopup>().Cleanup();
		popupLeftTop.transform.localPosition = popupInactivePos;
	}

	private void PutArmorOn(InventoryItemButton item)
	{
		if (!(PersonArmor == null))
		{
			if (item.shopItem.Slot.SlotId == ServerData.Slot.TypeE.Weapon)
			{
				SingletonT<ServerData>.I.MyWeapon = item.shopItem;
			}
			Utils.LogFrom("Bag", "PutArmorOn", item.name);
			PersonArmor.ChangeArmor(SingletonT<ServerData>.I.PlayerServerPersData.ModelId, item.shopItem, item.shopItem.Slot.SlotId, null);
		}
	}

	private void RemoveArmor(InventoryItemButton item)
	{
		if (!(PersonArmor == null))
		{
			Utils.LogFrom("Bag", "RemoveArmor", item.name);
			PersonArmor.ChangeArmor(SingletonT<ServerData>.I.PlayerServerPersData.ModelId, null, item.shopItem.Slot.SlotId, null);
		}
	}

	private void FilterItems()
	{
		_filterOn = true;
		ServerData.Slot.TypeE slot = _activeFilter.Slot;
		_activeFilter.SetSelected();
		bagLabel.Text_ = _bagNameUnfiltered + " :: " + SlotToString(slot);
		RefreshBag();
	}

	private void UnfilterItems()
	{
		bagLabel.Text_ = _bagNameUnfiltered;
		_filterOn = false;
		_activeFilter = null;
		PersSlot[] array = slots;
		foreach (PersSlot persSlot in array)
		{
			persSlot.SetUnselected();
		}
	}

	internal void Setup()
	{
		popupLeftTop.transform.localPosition = popupInactivePos;
		foreach (InventoryItemButton item in _items)
		{
			item.Remove();
			UnityEngine.Object.DestroyImmediate(item.gameObject);
		}
		_items.Clear();
		SingletonT<ServerData>.I.ForeachInBag(delegate(ServerData.Item serverItem)
		{
			if (serverItem.ElixirType == ServerData.Item.ElixirTypeE.None)
			{
				InventoryItemButton inventoryItemButton = Utils.Instaniate<InventoryItemButton>(ItemPrototype);
				inventoryItemButton.name = "shop_good_" + SpriteGui.UniqueId;
				inventoryItemButton.transform.parent = ItemsNode.transform;
				inventoryItemButton.shopItem = serverItem;
				inventoryItemButton.GetComponent<Renderer>().material.mainTexture = SingletonT<ResourcesManager>.I.LoadItemIcon(serverItem);
				if (serverItem.New)
				{
					inventoryItemButton.SetNew();
				}
				else
				{
					inventoryItemButton.RemoveNew();
				}
				_items.Add(inventoryItemButton);
			}
		});
		foreach (InventoryItemButton item2 in _items)
		{
			item2.Init();
		}
		SetupIdle();
		UnfilterItems();
		RefreshBag();
		UpdatePlusesMinuses();
		SetHealthCount(SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Heal));
		SetPoisonCount(SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Poison));
		SetCriticalCount(SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Critical));
	}

	private void SetupIdle()
	{
		GameObject pers = Pers;
		PersonShopData component = Globals.PlayerGameObject.GetComponent<PersonShopData>();
		string newName = "idle";
		if (component != null)
		{
			if (pers.GetComponent<Animation>()[Globals.ShopIdleAnimationName] == null)
			{
				AnimationClip[] array = Utils.MakeArray((AnimationClip _) => _ != null, component.HammerInShopIdles);
				if (array.Length > 0)
				{
					AnimationClip clip = array[UnityEngine.Random.Range(0, array.Length - 1)];
					newName = Globals.ShopIdleAnimationName;
					pers.GetComponent<Animation>().AddClip(clip, newName);
				}
			}
			else
			{
				newName = Globals.ShopIdleAnimationName;
			}
		}
		pers.GetComponent<Animation>()[newName].wrapMode = WrapMode.Loop;
		pers.GetComponent<Animation>().Play(newName);
		pers.GetComponent<Animation>().cullingType = AnimationCullingType.BasedOnRenderers;
	}

	public Rect GetInventoryFrame()
	{
		return inventoryFrame;
	}
}
