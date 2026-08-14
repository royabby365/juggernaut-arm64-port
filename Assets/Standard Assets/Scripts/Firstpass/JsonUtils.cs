using System;
using System.Collections.Generic;
using System.Text;
using LitJson;
using UnityEngine;

public class JsonUtils
{
	public class GetValues
	{
		public class MyPair<T>
		{
			public int Key;

			public T Value;

			public static int CompareTo(MyPair<T> x, MyPair<T> y)
			{
				return x.Key.CompareTo(y.Key);
			}
		}

		private Dictionary<string, object> _hash;

		private List<ActionD> _lazyList;

		public Dictionary<string, object> Hash
		{
			get
			{
				return _hash;
			}
			set
			{
				_hash = value;
			}
		}

		public GetValues(List<ActionD> lazyList, Dictionary<string, object> hash)
		{
			_hash = hash;
			_lazyList = lazyList;
		}

		public GetValues Get(out int value, string name, int ifNull)
		{
			value = JsonGetInt(name, _hash, ifNull);
			return this;
		}

		public GetValues Get(out int value, string name)
		{
			value = JsonGetInt(name, _hash);
			return this;
		}

		public GetValues Get(out float value, string name, float def)
		{
			value = JsonGetFloat(name, _hash, def);
			return this;
		}

		public GetValues Get(out float value, string name)
		{
			value = JsonGetFloat(name, _hash);
			return this;
		}

		public GetValues Get(out double value, string name)
		{
			value = JsonGetFloat(name, _hash);
			return this;
		}

		public GetValues Get(out bool value, string name, bool defaultValue)
		{
			if (_hash.ContainsKey(name))
			{
				object obj = _hash[name];
				if (obj != null)
				{
					Type type = obj.GetType();
					if (type == typeof(bool))
					{
						value = (bool)obj;
						return this;
					}
					switch (obj as string)
					{
					case "true":
						value = true;
						return this;
					case "false":
						value = false;
						return this;
					}
				}
			}
			value = defaultValue;
			return this;
		}

		public GetValues Get(out bool value, string name)
		{
			Invs.Inv(_hash.ContainsKey(name), "JsonGetBool", name);
			object obj = _hash[name];
			Invs.Inv(obj != null, "JsonGetBool value=null", name);
			try
			{
				Type type = obj.GetType();
				if (type == typeof(bool))
				{
					value = (bool)obj;
					return this;
				}
				switch (obj as string)
				{
				case "true":
					value = true;
					return this;
				case "false":
					value = false;
					return this;
				default:
					Invs.Inv(false, "Has ", type.Name, " Need bool");
					break;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Get(Dictionary<string,object>)." + name + " value = " + obj.ToString() + " Msg:" + ex.Message, ex);
			}
			value = false;
			return this;
		}

		public GetValues Get(out string value, string name)
		{
			value = JsonGet(name, _hash, string.Empty);
			return this;
		}

		public GetValues Get(out Dictionary<string, object> value, string name)
		{
			value = JsonGet(name, _hash, new Dictionary<string, object>());
			return this;
		}

		public GetValues Get(out string value, string name, string defaultValue)
		{
			if (_hash.ContainsKey(name))
			{
				object obj = _hash[name];
				string text = obj as string;
				value = ((text == null) ? defaultValue : text);
			}
			else
			{
				value = defaultValue;
			}
			return this;
		}

		public GetValues GetLazyInt(string name, int ifNull, ActionD<int> action)
		{
			int value = JsonGetInt(name, _hash, ifNull);
			if (value != ifNull)
			{
				_lazyList.Add(delegate
				{
					try
					{
						action(value);
					}
					catch (Exception)
					{
						Utils.LogForce("GetLazyInt failed", name);
						throw;
					}
				});
			}
			return this;
		}

		public GetValues GetLazyIntFrom<T>(string name, int ifNull, Dictionary<int, T> dict, Action<T> action)
		{
			return GetLazyIntFrom(name, ifNull, dict, action, null);
		}

		public GetValues GetLazyIntFrom<T>(string name, int ifNull, Dictionary<int, T> dict, Action<T> action, object debugInfo)
		{
			try
			{
				int value = JsonGetInt(name, _hash, ifNull);
				if (value != ifNull)
				{
					_lazyList.Add(delegate
					{
						try
						{
							action(dict[value]);
						}
						catch (Exception ex)
						{
							string message = ex.Message;
							throw new Exception("GetLazyIntFrom failed. Type=" + typeof(T).Name + " name=" + name + " value=" + value + " " + message + ". " + ((debugInfo != null) ? debugInfo.ToString() : string.Empty), ex);
						}
					});
				}
			}
			catch (Exception innerException)
			{
				throw new Exception("GetLazyIntFrom failed. " + ((debugInfo != null) ? debugInfo.ToString() : string.Empty), innerException);
			}
			return this;
		}

		public GetValues GetLazyIntFrom<T>(string name, int ifNull, Dictionary<int, T> dict, Action<T> action, object debugInfo, out int intValue)
		{
			try
			{
				int value = JsonGetInt(name, _hash, ifNull);
				intValue = value;
				if (value != ifNull)
				{
					_lazyList.Add(delegate
					{
						try
						{
							action(dict[value]);
						}
						catch (Exception innerException2)
						{
							throw new Exception("GetLazyIntFrom failed. Type=" + typeof(T).Name + " name=" + name + " value=" + value + ". " + ((debugInfo != null) ? debugInfo.ToString() : string.Empty), innerException2);
						}
					});
				}
			}
			catch (Exception innerException)
			{
				throw new Exception("GetLazyIntFrom failed. " + ((debugInfo != null) ? debugInfo.ToString() : string.Empty), innerException);
			}
			return this;
		}

		public GetValues GetAlwaysLazyIntFrom<T>(string name, int ifNull, Dictionary<int, T> dict, Action<T> action)
		{
			int value = JsonGetInt(name, _hash, ifNull);
			Invs.Inv(value != ifNull, "value != ifNull", name);
			_lazyList.Add(delegate
			{
				try
				{
					action(dict[value]);
				}
				catch (Exception innerException)
				{
					Utils.LogForce("GetLazyIntFrom failed. Type=", typeof(T).Name, "name=", name, " value=", value);
					throw new Exception("GetLazyIntFrom failed. Type=" + typeof(T).Name + " name=" + name + " value=" + value, innerException);
				}
			});
			return this;
		}

		public GetValues Get<T, T1>(out T[] array, string name, FuncD<T1, T> getElement)
		{
			if (_hash[name] == null)
			{
				array = new T[0];
				return this;
			}
			Dictionary<string, object> dictionary = JsonGet<Dictionary<string, object>>(name, _hash);
			List<MyPair<T>> list = new List<MyPair<T>>();
			foreach (string key in dictionary.Keys)
			{
				T val = getElement(JsonGet<T1>(key, dictionary));
				if (val != null)
				{
					list.Add(new MyPair<T>
					{
						Key = int.Parse(key),
						Value = val
					});
				}
			}
			List<T> list2 = new List<T>(list.Count);
			list.Sort(MyPair<T>.CompareTo);
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(list[i].Value);
			}
			array = list2.ToArray();
			return this;
		}

		public GetValues Get<T, T1>(out T[] array, T[] defaultArray, string name, FuncD<T1, T> getElement)
		{
			Dictionary<string, object> dictionary = JsonGet<Dictionary<string, object>>(name, _hash, null);
			if (dictionary == null)
			{
				array = defaultArray;
				return this;
			}
			List<int> list = new List<int>();
			List<T> list2 = new List<T>();
			foreach (string key in dictionary.Keys)
			{
				T val = getElement(JsonGet<T1>(key, dictionary));
				if (val != null)
				{
					list.Add(int.Parse(key));
					list2.Add(val);
				}
			}
			List<T> list3 = new List<T>(list2.Count);
			for (int i = 0; i < list2.Count; i++)
			{
				list3.Add(default(T));
			}
			for (int j = 0; j < list2.Count; j++)
			{
				list3[list[j] - 1] = list2[j];
			}
			array = list3.ToArray();
			return this;
		}

		public GetValues Get<T>(List<T> list, string name, FuncD<Dictionary<string, object>, T> getElement) where T : class
		{
			Dictionary<string, object> dictionary = JsonGet<Dictionary<string, object>>(name, _hash);
			foreach (string key in dictionary.Keys)
			{
				T val = getElement(JsonGet<Dictionary<string, object>>(key, dictionary));
				if (val != null)
				{
					list.Add(val);
				}
			}
			return this;
		}

		public GetValues Foreach(string name, ActionD<string, object> action)
		{
			Dictionary<string, object> dictionary = JsonGet<Dictionary<string, object>>(name, _hash);
			foreach (string key in dictionary.Keys)
			{
				action(key, dictionary[key]);
			}
			return this;
		}

		public GetValues Foreach<V>(string name, ActionD<string, V> action)
		{
			Dictionary<string, object> dictionary = JsonGet<Dictionary<string, object>>(name, _hash);
			foreach (string key in dictionary.Keys)
			{
				action(key, (V)dictionary[key]);
			}
			return this;
		}

		public GetValues ForeachLazy<V>(string name, ActionD<string, V> action)
		{
			Dictionary<string, object> d = JsonGet<Dictionary<string, object>>(name, _hash, null);
			if (d != null)
			{
				_lazyList.Add(delegate
				{
					foreach (string key in d.Keys)
					{
						try
						{
							action(key, (V)Convert.ChangeType(d[key], typeof(V)));
						}
						catch (Exception innerException)
						{
							throw new Exception($"ForeachLazy failed. Name={name} key={key}[{key.GetType().Name}] d[key]={d[key]}[{d[key].GetType().Name}]", innerException);
						}
					}
				});
			}
			return this;
		}

		internal GetValues Get(out Vector2 r, string p)
		{
			r = Vector2.zero;
			if (_hash.ContainsKey(p) && _hash[p] is string text)
			{
				string[] array = text.Split(';');
				if (array.Length == 2 && int.TryParse(array[0], out var result) && int.TryParse(array[1], out var result2))
				{
					r = new Vector2(result, result2);
				}
			}
			return this;
		}
	}

	public JsonUtils(Dictionary<string, object> hash)
	{
	}

	public static T JsonTryGet<T>(string name, Dictionary<string, object> t, T defaultValue)
	{
		if (t.ContainsKey(name))
		{
			object obj = t[name];
			if (obj == null)
			{
				return defaultValue;
			}
			if (obj.GetType() != typeof(T))
			{
				return defaultValue;
			}
			return (T)obj;
		}
		return defaultValue;
	}

	public static Dictionary<string, object> JsonGetHashtable(string name, Dictionary<string, object> t)
	{
		try
		{
			object obj = t[name];
			Invs.Inv(obj.GetType() == typeof(Dictionary<string, object>), "Has ", obj.GetType().Name, " Need Dictionary<string,object>");
			return (Dictionary<string, object>)obj;
		}
		catch (Exception ex)
		{
			throw new Exception("Get(Dictionary<string,object>)." + name + " Msg:" + ex.Message, ex);
		}
	}

	public static T JsonGet<T>(string name, Dictionary<string, object> t)
	{
		try
		{
			object obj = t[name];
			Invs.Inv(obj.GetType() == typeof(T), "Has ", obj.GetType().Name, " Need ", typeof(T).Name);
			return (T)obj;
		}
		catch (Exception ex)
		{
			throw new Exception("Get(Dictionary<string,object>)." + name + " Msg:" + ex.Message, ex);
		}
	}

	public static T JsonGet<T>(string name, Dictionary<string, object> t, T ifNull)
	{
		try
		{
			if (!t.ContainsKey(name))
			{
				return ifNull;
			}
			object obj = t[name];
			if (obj == null)
			{
				return ifNull;
			}
			Invs.Inv(obj.GetType() == typeof(T), "Has ", obj.GetType().Name, " Need ", typeof(T).Name, " name=", name);
			return (T)obj;
		}
		catch (Exception ex)
		{
			throw new Exception("Get(Dictionary<string,object>)." + name + " " + typeof(T).Name + " Msg:" + ex.Message, ex);
		}
	}

	public static float JsonGetFloat(string name, Dictionary<string, object> t)
	{
		object obj = t[name];
		if (obj.GetType() == typeof(float))
		{
			return (float)obj;
		}
		if (obj.GetType() == typeof(long))
		{
			return (long)obj;
		}
		if (obj.GetType() == typeof(double))
		{
			return (float)(double)obj;
		}
		Invs.Inv(false, "Has ", obj.GetType().Name, " Need float name=", name);
		return 0f;
	}

	public static double JsonGetDouble(string name, Dictionary<string, object> t)
	{
		return JsonGet<long>(name, t);
	}

	public static int JsonGetInt(string name, Dictionary<string, object> t, int ifNull)
	{
		try
		{
			if (!t.ContainsKey(name))
			{
				return ifNull;
			}
			object obj = t[name];
			if (obj == null)
			{
				return ifNull;
			}
			Type type = obj.GetType();
			if (type == typeof(int))
			{
				return (int)obj;
			}
			if (type == typeof(string))
			{
				if (string.IsNullOrEmpty((string)obj))
				{
					return ifNull;
				}
				return int.Parse((string)obj);
			}
			if (type == typeof(long))
			{
				return (int)(long)obj;
			}
			if (type == typeof(double))
			{
				return (int)(double)obj;
			}
			Invs.Inv(false, "Has ", type.Name, " Need int", "name=", name);
		}
		catch (Exception ex)
		{
			throw new Exception("Get(Dictionary<string,object>)." + name + " Msg:" + ex.Message, ex);
		}
		return -1;
	}

	public static int JsonGetInt(string name, Dictionary<string, object> t)
	{
		Invs.Inv(t.ContainsKey(name), "JsonGetInt no key", name);
		object obj = t[name];
		Invs.Inv(obj != null, "JsonGetInt value=null", name);
		try
		{
			Type type = obj.GetType();
			if (type == typeof(int))
			{
				return (int)obj;
			}
			if (type == typeof(string))
			{
				string text = obj as string;
				if (string.IsNullOrEmpty(text))
				{
					return 0;
				}
				return int.Parse(text);
			}
			if (type == typeof(long))
			{
				return (int)(long)obj;
			}
			if (type == typeof(double))
			{
				return (int)(double)obj;
			}
			Invs.Inv(false, "Has ", type.Name, " Need int");
		}
		catch (Exception ex)
		{
			throw new Exception("Get(Dictionary<string,object>). name=[" + name + "] value = [" + obj.ToString() + "] Msg:" + ex.Message, ex);
		}
		return -1;
	}

	public static float JsonGetFloat(string name, Dictionary<string, object> t, float def)
	{
		if (!t.ContainsKey(name))
		{
			return def;
		}
		object obj = t[name];
		if (obj == null)
		{
			return def;
		}
		try
		{
			Type type = obj.GetType();
			if (type == typeof(float))
			{
				return (float)obj;
			}
			if (type == typeof(string))
			{
				return float.Parse((string)obj);
			}
			if (type == typeof(long))
			{
				return (long)obj;
			}
			if (type == typeof(double))
			{
				return (float)(double)obj;
			}
			Invs.Inv(false, "Has ", type.Name, " Need float");
		}
		catch (Exception ex)
		{
			throw new Exception("Get(Dictionary<string,object>)." + name + " value = " + obj.ToString() + " Msg:" + ex.Message, ex);
		}
		return -1f;
	}

	public static void JsonGet<T>(out T r, string name, Dictionary<string, object> t)
	{
		try
		{
			object obj = t[name];
			Invs.Inv(obj.GetType() == typeof(T), "Has ", obj.GetType().Name, " Need ", typeof(T).Name);
			r = (T)obj;
		}
		catch (Exception ex)
		{
			throw new Exception("Get(Dictionary<string,object>)." + name + " Msg:" + ex.Message, ex);
		}
	}

	public static string AsTextJson(Action<JsonWriter> action)
	{
		StringBuilder stringBuilder = new StringBuilder();
		JsonWriter obj = new JsonWriter(stringBuilder);
		action(obj);
		return stringBuilder.ToString();
	}
}
