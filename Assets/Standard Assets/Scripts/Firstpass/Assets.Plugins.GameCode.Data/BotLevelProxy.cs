using System.ComponentModel;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data
{

[ProtoContract]
public class BotLevelProxy
{
	[ProtoMember(1)]
	public int Level;

	[ProtoMember(2)]
	public string Title;

	[ProtoMember(3)]
	public BonusProxy WinBonus;

	[ProtoMember(4)]
	public int WinExp;

	[ProtoMember(5)]
	public int TotalWeight;

	[ProtoMember(6)]
	public BonusProxy LossBonus;

	[ProtoMember(7)]
	public ServerData.SkillInfo[] Skills;

	[ProtoMember(8)]
	public bool ShowSectorControl;

	[ProtoMember(9)]
	public float SpeedMoveSectorControl;

	[ProtoMember(10)]
	public float SectorAngle;

	[ProtoMember(11)]
	public int WeakMagicP;

	[ProtoMember(12)]
	public int StrongMagicP;

	[ProtoMember(13)]
	[DefaultValue(1)]
	public int DifMagicgame;

	[ProtoMember(14)]
	public int ChangeViewDirPeriod;

	[ProtoMember(15)]
	public int ChangeViewDirProb;

	[ProtoMember(16)]
	public int ZoneSize;

	public static implicit operator ServerData.BotLevel(BotLevelProxy data)
	{
		if (data == null)
		{
			return null;
		}
		ServerData.BotLevel botLevel = new ServerData.BotLevel();
		botLevel.Level = data.Level;
		botLevel.Title = data.Title;
		botLevel.WinBonus = data.WinBonus;
		botLevel.WinExp = data.WinExp;
		botLevel.TotalWeight = data.TotalWeight;
		botLevel.LossBonus = data.LossBonus;
		botLevel.Skills = data.Skills;
		botLevel.ShowSectorControl = data.ShowSectorControl;
		botLevel.SpeedMoveSectorControl = data.SpeedMoveSectorControl;
		botLevel.SectorAngle = data.SectorAngle;
		botLevel.WeakMagicP = data.WeakMagicP;
		botLevel.StrongMagicP = data.StrongMagicP;
		botLevel.DifMagicgame = data.DifMagicgame;
		botLevel.ChangeViewDirPeriod = data.ChangeViewDirPeriod;
		botLevel.ChangeViewDirProb = data.ChangeViewDirProb;
		botLevel.ZoneSize = data.ZoneSize;
		return botLevel;
	}

	public static implicit operator BotLevelProxy(ServerData.BotLevel data)
	{
		if (data == null)
		{
			return null;
		}
		BotLevelProxy botLevelProxy = new BotLevelProxy();
		botLevelProxy.Level = data.Level;
		botLevelProxy.Title = data.Title;
		botLevelProxy.WinBonus = data.WinBonus;
		botLevelProxy.WinExp = data.WinExp;
		botLevelProxy.TotalWeight = data.TotalWeight;
		botLevelProxy.LossBonus = data.LossBonus;
		botLevelProxy.Skills = data.Skills;
		botLevelProxy.ShowSectorControl = data.ShowSectorControl;
		botLevelProxy.SpeedMoveSectorControl = data.SpeedMoveSectorControl;
		botLevelProxy.SectorAngle = data.SectorAngle;
		botLevelProxy.WeakMagicP = data.WeakMagicP;
		botLevelProxy.StrongMagicP = data.StrongMagicP;
		botLevelProxy.DifMagicgame = data.DifMagicgame;
		botLevelProxy.ChangeViewDirPeriod = data.ChangeViewDirPeriod;
		botLevelProxy.ChangeViewDirProb = data.ChangeViewDirProb;
		botLevelProxy.ZoneSize = data.ZoneSize;
		return botLevelProxy;
	}
}
}
