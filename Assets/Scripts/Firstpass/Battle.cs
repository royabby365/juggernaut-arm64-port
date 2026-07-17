using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Gesture;
using Gesture.CustomGestures;
using UnityEngine;
using Yarx;

public class Battle : MonoBehaviour
{
	internal enum StateE
	{
		WaitPlayerAttackEnd,
		WaitEnemyAttackEnd,
		WaitEnemyMiniGameEnd,
		PlayerTime,
		EnemyTime,
		FatalityMode,
		FatalityModeExecute,
		ShowFinishDialog,
		PreFatalityMode,
		ShowFightResult
	}

	internal enum FatalityStateE
	{
		None,
		Executed,
		Undone
	}

	internal GameObject FullSector;

	internal GameObject AttackSector;

	public GameObject BubbleBig;

	public GameObject BubbleBigSelect;

	public GameObject BubbleSmall;

	public GameObject BubbleSmallSelect;

	public GameObject MagicTrail;

	public GameObject PlayerCritFX;

	public GameObject PlayerHealFX1;

	public BattleGui BattleGui;

	public SelectGui SelectGui;

	public ViewSector ViewSector;

	public Camera BubbleCam;

	public GameObject PrefabViewSector;

	public GameObject ShadowPrefab;

	public GameObject MagicWeakBubblePregab;

	public GameObject FullScreenEffectPrefab;

	public GameObject FatalityBubblePrefab;

	public GameObject RageBubblePrefab;

	public GameObject RageBubblePrefabExpl;

	public GameObject EyeInBattle;

	public GameObject EyeInBattle2;

	public GameObject EyeExitFx;

	public StartBattleCamera StartBattleCamera;

	public BattleCameraController BattleCameraController;

	public GameObject TextPrefab;

	public GameObject PhrasePrefab;

	public GameObject BloodScreenEffectPrefab;

	public GameObject BloodScreenEffect1;

	public GameObject BloodScreenEffect2;

	public GameObject BloodScreenEffect3;

	public GameObject SlicePrefab;

	public AnimationCurve FightPhraseAlphaCurve;

	public GameObject FallingStarPrefab;

	internal GameObject _fallingStar;

	internal float _fallingStarPeriod;

	internal GameObject _landPrefab;

	internal string _landPrefabPath;

	internal GameObject _land;

	private CompositeDisposable _listeners;

	private bool _Pause;

	private bool _isNeedToSwitchToEnemyTime;

	internal static readonly string RageAbilityNameCrit = "crit";

	internal static readonly string RageAbilityNameBlock = "block";

	internal static readonly string RageAbilityNameStrong = "strong";

	private bool _waitSome1;

	private bool _second = true;

	internal int NextAttackIsWithForceElixir;

	private bool NextAttackIsRageCrit;

	private bool _attackUsed;

	private Recognizer _recognizer;

	private readonly List<Vector3> _positions = new List<Vector3>();

	public static ServerData.Location LocationToIncrement;

	private bool _invokeFatalitySphere;

	internal List<AttackE> Combo = new List<AttackE>();

	private int _ComboAttackDoneIndex;

	private float _lightOffK = 0.1f;

	private float _intensity = 1f;

	private TimePeriod _eyeTime;

	private Dictionary<MeshRenderer, Color> _lightRenderers = new Dictionary<MeshRenderer, Color>();

	internal HudMk1.GuiDesc _lastStartFightGui = new HudMk1.GuiDesc(GuiRoot.GuiType.None, null);

	private StateE _turnState = StateE.PlayerTime;

	internal static readonly string InBattleFxTag = "Inbattlefx";

	private bool _hasDeadPersons;

	private GameObject _arenaCenterCache;

	internal Bubbles _bubbles = new Bubbles();

	internal BubblesMagicGame2 _bubbles2 = new BubblesMagicGame2();

	private FatalityMode _fatalityMode;

	public int TurnsCount;

	public int CritsCount;

	public int FallingStarsClicked;

	internal FatalityStateE _fatalityExecuted;

	private Transform _viewSectorRootTransform;

	private ITouchscreen _touchscreen;

	private float _stopTurnTime = -1f;

	private bool _showEnemySector = true;

	private int _poisonDamage;

	internal float _lastSettedTimeRemainsTillTheEndOfState;

	private float _timeRemainsTillTheEndOfState;

	private int _lightMode;

	private GameObject _sceneObjectCache;

	internal Eye _eye;

	private bool _nextEnemyAttackBlocked;

	private bool _rageUsed;

	private int _startRagesCount;

	private Dictionary<ServerData.Skill.TypeE, int> _startSpellsUsedCount = new Dictionary<ServerData.Skill.TypeE, int>();

	private int _startManaCount;

	internal bool _invokeEndMiniGame;

	private GameObject _fireScreenFx;

	private GameObject _iceScreenFx;

	private GameObject _darkScreenFx;

	private GameObject _lightingScreenFx;

	internal bool IsFinished => _hasDeadPersons;

	internal bool IsOneDeadAndNoAnimations => IsInBattleMode && Globals.Player != null && Globals.Enemy != null && (Globals.Player.IsDeadAndNoAnimation || Globals.Enemy.IsDeadAndNoAnimation);

	internal GameObject ArenaCenter
	{
		get
		{
			if (_arenaCenterCache == null)
			{
				_arenaCenterCache = GameObject.Find("arena_center");
			}
			return _arenaCenterCache;
		}
	}

	internal bool IsSelectionMode
	{
		get
		{
			if (BattleGui == null)
			{
				BattleGui = GetComponent<BattleGui>();
			}
			return BattleGui != null && !BattleGui.enabled;
		}
	}

	internal bool IsInBattleMode
	{
		get
		{
			if (BattleGui == null)
			{
				BattleGui = GetComponent<BattleGui>();
			}
			return BattleGui != null && BattleGui.enabled;
		}
	}

	public bool Pause
	{
		get
		{
			return _Pause;
		}
		set
		{
			_Pause = value;
			Utils.Log("****PAUSE", value, "_isNeedToSwitchToEnemyTime", _isNeedToSwitchToEnemyTime);
			if (!_Pause && _isNeedToSwitchToEnemyTime)
			{
				State = StateE.EnemyTime;
				_isNeedToSwitchToEnemyTime = false;
			}
		}
	}

	internal bool IsInMagicMode => CurrentSpellLevel != -1 && _stopTurnTime > 0f;

	internal int CurrentSpellLevel { get; private set; }

	internal ServerData.Spell CurrentSpell { get; private set; }

	internal bool MagicAllowed => Globals.Player.Mana >= 10;

	internal IPadTouchscreen IPadTouchscreen { get; private set; }

	internal StateE State
	{
		get
		{
			return _turnState;
		}
		set
		{
			if (_turnState == value)
			{
				return;
			}
			if (value == StateE.PreFatalityMode)
			{
				ResumeTurnTime("StateE.PreFatalityMode");
			}
			if (value != StateE.PlayerTime)
			{
				Messenger.Invoke(Globals.MsgGuiBattle_Timer, 0f);
			}
			_attackUsed = false;
			if (value == StateE.PlayerTime)
			{
				_rageUsed = false;
			}
			if (CurrentSpellLevel >= 0)
			{
				CurrentSpellLevel = -1;
			}
			if (value == StateE.WaitPlayerAttackEnd && HudMk1.Instance != null)
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.EnemyTurn);
			}
			_turnState = value;
			if (BattleGui != null)
			{
				if (value != StateE.PlayerTime)
				{
					if (!_bubbles.HasKilledBubbles)
					{
						Messenger.Invoke(Globals.MsgGuiBattle_SetMagicBarVisible, arg1: true);
					}
				}
				else
				{
					Messenger.Invoke(Globals.MsgGuiBattle_SetMagicBarVisible, arg1: true);
				}
			}
			if (value != StateE.FatalityMode && value != StateE.FatalityModeExecute && _fatalityMode != null)
			{
				_fatalityMode.Destroy(ref _fatalityMode);
			}
			if (value == StateE.EnemyTime)
			{
				Messenger.Invoke(Globals.MsgMagicModeDisabled);
				SingletonT<ServerData>.I.BattleNewEnemyTurn();
			}
			if (value == StateE.FatalityMode)
			{
				TimeRemainsTillTheEndOfState = SingletonT<ServerData>.I.GameSettings.FatalityTime;
			}
			if (value == StateE.FatalityModeExecute && _fatalityExecuted == FatalityStateE.None)
			{
				_fatalityExecuted = FatalityStateE.Executed;
			}
			if (value == StateE.PlayerTime)
			{
				TurnsCount++;
				SingletonT<ServerData>.I.BattleNewPlayerTurn();
			}
			IPadTouchscreen.TrailEnabled = value == StateE.PlayerTime || value == StateE.EnemyTime || value == StateE.WaitEnemyAttackEnd || value == StateE.WaitEnemyMiniGameEnd || value == StateE.FatalityMode || value == StateE.WaitPlayerAttackEnd;
			if (value == StateE.ShowFinishDialog)
			{
				if (!Globals.Player.IsDead)
				{
					BattleCameraController.GoToWinState();
					SingletonT<SoundManager>.I.PlayGlobalSound((_fatalityExecuted != FatalityStateE.Executed) ? "Jug_win" : "Jug_win_fatality", fadeMusic: true);
					Globals.Enemy.Health = 0;
					Globals.Player.WaitForBattleWinAnimation(delegate
					{
						Messenger.Invoke(Globals.MsgFightShowEndDialog);
					});
				}
				else
				{
					SingletonT<SoundManager>.I.PlayGlobalSound("Jug_fail", fadeMusic: true);
					Messenger.Invoke(Globals.MsgFightShowEndDialog);
				}
				IPadTouchscreen.TrailEnabled = false;
			}
			if (value == StateE.PreFatalityMode)
			{
				if (!Globals.Player.IsDead)
				{
					Globals.Enemy.Health = 0;
					Globals.Battle.State = StateE.FatalityMode;
					Messenger.Invoke(Globals.MsgFatalityStarted);
					_fatalityMode = new FatalityMode(this, SingletonT<ServerData>.I.GameSettings.SlicesForFatality, SingletonT<ServerData>.I.GameSettings.FatalityTime, AttackE.Forward, ReactE.Death, 0);
				}
				else
				{
					Messenger.Invoke(Globals.MsgFightShowEndDialog);
				}
				IPadTouchscreen.TrailEnabled = false;
			}
			SetSectorRendererActive(_showEnemySector && value == StateE.PlayerTime && !Globals.Enemy.IsDead);
			Messenger.Invoke(Globals.MsgBattleStateChanged, _turnState);
		}
	}

	internal int ComboAttackDoneIndex
	{
		get
		{
			return _ComboAttackDoneIndex;
		}
		set
		{
			_ComboAttackDoneIndex = value;
			Messenger.Invoke(Globals.MsgGuiBattle_ComboAllowed, IsComboAllowed);
		}
	}

	internal bool IsComboAllowed => Combo.Count <= ComboAttackDoneIndex || Globals.DebugFastCombo;

	internal float TimeRemainsTillTheEndOfState
	{
		get
		{
			return _timeRemainsTillTheEndOfState;
		}
		set
		{
			_timeRemainsTillTheEndOfState = value;
			_lastSettedTimeRemainsTillTheEndOfState = value;
		}
	}

	[method: MethodImpl((MethodImplOptions)32)]
	internal event Action<bool> CameraMoveModeEnabled;

	internal void FireEventCameraMoveModeEnabled(bool value)
	{
		if (this.CameraMoveModeEnabled != null)
		{
			this.CameraMoveModeEnabled(value);
		}
	}

	public void FreeCamera()
	{
		Camera.main.transform.parent = null;
	}

	private void Awake()
	{
		Globals.Battle = this;
	}

	internal void DoStart(AreaData.MobData mob)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		FullSector = (GameObject)UnityEngine.Object.Instantiate(PrefabViewSector);
		ViewSector component = FullSector.GetComponent<ViewSector>();
		component.SpeedInAngles = 20f;
		ViewSector = FullSector.GetComponentInChildren<ViewSector>();
		_viewSectorRootTransform = ViewSector.transform.root.transform;
		TimeRemainsTillTheEndOfState = SingletonT<ServerData>.I.GameSettings.TurnTime;
		_touchscreen = Globals.CreateTouchscreen();
		IPadTouchscreen = new IPadTouchscreen(_touchscreen, this, () => MagicTrail);
		_touchscreen.OnTouchMove += Touchscreen_OnTouchMove;
		_touchscreen.OnTouchEnd += Touchscreen_OnTouchEnd;
		IPadTouchscreen.OnAttackForward += delegate(float clockAngle)
		{
			if (State == StateE.PlayerTime && !Globals.ForceDontProcessSliceAttack && Globals.CanAttack(AttackE.Forward))
			{
				if (HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.CastMagic)
				{
					GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(SlicePrefab);
					Slice componentInChildren = gameObject2.GetComponentInChildren<Slice>();
					componentInChildren.SetSlice(clockAngle);
				}
				ExecutePlayerAttack(AttackE.Forward);
			}
		};
		IPadTouchscreen.OnAttackLeft += delegate(float clockAngle)
		{
			if (State == StateE.PlayerTime && !Globals.ForceDontProcessSliceAttack && Globals.CanAttack(AttackE.Left))
			{
				if (HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.CastMagic)
				{
					GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(SlicePrefab);
					Slice componentInChildren = gameObject2.GetComponentInChildren<Slice>();
					componentInChildren.SetSlice(clockAngle);
				}
				ExecutePlayerAttack(AttackE.Left);
			}
		};
		IPadTouchscreen.OnAttackRight += delegate(float clockAngle)
		{
			if (State == StateE.PlayerTime && !Globals.ForceDontProcessSliceAttack && Globals.CanAttack(AttackE.Right))
			{
				if (HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.CastMagic)
				{
					GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(SlicePrefab);
					Slice componentInChildren = gameObject2.GetComponentInChildren<Slice>();
					componentInChildren.SetSlice(clockAngle);
				}
				ExecutePlayerAttack(AttackE.Right);
			}
		};
		InitTouchscreenCasts();
		if (_landPrefab == null)
		{
			_land = GameObject.Find(Globals.LocationGameObjectSceneGeomName);
		}
		if (_landPrefab != null || (_landPrefab == null && _land == null))
		{
			Invs.Inv(_landPrefab != null, "Land != null");
			_land = (GameObject)UnityEngine.Object.Instantiate(_landPrefab, Vector3.zero, Quaternion.identity);
			_land.AddComponent<FromAssetBundle>().Path = _landPrefabPath;
			_land.name = Globals.LocationGameObjectSceneGeomName;
		}
		_land.SetActiveRecursivelyMk1(setActive: true);
		if (!Globals.DebugDontLoadPlayer)
		{
			InitPlayer((SingletonT<ServerData>.I.PlayerServerPersData == null) ? "1" : SingletonT<ServerData>.I.PlayerServerPersData.ModelId);
			if (Globals.Player != null)
			{
				Globals.Player.SetupOnScene(this);
			}
		}
		SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.01f, delegate
		{
			if (SelectGui != null)
			{
				ReturnToArea(playMusic: false, showSelectGui: true);
			}
			SingletonT<ServerData>.I.BattleStartTime = Time.time;
		});
		if (!Globals.DebugDontLoadPlayer && Globals.Player != null)
		{
			ArmorData[] componentsInChildren = Globals.Player.GetComponentsInChildren<ArmorData>();
			foreach (ArmorData component2 in componentsInChildren)
			{
				Utils.SetAllRenderersActive(component2, value: true);
			}
		}
		Globals.PlayBattleMusic(Globals.MainMenu, wasSuccess: false);
		if (BloodScreenEffectPrefab != null)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BloodScreenEffectPrefab);
			InitBloodScreenEffect(gameObject, 1, ref BloodScreenEffect1);
			InitBloodScreenEffect(gameObject, 2, ref BloodScreenEffect2);
			InitBloodScreenEffect(gameObject, 3, ref BloodScreenEffect3);
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private void InitBloodScreenEffect(GameObject rootEffect, int index, ref GameObject bloodScreenEffect)
	{
		Transform transform = rootEffect.transform.FindChildByName("scr_blood_iPAD_0" + index);
		Transform transform2 = rootEffect.transform.FindChildByName("scr_blood_iPhone_0" + index);
		float z = 12.47f;
		bloodScreenEffect = transform.gameObject;
		transform2.gameObject.SetActiveRecursivelyMk1(setActive: false);
		Transform parent = GameObject.Find(Globals.LocationGameObjectBattleCamera).transform;
		bloodScreenEffect.transform.parent = parent;
		bloodScreenEffect.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		bloodScreenEffect.transform.localPosition = new Vector3(0f, 0f, z);
		Vector3 localScale = bloodScreenEffect.transform.localScale;
		localScale.x = (float)Screen.width / (float)Screen.height / 1.3333f;
		bloodScreenEffect.transform.localScale = localScale;
		bloodScreenEffect.SetActiveRecursivelyMk1(setActive: false);
	}

	private void OnMsgGuiBattle_CastGesture(string[] gestures)
	{
		if (CurrentSpellLevel < 0)
		{
			return;
		}
		foreach (string magic in gestures)
		{
			if (TryGestureCast(magic))
			{
				break;
			}
		}
	}

	private void Touchscreen_OnTouchEnd(Vector2 startPos, Vector2 pos, float time)
	{
	}

	private void Touchscreen_OnTouchMove(Vector2 offset, Vector2 pos)
	{
		if (CurrentSpellLevel >= 0)
		{
			_positions.Add(pos);
		}
	}

	private IEnumerator SetPrefatalityMode()
	{
		while ((Globals.Enemy != null && Globals.Enemy.animation.isPlaying) || Pause)
		{
			yield return null;
		}
		if (!(Globals.Enemy != null))
		{
			yield break;
		}
		if (Globals.DebugFatality || (Globals.UseFatalityWhiteSpheresMode && SingletonT<ServerData>.I.PlayerParams.ResurrectionSpheresCount > 0))
		{
			SingletonT<ServerData>.I.PlayerParams.ResurrectionSpheresCount = 0;
			Globals.Player._ignoreIdle = true;
			Globals.Player.PlayScenario(Globals.Battle, "botles_ressurect2", AttackE.None, ReactE.None, DamageTypeE.Timeout, 0, delegate
			{
				SingletonT<SoundManager>.I.PlayGlobalSound("click_resurrection_use");
				Globals.Player._ignoreIdle = false;
				AnimationState deathAnim = Globals.Enemy.animation["death"];
				deathAnim.speed = -1f;
				deathAnim.time = deathAnim.length;
				Globals.Enemy.animation.Play("death");
				SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(deathAnim.length, delegate
				{
					deathAnim.speed = 1f;
					State = StateE.PreFatalityMode;
					Globals.Enemy.ForceIdleForFatality();
				});
			});
		}
		else
		{
			Globals.Player.MakeVictoryIdle();
			State = StateE.ShowFinishDialog;
		}
	}

	public void EnterMode()
	{
		Pause = false;
		if (SelectGui != null)
		{
			SelectGui.enabled = true;
		}
		if (Globals.Enemy != null)
		{
			Globals.Enemy.gameObject.SetActiveRecursivelyMk1(setActive: true);
			PersonArmor component = Globals.Enemy.GetComponent<PersonArmor>();
			if (component != null)
			{
				component.HideBodyParts();
			}
		}
	}

	private void OnMsgPlayerAttackFinished(Player _)
	{
		if (State != StateE.FatalityMode && State != StateE.FatalityModeExecute && State != StateE.ShowFinishDialog && !Globals.Enemy.IsDead)
		{
			TimeRemainsTillTheEndOfState = SingletonT<ServerData>.I.GameSettings.TurnTime;
		}
		if (State == StateE.EnemyTime && Globals.Enemy.IsDead)
		{
			StartCoroutine(SetPrefatalityMode());
		}
		if (State == StateE.PlayerTime || State == StateE.WaitEnemyAttackEnd || State == StateE.WaitPlayerAttackEnd)
		{
			if (Globals.Enemy.IsDead)
			{
				StartCoroutine(SetPrefatalityMode());
			}
			else if (!Pause)
			{
				State = StateE.EnemyTime;
			}
			else
			{
				_isNeedToSwitchToEnemyTime = true;
			}
		}
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType prev, GuiRoot.GuiType current)
	{
		if (BattleCameraController == null || prev == current)
		{
			return;
		}
		switch (current)
		{
		case GuiRoot.GuiType.BagItems:
			if (prev != GuiRoot.GuiType.BagStats)
			{
				BattleCameraController.GoToBagState();
			}
			break;
		case GuiRoot.GuiType.BagStats:
			if (prev != GuiRoot.GuiType.BagItems)
			{
				BattleCameraController.GoToBagState();
			}
			break;
		case GuiRoot.GuiType.Compare:
			BattleCameraController.GoToBagState();
			break;
		case GuiRoot.GuiType.None:
		case GuiRoot.GuiType.BattleHud:
		case GuiRoot.GuiType.CastMagic:
		case GuiRoot.GuiType.StrongMagicMiniGame:
		case GuiRoot.GuiType.WeakMagicMiniGame:
		case GuiRoot.GuiType.EnemyTurn:
		case GuiRoot.GuiType.EnemyDefeated:
			BattleCameraController.GoToFightState();
			break;
		case GuiRoot.GuiType.ChapterInfo:
		case GuiRoot.GuiType.BattleResults:
			if (current == GuiRoot.GuiType.BattleResults && Globals.Enemy != null)
			{
				Globals.Enemy._destroyed = true;
			}
			BattleCameraController.GoToWinState();
			if (_fallingStar != null)
			{
				UnityEngine.Object.Destroy(_fallingStar);
			}
			Utils.DestroyGameObject(ref _eye);
			break;
		case GuiRoot.GuiType.Execution:
			if (_fallingStar != null)
			{
				UnityEngine.Object.Destroy(_fallingStar);
			}
			Utils.DestroyGameObject(ref _eye);
			BattleCameraController.GoToPreFatalityState();
			break;
		default:
			BattleCameraController.GoToStartState();
			break;
		}
	}

	private void OnMsgGuiSwitchToPre(GuiRoot.GuiType prev, GuiRoot.GuiType current)
	{
		if (_second && _waitSome1)
		{
			_waitSome1 = false;
			ReturnToArea(playMusic: true, showSelectGui: true);
		}
	}

	private void OnMsgRageSphereUse(int mode)
	{
		if (State != StateE.PlayerTime || _rageUsed)
		{
			return;
		}
		ServerData.PlayerParamsData playerParams = SingletonT<ServerData>.I.PlayerParams;
		int rageSpheresCount = playerParams.RageSpheresCount;
		if (rageSpheresCount == 0)
		{
			return;
		}
		if (mode == 1 && rageSpheresCount > 0)
		{
			if (!Globals.DebugDontDecRageBubble)
			{
				playerParams.RageSpheresCount--;
			}
			Metrics.OnPlayerRageBlock();
			_nextEnemyAttackBlocked = true;
			_rageUsed = true;
			Messenger.Invoke(Globals.MsgGuiBattle_FlashPhrase, ServerData.PhrasesE.InBattleNextBlock);
			Globals.Player.PlayScenario(this, "magic_baf_block", AttackE.None, ReactE.None, DamageTypeE.Timeout, 0, delegate
			{
			});
			Messenger.Invoke(Globals.MsgPlayerUseRage, RageAbilityNameBlock);
		}
		else if (mode == 2 && rageSpheresCount > 3)
		{
			if (!Globals.DebugDontDecRageBubble)
			{
				playerParams.RageSpheresCount -= 4;
			}
			Metrics.OnPlayerRageCrit();
			NextAttackIsRageCrit = true;
			_rageUsed = true;
			Messenger.Invoke(Globals.MsgGuiBattle_FlashPhrase, ServerData.PhrasesE.InBattleNextCrit);
			Globals.Player.PlayScenario(this, "fury", AttackE.None, ReactE.None, DamageTypeE.Timeout, 0, delegate
			{
			});
			Messenger.Invoke(Globals.MsgPlayerUseRage, RageAbilityNameCrit);
		}
		else if (mode == 3 && rageSpheresCount > 9 && !SingletonT<ServerData>.I._inBerserkMode)
		{
			if (!Globals.DebugDontDecRageBubble)
			{
				playerParams.RageSpheresCount -= 10;
			}
			Metrics.OnPlayerRageBerserk();
			Messenger.Invoke(Globals.MsgGuiBattle_FlashPhrase, ServerData.PhrasesE.InBattleBerserk);
			_rageUsed = true;
			Globals.Player.PlayScenario(this, "berserk3", AttackE.None, ReactE.None, DamageTypeE.Timeout, 0, delegate
			{
				SingletonT<ServerData>.I._inBerserkMode = true;
				Globals.Player.AddBerserk();
				if (SingletonT<ServerData>.I.GameSettings.BerserkMaxHealth > 0)
				{
					int num = Globals.Player.MaxHealth * SingletonT<ServerData>.I.GameSettings.BerserkMaxHealth / 100;
					Globals.Player.MaxHealth += num;
					Globals.Player.Health += num;
				}
			});
			Messenger.Invoke(Globals.MsgPlayerUseRage, RageAbilityNameStrong);
		}
		SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(1.5f, delegate
		{
			Messenger.Invoke(Globals.MsgGuiBattle_HideText);
		});
	}

	private void OnMsg_MagicGame2_Show(int count)
	{
		if (!(HudMk1.Instance == null))
		{
			_bubbles2.SpawnBubbles(count);
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.WeakMagicMiniGame);
		}
	}

	private void MsgPersonDamadgedHandler(Person person, int attackType, ReactE reaction, AttackE attack)
	{
		if (!person.Equals(Globals.Enemy))
		{
			ServerData.Settings gameSettings = SingletonT<ServerData>.I.GameSettings;
			if (Globals.ForceSpawnRageBalls)
			{
				_bubbles.SpawnRageBubble(this, Globals.Player);
			}
			else if (Globals.DebugAlwaysRageBubble)
			{
				_bubbles.SpawnRageBubble(this, Globals.Player);
				if (SingletonT<ServerData>.I.PlayerInBattleParams.BonusRage > 0)
				{
					int nRand = Utils.GetNRand(SingletonT<ServerData>.I.PlayerInBattleParams.BonusRage, SingletonT<ServerData>.I.GameSettings.rageBonusProb);
					for (int i = 0; i < nRand; i++)
					{
						_bubbles.SpawnRageBubble(this, Globals.Player);
					}
				}
			}
			else
			{
				if (!SingletonT<ServerData>.I.IsRageOpened || !reaction.HasFlag(ReactE.Damage) || reaction.HasFlag(ReactE.NoBubbles) || UnityEngine.Random.Range(0, 100) >= ((!reaction.HasFlag(ReactE.Critical)) ? gameSettings.RageShereProbSimple : gameSettings.RageShereProbCrit))
				{
					return;
				}
				_bubbles.SpawnRageBubble(this, Globals.Player);
				if (SingletonT<ServerData>.I.PlayerInBattleParams.BonusRage > 0)
				{
					int nRand2 = Utils.GetNRand(SingletonT<ServerData>.I.PlayerInBattleParams.BonusRage, SingletonT<ServerData>.I.GameSettings.rageBonusProb);
					for (int j = 0; j < nRand2; j++)
					{
						_bubbles.SpawnRageBubble(this, Globals.Player);
					}
				}
			}
		}
		else if (!reaction.HasFlag(ReactE.NoBubbles) && !reaction.HasFlag(ReactE.Death) && reaction.HasFlag(ReactE.Damage))
		{
			_bubbles.SpawnBubbles(this, reaction.HasFlag(ReactE.Critical), reaction.HasFlag(ReactE.FromRages), attack == AttackE.Combo);
		}
	}

	private void GoToLocationFromFightResult()
	{
		LeaveMode();
		Globals.MainMenu._gotoFromBattle = MainMenu.DestroyAllE.GoToLocation;
		Globals.MainMenu._destroyAll = MainMenu.DestroyAllE.GoToLocation;
	}

	private void GoToBagFromFightResult()
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (!AreaData.Current.Location.IsZachistkaOpened)
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BagItems);
			return;
		}
		if (_second)
		{
			Utils.Log("AAAAAA", Globals.MainMenu._lastLocationOrMapOrZachistka);
			_waitSome1 = true;
		}
		else
		{
			ReturnToArea(playMusic: true, showSelectGui: true);
			InitAreaView(SingletonT<ServerData>.I.GetLocationProgress(AreaData.Current.Location));
		}
		HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BagItems);
	}

	internal void ContinueFromFightResult()
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (!AreaData.Current.Location.IsZachistkaOpened)
		{
			if (Globals.MainMenu._lastMapOrZachistkaOrFight == GuiRoot.GuiType.MainMap)
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.MainMap);
			}
			else
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Location);
			}
		}
		else if (_second)
		{
			Utils.Log("BBBBBB", Globals.MainMenu._lastLocationOrMapOrZachistka);
			HudMk1.Instance.ChangeGuiTo(Globals.MainMenu._lastLocationOrMapOrZachistka);
			_waitSome1 = true;
		}
		else
		{
			ReturnToArea(playMusic: true, showSelectGui: true);
			InitAreaView(SingletonT<ServerData>.I.GetLocationProgress(AreaData.Current.Location));
		}
	}

	internal void ReturnToMainMap()
	{
		LeaveMode();
		Globals.MainMenu._destroyAll = MainMenu.DestroyAllE.GoToMainMap;
	}

	private void OnEnable()
	{
		_listeners = new CompositeDisposable();
		_listeners.Add(Messenger<Player>.AddListener(Globals.MsgPlayerAttackFinished, OnMsgPlayerAttackFinished));
		_listeners.Add(Messenger.AddListener(Globals.MsgFightEyeDieByClick, delegate
		{
			_eyeTime = null;
		}));
		_listeners.Add(Messenger.AddListener(Globals.MsgFightResult_Continue, ContinueFromFightResult));
		_listeners.Add(Messenger.AddListener(Globals.MsgFightResult_GoToBag, GoToBagFromFightResult));
		_listeners.Add(Messenger<Person>.AddListener(Globals.MsgPersonDie, MsgPersonDieHandler));
		_listeners.Add(Messenger<Person>.AddListener(Globals.MsgPersonManaChanged, MsgPersonManaChangedHandler));
		_listeners.Add(Messenger<Person, int, ReactE, AttackE>.AddListener(Globals.MsgPersonDamaged, MsgPersonDamadgedHandler));
		_listeners.Add(Messenger<Person>.AddListener(Globals.MsgPersonAttackFinished, MsgPersonAttackFinishedHandler));
		_listeners.Add(Messenger.AddListener(Globals.MsgElixirApplyPoisonOnEnemy, OnMsgElixirApplyPoisonOnEnemy));
		_listeners.Add(Messenger<int>.AddListener(Globals.Msg_MagicGame2_Show, OnMsg_MagicGame2_Show));
		_listeners.Add(Messenger<string[]>.AddListener(Globals.MsgGuiBattle_CastGesture, OnMsgGuiBattle_CastGesture));
		_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnMsgGuiSwitchToPre));
		_listeners.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
		_listeners.Add(Messenger<int>.AddListener(Globals.MsgRageSphereUse, OnMsgRageSphereUse));
		_listeners.Add(Messenger<Person>.AddListener(Globals.MsgBattleReactFx, delegate(Person _)
		{
			if (_invokeFatalitySphere && _ == Globals.Enemy && !Globals.Enemy.IsDead)
			{
				_invokeFatalitySphere = false;
				BubbleFatality.Create();
			}
		}));
		_listeners.Add(Messenger<bool>.AddListener(Globals.MsgFightPause, delegate(bool pause)
		{
			Utils.Log("BATTLE.PAUSE", pause, _eyeTime != null);
			if (_eyeTime != null)
			{
				_eyeTime.Pause = pause;
			}
			if (pause)
			{
				StopTurnTime();
			}
			else
			{
				ResumeTurnTime("MsgFightPause");
			}
			Pause = pause;
		}));
		_listeners.Add(Messenger<int, int>.AddListener(Globals.MsgSpawnBubblesFromPlayer, delegate(int count, int mana)
		{
			_bubbles.SpawnBubblesFromPlayer(this, Globals.Player, count, mana);
		}));
		_listeners.Add(Messenger<int>.AddListener(Globals.MsgFightViewSectorClicked, delegate(int index)
		{
			if (!Pause)
			{
				switch (index)
				{
				case 3:
					if (Globals.CanAttack(AttackE.Left))
					{
						ExecutePlayerAttack(AttackE.Left);
					}
					break;
				case 2:
					if (Globals.CanAttack(AttackE.Forward))
					{
						ExecutePlayerAttack(AttackE.Forward);
					}
					break;
				case 1:
					if (Globals.CanAttack(AttackE.Right))
					{
						ExecutePlayerAttack(AttackE.Right);
					}
					break;
				}
			}
		}));
		_listeners.Add(Messenger<MagicTypeE>.AddListener(Globals.MsgPlayerCastSpell, delegate(MagicTypeE spell)
		{
			if (!Globals.Enemy.ServerBotInfo.HasMagicImmunity(spell))
			{
				CritsCount++;
			}
		}));
		_listeners.Add(Messenger.AddListener(Globals.MsgFallingStarClicked, delegate
		{
			FallingStarsClicked++;
		}));
		_fallingStarPeriod = ((!Globals.DebugFrequentFallingStars) ? SingletonT<ServerData>.I.GameSettings.FallingStarCooldown : 10);
	}

	private void OnDisable()
	{
		if (_touchscreen != null)
		{
			_touchscreen.OnTouchMove -= Touchscreen_OnTouchMove;
			_touchscreen.OnTouchEnd -= Touchscreen_OnTouchEnd;
			_touchscreen = null;
		}
		Utils.Dispose(ref _listeners);
		_sceneObjectCache = null;
		_lightRenderers.Clear();
		_positions.Clear();
		_landPrefab = null;
		_arenaCenterCache = null;
		_viewSectorRootTransform = null;
	}

	public void LeaveMode()
	{
		Utils.Log("%%%%%%%%%%%%BATTLE LEAVE MODE");
		IPadTouchscreen.Restart();
		_touchscreen.OnTouchMove -= Touchscreen_OnTouchMove;
		_touchscreen.OnTouchEnd -= Touchscreen_OnTouchEnd;
		Globals.Player.Restart();
		_sceneObjectCache = null;
		_lightRenderers.Clear();
		StopAllCoroutines();
		_positions.Clear();
		BattleGui.enabled = false;
		if (SelectGui != null)
		{
			SelectGui.enabled = false;
		}
		GameObject land = _land;
		_land = null;
		UnityEngine.Object.Destroy(land);
		Utils.SetAllRenderersActive(Globals.Player.gameObject, value: false);
		DestroyEnemy();
		land = FullSector.gameObject;
		FullSector = null;
		UnityEngine.Object.Destroy(land);
		UnityEngine.Object.Destroy(Globals.Player);
		_landPrefab = null;
		_arenaCenterCache = null;
		_viewSectorRootTransform = null;
		Globals.Battle = null;
		Globals.InFight = false;
	}

	private void DestroyEnemy()
	{
		if (!(Globals.Enemy == null))
		{
			Globals.Enemy.Reset();
			SingletonT<SoundManager>.I.UnloadSounds(Globals.Enemy.ModelName);
			Globals.MainMenu.DestroyEnemy();
		}
	}

	private void Battle_TurnStateChanged(StateE state)
	{
		SetSectorRendererActive(_showEnemySector && state == StateE.PlayerTime);
	}

	private void SetSectorRendererActive(bool value)
	{
		if (FullSector != null)
		{
			Renderer[] componentsInChildren = FullSector.GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer in componentsInChildren)
			{
				GetComponent<Renderer>()enabled = value;
			}
		}
	}

	private void InitSectors()
	{
		if (FullSector != null)
		{
			FullSector.transform.position = Utils.XZ(Globals.Enemy.transform.position, 0.06f);
		}
	}

	private void MsgPersonManaChangedHandler(Person person)
	{
	}

	private void InitPlayer(string prototypeId)
	{
		GameObject playerGameObject = Globals.PlayerGameObject;
		if (playerGameObject == null)
		{
			return;
		}
		playerGameObject.animation.Rewind();
		Player player = Utils.DestroyComponentThenAddNew<Player>(playerGameObject);
		player.ModelName = prototypeId;
		Globals.Player = player;
		playerGameObject.SetActiveRecursivelyMk1(setActive: true);
		player.InitializeIdleAnimations();
		PersonArmor component = player.GetComponent<PersonArmor>();
		component.Weapon = null;
		ServerData.PersData playerServerPersData = SingletonT<ServerData>.I.PlayerServerPersData;
		if (playerServerPersData != null && playerServerPersData.SelectHair != null)
		{
			component.LoadHair(playerServerPersData.ModelId, playerServerPersData.SelectHair, playerServerPersData.SelectHairColor, delegate
			{
			});
		}
		ArmoronPersData[] componentsInChildren = Globals.PlayerGameObject.GetComponentsInChildren<ArmoronPersData>();
		foreach (ArmoronPersData armoronPersData in componentsInChildren)
		{
			if (armoronPersData.Slot == ServerData.Slot.TypeE.Weapon)
			{
				component.Weapon = armoronPersData.gameObject;
				break;
			}
		}
	}

	public void SetupEnemyOnScene()
	{
		if (IsSelectionMode)
		{
			Globals.Enemy.SetupEnemyOnSceneInSelectionMode();
			return;
		}
		Globals.Enemy.ViewSector = ViewSector;
		Globals.Enemy.SetupEnemyOnSceneInBattleMode();
	}

	private void ChangeSkin(GameObject person, string partName, Material mat)
	{
		if (mat == null)
		{
			return;
		}
		Transform transform = person.transform.FindChildByName(partName, includeInactive: false);
		if (transform == null)
		{
			return;
		}
		Renderer component = transform.GetComponent<Renderer>();
		if (!(component == null))
		{
			if (component.materials.Length > 1)
			{
				component.materials = new Material[2]
				{
					component.materials[0],
					mat
				};
				component.materials[1].color = new Color(0.5f, 0.5f, 0.5f);
				component.materials[0].color = new Color(0.5f, 0.5f, 0.5f);
			}
			else if (transform.GetComponent<ArmorData>() == null)
			{
				component.material = mat;
			}
		}
	}

	private void SetSkin(ServerData.BotInfo serverBotInfo, GameObject person)
	{
		if (!string.IsNullOrEmpty(serverBotInfo.skinColor))
		{
			SkinsData component = person.GetComponent<SkinsData>();
			if (component != null && component.Name == serverBotInfo.skinColor)
			{
				ChangeSkin(person, "torso", component.Torso);
				ChangeSkin(person, "pelvis", component.Pelvis);
				ChangeSkin(person, "boots", component.Legs);
				ChangeSkin(person, "hand_l", component.LeftHand);
				ChangeSkin(person, "hand_r", component.RightHand);
				ChangeSkin(person, "head", component.Head);
			}
		}
	}

	private void InitEnemy(ServerData.BotInfo serverBotInfo, ServerData.BotLevel levelData, ActionD<Enemy> onLoad)
	{
		SingletonT<ResourcesManager>.I.CreatePerson(this, serverBotInfo.Model, delegate(string _, GameObject EnemyGameObject)
		{
			EnemyGameObject.name = Globals.EnemyName(serverBotInfo.Id.ToString());
			Enemy enemy = (Enemy)EnemyGameObject.AddComponent(typeof(Enemy));
			enemy.SetServerData(serverBotInfo, levelData);
			Globals.Enemy = enemy;
			enemy.SetupOnScene(this);
			Messenger<int, int>.Invoke(Globals.MsgEnemyHealthChanged, enemy.MaxHealth, enemy.MaxHealth);
			SingletonT<SoundManager>.I.CacheSounds(this, enemy);
			SingletonT<ResourcesManager>.I.UnloadUnusedAssets(this, delegate
			{
				if (!string.IsNullOrEmpty(serverBotInfo.Armor))
				{
					InitEnemyArmor(serverBotInfo, enemy, serverBotInfo.Armor, delegate
					{
						SetSkin(serverBotInfo, EnemyGameObject);
						enemy.InitializeIdleAnimations();
						onLoad(enemy);
					});
				}
				else
				{
					SetSkin(serverBotInfo, EnemyGameObject);
					enemy.InitializeIdleAnimations();
					onLoad(enemy);
				}
			});
		});
	}

	private bool AddArmor(string s, string text, ServerData.Slot.TypeE slot, List<PersonArmor.ArmorLoadEntry> list)
	{
		if (s.StartsWith(text))
		{
			list.Add(new PersonArmor.ArmorLoadEntry
			{
				SetName = s.Remove(0, text.Length),
				Slot = slot
			});
			return true;
		}
		return false;
	}

	private void InitEnemyArmor(ServerData.BotInfo serverBotInfo, Enemy enemy, string armor, ActionD<Enemy> onLoad)
	{
		PersonArmor personArmor = Utils.DestroyComponentThenAddNew<PersonArmor>(enemy.gameObject);
		List<PersonArmor.ArmorLoadEntry> list = new List<PersonArmor.ArmorLoadEntry>();
		List<string> list2 = new List<string>(armor.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries));
		Utils.Log("InitEnemyArmor", armor, Utils.ParamsToString(list2.ToArray()));
		int num = list2.FindIndex((string _) => _.StartsWith("set="));
		if (num >= 0)
		{
			string text = list2[num].Remove(0, 4);
			list2.RemoveAt(num);
			list2.Add("belt=" + text);
			list2.Add("pelvis=" + text);
			list2.Add("boots=" + text);
			list2.Add("hand_l=" + text);
			list2.Add("hand_r=" + text);
			list2.Add("torso=" + text);
			list2.Add("shoulderstrap=" + text);
			list2.Add("helm=" + text);
		}
		int skinColor = -1;
		foreach (string item in list2)
		{
			if (!AddArmor(item, "belt=", ServerData.Slot.TypeE.Belt, list) && !AddArmor(item, "pelvis=", ServerData.Slot.TypeE.Pelvis, list) && !AddArmor(item, "boots=", ServerData.Slot.TypeE.Boots, list) && !AddArmor(item, "helm=", ServerData.Slot.TypeE.Helm, list) && !AddArmor(item, "hand_l=", ServerData.Slot.TypeE.HandLeft, list) && !AddArmor(item, "hand_r=", ServerData.Slot.TypeE.HandRight, list) && !AddArmor(item, "torso=", ServerData.Slot.TypeE.Torso, list) && !AddArmor(item, "shoulderstrap=", ServerData.Slot.TypeE.Shoulder, list) && !AddArmor(item, "weapon=", ServerData.Slot.TypeE.Weapon, list) && item.StartsWith("skin_color=") && !int.TryParse(item.Substring("skin_color=".Length), out skinColor))
			{
				skinColor = -1;
			}
		}
		string text2 = list2.Find((string _) => _.StartsWith("hairs="));
		string text3 = list2.Find((string _) => _.StartsWith("hairs_color="));
		if (text2 != null)
		{
			PersonArmor.ArmorLoadEntry armorLoadEntry = new PersonArmor.ArmorLoadEntry();
			armorLoadEntry.IsHair = true;
			armorLoadEntry.SetName = text2.Remove(0, 6);
			PersonArmor.ArmorLoadEntry armorLoadEntry2 = armorLoadEntry;
			if (text3 != null)
			{
				int.TryParse(text3.Remove(0, 12), out armorLoadEntry2.HairColor);
			}
			list.Add(armorLoadEntry2);
		}
		personArmor.PutAllEnemyArmor(serverBotInfo.Model, list, delegate
		{
			personArmor.HideBodyParts();
			if (skinColor >= 0 && PersonArmor._hairsColors.ContainsKey(skinColor))
			{
				Color color = PersonArmor._hairsColors[skinColor];
				Renderer[] componentsInChildren = personArmor.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					if (GetComponent<Renderer>()materials.Length > 1)
					{
						GetComponent<Renderer>()materials[1].color = color;
					}
					else
					{
						GetComponent<Renderer>()materials[0].color = color;
					}
				}
			}
			SingletonT<ResourcesManager>.I.LoadAnimations(Globals.DebugDontLoadAnimation, serverBotInfo.Model, this, personArmor, delegate(string _)
			{
				FromAssetBundleAnimations fromAssetBundleAnimations = enemy.gameObject.AddComponent<FromAssetBundleAnimations>();
				fromAssetBundleAnimations.AnimationsAssetBundlePath = _;
				onLoad(enemy);
			});
		});
	}

	private void MsgPersonAttackFinishedHandler(Person person)
	{
		if (!(this == null) && base.enabled && person == Globals.Enemy)
		{
			StartCoroutine(WaitPlayerAnimationEndAndGoToPlayerTimeState());
		}
	}

	private IEnumerator WaitPlayerAnimationEndAndGoToPlayerTimeState()
	{
		if (State != StateE.WaitEnemyAttackEnd)
		{
			yield break;
		}
		while ((Globals.IsPaused || Globals.Player.animation.isPlaying || Pause) && (Globals.IsPaused || Pause || !Globals.Player.IsIdleAnimationPlaying()))
		{
			yield return null;
		}
		if (State == StateE.WaitEnemyAttackEnd)
		{
			State = StateE.PlayerTime;
			TimeRemainsTillTheEndOfState = SingletonT<ServerData>.I.GameSettings.TurnTime;
			if (HudMk1.Instance != null)
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BattleHud);
			}
		}
	}

	internal bool ExecutePlayerAttack(AttackE attackType)
	{
		if (IsSelectionMode || _attackUsed || State != StateE.PlayerTime || CurrentSpellLevel >= 0 || (_eye != null && _eye.Hited))
		{
			return false;
		}
		Utils.Log("ExecutePlayerAttack", attackType);
		_attackUsed = true;
		ViewSector.SideE side = ViewSector.Side;
		ReactE reactE = ReactE.Damage;
		if (attackType == AttackE.Right)
		{
			switch (side)
			{
			case ViewSector.SideE.Right:
				reactE = ReactE.Dodge;
				break;
			case ViewSector.SideE.RightCenter:
				reactE = ReactE.Block;
				break;
			}
			Metrics.OnAttack(reactE);
		}
		if (attackType == AttackE.Left)
		{
			switch (side)
			{
			case ViewSector.SideE.Left:
				reactE = ReactE.Dodge;
				break;
			case ViewSector.SideE.LeftCenter:
				reactE = ReactE.Block;
				break;
			}
			Metrics.OnAttack(reactE);
		}
		if (attackType == AttackE.Forward)
		{
			if (side == ViewSector.SideE.Center && attackType != AttackE.Combo)
			{
				reactE = ReactE.Dodge;
			}
			else if (side == ViewSector.SideE.LeftCenter || side == ViewSector.SideE.RightCenter)
			{
				reactE = ReactE.Block;
			}
			Metrics.OnAttack(reactE);
		}
		int damage = 0;
		bool isCrit = false;
		if (reactE != ReactE.Dodge)
		{
			if (attackType == AttackE.Combo)
			{
				NextAttackIsWithForceElixir = 0;
			}
			SingletonT<ServerData>.I.GetDamage(SingletonT<ServerData>.I.PlayerInBattleParams, SingletonT<ServerData>.I.EnemyParams, attackType != AttackE.Combo, NextAttackIsRageCrit, NextAttackIsWithForceElixir, out isCrit, out damage);
			if (NextAttackIsRageCrit)
			{
				isCrit = true;
				reactE |= ReactE.FromRages;
			}
			if (Globals.DebugPlayerAttackAlwaysCrit)
			{
				isCrit = true;
			}
			if (Globals.DebugPlayerAttackNoCrit)
			{
				isCrit = false;
			}
			NextAttackIsWithForceElixir = 0;
			if (isCrit)
			{
				reactE |= ReactE.Critical;
			}
			if (reactE.HasFlag(ReactE.Block))
			{
				damage /= 2;
				if (damage < 1)
				{
					damage = 1;
				}
			}
		}
		NextAttackIsRageCrit = false;
		if (Globals.BuildType == Globals.BuildTypeE.ShowContent)
		{
			damage = 100;
		}
		if (Globals.DebugPlayerLargeDamage)
		{
			damage = Globals.Enemy.MaxHealth / 2 + 1;
		}
		if (Globals.DebugPlayerOneHitKill)
		{
			damage = 75000;
		}
		if (Globals.DebugSmallDamageOnEnemy)
		{
			damage = 1;
		}
		if (Globals.DebugNoDamageOnEnemy)
		{
			damage = 0;
		}
		if (attackType == AttackE.Combo)
		{
			damage = Mathf.CeilToInt(SingletonT<ServerData>.I.GameSettings.ComboK * (float)damage);
			reactE = ReactE.Critical | ReactE.Damage;
		}
		if (Globals.DebugEnemyAlwaysBlock)
		{
			reactE = ReactE.Block;
		}
		int num = Globals.Enemy.Health - damage;
		if (num <= 0)
		{
			bool flag = reactE.HasFlag(ReactE.Block);
			if (Globals.UseFatalityWhiteSpheresMode)
			{
				bool flag2 = reactE.HasFlag(ReactE.Critical);
				reactE = ReactE.Death | ReactE.NoBubbles | ReactE.Fatality;
				if (flag2)
				{
					reactE |= ReactE.Critical;
				}
			}
			else
			{
				reactE = ReactE.Critical | ReactE.Damage | ReactE.NoBubbles;
			}
			if (flag)
			{
				reactE |= ReactE.Block;
			}
			LocationToIncrement = AreaData.Current.Location;
		}
		if ((attackType == AttackE.Combo || attackType == AttackE.Magic || isCrit) && !reactE.HasFlag(ReactE.Block))
		{
			CritsCount++;
		}
		if (ComboAttackDoneIndex < Combo.Count && Combo[ComboAttackDoneIndex] == attackType)
		{
			ComboAttackDoneIndex++;
			UpdateComboView();
		}
		State = StateE.WaitPlayerAttackEnd;
		Messenger.Invoke(Globals.MsgPlayerReact, reactE);
		Messenger.Invoke(Globals.MsgPlayerAttack, attackType);
		Globals.Player.Attack(attackType, damage, DamageTypeE.Natural, reactE);
		return true;
	}

	internal void ExecuteFatalityDelayedPlayerAttack(AttackE attack)
	{
		Globals.Player.Attack(attack, 0, DamageTypeE.Natural, ReactE.Death | ReactE.Critical);
	}

	internal void ExecuteEnemyAttack(bool skipStep)
	{
		State = StateE.WaitEnemyAttackEnd;
		bool nextEnemyAttackBlocked = _nextEnemyAttackBlocked;
		_nextEnemyAttackBlocked = false;
		Globals.Enemy.ExecuteAttack(skipStep, nextEnemyAttackBlocked);
	}

	private void InitTouchscreenCasts()
	{
		_recognizer = new Recognizer();
		_recognizer.AddGesture(new Lightning());
		_recognizer.AddGesture(new Dark());
		_recognizer.AddGesture(new Fire());
		_recognizer.AddGesture(new Ice());
	}

	private DamageTypeE GetDamageType(string magic)
	{
		if (magic == Globals.MagicFire)
		{
			return DamageTypeE.FireMagic;
		}
		if (magic == Globals.MagicIce)
		{
			return DamageTypeE.IceMagic;
		}
		if (magic == Globals.MagicElectro)
		{
			return DamageTypeE.LightingMagic;
		}
		Invs.Inv(magic == Globals.MagicDarkness, "castName == Globals.MagicDarkness", magic);
		return DamageTypeE.DarkMagic;
	}

	private bool TryGestureCast(string magic)
	{
		DamageTypeE damageType = GetDamageType(magic);
		ServerData.Spell myMaxSpell = SingletonT<ServerData>.I.GetMyMaxSpell(damageType);
		if (myMaxSpell == null)
		{
			return false;
		}
		int mana = Globals.Player.Mana;
		if (myMaxSpell.EffectName.Length > 0)
		{
			Globals.Player.CastMagic(myMaxSpell, myMaxSpell.EffectName, mana, damageType, 1f);
			Messenger.Invoke(Globals.MsgGuiBattle_ShowMagicSuccess, GetMagicType(myMaxSpell.EffectName));
			SpellButtonPressed();
			State = StateE.WaitPlayerAttackEnd;
		}
		return true;
	}

	internal string GetMagicType(string scenario)
	{
		if (scenario.Contains("dark"))
		{
			return Globals.MagicDarkness;
		}
		if (scenario.Contains("fire"))
		{
			return Globals.MagicFire;
		}
		if (scenario.Contains("ice"))
		{
			return Globals.MagicIce;
		}
		return Globals.MagicElectro;
	}

	internal bool SpellButtonPressed()
	{
		if (State != StateE.PlayerTime)
		{
			return false;
		}
		if (CurrentSpellLevel == -1)
		{
			int level = 1;
			if (HudMk1.Instance != null)
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.CastMagic);
			}
			Messenger.Invoke(Globals.MsgGuiBattle_MagicCasts, SingletonT<ServerData>.I.IsHasSpell(level, ServerData.Skill.TypeE.MagicDark), SingletonT<ServerData>.I.IsHasSpell(level, ServerData.Skill.TypeE.MagicFire), SingletonT<ServerData>.I.IsHasSpell(level, ServerData.Skill.TypeE.MagicIce), SingletonT<ServerData>.I.IsHasSpell(level, ServerData.Skill.TypeE.MagicElectro));
			CurrentSpellLevel = 1;
			StopTurnTime();
			if (_eyeTime != null)
			{
				_eyeTime.Pause = true;
			}
			if (_eye != null)
			{
				_eye.Pause = true;
			}
			return true;
		}
		CurrentSpellLevel = -1;
		ResumeTurnTime("SpellButtonPressed");
		Messenger.Invoke(Globals.MsgMagicModeDisabled);
		return false;
	}

	private void OnMsgElixirApplyPoisonOnEnemy()
	{
		int num = Globals.Enemy.Health - _poisonDamage;
		ReactE reactE = ((num <= 0) ? (ReactE.Death | ReactE.Damage) : ReactE.Damage);
		reactE |= ReactE.NoBubbles;
		if (reactE.HasFlag(ReactE.Death))
		{
			Messenger.Invoke(Globals.MsgEnemyKilledByPoison);
			LocationToIncrement = AreaData.Current.Location;
			Globals.Enemy._dontAttack = true;
			Globals.Enemy.PlayScenario(Globals.Battle, "react_death", AttackE.None, reactE, DamageTypeE.AcidMagic, -_poisonDamage);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.5f, delegate
			{
				Messenger<Player>.Invoke(Globals.MsgPlayerAttackFinished, Globals.Player);
			});
		}
		else
		{
			if (Globals.Enemy.Health <= _poisonDamage)
			{
				Messenger.Invoke(Globals.MsgEnemyKilledByPoison);
			}
			Globals.Enemy.reactfx(AttackE.None, reactE, DamageTypeE.AcidMagic, -_poisonDamage, 0);
		}
	}

	private static List<AttackE> GenCombo(int length)
	{
		List<AttackE> list = new List<AttackE>(length);
		for (int i = 0; i < length; i++)
		{
			switch (UnityEngine.Random.Range(0, 3))
			{
			case 0:
				list.Add(AttackE.Left);
				break;
			case 1:
				list.Add(AttackE.Right);
				break;
			case 2:
				list.Add(AttackE.Forward);
				break;
			default:
				Invs.Inv(false, "GenCombo failed in random");
				break;
			}
		}
		return list;
	}

	internal void AttackCombo()
	{
		if (!IsComboAllowed)
		{
			return;
		}
		ComboAttackDoneIndex = 0;
		UpdateComboView();
		if (ExecutePlayerAttack(AttackE.Combo))
		{
			if (Globals.UseFatalityWhiteSpheresMode && SingletonT<ServerData>.I.PlayerParams.ResurrectionSpheresCount == 0 && (UnityEngine.Random.Range(0, 100) < SingletonT<ServerData>.I.GameSettings.ProbFatality || Globals.DebugFatalitySphereAlways))
			{
				_invokeFatalitySphere = true;
			}
			Metrics.OnPlayerCombo();
			Messenger.Invoke(Globals.MsgPlayerCombo);
		}
	}

	internal void UpdateComboView()
	{
		for (int i = 0; i < Combo.Count; i++)
		{
			if (i == ComboAttackDoneIndex)
			{
				Messenger.Invoke(Globals.MsgGuiBattle_NextCombo, Combo[i], i, Combo.Count);
				break;
			}
		}
	}

	internal void TryDoComboAttack(int index)
	{
		if (Combo != null && ComboAttackDoneIndex == index && index < Combo.Count && !Pause)
		{
			ExecutePlayerAttack(Combo[index]);
		}
	}

	private void CollectRenderersColor(GameObject go, Dictionary<MeshRenderer, Color> dict)
	{
		if (!(go != null))
		{
			return;
		}
		MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer.material.HasProperty("_Color"))
			{
				dict.Add(meshRenderer, meshRenderer.material.color);
			}
		}
	}

	private IEnumerator LightOff(float speed, float max)
	{
		if (_sceneObjectCache == null)
		{
			_sceneObjectCache = GameObject.Find(Globals.LocationGameObjectSceneGeomName);
		}
		if (_lightRenderers.Count > 0)
		{
			foreach (KeyValuePair<MeshRenderer, Color> c in _lightRenderers)
			{
				Material[] materials = c.Key.materials;
				foreach (Material cm in materials)
				{
					cm.color = c.Value;
				}
			}
			_lightRenderers.Clear();
		}
		CollectRenderersColor(_sceneObjectCache, _lightRenderers);
		_intensity = max;
		while (_lightMode == -1 && _intensity > _lightOffK)
		{
			_intensity = Mathf.Lerp(_intensity, 0f, Time.deltaTime * speed);
			foreach (KeyValuePair<MeshRenderer, Color> c2 in _lightRenderers)
			{
				Color m = c2.Key.material.color;
				Color t = c2.Value * _intensity;
				Material[] materials2 = c2.Key.materials;
				foreach (Material cm2 in materials2)
				{
					cm2.color = t;
				}
			}
			yield return null;
		}
	}

	private void LightOnNow()
	{
		if (_lightRenderers.Count == 0)
		{
			return;
		}
		if (_sceneObjectCache == null)
		{
			_sceneObjectCache = GameObject.Find(Globals.LocationGameObjectSceneGeomName);
		}
		foreach (KeyValuePair<MeshRenderer, Color> lightRenderer in _lightRenderers)
		{
			Color color = lightRenderer.Key.material.color;
			Color color2 = lightRenderer.Value * 1f;
			Material[] materials = lightRenderer.Key.materials;
			foreach (Material material in materials)
			{
				material.color = color2;
			}
		}
		_intensity = 1f;
	}

	private IEnumerator LightOn(float speed, float max)
	{
		if (_lightRenderers.Count == 0)
		{
			yield break;
		}
		if (_sceneObjectCache == null)
		{
			_sceneObjectCache = GameObject.Find(Globals.LocationGameObjectSceneGeomName);
		}
		while (_lightMode == 1 && _intensity < max)
		{
			_intensity = Mathf.Lerp(_intensity, max, Time.deltaTime * speed);
			foreach (KeyValuePair<MeshRenderer, Color> c in _lightRenderers)
			{
				Color m = c.Key.material.color;
				Color t = c.Value * _intensity;
				Material[] materials = c.Key.materials;
				foreach (Material cm in materials)
				{
					cm.color = t;
				}
			}
			yield return null;
		}
	}

	private void LateUpdate()
	{
		if (BattleGui.enabled)
		{
			UpdateBattle(Time.deltaTime);
		}
		else
		{
			UpdateSelect();
		}
	}

	private void UpdateSelect()
	{
		HideBloodScreenEffects();
	}

	private void Update_TimeRemainsTillTheEndOfState()
	{
		if (Globals.Player == null || Globals.Enemy == null || (State != StateE.PlayerTime && State != StateE.EnemyTime && State != StateE.FatalityMode && State != StateE.WaitEnemyMiniGameEnd) || Globals.ForceWeakMagicNoTimeLimit || Globals.ForceFatalityNoTimeLimit)
		{
			return;
		}
		float num = ((!(_stopTurnTime > 0f) || State == StateE.WaitEnemyMiniGameEnd) ? Time.deltaTime : 0f);
		_timeRemainsTillTheEndOfState -= num;
		if (_timeRemainsTillTheEndOfState > 0f || Globals.Player.InAttack || Globals.Enemy.InAttack)
		{
			return;
		}
		if (State == StateE.FatalityMode)
		{
			State = StateE.ShowFinishDialog;
		}
		else if (State == StateE.WaitEnemyMiniGameEnd)
		{
			if (_invokeEndMiniGame)
			{
				Messenger.Invoke(Globals.Msg_MagicGame_Finished, "weak", arg2: false);
			}
		}
		else if (State != StateE.EnemyTime)
		{
			State = ((State != StateE.EnemyTime) ? StateE.EnemyTime : StateE.PlayerTime);
			TimeRemainsTillTheEndOfState = SingletonT<ServerData>.I.GameSettings.TurnTime;
		}
	}

	private void Update()
	{
		if (BattleGui.enabled && _touchscreen != null)
		{
			_touchscreen.Update();
		}
	}

	private void UpdateBattle(float timeStep)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (State == StateE.FatalityMode || State == StateE.FatalityModeExecute)
		{
			if (_fatalityMode != null)
			{
				_fatalityMode.Update(timeStep);
			}
			Update_TimeRemainsTillTheEndOfState();
			return;
		}
		if (_bubbles != null)
		{
			_bubbles.Update(this);
		}
		if (_bubbles2 != null)
		{
			_bubbles2.Update(this);
		}
		if (Globals.Player != null && Globals.Enemy != null)
		{
			Vector3 position = Globals.Enemy.transform.position;
			_viewSectorRootTransform.position = new Vector3(position.x, ArenaCenter.transform.position.y, position.z);
			Vector3 position2 = Globals.Player.transform.position;
			_viewSectorRootTransform.LookAt(new Vector3(position2.x, _viewSectorRootTransform.position.y, position2.z));
		}
		Update_TimeRemainsTillTheEndOfState();
		if (_bubbles != null)
		{
			_bubbles.ProcessBubbles(this);
		}
		HudMk1.GuiDesc currentGui = HudMk1.Instance.CurrentGui;
		if (_eye == null && (currentGui.Type == GuiRoot.GuiType.BattleHud || currentGui.Type == GuiRoot.GuiType.EnemyTurn) && !IsInMagicMode && !Pause && Globals.Enemy != null && !Globals.Enemy.IsDead)
		{
			if (_eyeTime == null)
			{
				_eyeTime = new TimePeriod(SingletonT<ServerData>.I.GameSettings.EyePeriod);
			}
			else if (!_eyeTime.Fired)
			{
				_eyeTime.Update();
				if (_eyeTime.Fired && !TryCreateEye())
				{
					_eyeTime.Reset();
				}
			}
		}
		if (SingletonT<ServerData>.I.PlayerParams.Level > 6 && _fallingStar == null && FallingStarPrefab != null && (currentGui.Type == GuiRoot.GuiType.BattleHud || currentGui.Type == GuiRoot.GuiType.EnemyTurn) && !IsInMagicMode && !Pause)
		{
			_fallingStarPeriod -= Time.deltaTime;
			if (_fallingStarPeriod < 0f)
			{
				if (Globals.DebugFrequentFallingStars || UnityEngine.Random.Range(0, 100) < SingletonT<ServerData>.I.GameSettings.FallingStarProb)
				{
					Utils.Log("FALLING STAR CREATED", State);
					_fallingStar = (GameObject)UnityEngine.Object.Instantiate(FallingStarPrefab);
					_fallingStar.transform.parent = HudMk1.Instance.gameObject.transform;
					int num = 140;
					_fallingStar.transform.localPosition = new Vector3(num + (UnityEngine.Random.value * (float)(Camera2D.ScreenWidth - num * 2)).RoundToInt() - Camera2D.ScreenWidth / 2, Camera2D.ScreenHeight / 2, 1000f);
					Messenger.Invoke(Globals.MsgFallingStarSpawn);
				}
				_fallingStarPeriod = ((!Globals.DebugFrequentFallingStars) ? SingletonT<ServerData>.I.GameSettings.FallingStarCooldown : 10);
			}
		}
		if (!(Globals.Player != null))
		{
			return;
		}
		float current = 100f * ((float)Globals.Player.Health / (float)Globals.Player.MaxHealth);
		StateE state = Globals.Battle.State;
		if (!Globals.Player.IsDead && (state == StateE.PlayerTime || state == StateE.EnemyTime || state == StateE.WaitEnemyAttackEnd || state == StateE.WaitEnemyMiniGameEnd || state == StateE.WaitPlayerAttackEnd))
		{
			if (!TryShowBloodScreenEffect(current, SingletonT<ServerData>.I.GameSettings.BloodScreenHealthTreshold3, BloodScreenEffect3, BloodScreenEffect1, BloodScreenEffect2) && !TryShowBloodScreenEffect(current, SingletonT<ServerData>.I.GameSettings.BloodScreenHealthTreshold2, BloodScreenEffect2, BloodScreenEffect1, BloodScreenEffect3) && !TryShowBloodScreenEffect(current, SingletonT<ServerData>.I.GameSettings.BloodScreenHealthTreshold, BloodScreenEffect1, BloodScreenEffect2, BloodScreenEffect3))
			{
				HideBloodScreenEffects();
			}
		}
		else
		{
			HideBloodScreenEffects();
		}
	}

	private void HideBloodScreenEffects()
	{
		if (BloodScreenEffect1 != null && BloodScreenEffect1.active)
		{
			BloodScreenEffect1.SetActiveRecursivelyMk1(setActive: false);
		}
		if (BloodScreenEffect2 != null && BloodScreenEffect2.active)
		{
			BloodScreenEffect2.SetActiveRecursivelyMk1(setActive: false);
		}
		if (BloodScreenEffect3 != null && BloodScreenEffect3.active)
		{
			BloodScreenEffect3.SetActiveRecursivelyMk1(setActive: false);
		}
	}

	private bool TryShowBloodScreenEffect(float current, float treshold, GameObject effect, GameObject other1, GameObject other2)
	{
		if (effect == null)
		{
			return false;
		}
		if (current < treshold)
		{
			if (!effect.active)
			{
				effect.SetActiveRecursivelyMk1(setActive: true);
			}
		}
		else if (effect.active)
		{
			effect.SetActiveRecursivelyMk1(setActive: false);
		}
		if (effect.active)
		{
			if (other1 != null)
			{
				other1.SetActiveRecursivelyMk1(setActive: false);
			}
			if (other2 != null)
			{
				other2.SetActiveRecursivelyMk1(setActive: false);
			}
		}
		return effect.active;
	}

	private bool TryCreateEye()
	{
		int rageSpheresCount = SingletonT<ServerData>.I.PlayerParams.RageSpheresCount;
		if (rageSpheresCount <= 0)
		{
			return false;
		}
		ServerData.Settings gameSettings = SingletonT<ServerData>.I.GameSettings;
		int num = UnityEngine.Random.Range(0, 100);
		int num2 = ((rageSpheresCount <= 3) ? gameSettings.EyeProb1 : ((rageSpheresCount > 8) ? gameSettings.EyeProb3 : gameSettings.EyeProb2));
		if ((Globals.ShowEyeInBattle || num < num2) && SingletonT<ServerData>.I.PlayerParams.Level >= SingletonT<ServerData>.I.GameSettings.SkillRageLevel)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(EyeInBattle);
			gameObject.name = "__eye";
			_eye = gameObject.GetComponentInChildren<Eye>();
			Messenger.Invoke(Globals.MsgFightEyeShown);
			return true;
		}
		return false;
	}

	private void RestoreStartBattleData()
	{
		SingletonT<ServerData>.I.PlayerParams._rageSpheresCount = _startRagesCount;
		Dictionary<ServerData.Skill.TypeE, int> spellsUsedCount = SingletonT<ServerData>.I.PlayerParams.SpellsUsedCount;
		SingletonT<ServerData>.I.PlayerParams.SetSpellsUsedCount(_startSpellsUsedCount);
		Globals.Player.Mana = _startManaCount;
	}

	private void MsgPersonDieHandler(Person person)
	{
		Utils.DestroyGameObject(ref _eye);
		if (_fallingStar != null)
		{
			UnityEngine.Object.Destroy(_fallingStar);
		}
		if (person == Globals.Enemy)
		{
			SingletonT<ServerData>.I.BattleFinishedWithWin(Globals.Enemy.ServerBotInfo.Id, Time.time);
		}
		if (person == Globals.Player)
		{
			SingletonT<ServerData>.I.BattleFinishedWithLose(Globals.Enemy.ServerBotInfo.Id, Time.time);
			RestoreStartBattleData();
		}
		_hasDeadPersons = true;
		_bubbles.DestroyAll();
		if (person == Globals.Enemy && Globals.MainMenu._lastLocationOrMap == GuiRoot.GuiType.Location)
		{
			AreaData.Current.Location.Logic.MobFromFightDie();
		}
		ResetIfPutonFullXXX();
		if (person == Globals.Player)
		{
			State = StateE.ShowFinishDialog;
			Globals.InFight = false;
		}
	}

	private void ResetIfPutonFullXXX()
	{
		if (SingletonT<ServerData>.I.HasPutonWithSuperEffect(ServerData.Skill.TypeE.FullMana))
		{
			SingletonT<ServerData>.I.PlayerParams._mana = 0;
		}
		if (SingletonT<ServerData>.I.HasPutonWithSuperEffect(ServerData.Skill.TypeE.FullRage))
		{
			SingletonT<ServerData>.I.PlayerParams.RageSpheresCount = 0;
		}
	}

	internal void ChangeEnemy(AreaData.MobData mobData, ActionD onLoad)
	{
		Utils.Log("**** CHANGEENEMY", mobData);
		ServerData.BotInfo serverInfo = mobData.ServerInfo;
		string text = Globals.EnemyName(serverInfo.Id.ToString());
		Enemy enemy = Globals.Enemy;
		if (enemy != null && enemy.gameObject.name == text)
		{
			enemy.DoScale(enemy.ServerBotInfo.Scale, useInitScale: true);
			mobData.SetParamsToEnemy();
			Utils.SetAllRenderersActive(enemy.gameObject, value: true);
			enemy.ForceIdleForFatality();
			onLoad();
			return;
		}
		if (enemy != null)
		{
			SingletonT<SoundManager>.I.OnNewEnemy(Globals.Player, enemy);
			enemy._destroyed = true;
		}
		else
		{
			SingletonT<SoundManager>.I.OnNewEnemy(Globals.Player);
		}
		if (mobData.ServerInfo.Model == "1" || mobData.ServerInfo.Model == "2")
		{
			Globals.ShowLoadingScreen(delegate
			{
				ChangeEnemyImpl(mobData, onLoad);
			});
		}
		else
		{
			ChangeEnemyImpl(mobData, onLoad);
		}
	}

	private void ChangeEnemyImpl(AreaData.MobData mobData, ActionD onLoad)
	{
		ServerData.BotInfo serverInfo = mobData.ServerInfo;
		string text = Globals.EnemyName(serverInfo.Id.ToString());
		Enemy enemy = Globals.Enemy;
		GameObject enemyOnScene = GameObject.Find(text);
		if (enemyOnScene == null)
		{
			InitEnemy(serverInfo, mobData.LevelData, delegate(Enemy enemy2)
			{
				enemyOnScene = Globals.Enemy.gameObject;
				Globals.Enemy = enemyOnScene.GetComponent<Enemy>();
				Utils.SetAllRenderersActive(Globals.Enemy.gameObject, value: true);
				SingletonT<Fxs>.I.CleanFxCache("enemy");
				Globals.Enemy.CacheFxs();
				if (!Globals.DebugDontLoadPlayer)
				{
					Globals.Player.CacheFxs();
				}
				Utils.PrecacheAnimations(Globals.Enemy.gameObject);
				mobData.SetParamsToEnemy();
				enemy2.PlayAnim("idle", inLoop: false);
				enemyOnScene.SetActiveRecursivelyMk1(setActive: true);
				PersonArmor component = enemyOnScene.GetComponent<PersonArmor>();
				if (component != null)
				{
					component.HideBodyParts();
				}
				onLoad();
			});
		}
		Globals.SetColorAsBotsColor(enemyOnScene);
		if (enemy != null)
		{
			Person.DestroyPerson(enemy.gameObject);
		}
	}

	internal void SetPoisonOnEnemy(int damage)
	{
		_poisonDamage = damage;
	}

	internal void StartFight(AreaData.MobData enemyData)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		BattleResultsHud.ResetFightResultsStats();
		LocationLogic._chapterBonus = null;
		int num = UnityEngine.Random.Range(1, 3);
		SingletonT<SoundManager>.I.PlayGlobalSound("attack" + num + "_" + SingletonT<ServerData>.I.PlayerServerPersData.Id);
		Utils.LogForce("STARTFIGHT", HudMk1.Instance.CurrentGui, enemyData);
		if (HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Fight || HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.FightOnLocation)
		{
			_lastStartFightGui = HudMk1.Instance.CurrentGui;
		}
		Globals.MainMenu.SaveGame();
		Fxs.DestroyAllInbattleFxs();
		Globals.ResetTutorialsFlags();
		Messenger<bool>.Invoke(Globals.MsgFightPause, arg1: false);
		Globals.ViewSectorMoveForce = false;
		Globals.LevelAtStartFight = SingletonT<ServerData>.I.PlayerParams.Level;
		_invokeFatalitySphere = false;
		SingletonT<ServerData>.I.PlayerParams.ResurrectionSpheresCount = 0;
		_nextEnemyAttackBlocked = false;
		_rageUsed = false;
		_invokeEndMiniGame = false;
		SingletonT<ResourcesManager>.I.UnloadUnusedAssetsFake(Globals.MainMenu, delegate
		{
			if (_fallingStar != null)
			{
				UnityEngine.Object.Destroy(_fallingStar.gameObject);
			}
			_eyeTime = null;
			Time.timeScale = Globals.DefaultTimeScale;
			_hasDeadPersons = false;
			_attackUsed = false;
			CurrentSpellLevel = -1;
			NextAttackIsWithForceElixir = 0;
			SingletonT<ServerData>.I.UpdatePlayerArmorParams();
			Messenger.Invoke(Globals.MsgElixirCooldownChanged, ServerData.Item.ElixirTypeE.Poison, 1, 1);
			Messenger.Invoke(Globals.MsgElixirCooldownChanged, ServerData.Item.ElixirTypeE.Heal, 1, 1);
			Messenger.Invoke(Globals.MsgElixirCooldownChanged, ServerData.Item.ElixirTypeE.Critical, 1, 1);
			SingletonT<ServerData>.I.RefreshElixirsCount();
			Messenger.Invoke(Globals.MsgResurrectionCountChanged, 0);
			if (Globals.DebugInitRageBubblesCount >= 0)
			{
				SingletonT<ServerData>.I.PlayerParams.RageSpheresCount = Globals.DebugInitRageBubblesCount;
			}
			_startRagesCount = SingletonT<ServerData>.I.PlayerParams.RageSpheresCount;
			_startManaCount = SingletonT<ServerData>.I.PlayerParams._mana;
			_startSpellsUsedCount = new Dictionary<ServerData.Skill.TypeE, int>(SingletonT<ServerData>.I.PlayerParams.SpellsUsedCount);
			if (SingletonT<ServerData>.I.HasPutonWithSuperEffect(ServerData.Skill.TypeE.FullMana))
			{
				_startManaCount = 0;
				SingletonT<ServerData>.I.PlayerParams._mana = SingletonT<ServerData>.I.GameSettings.MaxMana;
			}
			if (SingletonT<ServerData>.I.HasPutonWithSuperEffect(ServerData.Skill.TypeE.FullRage))
			{
				_startRagesCount = 0;
				SingletonT<ServerData>.I.PlayerParams.RageSpheresCount = SingletonT<ServerData>.I.GameSettings.MaxRage;
			}
			SingletonT<ServerData>.I.NewBattle();
			_poisonDamage = 0;
			NextAttackIsRageCrit = false;
			SingletonT<ServerData>.I.PreparePlayerSkillDataForFight();
			ViewSector component = FullSector.GetComponent<ViewSector>();
			ServerData.BotLevel botLevel = SingletonT<ServerData>.I.GetBotLevel(enemyData);
			component.SpeedInAngles = botLevel.SpeedMoveSectorControl;
			_showEnemySector = botLevel.ShowSectorControl;
			SetSectorRendererActive(_showEnemySector);
			_fatalityExecuted = FatalityStateE.None;
			Player player = Globals.Player;
			Enemy enemy = Globals.Enemy;
			enemy.FromLocation = enemyData.FromLocation;
			if (!Globals.NewBerserkMode)
			{
				SingletonT<ServerData>.I._inBerserkMode = SingletonT<ServerData>.I.IsBerserkMode;
				if (SingletonT<ServerData>.I._inBerserkMode)
				{
					player.AddBerserk(1f);
				}
				else
				{
					player.RemoveBerserk();
				}
			}
			else
			{
				SingletonT<ServerData>.I._inBerserkMode = false;
				player.RemoveBerserk();
			}
			Globals.Player.Mana = SingletonT<ServerData>.I.PlayerParams._mana;
			player.MaxHealth = SingletonT<ServerData>.I.PlayerInBattleParams.HP;
			player.Health = player.MaxHealth;
			player.Restart();
			Globals.Player.Mana = SingletonT<ServerData>.I.PlayerParams._mana;
			int num2 = SingletonT<ServerData>.I.PlayerInBattleParams.Magic / 80;
			if (Globals.Player.Mana < num2)
			{
				Globals.Player.Mana = num2;
			}
			if (Globals.DebugFullMana)
			{
				player.Mana = 100;
			}
			enemy.SetServerData(enemyData.ServerInfo, enemyData.LevelData);
			enemy.MaxHealth = SingletonT<ServerData>.I.EnemyParams.HP;
			enemy.Health = enemy.MaxHealth;
			State = StateE.PlayerTime;
			TurnsCount = 1;
			CritsCount = 0;
			FallingStarsClicked = 0;
			TimeRemainsTillTheEndOfState = SingletonT<ServerData>.I.GameSettings.TurnTime;
			_bubbles = new Bubbles();
			player.gameObject.SetActiveRecursivelyMk1(setActive: true);
			player.GetComponent<PersonArmor>().HideBodyParts();
			player.gameObject.animation.cullingType = AnimationCullingType.BasedOnRenderers;
			player.SetupOnScene(this);
			if (SelectGui != null)
			{
				if (SelectGui.Hud != null)
				{
					SelectGui.Hud.HideHud();
				}
				SelectGui.enabled = false;
			}
			BattleGui.enabled = true;
			if (enemyData.LevelData.ZoneSize == 1)
			{
				component.ZoneSize = 2;
			}
			else if (enemyData.LevelData.ZoneSize == 2)
			{
				component.ZoneSize = 3;
			}
			else
			{
				component.ZoneSize = 1;
			}
			ViewSector.transform.root.gameObject.SetActiveRecursivelyMk1(setActive: true);
			ViewSector.Init();
			Combo = GenCombo(Globals.ComboSize);
			ComboAttackDoneIndex = 0;
			UpdateComboView();
			_touchscreen.Clear();
			Fxs.CurrentFxTag = InBattleFxTag;
			IPadTouchscreen.Restart();
			IPadTouchscreen.TrailEnabled = true;
			InitSectors();
			Messenger.Invoke(Globals.MsgGuiBattle_SetMagicBarVisible, arg1: true);
			ViewSector componentInChildren = FullSector.GetComponentInChildren<ViewSector>();
			componentInChildren.ChangeDirProb = SingletonT<ServerData>.I.EnemyParams.MobData.LevelData.ChangeViewDirProb;
			componentInChildren.ChangeDirPeriod = SingletonT<ServerData>.I.EnemyParams.MobData.LevelData.ChangeViewDirPeriod;
			Messenger.Invoke(Globals.MsgPersonManaChanged, Globals.Player);
			Messenger.Invoke(Globals.MsgRageSpheresCountChanged, SingletonT<ServerData>.I.PlayerParams.RageSpheresCount);
			Messenger.Invoke(Globals.MsgFightStarted);
			Globals.InFight = true;
		});
	}

	internal void RestartBattleWithSameEnemy(bool defeated)
	{
		if (!(HudMk1.Instance == null))
		{
			ReturnToArea(playMusic: false, showSelectGui: false);
			FightScreen fightScreen = (FightScreen)UnityEngine.Object.FindObjectOfType(typeof(FightScreen));
			if (!defeated)
			{
				RestoreStartBattleData();
				SingletonT<ServerData>.I.BattleBreaked();
			}
			Globals.Enemy.RemoveAllEffects();
			if (!defeated)
			{
				BreakImpl();
			}
			if (fightScreen != null && AreaData.Current.Mobs == null)
			{
				FightOnLocationHud componentInChildren = HudMk1.Instance.GetComponentInChildren<FightOnLocationHud>();
				Invs.Inv(componentInChildren != null, "Can't find FightOnLocationHud");
				Globals.Battle.StartFight(componentInChildren.MobData);
			}
			else if (fightScreen != null && fightScreen.ActiveMobIndex >= 0 && fightScreen.ActiveMobIndex < AreaData.Current.Mobs.Length)
			{
				StartFight(AreaData.Current.Mobs[fightScreen.ActiveMobIndex]);
			}
			else
			{
				StartFight(AreaData.Current.Mobs[SingletonT<ServerData>.I.GetLocationProgress(AreaData.Current.Location)]);
			}
		}
	}

	internal void BreakBattle()
	{
		if (!(HudMk1.Instance == null))
		{
			ReturnToArea(playMusic: false, showSelectGui: false);
			FightScreen fightScreen = (FightScreen)UnityEngine.Object.FindObjectOfType(typeof(FightScreen));
			HudMk1.Instance.ChangeGuiTo(_lastStartFightGui);
			SingletonT<ServerData>.I.BattleBreaked();
			RestoreStartBattleData();
			Globals.Enemy.RemoveAllEffects();
			BreakImpl();
		}
	}

	private void BreakImpl()
	{
		Utils.Log("BreakImpl");
		StopAllCoroutines();
		Globals.Player.DoScale(1f, useInitScale: true);
		if (Globals.Enemy != null)
		{
			Globals.Enemy.DoScale(Globals.Enemy.ServerBotInfo.Scale, useInitScale: true);
		}
		Utils.DestroyGameObject(ref _eye);
		if (_fallingStar != null)
		{
			UnityEngine.Object.Destroy(_fallingStar);
		}
		_bubbles.DestroyAll();
		_bubbles2.DestroyAll();
		Globals.Enemy.BreakBattle();
		Globals.Player.BreakBattle();
		Globals.Battle.State = StateE.PlayerTime;
		Globals.ForceWeakMagicNoTimeLimit = false;
		Globals.ForceFatalityNoTimeLimit = false;
		ResumeTurnTime("BreakImpl");
		LightOnNow();
		Messenger.Invoke(Globals.MsgGuiBattle_HidePhrase);
		Messenger.Invoke(Globals.MsgTutorialInfo, arg1: false);
		ResetIfPutonFullXXX();
		Messenger.Invoke(Globals.MsgFightBreak);
		Globals.InFight = false;
	}

	internal void ReturnToArea(bool playMusic, bool showSelectGui)
	{
		bool flag = SelectGui == null;
		if (showSelectGui && SelectGui != null)
		{
			SelectGui.enabled = true;
		}
		BattleGui.enabled = false;
		_invokeFatalitySphere = false;
		SingletonT<ServerData>.I.PlayerParams.ResurrectionSpheresCount = 0;
		Fxs.DestroyAllInbattleFxs();
		Fxs.CurrentFxTag = string.Empty;
		if (_bubbles != null)
		{
			_bubbles.DestroyAll();
		}
		if (!Globals.DebugDontLoadPlayer)
		{
			Globals.PlayerGameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		if (Globals.PlayerGameObject != null)
		{
			Globals.PlayerGameObject.SetActiveRecursivelyMk1(setActive: true);
			Globals.PlayerGameObject.GetComponent<PersonArmor>().HideBodyParts();
			Globals.PlayerGameObject.animation.cullingType = AnimationCullingType.BasedOnRenderers;
			Globals.PlayerGameObject.GetComponent<Player>().SetupOnScene(this);
			Globals.PlayerGameObject.GetComponent<Player>().PlayAnim("idle", inLoop: false);
			Globals.Player.Restart();
		}
		ViewSector.transform.root.gameObject.SetActiveRecursivelyMk1(setActive: false);
		if (playMusic)
		{
			Globals.PlayBattleMusic(this, wasSuccess: true);
		}
		IPadTouchscreen.TrailEnabled = false;
		if (flag)
		{
			GoToLocationFromFightResult();
		}
	}

	private void InitAreaView(int mobIndex)
	{
		if (HudMk1.Instance == null || AreaData.Current.Mobs.Length <= 0)
		{
			return;
		}
		ChangeEnemy(AreaData.Current.Mobs[mobIndex], delegate
		{
			SelectGui.UpdateSelected();
			HudMk1.Instance.InitLocationView(mobIndex, AreaData.Current);
			SetupEnemyOnScene();
			if (!_second)
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Fight);
			}
			Globals.HideLoadingScreen();
		});
	}

	internal Person Opponent(Person person)
	{
		if (Globals.Player.Equals(person))
		{
			return Globals.Enemy;
		}
		if (Globals.Enemy.Equals(person))
		{
			return Globals.Player;
		}
		return null;
	}

	internal void ShowLevelResultWin()
	{
		State = StateE.ShowFightResult;
		Messenger.Invoke(Globals.MsgGuiBattle_HideYouWin);
	}

	internal FightResultStats GenFightResults()
	{
		FightResultStats fightResultStats = new FightResultStats();
		int num = (fightResultStats.OldLevel = SingletonT<ServerData>.I.PlayerParams.Level);
		ServerData.BotInfo serverInfo = SingletonT<ServerData>.I.EnemyParams.MobData.ServerInfo;
		AreaData.MobData mobData = SingletonT<ServerData>.I.EnemyParams.MobData;
		ServerData.BotLevel botLevel = SingletonT<ServerData>.I.GetBotLevel(mobData);
		Invs.Inv(botLevel != null, "enemyLevel != null", mobData.Level);
		botLevel.GetWinBonus(out fightResultStats.AddedExperience);
		int num2 = (int)((float)(fightResultStats.AddedExperience * SingletonT<ServerData>.I.PlayerInBattleParams.BonusExp) / 100f);
		Utils.Log("BONUSEXP", num2, fightResultStats.AddedExperience, AreaData.Current.Location.IsOpened);
		fightResultStats.AddedExperience += num2;
		SingletonT<ServerData>.I.AddPlayerExperience(fightResultStats.AddedExperience);
		if (SingletonT<ServerData>.I.PlayerParams.Level != Globals.LevelAtStartFight)
		{
			Globals.MainMenu.AddOpenCondition(MainMenu.EventTypeE.NewLevel, 0);
		}
		int level = SingletonT<ServerData>.I.PlayerParams.Level;
		if (level != num)
		{
			fightResultStats.LevelWasChanged = true;
		}
		ServerData.Bonus winBonus = SingletonT<ServerData>.I.GetWinBonus(Globals.Enemy, mobData);
		if (winBonus != null)
		{
			fightResultStats.ChestBonuses = new List<ServerData.Bonus.DropElement>();
			winBonus.GetRandomDrop(5, fightResultStats.ChestBonuses);
		}
		fightResultStats.Fatality = _fatalityExecuted;
		fightResultStats.Damage = SingletonT<ServerData>.I.EnemyParams.HP;
		fightResultStats.Turns = TurnsCount;
		fightResultStats.Crits = CritsCount;
		fightResultStats.FallingStars = FallingStarsClicked;
		fightResultStats.AddedAnger = ((_fatalityExecuted != FatalityStateE.Executed) ? SingletonT<ServerData>.I.GameSettings.AngerPerBattle : SingletonT<ServerData>.I.GameSettings.AngerPerFatality);
		SingletonT<ServerData>.I.PlayerParams.Anger += fightResultStats.AddedAnger;
		ServerData.Settings gameSettings = SingletonT<ServerData>.I.GameSettings;
		int num3 = ((float)CritsCount / (float)TurnsCount * 100f).RoundToInt();
		fightResultStats.FightRating = ((num3 >= gameSettings.Crits5StarsPercent) ? 5 : ((num3 >= gameSettings.Crits4StarsPercent) ? 4 : ((num3 >= gameSettings.Crits3StarsPercent) ? 3 : ((num3 < gameSettings.Crits2StarsPercent) ? 1 : 2))));
		SingletonT<ServerData>.I.PlayerParams.MoneyStarsCount += fightResultStats.FightRating + fightResultStats.FallingStars;
		return fightResultStats;
	}

	internal void ShowFightResultWin()
	{
		if (!(HudMk1.Instance == null))
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BattleResults);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.001f, delegate
			{
				Globals.Player.RemoveBerserk();
				DestroyEnemy();
			});
		}
	}

	internal void StopTurnTime()
	{
		Utils.Log("StopTurnTime");
		_stopTurnTime = Time.time;
	}

	internal void ResumeTurnTime(string reason)
	{
		Utils.Log("ResumeTurnTime", reason);
		_stopTurnTime = -1f;
	}

	internal void LightOn(float speed, float wait, float max)
	{
		SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(wait, delegate
		{
			_lightMode = 1;
			StartCoroutine(LightOn(speed, max));
		});
	}

	internal void LightOff(float speed, float wait, float max)
	{
		if (!Globals.DebugFatalityNoDark || !(Globals.Battle != null) || Globals.Battle.State != StateE.FatalityMode)
		{
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(wait, delegate
			{
				_lightMode = -1;
				StartCoroutine(LightOff(speed, max));
			});
		}
	}

	internal void KillEye()
	{
		if (_eye != null && !IsInMagicMode)
		{
			_eye.Die();
			_eye = null;
			Utils.Log("KILLEYE");
			_eyeTime = null;
		}
	}

	internal void ShowMagicScreenFx(DamageTypeE damageType)
	{
		UnityEngine.Object obj = null;
		switch (damageType)
		{
		case DamageTypeE.FireMagic:
			if (_fireScreenFx == null)
			{
				_fireScreenFx = Util.Resource<GameObject>("magic/fx_magic_sceen_fire");
			}
			obj = _fireScreenFx;
			break;
		case DamageTypeE.DarkMagic:
			if (_darkScreenFx == null)
			{
				_darkScreenFx = Util.Resource<GameObject>("magic/fx_magic_sceen_dark");
			}
			obj = _darkScreenFx;
			break;
		case DamageTypeE.IceMagic:
			if (_iceScreenFx == null)
			{
				_iceScreenFx = Util.Resource<GameObject>("magic/fx_magic_sceen_ice");
			}
			obj = _iceScreenFx;
			break;
		case DamageTypeE.LightingMagic:
			if (_lightingScreenFx == null)
			{
				_lightingScreenFx = Util.Resource<GameObject>("magic/fx_magic_sceen_lighting");
			}
			obj = _lightingScreenFx;
			break;
		}
		if (obj != null)
		{
			UnityEngine.Object magicFx = UnityEngine.Object.Instantiate(obj, new Vector3(0f, 10000f, 0f), Quaternion.identity);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(1.5f, delegate
			{
				UnityEngine.Object.Destroy(magicFx);
			});
		}
	}
}
