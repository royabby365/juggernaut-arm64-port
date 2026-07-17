using System.ComponentModel;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class SettingsProxy
{
	[ProtoMember(1)]
	public int BerserkLength;

	[ProtoMember(2)]
	public int BerserkDamage;

	[ProtoMember(3)]
	public int BerserkMaxHealth;

	[ProtoMember(4)]
	public int AngerDownSpeed;

	[ProtoMember(5)]
	public int AngerPerBattle;

	[ProtoMember(6)]
	public int AngerPerFatality;

	[ProtoMember(7)]
	public int SlicesForFatality;

	[ProtoMember(8)]
	public int FatalityTime;

	[ProtoMember(9)]
	public int SellPrice;

	[ProtoMember(10)]
	public float ComboK;

	[ProtoMember(11)]
	public float CritK;

	[ProtoMember(12)]
	public float UpdateShopPeriodSeconds;

	[ProtoMember(13)]
	public int LocationMobLevel;

	[ProtoMember(14)]
	[DefaultValue(5)]
	public float TurnTime;

	[ProtoMember(15)]
	public float MobMagicDamageWeakK;

	[ProtoMember(16)]
	public float MobMagicDamageStrongK;

	[ProtoMember(17)]
	public int mgBM1;

	[ProtoMember(18)]
	public int mgBM2;

	[ProtoMember(19)]
	public int mgBM3;

	[ProtoMember(20)]
	public int mgBM4;

	[ProtoMember(21)]
	public int mg2BM1;

	[ProtoMember(22)]
	public int mg2BM2;

	[ProtoMember(23)]
	public int mg2BM3;

	[ProtoMember(24)]
	public int mg2BM4;

	[ProtoMember(25)]
	public int mg2Time;

	[ProtoMember(26)]
	public int ChestPeriod;

	[ProtoMember(27)]
	public int ChestLifeTime;

	[ProtoMember(28)]
	public float TimeTickMagicProtection;

	[ProtoMember(29)]
	public int MagicProtectionTime1;

	[ProtoMember(30)]
	public int MagicProtectionTime2;

	[ProtoMember(31)]
	public int MagicProtectionTime3;

	[ProtoMember(32)]
	public int Crits2StarsPercent;

	[ProtoMember(33)]
	public int Crits3StarsPercent;

	[ProtoMember(34)]
	public int Crits4StarsPercent;

	[ProtoMember(35)]
	public int Crits5StarsPercent;

	[DefaultValue(60)]
	[ProtoMember(36)]
	public int FallingStarCooldown;

	[ProtoMember(37)]
	public int FallingStarProb;

	[ProtoMember(38)]
	[DefaultValue(900)]
	public int FallingStarTime;

	[DefaultValue(2f)]
	[ProtoMember(41)]
	public float MusicChangeTime;

	[ProtoMember(42)]
	[DefaultValue(1)]
	public float IntroFadeInOutTime;

	[DefaultValue(1)]
	[ProtoMember(43)]
	public float IntroReadingTime;

	[ProtoMember(44)]
	[DefaultValue(1)]
	public float IntroDayPassTime;

	[ProtoMember(45)]
	public int ChestsMaxCount;

	[ProtoMember(46)]
	public int ProbFatality;

	[ProtoMember(47)]
	[DefaultValue(2)]
	public int CritOnPersonManaBallsCount;

	[ProtoMember(48)]
	public int RageShereProbSimple;

	[ProtoMember(49)]
	public int RageShereProbCrit;

	[ProtoMember(50)]
	public int EyePeriod;

	[ProtoMember(51)]
	public int EyeProb1;

	[ProtoMember(52)]
	public int EyeProb2;

	[ProtoMember(53)]
	public int EyeProb3;

	[ProtoMember(54)]
	public int manaInBall;

	[ProtoMember(55)]
	public int critMBCount;

	[ProtoMember(56)]
	public int critMBCountFromRages;

	[ProtoMember(57)]
	public int manaOCount;

	[ProtoMember(58)]
	public int manaOProb;

	[ProtoMember(59)]
	public int manaOInitProb;

	[ProtoMember(60)]
	public int manaBonusProb;

	[ProtoMember(61)]
	public int rageBonusProb;

	[ProtoMember(62)]
	public float czRadius;

	[ProtoMember(63)]
	[DefaultValue(1.8f)]
	public float czInterpolationSpeed;

	[ProtoMember(64)]
	[DefaultValue(1f)]
	public float MagicRes;

	[DefaultValue(5f)]
	[ProtoMember(65)]
	public float IdlePeriod;

	[ProtoMember(66)]
	[DefaultValue(50)]
	public int BloodScreenHealthTreshold;

	[ProtoMember(67)]
	[DefaultValue(50)]
	public int BloodScreenHealthTreshold2;

	[ProtoMember(68)]
	[DefaultValue(50)]
	public int BloodScreenHealthTreshold3;

	[ProtoMember(69)]
	[DefaultValue(4)]
	public int ComboManaBalls;

	[ProtoMember(70)]
	[DefaultValue(12)]
	public string DefaultRedWeapon;

	[ProtoMember(71)]
	[DefaultValue(12)]
	public string DefaultGreenWeapon;

	[DefaultValue(12)]
	[ProtoMember(72)]
	public string DefaultBlueWeapon;

	[ProtoMember(73)]
	public ServerData.Fatality DefaultFatality;

	[DefaultValue("http://juggermobile.com/")]
	[ProtoMember(74)]
	public string TeaserPage;

	[ProtoMember(75)]
	[DefaultValue("http://juggermobile.com/faq.php")]
	public string FaqPage;

	[ProtoMember(76)]
	public BotLocationInfoProxy[] Elfs;

	[ProtoMember(77)]
	public int ElfPeriod;

	[ProtoMember(78)]
	public int ElfProb;

	[ProtoMember(79)]
	[DefaultValue(50)]
	public int Idle2Prob;

	[ProtoMember(80)]
	public BonusProxy ElfBonus;

	[ProtoMember(81)]
	public BonusProxy MonsterFromLocationBonus;

	[ProtoMember(82)]
	[DefaultValue(30)]
	public int AchievmentSharingMoneyBonus;

	[ProtoMember(83)]
	[DefaultValue(360)]
	public int RatingSocialCulldown;

	[ProtoMember(84)]
	[DefaultValue(1)]
	public int RatingSharingMoneyBonus;

	[ProtoMember(85)]
	[DefaultValue(7)]
	public int LevelCheckNotifs;

	[ProtoMember(86)]
	[DefaultValue(2)]
	public int SkillComboLevel;

	[ProtoMember(87)]
	[DefaultValue(3)]
	public int SkillMagicLevel;

	[ProtoMember(88)]
	[DefaultValue(4)]
	public int SkillRageLevel;

	[ProtoMember(90)]
	public ServerData.MobRespProb[] MobsRespawnProbs;

	[ProtoMember(91)]
	[DefaultValue(60)]
	public int MobsRespCooldown;

	[ProtoMember(92)]
	public BonusProxy Match3BonusNormal;

	[ProtoMember(93)]
	public BonusProxy Match3BonusNormalFirstMove;

	[ProtoMember(94)]
	public BonusProxy Match3BonusEasy;

	[ProtoMember(95)]
	public BonusProxy Match3BonusEasyFirstMove;

	[ProtoMember(96)]
	public int Match3TimerNormal;

	[ProtoMember(97)]
	public int Match3TimerEasy;

	[ProtoMember(98)]
	public int Match3TimeKeysCostNormal;

	[ProtoMember(99)]
	public int Match3TimeKeysCostEasy;

	[ProtoMember(100)]
	public int Match3BonusProbNormal;

	[ProtoMember(101)]
	public int Match3BonusProbEasy;

	[ProtoMember(102)]
	public bool ShowFps;

	[ProtoMember(103)]
	public int LocationMobLevelMax;

	[ProtoMember(104)]
	public int LocationMobLevelOffset;

	public static implicit operator ServerData.Settings(SettingsProxy data)
	{
		ServerData.Settings settings = new ServerData.Settings();
		settings.BerserkLength = data.BerserkLength;
		settings.BerserkDamage = data.BerserkDamage;
		settings.BerserkMaxHealth = data.BerserkMaxHealth;
		settings.AngerDownSpeed = data.AngerDownSpeed;
		settings.AngerPerBattle = data.AngerPerBattle;
		settings.AngerPerFatality = data.AngerPerFatality;
		settings.SlicesForFatality = data.SlicesForFatality;
		settings.FatalityTime = data.FatalityTime;
		settings.SellPrice = data.SellPrice;
		settings.ComboK = data.ComboK;
		settings.CritK = data.CritK;
		settings.UpdateShopPeriodSeconds = data.UpdateShopPeriodSeconds;
		settings.LocationMobLevel = data.LocationMobLevel;
		settings.TurnTime = data.TurnTime;
		settings.MobMagicDamageWeakK = data.MobMagicDamageWeakK;
		settings.MobMagicDamageStrongK = data.MobMagicDamageStrongK;
		settings.mgBM1 = data.mgBM1;
		settings.mgBM2 = data.mgBM2;
		settings.mgBM3 = data.mgBM3;
		settings.mgBM4 = data.mgBM4;
		settings.mg2BM1 = data.mg2BM1;
		settings.mg2BM2 = data.mg2BM2;
		settings.mg2BM3 = data.mg2BM3;
		settings.mg2BM4 = data.mg2BM4;
		settings.mg2Time = data.mg2Time;
		settings.ChestPeriod = data.ChestPeriod;
		settings.ChestLifeTime = data.ChestLifeTime;
		settings.TimeTickMagicProtection = data.TimeTickMagicProtection;
		settings.MagicProtectionTime1 = data.MagicProtectionTime1;
		settings.MagicProtectionTime2 = data.MagicProtectionTime2;
		settings.MagicProtectionTime3 = data.MagicProtectionTime3;
		settings.Crits2StarsPercent = data.Crits2StarsPercent;
		settings.Crits3StarsPercent = data.Crits3StarsPercent;
		settings.Crits4StarsPercent = data.Crits4StarsPercent;
		settings.Crits5StarsPercent = data.Crits5StarsPercent;
		settings.FallingStarCooldown = data.FallingStarCooldown;
		settings.FallingStarProb = data.FallingStarProb;
		settings.FallingStarTime = data.FallingStarTime;
		settings.MusicChangeTime = data.MusicChangeTime;
		settings.IntroFadeInOutTime = data.IntroFadeInOutTime;
		settings.IntroReadingTime = data.IntroReadingTime;
		settings.IntroDayPassTime = data.IntroDayPassTime;
		settings.ChestsMaxCount = data.ChestsMaxCount;
		settings.ProbFatality = data.ProbFatality;
		settings.CritOnPersonManaBallsCount = data.CritOnPersonManaBallsCount;
		settings.RageShereProbSimple = data.RageShereProbSimple;
		settings.RageShereProbCrit = data.RageShereProbCrit;
		settings.EyePeriod = data.EyePeriod;
		settings.EyeProb1 = data.EyeProb1;
		settings.EyeProb2 = data.EyeProb2;
		settings.EyeProb3 = data.EyeProb3;
		settings.manaInBall = data.manaInBall;
		settings.critMBCount = data.critMBCount;
		settings.critMBCountFromRages = data.critMBCountFromRages;
		settings.manaOCount = data.manaOCount;
		settings.manaOProb = data.manaOProb;
		settings.manaOInitProb = data.manaOInitProb;
		settings.manaBonusProb = data.manaBonusProb;
		settings.rageBonusProb = data.rageBonusProb;
		settings.czRadius = data.czRadius;
		settings.czInterpolationSpeed = data.czInterpolationSpeed;
		settings.MagicRes = data.MagicRes;
		settings.IdlePeriod = data.IdlePeriod;
		settings.BloodScreenHealthTreshold = data.BloodScreenHealthTreshold;
		settings.BloodScreenHealthTreshold2 = data.BloodScreenHealthTreshold2;
		settings.BloodScreenHealthTreshold3 = data.BloodScreenHealthTreshold3;
		settings.ComboManaBalls = data.ComboManaBalls;
		settings.DefaultRedWeapon = data.DefaultRedWeapon;
		settings.DefaultGreenWeapon = data.DefaultGreenWeapon;
		settings.DefaultBlueWeapon = data.DefaultBlueWeapon;
		settings.DefaultFatality = data.DefaultFatality;
		settings.TeaserPage = data.TeaserPage;
		settings.FaqPage = data.FaqPage;
		settings.Elfs = data.Elfs.FromProxy();
		settings.ElfPeriod = data.ElfPeriod;
		settings.ElfProb = data.ElfProb;
		settings.Idle2Prob = data.Idle2Prob;
		settings.ElfBonus = data.ElfBonus;
		settings.MonsterFromLocationBonus = data.MonsterFromLocationBonus;
		settings.AchievmentSharingMoneyBonus = data.AchievmentSharingMoneyBonus;
		settings.RatingSocialCulldown = data.RatingSocialCulldown;
		settings.RatingSharingMoneyBonus = data.RatingSharingMoneyBonus;
		settings.LevelCheckNotifs = data.LevelCheckNotifs;
		settings.SkillComboLevel = data.SkillComboLevel;
		settings.SkillMagicLevel = data.SkillMagicLevel;
		settings.SkillRageLevel = data.SkillRageLevel;
		settings.MobsRespawnProbs = data.MobsRespawnProbs;
		settings.MobsRespCooldown = data.MobsRespCooldown;
		settings.Match3TimerNormal = data.Match3TimerNormal;
		settings.Match3TimerEasy = data.Match3TimerEasy;
		settings.Match3TimeKeysCostNormal = data.Match3TimeKeysCostNormal;
		settings.Match3TimeKeysCostEasy = data.Match3TimeKeysCostEasy;
		settings.Match3BonusProbNormal = data.Match3BonusProbNormal;
		settings.Match3BonusProbEasy = data.Match3BonusProbEasy;
		settings.ShowFps = data.ShowFps;
		settings.LocationMobLevelOffset = data.LocationMobLevelOffset;
		settings.LocationMobLevelMax = data.LocationMobLevelMax;
		return settings;
	}

	public static implicit operator SettingsProxy(ServerData.Settings data)
	{
		SettingsProxy settingsProxy = new SettingsProxy();
		settingsProxy.BerserkLength = data.BerserkLength;
		settingsProxy.BerserkDamage = data.BerserkDamage;
		settingsProxy.BerserkMaxHealth = data.BerserkMaxHealth;
		settingsProxy.AngerDownSpeed = data.AngerDownSpeed;
		settingsProxy.AngerPerBattle = data.AngerPerBattle;
		settingsProxy.AngerPerFatality = data.AngerPerFatality;
		settingsProxy.SlicesForFatality = data.SlicesForFatality;
		settingsProxy.FatalityTime = data.FatalityTime;
		settingsProxy.SellPrice = data.SellPrice;
		settingsProxy.ComboK = data.ComboK;
		settingsProxy.CritK = data.CritK;
		settingsProxy.UpdateShopPeriodSeconds = data.UpdateShopPeriodSeconds;
		settingsProxy.LocationMobLevel = data.LocationMobLevel;
		settingsProxy.TurnTime = data.TurnTime;
		settingsProxy.MobMagicDamageWeakK = data.MobMagicDamageWeakK;
		settingsProxy.MobMagicDamageStrongK = data.MobMagicDamageStrongK;
		settingsProxy.mgBM1 = data.mgBM1;
		settingsProxy.mgBM2 = data.mgBM2;
		settingsProxy.mgBM3 = data.mgBM3;
		settingsProxy.mgBM4 = data.mgBM4;
		settingsProxy.mg2BM1 = data.mg2BM1;
		settingsProxy.mg2BM2 = data.mg2BM2;
		settingsProxy.mg2BM3 = data.mg2BM3;
		settingsProxy.mg2BM4 = data.mg2BM4;
		settingsProxy.mg2Time = data.mg2Time;
		settingsProxy.ChestPeriod = data.ChestPeriod;
		settingsProxy.ChestLifeTime = data.ChestLifeTime;
		settingsProxy.TimeTickMagicProtection = data.TimeTickMagicProtection;
		settingsProxy.MagicProtectionTime1 = data.MagicProtectionTime1;
		settingsProxy.MagicProtectionTime2 = data.MagicProtectionTime2;
		settingsProxy.MagicProtectionTime3 = data.MagicProtectionTime3;
		settingsProxy.Crits2StarsPercent = data.Crits2StarsPercent;
		settingsProxy.Crits3StarsPercent = data.Crits3StarsPercent;
		settingsProxy.Crits4StarsPercent = data.Crits4StarsPercent;
		settingsProxy.Crits5StarsPercent = data.Crits5StarsPercent;
		settingsProxy.FallingStarCooldown = data.FallingStarCooldown;
		settingsProxy.FallingStarProb = data.FallingStarProb;
		settingsProxy.FallingStarTime = data.FallingStarTime;
		settingsProxy.MusicChangeTime = data.MusicChangeTime;
		settingsProxy.IntroFadeInOutTime = data.IntroFadeInOutTime;
		settingsProxy.IntroReadingTime = data.IntroReadingTime;
		settingsProxy.IntroDayPassTime = data.IntroDayPassTime;
		settingsProxy.ChestsMaxCount = data.ChestsMaxCount;
		settingsProxy.ProbFatality = data.ProbFatality;
		settingsProxy.CritOnPersonManaBallsCount = data.CritOnPersonManaBallsCount;
		settingsProxy.RageShereProbSimple = data.RageShereProbSimple;
		settingsProxy.RageShereProbCrit = data.RageShereProbCrit;
		settingsProxy.EyePeriod = data.EyePeriod;
		settingsProxy.EyeProb1 = data.EyeProb1;
		settingsProxy.EyeProb2 = data.EyeProb2;
		settingsProxy.EyeProb3 = data.EyeProb3;
		settingsProxy.manaInBall = data.manaInBall;
		settingsProxy.critMBCount = data.critMBCount;
		settingsProxy.critMBCountFromRages = data.critMBCountFromRages;
		settingsProxy.manaOCount = data.manaOCount;
		settingsProxy.manaOProb = data.manaOProb;
		settingsProxy.manaOInitProb = data.manaOInitProb;
		settingsProxy.manaBonusProb = data.manaBonusProb;
		settingsProxy.rageBonusProb = data.rageBonusProb;
		settingsProxy.czRadius = data.czRadius;
		settingsProxy.czInterpolationSpeed = data.czInterpolationSpeed;
		settingsProxy.MagicRes = data.MagicRes;
		settingsProxy.IdlePeriod = data.IdlePeriod;
		settingsProxy.BloodScreenHealthTreshold = data.BloodScreenHealthTreshold;
		settingsProxy.BloodScreenHealthTreshold2 = data.BloodScreenHealthTreshold2;
		settingsProxy.BloodScreenHealthTreshold3 = data.BloodScreenHealthTreshold3;
		settingsProxy.ComboManaBalls = data.ComboManaBalls;
		settingsProxy.DefaultRedWeapon = data.DefaultRedWeapon;
		settingsProxy.DefaultGreenWeapon = data.DefaultGreenWeapon;
		settingsProxy.DefaultBlueWeapon = data.DefaultBlueWeapon;
		settingsProxy.DefaultFatality = data.DefaultFatality;
		settingsProxy.TeaserPage = data.TeaserPage;
		settingsProxy.FaqPage = data.FaqPage;
		settingsProxy.Elfs = data.Elfs.ToProxy();
		settingsProxy.ElfPeriod = data.ElfPeriod;
		settingsProxy.ElfProb = data.ElfProb;
		settingsProxy.Idle2Prob = data.Idle2Prob;
		settingsProxy.ElfBonus = data.ElfBonus;
		settingsProxy.MonsterFromLocationBonus = data.MonsterFromLocationBonus;
		settingsProxy.AchievmentSharingMoneyBonus = data.AchievmentSharingMoneyBonus;
		settingsProxy.RatingSocialCulldown = data.RatingSocialCulldown;
		settingsProxy.RatingSharingMoneyBonus = data.RatingSharingMoneyBonus;
		settingsProxy.LevelCheckNotifs = data.LevelCheckNotifs;
		settingsProxy.SkillComboLevel = data.SkillComboLevel;
		settingsProxy.SkillMagicLevel = data.SkillMagicLevel;
		settingsProxy.SkillRageLevel = data.SkillRageLevel;
		settingsProxy.MobsRespawnProbs = data.MobsRespawnProbs;
		settingsProxy.MobsRespCooldown = data.MobsRespCooldown;
		settingsProxy.Match3TimerNormal = data.Match3TimerNormal;
		settingsProxy.Match3TimerEasy = data.Match3TimerEasy;
		settingsProxy.Match3TimeKeysCostNormal = data.Match3TimeKeysCostNormal;
		settingsProxy.Match3TimeKeysCostEasy = data.Match3TimeKeysCostEasy;
		settingsProxy.Match3BonusProbNormal = data.Match3BonusProbNormal;
		settingsProxy.Match3BonusProbEasy = data.Match3BonusProbEasy;
		settingsProxy.ShowFps = data.ShowFps;
		settingsProxy.LocationMobLevelOffset = data.LocationMobLevelOffset;
		settingsProxy.LocationMobLevelMax = data.LocationMobLevelMax;
		return settingsProxy;
	}
}
