using System;
using System.Collections;
using Boo.Lang.Runtime;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class funcs : MonoBehaviour
{
	public static void PushAllChildInArray(Transform target, UnityScript.Lang.Array arr)
	{
		if (!target)
		{
			return;
		}
		IEnumerator enumerator = UnityRuntimeServices.GetEnumerator(target);
		while (enumerator.MoveNext())
		{
			object obj = enumerator.Current;
			if (!(obj is Transform))
			{
				obj = RuntimeServices.Coerce(obj, typeof(Transform));
			}
			Transform transform = (Transform)obj;
			arr.push(transform);
			UnityRuntimeServices.Update(ref enumerator, transform);
			PushAllChildInArray(transform.transform, arr);
			UnityRuntimeServices.Update(ref enumerator, transform);
		}
	}

	public static Transform FindChildByName(Transform target, string childname)
	{
		object result;
		Transform target2;
		if ((bool)target)
		{
			if (target.name == childname)
			{
				result = target.transform;
				goto IL_0085;
			}
			IEnumerator enumerator = UnityRuntimeServices.GetEnumerator(target);
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				if (!(obj is Transform))
				{
					obj = RuntimeServices.Coerce(obj, typeof(Transform));
				}
				target2 = (Transform)obj;
				target2 = FindChildByName(target2, childname);
				UnityRuntimeServices.Update(ref enumerator, target2);
				if (!target2)
				{
					continue;
				}
				goto IL_0073;
			}
		}
		result = null;
		goto IL_0085;
		IL_0085:
		return (Transform)result;
		IL_0073:
		result = target2;
		goto IL_0085;
	}

	public static void PrintTemp(string text)
	{
	}

	public static string StrClean(string str)
	{
		str = str.ToLower();
		str = str.Replace(" ", string.Empty);
		str = str.Replace("\n", string.Empty);
		str = str.Replace("\r", string.Empty);
		str = str.Replace("\t", string.Empty);
		return str;
	}

	public static string Part(string msg, int id, string defaultvalue)
	{
		string text = StrSeparate(msg, id, ":");
		return string.IsNullOrEmpty(text) ? defaultvalue : text;
	}

	public static string StrSeparate(string text, int id, string separator)
	{
		int num = 0;
		int num2 = 0;
		object result;
		while (true)
		{
			if (num2 < UnityScript.Lang.Extensions.get_length(text))
			{
				if (text.Substring(num2, 1) == separator)
				{
					num++;
				}
				if (num == id)
				{
					if (id != 0)
					{
						num2++;
						if (UnityScript.Lang.Extensions.get_length(text) > num2 + 1 && text.Substring(num2, 1) == separator)
						{
							result = "0";
							break;
						}
					}
					int i;
					for (i = num2 + 1; i < UnityScript.Lang.Extensions.get_length(text) && !(text.Substring(i, 1) == separator); i++)
					{
					}
					if (num2 + (i - num2) - 1 < UnityScript.Lang.Extensions.get_length(text))
					{
						result = text.Substring(num2, i - num2);
						break;
					}
				}
				num2++;
				continue;
			}
			result = null;
			break;
		}
		return (string)result;
	}

	public static float RealDeltaTime()
	{
		return Time.deltaTime / Time.timeScale;
	}

	public static Vector3 AddY(Vector3 t, float y)
	{
		return new Vector3(t.x, t.y + y, t.z);
	}

	public static bool IsScenariosPlaying()
	{
		return false;
	}

	public virtual void Main()
	{
	}
}