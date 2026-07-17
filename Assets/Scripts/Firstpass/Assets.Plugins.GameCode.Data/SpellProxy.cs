using System.Collections.Generic;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class SpellProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public string Title;

	[ProtoMember(3)]
	public string Description;

	[ProtoMember(4)]
	public int Level;

	[ProtoMember(5)]
	public string SchoolName;

	[ProtoMember(7)]
	public string IconName;

	[ProtoMember(8)]
	public string EffectName;

	[ProtoMember(9)]
	public bool InMagicBook;

	[ProtoMember(10)]
	public string UpdateId;

	[ProtoMember(11)]
	public float PowerK;

	[ProtoMember(12)]
	public SpellProxy NextSpell;

	[ProtoMember(13)]
	public int Points;

	[ProtoMember(6)]
	public Dictionary<ServerData.MoneyType, int> Price { get; set; }

	public static implicit operator ServerData.Spell(SpellProxy data)
	{
		if (data == null)
		{
			return null;
		}
		ServerData.Spell spell = new ServerData.Spell();
		spell.Id = data.Id;
		spell.Title = data.Title;
		spell.Description = data.Description;
		spell.Level = data.Level;
		spell.SchoolName = data.SchoolName;
		spell.IconName = data.IconName;
		spell.EffectName = data.EffectName;
		spell.InMagicBook = data.InMagicBook;
		spell.UpdateId = data.UpdateId;
		spell.PowerK = data.PowerK;
		spell.NextSpell = data.NextSpell;
		spell.Points = data.Points;
		spell.Price = data.Price;
		return spell;
	}

	public static implicit operator SpellProxy(ServerData.Spell data)
	{
		if (data == null)
		{
			return null;
		}
		SpellProxy spellProxy = new SpellProxy();
		spellProxy.Id = data.Id;
		spellProxy.Title = data.Title;
		spellProxy.Description = data.Description;
		spellProxy.Level = data.Level;
		spellProxy.SchoolName = data.SchoolName;
		spellProxy.IconName = data.IconName;
		spellProxy.EffectName = data.EffectName;
		spellProxy.InMagicBook = data.InMagicBook;
		spellProxy.UpdateId = data.UpdateId;
		spellProxy.PowerK = data.PowerK;
		spellProxy.NextSpell = data.NextSpell;
		spellProxy.Points = data.Points;
		spellProxy.Price = data.Price;
		return spellProxy;
	}
}
