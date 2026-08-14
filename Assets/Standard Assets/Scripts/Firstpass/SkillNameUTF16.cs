public static class SkillNameUTF16
{
	public static string AsString(this ServerData.Skill skill)
	{
		switch (skill.Type)
		{
		case ServerData.Skill.TypeE.Strength:
		case ServerData.Skill.TypeE.Vitality:
		case ServerData.Skill.TypeE.Rage:
		case ServerData.Skill.TypeE.Magic:
		case ServerData.Skill.TypeE.BonusRage:
		case ServerData.Skill.TypeE.BonusMana:
		case ServerData.Skill.TypeE.BonusExp:
		case ServerData.Skill.TypeE.BonusMoney:
			return SingletonT<ServerData>.I.GetSkill(skill.Type).Title;
		case ServerData.Skill.TypeE.Unknown:
			return "Неизвестно";
		default:
			return "Невозможно!";
		}
	}
}
