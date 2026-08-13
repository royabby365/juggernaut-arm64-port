using ProtoBuf;
using UnityEngine;

namespace Assets.Plugins.GameCode.Data
{

[ProtoContract]
public class Vector2Proxy
{
	[ProtoMember(1)]
	public float X;

	[ProtoMember(2)]
	public float Y;

	public static implicit operator Vector2(Vector2Proxy data)
	{
		return new Vector2(data.X, data.Y);
	}

	public static implicit operator Vector2Proxy(Vector2 data)
	{
		Vector2Proxy vector2Proxy = new Vector2Proxy();
		vector2Proxy.X = data.x;
		vector2Proxy.Y = data.y;
		return vector2Proxy;
	}
}
}
