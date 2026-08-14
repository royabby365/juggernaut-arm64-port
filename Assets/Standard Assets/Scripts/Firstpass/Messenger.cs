using System;
using System.Collections.Generic;

public static class Messenger
{
	private class UserRef : IDisposable
	{
		public Callback callback;

		public string msg;

		public void Dispose()
		{
			if (callback != null)
			{
				RemoveListener(msg, callback);
				callback = null;
			}
		}
	}

	private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static IDisposable AddListener(string eventType, Callback handler)
	{
		lock (eventTable)
		{
			if (!eventTable.ContainsKey(eventType))
			{
				eventTable.Add(eventType, null);
			}
			eventTable[eventType] = (Callback)Delegate.Combine((Callback)eventTable[eventType], handler);
			UserRef userRef = new UserRef();
			userRef.callback = handler;
			userRef.msg = eventType;
			return userRef;
		}
	}

	public static void RemoveListener(string eventType, Callback handler)
	{
		lock (eventTable)
		{
			if (eventTable.ContainsKey(eventType))
			{
				eventTable[eventType] = (Callback)Delegate.Remove((Callback)eventTable[eventType], handler);
				if (eventTable[eventType] == null)
				{
					eventTable.Remove(eventType);
				}
			}
		}
	}

	public static void Invoke(string eventType)
	{
		if (eventTable.TryGetValue(eventType, out var value))
		{
			((Callback)value)?.Invoke();
		}
	}

	public static void Invoke<T1>(string eventType, T1 arg1)
	{
		Messenger<T1>.Invoke(eventType, arg1);
	}

	public static void Invoke<T1, T2>(string eventType, T1 arg1, T2 arg2)
	{
		Messenger<T1, T2>.Invoke(eventType, arg1, arg2);
	}

	public static void Invoke<T1, T2, T3>(string eventType, T1 arg1, T2 arg2, T3 arg3)
	{
		Messenger<T1, T2, T3>.Invoke(eventType, arg1, arg2, arg3);
	}

	public static void Invoke<T1, T2, T3, T4>(string eventType, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		Messenger<T1, T2, T3, T4>.Invoke(eventType, arg1, arg2, arg3, arg4);
	}
}
public static class Messenger<T>
{
	private class UserRef : IDisposable
	{
		public Callback<T> callback;

		public string msg;

		public void Dispose()
		{
			if (callback != null)
			{
				Messenger<T>.RemoveListener(msg, callback);
				callback = null;
			}
		}
	}

	private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static IDisposable AddListener(string eventType, Callback<T> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.ContainsKey(eventType))
			{
				eventTable.Add(eventType, null);
			}
			eventTable[eventType] = (Callback<T>)Delegate.Combine((Callback<T>)eventTable[eventType], handler);
			UserRef userRef = new UserRef();
			userRef.callback = handler;
			userRef.msg = eventType;
			return userRef;
		}
	}

	public static void RemoveListener(string eventType, Callback<T> handler)
	{
		lock (eventTable)
		{
			if (eventTable.ContainsKey(eventType))
			{
				eventTable[eventType] = (Callback<T>)Delegate.Remove((Callback<T>)eventTable[eventType], handler);
				if (eventTable[eventType] == null)
				{
					eventTable.Remove(eventType);
				}
			}
		}
	}

	public static void Invoke(string eventType, T arg1)
	{
		if (eventTable.TryGetValue(eventType, out var value))
		{
			((Callback<T>)value)?.Invoke(arg1);
		}
	}
}
public static class Messenger<T, U>
{
	private class UserRef : IDisposable
	{
		public Callback<T, U> callback;

		public string msg;

		public void Dispose()
		{
			if (callback != null)
			{
				Messenger<T, U>.RemoveListener(msg, callback);
				callback = null;
			}
		}
	}

	private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static IDisposable AddListener(string eventType, Callback<T, U> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.ContainsKey(eventType))
			{
				eventTable.Add(eventType, null);
			}
			eventTable[eventType] = (Callback<T, U>)Delegate.Combine((Callback<T, U>)eventTable[eventType], handler);
			UserRef userRef = new UserRef();
			userRef.callback = handler;
			userRef.msg = eventType;
			return userRef;
		}
	}

	public static void RemoveListener(string eventType, Callback<T, U> handler)
	{
		lock (eventTable)
		{
			if (eventTable.ContainsKey(eventType))
			{
				eventTable[eventType] = (Callback<T, U>)Delegate.Remove((Callback<T, U>)eventTable[eventType], handler);
				if (eventTable[eventType] == null)
				{
					eventTable.Remove(eventType);
				}
			}
		}
	}

	public static void Invoke(string eventType, T arg1, U arg2)
	{
		if (eventTable.TryGetValue(eventType, out var value))
		{
			((Callback<T, U>)value)?.Invoke(arg1, arg2);
		}
	}
}
public static class Messenger<T, U, W>
{
	private class UserRef : IDisposable
	{
		public Callback<T, U, W> callback;

		public string msg;

		public void Dispose()
		{
			if (callback != null)
			{
				Messenger<T, U, W>.RemoveListener(msg, callback);
				callback = null;
			}
		}
	}

	private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static IDisposable AddListener(string eventType, Callback<T, U, W> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.ContainsKey(eventType))
			{
				eventTable.Add(eventType, null);
			}
			eventTable[eventType] = (Callback<T, U, W>)Delegate.Combine((Callback<T, U, W>)eventTable[eventType], handler);
			UserRef userRef = new UserRef();
			userRef.callback = handler;
			userRef.msg = eventType;
			return userRef;
		}
	}

	public static void RemoveListener(string eventType, Callback<T, U, W> handler)
	{
		lock (eventTable)
		{
			if (eventTable.ContainsKey(eventType))
			{
				eventTable[eventType] = (Callback<T, U, W>)Delegate.Remove((Callback<T, U, W>)eventTable[eventType], handler);
				if (eventTable[eventType] == null)
				{
					eventTable.Remove(eventType);
				}
			}
		}
	}

	public static void Invoke(string eventType, T arg1, U arg2, W arg3)
	{
		if (eventTable.TryGetValue(eventType, out var value))
		{
			((Callback<T, U, W>)value)?.Invoke(arg1, arg2, arg3);
		}
	}
}
public static class Messenger<T, U, W, Z>
{
	private class UserRef : IDisposable
	{
		public Callback<T, U, W, Z> callback;

		public string msg;

		public void Dispose()
		{
			if (callback != null)
			{
				Messenger<T, U, W, Z>.RemoveListener(msg, callback);
				callback = null;
			}
		}
	}

	private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static IDisposable AddListener(string eventType, Callback<T, U, W, Z> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.ContainsKey(eventType))
			{
				eventTable.Add(eventType, null);
			}
			eventTable[eventType] = (Callback<T, U, W, Z>)Delegate.Combine((Callback<T, U, W, Z>)eventTable[eventType], handler);
			UserRef userRef = new UserRef();
			userRef.callback = handler;
			userRef.msg = eventType;
			return userRef;
		}
	}

	public static void RemoveListener(string eventType, Callback<T, U, W, Z> handler)
	{
		lock (eventTable)
		{
			if (eventTable.ContainsKey(eventType))
			{
				eventTable[eventType] = (Callback<T, U, W, Z>)Delegate.Remove((Callback<T, U, W, Z>)eventTable[eventType], handler);
				if (eventTable[eventType] == null)
				{
					eventTable.Remove(eventType);
				}
			}
		}
	}

	public static void Invoke(string eventType, T arg1, U arg2, W arg3, Z arg4)
	{
		if (eventTable.TryGetValue(eventType, out var value))
		{
			((Callback<T, U, W, Z>)value)?.Invoke(arg1, arg2, arg3, arg4);
		}
	}
}
