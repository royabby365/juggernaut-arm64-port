using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class BattleResultsHud : MonoBehaviour
{
	private enum StageE
	{
		Exp,
		Chests,
		RatingReal,
		RatingCrits,
		RatingStars,
		Money,
		FallingStars,
		Finish
	}

	private CompositeDisposable _subscriptions;

	private SpriteText _bonusLabel;

	public Transform ChestsRoot;

	public BattleResultsChestButton[] Chests;

	public Transform Buttons;

	public Sprite[] BonusChests;

	public SpriteText DamageStatBonus;

	public SpriteText TurnsMoney;

	public Sprite VictoryIcon;

	public Sprite VictoryText;

	public AnimationCurve FadeOutCurve;

	public SpriteText RatingText;

	public SpriteText RealMovesText;

	public SpriteText CritMovesText;

	public GameObject FxStarPrefab;

	public Sprite[] Stars;

	public GameObject ExpRoot;

	public GameObject BonusChestsRoot;

	public GameObject MoneyRoot;

	public GameObject RealTurnsRoot;

	public GameObject CritTurnsRoot;

	public GameObject RatingRoot;

	public GameObject BonusStarsRoot;

	private SpriteGui _gui;

	private static FightResultStats _fightResultStats;

	private int _chestCount;

	private int _nonDropBeginIndex;

	private string _dmgBonusFmt;

	private string _turnsStatFmt;

	private Vector3 _showButtons;

	private float _tick;

	private Vector3 _chstsRootActive;

	private static readonly Color DimBonusChestColor = new Color32(32, 32, 32, 220);

	private readonly FontManager.ColorE[] _ratingColors = new FontManager.ColorE[5]
	{
		FontManager.ColorE.FightResult0,
		FontManager.ColorE.FightResult1,
		FontManager.ColorE.FightResult2,
		FontManager.ColorE.FightResult3,
		FontManager.ColorE.FightResult4
	};

	private readonly ServerData.PhrasesE[] _ratingPhrases = new ServerData.PhrasesE[5]
	{
		ServerData.PhrasesE.BattleResultRatingName0,
		ServerData.PhrasesE.BattleResultRatingName1,
		ServerData.PhrasesE.BattleResultRatingName2,
		ServerData.PhrasesE.BattleResultRatingName3,
		ServerData.PhrasesE.BattleResultRatingName4
	};

	private Dictionary<StageE, GameObject> _roots;

	public static void ResetFightResultsStats()
	{
		_fightResultStats = null;
	}

	private void Awake()
	{
		_gui = base.transform.GetSpriteGui();
		_showButtons = Buttons.localPosition;
		BattleResultsChestButton[] chests = Chests;
		foreach (BattleResultsChestButton battleResultsChestButton in chests)
		{
			battleResultsChestButton.Init();
		}
	}

	private void Start()
	{
		_dmgBonusFmt = SingletonT<ServerData>.I.GetPhrase(DamageStatBonus.Phrase_);
		DamageStatBonus.Phrase_ = ServerData.PhrasesE.Custom;
		_chstsRootActive = ChestsRoot.transform.localPosition;
		_bonusLabel = ChestsRoot.GetComponent<SpriteText>();
		_gui.Release += ChestsHandler;
		_gui.MoveEnd += FastForward;
		_roots = new Dictionary<StageE, GameObject>
		{
			{
				StageE.Chests,
				BonusChestsRoot
			},
			{
				StageE.Exp,
				ExpRoot
			},
			{
				StageE.Money,
				MoneyRoot
			},
			{
				StageE.FallingStars,
				BonusStarsRoot
			},
			{
				StageE.RatingCrits,
				CritTurnsRoot
			},
			{
				StageE.RatingReal,
				RealTurnsRoot
			},
			{
				StageE.RatingStars,
				RatingRoot
			},
			{
				StageE.Finish,
				null
			}
		};
	}

	private void FastForward(Vector3 obj)
	{
		_tick = 0f;
	}

	private void ChestsHandler(SpriteButton button)
	{
		BattleResultsChestButton battleResultsChestButton = button as BattleResultsChestButton;
		if (battleResultsChestButton == null)
		{
			return;
		}
		if (battleResultsChestButton.Selected || _chestCount < 1)
		{
			ServerData.Bonus.DropElement loot = battleResultsChestButton.GetLoot();
			if (loot != null)
			{
				Messenger<ServerData.Bonus.DropElement>.Invoke(Globals.MsgCompareDropElement, battleResultsChestButton.GetLoot());
			}
			else if (Globals.IsDebugBuild)
			{
				Debug.Log("====== loot is NULL: {0} ".Fmt(base.name));
			}
			return;
		}
		battleResultsChestButton.SetLoot(_fightResultStats.ChestBonuses[--_chestCount]);
		DimBonusChest(_chestCount);
		battleResultsChestButton.SetSelected();
		if (_chestCount <= 0)
		{
			BattleResultsChestButton[] chests = Chests;
			foreach (BattleResultsChestButton battleResultsChestButton2 in chests)
			{
				if (!battleResultsChestButton2.Selected)
				{
					battleResultsChestButton2.SetNonLoot(_fightResultStats.ChestBonuses[_nonDropBeginIndex++]);
				}
			}
			Buttons.localPosition = _showButtons;
			_bonusLabel.Phrase_ = ServerData.PhrasesE.Custom;
			_bonusLabel.Text_ = string.Empty;
		}
		else
		{
			_bonusLabel.Phrase_ = ServerData.PhrasesE.ChoosePrizeExecution;
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnShowBattleResults));
	}

	private IEnumerator AnimateStages(IEnumerable<StageE> stages)
	{
		yield return new WaitForSeconds(_tick);
		bool hasMoneyStage = false;
		foreach (StageE stage in stages)
		{
			if (stage != StageE.Finish)
			{
				SingletonT<SoundManager>.I.PlayGlobalSound("battle_result_checkpoint");
			}
			GameObject root = _roots[stage];
			if (root != null)
			{
				root.SetActiveRecursivelyMk1(setActive: true);
			}
			switch (stage)
			{
			case StageE.Exp:
				DamageStatBonus.Text_ = _dmgBonusFmt.Fmt(_fightResultStats.AddedExperience);
				break;
			case StageE.Chests:
			{
				Sprite[] bonusChests = BonusChests;
				foreach (Sprite item in bonusChests)
				{
					item.ShowOrHide(show: false);
				}
				int count = ((_fightResultStats.Fatality != Battle.FatalityStateE.Executed) ? 1 : 2);
				for (int k = 0; k < count; k++)
				{
					TurnOnBonusChest(k);
				}
				break;
			}
			case StageE.RatingReal:
			{
				string fmt = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.BattleResultTurnsCount);
				RealMovesText.Phrase_ = ServerData.PhrasesE.Custom;
				RealMovesText.Text_ = fmt.Fmt(_fightResultStats.Turns);
				break;
			}
			case StageE.RatingCrits:
			{
				string fmt = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.BattleResultTurnsCount);
				CritMovesText.Phrase_ = ServerData.PhrasesE.Custom;
				CritMovesText.Text_ = fmt.Fmt(_fightResultStats.Crits);
				break;
			}
			case StageE.RatingStars:
				RatingText.Phrase_ = _ratingPhrases[Mathf.Clamp(_fightResultStats.FightRating, 1, 5) - 1];
				RatingText.NamedColorE_ = _ratingColors[Mathf.Clamp(_fightResultStats.FightRating, 1, 5) - 1];
				ResetRating();
				StartCoroutine(AnimateStars(_fightResultStats.FightRating));
				break;
			case StageE.Money:
			{
				hasMoneyStage = true;
				TurnsMoney.Phrase_ = ServerData.PhrasesE.Custom;
				string fmt = Globals.CharIconGold + "{0}";
				TurnsMoney.Text_ = fmt.Fmt(SingletonT<ServerData>.I.PlayerInBattleParams.BonusMoney);
				break;
			}
			case StageE.FallingStars:
			{
				if (hasMoneyStage)
				{
					root.transform.localPosition = new Vector3(0f, -216f, 0f);
				}
				else
				{
					root.transform.localPosition = new Vector3(0f, -156f, 0f);
				}
				int totalWidth = Mathf.Min(5, _fightResultStats.FallingStars) * 45;
				for (int i = 0; i < _fightResultStats.FallingStars; i++)
				{
					GameObject starGO = new GameObject();
					starGO.transform.SetLayerRecursively(root.transform);
					Sprite sprite = starGO.AddComponent<Sprite>();
					sprite.Origin = Quad.OriginPlace.Center;
					sprite.SpriteName_ = "star";
					starGO.transform.parent = root.transform;
					int padding = ((float)totalWidth / (float)_fightResultStats.FallingStars).RoundToInt();
					starGO.transform.localPosition = new Vector3(((0f - (float)totalWidth) / 2f + 22.5f + (float)(padding * i)).RoundToInt(), 0f, -50 - 10 * i);
				}
				break;
			}
			case StageE.Finish:
				ChestsRoot.localPosition = _chstsRootActive;
				break;
			}
			yield return new WaitForSeconds(_tick);
		}
	}

	private IEnumerator AnimateStars(int starsCount)
	{
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < starsCount; i++)
		{
			float time = 0f;
			SingletonT<SoundManager>.I.PlayGlobalSound("star");
			Sprite star = Stars[i];
			star.transform.localScale = Vector3.zero;
			star.gameObject.SetActiveRecursivelyMk1(setActive: true);
			GameObject fx = (GameObject)Object.Instantiate(FxStarPrefab);
			fx.transform.parent = star.transform.parent;
			fx.transform.localPosition = new Vector3(0f, 0f, -100f);
			while (time < 0.5f)
			{
				star.transform.localScale = Vector3.one * (time / 0.5f);
				time += Time.deltaTime;
				yield return null;
			}
			star.transform.localScale = Vector3.one;
			Object.Destroy(fx);
		}
	}

	private void ResetRating()
	{
		Sprite[] stars = Stars;
		foreach (Sprite sprite in stars)
		{
			sprite.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
	}

	private void TurnOnBonusChest(int i)
	{
		if (i >= 0 && i < BonusChests.Length)
		{
			Sprite sprite = BonusChests[i];
			sprite.ShowOrHide(show: true);
		}
	}

	private void DimBonusChest(int i)
	{
		if (i >= 0 && i < BonusChests.Length)
		{
			Sprite sprite = BonusChests[i];
			sprite.Tint_ = DimBonusChestColor;
		}
	}

	private void ResetAndHideBonusChests()
	{
		Sprite[] bonusChests = BonusChests;
		foreach (Sprite sprite in bonusChests)
		{
			sprite.Tint_ = Color.gray;
			sprite.ShowOrHide(show: false);
		}
	}

	private static int GetSteps(int to)
	{
		if (to < 3)
		{
			return to;
		}
		return Mathf.Min(1 + to / 3, 10);
	}

	private void OnShowBattleResults(GuiRoot.GuiType fromType, GuiRoot.GuiType toType)
	{
		if (_fightResultStats != null || toType != GuiRoot.GuiType.BattleResults || fromType == GuiRoot.GuiType.BattleResults || !(Globals.Battle != null))
		{
			return;
		}
		Globals.InFight = false;
		ClearTexts();
		FightResultStats fightResultStats = Globals.Battle.GenFightResults();
		Messenger.Invoke(Globals.MsgFightResult, fightResultStats);
		Metrics.OnFightFinish(Globals.Enemy.IsDead, fightResultStats.OldLevel, Globals.Enemy.ServerBotInfo, Globals.Enemy.FromLocation, fightResultStats.Fatality, fightResultStats.Turns);
		_fightResultStats = fightResultStats;
		_tick = 0.8f;
		MoneyRoot.SetActiveRecursivelyMk1(setActive: false);
		foreach (KeyValuePair<StageE, GameObject> root in _roots)
		{
			if (root.Value != null)
			{
				root.Value.SetActiveRecursivelyMk1(setActive: false);
			}
		}
		foreach (Transform item in _roots[StageE.FallingStars].transform)
		{
			Object.Destroy(item.gameObject);
		}
		List<StageE> list = new List<StageE>();
		list.Add(StageE.Exp);
		list.Add(StageE.Chests);
		List<StageE> list2 = list;
		if (SingletonT<ServerData>.I.PlayerInBattleParams.BonusMoney > 0)
		{
			list2.Add(StageE.Money);
		}
		if (_fightResultStats.FallingStars > 0)
		{
			list2.Add(StageE.FallingStars);
		}
		list2.Add(StageE.RatingReal);
		list2.Add(StageE.RatingCrits);
		list2.Add(StageE.RatingStars);
		list2.Add(StageE.Finish);
		ResetRating();
		_chestCount = ((_fightResultStats.Fatality != Battle.FatalityStateE.Executed) ? 1 : 2);
		switch (_chestCount)
		{
		case 1:
			BonusChests[0].transform.localPosition = new Vector3(12f, BonusChests[0].transform.localPosition.y, BonusChests[0].transform.localPosition.z);
			break;
		case 2:
			BonusChests[0].transform.localPosition = new Vector3(-20f, BonusChests[0].transform.localPosition.y, BonusChests[0].transform.localPosition.z);
			BonusChests[1].transform.localPosition = new Vector3(40f, BonusChests[1].transform.localPosition.y, BonusChests[1].transform.localPosition.z);
			break;
		}
		StartCoroutine(AnimateStages(list2));
		_bonusLabel.Phrase_ = ServerData.PhrasesE.ChoosePrize;
		VictoryText.SpriteName_ = ((_fightResultStats.Fatality != Battle.FatalityStateE.Executed) ? "you_win" : "fight_execution");
		VictoryText.transform.localPosition = ((_fightResultStats.Fatality != Battle.FatalityStateE.Executed) ? new Vector3(0f, -57f, -50f) : new Vector3(0f, 0f, -50f));
		VictoryText.Tint_ = new Color(0.5f, 0.5f, 0.5f, 1f);
		VictoryIcon.SpriteName_ = ((_fightResultStats.Fatality != Battle.FatalityStateE.Executed) ? "victory_wo_text" : "execution_wo_text");
		VictoryIcon.Tint_ = new Color(0.5f, 0.5f, 0.5f, 1f);
		_nonDropBeginIndex = _chestCount;
		ResetChests();
		StartCoroutine(FadeOutLogo());
	}

	private IEnumerator FadeOutLogo()
	{
		float fadeTime = FadeOutCurve.keys[FadeOutCurve.length - 1].time;
		float time = 0f;
		while (time < fadeTime)
		{
			Color c = new Color(0.5f, 0.5f, 0.5f, FadeOutCurve.Evaluate(time));
			VictoryText.Tint_ = c;
			VictoryIcon.Tint_ = c;
			time += Time.deltaTime;
			yield return null;
		}
		Color c2 = new Color(0.5f, 0.5f, 0.5f, 0f);
		VictoryText.Tint_ = c2;
		VictoryIcon.Tint_ = c2;
	}

	private void ResetChests()
	{
		BattleResultsChestButton[] chests = Chests;
		foreach (BattleResultsChestButton battleResultsChestButton in chests)
		{
			battleResultsChestButton.SetUnselected();
			if (_chestCount > 0)
			{
				battleResultsChestButton.SetActive();
			}
		}
	}

	private string GetMoneySymbol(ServerData.MoneyType type)
	{
		return type.Type switch
		{
			ServerData.MoneyType.TypeE.Gold => Globals.CharIconGold, 
			ServerData.MoneyType.TypeE.Diamond => Globals.CharIconDiamonds, 
			_ => string.Empty, 
		};
	}

	private void ClearTexts()
	{
		Buttons.localPosition = new Vector3(0f, -200f, 0f);
		ChestsRoot.transform.GoToHell();
		DamageStatBonus.Text_ = string.Empty;
		TurnsMoney.Text_ = string.Empty;
		ResetAndHideBonusChests();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}
}
