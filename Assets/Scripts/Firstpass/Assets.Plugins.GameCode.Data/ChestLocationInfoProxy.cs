using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class ChestLocationInfoProxy
{
	[ProtoMember(1)]
	public ChestProxy Chest;

	[ProtoMember(2)]
	public int Prob;

	public static implicit operator ChestLocationInfoProxy(ServerData.Location.ChestLocationInfo data)
	{
		ChestLocationInfoProxy chestLocationInfoProxy = new ChestLocationInfoProxy();
		chestLocationInfoProxy.Chest = data.Chest;
		chestLocationInfoProxy.Prob = data.Prob;
		return chestLocationInfoProxy;
	}

	public static implicit operator ServerData.Location.ChestLocationInfo(ChestLocationInfoProxy data)
	{
		ServerData.Location.ChestLocationInfo chestLocationInfo = new ServerData.Location.ChestLocationInfo();
		chestLocationInfo.Chest = data.Chest;
		chestLocationInfo.Prob = data.Prob;
		return chestLocationInfo;
	}
}
