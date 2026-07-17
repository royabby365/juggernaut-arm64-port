using UnityEngine;

internal class AreaData
{
	public sealed class MobData
	{
		public int MaxHealth = 1;

		public int Strength = 1;

		public int Rage = 1;

		public int Magic = 1;

		public int Level;

		public bool Fire;

		public bool Lighting;

		public bool Darkness;

		public bool Ice;

		public ServerData.BotInfo ServerInfo;

		public bool IsBoss;

		public ServerData.BotLevel LevelData;

		public string Reason = string.Empty;

		internal bool FromLocation;

		public MobData(ServerData.BotInfo info, int level, bool isBoss, string reason, bool fromLocation)
		{
			FromLocation = fromLocation;
			Reason = reason;
			ServerInfo = info;
			Fire = info.HasMagicImmunity(MagicTypeE.Fire);
			Lighting = info.HasMagicImmunity(MagicTypeE.Lighting);
			Darkness = info.HasMagicImmunity(MagicTypeE.Darkness);
			Ice = info.HasMagicImmunity(MagicTypeE.Ice);
			SetLevel(level, isBoss);
		}

		internal void SetParamsToEnemy()
		{
			ServerData.MobParamsData enemyParams = SingletonT<ServerData>.I.EnemyParams;
			if (ServerInfo.Id == MainMenu.SovereighnMobId)
			{
				int num = enemyParams.Level;
				int liveShamansCount = SingletonT<ServerData>.I.LiveShamansCount;
				if (liveShamansCount == 0 && SingletonT<ServerData>.I.GameSettings.sovLevS0 > 0)
				{
					num = SingletonT<ServerData>.I.GameSettings.sovLevS0;
				}
				else if (liveShamansCount == 1 && SingletonT<ServerData>.I.GameSettings.sovLevS1 > 0)
				{
					num = SingletonT<ServerData>.I.GameSettings.sovLevS1;
				}
				else if (liveShamansCount == 2 && SingletonT<ServerData>.I.GameSettings.sovLevS2 > 0)
				{
					num = SingletonT<ServerData>.I.GameSettings.sovLevS2;
				}
				if (enemyParams.Level != num)
				{
					SetLevel(num, IsBoss);
				}
				enemyParams.Level = num;
				Utils.LogForce("@@@@@@@@@@@@@@@@", num, liveShamansCount, SingletonT<ServerData>.I.GameSettings.sovLevS0, SingletonT<ServerData>.I.GameSettings.sovLevS1, SingletonT<ServerData>.I.GameSettings.sovLevS2);
			}
			enemyParams.HP = MaxHealth;
			enemyParams.Rage = Rage;
			enemyParams.Strength = Strength;
			enemyParams.Magic = Magic;
			enemyParams.Level = Level;
			enemyParams.MobData = this;
		}

		public override string ToString()
		{
			return "<{0} {1} {2} {3}>".Fmt(IsBoss, ServerInfo, LevelData, FromLocation);
		}

		private void SetLevel(int level, bool isBoss)
		{
			Level = level;
			IsBoss = isBoss;
			LevelData = SingletonT<ServerData>.I.GetBotLevel(this);
			ServerData.SkillInfo skill = LevelData.GetSkill(ServerData.Skill.TypeE.Vitality);
			ServerData.SkillInfo skill2 = LevelData.GetSkill(ServerData.Skill.TypeE.Strength);
			ServerData.SkillInfo skill3 = LevelData.GetSkill(ServerData.Skill.TypeE.Rage);
			ServerData.SkillInfo skill4 = LevelData.GetSkill(ServerData.Skill.TypeE.Magic);
			MaxHealth = Random.Range(skill.Min, skill.Max);
			Strength = Random.Range(skill2.Min, skill2.Max);
			Rage = Random.Range(skill3.Min, skill3.Max);
			if (skill4 != null)
			{
				Magic = Random.Range(skill4.Min, skill4.Max);
			}
		}
	}

	public readonly MobData[] Mobs;

	public ServerData.Location Location;

	private static AreaData _Current;

	public static AreaData Current
	{
		get
		{
			return _Current;
		}
		set
		{
			_Current = value;
			Utils.Log("****CURRENT", value.Location.Title);
		}
	}

	public AreaData(ServerData.Location location, MobData[] mobs)
	{
		Mobs = mobs;
		Location = location;
	}

	public static void MakeCurrent(ServerData.Location location, bool zachistka)
	{
		if (zachistka)
		{
			Utils.LogForce("MAKECURRENT", location);
			MobData[] array = new MobData[location.Bots.Length];
			for (int i = 0; i < array.Length; i++)
			{
				ServerData.Location.BotLocationInfo botLocationInfo = location.Bots[i];
				Invs.Inv(botLocationInfo.Bot != null, "c.Bot!=null", i);
				array[i] = new MobData(botLocationInfo.Bot, botLocationInfo.Level, botLocationInfo.IsBoss, "MakeCurrent", fromLocation: false);
			}
			Current = new AreaData(location, array);
		}
		else
		{
			Utils.LogForce("MAKECURRENTLOCATION", location);
			Current = new AreaData(location, null);
		}
	}
}
