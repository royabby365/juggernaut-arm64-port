using System.Collections.Generic;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class BonusProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public string Title;

	[ProtoMember(3)]
	public bool AllItems;

	[ProtoMember(4)]
	public int Count;

	[ProtoMember(5)]
	public List<DropElementProxy> Drop { get; set; }

	public static implicit operator ServerData.Bonus(BonusProxy data)
	{
		if (data == null)
		{
			return null;
		}
		ServerData.Bonus bonus = new ServerData.Bonus();
		bonus.Id = data.Id;
		bonus.Title = data.Title;
		bonus.AllItems = data.AllItems;
		bonus.Count = data.Count;
		bonus.Drop = data.Drop.FromProxy();
		return bonus;
	}

	public static implicit operator BonusProxy(ServerData.Bonus data)
	{
		if (data == null)
		{
			return null;
		}
		BonusProxy bonusProxy = new BonusProxy();
		bonusProxy.Id = data.Id;
		bonusProxy.Title = data.Title;
		bonusProxy.AllItems = data.AllItems;
		bonusProxy.Count = data.Count;
		bonusProxy.Drop = data.Drop.ToProxy();
		return bonusProxy;
	}
}
