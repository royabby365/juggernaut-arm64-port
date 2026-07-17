using UnityEngine;
using Yarx;

public class DefeatHud : MonoBehaviour
{
	public LevelItem LevelItemRecommended;

	public LevelItem LevelItemPremium;

	private CompositeDisposable _subscription;

	private ShopItemMk1 _recommendedShopItem;

	private ShopItemMk1 _premiumShopItem;

	private void OnEnable()
	{
		_subscription = new CompositeDisposable();
		_subscription.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnMsgGuiSwitchToPre));
	}

	private void OnDisable()
	{
		_subscription.Dispose();
	}

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += InstanceRelease;
			HudMk1.Instance.DragEndWithButton += InstanceRelease;
		}
	}

	private void InstanceRelease(SpriteButton obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.PopupDefeat)
		{
			if (obj.name == "button_goods_recomended")
			{
				int arg = ((_recommendedShopItem != null) ? _recommendedShopItem.AbsolutePointer : 0);
				_recommendedShopItem = null;
				Messenger<int>.Invoke(Globals.MsgShopChangePointer, arg);
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
			}
			if (obj.name == "button_goods_premium")
			{
				int arg2 = ((_premiumShopItem != null) ? _premiumShopItem.AbsolutePointer : 0);
				_premiumShopItem = null;
				Messenger<int>.Invoke(Globals.MsgShopChangePointer, arg2);
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
			}
		}
	}

	private void OnMsgGuiSwitchToPre(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.PopupDefeat && !(HudMk1.Instance == null))
		{
			ShopMk1 componentInChildren = HudMk1.Instance.GetComponentInChildren<ShopMk1>();
			if (LevelItemRecommended != null)
			{
				ShopItemMk1 randomRecommended = componentInChildren.GetRandomRecommended();
				LevelItemRecommended.ShopGood = randomRecommended.ShopGood;
				_recommendedShopItem = randomRecommended;
			}
			if (LevelItemPremium != null)
			{
				ShopItemMk1 randomPremium = componentInChildren.GetRandomPremium();
				LevelItemPremium.ShopGood = randomPremium.ShopGood;
				_premiumShopItem = randomPremium;
			}
		}
	}
}
