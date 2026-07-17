using System;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

public class IDreamSky
{
	public class Proxy
	{
		private class RequestCallbacks
		{
			public OnResult OnResultCb;

			public OnError OnErrorCb;

			public RequestCallbacks(OnResult onResult, OnError onError)
			{
				OnResultCb = onResult;
				OnErrorCb = onError;
			}
		}

		public delegate void OnResult(Dictionary<string, object> arguments);

		public delegate void OnError(Exception error);

		private static AndroidJavaObject CurrentActivity;

		private static AndroidJavaObject IDreamSkyStub;

		private static long RequestId = 0L;

		private static Dictionary<long, RequestCallbacks> RequestCallbacksMap = new Dictionary<long, RequestCallbacks>();

		public static void Init(string cbClassName, string cbFunctionName)
		{
			if (CurrentActivity == null)
			{
				Debug.Log("Loading IDreamSky");
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				CurrentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				IDreamSkyStub = CurrentActivity.Call<AndroidJavaObject>("getIDreamSkyStub", new object[0]);
				IDreamSkyStub.Call("setCallbackHandler", cbClassName, cbFunctionName);
			}
		}

		public static string GetLocalUserId()
		{
			return IDreamSkyStub.Call<string>("getLocalUserId", new object[0]);
		}

		public static void ShowAddFriendsUI()
		{
			IDreamSkyStub.Call("showAddFriendsUI");
		}

		public static void ShowLeaderboardUI(string leaderboardId)
		{
			IDreamSkyStub.Call("showLeaderboardUI", leaderboardId);
		}

		public static void MakeRequest(string methodName, Dictionary<string, object> arguments, OnResult onResult, OnError onError)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("IDreamSky.Proxy.MakeRequest: " + methodName);
			}
			try
			{
				long num = ++RequestId;
				RequestCallbacksMap.Add(num, new RequestCallbacks(onResult, onError));
				IDreamSkyStub.Call("makeRequest", methodName, num, Json.Serialize(arguments));
			}
			catch (Exception ex)
			{
				Utils.LogForce("MakeRequest FAILED", ex.Message);
			}
		}

		public static void ReceiveResponse(string serializedResponseData)
		{
			long key = 0L;
			try
			{
				Dictionary<string, object> dictionary = Json.Deserialize(serializedResponseData) as Dictionary<string, object>;
				key = Convert.ToInt64(dictionary["requestId"]);
				RequestCallbacks requestCallbacks = RequestCallbacksMap[key];
				if (dictionary.ContainsKey("data"))
				{
					requestCallbacks.OnResultCb(dictionary["data"] as Dictionary<string, object>);
				}
				else if (dictionary.ContainsKey("error"))
				{
					requestCallbacks.OnErrorCb(new Exception(dictionary["error"].ToString()));
				}
				else
				{
					requestCallbacks.OnErrorCb(new Exception("Invalid response JSON."));
				}
			}
			catch (Exception ex)
			{
				Utils.LogForce("ReceiveResponse FAILED", ex.Message);
			}
			finally
			{
				if (RequestCallbacksMap.ContainsKey(key))
				{
					RequestCallbacksMap.Remove(key);
				}
			}
		}
	}

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

	public static void Init(string cbClassName, string cbFunctionName)
	{
		Proxy.Init(cbClassName, cbFunctionName);
	}

	public static string GetLocalUserId()
	{
		return Proxy.GetLocalUserId();
	}

	public static void Authenticate(OnAuthenticated onSucc, OnError onErr)
	{
		Proxy.MakeRequest("Authenticate", new Dictionary<string, object>(), delegate
		{
			onSucc();
		}, delegate(Exception ex)
		{
			onErr(ex);
		});
	}

	public static void LoadAchievements(OnAchievementsLoaded onSucc, OnError onErr)
	{
		Proxy.MakeRequest("LoadAchievements", new Dictionary<string, object>(), delegate(Dictionary<string, object> data)
		{
			List<object> list = data["achievements"] as List<object>;
			Achievement[] array = new Achievement[list.Count];
			int num = 0;
			foreach (object item in list)
			{
				Dictionary<string, object> dictionary = item as Dictionary<string, object>;
				array[num] = new Achievement(dictionary["id"].ToString(), Convert.ToDouble(dictionary["percentCompleted"]));
				num++;
			}
			onSucc(array);
		}, delegate(Exception ex)
		{
			onErr(ex);
		});
	}

	public static void LoadScores(string leaderboardId, int filter, OnScoresLoaded onSucc, OnError onErr)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("leaderboardId", leaderboardId);
		dictionary.Add("filter", filter);
		Proxy.MakeRequest("LoadScores", dictionary, delegate(Dictionary<string, object> data)
		{
			List<object> list = data["scores"] as List<object>;
			Score[] array = new Score[list.Count];
			int num = 0;
			foreach (object item in list)
			{
				Dictionary<string, object> dictionary2 = item as Dictionary<string, object>;
				array[num] = new Score(dictionary2["userId"].ToString(), dictionary2["userName"].ToString(), dictionary2["userImageUrl"].ToString(), Convert.ToInt64(dictionary2["rank"]), Convert.ToInt64(dictionary2["score"]));
				num++;
			}
			onSucc(array);
		}, delegate(Exception ex)
		{
			onErr(ex);
		});
	}

	public static void ReportProgress(string achievmentId, double percentCompleted, OnProgressReported onSucc, OnError onErr)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("achievmentId", achievmentId);
		dictionary.Add("percentCompleted", percentCompleted);
		Proxy.MakeRequest("ReportProgress", dictionary, delegate
		{
			onSucc();
		}, delegate(Exception ex)
		{
			onErr(ex);
		});
	}

	public static void ReportScore(string leaderboardId, long score, OnScoreReported onSucc, OnError onErr)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("leaderboardId", leaderboardId);
		dictionary.Add("score", score);
		Proxy.MakeRequest("ReportScore", dictionary, delegate
		{
			onSucc();
		}, delegate(Exception ex)
		{
			onErr(ex);
		});
	}

	public static void DoPayment(string purchaseId, string purchaseName, double price, int quantity, string slot, string userId, OnPaymentDone onSucc, OnError onErr)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("purchaseId", purchaseId);
		dictionary.Add("purchaseName", purchaseName);
		dictionary.Add("price", price);
		dictionary.Add("quantity", quantity);
		dictionary.Add("slot", slot);
		dictionary.Add("userId", userId);
		Proxy.MakeRequest("DoPayment", dictionary, delegate
		{
			onSucc();
		}, delegate(Exception ex)
		{
			onErr(ex);
		});
	}

	public static void ShowAddFriendsUI()
	{
		Proxy.ShowAddFriendsUI();
	}

	public static void ShowLeaderboardUI(string leaderboardId)
	{
		Proxy.ShowLeaderboardUI(leaderboardId);
	}
}
