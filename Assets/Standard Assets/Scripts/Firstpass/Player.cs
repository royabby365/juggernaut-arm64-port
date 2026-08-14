using System.Collections;
using UnityEngine;

internal class Player : Person
{
	private string _currentVictoryIdleName;

	internal bool _ignoreIdle;

	private float _lastBagIdleTime = -1f;

	private float _lastBagIdleTimePeriod = -1f;

	private GameObject _shadow;

	private GameObject _shadow_r;

	private GameObject _shadow_l;

	private Transform _bones;

	private Transform _bone_toe_r;

	private Transform _bone_toe_l;

	private string _victoryAnimationName = "idle";

	private bool _makeVictoryIdle;

	internal bool InAttack { get; private set; }

	internal int Mana
	{
		get
		{
			return SingletonT<ServerData>.I.PlayerParams._mana;
		}
		set
		{
			if (SingletonT<ServerData>.I.PlayerParams._mana != value)
			{
				value = Mathf.Clamp(value, 0, SingletonT<ServerData>.I.GameSettings.MaxMana);
				SingletonT<ServerData>.I.PlayerParams._mana = value;
				Messenger.Invoke(Globals.MsgPersonManaChanged, this);
			}
		}
	}

	internal bool IsCastAllowed(int index)
	{
		return Globals.Battle != null && Globals.Battle.State == Battle.StateE.PlayerTime && Globals.Battle.CurrentSpellLevel >= 0 && Mana == 10;
	}

	public void CacheFxs()
	{
		if (Globals.CacheFxs)
		{
			CacheFxs(null, Globals.Enemy.ScenariosEvaluator, "fatality1", "react_death", "magic_direct", "magic_aoe", "attack_super", "attack", "attack_right", "attack_left", "botles_poison", "idle", "block", "damage", "death", "dodge", "magic_aoe", "magic_attack", "magic_baf", "attack_uppercot", "damage_force", "damage_uppercot", "step", "death_force", "death_uppercot", "react_death", "magic_baf_attack_fire", "magic_fireball", "magic_aoe_fire", "magic_baf_attack_ice", "magic_iceball", "magic_aoe_ice", "magic_baf_attack_dark", "magic_darkball", "magic_aoe_dark", "magic_baf_attack_electro", "magic_electroball", "magic_aoe_electro");
			SingletonT<Fxs>.I.LoadFx("react_dmg");
			SingletonT<Fxs>.I.LoadFx("react_dmgf");
		}
	}

	protected override void Idle()
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (!(HudMk1.Instance != null) || (HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.MainMap && HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Location))
		{
			if (base.IsDead && Globals.Battle.IsInBattleMode)
			{
				return;
			}
			if (InAttack)
			{
				InAttack = false;
				Messenger.Invoke(Globals.MsgPlayerAttackFinished, this);
			}
			if (_ignoreIdle)
			{
				return;
			}
		}
		if (!IsIdleAnimationPlaying())
		{
			_lastBagIdleTime = -1f;
			if (!string.IsNullOrEmpty(_currentVictoryIdleName))
			{
				PlayAnim(_currentVictoryIdleName, inLoop: false);
			}
			else
			{
				PlayAnim(GetNextIdleAnim(), inLoop: false);
			}
		}
	}

	protected override void Disabled()
	{
		base.Disabled();
		if (_shadow != null)
		{
			_shadow.SetActiveRecursivelyMk1(setActive: false);
		}
		if (_shadow_r != null)
		{
			_shadow_r.SetActiveRecursivelyMk1(setActive: false);
		}
		if (_shadow_l != null)
		{
			_shadow_l.SetActiveRecursivelyMk1(setActive: false);
		}
	}

	protected override void Enabled()
	{
		base.Enabled();
		if (_shadow != null)
		{
			_shadow.SetActiveRecursivelyMk1(setActive: true);
		}
		if (_shadow_r != null)
		{
			_shadow_r.SetActiveRecursivelyMk1(setActive: true);
		}
		if (_shadow_l != null)
		{
			_shadow_l.SetActiveRecursivelyMk1(setActive: true);
		}
	}

	protected override void StartImpl()
	{
		base.StartImpl();
		if (GetComponent<Animation>()[Globals.VictoryAnimationName] == null)
		{
			PersonData component = GetComponent<PersonData>();
			if (component != null)
			{
				AnimationClip animationClip = null;
				if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassRed && component.VictoryAnimationHummer != null)
				{
					animationClip = component.VictoryAnimationHummer;
				}
				else if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassGreen && component.VictoryAnimationGlave != null)
				{
					animationClip = component.VictoryAnimationGlave;
				}
				else if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassBlue && component.VictoryAnimation2Handed != null)
				{
					animationClip = component.VictoryAnimation2Handed;
				}
				if (animationClip != null)
				{
					GetComponent<Animation>().AddClip(animationClip, Globals.VictoryAnimationName);
				}
			}
		}
		_victoryAnimationName = Globals.VictoryAnimationName;
	}

	public void Restart()
	{
		_currentVictoryIdleName = null;
		PlayAnim(GetNextIdleAnim(), inLoop: false);
		if (!Globals.IsShadowEnabled)
		{
			return;
		}
		if (_shadow == null)
		{
			_shadow = (GameObject)Object.Instantiate(Globals.Battle.ShadowPrefab);
			_shadow.transform.localScale = new Vector3(4f, 4f, 1f);
		}
		if (_bones == null)
		{
			_bones = base.transform.FindChildByName("bones", includeInactive: true);
		}
		if (_bone_toe_r == null)
		{
			_bone_toe_r = base.transform.FindChildByName("bone_toe_r", includeInactive: true);
			if (_shadow_r == null)
			{
				_shadow_r = (GameObject)Object.Instantiate(Globals.Battle.ShadowPrefab);
			}
		}
		if (_bone_toe_l == null)
		{
			_bone_toe_l = base.transform.FindChildByName("bone_toe_l", includeInactive: true);
			if (_shadow_l == null)
			{
				_shadow_l = (GameObject)Object.Instantiate(Globals.Battle.ShadowPrefab);
			}
		}
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		if (deltaTime <= 0f)
		{
			return;
		}
		DoUpdate(deltaTime);
		if ((bool)GetComponent<Animation>() && !GetComponent<Animation>().isPlaying)
		{
			Idle();
		}
		if (Globals.IsShadowEnabled)
		{
			if (_shadow != null && _bones != null)
			{
				Vector3 position = _bones.position;
				position.y = base.transform.position.y;
				_shadow.transform.position = position;
			}
			if (_shadow_r != null && _bone_toe_r != null)
			{
				Vector3 position2 = _bone_toe_r.position;
				position2.y = base.transform.position.y;
				_shadow_r.transform.position = position2;
			}
			if (_shadow_l != null && _bone_toe_l != null)
			{
				Vector3 position3 = _bone_toe_l.position;
				position3.y = base.transform.position.y;
				_shadow_l.transform.position = position3;
			}
		}
	}

	internal void WaitForBattleWinAnimation(ActionD action)
	{
		Utils.Log("WaitForBattleWinAnimation", _victoryAnimationName);
		if (!GetComponent<Animation>().IsPlaying(_victoryAnimationName))
		{
			StartCoroutine(WaitForBattleWinAnimation_(action));
		}
		else
		{
			action();
		}
	}

	private IEnumerator WaitForBattleWinAnimation_(ActionD action)
	{
		while (!GetComponent<Animation>().IsPlaying(_victoryAnimationName))
		{
			yield return null;
		}
		action();
	}

	internal void AttackMagic(ServerData.Spell spell, string scenario, DamageTypeE damageType, float mult)
	{
		InAttack = true;
		ReactE reactE = ReactE.Critical | ReactE.Damage;
		int playerMagicDamage = SingletonT<ServerData>.I.GetPlayerMagicDamage(spell);
		mult *= SingletonT<ServerData>.I.GetEnemyMagicResist(damageType);
		playerMagicDamage = (int)((float)playerMagicDamage * mult);
		if (playerMagicDamage < 1)
		{
			playerMagicDamage = 1;
		}
		if (Globals.DebugPlayerOneHitKill)
		{
			playerMagicDamage = 77777;
		}
		if (Globals.DebugSmallDamageOnEnemy)
		{
			playerMagicDamage = 0;
		}
		if (Globals.DebugNoDamageOnEnemy)
		{
			playerMagicDamage = 0;
		}
		int num = Globals.Enemy.Health - playerMagicDamage;
		if (num <= 0)
		{
			if (Globals.UseFatalityWhiteSpheresMode)
			{
				reactE = ReactE.Death | ReactE.NoBubbles;
				if (reactE.HasFlag(ReactE.Critical))
				{
					reactE |= ReactE.Critical;
				}
			}
			else
			{
				reactE = ReactE.Critical | ReactE.Damage | ReactE.NoBubbles;
			}
			Battle.LocationToIncrement = AreaData.Current.Location;
		}
		reactE |= ReactE.NoBubbles;
		PlayScenario(Globals.Battle, scenario, AttackE.Magic, reactE, damageType, -playerMagicDamage);
		Metrics.OnPlayerCastSpell(spell);
		Messenger.Invoke(Globals.MsgPlayerCastSpell, damageType.AsMagic());
	}

	internal void Attack(AttackE attackType, int damage, DamageTypeE damageType, ReactE react)
	{
		InAttack = true;
		string text = ((attackType != AttackE.Combo) ? Utils.Select(attackType, null, AttackE.Forward, GetForwardAnimationName(), AttackE.Left, "attack_right", AttackE.Right, "attack_left") : "attack_super");
		Invs.Inv(text != null, "Player.Attack failed", base.name, attackType);
		PlayScenario(Globals.Battle, text, attackType, react, damageType, -damage);
	}

	internal void AttackCombo(int damage, ReactE react)
	{
		Attack(AttackE.Combo, damage, DamageTypeE.Natural, react);
	}

	public void SetupOnScene(Battle battle)
	{
		GameObject arenaCenter = battle.ArenaCenter;
		base.transform.position = arenaCenter.transform.position;
		if (Globals.Enemy != null)
		{
			base.transform.LookAt(Globals.Enemy.transform);
			base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
			base.transform.transform.Translate(0f, 0f, 0f - Globals.DistanceBetweenPersons);
		}
		else
		{
			base.transform.rotation = arenaCenter.transform.rotation;
			base.transform.transform.Translate(0f, 0f, Globals.DistanceBetweenPersons);
			base.transform.Rotate(0f, 180f, 0f);
		}
		Utils.DoWithComponent(this, includeInactive: true, delegate(PersonData _)
		{
			base.transform.Translate(_.TranslateAtScene);
		});
	}

	internal void CastPoisonElixir()
	{
		int damage = 0;
		int turns = 0;
		if (SingletonT<ServerData>.I.PlayerApplyPoison(out damage, out turns))
		{
			Globals.Battle.SetPoisonOnEnemy(damage);
			SingletonT<SoundManager>.I.PlayGlobalSound("use_elixir_poison");
			PlayScenario(Globals.Battle, "botles_poison", AttackE.None, ReactE.None, DamageTypeE.AcidMagic, 0);
			Messenger.Invoke(Globals.MsgPlayerUsePoison);
		}
	}

	internal void EatAttackForceElixir()
	{
		int num = SingletonT<ServerData>.I.PlayerEatCriticalElixir();
		if (num > 0)
		{
			Globals.Battle.NextAttackIsWithForceElixir = num;
			SingletonT<SoundManager>.I.PlayGlobalSound("use_elixir_strength");
			SingletonT<Fxs>.I.PlayPersonFx(Globals.Battle.PlayerCritFX, base.gameObject, "pos_hand_r");
			SingletonT<Fxs>.I.PlayPersonFx(Globals.Battle.PlayerCritFX, base.gameObject, "pos_hand_l");
		}
	}

	internal void MakeVictoryIdle()
	{
		if (_victoryAnimationName != null)
		{
			_currentVictoryIdleName = _victoryAnimationName;
		}
	}

	internal override void AnimationPlayed(string animName)
	{
		if (_makeVictoryIdle && !animName.StartsWith("idle"))
		{
			_makeVictoryIdle = false;
			MakeVictoryIdle();
		}
	}

	internal void PlayFatality(string scenario, ActionD onFinish)
	{
		InAttack = true;
		GetComponent<Animation>().wrapMode = WrapMode.Once;
		_makeVictoryIdle = true;
		PlayScenario(Globals.Battle, scenario, AttackE.None, ReactE.None, DamageTypeE.Natural, 0, onFinish);
	}

	internal void CastMagic(ServerData.Spell spell, string scenario, int manaDrop, DamageTypeE damageType, float mult)
	{
		if (Mana >= manaDrop)
		{
			if (!Globals.DebugFullMana)
			{
				Mana -= manaDrop;
			}
			SingletonT<ServerData>.I.PlayerParams.SpellCasted(spell);
			AttackMagic(spell, scenario, damageType, mult);
		}
	}

	internal void EatHealthPotion(GameObject fxPrefab)
	{
		if (SingletonT<ServerData>.I.PlayerEatHealthElixir() && fxPrefab != null)
		{
			SingletonT<SoundManager>.I.PlayGlobalSound("use_elixir_health");
			SingletonT<Fxs>.I.PlayPersonFx(fxPrefab, base.gameObject, "pos_spinecenter").transform.Translate(0f, 1f, 0f);
		}
	}

	internal void BreakBattle()
	{
		base.ScenariosEvaluator.StopAll(base.name);
		StopAllCoroutines();
		_ignoreIdle = false;
		InAttack = false;
		GetComponent<Animation>().Stop();
	}

	protected override void ShowNaturalReact(AttackE attack, string reactScenario, DamageTypeE damageType, int hp, string hpText)
	{
		base.ShowNaturalReact(attack, reactScenario, damageType, hp, hpText);
		if (damageType == DamageTypeE.DarkMagic || damageType == DamageTypeE.FireMagic || damageType == DamageTypeE.IceMagic || damageType == DamageTypeE.LightingMagic)
		{
			Globals.Battle.BattleCameraController.Shake();
		}
	}

	protected override GameObject GetDigitsRoot()
	{
		if (HudMk1.Instance == null)
		{
			return null;
		}
		PlayerHud componentInChildren = HudMk1.Instance.GetComponentInChildren<PlayerHud>();
		return componentInChildren.HpDigits.gameObject;
	}
}
