using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class BankHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private Dictionary<SpriteButton, ServerData.BankItem> _buttons;

	private GuiRoot.GuiType _prev;

	private bool _waitTimeout;

	public Transform BankRoot;

	public GameObject DiamondPrefab;

	public GameObject GoldPrefab;

	public GameObject RealPrefab;

	public GameObject PlatePrefab;

	public GameObject WaitMessage;

	public Transform FreeCristalsButton;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgBankBuyDoneSuccessful, OnMsgBankBuyDoneSuccessful));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgBankBuyDoneFail, OnMsgBankBuyDoneFail));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		Init();
		Vector3 localPosition = FreeCristalsButton.localPosition;
		localPosition.y = 4f + (float)Camera2D.ScreenHeight / 2f;
		FreeCristalsButton.localPosition = localPosition;
	}

	public void Init()
	{
		int num = 0;
		int num2 = 72;
		_buttons = new Dictionary<SpriteButton, ServerData.BankItem>();
		foreach (ServerData.BankItem bankItem in SingletonT<ServerData>.I._bankItems)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(PlatePrefab);
			Plate component = gameObject.GetComponent<Plate>();
			gameObject.transform.parent = BankRoot;
			gameObject.transform.localPosition = new Vector3(0f, 130 - num * num2, -50f);
			GameObject gameObject2 = ((bankItem.CountType.Type != ServerData.MoneyType.TypeE.Diamond) ? ((GameObject)UnityEngine.Object.Instantiate(GoldPrefab)) : ((GameObject)UnityEngine.Object.Instantiate(DiamondPrefab)));
			gameObject2.transform.parent = component.PivotCount;
			gameObject2.transform.localPosition = default(Vector3);
			Counter component2 = gameObject2.GetComponent<Counter>();
			component2.Count = bankItem.Count + bankItem.Bonus;
			if (bankItem.BonusPercent > 0)
			{
				GameObject gameObject3 = ((bankItem.CountType.Type != ServerData.MoneyType.TypeE.Diamond) ? ((GameObject)UnityEngine.Object.Instantiate(GoldPrefab)) : ((GameObject)UnityEngine.Object.Instantiate(DiamondPrefab)));
				gameObject3.transform.parent = component.PivotBonus;
				gameObject3.transform.localPosition = default(Vector3);
				component2 = gameObject3.GetComponent<Counter>();
				component2.IsBonus = true;
				component2.Count = bankItem.BonusPercent;
			}
			GameObject gameObject4 = (GameObject)UnityEngine.Object.Instantiate(RealPrefab);
			gameObject4.transform.parent = component.PivotReal;
			gameObject4.transform.localPosition = default(Vector3);
			component2 = gameObject4.GetComponent<Counter>();
			component2.Count = bankItem.Real;
			component.Button.name = $"button_bank_{num:00}";
			component.Button.Init();
			component.Button.SetActive();
			_buttons.Add(component.Button, bankItem);
			if (bankItem.Selected)
			{
				GameObject gameObject5 = new GameObject();
				Sprite sprite = gameObject5.AddComponent<Sprite>();
				sprite.Origin = Quad.OriginPlace.Center;
				gameObject5.transform.parent = component.PivotSelected;
				gameObject5.transform.localPosition = new Vector3(-8f, 0f, -50f);
				gameObject5.transform.SetLayerRecursively(component.PivotSelected);
				Sprite component3 = component.gameObject.GetComponent<Sprite>();
				sprite.SpriteName_ = "best_buy";
				component3.SpriteName_ = "popup_1_red";
			}
			num++;
		}
		SpriteGui spriteGui = base.transform.GetSpriteGui();
		spriteGui.Release += Gui_Release;
	}

	private void OnMsgBankBuyDoneSuccessful()
	{
		HideWaitMessage();
		_waitTimeout = false;
	}

	private void OnMsgBankBuyDoneFail()
	{
		HideWaitMessage();
		_waitTimeout = false;
	}

	private void Gui_Release(SpriteButton obj)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Bank || !_buttons.TryGetValue(obj, out var bankItem))
		{
			return;
		}
		if (Globals.DebugShopBuyLocal)
		{
			ShowWaitMessage(2f);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(3f, delegate
			{
				SingletonT<ServerData>.I.BankBuy(bankItem);
				Messenger.Invoke(Globals.MsgBankBuyDoneSuccessful);
			});
		}
		else if (UnityApi.NeedPurchaseConfirmation())
		{
			Messenger<ServerData.PhrasesE, ServerData.PhrasesE, string, Action>.Invoke(Globals.MsgPopup2ButtonYesHandlerCustomMessage, ServerData.PhrasesE.ButtonYes, ServerData.PhrasesE.ButtonNo, UnityApi.TranslatePurchaseConfirmation(bankItem), delegate
			{
				ShowWaitMessage(60f);
				UnityApi.BankBuy(bankItem);
			});
		}
		else
		{
			ShowWaitMessage(60f);
			UnityApi.BankBuy(bankItem);
		}
	}

	private IEnumerator WaitTimeout(float timeout)
	{
		_waitTimeout = true;
		float time = 0f;
		while (time < timeout && _waitTimeout)
		{
			time += Time.deltaTime;
			yield return null;
		}
		HideWaitMessage();
		if (_waitTimeout)
		{
			Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowAlert, ServerData.PhrasesE.BankTimeout);
			_waitTimeout = false;
		}
	}

	private void ShowWaitMessage(float timeout)
	{
		SpriteGui.DontReleaseButtons = true;
		WaitMessage.SetActiveRecursivelyMk1(setActive: true);
		StartCoroutine(WaitTimeout(timeout));
	}

	private void HideWaitMessage()
	{
		SpriteGui.DontReleaseButtons = false;
		WaitMessage.SetActiveRecursivelyMk1(setActive: false);
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.Bank)
		{
			_prev = old;
			WaitMessage.SetActiveRecursivelyMk1(setActive: false);
		}
	}
}
