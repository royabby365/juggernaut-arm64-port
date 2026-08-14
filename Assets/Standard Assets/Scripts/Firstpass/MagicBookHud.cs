using System;
using UnityEngine;
using Yarx;

public class MagicBookHud : MonoBehaviour
{
	private const string ImproveButtonStem = "improve_magic_";

	private CompositeDisposable _subscriptions;

	public SpriteText GlobalSkullCounter;

	public SpriteText MessageArea;

	public MagicBookSchool Fire;

	public MagicBookSchool Ice;

	public MagicBookSchool Dark;

	public MagicBookSchool Lightning;

	private SpriteGui _gui;

	private ServerData.PhrasesE _defaultMessage;

	private void Awake()
	{
		_gui = base.transform.GetSpriteGui();
		_defaultMessage = MessageArea.Phrase_;
	}

	public void Init()
	{
		SetupSkullCounter();
		Fire.Init();
		Ice.Init();
		Dark.Init();
		Lightning.Init();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.MoneyType.TypeE, string>.AddListener(Globals.MsgPlayerFundsChanged, OnPlayerFundsChanged));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgMagicBookNeedMoreSkulls, OnNeedMoreSkulls));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgMagicBookNeedMoreUsings, OnNeedMoreUsings));
		_gui.Release += ProcessSpellButtons;
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
		_gui.Release -= ProcessSpellButtons;
	}

	private void OnNeedMoreUsings()
	{
		Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowAlert, ServerData.PhrasesE.MBookNeedMoreSkill);
	}

	private void OnNeedMoreSkulls()
	{
		if (!(HudMk1.Instance == null))
		{
			Messenger<ServerData.PhrasesE, ServerData.PhrasesE, ServerData.PhrasesE, Action>.Invoke(Globals.MsgPopup2ButtonYesHandler, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, ServerData.PhrasesE.MBookNeedMoreSkulls, delegate
			{
				Messenger.Invoke(Globals.MsgShopSkullInsufficient);
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
			});
		}
	}

	private void ProcessSpellButtons(SpriteButton button)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.MagicBook || !button.name.Contains("improve_magic_"))
		{
			return;
		}
		switch (button.name.Substring("improve_magic_".Length))
		{
		case "fire":
			Fire.ButtonPressed();
			return;
		case "ice":
			Ice.ButtonPressed();
			return;
		case "dark":
			Dark.ButtonPressed();
			return;
		case "electro":
			Lightning.ButtonPressed();
			return;
		}
		if (Globals.IsDebugBuild)
		{
			Debug.LogError("inncorect button name -- {0}".Fmt(button.name));
		}
	}

	private void OnPlayerFundsChanged(ServerData.MoneyType.TypeE typeE, string reason)
	{
		if (typeE == ServerData.MoneyType.TypeE.Skull)
		{
			SetupSkullCounter();
		}
	}

	private void SetupSkullCounter()
	{
		ServerData.MoneyType.TypeE type = ServerData.MoneyType.TypeE.Skull;
		string text_ = type.GetPlayerFundsCount().ToString();
		GlobalSkullCounter.Text_ = text_;
	}
}
