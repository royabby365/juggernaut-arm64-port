using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

namespace Assets.Plugins.GameCode.Data
{

[ProtoContract]
public class AchievementProxy
{
	[DefaultValue(-1)]
	[ProtoMember(1)]
	public int Id = -1;

	[ProtoMember(2)]
	public string Title;

	[ProtoMember(3)]
	public string Info;

	[ProtoMember(4)]
	public string Image;

	[ProtoMember(5)]
	public int Points;

	[ProtoMember(6)]
	public int Order;

	[ProtoMember(7)]
	public Dictionary<string, ObjectProxy> Condition;

	public static implicit operator ServerData.Achievement(AchievementProxy data)
	{
		Dictionary<string, object> dictionary = data.Condition.FromProxy();
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, object>();
		}
		ServerData.Achievement achievement = new ServerData.Achievement();
		achievement.Id = data.Id;
		achievement.Title = data.Title;
		achievement.Info = data.Info;
		achievement.Image = data.Image;
		achievement.Points = data.Points;
		achievement.Order = data.Order;
		achievement.Condition = dictionary;
		return achievement;
	}

	public static implicit operator AchievementProxy(ServerData.Achievement data)
	{
		AchievementProxy achievementProxy = new AchievementProxy();
		achievementProxy.Id = data.Id;
		achievementProxy.Title = data.Title;
		achievementProxy.Info = data.Info;
		achievementProxy.Image = data.Image;
		achievementProxy.Points = data.Points;
		achievementProxy.Order = data.Order;
		achievementProxy.Condition = data.Condition.ToProxy();
		return achievementProxy;
	}
}
}
