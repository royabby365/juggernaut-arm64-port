using System;
using System.Collections.Generic;
using UnityEngine;
using Yarx;

internal class Tutorials : IDisposable
{
	private TutorialCastMagicCursor _castMagicCursor;

	private GameObject _fatalityCursor;

	private TutorialSliceAttackCursor _sliceAttackCursor;

	private string _state = string.Empty;

	private List<string> _progress = new List<string>();

	private static readonly string _tut_attack = "done=1";

	private static readonly string _tut_attack_1 = "done=1_1";

	private static readonly string _tut_attack_2 = "done=1_2";

	private static readonly string _tut_attack_3 = "done=1_3";

	private static readonly string _tut_slice_attack1 = "done=slice_attack1";

	private static readonly string _tut_slice_attack2 = "done=slice_attack2";

	private static readonly string _tut_slice_attack3 = "done=slice_attack3";

	private static readonly string _tut_slice_attack4 = "done=slice_attack4";

	private static readonly string _tut_slice_attack5 = "done=slice_attack5";

	private static readonly string _tut_combo = "done=2";

	private static readonly string _tut_combo_1 = "done=2_1";

	private static readonly string _tut_combo_2 = "done=2_2";

	private static readonly string _tut_mana = "done=4";

	private static readonly string _tut_mana_1 = "done=4_1";

	private static readonly string _tut_mana_2 = "done=4_2";

	private static readonly string _tut_mana_3 = "done=4_3";

	private static readonly string _tut_mana_4 = "done=4_4";

	private static readonly string _tut_mana_5 = "done=4_5";

	private static readonly string _tut_mana_6 = "done=4_6";

	private static readonly string _tut_rage = "done=6";

	private static readonly string _tut_rage_1 = "done=6_1";

	private static readonly string _tut_rage_2 = "done=6_2";

	private static readonly string _tut_strong_magic = "done=sm";

	private static readonly string _tut_weak_magic = "done=wm";

	private static readonly string _tut_fatality = "done=fatality";

	private static readonly string _tut_ressurection = "done=res";

	private static readonly string _tut_eye = "done=eye";

	private static readonly string _tut_eye_2 = "done=eye_2";

	private static readonly string _tut_eye_3 = "done=eye_3";

	private static readonly string _tut_immunity_z = "done=imm_z";

	private static readonly string _tut_immunity_in_fight = "done=imm_if";

	private static readonly string _tut_all_chests_unlocked = "done=all_ch_un";

	private static readonly string _tut_first_money_pile = "done=first_money";

	private static readonly string _tut_first_mob_attack = "done=_tut_first_mob_attack";

	private static readonly string _tut_shop = "done=shop";

	private static readonly string _tut_shop_filter2 = "done=shop_filter2";

	private static readonly string _tut_shop_filter3 = "done=shop_filter3";

	private static readonly string _tut_shop_filter4 = "done=shop_filter4";

	private static readonly string _tut_shop_filter5 = "done=shop_filter5";

	private static readonly string _tut_got_level1 = "done=got_level1";

	private static readonly string _tut_got_level2 = "done=got_level2";

	private static readonly string _tut_defeat = "done=defeat";

	private static readonly string _tut_location_scarab = "done=location_scarab";

	private static readonly string _tut_bag_rage = "done=bar_rage";

	private static readonly string _tut_bag_mana = "done=bar_mana";

	private static readonly string _tut_bag_exp = "done=bar_exp";

	private static readonly string _tut_bag_money = "done=bar_money";

	private static readonly string _tut_bag_magic = "done=bar_magic";

	private static readonly string _tut_magic_book = "done=_tut_magic_book";

	private static readonly string _tut_magic_book_button1 = "done=_tut_button1_magic_book";

	private static readonly string _tut_magic_book_button2 = "done=_tut_button2_magic_book";

	private static readonly string _tut_achievment1 = "done=_tut_achievment1";

	private static readonly string _tut_achievment2 = "done=_tut_achievment2";

	private static readonly string _tut_epic_item_puton1 = "done=_tut_epic_item_puton1";

	private static readonly string _tut_epic_item_puton2 = "done=_tut_epic_item_puton2";

	private static readonly string _tut_falling_star1 = "done=_tut_falling_star1";

	private static readonly string _tut_falling_star2 = "done=_tut_falling_star2";

	private static readonly string _tut_match3 = "done=_tut_match3";

	private static float _showDelay = 0.5f;

	private bool _enabled;

	private CompositeDisposable _listeners;

	private TutorialArrow _arrow;

	private int _arrow_Z = -1000;

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			Utils.LogForce("***************TUTORIALS", value);
			if (_enabled != value)
			{
				Utils.LogForce("***  TUTORIALS", value);
				_enabled = value;
			}
			Utils.Dispose(ref _listeners);
			if (_enabled)
			{
				InitListeners();
			}
			else
			{
				OnMsgFightRestarted();
			}
		}
	}

	internal string InnerState
	{
		get
		{
			return _state;
		}
		set
		{
			Utils.Log("TUTORIALS INNER STATE", value);
			_state = ((value != null) ? value : string.Empty);
			_progress.Clear();
		}
	}

	internal Tutorials()
	{
	}

	private bool CheckProgress(string name)
	{
		return _progress.Contains(name);
	}

	private bool ChangeProgress(string old, string @new)
	{
		if (old != null)
		{
			int num = _progress.IndexOf(old);
			if (num < 0)
			{
				return false;
			}
			_progress.RemoveAt(num);
		}
		if (@new != null)
		{
			_progress.Add(@new);
		}
		Utils.Log("****TUTORIAL PROGRESS", old, @new);
		return true;
	}

	private void InitListeners()
	{
		_listeners = new CompositeDisposable();
		_listeners.Add(Messenger.AddListener(Globals.MsgResurrectionSpawned, OnMsgResurrectionSpawned));
		_listeners.Add(Messenger.AddListener(Globals.MsgFightEyeDie, OnMsgFightEyeHited));
		_listeners.Add(Messenger.AddListener(Globals.MsgFightEyeShown, OnMsgFightEyeShown));
		_listeners.Add(Messenger.AddListener(Globals.MsgFatalityExecuted, OnMsgFatalityExecuted));
		_listeners.Add(Messenger.AddListener(Globals.MsgFatalityStarted, OnMsgFatalityStarted));
		_listeners.Add(Messenger.AddListener(Globals.MsgTutorialFullScreenInfoHided, OnMsgTutorialFullScreenInfoHided));
		_listeners.Add(Messenger.AddListener(Globals.MsgFightStarted, OnMsgFightStarted));
		_listeners.Add(Messenger.AddListener(Globals.MsgFightBreak, OnMsgFightRestarted));
		_listeners.Add(Messenger.AddListener(Globals.MsgGestureStarted, OnMsgGestureStarted));
		_listeners.Add(Messenger.AddListener(Globals.MsgEnemyWeakMagic, OnMsgEnemyWeakMagic));
		_listeners.Add(Messenger.AddListener(Globals.MsgEnemyStrongMagic, OnMsgEnemyStrongMagic));
		_listeners.Add(Messenger.AddListener(Globals.MsgPlayerAttackSpawnOneMagicBall, OnMsgPlayerAttackSpawnOneMagicBall));
		_listeners.Add(Messenger<int>.AddListener(Globals.MsgFightViewSectorClicked, OnMsgFightViewSectorClicked));
		_listeners.Add(Messenger<string>.AddListener(Globals.MsgGuiButtonPressed, OnMsgGuiButtonPressed));
		_listeners.Add(Messenger<Battle.StateE>.AddListener(Globals.MsgBattleStateChanged, OnMsgBattleStateChanged));
		_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnMsgGuiSwitchToPre));
		_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPost, OnMsgGuiSwitchToPost));
		_listeners.Add(Messenger<Player>.AddListener(Globals.MsgPlayerAttackFinished, OnMsgPlayerAttackFinished));
		_listeners.Add(Messenger<Player>.AddListener(Globals.MsgPersonManaChanged, OnMsgPersonManaChanged));
		_listeners.Add(Messenger<string, bool>.AddListener(Globals.Msg_MagicGame_Finished, OnMsg_MagicGame_Finished));
		_listeners.Add(Messenger<int>.AddListener(Globals.MsgRageSpheresCountChanged, OnMsgRageSpheresCountChanged));
		_listeners.Add(Messenger<Person>.AddListener(Globals.MsgPersonDie, OnMsgPersonDie));
		_listeners.Add(Messenger.AddListener(Globals.MsgEnemyImmunityShown, OnMsgEnemyImmunityShown));
		_listeners.Add(Messenger.AddListener(Globals.MsgAllChestsUnlocked, OnMsgAllChestsUnlocked));
		_listeners.Add(Messenger<MagicTypeE>.AddListener(Globals.MsgPlayerCastSpell, OnMsgPlayerCastSpell));
		_listeners.Add(Messenger.AddListener(Globals.MsgLocationMoneyPileClicked, OnMsgLocationMoneyPilesClicked));
		_listeners.Add(Messenger.AddListener(Globals.MsgLocationMobAttack, OnMsgLocationMobAttack));
		_listeners.Add(Messenger.AddListener(Globals.MsgPlayerDefeated, OnMsgPlayerDefeated));
		_listeners.Add(Messenger<ServerData.Item>.AddListener(Globals.MsgPlayerCompareItem, OnMsgPlayerCompareItem));
		_listeners.Add(Messenger<string>.AddListener(Globals.MsgShopFilterChanged, OnMsgShopFilterChanged));
		_listeners.Add(Messenger<int, int, string>.AddListener(Globals.MsgPlayerLevelChanged, OnMsgPlayerLevelChanged));
		_listeners.Add(Messenger<ServerData.Item>.AddListener(Globals.MsgItemPuton, OnMsgItemPuton));
		_listeners.Add(Messenger.AddListener(Globals.MsgFallingStarClicked, OnMsgFallingStarClicked));
	}

	private void AddDone(string name)
	{
		_state = _state + " " + name;
	}

	private bool Check(string name, ActionD action)
	{
		bool flag = _state.LastIndexOf(name) == _state.Length - name.Length;
		if (!flag)
		{
			flag = _state.Contains(name + " ");
		}
		if (!flag)
		{
			ChangeProgress(null, name);
			AddDone(name);
			Utils.Log("TUTORIAL", name, _state, Utils.ParamsToString(_progress.ToArray()));
			action();
			return true;
		}
		return false;
	}

	internal void Update()
	{
		if (_castMagicCursor != null && !_progress.Contains(_tut_mana_6))
		{
			UnityEngine.Object.Destroy(_castMagicCursor.gameObject);
		}
	}

	private void OnMsgItemPuton(ServerData.Item item)
	{
		if (item.MaxStars > 0)
		{
			Check(_tut_epic_item_puton1, delegate
			{
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutEpicItemPuton);
			});
		}
	}

	private void OnMsgFallingStarClicked()
	{
		Check(_tut_falling_star1, delegate
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: true);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutFallingStarPhrase1);
		});
	}

	private void OnMsgPlayerLevelChanged(int old, int @new, string reason)
	{
		if (!(reason != "AddPlayerExperience") && @new >= 2 && !_state.Contains(_tut_got_level2) && !CheckProgress(_tut_got_level1))
		{
			ChangeProgress(null, _tut_got_level1);
		}
	}

	private void OnMsgShopFilterChanged(string buttonName)
	{
		if (buttonName.Contains("_shop_division_"))
		{
			if (buttonName.Contains("2") && !_state.Contains(_tut_shop_filter2))
			{
				AddDone(_tut_shop_filter2);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutShopFilter2);
			}
			else if (buttonName.Contains("3") && !_state.Contains(_tut_shop_filter3))
			{
				AddDone(_tut_shop_filter3);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutShopFilter3);
			}
			else if (buttonName.Contains("4") && !_state.Contains(_tut_shop_filter4))
			{
				AddDone(_tut_shop_filter4);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutShopFilter4);
			}
			else if (buttonName.Contains("5") && !_state.Contains(_tut_shop_filter5))
			{
				AddDone(_tut_shop_filter5);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutShopFilter5);
			}
		}
	}

	private void OnMsgPlayerCompareItem(ServerData.Item item)
	{
		ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
		if (itemSkillInfo == null)
		{
			return;
		}
		switch (itemSkillInfo.Skill.Type)
		{
		case ServerData.Skill.TypeE.MagicIce:
		case ServerData.Skill.TypeE.MagicFire:
		case ServerData.Skill.TypeE.MagicDark:
		case ServerData.Skill.TypeE.MagicElectro:
			if (!_state.Contains(_tut_bag_magic))
			{
				AddDone(_tut_bag_magic);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutBagMagic);
			}
			break;
		case ServerData.Skill.TypeE.BonusRage:
			if (!_state.Contains(_tut_bag_rage))
			{
				AddDone(_tut_bag_rage);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutBagRage);
			}
			break;
		case ServerData.Skill.TypeE.BonusMana:
			if (!_state.Contains(_tut_bag_mana))
			{
				AddDone(_tut_bag_mana);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutBagMana);
			}
			break;
		case ServerData.Skill.TypeE.BonusExp:
			if (!_state.Contains(_tut_bag_exp))
			{
				AddDone(_tut_bag_exp);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutBagExp);
			}
			break;
		case ServerData.Skill.TypeE.BonusMoney:
			if (!_state.Contains(_tut_bag_money))
			{
				AddDone(_tut_bag_money);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutBagMoney);
			}
			break;
		}
	}

	private void OnMsgPlayerDefeated()
	{
		if (!_state.Contains(_tut_defeat))
		{
			AddDone(_tut_defeat);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutPlayerDefeat);
		}
	}

	private void OnMsgLocationMoneyPilesClicked()
	{
		if (!_state.Contains(_tut_first_money_pile) && !CheckProgress(_tut_first_money_pile))
		{
			AddDone(_tut_first_money_pile);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutFirstMoneyPile);
		}
	}

	private void OnMsgLocationMobAttack()
	{
		if (!_state.Contains(_tut_first_mob_attack) && !CheckProgress(_tut_first_mob_attack))
		{
			ChangeProgress(null, _tut_first_mob_attack);
		}
	}

	private void OnMsgPlayerCastSpell(MagicTypeE magicType)
	{
		if (_castMagicCursor != null)
		{
			UnityEngine.Object.Destroy(_castMagicCursor.gameObject);
		}
		if (_progress.Contains(_tut_mana_6))
		{
			ChangeProgress(_tut_mana_6, null);
		}
	}

	private void OnMsgEnemyImmunityShown()
	{
		if (_enabled)
		{
			Check(_tut_immunity_in_fight, delegate
			{
				Messenger.Invoke(Globals.MsgFightPause, arg1: true);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutImmBattle);
			});
		}
	}

	private void OnMsgAllChestsUnlocked()
	{
		if (_enabled && !_state.Contains(_tut_all_chests_unlocked) && !_progress.Contains(_tut_all_chests_unlocked))
		{
			ChangeProgress(null, _tut_all_chests_unlocked);
		}
	}

	private void OnMsgPersonDie(Person person)
	{
	}

	private void OnMsgResurrectionSpawned()
	{
		if (_enabled)
		{
			Check(_tut_ressurection, delegate
			{
				Globals.ForceDontClickRessurectionBubbles = true;
				Globals.ForceDontClickManaBubbles = true;
				Messenger.Invoke(Globals.MsgFightPause, arg1: true);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutResurrection);
			});
		}
	}

	private void OnMsgRageSpheresCountChanged(int count)
	{
		if (ChangeProgress(_tut_rage_2, null))
		{
			Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Globals.ForceDontProcessSliceAttack = false;
			Globals.ForceDontClickViewSector = false;
			Globals.ForceEnemyDontCastMagic = false;
		}
	}

	private void OnMsgFightEyeHited()
	{
		if (ChangeProgress(_tut_eye_3, null))
		{
			SpriteGui.DontReleaseButtons = false;
			Globals.ForceDontProcessSliceAttack = false;
			Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
		}
	}

	private void OnMsgFightEyeShown()
	{
		if (_enabled)
		{
			Check(_tut_eye, delegate
			{
				ChangeProgress(_tut_eye, _tut_eye_2);
				Globals.ForceDontHitEye = true;
				Globals.ForceDontProcessSliceAttack = true;
				Messenger.Invoke(Globals.MsgFightPause, arg1: true);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutEye);
			});
		}
	}

	private void OnMsgFatalityExecuted()
	{
		if (ChangeProgress(_tut_fatality, null))
		{
			Globals.ForceFatalityNoTimeLimit = false;
			if (_fatalityCursor != null)
			{
				UnityEngine.Object.Destroy(_fatalityCursor);
			}
		}
	}

	private void OnMsgFatalityStarted()
	{
		Utils.Log("OnMsgFatalityStarted 1");
		if (!_enabled)
		{
			return;
		}
		Utils.Log("OnMsgFatalityStarted 2", _state);
		Check(_tut_fatality, delegate
		{
			Utils.Log("OnMsgFatalityStarted 3");
			Globals.ForceFatalityNoTimeLimit = true;
			if (Globals.MainMenu.TutorialFatalityPrefab != null)
			{
				_fatalityCursor = (GameObject)UnityEngine.Object.Instantiate(Globals.MainMenu.TutorialFatalityPrefab);
				_fatalityCursor.transform.position = new Vector3(0f, 0f, 250f);
			}
		});
	}

	private void OnMsgTutorialFullScreenInfoHided()
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (ChangeProgress(_tut_strong_magic, null))
		{
			Globals.ForcePauseStrongMagic = false;
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Globals.Enemy.ContinueMagicGameExecution();
		}
		if (ChangeProgress(_tut_eye_2, _tut_eye_3))
		{
			SpriteGui.DontReleaseButtons = true;
			Globals.ForceDontHitEye = false;
			Messenger.Invoke(Globals.MsgGuiBattle_ShowPhrase, ServerData.PhrasesE.TutEye_2);
		}
		if (ChangeProgress(_tut_attack_1, _tut_attack_2))
		{
			Messenger.Invoke(Globals.MsgGuiBattle_ShowPhrase, ServerData.PhrasesE.Tut1_2);
		}
		if (ChangeProgress(_tut_slice_attack2, _tut_slice_attack3))
		{
			if (Globals.MainMenu.TutorialSliceAttackPrefab != null)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Globals.MainMenu.TutorialSliceAttackPrefab);
				_sliceAttackCursor = gameObject.GetComponent<TutorialSliceAttackCursor>();
				_sliceAttackCursor.transform.position = new Vector3(0f, 0f, 250f);
				_sliceAttackCursor.IsLeft = true;
			}
			Globals.Battle.ViewSector._angle = 0f;
			Globals.ForceDontClickViewSector = true;
			Globals.ForceDontProcessSliceAttack = false;
			Globals.ForceWantedAttack = AttackE.Right;
			Messenger.Invoke(Globals.MsgGuiBattle_ShowPhrase, ServerData.PhrasesE.TutSliceAttack2);
		}
		if (ChangeProgress(_tut_attack_3, null))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Globals.ForceDontProcessSliceAttack = false;
			HideArrow();
		}
		if (ChangeProgress(_tut_combo_1, _tut_combo_2))
		{
			ShowArrow("next_attack_button", new Vector3(-120f, 0f, _arrow_Z), TutorialArrow.Direction.E);
			Messenger.Invoke(Globals.MsgGuiBattle_ShowPhrase, ServerData.PhrasesE.Tut2_2);
		}
		if (ChangeProgress(_tut_mana_2, _tut_mana_3))
		{
			Messenger.Invoke(Globals.MsgGuiBattle_ShowPhrase, ServerData.PhrasesE.Tut4_1_1);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.5f, delegate
			{
				Globals.ForceDontClickManaBubbles = false;
			});
		}
		if (CheckProgress(_tut_mana_5))
		{
			Messenger.Invoke(Globals.MsgGuiBattle_ShowPhrase, ServerData.PhrasesE.Tut4_2_2);
			ShowArrow("use_magic_button", new Vector3(120f, 0f, _arrow_Z), TutorialArrow.Direction.W);
		}
		if (ChangeProgress(_tut_rage_1, _tut_rage_2))
		{
			Messenger.Invoke(Globals.MsgGuiBattle_ShowPhrase, ServerData.PhrasesE.Tut6_1_2);
			Globals.ForceDontClickRageBubbles = false;
		}
		if (ChangeProgress(_tut_ressurection, null))
		{
			Globals.ForceDontClickRessurectionBubbles = false;
			Globals.ForceDontClickManaBubbles = false;
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			if (!Globals.Enemy.IsDead)
			{
				Globals.Battle.State = Battle.StateE.EnemyTime;
			}
		}
		if (ChangeProgress(_tut_immunity_in_fight, null))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Globals.Battle.State = Battle.StateE.EnemyTime;
		}
		if (CheckProgress(_tut_got_level2))
		{
			ShowArrow("global_nav_2", new Vector3(-140f, -45f, _arrow_Z), TutorialArrow.Direction.E);
		}
		if (CheckProgress(_tut_magic_book_button2))
		{
			ShowArrow("global_nav_3", new Vector3(-140f, -45f, _arrow_Z), TutorialArrow.Direction.E);
		}
		if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Location && ChangeProgress(_tut_first_mob_attack, null))
		{
			AddDone(_tut_first_mob_attack);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutFirstMobAttack);
			return;
		}
		if (ChangeProgress(_tut_achievment1, _tut_achievment2))
		{
			ShowArrow("_achievement_button", new Vector3(120f, 0f, 0f), TutorialArrow.Direction.W);
		}
		if (ChangeProgress(_tut_epic_item_puton1, _tut_epic_item_puton2))
		{
			ShowArrow("star_count", new Vector3(55f, 45f, 0f), TutorialArrow.Direction.S);
		}
		if (ChangeProgress(_tut_falling_star1, null))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
		}
	}

	private void OnMsg_MagicGame_Finished(string type, bool _)
	{
		if (ChangeProgress(_tut_weak_magic, null))
		{
			Globals.ForceWeakMagicNoTimeLimit = false;
		}
	}

	private void OnMsgEnemyWeakMagic()
	{
		if (_enabled)
		{
			Check(_tut_weak_magic, delegate
			{
				Globals.ForceWeakMagicNoTimeLimit = true;
			});
		}
	}

	private void OnMsgEnemyStrongMagic()
	{
		if (_enabled)
		{
			Check(_tut_strong_magic, delegate
			{
				Globals.ForcePauseStrongMagic = true;
				Messenger.Invoke(Globals.MsgFightPause, arg1: true);
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutStrongMagic);
			});
		}
	}

	private void OnMsgBattleStateChanged(Battle.StateE state)
	{
		if (state == Battle.StateE.PlayerTime && ChangeProgress(_tut_mana_4, _tut_mana_5))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: true);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.1f, delegate
			{
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.Tut4_2);
			});
		}
		if (state == Battle.StateE.WaitPlayerAttackEnd && ChangeProgress(_tut_slice_attack3, _tut_slice_attack4))
		{
			Globals.ForceDontClickViewSector = false;
			Globals.ForceWantedAttack = AttackE.None;
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
			if (_sliceAttackCursor != null)
			{
				UnityEngine.Object.Destroy(_sliceAttackCursor.gameObject);
			}
		}
		if (state == Battle.StateE.WaitPlayerAttackEnd && ChangeProgress(_tut_slice_attack5, null))
		{
			Globals.ForceDontClickViewSector = false;
			Globals.ForceWantedAttack = AttackE.None;
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
			if (_sliceAttackCursor != null)
			{
				UnityEngine.Object.Destroy(_sliceAttackCursor.gameObject);
			}
		}
		if (state == Battle.StateE.PlayerTime && ChangeProgress(_tut_rage, _tut_rage_1))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: true);
			Globals.ForceSpawnRageBalls = false;
			Globals.ForceDontProcessSliceAttack = true;
			Globals.ForceDontClickViewSector = true;
			Globals.ForceEnemyDontCastMagic = true;
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.1f, delegate
			{
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.Tut6_1);
			});
		}
		if (state == Battle.StateE.PlayerTime && _progress.Contains(_tut_attack_3))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: true);
			Globals.ForceDontProcessSliceAttack = true;
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(SingletonT<ServerData>.I.GameSettings.TurnTime * 0.2f, delegate
			{
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutBattleTurn);
				ShowArrow("battle_timer_center_top", new Vector3(0f, -60f, _arrow_Z), TutorialArrow.Direction.N);
			});
		}
		if (state == Battle.StateE.PlayerTime && ChangeProgress(_tut_slice_attack4, _tut_slice_attack5))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: true);
			if (Globals.MainMenu.TutorialSliceAttackPrefab != null)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Globals.MainMenu.TutorialSliceAttackPrefab);
				_sliceAttackCursor = gameObject.GetComponent<TutorialSliceAttackCursor>();
				_sliceAttackCursor.transform.position = new Vector3(0f, 0f, 250f);
				_sliceAttackCursor.IsLeft = false;
			}
			Globals.Battle.ViewSector._angle = 0f;
			Globals.ForceDontClickViewSector = true;
			Globals.ForceWantedAttack = AttackE.Left;
			Messenger.Invoke(Globals.MsgGuiBattle_ShowPhrase, ServerData.PhrasesE.TutSliceAttack3);
		}
	}

	private void OnMsgPersonManaChanged(Player player)
	{
		if (ChangeProgress(_tut_mana_3, _tut_mana_4))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
			Globals.Battle.State = Battle.StateE.EnemyTime;
		}
	}

	private void OnMsgPlayerAttackFinished(Player player)
	{
		if (ChangeProgress(_tut_mana_1, _tut_mana_2))
		{
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.Tut4_1);
		}
	}

	private void OnMsgPlayerAttackSpawnOneMagicBall()
	{
		if (ChangeProgress(_tut_mana, _tut_mana_1))
		{
			Messenger.Invoke(Globals.MsgFightPause, arg1: true);
			Globals.ForceDontProcessSliceAttack = true;
			Globals.ForceDontClickViewSector = true;
		}
	}

	private void OnMsgFightViewSectorClicked(int index)
	{
		if (index != 2 && ChangeProgress(_tut_attack_2, _tut_attack_3))
		{
			Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Messenger.Invoke(Globals.MsgFightViewSectorClicked, index);
			Globals.ForceDontProcessSliceAttack = false;
		}
	}

	private void OnMsgGestureStarted()
	{
	}

	private void OnMsgGuiButtonPressed(string name)
	{
		if (name == "next_attack_button" && ChangeProgress(_tut_combo_2, null))
		{
			HideArrow();
			Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Globals.ViewSectorMoveForce = false;
			Globals.ForceDontClickViewSector = false;
			Globals.ForceDontProcessSliceAttack = false;
		}
		if (ChangeProgress(_tut_epic_item_puton2, null))
		{
			AddDone(_tut_epic_item_puton2);
			HideArrow();
		}
	}

	private void ShowStartTutorial(ServerData.PhrasesE phrase)
	{
		SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(_showDelay, delegate
		{
			Messenger.Invoke(Globals.MsgTutorialInfo, phrase);
		});
	}

	private void OnMsgGuiSwitchToPost(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (!_enabled)
		{
			return;
		}
		if (@new == GuiRoot.GuiType.Fight && !_state.Contains(_tut_immunity_z))
		{
			AreaData current = AreaData.Current;
			if (current != null && current.Mobs != null)
			{
				int locationProgress = SingletonT<ServerData>.I.GetLocationProgress(current.Location);
				if (locationProgress >= 0 && locationProgress < current.Mobs.Length)
				{
					AreaData.MobData mobData = current.Mobs[locationProgress];
					if (mobData != null && (mobData.Darkness || mobData.Ice || mobData.Fire || mobData.Lighting))
					{
						AddDone(_tut_immunity_z);
						Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutImmZ);
					}
				}
			}
		}
		if (@new == GuiRoot.GuiType.Fight && !_state.Contains(_tut_magic_book_button1) && !CheckProgress(_tut_magic_book_button1) && _state.Contains(_tut_mana) && SingletonT<ServerData>.I.IsMagicOpened)
		{
			ChangeProgress(null, _tut_magic_book_button1);
		}
		if (@new == GuiRoot.GuiType.Shop && !_state.Contains(_tut_shop))
		{
			AddDone(_tut_shop);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutShop);
		}
		if (@new == GuiRoot.GuiType.Location && !_state.Contains(_tut_location_scarab))
		{
			ChangeProgress(null, _tut_location_scarab);
			ShowArrow("find_chest", new Vector3(0f, 120f, _arrow_Z), TutorialArrow.Direction.S);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutLocationScarab);
			return;
		}
		if (!_state.Contains(_tut_got_level2) && CheckProgress(_tut_got_level1) && (@new == GuiRoot.GuiType.Fight || @new == GuiRoot.GuiType.Location))
		{
			ChangeProgress(_tut_got_level1, _tut_got_level2);
			AddDone(_tut_got_level2);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.1f, delegate
			{
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutGotLevel);
			});
		}
		if (@new == GuiRoot.GuiType.Location && CheckProgress(_tut_all_chests_unlocked))
		{
			ChangeProgress(_tut_all_chests_unlocked, null);
			AddDone(_tut_all_chests_unlocked);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutEndChests);
			return;
		}
		if (@new == GuiRoot.GuiType.Location && CheckProgress(_tut_first_mob_attack))
		{
			ChangeProgress(_tut_first_mob_attack, null);
			AddDone(_tut_first_mob_attack);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutFirstMobAttack);
			return;
		}
		if (@new == GuiRoot.GuiType.MagicBook && !_state.Contains(_tut_magic_book))
		{
			AddDone(_tut_magic_book);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutFirstMagicBook);
			return;
		}
		if (@new == GuiRoot.GuiType.Fight && CheckProgress(_tut_magic_book_button1))
		{
			ChangeProgress(_tut_magic_book_button1, _tut_magic_book_button2);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutMagicBookButton);
			return;
		}
		if (old == GuiRoot.GuiType.Achievments && @new == GuiRoot.GuiType.MainMap && !_state.Contains(_tut_achievment1))
		{
			ChangeProgress(null, _tut_achievment1);
			AddDone(_tut_achievment1);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutAchievmentButton);
			return;
		}
		if (@new == GuiRoot.GuiType.Match3 && !_state.Contains(_tut_match3))
		{
			AddDone(_tut_match3);
			Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.Match3Tutorial);
		}
		if (old == GuiRoot.GuiType.MainMap && ChangeProgress(_tut_achievment2, null))
		{
			HideArrow();
		}
	}

	private void OnMsgGuiSwitchToPre(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.BattleHud && ChangeProgress(_tut_attack, _tut_attack_1))
		{
			ShowStartTutorial(ServerData.PhrasesE.Tut1_1);
		}
		if (@new == GuiRoot.GuiType.BattleHud && ChangeProgress(_tut_slice_attack1, _tut_slice_attack2))
		{
			ShowStartTutorial(ServerData.PhrasesE.TutSliceAttack1);
		}
		if (@new == GuiRoot.GuiType.BattleHud && ChangeProgress(_tut_combo, _tut_combo_1))
		{
			Globals.Battle.ComboAttackDoneIndex = Globals.ComboSize - 2;
			Globals.Battle.UpdateComboView();
			ShowStartTutorial(ServerData.PhrasesE.Tut2_1);
		}
		if (@new == GuiRoot.GuiType.BattleHud && CheckProgress(_tut_mana))
		{
			Globals.Player.Mana = 9;
		}
		if (@new == GuiRoot.GuiType.CastMagic && ChangeProgress(_tut_mana_5, _tut_mana_6))
		{
			HideArrow();
			Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
			Messenger.Invoke(Globals.MsgFightPause, arg1: false);
			Globals.ForceDontProcessSliceAttack = false;
			Globals.ForceDontClickViewSector = false;
			Globals.Battle.StopTurnTime();
			if (Globals.MainMenu.TutorialCastMagicPrefab != null)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Globals.MainMenu.TutorialCastMagicPrefab);
				gameObject.transform.position = new Vector3(0f, 0f, 250f);
				_castMagicCursor = gameObject.GetComponent<TutorialCastMagicCursor>();
				ServerData.Spell spell = SingletonT<ServerData>.I._spells[SingletonT<ServerData>.I.MySpells[0]];
				_castMagicCursor.CurrentGesture = spell.SkillType;
			}
		}
		if (CheckProgress(_tut_got_level2))
		{
			ChangeProgress(_tut_got_level2, null);
			HideArrow();
		}
		if (old == GuiRoot.GuiType.Location && ChangeProgress(_tut_location_scarab, null))
		{
			AddDone(_tut_location_scarab);
			HideArrow();
		}
		if (old == GuiRoot.GuiType.Fight && ChangeProgress(_tut_magic_book_button2, null))
		{
			AddDone(_tut_magic_book_button1);
			AddDone(_tut_magic_book_button2);
			HideArrow();
		}
	}

	private void OnMsgFightRestarted()
	{
		Utils.Log("***OnMsgFightRestarted", _state);
		if (_arrow != null)
		{
			_arrow.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		_progress.Clear();
		Globals.ResetTutorialsFlags();
		Messenger.Invoke(Globals.MsgFightPause, arg1: false);
		Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
		if (_castMagicCursor != null)
		{
			UnityEngine.Object.Destroy(_castMagicCursor.gameObject);
		}
		if (_fatalityCursor != null)
		{
			UnityEngine.Object.Destroy(_fatalityCursor.gameObject);
		}
		if (_sliceAttackCursor != null)
		{
			UnityEngine.Object.Destroy(_sliceAttackCursor.gameObject);
		}
	}

	private void OnMsgFightStarted()
	{
		if (_enabled)
		{
			Utils.Log("***OnMsgFightStarted", _state);
			Globals.ResetTutorialsFlags();
			if (!Check(_tut_attack, delegate
			{
				Messenger.Invoke(Globals.MsgFightPause, arg1: true);
				Globals.ForceDontProcessSliceAttack = true;
			}) && (!_state.Contains(_tut_attack) || !Check(_tut_slice_attack1, delegate
			{
				Messenger.Invoke(Globals.MsgFightPause, arg1: true);
				Globals.ForceDontProcessSliceAttack = true;
				Globals.Battle.ViewSector._angle = 0f;
			})) && (!_state.Contains(_tut_slice_attack1) || !SingletonT<ServerData>.I.IsComboOpened || !Check(_tut_combo, delegate
			{
				Globals.ForceDontSpawnResurrection = true;
				Globals.ViewSectorMoveForce = true;
				Globals.ForceDontClickViewSector = true;
				Globals.ForceDontProcessSliceAttack = true;
				Messenger.Invoke(Globals.MsgFightPause, arg1: true);
			})) && (!_state.Contains(_tut_slice_attack1) || !SingletonT<ServerData>.I.IsMagicOpened || !Check(_tut_mana, delegate
			{
				Globals.PlayerAttackSpawnOneMagicBall = true;
				Globals.ForceDontClickManaBubbles = true;
			})) && _state.Contains(_tut_slice_attack1) && SingletonT<ServerData>.I.IsRageOpened && !Check(_tut_rage, delegate
			{
				Globals.ForceSpawnRageBalls = true;
				Globals.ForceDontClickRageBubbles = true;
				Globals.ForceEnemyDontCastMagic = true;
			}))
			{
			}
		}
	}

	private void ShowArrow(string name, Vector3 offset, TutorialArrow.Direction dir)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (_arrow == null)
		{
			if (Globals.MainMenu.TutorialArrowPrefab == null)
			{
				return;
			}
			_arrow = ((GameObject)UnityEngine.Object.Instantiate(Globals.MainMenu.TutorialArrowPrefab)).GetComponent<TutorialArrow>();
		}
		if (_arrow != null)
		{
			Transform transform = HudMk1.Instance.transform.FindChildByName(name, includeInactive: true);
			if (transform != null)
			{
				_arrow.gameObject.transform.position = transform.transform.position + offset;
				_arrow.gameObject.SetActiveRecursivelyMk1(setActive: true);
				_arrow.SetDirection(dir);
			}
		}
	}

	private void HideArrow()
	{
		if (_arrow != null)
		{
			_arrow.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
	}

	public void Dispose()
	{
		Utils.Dispose(ref _listeners);
	}

	internal bool TryStartTutorial(string name)
	{
		if (!_enabled)
		{
			return false;
		}
		if (_state.Contains(" " + name))
		{
			return false;
		}
		if (name == Globals.TutorialFindChest)
		{
			if (ChangeProgress(_tut_location_scarab, null))
			{
				AddDone(_tut_location_scarab);
				HideArrow();
			}
			return Check(name, delegate
			{
				Globals.MainMenu.SaveGame();
				Messenger.Invoke(Globals.MsgTutorialInfo, ServerData.PhrasesE.TutSearchTreasure);
			});
		}
		return false;
	}

	internal void OnNewGame()
	{
		Enabled = true;
		_state = string.Empty;
		_progress.Clear();
		Globals.ResetTutorialsFlags();
	}
}
