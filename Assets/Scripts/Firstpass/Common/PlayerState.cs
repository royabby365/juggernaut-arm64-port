using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

namespace Common;

[ProtoContract]
public class PlayerState
{
	[ProtoMember(6)]
	public int PlayerPersDataId;

	[ProtoMember(1)]
	public int Version { get; set; }

	[ProtoMember(2)]
	public ServerData.Item[] Inventory { get; set; }

	[ProtoMember(3)]
	public ServerData.PlayerParamsData PlayerParams { get; set; }

	[ProtoMember(4)]
	public List<ServerData.Location> Locations { get; set; }

	[ProtoMember(5)]
	public List<int> Spells { get; set; }

	[DefaultValue(1f)]
	[ProtoMember(7, IsRequired = false)]
	public float MusicVolume { get; set; }

	[ProtoMember(8, IsRequired = false)]
	[DefaultValue(1f)]
	public float SoundsVolume { get; set; }

	[ProtoMember(9, IsRequired = false)]
	[DefaultValue(true)]
	public bool HintsOn { get; set; }

	[ProtoMember(10, IsRequired = false)]
	[DefaultValue(false)]
	public bool WeekStatsOn { get; set; }

	[DefaultValue(-1)]
	[ProtoMember(11, IsRequired = false)]
	public int LastLoadedSceneServerId { get; set; }

	[DefaultValue(false)]
	[ProtoMember(12, IsRequired = false)]
	public bool TutorialsOn { get; set; }

	[DefaultValue("")]
	[ProtoMember(13, IsRequired = false)]
	public string TutorialsState { get; set; }

	[ProtoMember(14, IsRequired = false)]
	[DefaultValue(false)]
	public bool IsSovereighnFinalPlayed { get; set; }

	[DefaultValue(false)]
	[ProtoMember(15, IsRequired = false)]
	public bool IsFinalScreenShowed { get; set; }

	[DefaultValue(0)]
	[ProtoMember(16, IsRequired = false)]
	public long SaveTime { get; set; }

	[ProtoMember(17, IsRequired = false)]
	public List<int> BuyedMines { get; set; }

	[ProtoMember(18, IsRequired = false)]
	public bool IsUnaIntrolPlayed { get; set; }

	[ProtoMember(19, IsRequired = false)]
	public bool IsSwampIntroPlayed { get; set; }

	public override string ToString()
	{
		return $"PLAYERSTATE {Version} {PlayerPersDataId}";
	}
}
