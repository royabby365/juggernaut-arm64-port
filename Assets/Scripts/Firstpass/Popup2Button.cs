using System;
using UnityEngine;
using Yarx;

public class Popup2Button : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public NewGuiButtonMk1 Yes;

	public NewGuiButtonMk1 No;

	public SpriteText Message;

	private SpriteText _yesLabel;

	private SpriteText _noLabel;

	private Action _yesHandler;

	private void Awake()
	{
		_yesLabel = Yes.Label;
		_noLabel = No.Label;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.PhrasesE, ServerData.PhrasesE, ServerData.PhrasesE, Action>.AddListener(Globals.MsgPopup2ButtonYesHandler, OnPopup2Buttons));
		_subscriptions.Add(Messenger<ServerData.PhrasesE, ServerData.PhrasesE, string, Action>.AddListener(Globals.MsgPopup2ButtonYesHandlerCustomMessage, OnPopup2ButtonsCustomMessage));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += OnYesNoRelease;
			HudMk1.Instance.DragEndWithButton += OnYesNoRelease;
		}
	}

	private void OnYesNoRelease(SpriteButton spriteButton)
	{
		if (!(HudMk1.Instance == null))
		{
			if (spriteButton.name == Yes.name && _yesHandler != null)
			{
				_yesHandler();
			}
			if (spriteButton.name == Yes.name || spriteButton.name == No.name)
			{
				HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.FullScreenPopup2Button);
			}
		}
	}

	private void OnPopup2Buttons(ServerData.PhrasesE yes, ServerData.PhrasesE no, ServerData.PhrasesE message, Action yesHandler)
	{
		if (!(HudMk1.Instance == null))
		{
			_yesHandler = yesHandler;
			_yesLabel.Phrase_ = yes;
			_noLabel.Phrase_ = no;
			Message.Phrase_ = message;
			HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.FullScreenPopup2Button);
		}
	}

	private void OnPopup2ButtonsCustomMessage(ServerData.PhrasesE yes, ServerData.PhrasesE no, string message, Action yesHandler)
	{
		if (!(HudMk1.Instance == null))
		{
			_yesHandler = yesHandler;
			_yesLabel.Phrase_ = yes;
			_noLabel.Phrase_ = no;
			Message.Phrase_ = ServerData.PhrasesE.Custom;
			Message.Text_ = message;
			HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.FullScreenPopup2Button);
		}
	}
}
