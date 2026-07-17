using System;
using System.Collections;
using UnityEngine;

public class TapjoyPlugin
{
	private static AndroidJavaObject currentActivity;

	private static AndroidJavaClass tapjoyConnect;

	private static AndroidJavaObject tapjoyConnectInstance;

	public static void init()
	{
		if (currentActivity == null)
		{
			Debug.Log("Loading TapjoyPlugin");
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			currentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			tapjoyConnect = new AndroidJavaClass("com.tapjoy.TapjoyConnect");
		}
	}

	public static void SetCallbackHandler(string handlerName)
	{
		init();
		tapjoyConnect.CallStatic("setHandlerClass", handlerName);
	}

	public static void RequestTapjoyConnect(string appID, string secretKey)
	{
		init();
		tapjoyConnect.CallStatic("requestTapjoyConnect", currentActivity, appID, secretKey);
		tapjoyConnectInstance = tapjoyConnect.CallStatic<AndroidJavaObject>("getTapjoyConnectInstance", new object[0]);
	}

	public static void EnableLogging(bool enable)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.tapjoy.TapjoyLog");
		androidJavaClass.CallStatic("enableLogging", enable);
	}

	public static void ActionComplete(string actionID)
	{
		tapjoyConnectInstance.Call("actionComplete", actionID);
	}

	public static void SetUserID(string userID)
	{
		tapjoyConnectInstance.Call("setUserID", userID);
	}

	public static void ShowOffers()
	{
		tapjoyConnectInstance.Call("showOffers");
	}

	public static void GetTapPoints()
	{
		tapjoyConnectInstance.Call("getTapPoints");
	}

	public static void SpendTapPoints(int points)
	{
		tapjoyConnectInstance.Call("spendTapPoints", points);
	}

	public static void AwardTapPoints(int points)
	{
		tapjoyConnectInstance.Call("awardTapPoints", points);
	}

	public static int QueryTapPoints()
	{
		return tapjoyConnectInstance.Call<int>("getTapPointsTotal", new object[0]);
	}

	public static void SetEarnedPointsNotifier()
	{
		tapjoyConnectInstance.Call("setEarnedPointsNotifier");
	}

	public static void GetDisplayAd()
	{
		tapjoyConnectInstance.Call("getDisplayAd");
	}

	public static void ShowDisplayAd()
	{
		tapjoyConnectInstance.Call("showBannerAd");
	}

	public static void HideDisplayAd()
	{
		tapjoyConnectInstance.Call("hideBannerAd");
	}

	public static void SetDisplayAdContentSize(int size)
	{
		string text = "320x50";
		switch ((TapjoyBannerAdSize)size)
		{
		case TapjoyBannerAdSize.TJC_AD_BANNERSIZE_320X50:
			text = "320x50";
			break;
		case TapjoyBannerAdSize.TJC_AD_BANNERSIZE_640X100:
			text = "640x100";
			break;
		case TapjoyBannerAdSize.TJC_AD_BANNERSIZE_768X90:
			text = "768x90";
			break;
		default:
			text = "320x50";
			Debug.Log("*** Invalid banner ad size, using 320x50 as default ***");
			break;
		}
		tapjoyConnectInstance.Call("setBannerAdSize", text);
	}

	public static void RefreshDisplayAd(bool enable)
	{
		tapjoyConnectInstance.Call("enableBannerAdAutoRefresh", enable);
	}

	public static void MoveDisplayAd(int x, int y)
	{
		tapjoyConnectInstance.Call("setBannerAdPosition", x, y);
	}

	public static void GetFeaturedApp()
	{
		tapjoyConnectInstance.Call("getFeaturedApp");
	}

	public static void SetFeaturedAppDisplayCount(int displayCount)
	{
		tapjoyConnectInstance.Call("SetFeaturedAppDisplayCount", displayCount);
	}

	public static void ShowFeaturedAppFullScreenAd()
	{
		tapjoyConnectInstance.Call("showFeaturedAppFullScreenAd");
	}

	public static void InitVideoAd()
	{
		tapjoyConnectInstance.Call("initVideoAd");
	}

	public static void SetVideoCacheCount(int cacheCount)
	{
		tapjoyConnectInstance.Call("setVideoCacheCount", cacheCount);
	}

	public static void EnableVideoCache(bool enable)
	{
		tapjoyConnectInstance.Call("enableVideoCache", enable);
	}

	public static void ShowOffersWithCurrencyID(string currencyID, bool selector)
	{
		tapjoyConnectInstance.Call("showOffersWithCurrencyID", currencyID, selector);
	}

	public static void GetDisplayAdWithCurrencyID(string currencyID)
	{
		tapjoyConnectInstance.Call("getDisplayAdWithCurrencyID", currencyID);
	}

	public static void GetFeaturedAppWithCurrencyID(string currencyID)
	{
		tapjoyConnectInstance.Call("getFeaturedAppWithCurrencyID", currencyID);
	}

	public static void SetCurrencyMultiplier(float multiplier)
	{
		tapjoyConnectInstance.Call("setCurrencyMultiplier", multiplier);
	}

	public static void SetUserDefinedColorWithIntValue(int color)
	{
		tapjoyConnectInstance.Call("setUserDefinedColor", color);
	}

	public static void InitVirtualGoods()
	{
		tapjoyConnectInstance.Call("checkForVirtualGoods");
	}

	public static void ShowVirtualGoodsView()
	{
		tapjoyConnectInstance.Call("showVirtualGoods");
	}

	public static ArrayList GetPurchasedVirtualGoodsArray()
	{
		ArrayList arrayList = new ArrayList();
		int purchasedVirtualGoodCount = GetPurchasedVirtualGoodCount();
		for (int i = 0; i < purchasedVirtualGoodCount; i++)
		{
			arrayList.Add(GetVirtualGood(i));
		}
		return arrayList;
	}

	private static TapjoyVirtualGood GetVirtualGood(int index)
	{
		AndroidJNI.PushLocalFrame(16);
		TapjoyVirtualGood result = default(TapjoyVirtualGood);
		AndroidJavaObject androidJavaObject = tapjoyConnectInstance.Call<AndroidJavaObject>("getPurchasedItemAtIndex", new object[1] { index });
		if (androidJavaObject != null)
		{
			result.name = androidJavaObject.Call<string>("getName", new object[0]);
			result.description = androidJavaObject.Call<string>("getDescription", new object[0]);
			result.dataURL = androidJavaObject.Call<string>("getDatafileUrl", new object[0]);
			result.imageURL = androidJavaObject.Call<string>("getFullImageUrl", new object[0]);
			result.owned = androidJavaObject.Call<int>("getNumberOwned", new object[0]);
			result.price = androidJavaObject.Call<int>("getPrice", new object[0]);
			result.productID = androidJavaObject.Call<string>("getProductID", new object[0]);
			result.thumbURL = androidJavaObject.Call<string>("getThumbImageUrl", new object[0]);
			result.storeItemID = androidJavaObject.Call<string>("getVgStoreItemID", new object[0]);
			result.type = androidJavaObject.Call<string>("getVgStoreItemTypeName", new object[0]);
			result.attributes = new ArrayList();
			AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getVgStoreItemsAttributeValueList", new object[0]);
			int num = androidJavaObject2.Call<int>("size", new object[0]);
			for (int i = 0; i < num; i++)
			{
				AndroidJavaObject attributeData = androidJavaObject2.Call<AndroidJavaObject>("get", new object[1] { i });
				result.attributes.Add(GetAttributes(attributeData));
			}
		}
		AndroidJNI.PopLocalFrame(IntPtr.Zero);
		return result;
	}

	private static TapjoyVirtualGoodAttribute GetAttributes(AndroidJavaObject attributeData)
	{
		AndroidJNI.PushLocalFrame(2);
		TapjoyVirtualGoodAttribute result = new TapjoyVirtualGoodAttribute
		{
			type = attributeData.Call<string>("getAttributeType", new object[0]),
			value = attributeData.Call<string>("getAttributeValue", new object[0])
		};
		AndroidJNI.PopLocalFrame(IntPtr.Zero);
		return result;
	}

	private static int GetPurchasedVirtualGoodCount()
	{
		return tapjoyConnectInstance.Call<int>("getPurchasedItemsCount", new object[0]);
	}
}
