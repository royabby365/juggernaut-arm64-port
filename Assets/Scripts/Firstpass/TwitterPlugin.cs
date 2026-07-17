using System;
using System.Collections;
using UnityEngine;

public class TwitterPlugin
{
	private static string PROD_TWITTER_CONSUMER_KEY = "vHUA7OSMGMtfcJBiVKNQVw";

	private static string PROD_TWITTER_CONSUMER_SECRET = "SFRCAKlnjFwPZBQvXJJkjOsYt7UQx4qcJmOYQOUhF08";

	private static string DEV_TWITTER_CONSUMER_KEY = "GOTpGtD6MZEr8NPqqwXjsg";

	private static string DEV_TWITTER_CONSUMER_SECRET = "7lUnF8hUB0arxmyBcgbUyklvHgvoqtt2TZnVeThmDLQ";

	private static string SEPARATOR = "-*|";

	private static string IMAGE_NAME = "twitter_icon";

	private static string ACTION_URL = "http://juggermobile.com/?f";

	private static string _twitterMessage = string.Empty;

	private static string _twitterIconId = string.Empty;

	private static readonly string[] _twitterIconAssetNames = new string[94]
	{
		"1000", "1001", "1002", "1003", "1004", "1005", "1006", "1007", "1008", "1009",
		"1010", "1011", "1012", "1013", "1014", "1015", "1016", "1017", "1018", "1019",
		"1109", "1110", "1111", "1112", "1239", "1240", "1241", "1242", "1265", "1266",
		"1935", "1936", "1937", "1938", "1939", "847", "850", "851", "852", "853",
		"854", "855", "856", "857", "858", "859", "860", "861", "862", "863",
		"864", "865", "866", "867", "868", "869", "870", "871", "872", "873",
		"874", "875", "876", "877", "878", "879", "880", "881", "882", "883",
		"884", "885", "886", "887", "888", "889", "890", "891", "892", "893",
		"894", "895", "896", "897", "898", "899", "900", "901", "902", "903",
		"904", "905", "906", "999"
	};

	private static void loginDidSucceedEvent(string username)
	{
		Debug.Log("Twitter: loginDidSucceedEvent.  username: " + username);
		if (!string.IsNullOrEmpty(_twitterMessage))
		{
			PostUpdate(_twitterMessage);
		}
	}

	private static void loginDidFailEvent(string error)
	{
		Debug.Log("Twitter: loginDidFailEvent. error: " + error);
		MainMenu component = GameObject.Find("__main_menu").GetComponent<MainMenu>();
		if ("cancelled" == error)
		{
			component.IOSCall("_twitterCancelled");
		}
		else
		{
			component.IOSCall("_twitterNoInternet");
		}
	}

	private static void requestSucceededEvent(object response)
	{
		Debug.Log("Twitter: requestSucceededEvent");
		if (response != null)
		{
			ResultLogger.logObject(response);
		}
		if (response.GetType() == typeof(Hashtable))
		{
			Hashtable hashtable = (Hashtable)response;
			if (!hashtable.ContainsKey("error"))
			{
				string message = $"_twitterPostSucceded{SEPARATOR}{_twitterIconId}";
				MainMenu component = GameObject.Find("__main_menu").GetComponent<MainMenu>();
				component.IOSCall(message);
			}
			else
			{
				Debug.Log("Twitter: notification was not posted, because twitter returned error: " + hashtable["error"]);
			}
		}
		else
		{
			Debug.Log("Twitter: notification was not posted, because result was not a Hashtable");
		}
	}

	private static void requestFailedEvent(string error)
	{
		Debug.Log("Twitter: requestFailedEvent. error: " + error);
		MainMenu component = GameObject.Find("__main_menu").GetComponent<MainMenu>();
		component.IOSCall("_twitterNoInternet");
	}

	private static void twitterInitializedEvent()
	{
		Debug.Log("Twitter: twitterInitializedEvent, showing login dialog");
		TwitterAndroid.showLoginDialog();
	}

	public static bool IsTwitterEnabled()
	{
		return true;
	}

	public static void Enable()
	{
		TwitterAndroidManager.loginDidSucceedEvent += loginDidSucceedEvent;
		TwitterAndroidManager.loginDidFailEvent += loginDidFailEvent;
		TwitterAndroidManager.requestSucceededEvent += requestSucceededEvent;
		TwitterAndroidManager.requestFailedEvent += requestFailedEvent;
		TwitterAndroidManager.twitterInitializedEvent += twitterInitializedEvent;
	}

	public static void Disable()
	{
		TwitterAndroidManager.loginDidSucceedEvent -= loginDidSucceedEvent;
		TwitterAndroidManager.loginDidFailEvent -= loginDidFailEvent;
		TwitterAndroidManager.requestSucceededEvent -= requestSucceededEvent;
		TwitterAndroidManager.requestFailedEvent -= requestFailedEvent;
		TwitterAndroidManager.twitterInitializedEvent -= twitterInitializedEvent;
	}

	public static void Submit(string description)
	{
		_twitterMessage = description;
		if (!TwitterAndroid.isLoggedIn())
		{
			TwitterAndroid.init(PROD_TWITTER_CONSUMER_KEY, PROD_TWITTER_CONSUMER_SECRET);
		}
		else
		{
			PostUpdate(_twitterMessage);
		}
	}

	private static string GetTwitterIconAssetName()
	{
		string format = "{0}.png";
		string[] twitterIconAssetNames = _twitterIconAssetNames;
		foreach (string text in twitterIconAssetNames)
		{
			if (text == _twitterIconId)
			{
				return string.Format(format, _twitterIconId);
			}
		}
		return string.Format(format, IMAGE_NAME);
	}

	private static void PostUpdate(string description)
	{
		string[] array = description.Split(new string[1] { SEPARATOR }, StringSplitOptions.None);
		string messageText = array[0];
		messageText = messageText + " " + ACTION_URL;
		_twitterIconId = array[1];
		string twitterIconAssetName = GetTwitterIconAssetName();
		string name = "resources/twitter_achiev/" + twitterIconAssetName;
		SingletonT<ResourcesManager>.I.GetAssetBundleAsync(Globals.MainMenu, ResourcesManager.GetAssetBundlePath(name), delegate(string _, ResourcesManager.AssetBundleData ab, float time)
		{
			TextAsset textAsset = ab.Bundle.Load(twitterIconAssetName) as TextAsset;
			if (null == textAsset)
			{
				Debug.Log("Twitter: posting without image");
				TwitterAndroid.postUpdate(messageText);
			}
			else
			{
				Debug.Log($"Twitter: posting with image {textAsset.name}");
				TwitterAndroid.postUpdateWithImage(messageText, textAsset.bytes);
			}
			SingletonT<ResourcesManager>.I.RemoveAssetBundleNoActions(ab);
		}, delegate(string _, string errorMessage)
		{
			Utils.LogForce("TwitterPlugin.PostUpdate", errorMessage);
		});
	}
}
