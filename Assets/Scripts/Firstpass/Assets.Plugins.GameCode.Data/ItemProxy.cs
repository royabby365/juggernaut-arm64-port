using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class ItemProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public string Title;

	[ProtoMember(3)]
	public string TitleBlue;

	[ProtoMember(4)]
	public string TitleGreen;

	[ProtoMember(5)]
	public string Description;

	[ProtoMember(6)]
	public ServerData.Slot Slot;

	[ProtoMember(7)]
	public int Class;

	[ProtoMember(8)]
	public string Color;

	[ProtoMember(10)]
	public int Sex;

	[ProtoMember(11)]
	public int TotalWeight;

	[ProtoMember(12)]
	public int LifeTime;

	[ProtoMember(13)]
	public int DeathTime;

	[ProtoMember(14)]
	public string Model;

	[ProtoMember(15)]
	public string ModelBlue;

	[ProtoMember(16)]
	public string ModelGreen;

	[ProtoMember(17)]
	public string Picture;

	[ProtoMember(18)]
	public DateTime CreateTime;

	[ProtoMember(19)]
	public int Set;

	[ProtoMember(20)]
	public ServerData.SkillInfo[] Skills;

	[ProtoMember(21)]
	public bool PutOn;

	[ProtoMember(22)]
	public bool New;

	[ProtoMember(23)]
	public int RashodkaEffect;

	[ProtoMember(24)]
	public int RashodkaPower;

	[ProtoMember(25)]
	public int RashodkaNextTurns;

	[ProtoMember(26)]
	public int RashodkaEffectTurns;

	[ProtoMember(27)]
	public int RealItemsCount;

	[ProtoMember(28)]
	public ServerData.Fatality FatalityScenarioName;

	[ProtoMember(29)]
	public int TotalWeightMax;

	[ProtoMember(30)]
	public int GiveSkillsCount;

	[ProtoMember(31)]
	public string PictureInBattle;

	[ProtoMember(32)]
	public int MaxStars;

	[ProtoMember(33, IsRequired = false)]
	public int CurrentStars;

	[ProtoMember(9)]
	public Dictionary<ServerData.MoneyType, int> SellPrice { get; set; }

	public static implicit operator ServerData.Item(ItemProxy data)
	{
		if (data == null)
		{
			return null;
		}
		ServerData.Item item = new ServerData.Item();
		item.Id = data.Id;
		item.Title = data.Title;
		item.TitleBlue = data.TitleBlue;
		item.TitleGreen = data.TitleGreen;
		item.Description = data.Description;
		item.Slot = data.Slot;
		item.Class = data.Class;
		item.Color = data.Color;
		item.Sex = data.Sex;
		item.TotalWeight = data.TotalWeight;
		item.LifeTime = data.LifeTime;
		item.DeathTime = data.DeathTime;
		item.Model = data.Model;
		item.ModelBlue = data.ModelBlue;
		item.ModelGreen = data.ModelGreen;
		item.Picture = data.Picture;
		item.CreateTime = data.CreateTime;
		item.Set = data.Set;
		item.Skills = ((data.Skills == null) ? new ServerData.SkillInfo[0] : data.Skills);
		item.PutOn = data.PutOn;
		item.New = data.New;
		item.RashodkaEffect = data.RashodkaEffect;
		item.RashodkaPower = data.RashodkaPower;
		item.RashodkaNextTurns = data.RashodkaNextTurns;
		item.RashodkaEffectTurns = data.RashodkaEffectTurns;
		item.RealItemsCount = data.RealItemsCount;
		item.FatalityScenarioName = data.FatalityScenarioName;
		item.TotalWeightMax = data.TotalWeightMax;
		item.GiveSkillsCount = data.GiveSkillsCount;
		item.PictureInBattle = data.PictureInBattle;
		item.MaxStars = data.MaxStars;
		item.CurrentStars = data.CurrentStars;
		item.SellPrice = data.SellPrice;
		return item;
	}

	public static implicit operator ItemProxy(ServerData.Item data)
	{
		if (data == null)
		{
			return null;
		}
		ItemProxy itemProxy = new ItemProxy();
		itemProxy.Id = data.Id;
		itemProxy.Title = data.Title;
		itemProxy.TitleBlue = data.TitleBlue;
		itemProxy.TitleGreen = data.TitleGreen;
		itemProxy.Description = data.Description;
		itemProxy.Slot = data.Slot;
		itemProxy.Class = data.Class;
		itemProxy.Color = data.Color;
		itemProxy.Sex = data.Sex;
		itemProxy.TotalWeight = data.TotalWeight;
		itemProxy.LifeTime = data.LifeTime;
		itemProxy.DeathTime = data.DeathTime;
		itemProxy.Model = data.Model;
		itemProxy.ModelBlue = data.ModelBlue;
		itemProxy.ModelGreen = data.ModelGreen;
		itemProxy.Picture = data.Picture;
		itemProxy.CreateTime = data.CreateTime;
		itemProxy.Set = data.Set;
		itemProxy.Skills = data.Skills;
		itemProxy.PutOn = data.PutOn;
		itemProxy.New = data.New;
		itemProxy.RashodkaEffect = data.RashodkaEffect;
		itemProxy.RashodkaPower = data.RashodkaPower;
		itemProxy.RashodkaNextTurns = data.RashodkaNextTurns;
		itemProxy.RashodkaEffectTurns = data.RashodkaEffectTurns;
		itemProxy.RealItemsCount = data.RealItemsCount;
		itemProxy.FatalityScenarioName = data.FatalityScenarioName;
		itemProxy.TotalWeightMax = data.TotalWeightMax;
		itemProxy.GiveSkillsCount = data.GiveSkillsCount;
		itemProxy.PictureInBattle = data.PictureInBattle;
		itemProxy.MaxStars = data.MaxStars;
		itemProxy.CurrentStars = data.CurrentStars;
		itemProxy.SellPrice = data.SellPrice;
		return itemProxy;
	}
}
