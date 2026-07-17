using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Yarx.Collections;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class ServerDataData
{
	[ProtoMember(1)]
	public int Version { get; set; }

	[ProtoMember(10)]
	public List<AchievementProxy> AchievementsData { get; set; }

	[ProtoMember(20)]
	public List<ServerData.BankItem> BankItems { get; set; }

	[ProtoMember(30)]
	public ServerData.BattleParams BattleMathParamsData { get; set; }

	[ProtoMember(40)]
	public List<BonusProxy> BonusesData { get; set; }

	[ProtoMember(50)]
	public List<BotLevelProxy> BossLevelsData { get; set; }

	[ProtoMember(60)]
	public List<BotLevelProxy> BotLevelsData { get; set; }

	[ProtoMember(70)]
	public List<BotInfoProxy> Bots { get; set; }

	[ProtoMember(80)]
	public List<ChestProxy> ChestsData { get; set; }

	[ProtoMember(90)]
	public List<ServerData.ServerColor> ColorsData { get; set; }

	[ProtoMember(100)]
	public List<ConditionProxy> ConditionsData { get; set; }

	[ProtoMember(110)]
	public List<StorylineDialogProxy> StorylineDialogsData { get; set; }

	[ProtoMember(120)]
	public List<ServerData.Fatality> FatalitiesData { get; set; }

	[ProtoMember(130)]
	public Dictionary<string, ServerData.HintData> HintsData { get; set; }

	[ProtoMember(140)]
	public List<ItemProxy> ItemsData { get; set; }

	[ProtoMember(150)]
	public List<ServerData.LevelData> LevelsData { get; set; }

	[ProtoMember(160)]
	public List<string> LoadingTipsData { get; set; }

	[ProtoMember(170)]
	public List<LocationProxy> LocationsData { get; set; }

	[ProtoMember(180)]
	public List<ServerData.MoneyType> MoneyTypesData { get; set; }

	[ProtoMember(190)]
	public List<ServerData.Npc> NpcsData { get; set; }

	[ProtoMember(200)]
	public List<PersDataProxy> PersDataData { get; set; }

	[ProtoMember(210)]
	public Dictionary<string, string> PhrasesData { get; set; }

	[ProtoMember(220)]
	public SettingsProxy SettingsData { get; set; }

	[ProtoMember(230)]
	public Dictionary<string, ServerData.Skill> SkillsData { get; set; }

	[ProtoMember(240)]
	public List<ServerData.Slot> SlotsData { get; set; }

	[ProtoMember(250)]
	public List<SpellProxy> SpellsData { get; set; }

	[ProtoMember(260)]
	public List<ShopGoodProxy> ShopGoodsData { get; set; }

	[ProtoMember(270)]
	public List<ServerData.Subtitle> SubtitlesData { get; set; }

	public ServerDataData()
	{
	}

	public ServerDataData(ServerData data)
	{
		Version = data.Version;
		AchievementsData = ((IEnumerable<ServerData.Achievement>)data._achievements.Values).Select((Func<ServerData.Achievement, AchievementProxy>)((ServerData.Achievement d) => d)).ToList();
		BankItems = data._bankItems;
		BattleMathParamsData = data.BattleMathParams;
		BonusesData = ((IEnumerable<ServerData.Bonus>)data._bonuses.Values).Select((Func<ServerData.Bonus, BonusProxy>)((ServerData.Bonus d) => d)).ToList();
		BossLevelsData = ((IEnumerable<ServerData.BotLevel>)data._bossLevels.Values).Select((Func<ServerData.BotLevel, BotLevelProxy>)((ServerData.BotLevel d) => d)).ToList();
		BotLevelsData = ((IEnumerable<ServerData.BotLevel>)data._botLevels.Values).Select((Func<ServerData.BotLevel, BotLevelProxy>)((ServerData.BotLevel d) => d)).ToList();
		Bots = ((IEnumerable<ServerData.BotInfo>)data._bots.Values).Select((Func<ServerData.BotInfo, BotInfoProxy>)((ServerData.BotInfo d) => d)).ToList();
		ChestsData = ((IEnumerable<ServerData.Chest>)data.Chests.Values).Select((Func<ServerData.Chest, ChestProxy>)((ServerData.Chest d) => d)).ToList();
		ColorsData = data.Colors;
		ConditionsData = ((IEnumerable<ServerData.Condition>)data._conditions.Values).Select((Func<ServerData.Condition, ConditionProxy>)((ServerData.Condition d) => d)).ToList();
		StorylineDialogsData = ((IEnumerable<ServerData.StorylineDialog>)data._storylineDialogs.Values).Select((Func<ServerData.StorylineDialog, StorylineDialogProxy>)((ServerData.StorylineDialog d) => d)).ToList();
		FatalitiesData = data._fatalities.Values.ToList();
		HintsData = data._hints;
		ItemsData = ((IEnumerable<ServerData.Item>)data._items.Values).Select((Func<ServerData.Item, ItemProxy>)((ServerData.Item d) => d)).ToList();
		LevelsData = data._levels.Values.ToList();
		LoadingTipsData = data.LoadingTips;
		LocationsData = ((IEnumerable<ServerData.Location>)data._locations.Values).Select((Func<ServerData.Location, LocationProxy>)((ServerData.Location d) => d)).ToList();
		MoneyTypesData = data._moneyTypes.Values.ToList();
		NpcsData = data._npcs.Values.ToList();
		PersDataData = ((IEnumerable<ServerData.PersData>)data._persData).Select((Func<ServerData.PersData, PersDataProxy>)((ServerData.PersData d) => d)).ToList();
		PhrasesData = data._phrases;
		SettingsData = data.GameSettings;
		SkillsData = data._skills;
		SlotsData = data._slots.Values.ToList();
		SpellsData = ((IEnumerable<ServerData.Spell>)data._spells.Values).Select((Func<ServerData.Spell, SpellProxy>)((ServerData.Spell d) => d)).ToList();
		ShopGoodsData = ((IEnumerable<ServerData.ShopGood>)data._shopGoods.Values).Select((Func<ServerData.ShopGood, ShopGoodProxy>)((ServerData.ShopGood d) => d)).ToList();
		SubtitlesData = data._subtitles.Values.ToList();
	}

	public static ServerData CreateServerData(ServerDataData data)
	{
		ServerData serverData = new ServerData();
		data.CopyTo(serverData);
		Utils.LogForce("!!!!!!!!!!!!!!!!!!!!!!!!!  ", data.Version);
		return serverData;
	}

	public void CopyTo(ServerData data)
	{
		foreach (AchievementProxy achievementsDatum in AchievementsData)
		{
			data._achievements.Add(achievementsDatum.Id, achievementsDatum);
		}
		data._bankItems = BankItems;
		data.BattleMathParams = BattleMathParamsData;
		foreach (BonusProxy bonusesDatum in BonusesData)
		{
			data._bonuses.Add(bonusesDatum.Id, bonusesDatum);
		}
		foreach (BotLevelProxy bossLevelsDatum in BossLevelsData)
		{
			data._bossLevels.Add(bossLevelsDatum.Level, bossLevelsDatum);
		}
		foreach (BotLevelProxy botLevelsDatum in BotLevelsData)
		{
			data._botLevels.Add(botLevelsDatum.Level, botLevelsDatum);
		}
		foreach (BotInfoProxy bot in Bots)
		{
			data._bots.Add(bot.Id, bot);
		}
		foreach (ChestProxy chestsDatum in ChestsData)
		{
			data.Chests.Add(chestsDatum.Id, chestsDatum);
		}
		data.Colors = ColorsData;
		foreach (ConditionProxy conditionsDatum in ConditionsData)
		{
			data._conditions.Add(conditionsDatum.Id, conditionsDatum);
		}
		foreach (StorylineDialogProxy storylineDialogsDatum in StorylineDialogsData)
		{
			data._storylineDialogs.Add(Tuple.Create(storylineDialogsDatum.LocationBot01, storylineDialogsDatum.LocationBot02, storylineDialogsDatum.LocationBot03), storylineDialogsDatum);
		}
		foreach (ServerData.Fatality fatalitiesDatum in FatalitiesData)
		{
			data._fatalities.Add(fatalitiesDatum.Id, fatalitiesDatum);
		}
		data._hints = HintsData;
		foreach (ItemProxy itemsDatum in ItemsData)
		{
			data._items.Add(itemsDatum.Id, itemsDatum);
		}
		foreach (ServerData.LevelData levelsDatum in LevelsData)
		{
			data._levels.Add(levelsDatum.Id, levelsDatum);
		}
		data.LoadingTips = LoadingTipsData;
		foreach (LocationProxy locationsDatum in LocationsData)
		{
			data._locations.Add(locationsDatum.Id, locationsDatum);
		}
		foreach (ServerData.MoneyType moneyTypesDatum in MoneyTypesData)
		{
			data._moneyTypes.Add(moneyTypesDatum.Id, moneyTypesDatum);
		}
		foreach (ServerData.Npc npcsDatum in NpcsData)
		{
			data._npcs.Add(npcsDatum.Id, npcsDatum);
		}
		foreach (PersDataProxy persDataDatum in PersDataData)
		{
			data._persData.Add(persDataDatum);
		}
		data._phrases = PhrasesData;
		data.GameSettings = SettingsData;
		data._skills = SkillsData.ToDictionary((KeyValuePair<string, ServerData.Skill> pair) => pair.Key, (KeyValuePair<string, ServerData.Skill> pair) => pair.Value);
		foreach (ServerData.Slot slotsDatum in SlotsData)
		{
			data._slots.Add(slotsDatum.Id, slotsDatum);
		}
		foreach (SpellProxy spellsDatum in SpellsData)
		{
			data._spells.Add(spellsDatum.Id, spellsDatum);
		}
		foreach (ShopGoodProxy shopGoodsDatum in ShopGoodsData)
		{
			data._shopGoods.Add(shopGoodsDatum.Id, shopGoodsDatum);
		}
		foreach (ServerData.Subtitle subtitlesDatum in SubtitlesData)
		{
			data._subtitles.Add(subtitlesDatum.Id, subtitlesDatum);
		}
	}
}
