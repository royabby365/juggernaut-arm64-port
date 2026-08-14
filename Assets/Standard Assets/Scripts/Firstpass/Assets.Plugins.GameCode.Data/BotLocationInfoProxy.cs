using ProtoBuf;

namespace Assets.Plugins.GameCode.Data
{

[ProtoContract]
public class BotLocationInfoProxy
{
	[ProtoMember(1)]
	public BotInfoProxy Bot;

	[ProtoMember(2)]
	public int Level;

	[ProtoMember(3)]
	public bool IsBoss;

	public static implicit operator ServerData.Location.BotLocationInfo(BotLocationInfoProxy data)
	{
		ServerData.Location.BotLocationInfo botLocationInfo = new ServerData.Location.BotLocationInfo();
		botLocationInfo.Bot = data.Bot;
		botLocationInfo.Level = data.Level;
		botLocationInfo.IsBoss = data.IsBoss;
		return botLocationInfo;
	}

	public static implicit operator BotLocationInfoProxy(ServerData.Location.BotLocationInfo data)
	{
		BotLocationInfoProxy botLocationInfoProxy = new BotLocationInfoProxy();
		botLocationInfoProxy.Bot = data.Bot;
		botLocationInfoProxy.Level = data.Level;
		botLocationInfoProxy.IsBoss = data.IsBoss;
		return botLocationInfoProxy;
	}
}
}
