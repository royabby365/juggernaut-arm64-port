using System;
using System.Collections;
using Boo.Lang.Runtime;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class assets : MonoBehaviour
{
	public Texture transparent_texture;

	public Texture black_ac_texture;

	public Material blood_material;

	[NonSerialized]
	public static Texture transparent;

	[NonSerialized]
	public static Texture black_ac;

	[NonSerialized]
	public static Material berserk_blood;

	[NonSerialized]
	public static object downloader;

	[NonSerialized]
	public static object loader;

	[NonSerialized]
	public static object effects;

	[NonSerialized]
	public static string scenarios;

	[NonSerialized]
	public static SortedList scenarios_cache;

	[NonSerialized]
	public static string colors;

	[NonSerialized]
	public static object sounds;

	[NonSerialized]
	public static object scene_parameters;

	[NonSerialized]
	public static GameObject player;

	[NonSerialized]
	public static Transform player_bones_transform_cache;

	[NonSerialized]
	public static Transform player_transform_cache;

	[NonSerialized]
	public static object player_character_cache;

	[NonSerialized]
	public static GameObject enemy;

	[NonSerialized]
	public static Transform enemy_bones_transform_cache;

	[NonSerialized]
	public static Transform enemy_transform_cache;

	[NonSerialized]
	public static object enemy_character_cache;

	public virtual void Start()
	{
		transparent = transparent_texture;
		black_ac = black_ac_texture;
		berserk_blood = blood_material;
		downloader = gameObject.AddComponent(System.Type.GetType("assets_downloader, Assembly-CSharp"));
		loader = gameObject.AddComponent(System.Type.GetType("assets_loader, Assembly-CSharp"));
	}

	public static Color ColorById(int color_id)
	{
		Color color = default(Color);
		if (!string.IsNullOrEmpty(colors))
		{
			string text = color_id + ":";
			string value = null;
			int num = -1;
			for (int i = 0; i < UnityScript.Lang.Extensions.get_length(colors) - 1; i++)
			{
				if (num < 0)
				{
					if (i > UnityScript.Lang.Extensions.get_length(colors) - UnityScript.Lang.Extensions.get_length(text))
					{
						break;
					}
					if (colors.Substring(i, UnityScript.Lang.Extensions.get_length(text)) == text)
					{
						num = i + UnityScript.Lang.Extensions.get_length(text);
						i += UnityScript.Lang.Extensions.get_length(text);
					}
				}
				else if (colors.Substring(i, 1) == ";")
				{
					value = colors.Substring(num, i - num);
					break;
				}
			}
			if (string.IsNullOrEmpty(value))
			{
			}
		}
		if (RuntimeServices.EqualityOperator(color, null))
		{
			return new Color(0.5f, 0.5f, 0.5f, 1f);
		}
		return color;
	}

	public static GameObject GetCharOfType(string chartype)
	{
		return (chartype == "player") ? player : ((!(chartype == "enemy")) ? null : enemy);
	}

	public static void SetCharOfType(GameObject character, string chartype)
	{
		if ((bool)character)
		{
			object obj = null;
			if (chartype == "player")
			{
				obj = player;
				player = character;
				player_bones_transform_cache = (player.GetComponent<character_parameters>() as character_parameters).Bones.transform;
				player_transform_cache = player.transform;
				player_character_cache = player.GetComponent<character_parameters>();
			}
			else
			{
				obj = character;
				enemy = character;
				enemy_bones_transform_cache = (enemy.GetComponent<character_parameters>() as character_parameters).Bones.transform;
				enemy_transform_cache = enemy.transform;
				enemy_character_cache = enemy.GetComponent<character_parameters>();
			}
		}
	}

	public static void CreateScenariosCache(string file, SortedList cache, object manekenAction)
	{
		int num = 0;
		while (true)
		{
			num = file.IndexOf("$", num);
			if (num == -1)
			{
				break;
			}
			num++;
			int num2 = file.IndexOf("{", num);
			if (num2 == -1)
			{
				num2 = UnityScript.Lang.Extensions.get_length(file);
			}
			string text = file.Substring(num, num2 - num);
			if (!string.IsNullOrEmpty(text))
			{
				string key = text.Trim();
				if (!RuntimeServices.ToBool(cache[key]))
				{
					cache.Add(key, GetScenarioText(file, text));
				}
			}
		}
		if (RuntimeServices.EqualityOperator(manekenAction, null))
		{
			return;
		}
		num = 0;
		while (true)
		{
			num = file.IndexOf("#maneken(", num);
			if (num == -1)
			{
				break;
			}
			int num3 = file.IndexOf(",", num + 1);
			if (num3 == -1)
			{
				break;
			}
			int num4 = file.IndexOf(");", num3 + 1);
			if (num4 == -1)
			{
				break;
			}
			string text2 = file.Substring(num3 + 1, num4 - num3 - 1);
			int num5 = int.Parse(file.Substring(num + 9, num3 - num - 9));
			num = num4 + 1;
		}
	}

	public static string GetScenarioText(string scenarios_file, string name)
	{
		object result;
		if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(scenarios_file) && UnityScript.Lang.Extensions.get_length(scenarios_file) >= 1)
		{
			string text = "$" + name;
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < UnityScript.Lang.Extensions.get_length(scenarios_file); i++)
			{
				if (num < 0)
				{
					i = scenarios_file.IndexOf(text, i);
					if (i == -1)
					{
						break;
					}
					num2 = scenarios_file.IndexOf("{", i);
					if (num2 == -1)
					{
						break;
					}
					if (funcs.StrClean(scenarios_file.Substring(i, num2 - i - 1)) == funcs.StrClean(text))
					{
						num = num2 + 1;
						num2 = -1;
						i = num;
					}
					else
					{
						num2 = -1;
					}
				}
				else if (scenarios_file.Substring(i, 1) == "}")
				{
					num2 = i - 1;
					break;
				}
			}
			if (num >= 0 && num2 >= 0)
			{
				result = scenarios_file.Substring(num, num2 - num);
				goto IL_0107;
			}
		}
		result = null;
		goto IL_0107;
		IL_0107:
		return (string)result;
	}

	public virtual void Main()
	{
	}
}
