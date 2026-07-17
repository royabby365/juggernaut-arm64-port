using System.Collections.Generic;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class ShopGoodProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public ItemProxy Item;

	[ProtoMember(3)]
	public int Probability;

	[ProtoMember(4)]
	public int LevelMin;

	[ProtoMember(5)]
	public int LevelMax;

	[ProtoMember(6)]
	public int Count;

	[ProtoMember(7)]
	public bool Relict;

	[ProtoMember(8)]
	public Dictionary<ServerData.MoneyType, int> Price;

	public static implicit operator ServerData.ShopGood(ShopGoodProxy data)
	{
		ServerData.ShopGood shopGood = new ServerData.ShopGood();
		shopGood.Id = data.Id;
		shopGood.Item = data.Item;
		shopGood.Probability = data.Probability;
		shopGood.LevelMin = data.LevelMin;
		shopGood.LevelMax = data.LevelMax;
		shopGood.Count = data.Count;
		shopGood.Relict = data.Relict;
		shopGood.Price = ((data.Price == null) ? new Dictionary<ServerData.MoneyType, int>() : data.Price);
		return shopGood;
	}

	public static implicit operator ShopGoodProxy(ServerData.ShopGood data)
	{
		ShopGoodProxy shopGoodProxy = new ShopGoodProxy();
		shopGoodProxy.Id = data.Id;
		shopGoodProxy.Item = data.Item;
		shopGoodProxy.Probability = data.Probability;
		shopGoodProxy.LevelMin = data.LevelMin;
		shopGoodProxy.LevelMax = data.LevelMax;
		shopGoodProxy.Count = data.Count;
		shopGoodProxy.Relict = data.Relict;
		shopGoodProxy.Price = data.Price;
		return shopGoodProxy;
	}
}
