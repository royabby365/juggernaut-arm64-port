using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class ChestProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public int PicturesCount;

	[ProtoMember(3)]
	public int Type;

	[ProtoMember(4)]
	public BonusProxy Bonus;

	[ProtoMember(5)]
	public string Index;

	[ProtoMember(6)]
	public int ElfProbability;

	[ProtoMember(7)]
	public int ElfLevelDifference;

	[ProtoMember(8)]
	public int ElfModel;

	public static implicit operator ServerData.Chest(ChestProxy data)
	{
		ServerData.Chest chest = new ServerData.Chest();
		chest.Id = data.Id;
		chest.PicturesCount = data.PicturesCount;
		chest.Type = data.Type;
		chest.Bonus = data.Bonus;
		chest.Index = data.Index;
		chest.ElfProbability = data.ElfProbability;
		chest.ElfLevelDifference = data.ElfLevelDifference;
		chest.ElfModel = data.ElfModel;
		return chest;
	}

	public static implicit operator ChestProxy(ServerData.Chest data)
	{
		ChestProxy chestProxy = new ChestProxy();
		chestProxy.Id = data.Id;
		chestProxy.PicturesCount = data.PicturesCount;
		chestProxy.Type = data.Type;
		chestProxy.Bonus = data.Bonus;
		chestProxy.Index = data.Index;
		chestProxy.ElfProbability = data.ElfProbability;
		chestProxy.ElfLevelDifference = data.ElfLevelDifference;
		chestProxy.ElfModel = data.ElfModel;
		return chestProxy;
	}
}
