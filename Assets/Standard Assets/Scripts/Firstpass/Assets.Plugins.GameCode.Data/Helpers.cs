using System.Collections.Generic;

namespace Assets.Plugins.GameCode.Data
{

public static class Helpers
{
	public static ServerData.Location.BotLocationInfo[] FromProxy(this BotLocationInfoProxy[] data)
	{
		if (data == null)
		{
			return null;
		}
		ServerData.Location.BotLocationInfo[] array = new ServerData.Location.BotLocationInfo[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			array[i] = data[i];
		}
		return array;
	}

	public static BotLocationInfoProxy[] ToProxy(this ServerData.Location.BotLocationInfo[] data)
	{
		if (data == null)
		{
			return null;
		}
		BotLocationInfoProxy[] array = new BotLocationInfoProxy[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			array[i] = data[i];
		}
		return array;
	}

	public static List<ServerData.Bonus.DropElement> FromProxy(this IEnumerable<DropElementProxy> data)
	{
		if (data == null)
		{
			return null;
		}
		List<ServerData.Bonus.DropElement> list = new List<ServerData.Bonus.DropElement>();
		foreach (DropElementProxy datum in data)
		{
			list.Add(datum);
		}
		return list;
	}

	public static List<DropElementProxy> ToProxy(this IEnumerable<ServerData.Bonus.DropElement> data)
	{
		if (data == null)
		{
			return null;
		}
		List<DropElementProxy> list = new List<DropElementProxy>();
		foreach (ServerData.Bonus.DropElement datum in data)
		{
			list.Add(datum);
		}
		return list;
	}

	public static Dictionary<string, ObjectProxy> ToProxy(this Dictionary<string, object> data)
	{
		if (data == null)
		{
			return null;
		}
		Dictionary<string, ObjectProxy> dictionary = new Dictionary<string, ObjectProxy>();
		foreach (KeyValuePair<string, object> datum in data)
		{
			dictionary.Add(datum.Key, ObjectProxy.FromObject(datum.Value));
		}
		return dictionary;
	}

	public static Dictionary<string, object> FromProxy(this Dictionary<string, ObjectProxy> data)
	{
		if (data == null)
		{
			return null;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (KeyValuePair<string, ObjectProxy> datum in data)
		{
			dictionary.Add(datum.Key, ObjectProxy.ToObject(datum.Value));
		}
		return dictionary;
	}

	public static ServerData.Location.ChestLocationInfo[] FromProxy(this ChestLocationInfoProxy[] data)
	{
		if (data == null)
		{
			return null;
		}
		ServerData.Location.ChestLocationInfo[] array = new ServerData.Location.ChestLocationInfo[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			array[i] = data[i];
		}
		return array;
	}

	public static ChestLocationInfoProxy[] ToProxy(this ServerData.Location.ChestLocationInfo[] data)
	{
		if (data == null)
		{
			return null;
		}
		ChestLocationInfoProxy[] array = new ChestLocationInfoProxy[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			array[i] = data[i];
		}
		return array;
	}
}
}
