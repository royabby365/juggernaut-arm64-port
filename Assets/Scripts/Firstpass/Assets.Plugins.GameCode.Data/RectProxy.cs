using ProtoBuf;
using UnityEngine;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class RectProxy
{
	[ProtoMember(1)]
	public float X;

	[ProtoMember(2)]
	public float Y;

	[ProtoMember(3)]
	public float W;

	[ProtoMember(4)]
	public float H;

	public static implicit operator Rect(RectProxy data)
	{
		return new Rect(data.X, data.Y, data.W, data.H);
	}
}
