using System;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class ShopItemMk1 : MonoBehaviour
{
	public enum State
	{
		Show,
		ItemNeedEquip,
		ItemEquipped,
		Relic
	}

	public const string EpicItemInfoButtonName = "epic_item_info_";

	private CompositeDisposable _subscriptions;

	public Transform ButtonFrame;

	public ShopBuyButtonMk1 ButtonBuy;

	public ShopBuyButtonMk1 ButtonBuyForGold;

	public ShopBuyButtonMk1 ButtonBuyForDiamonds;

	public SpriteText Count;

	public SpriteText CountSuffix;

	public CompareOneStat Compare1;

	public CompareOneStat Compare2;

	public SpriteText NonItemDescription;

	public Sprite ItemFrame;

	public Sprite ItemIcon;

	public SpriteText ItemName;

	public SpriteText LevelLockText;

	public Sprite Star;

	public GameObject BestBuyProto;

	private readonly Color _darkTint = new Color32(128, 128, 128, 96);

	private string _levelTooLowFmt;

	private ServerData.ShopGood _shopGood;

	private State _state;

	private ShopItemMk1 _altShopItem;

	private int _absolutePointer;

	public ServerData.ShopGood ShopGood
	{
		get
		{
			return _shopGood;
		}
		set
		{
			_shopGood = value;
			ButtonBuy.MyShopItem = this;
			ButtonBuyForGold.MyShopItem = this;
			ButtonBuyForGold.SetInactive();
			ButtonBuyForDiamonds.SetInactive();
			SetItemLook(value);
			UpdateCompare();
			if (value.Discount > 0)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BestBuyProto);
				gameObject.layer = base.gameObject.layer;
				gameObject.transform.parent = ItemFrame.transform;
				gameObject.transform.localPosition = new Vector3(-30f, 7f, -100f);
				DayDeal component = gameObject.transform.GetComponent<DayDeal>();
				if (component != null)
				{
					component.Discount.Text_ = "-" + value.Discount;
				}
			}
		}
	}

	public int AbsolutePointer => (_absolutePointer > 0) ? _absolutePointer : 0;

	public SpriteButton EpicInfoButton { get; private set; }

	private void Awake()
	{
		_levelTooLowFmt = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.ShopLevelTooLow);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	public void AddAlternateShopItem(ShopItemMk1 altShopItem)
	{
		_altShopItem = altShopItem;
		ButtonBuy.SetInactive();
		ButtonBuyForGold.SetActive();
		ButtonBuyForDiamonds.SetActive();
		ButtonBuyForDiamonds.MyShopItem = altShopItem;
	}

	public void RefreshState(int absolutePointer)
	{
		_absolutePointer = absolutePointer;
		if (_shopGood.Item.IsElixirType())
		{
			SetState(State.Show);
		}
		else if (_shopGood.PuppetHasIt())
		{
			SetState(State.ItemEquipped);
		}
		else if (_shopGood.PlayerHasIt())
		{
			SetState(State.ItemNeedEquip);
		}
		else
		{
			SetState(State.Show);
		}
	}

	private int LevelCapped()
	{
		int level = SingletonT<ServerData>.I.PlayerParams.Level;
		if (level < _shopGood.LevelMin)
		{
			return _shopGood.LevelMin;
		}
		return -1;
	}

	private void SetState(State state)
	{
		if (ShopGood.Relict)
		{
			state = State.Relic;
		}
		switch (state)
		{
		case State.Show:
			ItemIcon.Tint_ = Color.gray;
			if (_altShopItem == null)
			{
				int num = LevelCapped();
				bool flag = num < 0;
				ButtonBuy.SetState((!flag) ? ShopBuyButtonMk1.ShopButtonState.Disable : ShopBuyButtonMk1.ShopButtonState.Buy);
				ShowOrHideLevelLock(num);
				ButtonBuyForGold.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
				ButtonBuyForDiamonds.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
			}
			else
			{
				ButtonBuy.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
				int num2 = LevelCapped();
				int num3 = _altShopItem.LevelCapped();
				int lvl = ((num3 <= 0) ? num2 : num3);
				ShowOrHideLevelLock(lvl);
				ButtonBuyForGold.SetState((num2 >= 0) ? ShopBuyButtonMk1.ShopButtonState.Disable : ShopBuyButtonMk1.ShopButtonState.Buy);
				ButtonBuyForDiamonds.SetState((num3 >= 0) ? ShopBuyButtonMk1.ShopButtonState.Disable : ShopBuyButtonMk1.ShopButtonState.Buy);
			}
			_state = state;
			break;
		case State.ItemNeedEquip:
			if (!_shopGood.Item.IsElixirType())
			{
				ShopMk1.RecommendationsNeedRegeneration = true;
				_state = state;
				ButtonBuy.SetState(ShopBuyButtonMk1.ShopButtonState.NeedEquip);
				ShowOrHideLevelLock(-1);
				ButtonBuyForGold.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
				ButtonBuyForDiamonds.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
			}
			break;
		case State.ItemEquipped:
			ShopMk1.RecommendationsNeedRegeneration = true;
			ButtonBuy.SetState(ShopBuyButtonMk1.ShopButtonState.Equipped);
			ShowOrHideLevelLock(-1);
			ButtonBuyForGold.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
			ButtonBuyForDiamonds.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
			_state = state;
			break;
		case State.Relic:
			NonItemDescription.ShowOrHide(show: true);
			break;
		default:
			throw new ArgumentOutOfRangeException("state");
		}
	}

	public void DoPurchaseItem()
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (_state == State.ItemNeedEquip)
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BagItems);
			return;
		}
		System.Tuple<ServerData.MoneyType.TypeE, int, string> itemBuyPrice = _shopGood.GetItemBuyPrice();
		if (itemBuyPrice.Item1.GetPlayerFundsCount() < itemBuyPrice.Item2)
		{
			Messenger.Invoke(Globals.MsgInsufficientFunds);
			return;
		}
		itemBuyPrice.Item1.ChangePlayerFundsCount(-itemBuyPrice.Item2);
		if (_shopGood.Item.IsElixirType())
		{
			ServerData.ShopGood shopGood = _shopGood.MakeRealItem(forShop: true);
			ServerData.Item item = shopGood.Item;
			if (_shopGood.Item.IsMoney())
			{
				_shopGood.Item.GetMoneyTypeFromItem().ChangePlayerFundsCount(_shopGood.Count);
			}
			else
			{
				SingletonT<ServerData>.I.AddToBag(item);
				Messenger<ServerData.Item.ElixirTypeE, int>.Invoke(Globals.MsgElixirCountChanged, item.ElixirType, _shopGood.Count);
			}
			Messenger<ServerData.Item, ServerData.MoneyType.TypeE>.Invoke(Globals.MsgItemBuyInShop, item, itemBuyPrice.Item1);
		}
		else
		{
			ServerData.ShopGood shopGood2 = _shopGood.MakeRealItem(forShop: true);
			shopGood2.Item.PutOn = false;
			shopGood2.Item.New = true;
			SingletonT<ServerData>.I.AddToBag(shopGood2.Item);
			Messenger.Invoke(Globals.MsgBagNeedRefresh);
			SetState(State.ItemNeedEquip);
			Messenger<ServerData.Item, ServerData.MoneyType.TypeE>.Invoke(Globals.MsgItemBuyInShop, shopGood2.Item, itemBuyPrice.Item1);
		}
	}

	private void SetItemLook(ServerData.ShopGood shopGood)
	{
		if (SingletonT<ServerData>.I.PlayerServerPersData == null)
		{
			return;
		}
		ServerData.Item item = shopGood.Item;
		FontManager.ColorE colorE = item.DecodeColor();
		Color bottomColor = FontManager.Instance.GetNamedColor(colorE).BottomColor;
		ItemFrame.Tint_ = bottomColor;
		ItemIcon.SpriteName_ = SingletonT<ServerData>.I.GetItemImageName(item);
		ItemName.Text_ = item.TitleString;
		ItemName.NamedColorE_ = colorE;
		if (shopGood.Relict)
		{
			NonItemDescription.transform.localPosition -= 65f * Vector3.up;
			NonItemDescription.ShowOrHide(show: true);
			NonItemDescription.Text_ = ((!item.Description.IsNullOrEmpty()) ? item.Description : "Some uber sword from Ancient ruins. The Ancient Ruins are located in the Tarm Ruins, near the far corner of the map");
			ShowOrHideLevelLock(-1);
			ButtonBuy.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
			ButtonBuyForGold.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
			ButtonBuyForDiamonds.SetState(ShopBuyButtonMk1.ShopButtonState.HideForever);
			ButtonFrame.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		else
		{
			bool flag = shopGood.Count > 1;
			Count.gameObject.SetActiveRecursivelyMk1(flag);
			CountSuffix.gameObject.SetActiveRecursivelyMk1(flag);
			if (flag)
			{
				Count.Text_ = shopGood.Count.ToString();
			}
		}
		bool flag2 = BagItemButton.GetUpgradeId(shopGood.Item) > 0;
		Star.gameObject.SetActiveRecursivelyMk1(flag2);
		if (flag2)
		{
			SimplestDraggableButtonNoInit simplestDraggableButtonNoInit = ItemIcon.gameObject.AddComponent<SimplestDraggableButtonNoInit>();
			simplestDraggableButtonNoInit.name = "epic_item_info_" + SpriteGui.UniqueId;
			simplestDraggableButtonNoInit.Init();
			simplestDraggableButtonNoInit.SetActive();
			EpicInfoButton = simplestDraggableButtonNoInit;
		}
	}

	private void ShowOrHideLevelLock(int lvl)
	{
		bool flag = lvl > 0;
		LevelLockText.transform.parent.gameObject.SetActiveRecursivelyMk1(flag);
		if (flag)
		{
			LevelLockText.Text_ = _levelTooLowFmt.Fmt(lvl);
		}
	}

	public void UpdateCompare()
	{
		ServerData.Item puppetItem = ShopGood.Item.GetPuppetItem();
		SetItemCompare(ShopGood.Item, puppetItem);
	}

	private void SetItemCompare(ServerData.Item item, ServerData.Item opposite)
	{
		if (item == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("[FUCKUP]");
			}
			return;
		}
		ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
		NonItemDescription.ShowOrHide(show: false);
		if (itemSkillInfo == null || itemSkillInfo.Skill.Type == ServerData.Skill.TypeE.FullRage || itemSkillInfo.Skill.Type == ServerData.Skill.TypeE.FullMana || item.IsItemSet())
		{
			Compare1.SetCompareEmpty();
			Compare2.SetCompareEmpty();
			NonItemDescription.Text_ = ((!item.Description.IsNullOrEmpty()) ? item.Description : "<empty>");
			NonItemDescription.ShowOrHide(show: true);
			return;
		}
		if (opposite == null)
		{
			Compare2.SetCompareEmpty();
			int current = itemSkillInfo.Current;
			System.Tuple<string, int> itemDescription = item.GetItemDescription();
			Compare1.SetCompare(itemDescription, current, oppositeState: false);
		}
		else
		{
			ServerData.SkillInfo itemSkillInfo2 = opposite.GetItemSkillInfo();
			if (itemSkillInfo2.Skill.Type == itemSkillInfo.Skill.Type)
			{
				Compare2.SetCompareEmpty();
				int delta = itemSkillInfo.Current - itemSkillInfo2.Current;
				System.Tuple<string, int> itemDescription2 = item.GetItemDescription();
				Compare1.SetCompare(itemDescription2, delta, oppositeState: false);
			}
			else
			{
				int current2 = itemSkillInfo.Current;
				int current3 = itemSkillInfo2.Current;
				System.Tuple<string, int> itemDescription3 = item.GetItemDescription();
				System.Tuple<string, int> itemDescription4 = opposite.GetItemDescription();
				Compare1.SetCompare(itemDescription3, current2, oppositeState: false);
				Compare2.SetCompare(itemDescription4, current3, oppositeState: true);
			}
		}
		RefreshState(AbsolutePointer);
	}
}
