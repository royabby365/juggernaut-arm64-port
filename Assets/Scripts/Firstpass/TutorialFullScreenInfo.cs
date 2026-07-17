using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class TutorialFullScreenInfo : MonoBehaviour
{
	private enum Quadrant
	{
		Ltop,
		Lbottom,
		Rtop,
		Rbottom
	}

	private enum Gender
	{
		Man,
		Woman
	}

	private enum Origin
	{
		BottomLeft,
		BottomCenter
	}

	private CompositeDisposable _subscriptions;

	private Action _callback;

	public static readonly Queue<Action> Dialogs = new Queue<Action>();

	public SpriteText InfoTextLtop;

	public SpriteText InfoTextRtop;

	public SpriteText InfoTextLbottom;

	public SpriteText NameTextLtop;

	public SpriteText NameTextRtop;

	public SpriteText NameTextLbottom;

	public Sprite Man;

	public TutorialContinueButton LtopContinue;

	public TutorialContinueButton RtopContinue;

	public TutorialContinueButton LbottomContinue;

	public Sprite FullScreenBg;

	private readonly Color _darkBg = new Color32(128, 128, 128, 200);

	private readonly Color _liteBg = new Color32(128, 128, 128, 64);

	private readonly Dictionary<ServerData.PhrasesE, Tuple<Gender, Quadrant>> _phrases = new Dictionary<ServerData.PhrasesE, Tuple<Gender, Quadrant>>
	{
		{
			ServerData.PhrasesE.Tut1_1,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.Tut1_2,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.Tut2_1,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.Tut2_2,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.Tut4_1,
			Tuple.Create(Gender.Man, Quadrant.Ltop)
		},
		{
			ServerData.PhrasesE.Tut4_1_1,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.Tut4_2,
			Tuple.Create(Gender.Man, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.Tut4_2_2,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.Tut6_1,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.Tut6_1_2,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutBattleTurn,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutEndChests,
			Tuple.Create(Gender.Woman, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutEye,
			Tuple.Create(Gender.Man, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutEye_2,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutImmBattle,
			Tuple.Create(Gender.Man, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutImmZ,
			Tuple.Create(Gender.Man, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutResurrection,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutSearchTreasure,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutShop,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutStrongMagic,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutFirstMoneyPile,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutPlayerDefeat,
			Tuple.Create(Gender.Woman, Quadrant.Ltop)
		},
		{
			ServerData.PhrasesE.TutLocationScarab,
			Tuple.Create(Gender.Woman, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutBagRage,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutBagMana,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutBagExp,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutBagMoney,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutBagMagic,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutShopFilter2,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutShopFilter3,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutShopFilter4,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutShopFilter5,
			Tuple.Create(Gender.Woman, Quadrant.Lbottom)
		},
		{
			ServerData.PhrasesE.TutGotLevel,
			Tuple.Create(Gender.Woman, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutFirstMobAttack,
			Tuple.Create(Gender.Woman, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutFirstMagicBook,
			Tuple.Create(Gender.Woman, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutMagicBookButton,
			Tuple.Create(Gender.Woman, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutSliceAttack1,
			Tuple.Create(Gender.Man, Quadrant.Rtop)
		},
		{
			ServerData.PhrasesE.TutAchievmentButton,
			Tuple.Create(Gender.Man, Quadrant.Ltop)
		},
		{
			ServerData.PhrasesE.TutEpicItemPuton,
			Tuple.Create(Gender.Woman, Quadrant.Ltop)
		},
		{
			ServerData.PhrasesE.TutFallingStarPhrase1,
			Tuple.Create(Gender.Man, Quadrant.Ltop)
		},
		{
			ServerData.PhrasesE.Match3Tutorial,
			Tuple.Create(Gender.Woman, Quadrant.Ltop)
		}
	};

	private Vector3 _leftRoot;

	private Vector3 _rightRoot;

	private Vector3 _leftCorner;

	private Vector3 _rightCorner;

	private float _lastClose;

	public static bool _showDialog = false;

	public static string _showDialogAAA = string.Empty;

	public string AAA = string.Empty;

	private Action Callback
	{
		get
		{
			return _callback;
		}
		set
		{
			Action callback = _callback;
			_callback = value;
			callback?.Invoke();
		}
	}

	public static bool IsShowDialog
	{
		get
		{
			return _showDialog;
		}
		set
		{
			if (_showDialog != value)
			{
				Utils.LogForce("SHOWDIALOG", _showDialog, "->", value);
				_showDialog = value;
				if (_showDialog)
				{
					StackTrace stackTrace = new StackTrace();
					_showDialogAAA = stackTrace.ToString();
				}
			}
		}
	}

	private string SpriteName(Gender gender)
	{
		return gender switch
		{
			Gender.Man => "mentor_male", 
			Gender.Woman => "mentor_female", 
			_ => throw new ArgumentOutOfRangeException("gender"), 
		};
	}

	private Origin SpriteOrigin(string spritename)
	{
		if (spritename.Contains("_clip"))
		{
			return Origin.BottomLeft;
		}
		return Origin.BottomCenter;
	}

	private void Awake()
	{
		_leftRoot = new Vector3(-245f, -70f, 0f);
		_rightRoot = new Vector3(245f, -70f, 0f);
		InfoTextLtop.Phrase_ = ServerData.PhrasesE.Custom;
		InfoTextRtop.Phrase_ = ServerData.PhrasesE.Custom;
		InfoTextLbottom.Phrase_ = ServerData.PhrasesE.Custom;
		NameTextLtop.Phrase_ = ServerData.PhrasesE.Custom;
		NameTextRtop.Phrase_ = ServerData.PhrasesE.Custom;
		NameTextLbottom.Phrase_ = ServerData.PhrasesE.Custom;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.PhrasesE>.AddListener(Globals.MsgTutorialInfo, OnTutorialInfoText));
		_subscriptions.Add(Messenger<string, int, string, Action>.AddListener(Globals.MsgShowStorylineDialog, OnDialogText));
	}

	private void OnDialogText(string text, int npcId, string side, Action callback)
	{
		Dictionary<int, ServerData.Npc> npcs = SingletonT<ServerData>.I.GetNpcs();
		if (!npcs.ContainsKey(npcId))
		{
			if (Globals.IsDebugBuild)
			{
				UnityEngine.Debug.LogError("====================== Cannot find NPC id:{0}".Fmt(npcId));
			}
			callback?.Invoke();
			return;
		}
		ServerData.Npc npc = npcs[npcId];
		bool flag = side.ToLower().Contains("left");
		string picture = npc.Picture;
		string title = npc.Title;
		HandleText(flag ? Quadrant.Rtop : Quadrant.Ltop, text, title);
		HandleMentor(!flag, picture, SpriteOrigin(picture));
		Callback = callback;
		FullScreenBg.Tint_ = _liteBg;
		HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.TutorialFullScreenInfo);
		IsShowDialog = true;
	}

	private void Start()
	{
		_leftCorner = new Vector3((float)(-Camera2D.ScreenWidth) / 2f, -70f, 0f);
		_rightCorner = new Vector3((float)Camera2D.ScreenWidth / 2f, -70f, 0f);
		LtopContinue.Click += RemoveTutorial;
		RtopContinue.Click += RemoveTutorial;
		LbottomContinue.Click += RemoveTutorial;
		HudMk1.Instance.Release += OnButtonRelease;
		HudMk1.Instance.DragEndWithButton += OnButtonRelease;
	}

	private void TurnOffRenderer()
	{
		RemoveTutorial();
	}

	public void RemoveTutorial()
	{
		SingletonT<SoundManager>.I.PlaySoundClickPopup();
		_lastClose = Time.time;
		IsShowDialog = false;
		HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.TutorialFullScreenInfo);
		Callback = null;
	}

	private void OnButtonRelease(SpriteButton spriteButton)
	{
		if (spriteButton.name != "__catch_all_tutorial_info_button" && !(HudMk1.Instance == null))
		{
			HudMk1.Instance.ShowFightButton(show: true);
		}
	}

	private void OnTutorialInfoText(ServerData.PhrasesE phrase1)
	{
		Action item = delegate
		{
			if (_phrases.ContainsKey(phrase1))
			{
				Tuple<Gender, Quadrant> tuple = _phrases[phrase1];
				ShowMessage(tuple, phrase1);
			}
			else if (Globals.IsDebugBuild)
			{
				UnityEngine.Debug.Log("@@@== NO TUTORIAL: {0}==".Fmt(phrase1));
			}
			Callback = delegate
			{
				Messenger.Invoke(Globals.MsgTutorialFullScreenInfoHided);
			};
		};
		Dialogs.Enqueue(item);
	}

	private void ShowMessage(Tuple<Gender, Quadrant> tuple, ServerData.PhrasesE phrase)
	{
		HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.TutorialFullScreenInfo);
		IsShowDialog = true;
		Gender item = tuple.Item1;
		Quadrant item2 = tuple.Item2;
		string phrase2 = SingletonT<ServerData>.I.GetPhrase(phrase);
		string npcName = GetName(item);
		HandleText(item2, phrase2, npcName);
		string text = SpriteName(item);
		HandleMentor(IsLeft(item2), text, SpriteOrigin(text));
		FullScreenBg.Tint_ = ((item != Gender.Woman) ? _liteBg : _darkBg);
	}

	private bool IsLeft(Quadrant quadrant)
	{
		switch (quadrant)
		{
		case Quadrant.Ltop:
		case Quadrant.Lbottom:
			return true;
		case Quadrant.Rtop:
		case Quadrant.Rbottom:
			return false;
		default:
			throw new ArgumentOutOfRangeException("quadrant");
		}
	}

	private void HandleMentor(bool isLeft, string spriteName, Origin origin)
	{
		Man.ShowOrHide(show: true);
		Man.SpriteName_ = spriteName;
		Vector3 localPosition;
		if (origin == Origin.BottomLeft)
		{
			localPosition = ((!isLeft) ? _leftCorner : _rightCorner);
			Man.Origin = ((!isLeft) ? Quad.OriginPlace.BottomLeft : Quad.OriginPlace.BottomRight);
		}
		else
		{
			localPosition = ((!isLeft) ? _leftRoot : _rightRoot);
			Man.Origin = Quad.OriginPlace.BottomCenter;
		}
		Man.QuadMirror = (isLeft ? Quad.Mirror.Horizontal : Quad.Mirror.None);
		Man.transform.localPosition = localPosition;
		Man.Refresh();
	}

	private string GetName(Gender gender)
	{
		return gender switch
		{
			Gender.Man => SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.MentorManName), 
			Gender.Woman => SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.MentorWomanName), 
			_ => throw new ArgumentOutOfRangeException("gender"), 
		};
	}

	private void HandleText(Quadrant quadrant, string phrase, string npcName)
	{
		InfoTextLtop.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
		InfoTextLbottom.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
		InfoTextRtop.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
		NameTextLtop.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
		NameTextRtop.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
		NameTextLbottom.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
		LtopContinue.SetInactive();
		RtopContinue.SetInactive();
		LbottomContinue.SetInactive();
		switch (quadrant)
		{
		case Quadrant.Ltop:
			InfoTextLtop.Text_ = phrase;
			InfoTextLtop.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: true);
			NameTextLtop.Text_ = npcName;
			NameTextLtop.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: true);
			LtopContinue.SetActive();
			break;
		case Quadrant.Lbottom:
			InfoTextLbottom.Text_ = phrase;
			InfoTextLbottom.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: true);
			NameTextLbottom.Text_ = npcName;
			NameTextLbottom.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: true);
			LbottomContinue.SetActive();
			break;
		case Quadrant.Rtop:
			InfoTextRtop.Text_ = phrase;
			InfoTextRtop.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: true);
			NameTextRtop.Text_ = npcName;
			NameTextRtop.transform.parent.gameObject.SetActiveRecursivelyMk1(setActive: true);
			RtopContinue.SetActive();
			break;
		case Quadrant.Rbottom:
			if (Globals.IsDebugBuild)
			{
				UnityEngine.Debug.Log("!!! == NO RBOTTOM YET: {0} ==".Fmt(phrase));
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("quadrant");
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Update()
	{
		AAA = _showDialogAAA;
		if (!_showDialog && Dialogs.Count > 0)
		{
			Action action = Dialogs.Dequeue();
			action();
		}
	}
}
