using UnityEngine;

public class AppsFirePlugin
{
	private static AndroidJavaClass plugin;

	private static void Init()
	{
		if (plugin == null)
		{
			plugin = new AndroidJavaClass("ru.mail.games.juggernaut.AppsFirePlugin");
		}
	}

	public static void ShowNotificationsBar()
	{
		Init();
		plugin.CallStatic("showNotificationsBar");
	}

	public static void ShowNotifications()
	{
		Init();
		plugin.CallStatic("showNotifications");
	}

	public static int GetNumberOfPendingNotifications()
	{
		Init();
		return plugin.CallStatic<int>("getNumberOfPendingNotifications", new object[0]);
	}
}
