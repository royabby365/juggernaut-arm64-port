using System.Collections.Generic;

public class FightResultStats
{
	public int Damage;

	internal Battle.FatalityStateE Fatality;

	public int Turns;

	public int Crits;

	public int AddedExperience;

	public int AddedAnger;

	public bool LevelWasChanged;

	public int OldLevel;

	public int FightRating;

	public int FallingStars;

	public List<ServerData.Bonus.DropElement> ChestBonuses { get; set; }

	public KeyValuePair<ServerData.Skill, int> DamageBonus { get; set; }

	public KeyValuePair<ServerData.Skill, int> ExecutionBonus { get; set; }
}
