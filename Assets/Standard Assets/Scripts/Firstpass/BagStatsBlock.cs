using UnityEngine;
using Yarx;
using Yarx.Collections;

public class BagStatsBlock : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText PointsAvailable;

	public BagStat[] Stats;

	private string _pointsLeftFmt;

	private void Awake()
	{
		_pointsLeftFmt = SingletonT<ServerData>.I.GetPhrase(PointsAvailable.Phrase_) ?? "<???> {0} %";
		PointsAvailable.Phrase_ = ServerData.PhrasesE.Custom;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger.AddListener(Globals.MsgPlayerSkillChanged, MySkillChanged));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgPlayerSkillPointsChanged, MySkillPointsChanged));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnGuiSwitch));
	}

	private void OnGuiSwitch(GuiRoot.GuiType fromGui, GuiRoot.GuiType toGui)
	{
		if (toGui != GuiRoot.GuiType.BagStats)
		{
			CommitStats();
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		SpriteGui spriteGui = base.transform.GetSpriteGui();
		if (spriteGui != null)
		{
			spriteGui.Release += ProcessStatsButton;
		}
	}

	private void ProcessStatsButton(SpriteButton obj)
	{
		BagStat bagStat = null;
		switch (obj.name)
		{
		case "plus_health":
			bagStat = GetStat(ServerData.Skill.TypeE.Vitality);
			break;
		case "plus_magic":
			bagStat = GetStat(ServerData.Skill.TypeE.Magic);
			break;
		case "plus_strength":
			bagStat = GetStat(ServerData.Skill.TypeE.Strength);
			break;
		case "plus_rage":
			bagStat = GetStat(ServerData.Skill.TypeE.Rage);
			break;
		}
		if (bagStat != null)
		{
			int playerSkillPoints = Extensions.GetPlayerSkillPoints();
			if (playerSkillPoints > 0)
			{
				bagStat.AddToSkillCount++;
				bagStat.SkillType.AddToSkill(1);
				Messenger.Invoke(Globals.MsgPlayerSkillChanged);
				Extensions.SetPlayerSkillPoints(playerSkillPoints - 1);
				Messenger.Invoke(Globals.MsgPlayerSkillPointsChanged);
			}
		}
	}

	private void MySkillPointsChanged()
	{
		int playerSkillPoints = Extensions.GetPlayerSkillPoints();
		BagStat[] stats = Stats;
		foreach (BagStat bagStat in stats)
		{
			if (playerSkillPoints <= 0)
			{
				bagStat.SetInactive();
			}
			else
			{
				bagStat.SetActive();
			}
		}
		int playerSkillPoints2 = Extensions.GetPlayerSkillPoints();
		PointsAvailable.Text_ = _pointsLeftFmt.Fmt(playerSkillPoints2);
		PointsAvailable.ShowOrHide(playerSkillPoints2 > 0);
	}

	private BagStat GetStat(ServerData.Skill.TypeE type)
	{
		BagStat[] stats = Stats;
		foreach (BagStat bagStat in stats)
		{
			if (bagStat.SkillType == type)
			{
				return bagStat;
			}
		}
		return null;
	}

	private void MySkillChanged()
	{
		BagStat[] stats = Stats;
		foreach (BagStat bagStat in stats)
		{
			System.Tuple<int, int> playerSkill = bagStat.SkillType.GetPlayerSkill();
			bagStat.SkillCount.Text_ = (playerSkill.Item1 + playerSkill.Item2).ToString();
		}
	}

	private void CommitStats()
	{
		BagStat[] stats = Stats;
		foreach (BagStat bagStat in stats)
		{
			bagStat.AddToSkillCount = 0;
		}
	}
}
