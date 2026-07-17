using ProtoBuf;
using UnityEngine;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class Vector3Proxy
{
	[ProtoMember(1)]
	public float X;

	[ProtoMember(2)]
	public float Y;

	[ProtoMember(3)]
	public float Z;

	public static implicit operator Vector3(Vector3Proxy data)
	{
		return new Vector3(data.X, data.Y, data.Z);
	}
}
