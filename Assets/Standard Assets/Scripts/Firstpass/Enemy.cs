using System;
using System.Collections.Generic;
using UnityEngine;

internal class Enemy : Person
{
	internal ViewSector ViewSector;

	private GameObject _shadow;

	private Transform _bones;

	private bool _inAttack;

	internal bool FromLocation;

	private GameObject _lastFx;

	public bool _dontAttack;

	private Renderer[] _cachedRenderes;

	internal Vector3 StartIdlePos;

	internal bool _destroyed;

	private Dictionary<MagicTypeE, FontManager.ColorE> _magicColors = new Dictionary<MagicTypeE, FontManager.ColorE>
	{
		{
			MagicTypeE.Darkness,
			FontManager.ColorE.MagicDark
		},
		{
			MagicTypeE.Fire,
			FontManager.ColorE.MagicFire
		},
		{
			MagicTypeE.Lighting,
			FontManager.ColorE.MagicElectro
		},
		{
			MagicTypeE.Ice,
			FontManager.ColorE.MagicIce
		}
	};

	private IDisposable _OnMsg_MagicGame_Finished;

	private bool _waitStepsEndForEnemyAttack;

	private bool _attackBlockedByPlayer;

	private bool _waitStrongMagic;

	private bool _waitWeakMagic;

	private AttackE[] _enemyAttacks = new AttackE[3]
	{
		AttackE.Left,
		AttackE.Right,
		AttackE.Forward
	};

	internal bool InAttack
	{
		get
		{
			return _inAttack;
		}
		private set
		{
			_inAttack = value;
		}
	}

	private string MagicBafScenarioName => (!(ModelName == "1") && !(ModelName == "2")) ? "magic_baf" : "magic_baf_fire";

	public void CacheFxs()
	{
		if (!Globals.DebugDontLoadPlayer)
		{
			CacheFxs("enemy", Globals.Player.ScenariosEvaluator, "magic_direct", "magic_aoe", "attack", "attack_left", "attack_right", "magic_baf", "idle", "idle2", "idle3", "block", "damage", "death", "dodge", "magic_attack", "attack_uppercot", "damage_force", "damage_uppercot", "step", "death_force", "death_uppercot", "react_death");
		}
	}

	internal override void SetServerData(ServerData.BotInfo botInfo, ServerData.BotLevel levelData)
	{
		base.SetServerData(botInfo, levelData);
		_attackJustExecuted = false;
		if (_serverBotInfo == null || _serverBotInfo.ClosedActions == null)
		{
			return;
		}
		List<AttackE> list = new List<AttackE>(_enemyAttacks);
		string[] closedActions = _serverBotInfo.ClosedActions;
		for (int i = 0; i < closedActions.Length; i++)
		{
			switch (closedActions[i])
			{
			case "attack_left":
				list.Remove(AttackE.Left);
				break;
			case "attack_right":
				list.Remove(AttackE.Right);
				break;
			case "attack_forward":
				list.Remove(AttackE.Forward);
				break;
			}
		}
		if (_enemyAttacks.Length != list.Count)
		{
			_enemyAttacks = list.ToArray();
		}
	}

	public void Reset()
	{
		PlayAnim(GetNextIdleAnim(), inLoop: false);
		InAttack = false;
		_dontAttack = false;
	}

	protected override void StartImpl()
	{
		base.StartImpl();
		if (base.ScenariosEvaluator.HasScenario("onstart"))
		{
			PlayScenario(Globals.Battle, "onstart", AttackE.None, ReactE.None, DamageTypeE.Timeout, 0, delegate
			{
			});
		}
	}

	protected override void Idle()
	{
		if (base.Health <= 0 && Globals.Battle.IsInBattleMode)
		{
			return;
		}
		if (_waitStepsEndForEnemyAttack)
		{
			_waitStepsEndForEnemyAttack = false;
			ExecuteAttack(skipStep: true, _attackBlockedByPlayer);
			return;
		}
		if (!IsIdleAnimationPlaying())
		{
			PlayAnim(GetNextIdleAnim(), inLoop: false);
		}
		if (InAttack)
		{
			InAttack = false;
			Messenger<Person>.Invoke(Globals.MsgPersonAttackFinished, this);
		}
	}

	public void ForceIdleForFatality()
	{
		if (_idleAnims.Count > 0)
		{
			PlayAnim(_idleAnims[0], inLoop: true);
		}
	}

	public void ForceIdle2()
	{
		_attackJustExecuted = true;
	}

	private void Update()
	{
		if (Time.deltaTime <= 0f || Globals.IsPaused)
		{
			return;
		}
		DoUpdate(Time.deltaTime);
		if (Globals.Battle == null)
		{
			Idle();
			return;
		}
		if (Globals.IsShadowEnabled && _shadow != null && _bones != null)
		{
			UpdateShadowBounds();
			Vector3 position = _bones.position;
			position.y = base.transform.position.y;
			_shadow.transform.position = new Vector3(position.x, Globals.Battle.ArenaCenter.transform.position.y, position.z);
		}
		if (Globals.Battle.IsSelectionMode && !_destroyed)
		{
			Idle();
		}
		else if (!Globals.Battle.IsFinished && !base.IsDead)
		{
			if (Globals.Battle.State == Battle.StateE.EnemyTime)
			{
			}
			if (Globals.Battle.State == Battle.StateE.EnemyTime && IsIdleAnimationPlaying() && !base.IsDead && !_dontAttack && !Globals.Battle.Pause)
			{
				Globals.Battle.ExecuteEnemyAttack(skipStep: false);
			}
			else if ((bool)GetComponent<Animation>() && !GetComponent<Animation>().isPlaying)
			{
				Idle();
			}
		}
	}

	protected override void ShowMagicReact(AttackE attack, string reactScenario, DamageTypeE damageType, int hp, string hpText)
	{
		MagicTypeE magicTypeE = damageType.AsMagic();
		bool flag = false;
		if (base.ServerBotInfo != null && base.ServerBotInfo.HasMagicImmunity(magicTypeE))
		{
			ShowFightPhraseOnHpBar(FightPhraseE.Immunity, 1.5f, new Vector3(0f, -50f, -50f));
			flag = true;
			SingletonT<SoundManager>.I.PlayFailSound();
			Messenger.Invoke(Globals.MsgEnemyImmunityShown);
		}
		if (hp != 0)
		{
			switch (damageType)
			{
			case DamageTypeE.DarkMagic:
				hpText += Globals.CharIconDark;
				break;
			case DamageTypeE.LightingMagic:
				hpText += Globals.CharIconElectro;
				break;
			case DamageTypeE.FireMagic:
				hpText += Globals.CharIconFire;
				break;
			case DamageTypeE.IceMagic:
				hpText += Globals.CharIconIce;
				break;
			}
		}
		ShowTextOnHpBar(hpText, Person.TextTimeScreenDigits, new Vector3(0f, -20 - (flag ? 50 : 0), -50f), Person.TextSpeedScreenDigits, _magicColors[magicTypeE]);
		Globals.Battle.BattleCameraController.Shake();
	}

	protected override void ShowNaturalReact(AttackE attack, string reactScenario, DamageTypeE damageType, int hp, string hpText)
	{
		base.ShowNaturalReact(attack, reactScenario, damageType, hp, hpText);
		if (attack == AttackE.Combo)
		{
			Globals.Battle.BattleCameraController.Shake();
		}
	}

	protected override void Enabled()
	{
		base.Enabled();
		_OnMsg_MagicGame_Finished = Messenger<string, bool>.AddListener(Globals.Msg_MagicGame_Finished, OnMsg_MagicGame_Finished);
		if (Globals.IsShadowEnabled)
		{
			if (_shadow != null)
			{
				_shadow.SetActiveRecursivelyMk1(setActive: true);
			}
			if (_shadow == null)
			{
				_shadow = (GameObject)UnityEngine.Object.Instantiate(Globals.Battle.ShadowPrefab);
				UpdateShadowBounds();
			}
			if (_bones == null)
			{
				_bones = base.transform.FindChildByName("bones", includeInactive: true);
			}
		}
	}

	internal override void Die()
	{
		base.Die();
		Utils.Destroy(ref _shadow);
	}

	private void UpdateShadowBounds()
	{
		if (_cachedRenderes == null)
		{
			List<Renderer> list = new List<Renderer>(GetComponentsInChildren<Renderer>());
			if (list.Count == 0)
			{
				return;
			}
			_cachedRenderes = list.ToArray();
		}
		Bounds bounds = default(Bounds);
		Renderer[] cachedRenderes = _cachedRenderes;
		foreach (Renderer renderer in cachedRenderes)
		{
			bounds.Encapsulate(GetComponent<Renderer>().bounds);
		}
		_shadow.transform.localScale = new Vector3(bounds.size.x, bounds.size.z, 1f);
	}

	internal bool HasWeakMagicScenario()
	{
		string text = ((!(ModelName == "1") && !(ModelName == "2")) ? "magic_direct" : "magic_fireball");
		return base.ScenariosEvaluator.HasScenario(text);
	}

	internal bool HasStrongMagicScenario()
	{
		string text = ((!(ModelName == "1") && !(ModelName == "2")) ? "magic_aoe" : "magic_aoe_fire");
		return base.ScenariosEvaluator.HasScenario(text);
	}

	private void OnMsg_MagicGame_Finished(string type, bool successed)
	{
		if (!(HudMk1.Instance == null))
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.EnemyTurn);
			Messenger<bool>.Invoke(Globals.MsgGuiBattle_ShowWeakMagicProtectionBar, arg1: false);
			Globals.Battle.ResumeTurnTime("OnMsg_MagicGame_Finished");
			string scenario = ((!_waitWeakMagic) ? "magic_aoe" : "magic_direct");
			if (ModelName == "1" || ModelName == "2")
			{
				scenario = ((!_waitWeakMagic) ? "magic_aoe_fire" : "magic_fireball");
			}
			int num = ((!successed) ? SingletonT<ServerData>.I.GetEnemyMagicDamage(_waitWeakMagic) : 0);
			_waitWeakMagic = false;
			_waitStrongMagic = false;
			if (!successed && num <= 0)
			{
				num = 1;
			}
			if (Globals.DebugEnemyLargeDamage)
			{
				num = 51;
			}
			if (Globals.DebugEnemyOneHitKill)
			{
				num = 88888;
			}
			if (Globals.DebugNoDamageOnPlayer)
			{
				num = 0;
			}
			ReactE reactE = ((num > 0) ? ReactE.Damage : (successed ? ReactE.Dodge : ReactE.None));
			if (num > 0 && Globals.Player.Health <= num)
			{
				reactE |= ReactE.Death;
				Messenger.Invoke(Globals.MsgPlayerKilledByMagic, type);
			}
			InAttack = true;
			if (Globals.Battle.State == Battle.StateE.WaitEnemyMiniGameEnd)
			{
				Globals.Battle.State = Battle.StateE.WaitEnemyAttackEnd;
			}
			PlayScenario(Globals.Battle, scenario, AttackE.None, reactE, base.ServerBotInfo.MagicDamageType, -num, delegate
			{
				ForceIdle2();
			});
			if (successed)
			{
				Messenger.Invoke(Globals.MsgPlayerSpellReact, type, reactE);
			}
		}
	}

	protected override void Disabled()
	{
		base.Disabled();
		Utils.Dispose(ref _OnMsg_MagicGame_Finished);
		Utils.Destroy(ref _shadow);
	}

	internal void Attack(AttackE attackType, int damage, DamageTypeE damageType, ReactE react)
	{
		InAttack = true;
		string text = Utils.Select(attackType, null, AttackE.Forward, GetForwardAnimationName(), AttackE.Left, "attack_left", AttackE.Right, "attack_right");
		Utils.Log("Attack", base.name, text, attackType, damage, damageType, react);
		Invs.Inv(text != null, "Enemy.Attack failed", base.name, attackType);
		PlayScenario(Globals.Battle, text, attackType, react, damageType, -damage, delegate
		{
			Utils.Log("ForceIdle2", base.name);
			ForceIdle2();
		});
	}

	public void SetupOnScene(Battle battle)
	{
		GameObject arenaCenter = Globals.Battle.ArenaCenter;
		base.transform.position = arenaCenter.transform.position;
		base.transform.position = arenaCenter.transform.position;
		if (Globals.Player != null)
		{
			base.transform.LookAt(Globals.Player.transform);
			base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
		}
		else
		{
			base.transform.rotation = arenaCenter.transform.rotation;
		}
		base.transform.transform.Translate(0f, -0.02f, 0f - Globals.DistanceBetweenPersons);
		Utils.DoWithComponent(this, delegate(PersonData _)
		{
			base.transform.Translate(_.TranslateAtScene);
		});
		StartIdlePos = base.transform.position;
	}

	internal void ExecuteAttack(bool skipStep, bool blockedByPlayer)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		Utils.Log("ExecuteAttack", base.name, skipStep, blockedByPlayer, _waitStepsEndForEnemyAttack);
		HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.EnemyTurn);
		_attackBlockedByPlayer = blockedByPlayer;
		if (!skipStep)
		{
			bool flag = UnityEngine.Random.Range(0, 100) < Globals.EnemyAttackRandomStepMax;
			if (Globals.DebugEnemyDoAlwayStep)
			{
				flag = true;
			}
			if (Globals.BuildType == Globals.BuildTypeE.ShowContent)
			{
				flag = true;
			}
			if (flag && base.ScenariosEvaluator != null && base.ScenariosEvaluator.HasScenario("step_l") && base.ScenariosEvaluator.HasScenario("step_r"))
			{
				_waitStepsEndForEnemyAttack = true;
				PlayScenario(Globals.Battle, (!Utils.RandomBool()) ? "step_r" : "step_l", AttackE.Forward, ReactE.None, DamageTypeE.Natural, 0);
			}
		}
		if (!_waitStepsEndForEnemyAttack)
		{
			if (!ExecuteMagicAttack())
			{
				ExecuteNaturalAttack();
			}
			_attackBlockedByPlayer = false;
		}
	}

	private bool ExecuteMagicAttack()
	{
		if (Globals.ForceEnemyDontCastMagic)
		{
			return false;
		}
		if (!Globals.DebugEnemyNoMagic)
		{
			if (Globals.DebugEnemyAlwaysWeakMagic || (_serverLevelInfo.WeakMagicP > 0 && UnityEngine.Random.Range(0, 100) < _serverLevelInfo.WeakMagicP))
			{
				_waitWeakMagic = true;
			}
			if (Globals.DebugEnemyAlwaysStrongMagic || (!_waitWeakMagic && _serverLevelInfo.StrongMagicP > 0 && UnityEngine.Random.Range(0, 100) < _serverLevelInfo.StrongMagicP))
			{
				_waitStrongMagic = true;
			}
		}
		if (_waitWeakMagic || _waitStrongMagic)
		{
			if (_waitWeakMagic && !HasWeakMagicScenario())
			{
				return false;
			}
			if (_waitStrongMagic && !HasStrongMagicScenario())
			{
				return false;
			}
			if (_waitStrongMagic)
			{
				Messenger.Invoke(Globals.MsgEnemyStrongMagic);
			}
			else if (_waitWeakMagic)
			{
				Messenger.Invoke(Globals.MsgEnemyWeakMagic);
			}
			Globals.Battle.StopTurnTime();
			if (!Globals.ForcePauseStrongMagic)
			{
				PlayScenario(Globals.Battle, MagicBafScenarioName, AttackE.Magic, ReactE.None, DamageTypeE.Timeout, 0, delegate
				{
					if (_waitStrongMagic)
					{
						int num = SingletonT<ServerData>.I.GetMinigameCount(SingletonT<ServerData>.I.EnemyParams.MobData);
						if (num <= 0)
						{
							num = 1;
						}
						Messenger<int>.Invoke(Globals.Msg_MagicGame_Show, num);
					}
					else
					{
						SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(base.name, 0.8f, delegate
						{
							int num2 = SingletonT<ServerData>.I.GetMinigame2Count(SingletonT<ServerData>.I.EnemyParams.MobData);
							if (num2 <= 0)
							{
								num2 = 1;
							}
							Messenger<int>.Invoke(Globals.Msg_MagicGame2_Show, num2);
						});
					}
				});
			}
			else
			{
				Idle();
			}
			return true;
		}
		return false;
	}

	internal void ContinueMagicGameExecution()
	{
		PlayScenario(Globals.Battle, MagicBafScenarioName, AttackE.Magic, ReactE.None, DamageTypeE.Timeout, 0, delegate
		{
			if (_waitStrongMagic)
			{
				int num = SingletonT<ServerData>.I.GetMinigameCount(SingletonT<ServerData>.I.EnemyParams.MobData);
				if (num <= 0)
				{
					num = 1;
				}
				Messenger<int>.Invoke(Globals.Msg_MagicGame_Show, num);
			}
			else
			{
				SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(base.name, 0.8f, delegate
				{
					int num2 = SingletonT<ServerData>.I.GetMinigame2Count(SingletonT<ServerData>.I.EnemyParams.MobData);
					if (num2 <= 0)
					{
						num2 = 1;
					}
					Messenger<int>.Invoke(Globals.Msg_MagicGame2_Show, num2);
				});
			}
		});
	}

	private void ExecuteNaturalAttack()
	{
		AttackE attackType = _enemyAttacks[UnityEngine.Random.Range(0, _enemyAttacks.Length)];
		int damage = 0;
		bool isCrit = false;
		SingletonT<ServerData>.I.GetDamage(SingletonT<ServerData>.I.EnemyParams, SingletonT<ServerData>.I.PlayerInBattleParams, critAllowed: true, forceCrit: false, 0, out isCrit, out damage);
		if (Globals.DebugEnemyAlwaysCrit)
		{
			isCrit = true;
		}
		if (Globals.DebugEnemyLargeDamage)
		{
			damage = 51;
		}
		if (Globals.DebugEnemyOneHitKill)
		{
			damage = 10000;
		}
		if (Globals.DebugNoDamageOnPlayer)
		{
			damage = 0;
		}
		if (_attackBlockedByPlayer)
		{
			damage /= 2;
		}
		int num = Globals.Player.Health - damage;
		ReactE reactE = ((num > 0) ? ReactE.Damage : ReactE.None);
		if (isCrit)
		{
			reactE |= ReactE.Critical;
		}
		if (_attackBlockedByPlayer)
		{
			reactE = ReactE.Block | ReactE.NoBubbles;
		}
		if (Globals.DebugPlayerAlwaysBlock)
		{
			reactE = ReactE.Block | ReactE.NoBubbles;
		}
		if (Globals.DebugPlayerAlwaysDodge)
		{
			reactE = ReactE.Dodge | ReactE.NoBubbles;
		}
		if (num <= 0)
		{
			reactE |= ReactE.Death;
		}
		Globals.Enemy.Attack(attackType, damage, DamageTypeE.Natural, reactE);
	}

	internal void SetupEnemyOnSceneInBattleMode()
	{
		Transform transform = ViewSector.transform.gameObject.transform;
		transform.position = Globals.Enemy.transform.position;
		transform.rotation = Globals.Enemy.transform.rotation;
		ViewSector.Init();
	}

	internal void SetupEnemyOnSceneInSelectionMode()
	{
		Globals.Enemy.PlayAnim(GetNextIdleAnim(), inLoop: true);
	}

	internal void RemoveAllEffects()
	{
		Dictionary<GameObject, bool> dictionary = new Dictionary<GameObject, bool>();
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name.Contains(Person.FxNameTag) && !transform.name.EndsWith(Person.FxNameContext + "onstart"))
			{
				dictionary[transform.gameObject] = true;
			}
		}
		foreach (GameObject key in dictionary.Keys)
		{
			UnityEngine.Object.Destroy(key);
		}
	}

	internal void BreakBattle()
	{
		base.ScenariosEvaluator.StopAll(base.name);
		StopAllCoroutines();
		_dontAttack = false;
		InAttack = false;
		GetComponent<Animation>().Stop();
		_waitStepsEndForEnemyAttack = false;
		_attackBlockedByPlayer = false;
		_waitStrongMagic = false;
		_waitWeakMagic = false;
	}

	protected override GameObject GetDigitsRoot()
	{
		if (HudMk1.Instance == null)
		{
			return null;
		}
		MobHud componentInChildren = HudMk1.Instance.GetComponentInChildren<MobHud>();
		return componentInChildren.HpDigits.gameObject;
	}
}
