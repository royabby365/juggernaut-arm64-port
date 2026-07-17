using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class DropElementProxy
{
	[ProtoMember(1)]
	public int Probability;

	[ProtoMember(2)]
	public int Type;

	[ProtoMember(3)]
	public int Count;

	[ProtoMember(4)]
	public ItemProxy Item;

	[ProtoMember(5)]
	public BonusProxy Bonus;

	public static implicit operator ServerData.Bonus.DropElement(DropElementProxy data)
	{
		ServerData.Bonus.DropElement dropElement = new ServerData.Bonus.DropElement();
		dropElement.Probability = data.Probability;
		dropElement.Type = data.Type;
		dropElement.Count = data.Count;
		dropElement.Item = data.Item;
		dropElement.Bonus = data.Bonus;
		return dropElement;
	}

	public static implicit operator DropElementProxy(ServerData.Bonus.DropElement data)
	{
		DropElementProxy dropElementProxy = new DropElementProxy();
		dropElementProxy.Probability = data.Probability;
		dropElementProxy.Type = data.Type;
		dropElementProxy.Count = data.Count;
		dropElementProxy.Item = data.Item;
		dropElementProxy.Bonus = data.Bonus;
		return dropElementProxy;
	}
}
