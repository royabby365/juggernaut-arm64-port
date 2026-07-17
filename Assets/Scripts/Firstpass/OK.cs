using System;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

public class OK
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

		private static AndroidJavaObject OKStub;

		private static long RequestId = 0L;

		private static Dictionary<long, RequestCallbacks> RequestCallbacksMap = new Dictionary<long, RequestCallbacks>();

		public static void Init(string cbClassName, string cbFunctionName)
		{
			if (CurrentActivity == null)
			{
				Debug.Log("Loading OK");
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				CurrentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				OKStub = CurrentActivity.Call<AndroidJavaObject>("getOKStub", new object[0]);
				OKStub.Call("setCallbackHandler", cbClassName, cbFunctionName);
			}
		}

		public static void MakeRequest(string methodName, Dictionary<string, object> arguments, OnResult onResult, OnError onError)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("OKStub.Proxy.MakeRequest: " + methodName);
			}
			try
			{
				long num = ++RequestId;
				RequestCallbacksMap.Add(num, new RequestCallbacks(onResult, onError));
				OKStub.Call("makeRequest", methodName, num, Json.Serialize(arguments));
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

		public static bool IsAuthorized()
		{
			return OKStub.Call<bool>("isAuthorized", new object[0]);
		}
	}

	public delegate void OnError(Exception err);

	public delegate void OnAuthenticated();

	public static void Init(string cbClassName, string cbFunctionName)
	{
		Proxy.Init(cbClassName, cbFunctionName);
	}

	public static bool IsAuthorized()
	{
		return Proxy.IsAuthorized();
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
}
