using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FacebookPlugin
{
	private static string SEPARATOR = "-*|";

	private static string ACTION_LINK = "http://juggermobile.com/?f";

	private static string PIC_LINK = "http://juggermobile.com/images/data/achiv/";

	private static string DEFAULT_PIC_LINK = "http://juggermobile.com/images/icon-114.png";

	private static string _appId = string.Empty;

	private static string _facebookMessage = string.Empty;

	private static string _facebookIconId = string.Empty;

	private static byte[] _facebookScreenshot;

	public static void Enable()
	{
		FacebookManager.loginSucceededEvent += facebookLogin;
		FacebookManager.loginFailedEvent += facebookLoginFailed;
		FacebookManager.loggedOutEvent += facebookDidLogoutEvent;
		FacebookManager.accessTokenExtendedEvent += facebookDidExtendTokenEvent;
		FacebookManager.sessionInvalidatedEvent += facebookSessionInvalidatedEvent;
		FacebookManager.dialogCompletedEvent += facebokDialogCompleted;
		FacebookManager.dialogCompletedWithUrlEvent += facebookDialogCompletedWithUrl;
		FacebookManager.dialogDidNotCompleteEvent += facebookDialogDidntComplete;
		FacebookManager.dialogFailedEvent += facebookDialogFailed;
		FacebookManager.customRequestReceivedEvent += facebookReceivedCustomRequest;
		FacebookManager.customRequestFailedEvent += facebookCustomRequestFailed;
	}

	public static void Disable()
	{
		FacebookManager.loginSucceededEvent -= facebookLogin;
		FacebookManager.loginFailedEvent -= facebookLoginFailed;
		FacebookManager.loggedOutEvent -= facebookDidLogoutEvent;
		FacebookManager.accessTokenExtendedEvent -= facebookDidExtendTokenEvent;
		FacebookManager.sessionInvalidatedEvent -= facebookSessionInvalidatedEvent;
		FacebookManager.dialogCompletedEvent -= facebokDialogCompleted;
		FacebookManager.dialogCompletedWithUrlEvent -= facebookDialogCompletedWithUrl;
		FacebookManager.dialogDidNotCompleteEvent -= facebookDialogDidntComplete;
		FacebookManager.dialogFailedEvent -= facebookDialogFailed;
		FacebookManager.customRequestReceivedEvent -= facebookReceivedCustomRequest;
		FacebookManager.customRequestFailedEvent -= facebookCustomRequestFailed;
	}

	private static void facebookLogin()
	{
		Debug.Log("Successfully logged in to Facebook");
		if (!string.IsNullOrEmpty(_facebookMessage))
		{
			PostUpdate(_facebookMessage);
		}
		if (_facebookScreenshot != null)
		{
			PostScreenshot(_facebookScreenshot);
		}
	}

	private static void facebookLoginFailed(string error)
	{
		Debug.Log("Facebook login failed: " + error);
	}

	private static void facebookDidLogoutEvent()
	{
		Debug.Log("facebookDidLogoutEvent");
	}

	private static void facebookDidExtendTokenEvent(DateTime newExpiry)
	{
		Debug.Log("facebookDidExtendTokenEvent: " + newExpiry);
	}

	private static void facebookSessionInvalidatedEvent()
	{
		Debug.Log("facebookSessionInvalidatedEvent");
	}

	private static void facebokDialogCompleted()
	{
		Debug.Log("facebokDialogCompleted");
	}

	private static void facebookDialogCompletedWithUrl(string url)
	{
		Debug.Log("facebookDialogCompletedWithUrl: " + url);
	}

	private static void facebookDialogDidntComplete()
	{
		Debug.Log("facebookDialogDidntComplete");
	}

	private static void facebookDialogFailed(string error)
	{
		Debug.Log("facebookDialogFailed: " + error);
	}

	private static void facebookReceivedCustomRequest(object obj)
	{
		Debug.Log("facebookReceivedCustomRequest");
		if (obj != null)
		{
			ResultLogger.logObject(obj);
		}
	}

	private static void facebookCustomRequestFailed(string error)
	{
		Debug.Log("facebookCustomRequestFailed failed: " + error);
	}

	public static void Submit(string description)
	{
		Debug.Log("FacebookPlugin.Submit(" + description + ")");
		_facebookMessage = description;
		_facebookIconId = string.Empty;
		_facebookScreenshot = null;
		if (!FacebookAndroid.isSessionValid())
		{
			FacebookAndroid.init(_appId);
			FacebookAndroid.loginWithRequestedPermissions(new string[2] { "publish_stream", "user_photos" });
		}
		else
		{
			PostUpdate(_facebookMessage);
		}
	}

	public static void SubmitScreenshot(byte[] screenshot)
	{
		Debug.Log("FacebookPlugin.SubmitScreenshot()");
		_facebookMessage = string.Empty;
		_facebookIconId = string.Empty;
		_facebookScreenshot = screenshot;
		if (!FacebookAndroid.isSessionValid())
		{
			FacebookAndroid.init(_appId);
			FacebookAndroid.loginWithRequestedPermissions(new string[2] { "publish_stream", "user_photos" });
		}
		else
		{
			PostScreenshot(_facebookScreenshot);
		}
	}

	private static void PostScreenshot(byte[] screenshot)
	{
		string text = "screenshot.jpg";
		string text2 = Application.persistentDataPath + "/" + text;
		Debug.Log("Writing screenshot to " + text2);
		File.WriteAllBytes(text2, screenshot);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("source", screenshot);
		dictionary.Add("message", null);
		Facebook.instance.post("me/photos", dictionary, facebookUpdatePosted);
	}

	private static void PostUpdate(string description)
	{
		string[] array = description.Split(new string[1] { SEPARATOR }, StringSplitOptions.None);
		string value = array[0];
		string value2 = array[1];
		string text = array[2];
		_facebookIconId = array[3];
		string empty = string.Empty;
		empty = ((!("-1" == _facebookIconId)) ? $"{PIC_LINK}{_facebookIconId}.png" : DEFAULT_PIC_LINK);
		if (string.IsNullOrEmpty(value2))
		{
			value2 = text;
			text = string.Empty;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("name", value);
		dictionary.Add("caption", value2);
		dictionary.Add("description", text);
		dictionary.Add("link", ACTION_LINK);
		dictionary.Add("picture", empty);
		Facebook.instance.post("me/feed", dictionary, facebookUpdatePosted);
	}

	private static void facebookUpdatePosted(string str, object obj)
	{
		Debug.Log("Facebook: message post result: " + str);
		if (obj != null)
		{
			ResultLogger.logObject(obj);
		}
		MainMenu component = GameObject.Find("__main_menu").GetComponent<MainMenu>();
		string empty = string.Empty;
		empty = ((!string.IsNullOrEmpty(str)) ? $"_facebookNotPosted{SEPARATOR}{-1}" : $"_facebookPostSucceded{SEPARATOR}{_facebookIconId}");
		component.IOSCall(empty);
	}

	public static void Init(string appId)
	{
		_appId = appId;
	}
}
