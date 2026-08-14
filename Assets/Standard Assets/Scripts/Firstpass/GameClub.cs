using System;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

public class GameClub
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

		private static AndroidJavaObject GameClubStub;

		private static long RequestId = 0L;

		private static Dictionary<long, RequestCallbacks> RequestCallbacksMap = new Dictionary<long, RequestCallbacks>();

		public static void Init(string cbClassName, string cbFunctionName)
		{
			if (CurrentActivity == null)
			{
				Debug.Log("Loading GameClub");
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				CurrentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				GameClubStub = CurrentActivity.Call<AndroidJavaObject>("getGameClubStub", new object[0]);
				GameClubStub.Call("setCallbackHandler", cbClassName, cbFunctionName);
			}
		}

		public static string GetLocalUserId()
		{
			return GameClubStub.Call<string>("getLocalUserId", new object[0]);
		}

		public static void ShowAddFriendsUI()
		{
			GameClubStub.Call("showAddFriendsUI");
		}

		public static void ShowLeaderboardUI(string leaderboardId)
		{
			GameClubStub.Call("showLeaderboardUI", leaderboardId);
		}

		public static void ShowDashboardUI()
		{
			GameClubStub.Call("showDashboardUI");
		}

		public static void MakeRequest(string methodName, Dictionary<string, object> arguments, OnResult onResult, OnError onError)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("GameClub.Proxy.MakeRequest: " + methodName);
			}
			try
			{
				long num = ++RequestId;
				RequestCallbacksMap.Add(num, new RequestCallbacks(onResult, onError));
				GameClubStub.Call("makeRequest", methodName, num, Json.Serialize(arguments));
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

	public delegate void OnError(Exception err);

	public delegate void OnAuthenticated();

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

	public static void ShowDashboardUI()
	{
		Proxy.ShowDashboardUI();
	}
}
