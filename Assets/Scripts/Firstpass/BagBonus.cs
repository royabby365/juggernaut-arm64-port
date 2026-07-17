using System;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class BagBonus : MonoBehaviour
{
	private const string FormatString = "{0} +{1}";

	private CompositeDisposable _subscriptions;

	public Transform Background;

	public ServerData.Skill.TypeE SkillType;

	public SpriteText SkillCount;

	public Transform AnimationRoot;

	public GameObject IceIco;

	public GameObject FireIco;

	public GameObject LigtningIco;

	public GameObject DarkIco;

	public GameObject ManaIco;

	public GameObject ExpIco;

	public GameObject MoneyIco;

	public GameObject RageIco;

	public GameObject FullRageIco;

	public GameObject FullManaIco;

	private AltButton _alt;

	public void SetBonus(Tuple<ServerData.Skill.TypeE, string, int> bonus)
	{
		SkillType = bonus.Item1;
		bool flag = bonus.Item1 == ServerData.Skill.TypeE.Unknown;
		SkillCount.Text_ = ((!flag) ? "{0} +{1}".Fmt(bonus.Item2, bonus.Item3) : string.Empty);
		Background.ShowOrHide(!flag);
		SetIcon(SkillType);
		SetHint(SkillType);
	}

	private void SetIcon(ServerData.Skill.TypeE bonus)
	{
		foreach (Transform item in AnimationRoot)
		{
			GameObject obj = item.gameObject;
			UnityEngine.Object.Destroy(obj);
		}
		if (bonus != ServerData.Skill.TypeE.Unknown)
		{
			InstaniateAnimation();
		}
	}

	private void InstaniateAnimation()
	{
		GameObject gameObject = null;
		switch (SkillType)
		{
		case ServerData.Skill.TypeE.MagicIce:
			gameObject = IceIco;
			break;
		case ServerData.Skill.TypeE.MagicFire:
			gameObject = FireIco;
			break;
		case ServerData.Skill.TypeE.MagicDark:
			gameObject = DarkIco;
			break;
		case ServerData.Skill.TypeE.MagicElectro:
			gameObject = LigtningIco;
			break;
		case ServerData.Skill.TypeE.BonusRage:
			gameObject = RageIco;
			break;
		case ServerData.Skill.TypeE.BonusMana:
			gameObject = ManaIco;
			break;
		case ServerData.Skill.TypeE.BonusExp:
			gameObject = ExpIco;
			break;
		case ServerData.Skill.TypeE.BonusMoney:
			gameObject = MoneyIco;
			break;
		case ServerData.Skill.TypeE.FullMana:
			gameObject = FullManaIco;
			break;
		case ServerData.Skill.TypeE.FullRage:
			gameObject = FullRageIco;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ServerData.Skill.TypeE.Unknown:
		case ServerData.Skill.TypeE.Strength:
		case ServerData.Skill.TypeE.Vitality:
		case ServerData.Skill.TypeE.Rage:
		case ServerData.Skill.TypeE.Magic:
			break;
		}
		if (!(gameObject == null))
		{
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(gameObject);
			gameObject2.transform.parent = AnimationRoot;
			gameObject2.transform.localPosition = Vector3.zero;
		}
	}

	private void SetHint(ServerData.Skill.TypeE skillType)
	{
		_alt.HintCode = ServerData.HintCodesE.none;
		switch (skillType)
		{
		case ServerData.Skill.TypeE.MagicIce:
			_alt.HintCode = ServerData.HintCodesE.bonusice;
			break;
		case ServerData.Skill.TypeE.MagicFire:
			_alt.HintCode = ServerData.HintCodesE.bonusfire;
			break;
		case ServerData.Skill.TypeE.MagicDark:
			_alt.HintCode = ServerData.HintCodesE.bonusdark;
			break;
		case ServerData.Skill.TypeE.MagicElectro:
			_alt.HintCode = ServerData.HintCodesE.bonuslightning;
			break;
		case ServerData.Skill.TypeE.BonusRage:
			_alt.HintCode = ServerData.HintCodesE.bonusrage;
			break;
		case ServerData.Skill.TypeE.BonusMana:
			_alt.HintCode = ServerData.HintCodesE.bonusmana;
			break;
		case ServerData.Skill.TypeE.BonusExp:
			_alt.HintCode = ServerData.HintCodesE.bonusexp;
			break;
		case ServerData.Skill.TypeE.BonusMoney:
			_alt.HintCode = ServerData.HintCodesE.bonusmoney;
			break;
		case ServerData.Skill.TypeE.FullMana:
			_alt.HintCode = ServerData.HintCodesE.bonusarhimagia;
			break;
		case ServerData.Skill.TypeE.FullRage:
			_alt.HintCode = ServerData.HintCodesE.bonusarhifury;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ServerData.Skill.TypeE.Unknown:
		case ServerData.Skill.TypeE.Strength:
		case ServerData.Skill.TypeE.Vitality:
		case ServerData.Skill.TypeE.Rage:
		case ServerData.Skill.TypeE.Magic:
			break;
		}
		if (_alt.HintCode == ServerData.HintCodesE.none)
		{
			_alt.SetInactive();
		}
		else
		{
			_alt.SetActive();
		}
	}

	private void Awake()
	{
		SkillCount.Phrase_ = ServerData.PhrasesE.Custom;
		_alt = GetComponent<AltButton>();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}
}
