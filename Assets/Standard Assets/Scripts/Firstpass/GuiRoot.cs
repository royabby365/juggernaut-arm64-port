using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarx.Collections;

public class GuiRoot : MonoBehaviour
{
	public enum Anchor
	{
		LeftTop,
		CenterTop,
		RightTop,
		LeftCenter,
		CenterCenter,
		RightCenter,
		LeftBottom,
		CenterBottom,
		RightBottom
	}

	public enum GuiType
	{
		Fight = 0,
		BagItems = 1,
		BagStats = 2,
		Shop = 3,
		None = 4,
		Compare = 5,
		ChapterInfo = 6,
		SovereighnInfo = 7,
		BattleHud = 8,
		Location = 9,
		MainMap = 10,
		Execution = 11,
		CastMagic = 12,
		BattleResults = 13,
		StrongMagicMiniGame = 14,
		WeakMagicMiniGame = 15,
		PopupDefeat = 16,
		ChestMiniGame = 17,
		MagicBook = 18,
		EnemyTurn = 19,
		Options = 20,
		GlobalAlertPopup = 21,
		ExtraChapterInfo = 22,
		GotLevelScreen = 23,
		GotLevelNewItems = 24,
		FightOnLocation = 25,
		ExtraChapterCongratulations = 26,
		Bank = 27,
		GlobalComparePopup = 28,
		TutorialFullScreenInfo = 32,
		FullScreenPopup2Button = 33,
		Pause = 35,
		EnemyDefeated = 36,
		Achievments = 37,
		AchievementsScroll = 38,
		SkillBonus = 39,
		Final = 40,
		StartMenu = 41,
		ChooseChar = 42,
		ElfPopup = 43,
		FightScreenshot = 44,
		Match3 = 45,
		Match3StartScreen = 46,
		SupportPopup = 47,
		Advertising = 48
	}

	public enum State
	{
		On,
		Off
	}

	private AnimationCurve Easing;

	public GuiType[] GuiTypes;

	public bool ResetEveryTime;

	public Anchor RootAnchor;

	public float TransitionSec = 1f;

	public GuiType ActiveType = GuiType.None;

	public bool Hide = true;

	public static readonly HashSet<GuiType> BagTypes = new HashSet<GuiType>
	{
		GuiType.BagStats,
		GuiType.BagItems
	};

	public static readonly HashSet<GuiType> ModalTypes = new HashSet<GuiType>
	{
		GuiType.Shop,
		GuiType.BagItems,
		GuiType.BagStats,
		GuiType.ExtraChapterInfo,
		GuiType.MagicBook,
		GuiType.Options,
		GuiType.SupportPopup,
		GuiType.Bank,
		GuiType.AchievementsScroll,
		GuiType.Match3,
		GuiType.Match3StartScreen,
		GuiType.Advertising
	};

	public static readonly HashSet<GuiType> CancelableTypes = new HashSet<GuiType>
	{
		GuiType.ChestMiniGame,
		GuiType.Shop,
		GuiType.BagItems,
		GuiType.BagStats,
		GuiType.Location,
		GuiType.ChapterInfo,
		GuiType.SovereighnInfo,
		GuiType.BattleResults,
		GuiType.PopupDefeat,
		GuiType.ElfPopup,
		GuiType.EnemyDefeated,
		GuiType.GlobalAlertPopup,
		GuiType.GlobalComparePopup,
		GuiType.GotLevelScreen,
		GuiType.GotLevelNewItems,
		GuiType.Pause,
		GuiType.SupportPopup,
		GuiType.ExtraChapterInfo,
		GuiType.ExtraChapterCongratulations,
		GuiType.MagicBook,
		GuiType.Options,
		GuiType.Bank,
		GuiType.Achievments,
		GuiType.AchievementsScroll,
		GuiType.Match3,
		GuiType.Match3StartScreen,
		GuiType.Final,
		GuiType.Advertising
	};

	public static readonly HashSet<GuiType> NeedTransition = new HashSet<GuiType>
	{
		GuiType.MainMap,
		GuiType.Location
	};

	public static readonly HashSet<GuiType> Layer0 = new HashSet<GuiType>
	{
		GuiType.Fight,
		GuiType.FightOnLocation,
		GuiType.FightScreenshot,
		GuiType.BattleResults,
		GuiType.CastMagic,
		GuiType.StrongMagicMiniGame,
		GuiType.WeakMagicMiniGame,
		GuiType.Execution,
		GuiType.EnemyTurn,
		GuiType.MainMap,
		GuiType.Location,
		GuiType.BattleHud,
		GuiType.GotLevelScreen,
		GuiType.GotLevelNewItems,
		GuiType.PopupDefeat,
		GuiType.EnemyDefeated,
		GuiType.StartMenu,
		GuiType.ChooseChar
	};

	public static readonly HashSet<GuiType> Layer1 = new HashSet<GuiType>
	{
		GuiType.BagItems,
		GuiType.BagStats,
		GuiType.SkillBonus,
		GuiType.Bank,
		GuiType.Shop,
		GuiType.Options,
		GuiType.SupportPopup,
		GuiType.Compare,
		GuiType.ChapterInfo,
		GuiType.SovereighnInfo,
		GuiType.ExtraChapterInfo,
		GuiType.ExtraChapterCongratulations,
		GuiType.Pause,
		GuiType.ChestMiniGame,
		GuiType.MagicBook,
		GuiType.Final,
		GuiType.AchievementsScroll,
		GuiType.Achievments,
		GuiType.Match3,
		GuiType.Match3StartScreen
	};

	public static readonly HashSet<GuiType> Layer2 = new HashSet<GuiType>
	{
		GuiType.GlobalAlertPopup,
		GuiType.GlobalComparePopup,
		GuiType.TutorialFullScreenInfo,
		GuiType.FullScreenPopup2Button,
		GuiType.ElfPopup,
		GuiType.Advertising
	};

	private State _state = State.Off;

	private static bool _inited;

	public static Transform CurrentInstantiationParent;

	public float OnOff(bool on, GuiType what)
	{
		if (what == GuiType.None)
		{
			return 0f;
		}
		if (GuiTypesContains(what))
		{
			int layer = GetLayer(what);
			if (on && _state != State.On)
			{
				StartCoroutine("GoToNewPosition", System.Tuple.Create(layer, State.On));
				_state = State.On;
				ActiveType = what;
				return TransitionSec;
			}
			if (!on && _state != State.Off)
			{
				StartCoroutine("GoToNewPosition", System.Tuple.Create(layer, State.Off));
				_state = State.Off;
				ActiveType = GuiType.None;
				return TransitionSec;
			}
		}
		return 0f;
	}

	public float Remove(GuiType what)
	{
		if (GuiTypesContains(what))
		{
			int layer = GetLayer(what);
			_state = State.Off;
			base.transform.localPosition = GetNewPosition(_state, layer);
		}
		return 0f;
	}

	public float MoveOnLayer(GuiType what)
	{
		if (GuiTypesContains(what))
		{
			int layer = GetLayer(what);
			base.transform.localPosition = GetNewPosition(_state, layer);
		}
		return 0f;
	}

	public float ChangeState(State phase, GuiType to, bool transition)
	{
		if (_state == phase || to == GuiType.None)
		{
			return 0f;
		}
		int num = GetLayer(to);
		switch (_state)
		{
		case State.On:
			if (GuiTypesContains(to) && !ResetEveryTime)
			{
				return 0f;
			}
			StartCoroutine("GoToNewPosition", System.Tuple.Create(num, State.Off));
			_state = State.Off;
			ActiveType = GuiType.None;
			return TransitionSec;
		case State.Off:
			if (!GuiTypesContains(to))
			{
				return 0f;
			}
			if (transition)
			{
				num++;
			}
			StartCoroutine("GoToNewPosition", System.Tuple.Create(num, State.On));
			_state = State.On;
			ActiveType = to;
			return TransitionSec;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private IEnumerator GoToNewPosition(System.Tuple<int, State> layerState)
	{
		int layer = layerState.Item1;
		State state = layerState.Item2;
		Vector3 newLocalPosition = GetNewPosition(state, layer);
		Vector3 startPosition = base.transform.localPosition;
		float startTime = Time.time;
		if (state == State.On && Globals.DebugRenderersStage > Globals.DebugRenderers.DoNothing)
		{
			BroadcastMessage("TurnOnRenderer", SendMessageOptions.DontRequireReceiver);
		}
		float sofar;
		do
		{
			yield return null;
			sofar = Time.time - startTime;
			float dt = Easing.Evaluate(sofar);
			base.transform.localPosition = Vector3.Lerp(startPosition, newLocalPosition, dt);
		}
		while (!(sofar >= TransitionSec));
		base.transform.localPosition = newLocalPosition;
		if (state == State.Off && _state == State.Off && Globals.DebugRenderersStage > Globals.DebugRenderers.DoNothing)
		{
			BroadcastMessage("TurnOffRenderer", SendMessageOptions.DontRequireReceiver);
		}
		yield return null;
	}

	private void Awake()
	{
		if (CurrentInstantiationParent != null)
		{
			base.transform.parent = CurrentInstantiationParent;
		}
		Easing = AnimationCurve.EaseInOut(0f, 0f, TransitionSec, 1f);
		base.transform.localPosition = GetNewPosition(State.Off, GetMyLayer());
		if (!_inited)
		{
			_inited = true;
			DebugLayers();
		}
	}

	private int GetMyLayer()
	{
		return (GuiTypes == null || GuiTypes.Length <= 0) ? 2 : GetLayer(GuiTypes[0]);
	}

	public static int GetLayer(GuiType guiType)
	{
		if (guiType == GuiType.None)
		{
			return -1;
		}
		if (Layer0.Contains(guiType))
		{
			return 0;
		}
		if (Layer1.Contains(guiType))
		{
			return 1;
		}
		if (Layer2.Contains(guiType))
		{
			return 2;
		}
		throw new ApplicationException("IMPOSIBLE LAYER {0}".Fmt(guiType));
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
	}

	private Vector3 GetNewPosition(State guiState, int layer)
	{
		float num = 2f * (float)Camera2D.ScreenWidth;
		float num2 = 2f * (float)Camera2D.ScreenHeight;
		switch (guiState)
		{
		case State.On:
			return GetOnCoord(layer);
		case State.Off:
		{
			Vector3 onCoord = GetOnCoord(layer);
			return RootAnchor switch
			{
				Anchor.LeftTop => onCoord + new Vector3(0f - num, num2, 0f), 
				Anchor.CenterTop => onCoord + new Vector3(0f, num2, 0f), 
				Anchor.RightTop => onCoord + new Vector3(num, num2, 0f), 
				Anchor.LeftCenter => onCoord + new Vector3(0f - num, 0f), 
				Anchor.CenterCenter => onCoord + new Vector3(0f, -2f * num2, 0f), 
				Anchor.RightCenter => onCoord + new Vector3(num, 0f, 0f), 
				Anchor.LeftBottom => onCoord + new Vector3(0f - num, 0f - num2, 0f), 
				Anchor.CenterBottom => onCoord + new Vector3(0f, 0f - num2, 0f), 
				Anchor.RightBottom => onCoord + new Vector3(num, 0f - num2, 0f), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		default:
			throw new ArgumentException("guiState");
		}
	}

	private bool GuiTypesContains(GuiType type)
	{
		GuiType[] guiTypes = GuiTypes;
		foreach (GuiType guiType in guiTypes)
		{
			if (guiType == type)
			{
				return true;
			}
		}
		return false;
	}

	private Vector3 GetOnCoord(int layer)
	{
		int num = Camera2D.ScreenWidth / 2;
		int num2 = Camera2D.ScreenHeight / 2;
		int num3 = 2200 - layer * 700;
		return RootAnchor switch
		{
			Anchor.LeftTop => new Vector3(-num, num2, num3), 
			Anchor.CenterTop => new Vector3(0f, num2, num3), 
			Anchor.RightTop => new Vector3(num, num2, num3), 
			Anchor.LeftCenter => new Vector3(-num, 0f, num3), 
			Anchor.CenterCenter => new Vector3(0f, 0f, num3), 
			Anchor.RightCenter => new Vector3(num, 0f, num3), 
			Anchor.LeftBottom => new Vector3(-num, -num2, num3), 
			Anchor.CenterBottom => new Vector3(0f, -num2, num3), 
			Anchor.RightBottom => new Vector3(num, -num2, num3), 
			_ => throw new ApplicationException("cannot be here"), 
		};
	}

	private static void DebugLayers()
	{
		foreach (int value in Enum.GetValues(typeof(GuiType)))
		{
			if (!Layer0.Contains((GuiType)value) && !Layer1.Contains((GuiType)value) && !Layer2.Contains((GuiType)value) && Globals.IsDebugBuild)
			{
				Debug.Log("--------------NOT HERE -----------> {0}".Fmt((GuiType)value));
			}
		}
		HashSet<GuiType> hashSet = new HashSet<GuiType>(Layer0);
		if (Globals.IsDebugBuild)
		{
			Debug.Log("1 <---> 2");
		}
		foreach (GuiType item in Layer1)
		{
			if (hashSet.Contains(item) && Globals.IsDebugBuild)
			{
				Debug.Log(" ------------------> COLLISION! {0} ".Fmt(item));
			}
		}
		hashSet = new HashSet<GuiType>(Layer0.Concat(Layer1));
		if (Globals.IsDebugBuild)
		{
			Debug.Log("1,2 <---> 3");
		}
		foreach (GuiType item2 in Layer2)
		{
			if (hashSet.Contains(item2) && Globals.IsDebugBuild)
			{
				Debug.Log(" ------------------> COLLISION! {0} ".Fmt(item2));
			}
		}
	}
}
