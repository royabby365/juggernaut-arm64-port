using ProtoBuf;

namespace Assets.Plugins.GameCode.Data
{

[ProtoContract]
public class StringObjectProxy : ObjectProxy
{
	[ProtoMember(1)]
	public string Payload { get; set; }

	public override object Get()
	{
		return Payload;
	}
}
}
