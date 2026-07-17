using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoInclude(200, typeof(StringObjectProxy))]
[ProtoContract]
[ProtoInclude(100, typeof(IntObjectProxy))]
public class ObjectProxy
{
	public virtual object Get()
	{
		return null;
	}

	public static ObjectProxy FromObject(object o)
	{
		if (o is string payload)
		{
			StringObjectProxy stringObjectProxy = new StringObjectProxy();
			stringObjectProxy.Payload = payload;
			return stringObjectProxy;
		}
		string s = $"{o}";
		object result2;
		if (int.TryParse(s, out var result))
		{
			IntObjectProxy intObjectProxy = new IntObjectProxy();
			intObjectProxy.Payload = result;
			result2 = intObjectProxy;
		}
		else
		{
			result2 = new ObjectProxy();
		}
		return (ObjectProxy)result2;
	}

	public static object ToObject(ObjectProxy data)
	{
		return data.Get();
	}
}
