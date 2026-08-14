using System.ComponentModel;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data
{

[ProtoContract]
public class BotInfoProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public string Model;

	[ProtoMember(3)]
	public string Armor;

	[ProtoMember(4)]
	public string Picture;

	[DefaultValue(-1)]
	[ProtoMember(5)]
	public int Eyes;

	[ProtoMember(6)]
	public string Title;

	[ProtoMember(7)]
	public float Scale;

	[ProtoMember(8)]
	public int _magic;

	[ProtoMember(9)]
	public MagicTypeE[] _magicImmunity;

	[ProtoMember(10)]
	public string[] ClosedActions;

	[ProtoMember(11)]
	public string SkinColor;

	public static implicit operator ServerData.BotInfo(BotInfoProxy data)
	{
		ServerData.BotInfo botInfo = new ServerData.BotInfo();
		botInfo.Id = data.Id;
		botInfo.Model = data.Model;
		botInfo.Armor = data.Armor;
		botInfo.Picture = data.Picture;
		botInfo.Eyes = data.Eyes;
		botInfo.Title = data.Title;
		botInfo.Scale = data.Scale;
		botInfo._magic = data._magic;
		botInfo._magicImmunity = data._magicImmunity;
		botInfo.ClosedActions = data.ClosedActions;
		botInfo.skinColor = data.SkinColor;
		return botInfo;
	}

	public static implicit operator BotInfoProxy(ServerData.BotInfo data)
	{
		BotInfoProxy botInfoProxy = new BotInfoProxy();
		botInfoProxy.Id = data.Id;
		botInfoProxy.Model = data.Model;
		botInfoProxy.Armor = data.Armor;
		botInfoProxy.Picture = data.Picture;
		botInfoProxy.Eyes = data.Eyes;
		botInfoProxy.Title = data.Title;
		botInfoProxy.Scale = data.Scale;
		botInfoProxy._magic = data._magic;
		botInfoProxy._magicImmunity = data._magicImmunity;
		botInfoProxy.ClosedActions = data.ClosedActions;
		botInfoProxy.SkinColor = data.skinColor;
		return botInfoProxy;
	}
}
}
