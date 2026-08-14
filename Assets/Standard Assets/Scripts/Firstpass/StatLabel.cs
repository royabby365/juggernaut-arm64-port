using System;
using UnityEngine;

public class StatLabel : MonoBehaviour
{
	public string Blocklabel;

	public int StartStat = 100;

	public SpriteText Banner;

	public SpriteText Stat;

	public SpriteText ToAdd;

	public SpriteButton plus;

	public SpriteButton minus;

	public ServerData.Skill.TypeE skillType;

	private int _stat;

	private int _toadd;

	private int _addToStatFromItems;

	private void Awake()
	{
		_stat = StartStat;
	}

	private void Start()
	{
		Banner.Text_ = Blocklabel;
	}

	public void SetStat(int amount)
	{
		_stat = amount;
		RefreshStat();
	}

	public void SetPlusMinus()
	{
		int skillPoints = SingletonT<ServerData>.I.PlayerParams._skillPoints;
		ServerData.Skill skill = SingletonT<ServerData>.I.GetSkill(skillType);
		if (skillPoints < skill.SkillPoint)
		{
			plus.SetInactive();
		}
		else
		{
			plus.SetActive();
		}
		if (_toadd <= 0)
		{
			minus.SetInactive();
		}
		else
		{
			minus.SetActive();
		}
		ToAdd.Text_ = ((_toadd >= 0) ? "+" : "-") + _toadd;
	}

	public void AddToStat(int toadd)
	{
		_addToStatFromItems = toadd;
		RefreshStat();
	}

	private void RefreshStat()
	{
		Stat.Text_ = (_stat + _addToStatFromItems).ToString();
	}

	public void AddOne()
	{
		int skillPoints = SingletonT<ServerData>.I.PlayerParams._skillPoints;
		ServerData.Skill skill = SingletonT<ServerData>.I.GetSkill(skillType);
		if (skillPoints >= skill.SkillPoint)
		{
			_toadd++;
			SingletonT<ServerData>.I.PlayerParams._skillPoints -= skill.SkillPoint;
		}
	}

	public void MinusOne()
	{
		ServerData.Skill skill = SingletonT<ServerData>.I.GetSkill(skillType);
		if (_toadd > 0)
		{
			_toadd--;
			SingletonT<ServerData>.I.PlayerParams._skillPoints += skill.SkillPoint;
		}
	}

	public void Commit()
	{
		switch (skillType)
		{
		case ServerData.Skill.TypeE.Unknown:
			break;
		case ServerData.Skill.TypeE.Strength:
			SingletonT<ServerData>.I.PlayerParams.Strength += _toadd;
			break;
		case ServerData.Skill.TypeE.Vitality:
			SingletonT<ServerData>.I.PlayerParams.HP += _toadd;
			break;
		case ServerData.Skill.TypeE.Rage:
			SingletonT<ServerData>.I.PlayerParams.Rage += _toadd;
			break;
		case ServerData.Skill.TypeE.Magic:
			SingletonT<ServerData>.I.PlayerParams.Magic += _toadd;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
