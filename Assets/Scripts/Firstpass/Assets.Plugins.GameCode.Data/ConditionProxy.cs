using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class ConditionProxy
{
	[ProtoMember(1)]
	public int Id;

	[DefaultValue(-1)]
	[ProtoMember(2)]
	public int Count;

	[ProtoMember(3)]
	public string Type;

	[ProtoMember(4)]
	public string OpenPhrase;

	[ProtoMember(5)]
	public Dictionary<string, ObjectProxy> Params { get; set; }

	public static implicit operator ServerData.Condition(ConditionProxy data)
	{
		if (data == null)
		{
			return null;
		}
		Dictionary<string, object> dictionary = data.Params.FromProxy();
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, object>();
		}
		ServerData.Condition condition = new ServerData.Condition();
		condition.Id = data.Id;
		condition.Count = data.Count;
		condition.Type = data.Type;
		condition.OpenPhrase = data.OpenPhrase;
		condition.Params = dictionary;
		return condition;
	}

	public static implicit operator ConditionProxy(ServerData.Condition data)
	{
		if (data == null)
		{
			return null;
		}
		ConditionProxy conditionProxy = new ConditionProxy();
		conditionProxy.Id = data.Id;
		conditionProxy.Count = data.Count;
		conditionProxy.Type = data.Type;
		conditionProxy.OpenPhrase = data.OpenPhrase;
		conditionProxy.Params = data.Params.ToProxy();
		return conditionProxy;
	}
}
