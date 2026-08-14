using System;
using System.Collections.Generic;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class Compare : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public CompareItem OldLeft;

	public CompareItem NewRight;

	public NewGuiButtonMk1 ButtonBuyPuton;

	public NewGuiButtonMk1 ButtonSell;

	public NewGuiButtonMk1 ButtonCancel;

	private HudMk1.GuiDesc _from = new HudMk1.GuiDesc(GuiRoot.GuiType.None, null);

	private string _buyPutOnName;

	private string _sellName;

	private string _cancellName;

	private ServerData.Item _newItem;

	private ServerData.Item _oldItem;

	private BagItemButton _toRemove;

	private readonly Queue<System.Tuple<string, bool, string, bool>> _itemsChanged = new Queue<System.Tuple<string, bool, string, bool>>();

	private ServerData.ShopGood _shopGood;

	private float _itemsChangeDt;

	private void Awake()
	{
		_buyPutOnName = ButtonBuyPuton.name;
		_sellName = ButtonSell.name;
		_cancellName = ButtonCancel.name;
	}

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += GuiOnRelease;
			HudMk1.Instance.DragEndWithButton += GuiOnRelease;
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.Item>.AddListener(Globals.MsgPlayerCompareItem, OnCompareItem));
		_subscriptions.Add(Messenger<BagItemButton>.AddListener(Globals.MsgPlayerRemoveItem, OnRemoveItem));
		_subscriptions.Add(Messenger<ServerData.ShopGood>.AddListener(Globals.MsgPlayerCompareShopGood, OnCompareShopGood));
		_subscriptions.Add(Messenger<System.Tuple<string, bool, string, bool>>.AddListener(Globals.MsgPlayerItemsChanged, OnPlayerItemsChanged));
	}

	private void OnPlayerItemsChanged(System.Tuple<string, bool, string, bool> tuple)
	{
		_itemsChanged.Enqueue(tuple);
	}

	private void OnCompareShopGood(ServerData.ShopGood shopGood)
	{
		_shopGood = shopGood;
		OnCompareItem(_shopGood.Item);
	}

	private void GuiOnRelease(SpriteButton spriteButton)
	{
		string text = spriteButton.name;
		if (text == _buyPutOnName)
		{
			if (_toRemove == null)
			{
				System.Tuple<string, bool, string, bool> changeStatsDigits = Extensions.GetChangeStatsDigits(_newItem, _oldItem);
				Messenger.Invoke(Globals.MsgPlayerItemsChanged, changeStatsDigits);
			}
			PutOnOffNewItem();
		}
		else if (text == _sellName)
		{
			SellNewItem();
		}
		else if (text == _cancellName)
		{
			Cancel();
		}
	}

	private void Cancel()
	{
		_newItem = null;
		_oldItem = null;
		_shopGood = null;
		_toRemove = null;
		if (!(HudMk1.Instance == null))
		{
			HudMk1.Instance.ChangeGuiTo(_from);
		}
	}

	private void SellNewItem()
	{
		ServerData.Item itemToSell = ((!(_toRemove == null)) ? _oldItem : _newItem);
		System.Tuple<ServerData.MoneyType.TypeE, int, string> itemSellPrice = itemToSell.GetItemSellPrice();
		Messenger<ServerData.PhrasesE, ServerData.PhrasesE, string, Action>.Invoke(Globals.MsgPopup2ButtonYesHandlerCustomMessage, ServerData.PhrasesE.ButtonSell, ServerData.PhrasesE.ButtonCancel, string.Format(SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.CompareSellItem), itemSellPrice.Item2 + itemSellPrice.Item3), delegate
		{
			ServerData.Item item = itemToSell;
			SingletonT<ServerData>.I.SellItem(item);
			Messenger.Invoke(Globals.MsgBagNeedRefresh);
			ShopMk1.RecommendationsNeedRegeneration = true;
			Cancel();
		});
	}

	private void PutOnOffNewItem()
	{
		ServerData.Item newItem = _newItem;
		if (_toRemove != null)
		{
			BagInventory.Instance.RemoveFromPers(_toRemove, showStatChanging: true);
			BagInventory.Instance.RearrangeBag();
		}
		else
		{
			if (_oldItem != null)
			{
				_oldItem.PutOn = false;
			}
			newItem.SetPutonTrue();
		}
		Messenger.Invoke(Globals.MsgBagNeedRefresh);
		Cancel();
	}

	private void OnRemoveItem(BagItemButton itemButton)
	{
		if (!(HudMk1.Instance == null))
		{
			_from = HudMk1.Instance.CurrentGui;
			_oldItem = itemButton.item;
			_toRemove = itemButton;
			_newItem = null;
			ButtonBuyPuton.Label.Phrase_ = ServerData.PhrasesE.ButtonPutoff;
			Vector3 localPosition = OldLeft.transform.localPosition;
			OldLeft.transform.localPosition = new Vector3(300f, localPosition.y, localPosition.z);
			Vector3 localPosition2 = NewRight.transform.localPosition;
			NewRight.transform.localPosition = new Vector3(-600f, localPosition2.y, localPosition2.z);
			OldLeft.Item = _oldItem;
			OldLeft.OppsiteItem = _newItem;
			NewRight.Item = _newItem;
			NewRight.OppsiteItem = _oldItem;
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Compare);
		}
	}

	private void OnCompareItem(ServerData.Item item)
	{
		if (!(HudMk1.Instance == null))
		{
			_from = HudMk1.Instance.CurrentGui;
			_newItem = item;
			if (_from.Type == GuiRoot.GuiType.BagItems || _from.Type == GuiRoot.GuiType.BagStats)
			{
				ButtonBuyPuton.Label.Phrase_ = ServerData.PhrasesE.ButtonPuton;
				ButtonSell.transform.localPosition = Vector3.zero;
			}
			else if (_from.Type == GuiRoot.GuiType.Shop)
			{
				ButtonBuyPuton.Label.Phrase_ = ServerData.PhrasesE.ButtonBuy;
				ButtonSell.transform.GoToHell();
			}
			_oldItem = item.GetPuppetItem();
			Vector3 localPosition = OldLeft.transform.localPosition;
			int num = ((_oldItem == null) ? (-300) : 0);
			OldLeft.transform.localPosition = new Vector3(num, localPosition.y, localPosition.z);
			Vector3 localPosition2 = NewRight.transform.localPosition;
			NewRight.transform.localPosition = new Vector3(300f, localPosition2.y, localPosition2.z);
			OldLeft.Item = _oldItem;
			OldLeft.OppsiteItem = item;
			NewRight.Item = item;
			NewRight.OppsiteItem = _oldItem;
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Compare);
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Update()
	{
		_itemsChangeDt += Time.deltaTime;
		if (_itemsChanged.Count > 0 && _itemsChangeDt > 2f)
		{
			System.Tuple<string, bool, string, bool> tuple = _itemsChanged.Dequeue();
			SingletonT<SoundManager>.I.PlayGlobalSound("click_change_armor");
			Globals.Player.ShowStatsChanging(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}
	}
}
