using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleGui : MonoBehaviour
{
	internal static readonly string ComboButtonName = "combo_button";

	public GameObject Timeout;

	public GameObject FxDamageHP;

	private bool _showEndDialog;

	private List<IDisposable> _listeners;

	internal bool BattleResultContinueWasClicked;

	private void OnEnable()
	{
		if (!(HudMk1.Instance == null))
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BattleHud);
			Battle battle = Globals.Battle;
			if (battle == null)
			{
				battle = Utils.FindObjectOfType<Battle>();
				Invs.Inv(battle != null, "_battle != null");
			}
			_showEndDialog = false;
			_listeners = new List<IDisposable>();
			_listeners.Add(Messenger<Battle.StateE>.AddListener(Globals.MsgBattleStateChanged, _battle_TurnStateChanged));
			_listeners.Add(Messenger.AddListener(Globals.MsgFightShowEndDialog, MsgBattleShowEndDialogHandler));
			_listeners.Add(Messenger<string>.AddListener(Globals.MsgGuiButtonPressed, MsgGuiButtonPressedHandler));
			_listeners.Add(Messenger.AddListener(Globals.MsgFightStarted, delegate
			{
				BattleResultContinueWasClicked = false;
			}));
			Messenger.Invoke(Globals.MsgGuiBattle_HideYouWin);
			Messenger.Invoke(Globals.MsgGuiBattle_HideYouLose);
			Messenger.Invoke(Globals.MsgGuiBattle_ShowFatalityBar, arg1: false);
			Messenger.Invoke(Globals.MsgGuiBattle_PlayerLevel, SingletonT<ServerData>.I.PlayerParams.Level);
			Messenger.Invoke(Globals.MsgGuiBattle_EnemyLevel, SingletonT<ServerData>.I.EnemyParams.Level);
			if (Globals.Player != null)
			{
				Messenger.Invoke(Globals.MsgPlayerHealthChanged, Globals.Player.Health, Globals.Player.MaxHealth);
			}
			if (Globals.Enemy != null)
			{
				Messenger.Invoke(Globals.MsgEnemyHealthChanged, Globals.Enemy.Health, Globals.Enemy.MaxHealth);
			}
			Messenger.Invoke(Globals.MsgGuiBattle_EnemyAvatar, Globals.Enemy.ServerBotInfo.Picture);
			ServerData.Item item = SingletonT<ServerData>.I.FindInBag((ServerData.Item _) => _.PutOn && _.Slot.SlotId == ServerData.Slot.TypeE.Helm);
			Messenger.Invoke(Globals.MsgGuiBattle_PlayerAvatar, (item == null) ? string.Empty : item.PictureInBattle);
		}
	}

	private void OnDisable()
	{
		foreach (IDisposable listener in _listeners)
		{
			listener.Dispose();
		}
		_listeners.Clear();
	}

	private void Update()
	{
		if (!_showEndDialog)
		{
			ShowTurnTimeRemains();
		}
	}

	private void YouWinPressed()
	{
		if (!(HudMk1.Instance == null))
		{
			int level = SingletonT<ServerData>.I.PlayerParams.Level;
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BattleResults);
			_showEndDialog = false;
		}
	}

	private void MsgBattleShowEndDialogHandler()
	{
		if (!(HudMk1.Instance == null))
		{
			_showEndDialog = true;
			Messenger<bool>.Invoke(Globals.MsgGuiBattle_ShowFatalityBar, arg1: false);
			Messenger<bool>.Invoke(Globals.MsgGuiBattle_ShowItemsBar, arg1: false);
			if (!Globals.Player.IsDead)
			{
				YouWinPressed();
				return;
			}
			Metrics.OnFightFinish(victory: false, SingletonT<ServerData>.I.PlayerParams.Level, Globals.Enemy.ServerBotInfo, Globals.Enemy.FromLocation, Battle.FatalityStateE.None, Globals.Battle.TurnsCount);
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.PopupDefeat);
			Messenger.Invoke(Globals.MsgPlayerDefeated);
		}
	}

	private void _battle_TurnStateChanged(Battle.StateE state)
	{
		switch (state)
		{
		case Battle.StateE.FatalityMode:
			Messenger<bool>.Invoke(Globals.MsgGuiBattle_ShowFatalityBar, arg1: true);
			Messenger<int, int>.Invoke(Globals.MsgFatalityModeSlicesChanged, 0, 1);
			break;
		case Battle.StateE.FatalityModeExecute:
			Messenger<bool>.Invoke(Globals.MsgGuiBattle_ShowFatalityBar, arg1: false);
			break;
		}
	}

	private void Start()
	{
		Messenger.Invoke(Globals.MsgGuiBattle_ShowItemsBar, arg1: true);
	}

	private void MsgGuiButtonPressedHandler(string button)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		switch (button)
		{
		case "item_crit":
			if (!Globals.Battle.Pause)
			{
				Globals.Player.EatAttackForceElixir();
			}
			return;
		case "item_poison":
			if (!Globals.Battle.Pause)
			{
				Globals.Player.CastPoisonElixir();
			}
			return;
		case "item_life":
			if (!Globals.Battle.Pause)
			{
				Globals.Player.EatHealthPotion(Globals.Battle.PlayerHealFX1);
			}
			return;
		case "use_magic_button":
			if (!Globals.ForceDontClickManaButton)
			{
				if (!Globals.Battle.SpellButtonPressed() && Globals.Battle.State == Battle.StateE.PlayerTime)
				{
					HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BattleHud);
				}
				return;
			}
			break;
		}
		if (button == "use_rage1_button")
		{
			if (!Globals.Battle.IsInMagicMode && !Globals.ForceDontProcessRageButtons)
			{
				Messenger.Invoke(Globals.MsgRageSphereUse, 1);
			}
			return;
		}
		if (button == "use_rage2_button" && !Globals.ForceDontProcessRageButtons)
		{
			if (!Globals.Battle.IsInMagicMode)
			{
				Messenger.Invoke(Globals.MsgRageSphereUse, 2);
			}
			return;
		}
		if (button == "use_rage3_button" && !Globals.ForceDontProcessRageButtons)
		{
			if (!Globals.Battle.IsInMagicMode)
			{
				Messenger.Invoke(Globals.MsgRageSphereUse, 3);
			}
			return;
		}
		switch (button)
		{
		case "next_attack_button":
			if (Globals.Battle.IsComboAllowed)
			{
				if (Globals.Battle.State == Battle.StateE.PlayerTime)
				{
					Globals.Battle.AttackCombo();
				}
			}
			else
			{
				Globals.Battle.TryDoComboAttack(Globals.Battle.ComboAttackDoneIndex);
			}
			break;
		case "battle_results_to_bag":
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.None);
			Globals.ShowLoadingScreen(delegate
			{
				Messenger.Invoke(Globals.MsgFightResult_GoToBag);
			});
			break;
		case "battle_results_continue":
			if (Battle.LocationToIncrement != null)
			{
				ServerData.Location locationToIncrement = Battle.LocationToIncrement;
				Battle.LocationToIncrement = null;
				SingletonT<ServerData>.I.IncLocationProgress(locationToIncrement);
				if (LocationLogic._chapterBonus != null)
				{
					SingletonT<ServerData>.I.AddToBag(LocationLogic._chapterBonus);
					LocationLogic._chapterBonus = null;
				}
				if (!AreaData.Current.Location.IsZachistkaOpened && Globals.MainMenu._lastLocationOrMap != GuiRoot.GuiType.Location)
				{
					Globals.MainMenu.AddOpenCondition(MainMenu.EventTypeE.ChapterDone, 0);
				}
			}
			else if (Globals.IsDebugBuild)
			{
				Debug.LogError("ERROR -- Battle.LocationToIncrement == null");
			}
			ServerData.MoneyType.TypeE.Gold.ChangePlayerFundsCount(SingletonT<ServerData>.I.PlayerInBattleParams.BonusMoney);
			Globals.MainMenu.SaveGame();
			if (Globals.MainMenu._openConditionProgressChanged.Count > 0)
			{
				BattleResultContinueWasClicked = true;
				Globals.MainMenu.StartShowAfterFightScreens();
			}
			else
			{
				ShowLevelUpScreenOrContinue();
			}
			break;
		case "_got_level_continue_1":
			Globals.MainMenu.OnMsgGuiExitAchievmentOrExtraChapter();
			break;
		case "_got_level_continue_2":
			Globals.MainMenu.OnMsgGuiExitAchievmentOrExtraChapter();
			break;
		case "button_defeat_continue":
			Globals.MainMenu.GoToMainMap();
			break;
		case "button_defeat_restart":
			Globals.Battle.RestartBattleWithSameEnemy(defeated: true);
			break;
		}
	}

	internal void ShowLevelUpScreenOrContinue()
	{
		BattleResultContinueWasClicked = false;
		Messenger.Invoke(Globals.MsgFightResult_Continue);
	}

	private void ShowTurnTimeRemains()
	{
		Battle battle = Globals.Battle;
		if (!(battle == null) && battle != null && (battle.State == Battle.StateE.PlayerTime || battle.State == Battle.StateE.FatalityMode || battle.State == Battle.StateE.WaitEnemyMiniGameEnd))
		{
			float arg = 0f;
			if (battle._lastSettedTimeRemainsTillTheEndOfState > 0f)
			{
				arg = battle.TimeRemainsTillTheEndOfState / battle._lastSettedTimeRemainsTillTheEndOfState;
			}
			Messenger.Invoke(Globals.MsgGuiBattle_Timer, arg);
		}
	}
}
