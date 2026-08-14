using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class TutorialTip : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private SpriteText _tip;

	private static Vector3 _startPos = new Vector3(0f, -100f, -500f);

	private Vector3 _strongMagic;

	private Vector3 _weakMagic;

	private static readonly Vector3 _hidePos = new Vector3(0f, 5000f, 1000f);

	private static readonly HashSet<GuiRoot.GuiType> StaticMessages = new HashSet<GuiRoot.GuiType>
	{
		GuiRoot.GuiType.Execution,
		GuiRoot.GuiType.CastMagic,
		GuiRoot.GuiType.StrongMagicMiniGame,
		GuiRoot.GuiType.WeakMagicMiniGame
	};

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.PhrasesE>.AddListener(Globals.MsgGuiBattle_ShowPhrase, ShowTip));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgGuiBattle_HidePhrase, HideTip));
		_subscriptions.Add(Messenger<ServerData.PhrasesE>.AddListener(Globals.MsgGuiBattle_FlashPhrase, OnFlashPhrase));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPost, OnGuiChangedPost));
	}

	private void OnFlashPhrase(ServerData.PhrasesE phrase)
	{
		_tip.Phrase_ = phrase;
		StartCoroutine("ShowTipCoro");
	}

	private void OnGuiChangedPost(GuiRoot.GuiType from, GuiRoot.GuiType toType)
	{
		if (StaticMessages.Contains(from))
		{
			base.transform.localPosition = _hidePos;
		}
		switch (toType)
		{
		case GuiRoot.GuiType.Execution:
			_tip.Phrase_ = ServerData.PhrasesE.ExecutionHelp;
			base.transform.localPosition = _weakMagic;
			break;
		case GuiRoot.GuiType.CastMagic:
			_tip.Phrase_ = ServerData.PhrasesE.CastGestureHelp;
			base.transform.localPosition = _startPos;
			break;
		case GuiRoot.GuiType.StrongMagicMiniGame:
			_tip.Phrase_ = ServerData.PhrasesE.MagicProtectionHelp;
			base.transform.localPosition = _strongMagic;
			break;
		case GuiRoot.GuiType.WeakMagicMiniGame:
			_tip.Phrase_ = ServerData.PhrasesE.WeakMagicMessage;
			base.transform.localPosition = _weakMagic;
			break;
		case GuiRoot.GuiType.BattleResults:
			break;
		}
	}

	private void HideTip()
	{
		StopCoroutine("ShowTipCoro");
		base.transform.localPosition = _hidePos;
	}

	private void ShowTip(ServerData.PhrasesE phrase)
	{
		_tip.Phrase_ = phrase;
		base.transform.localPosition = _startPos;
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		_startPos = base.transform.localPosition;
		base.transform.localPosition = _hidePos;
		if (Globals.IsDebugBuild)
		{
			Debug.Log("==== TO HIDE POS ====");
		}
		_tip = GetComponent<SpriteText>();
		_strongMagic = new Vector3(0f, 50 - Camera2D.ScreenHeight, -500f);
		_weakMagic = _startPos + 60f * Vector3.down;
	}

	private IEnumerator ShowTipCoro()
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log("==== TO HIDE POS ====");
		}
		base.transform.localPosition = _startPos;
		yield return new WaitForSeconds(3f);
		base.transform.localPosition = _hidePos;
	}
}
