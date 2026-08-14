using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

namespace Common
{

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
	[ProtoMember(7)]public float MusicVolume { get; set; }

	[ProtoMember(8)][DefaultValue(1f)]
	public float SoundsVolume { get; set; }

	[ProtoMember(9)][DefaultValue(true)]
	public bool HintsOn { get; set; }

	[ProtoMember(10)][DefaultValue(false)]
	public bool WeekStatsOn { get; set; }

	[DefaultValue(-1)]
	[ProtoMember(11)]public int LastLoadedSceneServerId { get; set; }

	[DefaultValue(false)]
	[ProtoMember(12)]public bool TutorialsOn { get; set; }

	[DefaultValue("")]
	[ProtoMember(13)]public string TutorialsState { get; set; }

	[ProtoMember(14)][DefaultValue(false)]
	public bool IsSovereighnFinalPlayed { get; set; }

	[DefaultValue(false)]
	[ProtoMember(15)]public bool IsFinalScreenShowed { get; set; }

	[DefaultValue(0)]
	[ProtoMember(16)]public long SaveTime { get; set; }

	[ProtoMember(17)]public List<int> BuyedMines { get; set; }

	[ProtoMember(18)]public bool IsUnaIntrolPlayed { get; set; }

	[ProtoMember(19)]public bool IsSwampIntroPlayed { get; set; }

	public override string ToString()
	{
		return $"PLAYERSTATE {Version} {PlayerPersDataId}";
	}
}
}
