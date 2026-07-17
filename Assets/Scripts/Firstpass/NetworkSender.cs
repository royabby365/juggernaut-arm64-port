using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkSender
{
	private static readonly string ServerUrl = Globals.MainServerUrl;

	private static readonly string MetricUrl = ServerUrl + "metric.php";

	private static readonly string StatUrl = ServerUrl + "action.php";

	private static readonly string StatRequestUrl = ServerUrl + "stats.php";

	private static readonly string MoneyRequestUrl = ServerUrl + "sync.php";

	public static IEnumerator SendMetric(int type, int value, int level, int objectInt)
	{
		string url = $"{MetricUrl}?type={type}&value={value}&level={level}&object_id={objectInt}&product={Escape(UniqueData.BundleIdentifier)}";
		yield return new WWW(url);
	}

	public static IEnumerator SendStat(int eventId, int subEventId)
	{
		string token = string.Empty;
		string odin1 = string.Empty;
		string openUDID = string.Empty;
		int developer = 0;
		if (eventId == 10)
		{
			developer = 0;
			token = $"{developer}_{UniqueData.DeviceToken}";
		}
		if (eventId == 11)
		{
			odin1 = UniqueData.ODIN1;
			openUDID = UniqueData.OpenUDID;
		}
		string url = $"{StatUrl}?dev={Escape(SystemInfo.deviceModel)}&id={Escape(UniqueData.UniqueIdentifier)}&udid={Escape(UniqueData.MacAdressGlobalUniqueIdentifier)}&version={Escape(SystemInfo.operatingSystem)}&event_id={eventId}&time={UniqueData.GetSecondsSince1970()}&sub_event_id={subEventId}&product={Escape(UniqueData.BundleIdentifier)}&app_version={Escape(UniqueData.GameVersion)}&devToken={Escape(token)}&odin={Escape(odin1)}&oudid={Escape(openUDID)}";
		yield return new WWW(url);
	}

	public static IEnumerator SendStatRequest(string body)
	{
		yield return new WWW(postData: Encoding.UTF8.GetBytes(body), url: StatRequestUrl);
	}

	public static IEnumerator SendMoneyRequest(ActionD<string> onLoad)
	{
		string url = $"{MoneyRequestUrl}?akey={Escape(UniqueData.BundleIdentifier)}&uid={Escape(UniqueData.GetCurrentSlotUUID())}&s={Escape(UniqueData.SlotId)}&vc={UnityApi.GetPackageVersionCode()}";
		Utils.LogForce("SendMoneyRequest, url: " + url);
		WWW www = new WWW(url);
		yield return www;
		if (www.error != null || www.bytes == null)
		{
			onLoad(string.Empty);
		}
		else
		{
			onLoad(www.text);
		}
	}

	public static Dictionary<string, string> GetStatRequestDict()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["uid"] = Escape(UniqueData.GetCurrentSlotUUID());
		dictionary["akey"] = Escape(UniqueData.BundleIdentifier);
		dictionary["did"] = Escape(UniqueData.UniqueIdentifier);
		dictionary["appv"] = Escape($"{UniqueData.GameVersion}:{UniqueData.BuildVersion}");
		dictionary["maddr"] = Escape(UniqueData.MacAdressGlobalUniqueIdentifier);
		dictionary["osv"] = Escape(SystemInfo.operatingSystem);
		dictionary["dt"] = Escape(SystemInfo.deviceModel);
		dictionary["s"] = Escape(UniqueData.SlotId);
		return dictionary;
	}

	public static string Escape(string st)
	{
		return WWW.EscapeURL(st, Encoding.UTF8);
	}
}
