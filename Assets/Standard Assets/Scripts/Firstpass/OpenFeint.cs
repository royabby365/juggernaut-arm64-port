using System;

public class OpenFeint
{
	public class Achievement
	{
		public string id;

		public double percentCompleted;

		public Achievement(string id, double percentCompleted)
		{
			this.id = id;
			this.percentCompleted = percentCompleted;
		}
	}

	public class Score
	{
		public string userId;

		public string userName;

		public string userImageUrl;

		public long rank;

		public long score;

		public Score(string userId, string userName, string userImageUrl, long rank, long score)
		{
			this.userId = userId;
			this.userName = userName;
			this.userImageUrl = userImageUrl;
			this.rank = rank;
			this.score = score;
		}
	}

	public delegate void OnError(Exception err);

	public delegate void OnAuthenticated();

	public delegate void OnAchievementsLoaded(Achievement[] achievements);

	public delegate void OnScoresLoaded(Score[] scores);

	public delegate void OnProgressReported();

	public delegate void OnScoreReported();

	public delegate void OnPaymentDone();

	public static string GetLocalUserId()
	{
		return string.Empty;
	}

	public static void Authenticate(OnAuthenticated onSucc, OnError onErr)
	{
		onSucc();
	}

	public static void LoadAchievements(OnAchievementsLoaded onSucc, OnError onErr)
	{
		Achievement[] achievements = new Achievement[0];
		onSucc(achievements);
	}

	public static void LoadScores(string leaderboardId, int filter, OnScoresLoaded onSucc, OnError onErr)
	{
		Score[] scores = new Score[0];
		onSucc(scores);
	}

	public static void ReportProgress(string achievmentId, double percentCompleted, OnProgressReported onSucc, OnError onErr)
	{
		onSucc();
	}

	public static void ReportScore(string leaderboardId, long score, OnScoreReported onSucc, OnError onErr)
	{
		onSucc();
	}

	public static void DoPayment(string purchaseId, string purchaseName, double price, int quantity, string slot, string userId, OnPaymentDone onSucc, OnError onErr)
	{
		onSucc();
	}

	public static void ShowAddFriendsUI()
	{
	}

	public static void ShowLeaderboardUI(string leaderboardId)
	{
	}
}
