using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Yarx.Collections;

public static class Utils
{
	private class PushData : IComparable
	{
		internal int Id;

		internal string Name;

		int IComparable.CompareTo(object obj)
		{
			PushData pushData = (PushData)obj;
			return pushData.Id.CompareTo(pushData.Id);
		}
	}

	public delegate void OnWWWBinarySuccessD(string path, byte[] www);

	public delegate void OnWWWTextSuccessD(string path, string www);

	public delegate void OnWWWSuccessD(string path, WWW www);

	public delegate void OnWWWErrorD(string path, string errorMessage);

	public delegate void RandomActionD(int number, int indexInList);

	internal static readonly string Version = "2011.08.14 development";

	internal static readonly bool LogToDebug = Globals.MyDebug;

	private static List<PushData> _pushData = new List<PushData>();

	public static string ReadAllText(string path)
	{
		return File.ReadAllText(path);
	}

	internal static void Dispose<T>(T some) where T : class
	{
		if (some is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	internal static void DisposeAndSetNull<T>(ref T some) where T : class
	{
		if (some is IDisposable disposable)
		{
			disposable.Dispose();
			some = (T)null;
		}
	}

	internal static T TakeAndSetNull<T>(ref T value) where T : class
	{
		T result = value;
		value = (T)null;
		return result;
	}

	public static void WriteAllText(string path, string text)
	{
		File.WriteAllText(path, text);
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		File.WriteAllBytes(path, bytes);
	}

	public static void WriteAllLines(string path, string[] lines)
	{
		string text = string.Join("\n", lines);
		WriteAllText(path, text);
	}

	public static byte[] ReadAllBytes(string path)
	{
		using FileStream stream = new FileStream(path, FileMode.Open);
		return Util.ReadFully(stream);
	}

	public static byte[] StringToBytes(string text)
	{
		byte[] array = new byte[text.Length * 2];
		Buffer.BlockCopy(text.ToCharArray(), 0, array, 0, array.Length);
		return array;
	}

	public static string StringFromBytes(byte[] bytes)
	{
		char[] array = new char[bytes.Length / 2];
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
		return new string(array);
	}

	public static T[] ReplaceInplace<T>(this T[] array, FuncD<T, T> conv)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = conv(array[i]);
		}
		return array;
	}

	public static int IndexOf<T>(this T[] array, FuncD<T, bool> cond)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (cond(array[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static bool IsTransformChildrenOf(Transform t, Transform root)
	{
		Transform parent = t.parent;
		while (parent != null)
		{
			if (parent == root)
			{
				return true;
			}
			parent = parent.parent;
		}
		return false;
	}

	public static string ToString<T1, T2>(Dictionary<T1, T2> dict)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<T1, T2> item in dict)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append("|");
			}
			stringBuilder.AppendFormat("{0}={1}", item.Key, item.Value);
		}
		return stringBuilder.ToString();
	}

	public static Dictionary<string, string> ToDict(string str)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		char[] separator = new char[1] { '=' };
		string[] array = str.Split('|');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(separator);
			dictionary[array3[0]] = array3[1];
		}
		return dictionary;
	}

	public static List<KeyValuePair<T1, T2>> ToList<T1, T2>(Dictionary<T1, T2> dict)
	{
		List<KeyValuePair<T1, T2>> list = new List<KeyValuePair<T1, T2>>();
		foreach (KeyValuePair<T1, T2> item in dict)
		{
			list.Add(item);
		}
		return list;
	}

	public static Dictionary<T1, T2> ToDict<T1, T2>(List<KeyValuePair<T1, T2>> list)
	{
		Dictionary<T1, T2> dictionary = new Dictionary<T1, T2>();
		foreach (KeyValuePair<T1, T2> item in list)
		{
			dictionary[item.Key] = item.Value;
		}
		return dictionary;
	}

	public static IEnumerator LoadAssetBundleAsync<T>(AssetBundle assetBundle, string name, ActionD<string, T> onSuccess) where T : UnityEngine.Object
	{
		AssetBundleRequest request = assetBundle.LoadAsync(name, typeof(T));
		yield return request;
		onSuccess(name, (T)request.asset);
	}

	public static IEnumerator WWWLoad(string path, float timeout, OnWWWBinarySuccessD onSuccess, OnWWWErrorD onError)
	{
		return WWWLoadImpl(path, timeout, delegate(string _, WWW www)
		{
			onSuccess(_, www.bytes);
		}, onError, null);
	}

	public static IEnumerator WWWLoad(string path, float timeout, OnWWWTextSuccessD onSuccess, OnWWWErrorD onError)
	{
		return WWWLoadImpl(path, timeout, delegate(string _, WWW www)
		{
			onSuccess(_, www.text);
		}, onError, null);
	}

	public static IEnumerator WWWLoadForm(string path, float timeout, OnWWWTextSuccessD onSuccess, OnWWWErrorD onError, Dictionary<string, string> formParams)
	{
		return WWWLoadImpl(path, timeout, delegate(string _, WWW www)
		{
			onSuccess(_, www.text);
		}, onError, formParams);
	}

	public static IEnumerator WWWLoadAssetBundle(string path, float timeout, OnWWWSuccessD onSuccess, OnWWWErrorD onError)
	{
		return WWWLoadAssetBundleImpl(new WWW(path), timeout, onSuccess, onError);
	}

	public static IEnumerator WWWLoadAssetBundleImpl(WWW www, float timeout, OnWWWSuccessD onSuccess, OnWWWErrorD onError)
	{
		float time = Time.realtimeSinceStartup;
		bool onTimeout = false;
		while (!www.isDone && www.error == null)
		{
			if (timeout > 0f && Time.realtimeSinceStartup - time > timeout)
			{
				onTimeout = true;
				break;
			}
			yield return www;
		}
		string errorMsg = null;
		if (onTimeout)
		{
			errorMsg = "TIMEOUT";
		}
		else if (!www.isDone)
		{
			errorMsg = ((www.error == null) ? "UNKNOWN ERROR" : www.error);
		}
		else if (www.error != null)
		{
			errorMsg = www.error;
		}
		else if (!www.url.Contains("file:///") && www.text.Contains("404 Not Found"))
		{
			errorMsg = "404 Not Found";
		}
		else if (www.assetBundle == null)
		{
			errorMsg = "www.assetBundle == null";
		}
		else
		{
			onSuccess(www.url, www);
		}
		if (errorMsg != null)
		{
			onError?.Invoke(www.url, errorMsg);
		}
		www.Dispose();
		yield return null;
	}

	public static IEnumerator WWWLoad(string path, float timeout, OnWWWSuccessD onSuccess, OnWWWErrorD onError)
	{
		return WWWLoadImpl(path, timeout, onSuccess, onError, null);
	}

	public static IEnumerator WWWLoadImpl(string url, float timeout, OnWWWSuccessD onSuccess, OnWWWErrorD onError, Dictionary<string, string> formParams)
	{
		WWW www;
		if (formParams == null || formParams.Count == 0)
		{
			www = new WWW(url);
		}
		else
		{
			WWWForm wWWForm = new WWWForm();
			foreach (KeyValuePair<string, string> item in formParams.AsEnumerable())
			{
				wWWForm.AddField(item.Key, item.Value);
			}
			www = new WWW(url, wWWForm);
		}
		return WWWLoadImpl(www, timeout, onSuccess, onError);
	}

	public static IEnumerator WWWLoadImpl(WWW www, float timeout, OnWWWSuccessD onSuccess, OnWWWErrorD onError)
	{
		float time = Time.realtimeSinceStartup;
		bool onTimeout = false;
		while (!www.isDone && www.error == null)
		{
			if (timeout > 0f && Time.realtimeSinceStartup - time > timeout)
			{
				onTimeout = true;
				break;
			}
			yield return www;
		}
		string errorMsg = null;
		if (onTimeout)
		{
			errorMsg = "TIMEOUT";
		}
		else if (!www.isDone)
		{
			errorMsg = ((www.error == null) ? "UNKNOWN ERROR" : www.error);
		}
		else if (www.error != null)
		{
			errorMsg = www.error;
		}
		else if (!www.url.Contains("file:///") && www.text.Contains("404 Not Found"))
		{
			errorMsg = "404 Not Found";
		}
		else if (www.bytes == null)
		{
			errorMsg = "www.bytes == null";
		}
		else if (www.bytes.Length == 0)
		{
			errorMsg = "www.bytes.Length == 0";
		}
		else
		{
			onSuccess(www.url, www);
		}
		if (errorMsg != null)
		{
			onError?.Invoke(www.url, errorMsg);
		}
		www.Dispose();
		yield return null;
	}

	public static string RemoveLastDir(string path)
	{
		return RemoveLastDir(path, 1);
	}

	public static string RemoveLastDir(string path, int removeCount)
	{
		string[] array = path.Split(new char[4]
		{
			'/',
			'\\',
			Path.AltDirectorySeparatorChar,
			Path.DirectorySeparatorChar
		}, StringSplitOptions.None);
		string text = string.Empty;
		for (int i = 0; i < array.Length - removeCount; i++)
		{
			text = text + array[i] + '/';
		}
		return text;
	}

	public static int Sum<T>(int start, IEnumerable<T> seq, FuncD<T, int> g)
	{
		int num = start;
		foreach (T item in seq)
		{
			num += g(item);
		}
		return num;
	}

	public static V GetInv<K, V>(this Dictionary<K, V> dict, K key)
	{
		V value = default(V);
		Invs.Inv(dict.TryGetValue(key, out value), "dict.TryGetValue", key);
		return value;
	}

	public static V Get<K, V>(this Dictionary<K, V> dict, K key, V ifNo)
	{
		V value = ifNo;
		if (!dict.TryGetValue(key, out value))
		{
			return ifNo;
		}
		return value;
	}

	public static V GetInv<K, V>(this Dictionary<K, V> dict, K key, string message)
	{
		V value = default(V);
		Invs.Inv(dict.TryGetValue(key, out value), "dict.TryGetValue", message, key);
		return value;
	}

	public static void PrecacheAnimations(GameObject go)
	{
		string[] array = new string[17]
		{
			"attack", "idle", "attack_left", "attack_right", "block", "damage", "death", "dodge", "magic_aoe", "magic_attack",
			"magic_baf", "attack_uppercot", "damage_force", "damage_uppercot", "step", "death_force", "death_uppercot"
		};
		foreach (string text in array)
		{
			if (go.animation[text] != null)
			{
				go.animation.Play(text);
			}
		}
		go.animation.Stop();
	}

	public static void Dispose(ref IDisposable d)
	{
		if (d != null)
		{
			d.Dispose();
			d = null;
		}
	}

	public static void Dispose<T>(ref T d) where T : class
	{
		if (d != null)
		{
			if (d is IDisposable disposable)
			{
				disposable.Dispose();
			}
			d = (T)null;
		}
	}

	public static int Random(int min, int maxInclusive, ref int last)
	{
		if (min >= maxInclusive)
		{
			last = min;
			return min;
		}
		int num = last;
		do
		{
			num = UnityEngine.Random.Range(min, maxInclusive + 1);
		}
		while (num == last);
		last = num;
		return num;
	}

	public static void Random<T>(List<T> items, FuncD<T, int> genProb, int count, bool allowDuplicates, RandomActionD action)
	{
		List<int> list = new List<int>(items.Count);
		foreach (T item in items)
		{
			list.Add(genProb(item));
		}
		Random(list, count, allowDuplicates, action);
	}

	public static void Random<T>(T[] items, FuncD<T, int> genProb, int count, bool allowDuplicates, RandomActionD action)
	{
		List<int> list = new List<int>(items.Length);
		foreach (T v in items)
		{
			list.Add(genProb(v));
		}
		Random(list, count, allowDuplicates, action);
	}

	public static void Random(List<int> probability, int count, bool allowDuplicates, RandomActionD action)
	{
		if (count <= 0 || probability.Count == 0)
		{
			return;
		}
		List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>(probability.Count);
		int num = 0;
		foreach (int item in probability)
		{
			list.Add(new KeyValuePair<int, int>(num++, item));
		}
		int num2 = probability.Sum();
		int num3 = 0;
		while (count > 0 && list.Count > 0)
		{
			int num4 = UnityEngine.Random.Range(0, num2 + 1);
			num = 0;
			int num5 = 0;
			foreach (KeyValuePair<int, int> item2 in list)
			{
				num5 += item2.Value;
				if (num5 >= num4)
				{
					action(num3, item2.Key);
					num2 -= item2.Value;
					if (!allowDuplicates)
					{
						list.RemoveAt(num);
					}
					num3++;
					break;
				}
				num++;
			}
			count--;
		}
	}

	public static void ClearBuffers()
	{
		ClearBuffers(Color.gray);
	}

	public static void ClearBuffers(Color color)
	{
		GL.Clear(clearDepth: false, clearColor: true, color);
	}

	public static void ForeachChild(Transform root, ActionD<Transform> action)
	{
		int childCount = root.GetChildCount();
		for (int i = 0; i < childCount; i++)
		{
			action(root.GetChild(i));
		}
	}

	public static T FindSingle<T>(IEnumerable<T> e, T ifNotOne, FuncD<T, bool> cond)
	{
		T result = ifNotOne;
		bool flag = false;
		foreach (T item in e)
		{
			if (cond(item))
			{
				if (flag)
				{
					return ifNotOne;
				}
				result = item;
				flag = true;
			}
		}
		return result;
	}

	public static T First<T>(IEnumerable<T> e)
	{
		using (IEnumerator<T> enumerator = e.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return default(T);
	}

	internal static Vector3 WP(Vector2 point)
	{
		Camera mainCamera = Camera.mainCamera;
		return mainCamera.ScreenPointToRay(point).GetPoint(mainCamera.nearClipPlane * 10f);
	}

	internal static bool Downloaded(WWW www)
	{
		return www.isDone && www.error == null && !www.text.Contains("404 Not Found");
	}

	public static void SetAllRenderersActive(Component component, bool value)
	{
		Renderer[] componentsInChildren = component.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			GetComponent<Renderer>()enabled = value;
		}
	}

	public static void SetAllRenderersActive(GameObject component, bool value)
	{
		Renderer[] componentsInChildren = component.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			GetComponent<Renderer>()enabled = value;
		}
	}

	public static R Select<K, R>(K value, R defaultResult, K k1, R r1, K k2, R r2)
	{
		object obj = k1;
		if (value.Equals(obj))
		{
			return r1;
		}
		object obj2 = k2;
		if (value.Equals(obj2))
		{
			return r2;
		}
		return defaultResult;
	}

	public static R Select<K, R>(K value, R defaultResult, K k1, R r1, K k2, R r2, K k3, R r3)
	{
		object obj = k1;
		if (value.Equals(obj))
		{
			return r1;
		}
		object obj2 = k2;
		if (value.Equals(obj2))
		{
			return r2;
		}
		object obj3 = k3;
		if (value.Equals(obj3))
		{
			return r3;
		}
		return defaultResult;
	}

	public static R Select<K, R>(K value, R defaultResult, K k1, R r1, K k2, R r2, K k3, R r3, K k4, R r4)
	{
		object obj = k1;
		if (value.Equals(obj))
		{
			return r1;
		}
		object obj2 = k2;
		if (value.Equals(obj2))
		{
			return r2;
		}
		object obj3 = k3;
		if (value.Equals(obj3))
		{
			return r3;
		}
		object obj4 = k4;
		if (value.Equals(obj4))
		{
			return r4;
		}
		return defaultResult;
	}

	public static bool RandomBool()
	{
		return UnityEngine.Random.Range(0, 1000) < 500;
	}

	public static string GetParentName(this Transform transform, string ifNoParent)
	{
		Transform parent = transform.parent;
		return (!(parent != null)) ? ifNoParent : parent.name;
	}

	public static KeyValuePair<K, V> KeyValue<K, V>(K key, V value)
	{
		return new KeyValuePair<K, V>(key, value);
	}

	public static void Destroy<T>(ref T mb) where T : UnityEngine.Object
	{
		if (mb != null)
		{
			T obj = mb;
			mb = (T)null;
			UnityEngine.Object.Destroy(obj);
		}
	}

	public static void DestroyGameObject<T>(ref T mb) where T : Component
	{
		if (mb != null)
		{
			GameObject gameObject = mb.gameObject;
			mb = (T)null;
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public static void TryDo(Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			LogError_(ex.Message + "  Stacktrace " + ex.StackTrace.ToString());
		}
	}

	public static void TryDo(Action action, Func<string> getUserData)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			LogError_(ex.Message + ". Data= " + getUserData() + "  Stacktrace " + ex.StackTrace.ToString());
		}
	}

	public static void DoWithComponent<T>(Component component, Action<T> action) where T : Component
	{
		T component2 = component.GetComponent<T>();
		if (component2 != null)
		{
			action(component2);
		}
	}

	public static void DoWithComponent<T>(Component component, bool includeInactive, Action<T> action) where T : Component
	{
		T[] componentsInChildren = component.GetComponentsInChildren<T>(includeInactive);
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			action(componentsInChildren[0]);
		}
	}

	private static void Log_(string text)
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log(Time.frameCount + ":" + Time.time + ": " + text);
		}
	}

	private static void LogError_(string text)
	{
		if (Globals.IsDebugBuild)
		{
			Debug.LogError(Time.frameCount + ": " + text);
		}
	}

	private static void LogWarning_(string text)
	{
		if (Globals.IsDebugBuild)
		{
			Debug.LogWarning(Time.frameCount + ": " + text);
		}
	}

	public static void Log(string text)
	{
		if (LogToDebug)
		{
			Log_(text);
		}
	}

	internal static void LogFrom(object source, string text)
	{
		if (LogToDebug)
		{
			Log_("[[" + source.ToString() + "]]: " + text);
		}
	}

	internal static void Log(params object[] args)
	{
		if (LogToDebug)
		{
			Log_(ParamsToString(args));
		}
	}

	internal static void LogForce(params object[] args)
	{
		if (LogToDebug)
		{
			Log_(ParamsToString(args));
		}
	}

	internal static string ParamsToString(params object[] args)
	{
		StringBuilder stringBuilder = new StringBuilder(124);
		foreach (object obj in args)
		{
			stringBuilder.Append((obj == null) ? "NULL" : obj.ToString());
			stringBuilder.Append(" ");
		}
		return stringBuilder.ToString();
	}

	internal static string ParamsToString<T>(T[] args)
	{
		StringBuilder stringBuilder = new StringBuilder(124);
		for (int i = 0; i < args.Length; i++)
		{
			T val = args[i];
			stringBuilder.Append((val == null) ? "NULL" : val.ToString());
			stringBuilder.Append(" ");
		}
		return stringBuilder.ToString();
	}

	internal static string ParamsToString<T>(T[] args, string pre, string post)
	{
		StringBuilder stringBuilder = new StringBuilder(124);
		for (int i = 0; i < args.Length; i++)
		{
			T val = args[i];
			stringBuilder.Append(pre);
			stringBuilder.Append((val == null) ? "NULL" : val.ToString());
			stringBuilder.Append(post);
		}
		return stringBuilder.ToString();
	}

	internal static void LogFrom(object source, params object[] args)
	{
		if (LogToDebug)
		{
			Log_(MergeArgs(source, args));
		}
	}

	internal static void LogErrorFrom(object source, params object[] args)
	{
		LogError_(MergeArgs(source, args));
	}

	internal static void LogWarningFrom(object source, params object[] args)
	{
		LogWarning_(MergeArgs(source, args));
	}

	private static string MergeArgs(object source, object[] args)
	{
		StringBuilder stringBuilder = new StringBuilder(124);
		stringBuilder.Append("[[" + source.ToString() + "]]: ");
		foreach (object obj in args)
		{
			stringBuilder.Append((obj == null) ? "NULL" : obj.ToString());
			stringBuilder.Append(" ");
		}
		return stringBuilder.ToString();
	}

	internal static GameObject NewWithOffset(UnityEngine.Object prototype, Vector3 position, float x, float y, float z)
	{
		return (GameObject)UnityEngine.Object.Instantiate(prototype, position + new Vector3(x, y, z), Quaternion.identity);
	}

	internal static GameObject NewWithOffset(UnityEngine.Object prototype, Vector3 position, float x, float y, float z, Action<GameObject> action)
	{
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(prototype, position + new Vector3(x, y, z), Quaternion.identity);
		action(gameObject);
		return gameObject;
	}

	internal static Vector3 XZ(Vector3 v, float y)
	{
		return new Vector3(v.x, y, v.z);
	}

	internal static T FindObjectOfType<T>() where T : class
	{
		UnityEngine.Object obj = UnityEngine.Object.FindObjectOfType(typeof(T));
		Invs.Inv(obj != null, "FindObjectOfType failed", typeof(T).Name);
		return obj as T;
	}

	internal static T FindObjectOfTypeNoThrow<T>() where T : class
	{
		UnityEngine.Object obj = UnityEngine.Object.FindObjectOfType(typeof(T));
		return obj as T;
	}

	internal static void FindIfNull(ref GameObject go, string name)
	{
		if (go == null)
		{
			go = GameObject.Find(name);
			Invs.Inv(go != null, "FindIfNotNull failed", name);
		}
	}

	internal static T Instaniate<T>(UnityEngine.Object prototype) where T : Component
	{
		Invs.Inv(prototype != null, "Instaniate failed : prototype = null.");
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(prototype);
		Component componentInChildren = gameObject.GetComponentInChildren(typeof(T));
		Invs.Inv(componentInChildren != null, "Instaniate failed. Wrong prototype type", prototype.GetType().Name, "needs", typeof(T).Name);
		return (T)componentInChildren;
	}

	internal static bool SetValue(Component component, string fieldName, object value)
	{
		FieldInfo field = component.GetType().GetField(fieldName);
		if (field == null)
		{
			return false;
		}
		field.SetValue(component, value);
		return true;
	}

	internal static bool SetValue(Component component, string fieldName, object value, ref FieldInfo fieldInfoCache)
	{
		if (fieldInfoCache == null)
		{
			fieldInfoCache = component.GetType().GetField(fieldName);
			if (fieldInfoCache == null)
			{
				return false;
			}
		}
		fieldInfoCache.SetValue(component, value);
		return true;
	}

	internal static object GetValue(Component component, string name)
	{
		FieldInfo field = component.GetType().GetField(name);
		return field.GetValue(component);
	}

	internal static T GetValueObject<T>(Component component, string fieldName, ref FieldInfo fieldInfoCache) where T : class
	{
		if (fieldInfoCache == null)
		{
			fieldInfoCache = component.GetType().GetField(fieldName);
			Invs.Inv(fieldInfoCache != null, "Can't find field. Name=", component.name, " field name=", fieldName);
		}
		object value = fieldInfoCache.GetValue(component);
		if (value == null)
		{
			return (T)null;
		}
		T val = value as T;
		Invs.Inv(val != null, "Can't find field. Name=", component.name, " field name=", fieldName);
		return val;
	}

	internal static T InvokeMethod<T>(Component component, string methodName, ref MethodInfo methodInfoCache, params object[] args) where T : class
	{
		if (methodInfoCache == null)
		{
			methodInfoCache = component.GetType().GetMethod(methodName);
			Invs.Inv(methodInfoCache != null, "Can't find method. Name=", component.name, " method name=", methodName);
		}
		object obj = methodInfoCache.Invoke(component, args);
		if (obj == null)
		{
			return (T)null;
		}
		T val = obj as T;
		Invs.Inv(val != null, "Can't find method. Name=", component.name, " method name=", methodName);
		return val;
	}

	internal static void InvokeMethod(Component component, string methodName, params object[] args)
	{
		MethodInfo method = component.GetType().GetMethod(methodName);
		Invs.Inv(method != null, "Can't find method. Name=", component.name, " method name=", methodName);
		method.Invoke(component, args);
	}

	internal static IEnumerable<T> Filter<T>(IEnumerable<T> list, FuncD<T, bool> includePredicate)
	{
		foreach (T c in list)
		{
			if (includePredicate(c))
			{
				yield return c;
			}
		}
	}

	internal static T[] MakeArray<T>(Predicate<T> includeInArray, T[] args) where T : class
	{
		T[] array = new T[args.Length];
		int num = 0;
		foreach (T val in args)
		{
			T val2 = (T)val;
			if (val2 != null && includeInArray(val2))
			{
				array[num++] = val2;
			}
		}
		if (num != args.Length)
		{
			Array.Resize(ref array, num);
		}
		return array;
	}

	internal static T[] MakeArray<T>(Predicate<T> includeInArray, params object[] args) where T : class
	{
		return MakeArray(includeInArray, args);
	}

	internal static Vector3 ChangeY(Vector3 v, float y)
	{
		return new Vector3(v.x, y, v.z);
	}

	internal static Vector3 Midpoint(Vector3 a, Vector3 b)
	{
		return new Vector3((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f, (a.z + b.z) * 0.5f);
	}

	internal static Component GetComponentInChildren(GameObject comp, string name)
	{
		Transform[] componentsInChildren = comp.GetComponentsInChildren<Transform>(includeInactive: true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			Component component = transform.GetComponent(name);
			if (component != null)
			{
				return component;
			}
		}
		return null;
	}

	internal static Transform FindChildByName(this Transform target, string childName)
	{
		return target.FindChildByName(childName, includeInactive: false);
	}

	internal static Transform FindOneOfChildByName(this Transform target, bool includeInactive, params string[] childName)
	{
		if (target == null)
		{
			return null;
		}
		if (childName.IndexOf(target.name) >= 0)
		{
			return target.transform;
		}
		Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>(includeInactive);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (childName.IndexOf(transform.name) >= 0)
			{
				return transform;
			}
		}
		return null;
	}

	internal static Transform FindChildByName(this Transform target, string childName, bool includeInactive)
	{
		if (target == null)
		{
			return null;
		}
		if (target.name == childName)
		{
			return target.transform;
		}
		Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>(includeInactive);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == childName)
			{
				return transform;
			}
		}
		return null;
	}

	public static int IndexOf<T>(this T[] array, T value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			ref T reference = ref array[i];
			object obj = value;
			if (reference.Equals(obj))
			{
				return i;
			}
		}
		return -1;
	}

	public static T[] Clone<T>(this T[] array, Func<T, T> transform)
	{
		int num = array.Length;
		T[] array2 = new T[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = transform(array[i]);
		}
		return array2;
	}

	public static T1[] ToArray<T, T1>(this T[] array, Func<T, T1> conv)
	{
		int num = array.Length;
		T1[] array2 = new T1[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = conv(array[i]);
		}
		return array2;
	}

	public static T Get<T>(this T[] array, FuncD<T, bool> cond, T ifNo)
	{
		foreach (T val in array)
		{
			if (cond(val))
			{
				return val;
			}
		}
		return ifNo;
	}

	public static string ConcatAsStrings(this object[] args)
	{
		int sl = 0;
		string[] array = args.ToArray(delegate(object _)
		{
			if (_ != null)
			{
				string text = _.ToString();
				sl += text.Length;
				return text;
			}
			return string.Empty;
		});
		if (sl == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(sl + args.Length);
		stringBuilder.Append(array[0]);
		for (int num = 1; num < array.Length; num++)
		{
			stringBuilder.Append(' ');
			stringBuilder.Append(array[num]);
		}
		return stringBuilder.ToString();
	}

	internal static void HandleError(Exception ex_, params string[] message)
	{
		LogForce("ERROR", message.ConcatAsStrings() + Environment.NewLine + ex_.MessageAndStacktraceWithInners(string.Empty, Environment.NewLine, string.Empty));
		throw ex_;
	}

	internal static void HandleError(MonoBehaviour mb, string message, Exception ex)
	{
		HandleError(ex, "[", mb.name, "]", message);
	}

	public static string MessageAndStacktraceWithInners(this Exception e, string prefix, string separator, string suffix)
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		stringBuilder.Append(prefix);
		Exception ex = e;
		string newLine = Environment.NewLine;
		while (ex != null)
		{
			stringBuilder.Append("EXCEPTION TYPE " + e.GetType().FullName);
			stringBuilder.Append(newLine);
			stringBuilder.Append("MESSAGE: " + ex.Message);
			stringBuilder.Append(newLine);
			stringBuilder.Append("  STACKTRACE: " + ex.StackTrace);
			stringBuilder.Append(newLine);
			ex = ex.InnerException;
			if (ex != null)
			{
				stringBuilder.Append(separator);
			}
		}
		stringBuilder.Append(suffix);
		return stringBuilder.ToString();
	}

	public static T DestroyComponentThenAddNew<T>(GameObject gameObject) where T : Component
	{
		T component = gameObject.GetComponent<T>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		return gameObject.AddComponent<T>();
	}

	internal static void Destroy(Component component)
	{
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	internal static int GetRandom(int min, int max, int current)
	{
		int num;
		do
		{
			num = UnityEngine.Random.Range(min, max);
		}
		while (current == num);
		return num;
	}

	internal static int GetNRand(int max, int prob)
	{
		int num = 0;
		for (int i = 0; i < max; i++)
		{
			if (UnityEngine.Random.Range(0, 100) < prob)
			{
				num++;
			}
		}
		return num;
	}

	internal static void DoNothing(object obj)
	{
	}

	private static int GetObjectSize(UnityEngine.Object obj)
	{
		return 0;
	}

	private static string GetResourcesCountImpl(bool withNames, bool accumSize, Type type, UnityEngine.Object[] objs)
	{
		int num = 0;
		string text = "{0} {1}".Fmt(type.Name, objs.Length);
		if (!withNames)
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(50 * objs.Length);
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		foreach (UnityEngine.Object obj in objs)
		{
			int num2 = ((!accumSize) ? (-1) : GetObjectSize(obj));
			if (accumSize)
			{
				num += num2;
			}
			Texture2D texture2D = obj as Texture2D;
			string empty = string.Empty;
			empty = ((!(texture2D != null)) ? "   {0} {1}".Fmt(obj.name, num2) : "   {0} {1}*{2} {3} {4}".Fmt(texture2D.name, texture2D.width, texture2D.height, texture2D.format, num2));
			if (!withNames || !accumSize)
			{
				stringBuilder.AppendLine(empty);
			}
			else
			{
				list.Add(new Tuple<int, string>(num2, empty));
			}
		}
		if (!withNames || !accumSize)
		{
			stringBuilder.Insert(0, text + " " + num + " \n");
		}
		else
		{
			stringBuilder.Insert(0, text + " " + num + " \n");
			list.Sort((Tuple<int, string> x, Tuple<int, string> y) => x.Item1.CompareTo(y.Item1));
			foreach (Tuple<int, string> item in list)
			{
				stringBuilder.AppendLine(item.Item2);
			}
		}
		return stringBuilder.ToString();
	}

	private static string GetSceneResourcesCount<T>(bool withNames, bool accumSize) where T : UnityEngine.Object
	{
		return GetResourcesCountImpl(withNames, accumSize, typeof(T), UnityEngine.Object.FindSceneObjectsOfType(typeof(T)));
	}

	private static string GetResourcesCount<T>(bool withNames, bool accumSize) where T : UnityEngine.Object
	{
		return GetResourcesCountImpl(withNames, accumSize, typeof(T), UnityEngine.Object.FindObjectsOfTypeAll(typeof(T)));
	}

	internal static void PrintResourcesStat()
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		stringBuilder.AppendLine(GetSceneResourcesCount<AssetBundle>(withNames: true, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<GameObject>(withNames: true, accumSize: true));
		stringBuilder.AppendLine(GetResourcesCount<GameObject>(withNames: false, accumSize: false));
		stringBuilder.AppendLine(GetSceneResourcesCount<Animation>(withNames: true, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<AudioClip>(withNames: true, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<AudioSource>(withNames: false, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<SkinnedMeshRenderer>(withNames: true, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<MeshRenderer>(withNames: false, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<MeshFilter>(withNames: false, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<ParticleSystem>(withNames: false, accumSize: false));
		stringBuilder.AppendLine(GetSceneResourcesCount<Texture>(withNames: false, accumSize: false));
		stringBuilder.AppendLine(GetResourcesCount<Texture>(withNames: false, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<Mesh>(withNames: false, accumSize: true));
		stringBuilder.AppendLine(GetSceneResourcesCount<Material>(withNames: true, accumSize: true));
		Log(stringBuilder.ToString());
	}

	internal static void ShowChanges()
	{
		List<PushData> list = new List<PushData>();
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
		foreach (UnityEngine.Object obj in array)
		{
			list.Add(new PushData
			{
				Id = obj.GetInstanceID(),
				Name = obj.name
			});
		}
		foreach (PushData pushDatum in _pushData)
		{
			if (!list.Contains(pushDatum))
			{
				Log("REMOVEed", pushDatum.Id, pushDatum.Name);
			}
		}
		if (_pushData.Count > 0)
		{
			foreach (PushData item in list)
			{
				if (!_pushData.Contains(item))
				{
					Log("ADDed", item.Id, item.Name);
				}
			}
		}
		_pushData = list;
	}

	internal static int PrefsGetInt(string key)
	{
		if (PlayerPrefs.HasKey(key))
		{
			return PlayerPrefs.GetInt(key);
		}
		return 0;
	}

	internal static void PrefsAddInt(string key, int count)
	{
		PlayerPrefs.SetInt(key, PrefsGetInt(key) + count);
	}

	internal static string PutArg0(string text, string arg0)
	{
		if (text == null)
		{
			return string.Empty;
		}
		if (text.Contains("{0}"))
		{
			return text.Fmt(arg0);
		}
		return text;
	}
}
