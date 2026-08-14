using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GregoryAdam.Base.Security.Hashing;
using UnityEngine;

public static class Util
{
	private const char WideSpace = '\u3000';

	private static readonly char[] Separators = new char[5] { ' ', '\r', '\n', '_', '\u3000' };

	public static List<int> Utf8BytesToOrds(IList<byte> bytes)
	{
		List<int> list = new List<int>();
		int i = 0;
		for (int count = bytes.Count; i < count; i++)
		{
			int num = bytes[i];
			if (num >= 128)
			{
				int num2 = 0;
				if (num < 224)
				{
					num2 = 2;
				}
				else if (num < 240)
				{
					num2 = 3;
				}
				else if (num < 248)
				{
					num2 = 4;
				}
				int num3 = num - 192 - ((num2 > 2) ? 32 : 0) - ((num2 > 3) ? 16 : 0);
				for (int j = 0; j < num2 - 1; j++)
				{
					i++;
					int num4 = bytes[i] - 128;
					num3 = 64 * num3 + num4;
				}
				num = num3;
			}
			list.Add(num);
		}
		return list;
	}

	public static bool Dice0(this int i)
	{
		(i > 0).Assert("i is {0}, must be > 0".Fmt(i));
		return UnityEngine.Random.Range(0, i) == 0;
	}

	public static Vector3 Floor2D(this Vector3 v3)
	{
		return new Vector3(Mathf.Floor(v3.x), Mathf.Floor(v3.y), v3.z);
	}

	public static byte[] ReadFully(Stream stream, int initialLength)
	{
		if (initialLength < 1)
		{
			initialLength = 32768;
		}
		byte[] array = new byte[initialLength];
		int num = 0;
		int num2;
		while ((num2 = stream.Read(array, num, array.Length - num)) > 0)
		{
			num += num2;
			if (num == array.Length)
			{
				int num3 = stream.ReadByte();
				if (num3 == -1)
				{
					return array;
				}
				byte[] array2 = new byte[array.Length * 2];
				Array.Copy(array, array2, array.Length);
				array2[num] = (byte)num3;
				array = array2;
				num++;
			}
		}
		byte[] array3 = new byte[num];
		Array.Copy(array, array3, num);
		return array3;
	}

	public static int IndexOfOccurence(string s, string match, int occurence)
	{
		int i = 1;
		int num = -1;
		for (; i <= occurence; i++)
		{
			if ((num = s.IndexOf(match, num + 1)) == -1)
			{
				break;
			}
			if (i == occurence)
			{
				return num;
			}
		}
		return -1;
	}

	public static byte[] ReadFully(Stream stream)
	{
		return ReadFully(stream, -1);
	}

	public static string MD5(this string input)
	{
		MD5 mD = new MD5();
		byte[] array = mD.Hash(input);
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static T Resource<T>(string path) where T : UnityEngine.Object
	{
		UnityEngine.Object obj = Resources.Load(path);
		if (obj == null && Globals.IsDebugBuild)
		{
			Debug.LogError("! can not load the asset: " + path);
		}
		return obj as T;
	}

	public static T Resource<T>(string path, Type systemTypeInstance) where T : UnityEngine.Object
	{
		UnityEngine.Object obj = Resources.Load(path, systemTypeInstance);
		if (obj == null && Globals.IsDebugBuild)
		{
			Debug.LogError("! can not load the asset: " + path);
		}
		return obj as T;
	}

	public static T AssertNotNull<T>(this T x) where T : class
	{
		return x.AssertNotNull("Fuckup!");
	}

	public static T AssertNotNull<T>(this T x, string message) where T : class
	{
		if (x == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogError(message);
			}
			Debug.Break();
		}
		return x;
	}

	public static void Assert(this bool p)
	{
		p.Assert("Fuckup!");
	}

	public static void Assert(this bool p, string message)
	{
		if (!p)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogError(message);
			}
			Debug.Break();
		}
	}

	public static T Trace<T>(this T o, string bt)
	{
		return o;
	}

	public static void SetLayerRecursively(this Transform o, Transform layerHolder)
	{
		o.gameObject.layer = layerHolder.gameObject.layer;
		if (o.childCount <= 0)
		{
			return;
		}
		foreach (Transform item in o)
		{
			item.SetLayerRecursively(layerHolder);
		}
	}

	public static void SetLayerRecursively(this Transform o, int layer)
	{
		o.gameObject.layer = layer;
		if (o.childCount <= 0)
		{
			return;
		}
		foreach (Transform item in o)
		{
			item.SetLayerRecursively(layer);
		}
	}

	private static string PrefixIdeograms(this string input, char prefix = '_')
	{
		StringBuilder stringBuilder = new StringBuilder(40);
		foreach (char c in input)
		{
			if (c.IsIdeograph())
			{
				stringBuilder.Append(prefix);
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private static bool IsIdeograph(this char ch)
	{
		return ch >= '一' && ch <= '鿌';
	}

	public static string ForceTextIntoMultipleLines(this string inputText, int maxLineLength)
	{
		if (string.IsNullOrEmpty(inputText))
		{
			return string.Empty;
		}
		switch (UnityApi.GetLanguage())
		{
		case "zh":
		case "cn":
		case "jp":
			inputText = inputText.PrefixIdeograms();
			break;
		}
		string[] array = inputText.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1)
		{
			return array[0];
		}
		StringBuilder stringBuilder = new StringBuilder(array[0]);
		int num = array[0].Length;
		bool flag = false;
		for (int i = 1; i < array.Length; i++)
		{
			string text = array[i];
			switch (text[0])
			{
			case '$':
				text = Globals.CharIconGold + ((text.Length <= 1) ? string.Empty : text.Substring(1));
				break;
			case '@':
				text = Globals.CharIconDiamonds + ((text.Length <= 1) ? string.Empty : text.Substring(1));
				break;
			}
			if (text == "^^")
			{
				stringBuilder.Append("\n");
				flag = true;
				num = 0;
				continue;
			}
			if (text == "^^^")
			{
				stringBuilder.Append('\n');
				stringBuilder.Append('\n');
				flag = true;
				num = 0;
				continue;
			}
			if (text.Length + num > maxLineLength)
			{
				stringBuilder.Append('\n');
				stringBuilder.Append(text);
				num = text.Length;
				continue;
			}
			bool flag2 = text[0].IsIdeograph();
			if (!flag && !flag2)
			{
				stringBuilder.Append(' ');
				num++;
			}
			flag = false;
			stringBuilder.Append(text);
			num += text.Length;
		}
		return stringBuilder.ToString();
	}

	public static string Upcase(this string inputText)
	{
		if (string.IsNullOrEmpty(inputText))
		{
			return inputText;
		}
		StringBuilder stringBuilder = new StringBuilder(inputText.Length);
		foreach (char c in inputText)
		{
			stringBuilder.Append(char.ToUpperInvariant(c));
		}
		return stringBuilder.ToString();
	}

	public static string CapitalizeEx(this string inputText, int maxUntouchlength, string[] exceptions)
	{
		if (string.IsNullOrEmpty(inputText))
		{
			return inputText;
		}
		string[] array = inputText.Split(new char[3] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length < 1)
		{
			return inputText;
		}
		IEnumerable<string> source = array.Select((string word) => (word.Length > maxUntouchlength && !Array.Exists(exceptions, (string s) => s.Equals(word, StringComparison.OrdinalIgnoreCase))) ? word.CapitalizeFirstLetter() : word);
		return string.Join(" ", source.ToArray());
	}

	public static string CapitalizeFirstLetter(this string inputText)
	{
		char[] array = inputText.ToCharArray();
		if (array.Length < 1)
		{
			return inputText;
		}
		array[0] = char.ToUpperInvariant(array[0]);
		return new string(array);
	}

	public static void ChangeLocalizedTexture(this Transform entity, string assetPath, string fromLoc, string toLoc)
	{
		entity.ChangeLocalizedOneTexture(assetPath, fromLoc, toLoc);
		foreach (Transform item in entity)
		{
			item.ChangeLocalizedOneTexture(assetPath, fromLoc, toLoc);
		}
	}

	public static void ChangeLocalizedOneTexture(this Component entity, string assetPath, string fromLoc, string toLoc)
	{
		string name = entity.GetComponent<Renderer>().material.mainTexture.name;
		if (!name.Contains("_" + fromLoc))
		{
			return;
		}
		string text = name.Replace($"_{fromLoc}", $"_{toLoc}");
		string text2 = assetPath + "/fragments/" + text;
		Texture2D texture2D = Resource<Texture2D>(text2, typeof(Texture2D));
		if (texture2D == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogError("there id no Texture2D @ " + text2);
			}
		}
		else
		{
			entity.GetComponent<Renderer>().material.mainTexture = texture2D;
		}
	}

	public static FontManager.ColorE DecodeColor(this ServerData.Item item)
	{
		if (item.Color.Contains("gray"))
		{
			return FontManager.ColorE.ItemGray;
		}
		if (item.Color.Contains("green"))
		{
			return FontManager.ColorE.ItemGreen;
		}
		if (item.Color.Contains("blue"))
		{
			return FontManager.ColorE.ItemBlue;
		}
		if (item.Color.Contains("purple"))
		{
			return FontManager.ColorE.ItemViolet;
		}
		if (item.Color.Contains("red"))
		{
			return FontManager.ColorE.ItemRed;
		}
		if (item.Color.Contains("gold"))
		{
			return FontManager.ColorE.ItemGold;
		}
		return FontManager.ColorE.ItemGray;
	}

	public static SpriteGui GetSpriteGui(this Transform me)
	{
		Transform root = me.root;
		GuiRoot component = root.GetComponent<GuiRoot>();
		if (component != null && GuiRoot.CurrentInstantiationParent != null)
		{
			component.transform.parent = GuiRoot.CurrentInstantiationParent;
			GuiRoot.CurrentInstantiationParent = null;
			root = me.root;
		}
		SpriteGui spriteGui = ((!root.name.Contains("main_menu")) ? root.GetComponent<SpriteGui>() : root.GetComponentInChildren<SpriteGui>());
		if (spriteGui == null && Globals.IsDebugBuild)
		{
			Debug.Log("there is no SpriteGui@root: " + root.name);
		}
		return spriteGui;
	}
}
