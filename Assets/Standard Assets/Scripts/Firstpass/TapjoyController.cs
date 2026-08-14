using UnityEngine;

public class TapjoyController : MonoBehaviour
{
	private static string PROD_APP_ID = "d92f9a80-7e50-475d-b164-cbc4ea918b59";

	private static string PROD_APP_KEY = "b3CfqB5wJNhe727Lriw9";

	private static string DEV_APP_ID = "49eb0b8d-2e14-4546-a4a8-a49af9fba285";

	private static string DEV_APP_KEY = "jWnWVCfFY6JKlaJ3maI6";

	private string tapPointsLabel = string.Empty;

	private string earnedLabel = string.Empty;

	private bool hideBanner;

	private int bannerY = -9999;

	private string showOffers = "show offers";

	private string featuredApp = "show full screen ad";

	private string displayAd = "banner ad";

	private string getPoints = "get tap points";

	private string spendPoints = "spend points";

	private string awardPoints = "award points";

	private string bannerHide = "hide banner";

	private string vg = "virtual goods";

	private string vgPurchasedItems = "get purchased vg items";

	private void Start()
	{
		TapjoyPlugin.EnableLogging(enable: true);
		TapjoyPlugin.SetCallbackHandler("TapjoyManager");
		TapjoyPlugin.RequestTapjoyConnect(PROD_APP_ID, PROD_APP_KEY);
		TapjoyPlugin.SetEarnedPointsNotifier();
		TapjoyPlugin.SetUserDefinedColorWithIntValue(21913);
	}

	public void TapPointsLoaded(string message)
	{
		int num = TapjoyPlugin.QueryTapPoints();
		MonoBehaviour.print("TapPointsLoaded: " + message + ", total tap points: " + num);
	}

	public void TapPointsLoadedError(string message)
	{
		MonoBehaviour.print("TapPointsLoadedError: " + message);
		tapPointsLabel = "TapPointsLoadedError: " + message;
	}

	public void TapPointsSpent(string message)
	{
		MonoBehaviour.print("TapPointsSpent: " + message);
		tapPointsLabel = "Total TapPoints: " + TapjoyPlugin.QueryTapPoints();
		earnedLabel = string.Empty;
	}

	public void TapPointsSpendError(string message)
	{
		MonoBehaviour.print("TapPointsSpendError: " + message);
		tapPointsLabel = "TapPointsSpendError: " + message;
		earnedLabel = string.Empty;
	}

	public void TapPointsAwarded(string message)
	{
		MonoBehaviour.print("TapPointsAwarded: " + message);
		tapPointsLabel = "Total TapPoints: " + TapjoyPlugin.QueryTapPoints();
		earnedLabel = string.Empty;
	}

	public void TapPointsAwardError(string message)
	{
		MonoBehaviour.print("TapPointsAwardError: " + message);
		tapPointsLabel = "TapPointsAwardError: " + message;
		earnedLabel = string.Empty;
	}

	public void CurrencyEarned(string message)
	{
		MonoBehaviour.print("CurrencyEarned: " + message);
		earnedLabel = "Just earned currency: " + message;
		if (!string.IsNullOrEmpty(message))
		{
			int num = int.Parse(message);
			if (num > 0)
			{
				string message2 = $"GiveMoney diamand {num}";
				GameObject.Find("__main_menu").GetComponent<MainMenu>().IOSCall(message2);
				TapjoyPlugin.SpendTapPoints(num);
			}
		}
	}

	public void FeaturedAppLoaded(string message)
	{
		MonoBehaviour.print("FeaturedAppLoaded: " + message);
		tapPointsLabel = "FeaturedAppLoaded: " + message;
		TapjoyPlugin.ShowFeaturedAppFullScreenAd();
		earnedLabel = string.Empty;
	}

	public void FeaturedAppError(string message)
	{
		MonoBehaviour.print("FeaturedAppError: " + message);
		tapPointsLabel = "FeaturedAppError: " + message;
		earnedLabel = string.Empty;
	}

	public void DisplayAdLoaded(string message)
	{
		MonoBehaviour.print("DisplayAdLoaded: " + message);
		tapPointsLabel = "DisplayAdLoaded: " + message;
		if (!hideBanner)
		{
			TapjoyPlugin.ShowDisplayAd();
		}
	}

	public void DisplayAdError(string message)
	{
		MonoBehaviour.print("DisplayAdError: " + message);
		tapPointsLabel = "DisplayAdError: " + message;
	}

	public void VideoReady(string message)
	{
		MonoBehaviour.print("VideoReady: " + message);
		tapPointsLabel = "VideoReady: " + message;
	}

	public void VideoError(string message)
	{
		MonoBehaviour.print("VideoError: " + message);
		tapPointsLabel = "VideoError: " + message;
	}

	public void VideoComplete(string message)
	{
		MonoBehaviour.print("VideoComplete: " + message);
	}

	public void VirtualGoodsDownloadListener(string message)
	{
		MonoBehaviour.print("VirtualGoodsDownloadListener: " + message);
	}
}
