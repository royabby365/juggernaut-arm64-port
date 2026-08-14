using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UniqueData
{
	public static readonly string UniqueIdentifier = string.Empty;

	public static string BundleIdentifier = string.Empty;

	public static string MacAdressGlobalUniqueIdentifier = string.Empty;

	public static string GameVersion = string.Empty;

	public static string ODIN1 = string.Empty;

	public static string OpenUDID = string.Empty;

	public static string DeviceToken = string.Empty;

	public static int BuildVersion = 0;

	public static Dictionary<string, string> SlotUUIDs = new Dictionary<string, string>();

	public static string SlotId = string.Empty;

	public static bool InGame = false;

	private static readonly string SlotsSaveBasePath = Application.persistentDataPath;

	private static DateTime Date1970 = new DateTime(1970, 1, 1);

	public static void SaveSlots()
	{
		string text = Utils.ToString(SlotUUIDs);
		Utils.Log("SaveSlots, uuids: ", text);
		try
		{
			string slotsSaveBasePath = SlotsSaveBasePath;
			if (!Directory.Exists(slotsSaveBasePath))
			{
				Directory.CreateDirectory(slotsSaveBasePath);
			}
			Utils.WriteAllText(slotsSaveBasePath + "/slots", text);
		}
		catch (Exception ex)
		{
			Utils.Log("Exception when saving slots: ", ex.Message);
		}
	}

	public static void LoadSlots()
	{
		string text = string.Empty;
		try
		{
			text = Utils.ReadAllText(SlotsSaveBasePath + "/slots");
		}
		catch (Exception ex)
		{
			Utils.Log("Exception when loading slots: ", ex.Message);
		}
		if (string.IsNullOrEmpty(text))
		{
			text = PlayerPrefs.GetString(Globals.PlayerPrefSlotUUIDs);
		}
		else
		{
			PlayerPrefs.SetString(Globals.PlayerPrefSlotUUIDs, string.Empty);
			PlayerPrefs.Save();
		}
		Utils.Log("LoadSlots, uuids: ", text);
		if (!string.IsNullOrEmpty(text))
		{
			SlotUUIDs = Utils.ToDict(text);
		}
	}

	public static void SetDeviceTokenDescription(string tokenDescription)
	{
		DeviceToken = tokenDescription.Replace("<", string.Empty).Replace(">", string.Empty).Replace(" ", string.Empty);
	}

	public static string GetCurrentSlotUUID()
	{
		string value = string.Empty;
		SlotUUIDs.TryGetValue(SlotId, out value);
		return value;
	}

	public static long GetSecondsSince1970()
	{
		TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks - Date1970.Ticks);
		return (long)timeSpan.TotalSeconds;
	}
}
