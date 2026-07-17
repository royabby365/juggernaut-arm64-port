using UnityEngine;
using Yarx;

public class BagSwitch : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteButton ToBagItems;

	public SpriteButton ToBagStats;

	private HudMk1 _hud;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, Handler));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		_hud = base.transform.GetSpriteGui() as HudMk1;
		(_hud != null).Assert();
		if (_hud != null)
		{
			_hud.Release += HudOnRelease;
		}
	}

	private void HudOnRelease(SpriteButton spriteButton)
	{
		switch (spriteButton.name)
		{
		case "to_bag_items":
			_hud.ChangeGuiTo(GuiRoot.GuiType.BagItems);
			break;
		case "to_bag_stats":
			_hud.ChangeGuiTo(GuiRoot.GuiType.BagStats);
			break;
		}
	}

	private void Handler(GuiRoot.GuiType from, GuiRoot.GuiType to)
	{
		switch (to)
		{
		case GuiRoot.GuiType.BagStats:
			ToBagStats.SetInactive();
			ToBagStats.SetSelected();
			ToBagItems.SetActive();
			ToBagItems.SetUnselected();
			break;
		case GuiRoot.GuiType.BagItems:
			ToBagStats.SetActive();
			ToBagStats.SetUnselected();
			ToBagItems.SetInactive();
			ToBagItems.SetSelected();
			break;
		}
	}
}
