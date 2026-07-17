using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class LocationProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public int MapId;

	[ProtoMember(3)]
	public int MapModel;

	[DefaultValue("")]
	[ProtoMember(4)]
	public string Title;

	[ProtoMember(5)]
	public BotLocationInfoProxy[] Bots;

	[ProtoMember(6)]
	public ChestLocationInfoProxy[] Chests;

	[ProtoMember(7)]
	public Vector2Proxy IconMobsCoord;

	[ProtoMember(8)]
	public Vector2Proxy IconZachistkaCoord;

	[ProtoMember(9)]
	public Vector2Proxy IconMoneyCoord;

	[ProtoMember(10)]
	public Vector2Proxy IconChestCoord;

	[ProtoMember(11)]
	public int RespawnPeriodSeconds;

	[ProtoMember(12)]
	public int RespawnProbability;

	[ProtoMember(13)]
	public int RespawnMax;

	[ProtoMember(14)]
	public int RespawnKill;

	[ProtoMember(15)]
	public int RespawnKillPeriodSeconds;

	[ProtoMember(16)]
	public int PopulationPeriodSeconds;

	[ProtoMember(17)]
	public int PopulationPointsUp;

	[ProtoMember(18)]
	public int PopulationMax;

	[ProtoMember(19)]
	public int MoneyMax;

	[ProtoMember(20)]
	public int MoneyPeriod;

	[ProtoMember(21)]
	public int MoneyPerPerson;

	[ProtoMember(22)]
	public LocationProxy OpenAfter;

	[ProtoMember(23)]
	public LocationProxy OpenAfterAlt;

	[ProtoMember(24)]
	public bool IsMiniBoss;

	[ProtoMember(25)]
	public bool IsAltPath;

	[ProtoMember(26)]
	public BonusProxy Bonus;

	[ProtoMember(27)]
	public string Description;

	[ProtoMember(28)]
	public string IconPers;

	[ProtoMember(29)]
	public ConditionProxy Condition;

	[ProtoMember(31)]
	public bool Elfs;

	[ProtoMember(32)]
	public bool IsCave;

	[ProtoMember(33)]
	public string CaveName = string.Empty;

	[ProtoMember(34)]
	public string CaveDiff = string.Empty;

	[ProtoMember(35)]
	public Vector2Proxy CaveGlobalCoord;

	[ProtoMember(36)]
	public Vector2Proxy CaveLocationCoord;

	[ProtoMember(30)]
	public Dictionary<ServerData.MoneyType, int> OpenPrice { get; set; }

	public static implicit operator ServerData.Location(LocationProxy data)
	{
		if (data == null)
		{
			return null;
		}
		ServerData.Location location = new ServerData.Location();
		location.Id = data.Id;
		location.MapId = data.MapId;
		location.MapModel = data.MapModel;
		location.Title = data.Title;
		location.Bots = data.Bots.FromProxy();
		location.Chests = data.Chests.FromProxy();
		location.IconMobsCoord = data.IconMobsCoord;
		location.IconZachistkaCoord = data.IconZachistkaCoord;
		location.IconMoneyCoord = data.IconMoneyCoord;
		location.IconChestCoord = data.IconChestCoord;
		location.RespawnPeriodSeconds = data.RespawnPeriodSeconds;
		location.RespawnProbability = data.RespawnProbability;
		location.RespawnMax = data.RespawnMax;
		location.RespawnKill = data.RespawnKill;
		location.RespawnKillPeriodSeconds = data.RespawnKillPeriodSeconds;
		location.PopulationPeriodSeconds = data.PopulationPeriodSeconds;
		location.PopulationPointsUp = data.PopulationPointsUp;
		location.PopulationMax = data.PopulationMax;
		location.MoneyMax = data.MoneyMax;
		location.MoneyPeriod = data.MoneyPeriod;
		location.MoneyPerPerson = data.MoneyPerPerson;
		location.OpenAfter = data.OpenAfter;
		location.OpenAfterAlt = data.OpenAfterAlt;
		location.IsMiniBoss = data.IsMiniBoss;
		location.IsAltPath = data.IsAltPath;
		location.Bonus = data.Bonus;
		location.Description = data.Description;
		location.IconPers = data.IconPers;
		location.Condition = data.Condition;
		location.Elfs = data.Elfs;
		location.OpenPrice = data.OpenPrice;
		location.IsCave = data.IsCave;
		location.CaveName = data.CaveName;
		location.CaveDiff = data.CaveDiff;
		location.CaveGlobalCoord = data.CaveGlobalCoord;
		location.CaveLocationCoord = data.CaveLocationCoord;
		return location;
	}

	public static implicit operator LocationProxy(ServerData.Location data)
	{
		if (data == null)
		{
			return null;
		}
		LocationProxy locationProxy = new LocationProxy();
		locationProxy.Id = data.Id;
		locationProxy.MapId = data.MapId;
		locationProxy.MapModel = data.MapModel;
		locationProxy.Title = data.Title;
		locationProxy.Bots = data.Bots.ToProxy();
		locationProxy.Chests = data.Chests.ToProxy();
		locationProxy.IconMobsCoord = data.IconMobsCoord;
		locationProxy.IconZachistkaCoord = data.IconZachistkaCoord;
		locationProxy.IconMoneyCoord = data.IconMoneyCoord;
		locationProxy.IconChestCoord = data.IconChestCoord;
		locationProxy.RespawnPeriodSeconds = data.RespawnPeriodSeconds;
		locationProxy.RespawnProbability = data.RespawnProbability;
		locationProxy.RespawnMax = data.RespawnMax;
		locationProxy.RespawnKill = data.RespawnKill;
		locationProxy.RespawnKillPeriodSeconds = data.RespawnKillPeriodSeconds;
		locationProxy.PopulationPeriodSeconds = data.PopulationPeriodSeconds;
		locationProxy.PopulationPointsUp = data.PopulationPointsUp;
		locationProxy.PopulationMax = data.PopulationMax;
		locationProxy.MoneyMax = data.MoneyMax;
		locationProxy.MoneyPeriod = data.MoneyPeriod;
		locationProxy.MoneyPerPerson = data.MoneyPerPerson;
		locationProxy.OpenAfter = data.OpenAfter;
		locationProxy.OpenAfterAlt = data.OpenAfterAlt;
		locationProxy.IsMiniBoss = data.IsMiniBoss;
		locationProxy.IsAltPath = data.IsAltPath;
		locationProxy.Bonus = data.Bonus;
		locationProxy.Description = data.Description;
		locationProxy.IconPers = data.IconPers;
		locationProxy.Condition = data.Condition;
		locationProxy.Elfs = data.Elfs;
		locationProxy.OpenPrice = data.OpenPrice;
		locationProxy.IsCave = data.IsCave;
		locationProxy.CaveName = data.CaveName;
		locationProxy.CaveDiff = data.CaveDiff;
		locationProxy.CaveGlobalCoord = data.CaveGlobalCoord;
		locationProxy.CaveLocationCoord = data.CaveLocationCoord;
		return locationProxy;
	}
}
