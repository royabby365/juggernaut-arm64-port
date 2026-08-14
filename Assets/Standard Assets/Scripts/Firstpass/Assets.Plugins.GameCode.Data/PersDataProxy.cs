using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data
{

[ProtoContract]
public class PersDataProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public string Title;

	[ProtoMember(3)]
	public string Description;

	[ProtoMember(4)]
	public string Feature;

	[ProtoMember(5)]
	public int Class;

	[ProtoMember(6)]
	public int Sex;

	[ProtoMember(7)]
	public string Cloth;

	[DefaultValue(1)]
	[ProtoMember(10)]
	public string SelectSet;

	[ProtoMember(11)]
	[DefaultValue(1)]
	public string SelectWeapon;

	[ProtoMember(12)]
	public string SelectHair;

	[ProtoMember(14)]
	public SpellProxy StartSpell;

	[ProtoMember(15)]
	public ItemProxy Bonus;

	[ProtoMember(16)]
	public int SkillsPoints;

	[ProtoMember(8)]
	public Dictionary<ServerData.MoneyType, int> Money { get; set; }

	[ProtoMember(9)]
	public ServerData.SkillInfo[] Skills { get; set; }

	public static implicit operator ServerData.PersData(PersDataProxy data)
	{
		ServerData.PersData persData = new ServerData.PersData();
		persData.Id = data.Id;
		persData.Title = data.Title;
		persData.Description = data.Description;
		persData.Feature = data.Feature;
		persData.Class = data.Class;
		persData.Sex = data.Sex;
		persData.Cloth = data.Cloth;
		persData.SelectSet = data.SelectSet;
		persData.SelectWeapon = data.SelectWeapon;
		persData.SelectHair = data.SelectHair;
		persData.StartSpell = data.StartSpell;
		persData.Bonus = data.Bonus;
		persData.SkillsPoints = data.SkillsPoints;
		persData.Money = data.Money;
		persData.Skills = data.Skills;
		return persData;
	}

	public static implicit operator PersDataProxy(ServerData.PersData data)
	{
		PersDataProxy persDataProxy = new PersDataProxy();
		persDataProxy.Id = data.Id;
		persDataProxy.Title = data.Title;
		persDataProxy.Description = data.Description;
		persDataProxy.Feature = data.Feature;
		persDataProxy.Class = data.Class;
		persDataProxy.Sex = data.Sex;
		persDataProxy.Cloth = data.Cloth;
		persDataProxy.SelectSet = data.SelectSet;
		persDataProxy.SelectWeapon = data.SelectWeapon;
		persDataProxy.SelectHair = data.SelectHair;
		persDataProxy.StartSpell = data.StartSpell;
		persDataProxy.Bonus = data.Bonus;
		persDataProxy.SkillsPoints = data.SkillsPoints;
		persDataProxy.Money = data.Money;
		persDataProxy.Skills = data.Skills;
		return persDataProxy;
	}
}
}
