using System;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class Storyline : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private ServerData.StorylineDialog _currentDialogs;

	private int _dialogIndex;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPost, OnMsgGuiSwitchToPost));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void OnMsgGuiSwitchToPost(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (HudMk1.Instance == null || @new != GuiRoot.GuiType.Fight)
		{
			return;
		}
		if (_currentDialogs == null)
		{
			_currentDialogs = GetCurrentDialog();
			_dialogIndex = 0;
		}
		else
		{
			Yarx.Collections.Tuple<int, int, int> currentKey = GetCurrentKey();
			if (!_currentDialogs.LocationBot.Equals(currentKey))
			{
				_currentDialogs = GetCurrentDialog();
				_dialogIndex = 0;
			}
		}
		if (_currentDialogs == null || _currentDialogs.Dialogs == null || _currentDialogs.Dialogs.Count <= 0)
		{
			return;
		}
		BattleCameraController controller = Globals.Battle.BattleCameraController;
		FightScreenFightButton fightButton = HudMk1.Instance.GetComponentInChildren<FightScreenFightButton>();
		while (_dialogIndex < _currentDialogs.Dialogs.Count)
		{
			controller.GoToDialogState();
			fightButton.transform.parent.GoToHell();
			ServerData.DialogPhrase dialog = _currentDialogs.Dialogs[_dialogIndex++];
			bool lastDialog = _dialogIndex >= _currentDialogs.Dialogs.Count;
			TutorialFullScreenInfo.Dialogs.Enqueue(delegate
			{
				if (lastDialog)
				{
					Messenger<string, int, string, Action>.Invoke(Globals.MsgShowStorylineDialog, dialog.Text, dialog.Npc, dialog.Origin, delegate
					{
						controller.GoToStartState();
						fightButton.transform.parent.localPosition = new Vector3(-4f, -50f, -50f);
					});
				}
				else
				{
					Messenger<string, int, string, Action>.Invoke(Globals.MsgShowStorylineDialog, dialog.Text, dialog.Npc, dialog.Origin, controller.GoToDialogState);
				}
			});
		}
	}

	private void ShowDialog()
	{
		if (_currentDialogs != null && _dialogIndex < _currentDialogs.Dialogs.Count)
		{
			ServerData.DialogPhrase dialogPhrase = _currentDialogs.Dialogs[_dialogIndex];
			Messenger<string, int, string, Action>.Invoke(Globals.MsgShowStorylineDialog, dialogPhrase.Text, dialogPhrase.Npc, dialogPhrase.Origin, StorylineHandler);
		}
	}

	private void StorylineHandler()
	{
		_dialogIndex++;
		ShowDialog();
	}

	private ServerData.StorylineDialog GetCurrentDialog()
	{
		Yarx.Collections.Tuple<int, int, int> currentKey = GetCurrentKey();
		ServerData.StorylineDialog value = null;
		if (currentKey != null)
		{
			SingletonT<ServerData>.I._storylineDialogs.TryGetValue(currentKey, out value);
		}
		return value;
	}

	private Yarx.Collections.Tuple<int, int, int> GetCurrentKey()
	{
		int locationProgress = SingletonT<ServerData>.I.GetLocationProgress(AreaData.Current.Location);
		AreaData.MobData mobData = null;
		if (locationProgress < AreaData.Current.Mobs.Length)
		{
			mobData = AreaData.Current.Mobs[locationProgress];
		}
		if (mobData == null)
		{
			return null;
		}
		return Yarx.Collections.Tuple.Create(AreaData.Current.Location.Id, mobData.ServerInfo.Id, mobData.Level);
	}
}
