using System;
using System.Collections;
using System.Collections.Generic;
using Scenarios.TestEvaluator;
using UnityEngine;
using Yarx;

public class Person : MonoBehaviour
{
	public enum FightPhraseE
	{
		Dodge,
		Immunity,
		Block,
		Execution
	}

	private class StepData
	{
		internal Transform Person;

		internal Transform Root;

		private Transform Helper;

		private Transform Target;

		private readonly float _speed;

		internal readonly bool IdleOnDone;

		internal bool Done { get; private set; }

		internal StepData(Transform person, Transform root, float angle, float speed, bool idleOnEnd)
		{
			Person = person;
			Root = root;
			Helper = new GameObject(person.gameObject.name + ".step.helper").transform;
			Target = new GameObject(person.gameObject.name + ".step.target").transform;
			Target.position = person.transform.position;
			Target.RotateAround(root.transform.position, Vector3.up, angle);
			Done = false;
			_speed = speed;
			IdleOnDone = idleOnEnd;
		}

		internal void Update(float dt)
		{
			if (!Done)
			{
				Done = (double)Vector3.Distance(Person.position, Target.position) <= 0.1;
				if (Done)
				{
					Person.position = Target.position;
					UnityEngine.Object.Destroy(Helper.gameObject);
					UnityEngine.Object.Destroy(Target.gameObject);
					return;
				}
				Helper.position = Person.position;
				Helper.LookAt(Target.position);
				Helper.Translate(0f, 0f, dt * _speed);
				Person.position = Helper.position;
				Person.LookAt(Root.position, Vector3.up);
				Quaternion rotation = Person.rotation;
				rotation.z = 0f;
				rotation.x = 0f;
				Person.rotation = rotation;
			}
		}
	}

	internal string ModelName;

	internal Vector3 InitScale;

	internal Vector3 _size;

	protected ServerData.BotInfo _serverBotInfo;

	protected ServerData.BotLevel _serverLevelInfo;

	private CompositeDisposable _listeners = new CompositeDisposable();

	private bool _hasFindWeaponTrail;

	public static readonly string[] CommonIdleAnims = (Globals.UseOnlyIdle ? new string[1] { "idle" } : new string[2] { "idle", "idle2" });

	protected List<string> _idleAnims = new List<string>();

	private float _idleTime;

	private int _health;

	private bool _callDeathEvent;

	protected bool _attackJustExecuted;

	protected static readonly string FxNameTag = "_fx_tag_person_effect_";

	protected static readonly string FxNameContext = "_context:::";

	internal static readonly float TextSpeedDigits = 1.4f;

	internal static readonly float TextTimeScreenDigits = 1.4f;

	internal static readonly float TextSpeedScreenDigits = 80f;

	internal static readonly float TextSpeedText = 1.1f;

	private float _textSpeedStats = 1f;

	private List<GameObject> _fxs = new List<GameObject>();

	private StepData _step;

	public TestEvaluator ScenariosEvaluator { get; set; }

	public GameObject Weapon { get; private set; }

	public GameObject WeaponTrail { get; private set; }

	internal ServerData.BotInfo ServerBotInfo => _serverBotInfo;

	public int MaxHealth { get; set; }

	internal int Health
	{
		get
		{
			return _health;
		}
		set
		{
			if (value > 0 || _health > 0)
			{
				int health = _health;
				_health = ((value > MaxHealth) ? MaxHealth : ((value >= 0) ? value : 0));
				if (health != _health)
				{
					Messenger<int, int>.Invoke((!(Globals.PlayerGameObject == base.gameObject)) ? Globals.MsgEnemyHealthChanged : Globals.MsgPlayerHealthChanged, _health, MaxHealth);
				}
				_callDeathEvent = _health <= 0 && health != _health && Globals.Battle.IsInBattleMode;
			}
		}
	}

	public bool IsDead => _health <= 0;

	public bool IsDeadAndNoAnimation => IsDead && !base.animation.isPlaying;

	internal bool IsFemale
	{
		get
		{
			PersonData componentInChildren = base.gameObject.GetComponentInChildren<PersonData>();
			return componentInChildren != null && componentInChildren.ScenariosIndex == "2";
		}
	}

	internal AnimationTypes CurrentWeaponType
	{
		get
		{
			PersonArmor componentInChildren = GetComponentInChildren<PersonArmor>();
			if (componentInChildren == null)
			{
				return AnimationTypes.None;
			}
			if (componentInChildren.Weapon == null)
			{
				return AnimationTypes.None;
			}
			ArmorData component = componentInChildren.Weapon.GetComponent<ArmorData>();
			Invs.Inv(component != null, "weaponData != null", base.name);
			return component.WeaponAnimationType;
		}
	}

	internal bool InStep => _step != null;

	protected Person()
	{
		_listeners.Add(Messenger<GameObject>.AddListener(Globals.MsgPlayerWeaponChanged, delegate(GameObject weapon)
		{
			Weapon = weapon;
			_hasFindWeaponTrail = false;
		}));
	}

	public void Pause(bool pause)
	{
		if (!pause)
		{
		}
	}

	protected void CacheFxs(string tag, TestEvaluator other, params string[] scenarios)
	{
		if (ScenariosEvaluator == null)
		{
			ScenariosEvaluator = SingletonT<PersonsScenarios>.I.LoadEvaluator(GetComponent<PersonData>().ScenariosIndex);
		}
		if (!Globals.CacheFxs)
		{
			return;
		}
		foreach (string scenarioName in scenarios)
		{
			ScenariosEvaluator.VisitEachFX(scenarioName, other, delegate(string fx)
			{
				SingletonT<Fxs>.I.LoadFx(fx, tag);
			});
		}
	}

	internal virtual void SetServerData(ServerData.BotInfo botInfo, ServerData.BotLevel levelData)
	{
		if (_serverBotInfo != botInfo)
		{
			_serverBotInfo = botInfo;
			if (botInfo != null)
			{
				ModelName = botInfo.Model;
			}
			if (botInfo != null && botInfo.Scale != 0f)
			{
				DoScale(1f, useInitScale: true);
				DoScale(botInfo.Scale, useInitScale: true);
			}
		}
		_serverLevelInfo = levelData;
	}

	private void Awake()
	{
		if (ScenariosEvaluator == null)
		{
			ScenariosEvaluator = SingletonT<PersonsScenarios>.I.LoadEvaluator(GetComponent<PersonData>().ScenariosIndex);
		}
		InitScale = base.transform.localScale;
		_size = Vector3.one;
	}

	private void Start()
	{
		try
		{
			StartImpl();
		}
		catch (Exception ex)
		{
			Utils.HandleError(this, "Person.Start failed", ex);
		}
	}

	public void InitializeIdleAnimations()
	{
		_idleAnims = new List<string>();
		for (int i = 0; i < CommonIdleAnims.Length; i++)
		{
			string text = CommonIdleAnims[i];
			if (base.animation[text] != null)
			{
				base.animation[text].wrapMode = WrapMode.Once;
				_idleAnims.Add(CommonIdleAnims[i]);
			}
		}
		if (_idleAnims.Count == 0 && Globals.IsDebugBuild)
		{
			Debug.LogError("Can't find any idle animation");
		}
	}

	private void OnDisable()
	{
		Disabled();
	}

	protected virtual void Disabled()
	{
		StopAllCoroutines();
		List<GameObject> list = new List<GameObject>(_fxs);
		_fxs.Clear();
		foreach (GameObject item in list)
		{
			UnityEngine.Object.Destroy(item);
		}
		_listeners.Dispose();
	}

	private void OnEnable()
	{
		Enabled();
	}

	protected virtual void Enabled()
	{
	}

	public void DoScale(float s, bool useInitScale)
	{
		if (base.transform != null)
		{
			base.transform.localScale = ((!useInitScale) ? Vector3.one : InitScale) * s;
		}
		_size = new Vector3(s, s, s);
	}

	protected virtual void StartImpl()
	{
	}

	protected virtual void Idle()
	{
	}

	protected void DoUpdate(float dt)
	{
		if (_step != null)
		{
			_step.Update(dt);
			if (_step.Done)
			{
				if (_step.IdleOnDone && !base.animation.IsPlaying("damage") && !base.animation.IsPlaying("damage_force") && !base.animation.IsPlaying("block") && !base.animation.IsPlaying("dodge"))
				{
					Idle();
				}
				_step = null;
			}
		}
		if (IsDead && _callDeathEvent && !base.animation.isPlaying)
		{
			_callDeathEvent = false;
			Messenger<Person>.Invoke(Globals.MsgPersonDie, this);
		}
		if (Weapon != null && !_hasFindWeaponTrail)
		{
			_hasFindWeaponTrail = true;
			Transform transform = Weapon.transform.FindOneOfChildByName(true, "sfx_weapon_trail(Clone)");
			if (transform != null)
			{
				transform.gameObject.SetActive(true);
				WeaponTrail = transform.gameObject;
			}
		}
	}

	internal virtual void Die()
	{
		if (Health > 0)
		{
			Health = 0;
		}
		_callDeathEvent = false;
		_attackJustExecuted = false;
		Messenger<Person>.Invoke(Globals.MsgPersonDie, this);
	}

	protected string GetNextIdleAnim()
	{
		foreach (string idleAnim in _idleAnims)
		{
			if (base.animation.IsPlaying(idleAnim))
			{
				return idleAnim;
			}
		}
		SingletonT<ServerData>.I.GameSettings.IdlePeriod = 2f;
		string result = "idle";
		if (_step != null && !_step.Done)
		{
			return result;
		}
		bool flag = Globals.Enemy != null && Globals.Enemy.Equals(this);
		if ((!flag && Time.time - _idleTime > SingletonT<ServerData>.I.GameSettings.IdlePeriod) || (flag && _attackJustExecuted))
		{
			if (_idleAnims.Count > 0)
			{
				result = ((!flag) ? _idleAnims[UnityEngine.Random.Range(0, _idleAnims.Count)] : ((!_attackJustExecuted || _idleAnims.Count <= 1 || UnityEngine.Random.Range(0, 100) >= SingletonT<ServerData>.I.GameSettings.Idle2Prob) ? _idleAnims[0] : _idleAnims[UnityEngine.Random.Range(1, _idleAnims.Count)]));
				_attackJustExecuted = false;
				_idleTime = Time.time + base.animation[result].length;
			}
			else
			{
				_idleTime = Time.time;
			}
		}
		else if (_idleAnims.Count > 0)
		{
			result = _idleAnims[0];
		}
		return result;
	}

	internal bool IsIdleAnimationPlaying()
	{
		foreach (string idleAnim in _idleAnims)
		{
			if (base.animation.IsPlaying(idleAnim))
			{
				return true;
			}
		}
		return false;
	}

	internal void PlayAnim(string animName, bool inLoop)
	{
		PlayAnim(animName, inLoop, crossFade: false);
	}

	internal void PlayAnim(string animName, bool inLoop, bool crossFade)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (WeaponTrail != null)
		{
			WeaponTrail.SetActiveRecursivelyMk1(animName.StartsWith("attack"));
		}
		if (inLoop)
		{
			AnimationState animationState = base.animation[animName];
			if (animationState != null && animationState.wrapMode != WrapMode.Loop)
			{
				animationState.wrapMode = WrapMode.Loop;
			}
		}
		if (!(base.animation[animName] != null))
		{
			return;
		}
		if (crossFade)
		{
			base.animation.CrossFade(animName, 0.5f);
		}
		else
		{
			base.animation.Play(animName);
		}
		if (Globals.MainMenu != null && HudMk1.Instance != null && (animName == "idle2" || animName == "idle3"))
		{
			HudMk1.GuiDesc currentGui = HudMk1.Instance.CurrentGui;
			if (currentGui.Type == GuiRoot.GuiType.Fight || currentGui.Type == GuiRoot.GuiType.EnemyTurn || currentGui.Type == GuiRoot.GuiType.CastMagic || currentGui.Type == GuiRoot.GuiType.Execution || currentGui.Type == GuiRoot.GuiType.BattleHud || currentGui.Type == GuiRoot.GuiType.StrongMagicMiniGame || currentGui.Type == GuiRoot.GuiType.WeakMagicMiniGame || currentGui.Type == GuiRoot.GuiType.PopupDefeat)
			{
				PlayScenario(Globals.Battle, "onidle", AttackE.None, ReactE.None, DamageTypeE.Timeout, 0);
			}
		}
	}

	internal void FxDestroy(string groupName, float delayTime)
	{
		string filterName = groupName + "(Clone)";
		List<GameObject> list = new List<GameObject>(_fxs.FindAll((GameObject _) => _ != null && _.name.StartsWith(filterName) && !_.name.EndsWith("onstart") && (_.transform.parent == null || _.transform.parent == base.transform || _.transform.root == base.transform || Utils.IsTransformChildrenOf(_.transform, base.transform))));
		if (list.Count > 0)
		{
			StartCoroutine(FxDestroy2_(groupName, delayTime));
		}
	}

	private IEnumerator FxDestroy2_(string groupName, float delayTime)
	{
		while (delayTime > 0f)
		{
			delayTime -= Time.deltaTime;
		}
		string filterName = groupName + "(Clone)";
		List<GameObject> fxs = new List<GameObject>(_fxs.FindAll((GameObject _) => _ != null && _.name.StartsWith(filterName) && !_.name.EndsWith("onstart")));
		while (fxs.Count > 0)
		{
			DestroyFxs(fxs);
			yield return null;
		}
	}

	private void DestroyFxs(List<GameObject> fxs)
	{
		for (int i = 0; i < fxs.Count; i++)
		{
			GameObject gameObject = fxs[i];
			bool flag = false;
			if (gameObject == null)
			{
				flag = true;
			}
			else
			{
				bool flag2 = false;
				ParticleEmitter[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleEmitter>();
				foreach (ParticleEmitter particleEmitter in componentsInChildren)
				{
					particleEmitter.emit = false;
					if (particleEmitter.particleCount > 0)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
			}
			if (flag)
			{
				fxs.RemoveAt(i);
				i--;
				if (gameObject != null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}
	}

	private IEnumerator FxDestroy_(List<GameObject> fxs, float delayTime)
	{
		while (delayTime > 0f)
		{
			delayTime -= Time.deltaTime;
		}
		while (fxs.Count > 0)
		{
			DestroyFxs(fxs);
			yield return null;
		}
	}

	internal void Fx(string id, string fxName, string posName, bool isParent, float destroyTime, string context)
	{
		Transform transform = BodyPart(posName);
		if (transform == null)
		{
			return;
		}
		if (fxName == "sfx_react_fatality")
		{
			GameObject gameObject = ShowFightPhrase(FightPhraseE.Execution, 1.5f, 0f, TextSpeedText);
			if (gameObject != null && Globals.Enemy != null && Globals.Player != null)
			{
				Vector3 position = (Globals.Enemy.transform.position + Globals.Player.transform.position) / 2f;
				position.y = gameObject.transform.position.y;
				gameObject.transform.position = position;
			}
			return;
		}
		GameObject newFx = SingletonT<Fxs>.I.NewFx(fxName, transform.position, base.transform.rotation, (!isParent) ? null : transform, forceDraw: false);
		if (newFx == null)
		{
			Utils.Log("NOFX", fxName);
			return;
		}
		GameObject obj = newFx;
		string text = obj.name;
		obj.name = text + FxNameTag + id + FxNameContext + context;
		_fxs.Add(newFx);
		if (destroyTime < 0f)
		{
			return;
		}
		if (destroyTime == 0f)
		{
			destroyTime = Globals.FxTimeout;
		}
		SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent("Person.DestroyFx", destroyTime, delegate
		{
			if (newFx != null && newFx.gameObject != null)
			{
				newFx.gameObject.SetActiveRecursivelyMk1(setActive: false);
				if (Fxs.CurrentFxTag.Length > 0)
				{
					Fxs.Fxses.Add(newFx.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(newFx);
				}
			}
		});
	}

	internal Transform BodyPart(string posName)
	{
		if (this == null)
		{
			return null;
		}
		if (base.gameObject == null)
		{
			return null;
		}
		Transform transform = null;
		PersonData componentInChildren = base.gameObject.GetComponentInChildren<PersonData>();
		if (componentInChildren == null)
		{
			return null;
		}
		switch (posName)
		{
		case "pos_head":
			transform = componentInChildren.PosHead;
			break;
		case "pos_eyes":
			transform = componentInChildren.PosEyes;
			break;
		case "pos_hand_l":
			transform = componentInChildren.PosHandL;
			break;
		case "pos_hand_r":
			transform = componentInChildren.PosHandR;
			break;
		case "pos_weapon":
			transform = componentInChildren.PosWeapon;
			break;
		case "pos_spinecenter":
			transform = componentInChildren.PosSpineCenter;
			break;
		case "pos_toe_l":
			transform = componentInChildren.PosToeL;
			break;
		case "pos_toe_r":
			transform = componentInChildren.PosToeR;
			break;
		case "pos_middle":
			transform = componentInChildren.PosMiddle;
			break;
		case "pos_hip_r":
			transform = componentInChildren.PosHipR;
			break;
		case "pos_hip_l":
			transform = componentInChildren.PosHipL;
			break;
		case "pos_shoulder_l":
			transform = componentInChildren.PosShoulderL;
			break;
		case "pos_shoulder_r":
			transform = componentInChildren.PosShoulderR;
			break;
		case "middle":
		{
			GameObject gameObject = GameObject.Find("camera_target_look");
			if (gameObject != null)
			{
				transform = gameObject.transform;
			}
			break;
		}
		}
		return (!(transform != null)) ? base.transform : transform;
	}

	internal void FxPos(string groupName, Vector3 pos, bool absolute)
	{
		string value = "_" + groupName;
		bool flag = false;
		foreach (GameObject fx in _fxs)
		{
			if (fx == null)
			{
				flag = true;
			}
			else if (fx.name.Contains(value))
			{
				Transform transform = fx.transform;
				Transform parent = transform.parent;
				if (parent == base.transform || parent == null)
				{
					transform.Translate(pos, (!absolute) ? Space.Self : Space.World);
				}
			}
		}
		if (flag)
		{
			_fxs.RemoveAll((GameObject _) => _ == null);
		}
	}

	internal void FxRot(string groupName, Vector3 pos, bool absolute)
	{
		string value = "_" + groupName;
		foreach (GameObject fx in _fxs)
		{
			if (fx.name.Contains(value))
			{
				Transform transform = fx.transform;
				Transform parent = transform.parent;
				if (parent == base.transform || parent == null)
				{
					transform.Rotate(pos, (!absolute) ? Space.Self : Space.World);
				}
			}
		}
	}

	internal void reactfx(AttackE attack, ReactE reaction, DamageTypeE damageType, int hp, int absorb)
	{
		Messenger<Person>.Invoke(Globals.MsgBattleReactFx, this);
		string reactScenario = reaction.ReactionScenario("0");
		MagicTypeE magicTypeE = damageType.AsMagic();
		if (hp < 0)
		{
			Health += hp;
		}
		string text = hp.ToString();
		if (!text.Contains("-"))
		{
			text = "+" + text;
		}
		else
		{
			Messenger<Person, int, ReactE, AttackE>.Invoke(Globals.MsgPersonDamaged, this, hp, reaction, attack);
		}
		if (magicTypeE != MagicTypeE.None)
		{
			ShowMagicReact(attack, reactScenario, damageType, hp, text);
		}
		else
		{
			ShowNaturalReact(attack, reactScenario, damageType, hp, text);
		}
	}

	protected virtual void ShowNaturalReact(AttackE attack, string reactScenario, DamageTypeE damageType, int hp, string hpText)
	{
		bool flag = false;
		if (reactScenario == Globals.ReactBlock)
		{
			ShowFightPhraseOnHpBar(FightPhraseE.Block, 1.5f, new Vector3(0f, -50f, -50f));
			flag = true;
		}
		else if (reactScenario == Globals.ReactDodge)
		{
			ShowFightPhraseOnHpBar(FightPhraseE.Dodge, 1.5f, new Vector3(0f, -50f, -50f));
			flag = true;
			if (Globals.Enemy == this)
			{
				SingletonT<SoundManager>.I.PlayFailSound();
			}
		}
		Vector3 offset = new Vector3(0f, -20 - (flag ? 50 : 0), -50f);
		if (hp != 0)
		{
			if (damageType == DamageTypeE.AcidMagic)
			{
				hpText += Globals.CharIconPoison;
				ShowTextOnHpBar(hpText, TextTimeScreenDigits, offset, TextSpeedScreenDigits, FontManager.ColorE.PoisonDamage);
			}
			else if (reactScenario == Globals.ReactDamage || reactScenario == Globals.ReactDeath)
			{
				ShowTextOnHpBar(hpText, TextTimeScreenDigits, offset, TextSpeedScreenDigits, FontManager.ColorE.BagMoney);
			}
			else if (reactScenario == Globals.ReactDamageForced || reactScenario == Globals.ReactDeathForced)
			{
				ShowTextOnHpBar(hpText, TextTimeScreenDigits, offset, TextSpeedScreenDigits, FontManager.ColorE.DamageForced);
			}
			else if (reactScenario == Globals.ReactHeal)
			{
				ShowTextOnHpBar(hpText, TextTimeScreenDigits, offset, TextSpeedScreenDigits, FontManager.ColorE.CompareGreen);
			}
			else if (reactScenario == Globals.ReactBlock && hp < 0)
			{
				ShowTextOnHpBar(hpText, TextTimeScreenDigits, offset, TextSpeedScreenDigits, FontManager.ColorE.BagMoney);
			}
		}
	}

	protected virtual void ShowMagicReact(AttackE attack, string reactScenario, DamageTypeE damageType, int hp, string hpText)
	{
		ShowNaturalReact(attack, reactScenario, damageType, hp, hpText);
	}

	internal GameObject ShowText(string text, float worldSize, float time, float yOffset, float speed, FontManager.ColorE color)
	{
		if (Globals.Battle == null || Globals.Battle.TextPrefab == null)
		{
			return null;
		}
		Transform transform = base.transform.FindChildByName("bone_head");
		if (transform == null)
		{
			transform = base.transform.FindChildByName("pos_head");
		}
		Transform transform2 = base.transform.FindChildByName("bones");
		if (transform == null)
		{
			if (transform2 == null)
			{
				return null;
			}
			transform = transform2;
		}
		else if (transform2 != null)
		{
			transform = ((!(transform.position.y > transform2.position.y)) ? transform2 : transform);
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Globals.Battle.TextPrefab);
		gameObject.transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
		SpriteText componentInChildren = gameObject.GetComponentInChildren<SpriteText>();
		float num = worldSize / (float)componentInChildren.PixSize_;
		gameObject.transform.localScale = new Vector3(num, num, num);
		componentInChildren.Text_ = text;
		componentInChildren.NamedColorE_ = color;
		TextAnimator component = gameObject.GetComponent<TextAnimator>();
		component.LifeTime = time;
		component.Speed = speed;
		return gameObject;
	}

	internal void ShowTextOnHpBar(string text, float time, Vector3 offset, float speed, FontManager.ColorE color)
	{
		Globals.MainMenu.StartCoroutine(ShowTextOnHpBarCoro(text, time, offset, speed, color));
	}

	internal IEnumerator ShowTextOnHpBarCoro(string text, float time, Vector3 offset, float speed, FontManager.ColorE color)
	{
		GameObject digitsRoot = GetDigitsRoot();
		GameObject textGO = new GameObject();
		textGO.transform.SetLayerRecursively(digitsRoot.transform);
		textGO.transform.position = digitsRoot.transform.position + offset;
		textGO.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		SpriteText spriteText = textGO.AddComponent<SpriteText>();
		spriteText.Anchor_ = TextAnchor.MiddleCenter;
		spriteText.Bold_ = true;
		spriteText.Text_ = text;
		spriteText.NamedColorE_ = color;
		float currentTime = 0f;
		while (currentTime < time)
		{
			textGO.transform.position += new Vector3(0f, (0f - speed) * Time.deltaTime, 0f);
			spriteText.TextAlpha_ = 1f - currentTime / time;
			currentTime += Time.deltaTime;
			yield return null;
		}
		UnityEngine.Object.Destroy(textGO);
	}

	protected virtual GameObject GetDigitsRoot()
	{
		throw new NotImplementedException();
	}

	private string ConvertPhrase(FightPhraseE phrase)
	{
		return phrase switch
		{
			FightPhraseE.Dodge => "fight_dodge", 
			FightPhraseE.Immunity => "fight_immunity", 
			FightPhraseE.Block => "fight_block", 
			FightPhraseE.Execution => "fight_execution", 
			_ => string.Empty, 
		};
	}

	protected GameObject ShowFightPhrase(FightPhraseE phrase, float time, float yOffset, float speed)
	{
		if (Globals.Battle == null || Globals.Battle.TextPrefab == null)
		{
			return null;
		}
		Transform transform = base.transform.FindChildByName("bone_head");
		if (transform == null)
		{
			transform = base.transform.FindChildByName("pos_head");
		}
		Transform transform2 = base.transform.FindChildByName("bones");
		if (transform == null)
		{
			if (transform2 == null)
			{
				return null;
			}
			transform = transform2;
		}
		else if (transform2 != null)
		{
			transform = ((!(transform.position.y > transform2.position.y)) ? transform2 : transform);
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Globals.Battle.PhrasePrefab);
		gameObject.transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
		Sprite componentInChildren = gameObject.GetComponentInChildren<Sprite>();
		componentInChildren.SpriteName_ = ConvertPhrase(phrase);
		TextAnimator component = gameObject.GetComponent<TextAnimator>();
		component.LifeTime = time;
		component.Speed = speed;
		return gameObject;
	}

	protected void ShowFightPhraseOnHpBar(FightPhraseE phrase, float time, Vector3 offset)
	{
		Globals.MainMenu.StartCoroutine(ShowFightPhraseOnHpBarCoro(phrase, time, offset));
	}

	protected IEnumerator ShowFightPhraseOnHpBarCoro(FightPhraseE phrase, float time, Vector3 offset)
	{
		GameObject root = GetDigitsRoot();
		GameObject phraseGO = new GameObject();
		phraseGO.transform.SetLayerRecursively(root.transform);
		phraseGO.transform.position = root.transform.position + offset;
		phraseGO.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		Sprite sprite = phraseGO.AddComponent<Sprite>();
		sprite.SpriteName_ = ConvertPhrase(phrase);
		sprite.Origin = Quad.OriginPlace.Center;
		float currentTime = 0f;
		float curveTimeScale = Globals.Battle.FightPhraseAlphaCurve.Duration() / time;
		while (currentTime < time)
		{
			sprite.Tint_ = new Color(a: Globals.Battle.FightPhraseAlphaCurve.Evaluate(currentTime * curveTimeScale), r: sprite.Tint.r, g: sprite.Tint.g, b: sprite.Tint.b);
			currentTime += Time.deltaTime;
			yield return null;
		}
		UnityEngine.Object.Destroy(phraseGO);
	}

	public void ShowStatsChanging(string text1, bool sign1, string text2, bool sign2)
	{
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		FontManager.ColorE colorE = FontManager.ColorE.CompareGreen;
		if (!string.IsNullOrEmpty(text1))
		{
			colorE = ((!sign1) ? FontManager.ColorE.CompareRed : FontManager.ColorE.CompareGreen);
			gameObject = ShowText(text1, 0.3f, 2.5f, -0.7f, _textSpeedStats, colorE);
		}
		if (!string.IsNullOrEmpty(text2))
		{
			colorE = ((!sign2) ? FontManager.ColorE.CompareRed : FontManager.ColorE.CompareGreen);
			gameObject2 = ShowText(text2, 0.3f, 2.5f, -0.7f, _textSpeedStats, colorE);
		}
		if (gameObject != null && gameObject2 != null)
		{
			SpriteText componentInChildren = gameObject.GetComponentInChildren<SpriteText>();
			SpriteText componentInChildren2 = gameObject2.GetComponentInChildren<SpriteText>();
			componentInChildren.transform.localPosition = new Vector3(componentInChildren.transform.localPosition.x - 35f, componentInChildren.transform.localPosition.y, componentInChildren.transform.localPosition.z);
			componentInChildren2.transform.localPosition = new Vector3(componentInChildren2.transform.localPosition.x + 35f, componentInChildren2.transform.localPosition.y, componentInChildren2.transform.localPosition.z);
		}
	}

	internal GameObject sfx(string fxName)
	{
		UnityEngine.Object obj = SingletonT<Fxs>.I.LoadFx(fxName + "_" + UnityApi.GetLanguage());
		if (obj == null)
		{
			obj = SingletonT<Fxs>.I.LoadFx(fxName);
		}
		if (obj == null)
		{
			Utils.Log("sfx failed no fx", base.name, fxName);
			return null;
		}
		Transform transform = base.transform.Find("bones");
		if (transform == null)
		{
			Utils.Log("sfx failed no 'bones'", base.name, fxName);
			return null;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(obj, transform.transform.position, Quaternion.identity);
		gameObject.transform.parent = transform;
		return gameObject;
	}

	internal virtual void AnimationPlayed(string animName)
	{
	}

	internal float PlayAnimPart(string animName, float speed, int start, int end)
	{
		AnimationState animationState = base.animation[animName];
		if (animationState == null)
		{
			Utils.Log("PlayAnimPart can't find", animName);
			return 0f;
		}
		string newName = "play_part_" + animName + "_" + start + "_" + end;
		if (base.animation[newName] == null)
		{
			base.animation.AddClip(animationState.clip, newName, start, end);
		}
		AnimationState animationState2 = base.animation[newName];
		animationState2.speed = speed;
		base.animation.wrapMode = WrapMode.Once;
		base.animation.CrossFade(newName, 0.3f, PlayMode.StopAll);
		return (animationState2.length - 0.3f) / animationState2.speed;
	}

	internal float PlayAnim(string animName, float wait)
	{
		if (WeaponTrail != null)
		{
			WeaponTrail.SetActiveRecursivelyMk1(animName.StartsWith("attack"));
		}
		string text = animName;
		if (animName.Length > 4 && base.animation[animName] == null)
		{
			if (animName.StartsWith("magic"))
			{
				animName = "magic";
			}
			else if (animName.StartsWith("attac"))
			{
				animName = "attack";
			}
			else if (animName.StartsWith("death"))
			{
				animName = "death";
			}
			else if (animName.StartsWith("damag"))
			{
				animName = "damage";
			}
		}
		AnimationState animationState = base.animation[animName];
		Invs.Inv(animationState != null, "PlayAnim failed, cant find animation", text, animName);
		base.animation.wrapMode = WrapMode.Once;
		base.animation[animName].wrapMode = WrapMode.Once;
		if (base.animation.IsPlaying(animName))
		{
			base.animation.Play(animName);
		}
		else
		{
			base.animation.CrossFade(animName, 0.3f, PlayMode.StopAll);
		}
		if (wait > 0f)
		{
			wait = animationState.length / 100f * wait;
		}
		AnimationPlayed(animName);
		return wait;
	}

	public void PlayScenario(Battle battle, string scenario)
	{
		PlayScenario(battle, scenario, AttackE.None, ReactE.None, DamageTypeE.Timeout, 0, null);
	}

	public void PlayScenario(Battle battle, string scenario, AttackE attack, ReactE react, DamageTypeE damageType, int hp)
	{
		PlayScenario(battle, scenario, attack, react, damageType, hp, null);
	}

	public void PlayScenario(Battle battle, string scenario, AttackE attack, ReactE react, DamageTypeE damageType, int hp, ActionD onFinish)
	{
		try
		{
			ScenariosEvaluator.PlayScenario(scenario, new PersonsScenarios.Context(base.name, scenario, battle, this, attack, react, hp, damageType), onFinish);
		}
		catch (Exception ex_)
		{
			Utils.HandleError(ex_, "PlayScenario failed name=" + base.name, " scenario=" + scenario);
		}
	}

	internal float react(AttackE attack, ReactE reaction, DamageTypeE damageType, int hp, string dir, int wait)
	{
		Person opponent = Globals.Battle.Opponent(this);
		Messenger.Invoke(Globals.MsgPersonReact, opponent, attack, reaction, damageType);
		if (damageType.IsMagic() && reaction.HasFlag(ReactE.Damage) && opponent.gameObject.Equals(Globals.Player.gameObject))
		{
			ServerData.Settings gameSettings = SingletonT<ServerData>.I.GameSettings;
			int count = ((gameSettings.CritOnPersonManaBallsCount >= Globals.Player.Mana) ? Globals.Player.Mana : gameSettings.CritOnPersonManaBallsCount);
			if (count > 0)
			{
				Globals.Player.Mana -= count;
				if (wait != 1)
				{
					Messenger.Invoke(Globals.MsgSpawnBubblesFromPlayer, count, 1);
				}
				else
				{
					SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(base.name, Globals.ReactWaitTime, delegate
					{
						Messenger.Invoke(Globals.MsgSpawnBubblesFromPlayer, count, 1);
					});
				}
			}
		}
		if (Globals.Battle.State == Battle.StateE.FatalityMode || Globals.Battle.State == Battle.StateE.FatalityModeExecute)
		{
			SingletonT<SoundManager>.I.PlayGlobalSound("fatality_explosion");
		}
		string text = reaction.ReactionScenario(dir);
		if (text != null)
		{
			Utils.LogForce(">>>>>>>>>>react", text, base.name);
			opponent.PlayScenario(Globals.Battle, text, attack, reaction, damageType, hp, delegate
			{
				if (reaction.HasFlag(ReactE.Death) && reaction.HasFlag(ReactE.Block))
				{
					opponent.PlayAnim("death", inLoop: false);
				}
			});
		}
		else
		{
			Utils.Log("react NO SCENARIO", reaction, damageType, dir);
		}
		if (wait == 1)
		{
			return Globals.ReactWaitTime;
		}
		if (Globals.Player == this)
		{
			Globals.Battle.ShowMagicScreenFx(damageType);
		}
		return 0f;
	}

	internal float GetStepSpeed(float stepSpeed)
	{
		if (stepSpeed != 0f)
		{
			return stepSpeed;
		}
		AnimationState animationState = base.animation["step"];
		if (animationState != null)
		{
			return animationState.length / 1.15f;
		}
		return 2f;
	}

	internal void step(int dir, int rotateDir, int animate, float stepSpeed)
	{
		string text = "step";
		float stepSpeed2 = GetStepSpeed(stepSpeed);
		float angle = ((rotateDir != -1) ? 30f : (-30f));
		AnimationState animationState = base.animation[text];
		if (animate == 1 && animationState != null)
		{
			animationState.speed = ((dir != 1) ? (-1f) : 1f);
			animationState.time = 0f;
			animationState.wrapMode = WrapMode.Loop;
			base.animation.Play(text);
		}
		_step = new StepData(base.transform, Globals.Battle.ArenaCenter.transform, angle, stepSpeed2, animate == 1);
	}

	private void BloodOnWeapon()
	{
	}

	private void RemoveBloodFromWeapon()
	{
	}

	private void BloodOnBody()
	{
	}

	private void RemoveBloodFromBody()
	{
	}

	public void AddBerserk(float time)
	{
		DoScale(1f, useInitScale: false);
		BloodOnWeapon();
		BloodOnBody();
		ScaleForTime scaleForTime = base.gameObject.AddComponent<ScaleForTime>();
		scaleForTime.ScaleTime = 0f;
		scaleForTime.Size = 1.2f;
	}

	public void AddBerserk()
	{
		BloodOnWeapon();
		BloodOnBody();
	}

	public void ScaleForTime(float size, float time)
	{
		DoScale(1f, useInitScale: true);
		ScaleForTime scaleForTime = base.gameObject.AddComponent<ScaleForTime>();
		scaleForTime.ScaleTime = time;
		scaleForTime.Size = size;
	}

	public void MoveTo(Vector3 offset, float time)
	{
		StartCoroutine(MoveToImpl(offset, time));
	}

	private IEnumerator MoveToImpl(Vector3 offset, float time)
	{
		if (!(time <= 0f))
		{
			Vector3 speed = offset / time;
			while (time > 0f)
			{
				yield return null;
				base.transform.position += Time.deltaTime * speed;
				time -= Time.deltaTime;
			}
		}
	}

	public void RemoveBerserk()
	{
		RemoveBloodFromBody();
		RemoveBloodFromWeapon();
		DoScale(1f, useInitScale: false);
	}

	internal void hide()
	{
		Utils.SetAllRenderersActive(base.gameObject, value: false);
	}

	protected string GetForwardAnimationName()
	{
		string result = "attack";
		if (base.animation["attack_uppercot"] != null && UnityEngine.Random.Range(0, 100) < 50)
		{
			result = "attack_uppercot";
		}
		return result;
	}

	internal static void DestroyPerson(GameObject enemy)
	{
		UnityEngine.Object.Destroy(enemy);
	}
}
