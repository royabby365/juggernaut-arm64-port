using UnityEngine;
using UnityEngine.SocialPlatforms;

public abstract class SocialAspect
{
	public class ScoresInfo
	{
		internal readonly long Score;

		internal readonly bool IsMe;

		internal string Id { get; private set; }

		internal string UserName { get; private set; }

		internal Texture2D Image { get; private set; }

		public ScoresInfo(string id, string userName, Texture2D image, long score, bool isMe)
		{
			Id = id;
			UserName = userName;
			Image = image;
			Score = score;
			IsMe = isMe;
		}

		public ScoresInfo(IUserProfile profile, long score, bool isMe)
		{
			Id = profile.id;
			UserName = profile.userName;
			Image = profile.image.CheckTexture2D();
			Score = score;
			IsMe = isMe;
		}

		public override string ToString()
		{
			return "{0} {1} {2} {3} {4}".Fmt(Id, Score, UserName, Image, IsMe);
		}
	}

	public abstract void ProcessAuthentication(ActionD<bool> action);

	public abstract void SyncWithExternal();

	public abstract string GetLeaderboardId();

	public abstract string GetLeaderboardCaveId();

	public abstract void GetFriendsScores(ActionD<ScoresInfo[]> action, ActionD onError);

	public abstract void GetFriendsCaveScores(ActionD<ScoresInfo[]> action, ActionD onError);
}
