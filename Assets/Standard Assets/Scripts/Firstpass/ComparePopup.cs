using System;
using UnityEngine;
using Yarx;

public class ComparePopup : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public ComparePopupMk1 CompareItem;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.Bonus.DropElement>.AddListener(Globals.MsgCompareDropElement, OnCompareDropElement));
		_subscriptions.Add(Messenger<ServerData.Bonus.DropElement>.AddListener(Globals.MsgCompareExtraDropElement, OnCompareDropElement));
	}

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += OnButtonRelease;
			HudMk1.Instance.DragEndWithButton += OnButtonRelease;
		}
	}

	private void OnButtonRelease(SpriteButton spriteButton)
	{
		if (!(HudMk1.Instance == null) && spriteButton != null && spriteButton.name == "_catch_all_compare_button")
		{
			HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.GlobalComparePopup);
		}
	}

	private void OnCompareDropElement(ServerData.Bonus.DropElement dropElement)
	{
		if (!(HudMk1.Instance == null))
		{
			if (dropElement == null)
			{
				throw new ArgumentNullException("dropElement");
			}
			if (CompareItem == null)
			{
				throw new ArgumentNullException("CompareItem");
			}
			CompareItem.SetDropElement(dropElement);
			HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.GlobalComparePopup);
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Update()
	{
	}
}
