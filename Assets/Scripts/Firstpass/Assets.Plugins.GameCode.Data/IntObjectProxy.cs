using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class IntObjectProxy : ObjectProxy
{
	[ProtoMember(1)]
	public int Payload { get; set; }

	public override object Get()
	{
		return Payload;
	}
}
