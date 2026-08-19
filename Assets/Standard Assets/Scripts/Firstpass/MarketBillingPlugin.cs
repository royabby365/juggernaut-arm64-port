using System.Collections.Generic;
using UnityEngine;

public class MarketBillingPlugin
{
	private static AndroidJavaObject currentActivity;

	private static void Init()
	{
		if (currentActivity == null)
		{
			Debug.Log("Loading MarketBillingPlugin");
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			currentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		}
	}

	public static void SetCallbackHandler(string className, string functionName)
	{
		Init();
		if (currentActivity != null)
		{
			try
			{
				currentActivity.Call("setCallbackHandler", className, functionName);
			}
			catch (System.Exception ex)
			{
				Debug.Log("MarketBillingPlugin.SetCallbackHandler failed: " + ex.Message);
			}
		}
	}

	public static void RequestPurchase(string productId)
	{
		Init();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("purchase_1", "ru.mail.games.juggernaut.purchase2");
		dictionary.Add("purchase_2", "ru.mail.games.juggernaut.purchase3");
		dictionary.Add("purchase_3", "ru.mail.games.juggernaut.purchase4");
		dictionary.Add("purchase_4", "ru.mail.games.juggernaut.purchase5");
		dictionary.Add("purchase_5", "ru.mail.games.juggernaut.purchase6");
		dictionary.Add("purchase_6", "ru.mail.games.juggernaut.purchase1");
		dictionary.Add("purchase_7", "ru.mail.games.juggernaut.purchase7");
		dictionary.Add("purchase_9", "ru.mail.games.juggernaut.purchase9");
		string text = dictionary[productId];
		if (currentActivity != null)
		{
			try
			{
				currentActivity.Call("buyProduct", text);
			}
			catch (System.Exception ex)
			{
				Debug.Log("MarketBillingPlugin.RequestPurchase failed: " + ex.Message);
			}
		}
	}

	public static void ClosePurchase(string itemData)
	{
		Init();
		if (currentActivity != null)
		{
			try
			{
				currentActivity.Call("closePayment", itemData);
			}
			catch (System.Exception ex)
			{
				Debug.Log("MarketBillingPlugin.ClosePurchase failed: " + ex.Message);
			}
		}
	}

	public static void RestoreTransaction()
	{
		Init();
		if (currentActivity != null)
		{
			try
			{
				currentActivity.Call("restoreTransaction");
			}
			catch (System.Exception ex)
			{
				Debug.Log("MarketBillingPlugin.RestoreTransaction failed: " + ex.Message);
			}
		}
	}
}
