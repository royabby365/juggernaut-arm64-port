using System;
using UnityEngine;
using Yarx;

public class AlertContinue : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private Action _callback;

	public SpriteText Message;

	public SpriteButton FullScreen;

	public SpriteButton InnerButton;

	public Camera Viewport1;

	private bool _viewportState;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.PhrasesE>.AddListener(Globals.MsgShowAlert, OnShowAlert));
		_subscriptions.Add(Messenger<ServerData.PhrasesE, Action>.AddListener(Globals.MsgShowAlertWithCallback, OnMsgShowAlertWithCallback));
		_subscriptions.Add(Messenger<ServerData.HintCodesE>.AddListener(Globals.MsgShowHint, OnShowAltHint));
	}

	private void OnShowAltHint(ServerData.HintCodesE hintCode)
	{
		if (HudMk1.Instance == null || hintCode == ServerData.HintCodesE.none)
		{
			return;
		}
		string text = ((hintCode != ServerData.HintCodesE.none) ? SingletonT<ServerData>.I.GetHint(hintCode) : string.Empty);
		switch (hintCode)
		{
		case ServerData.HintCodesE.money:
		case ServerData.HintCodesE.cristals:
			Messenger<ServerData.PhrasesE, ServerData.PhrasesE, string, Action>.Invoke(Globals.MsgPopup2ButtonYesHandlerCustomMessage, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, text, delegate
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Bank);
			});
			break;
		case ServerData.HintCodesE.sculls:
		case ServerData.HintCodesE.vzlom:
		case ServerData.HintCodesE.scarab:
			Messenger<ServerData.PhrasesE, ServerData.PhrasesE, string, Action>.Invoke(Globals.MsgPopup2ButtonYesHandlerCustomMessage, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, text, delegate
			{
				Messenger<int>.Invoke(Globals.MsgShopChangeFilter, 3);
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
			});
			break;
		case ServerData.HintCodesE.healpotion:
		case ServerData.HintCodesE.powerpotion:
		case ServerData.HintCodesE.damagepotion:
			Messenger<ServerData.PhrasesE, ServerData.PhrasesE, string, Action>.Invoke(Globals.MsgPopup2ButtonYesHandlerCustomMessage, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, text, delegate
			{
				Messenger<int>.Invoke(Globals.MsgShopChangeFilter, 10);
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
			});
			break;
		case ServerData.HintCodesE.stathp:
		case ServerData.HintCodesE.statfury:
		case ServerData.HintCodesE.statstrength:
		case ServerData.HintCodesE.statmagic:
		case ServerData.HintCodesE.bonusfire:
		case ServerData.HintCodesE.bonusice:
		case ServerData.HintCodesE.bonusdark:
		case ServerData.HintCodesE.bonuslightning:
		case ServerData.HintCodesE.expa:
		case ServerData.HintCodesE.bonusexp:
		case ServerData.HintCodesE.bonusmoney:
		case ServerData.HintCodesE.bonusmana:
		case ServerData.HintCodesE.bonusrage:
		case ServerData.HintCodesE.map:
		case ServerData.HintCodesE.bonusarhimagia:
		case ServerData.HintCodesE.bonusarhifury:
		case ServerData.HintCodesE.socialbutton:
		case ServerData.HintCodesE.achivmentprogressbar:
		case ServerData.HintCodesE.legendarypoints:
			Message.Phrase_ = ServerData.PhrasesE.Custom;
			Message.Text_ = text;
			TurnOffViewport();
			HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.GlobalAlertPopup);
			break;
		}
	}

	private void TurnOffViewport()
	{
		_viewportState = Viewport1.enabled;
		Viewport1.enabled = false;
	}

	private void TurnOnViewport()
	{
		Viewport1.enabled = _viewportState;
	}

	private void OnShowAlert(ServerData.PhrasesE messagePhrase)
	{
		if (!(HudMk1.Instance == null))
		{
			Message.Phrase_ = messagePhrase;
			HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.GlobalAlertPopup);
			TurnOffViewport();
		}
	}

	private void OnMsgShowAlertWithCallback(ServerData.PhrasesE messagePhrase, Action callback)
	{
		_callback = callback;
		OnShowAlert(messagePhrase);
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += OnAlertClose;
		}
	}

	private void OnAlertClose(SpriteButton spriteButton)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (spriteButton.name == FullScreen.name || (InnerButton != null && spriteButton.name == InnerButton.name))
		{
			HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.GlobalAlertPopup);
			TurnOnViewport();
			if (_callback != null)
			{
				_callback();
				_callback = null;
			}
		}
		else if (spriteButton.name != "popup2button_yes")
		{
			AltButton altButton = spriteButton as AltButton;
			if (altButton != null)
			{
				OnShowAltHint(altButton.HintCode);
			}
		}
	}
}
