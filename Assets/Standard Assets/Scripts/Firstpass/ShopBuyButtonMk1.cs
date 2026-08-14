using System;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class ShopBuyButtonMk1 : SpriteButton
{
	public enum ShopButtonState
	{
		NeedEquip,
		Equipped,
		Buy,
		Disable,
		HideForever
	}

	private const ServerData.PhrasesE BuyPhrase = ServerData.PhrasesE.ShopBuy;

	private const ServerData.PhrasesE Equpped = ServerData.PhrasesE.ShopAlreadyPuton;

	private const ServerData.PhrasesE EquipMe = ServerData.PhrasesE.ButtonPuton;

	private CompositeDisposable _subscriptions;

	private ShopItemMk1 _myShopItem;

	public SpriteText LabelTop;

	public SpriteText LabelPrice;

	public Sprite Button;

	public int ColliderBorderX;

	public int ColliderBorderY;

	public float EnteredScaleX = 1.1f;

	public float EnteredScaleY = 1.1f;

	public ServerData.PhrasesE TopPhrase;

	public Color EnteredTint = new Color(0.8f, 0.8f, 0.8f, 1f);

	public Color LeavedTint = new Color(0.5f, 0.5f, 0.5f, 1f);

	private Vector3 _startScale;

	private string _priceText;

	private Vector3 _buyLabelPosition;

	public ShopItemMk1 MyShopItem
	{
		private get
		{
			return _myShopItem;
		}
		set
		{
			_myShopItem = value;
			SetPrice(_myShopItem.ShopGood);
		}
	}

	private void SetPrice(ServerData.ShopGood shopGood)
	{
		System.Tuple<ServerData.MoneyType.TypeE, int, string> itemBuyPrice = shopGood.GetItemBuyPrice();
		_priceText = itemBuyPrice.Item3 + itemBuyPrice.Item2;
	}

	public void SetState(ShopButtonState state)
	{
		switch (state)
		{
		case ShopButtonState.NeedEquip:
			DimText(p0: false);
			base.gameObject.SetActiveRecursivelyMk1(setActive: true);
			Button.SpriteName_ = "big_shop_button_green";
			SetActive();
			LabelTop.Phrase_ = ServerData.PhrasesE.ButtonPuton;
			LabelTop.transform.localPosition = _buyLabelPosition + 13f * Vector3.down;
			LabelPrice.Text_ = string.Empty;
			break;
		case ShopButtonState.Equipped:
			DimText(p0: true);
			base.gameObject.SetActiveRecursivelyMk1(setActive: true);
			Button.SpriteName_ = "big_shop_button_sepia";
			SetInactive();
			LabelTop.Phrase_ = ServerData.PhrasesE.ShopAlreadyPuton;
			LabelTop.transform.localPosition = _buyLabelPosition + 13f * Vector3.down;
			LabelPrice.Text_ = string.Empty;
			break;
		case ShopButtonState.Buy:
			DimText(p0: false);
			base.gameObject.SetActiveRecursivelyMk1(setActive: true);
			Button.SpriteName_ = "big_shop_button_red";
			SetActive();
			LabelTop.Phrase_ = ServerData.PhrasesE.ShopBuy;
			LabelTop.transform.localPosition = _buyLabelPosition;
			LabelPrice.Text_ = _priceText;
			break;
		case ShopButtonState.Disable:
			DimText(p0: true);
			base.gameObject.SetActiveRecursivelyMk1(setActive: true);
			Button.SpriteName_ = "big_shop_button_sepia";
			SetInactive();
			LabelTop.Phrase_ = ServerData.PhrasesE.ShopBuy;
			LabelTop.transform.localPosition = _buyLabelPosition;
			LabelPrice.Text_ = _priceText;
			break;
		case ShopButtonState.HideForever:
			base.gameObject.SetActiveRecursivelyMk1(setActive: false);
			break;
		default:
			throw new ArgumentOutOfRangeException("state");
		}
	}

	private void DimText(bool p0)
	{
		if (p0)
		{
			LabelTop.NamedColorE = FontManager.ColorE.BagSectionColor;
			LabelPrice.NamedColorE = FontManager.ColorE.BagSectionColor;
		}
		else
		{
			LabelTop.NamedColorE = FontManager.ColorE.BagMoney;
			LabelPrice.NamedColorE = FontManager.ColorE.BagMoney;
		}
	}

	private void Awake()
	{
		base.name = $"{base.name}_{SpriteGui.UniqueId}";
		_startScale = base.transform.localScale;
		LabelTop.Phrase_ = TopPhrase;
		_buyLabelPosition = LabelTop.transform.localPosition;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		Init(ColliderBorderX, ColliderBorderY);
	}

	public override void Released()
	{
		base.Released();
		MyShopItem.DoPurchaseItem();
	}

	public override void SetActive()
	{
		base.SetActive();
		DoSetTint(LeavedTint);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		DoSetTint(LeavedTint);
	}

	public override void Entered()
	{
		base.Entered();
		DoSetTint(EnteredTint);
		if (!EnteredScaleX.Eqv(1f) || !EnteredScaleY.Eqv(1f))
		{
			base.transform.localScale = new Vector3(EnteredScaleX, EnteredScaleY, 1f);
		}
	}

	public override void Left()
	{
		base.Left();
		DoSetTint(LeavedTint);
		base.transform.localScale = _startScale;
	}

	private void DoSetTint(Color tint)
	{
		Button.Tint_ = tint;
	}
}
