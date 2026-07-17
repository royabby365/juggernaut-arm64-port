using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Yarx.Collections;

public static class Extensions
{
	private static readonly Tuple<string, int> _emptyDescription = Tuple.Create(string.Empty, -1);

	private static readonly Tuple<string, bool, string, bool> EmptyDigits = Tuple.Create(string.Empty, item2: false, string.Empty, item4: false);

	public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
	{
		foreach (T item in ie)
		{
			action(item);
		}
	}

	public static void SetActiveRecursivelyMk1(this GameObject @this, bool setActive)
	{
		@this.SetActiveRecursively(setActive);
		if (setActive)
		{
			RendererHandler[] componentsInChildren = @this.GetComponentsInChildren<RendererHandler>();
			RendererHandler[] array = componentsInChildren;
			foreach (RendererHandler rendererHandler in array)
			{
				rendererHandler.ActiveShow();
			}
		}
	}

	public static void ShowOrHide(this Component @this, bool show)
	{
		if (!(@this == null))
		{
			RendererHandler component = @this.GetComponent<RendererHandler>();
			if (component == null)
			{
				@this.renderer.enabled = show;
			}
			else
			{
				component.ShowOrHideMethod(show);
			}
		}
	}

	public static Texture2D CheckTexture2D(this Texture2D tex)
	{
		if (tex == null)
		{
			return null;
		}
		Color32[] pixels = tex.GetPixels32();
		if (pixels == null || pixels.Length == 0)
		{
			return tex;
		}
		Color32 color = pixels[0];
		Color32[] array = pixels;
		foreach (Color32 color2 in array)
		{
			if ((Color)color != (Color)color2)
			{
				return tex;
			}
		}
		return null;
	}

	public static int DivideBy2DScale(this int x)
	{
		return ((float)x / Camera2D.Scale).RoundToInt();
	}

	public static float DivideBy2DScale(this float x)
	{
		return x / Camera2D.Scale;
	}

	public static Vector3 DivideBy2DScale(this Vector3 v3)
	{
		return v3 / Camera2D.Scale;
	}

	public static Vector2 DivideBy2DScale(this Vector2 v2)
	{
		return v2 / Camera2D.Scale;
	}

	public static int MultiplyBy2DScale(this int x)
	{
		return ((float)x * Camera2D.Scale).RoundToInt();
	}

	public static float MultiplyBy2DScale(this float x)
	{
		return x * Camera2D.Scale;
	}

	public static Vector3 MultiplyBy2DScale(this Vector3 v3)
	{
		return v3 * Camera2D.Scale;
	}

	public static Vector2 MultiplyBy2DScale(this Vector2 v2)
	{
		return v2 * Camera2D.Scale;
	}

	public static int ClampNegative(this int n)
	{
		return (n >= 0) ? n : 0;
	}

	public static int Absi(this int n)
	{
		return Mathf.Abs(n);
	}

	public static bool IsContainsPoint(this Rect r, Vector2 p)
	{
		return p.x >= r.x && p.x <= r.x + r.width && p.y <= r.y && p.y >= r.y - r.height;
	}

	public static bool IsContainsPoint(this Rect r, Vector3 p)
	{
		return p.x >= r.x && p.x <= r.x + r.width && p.y <= r.y && p.y >= r.y - r.height;
	}

	public static Rect ToRect(this Mesh m)
	{
		return new Rect(m.vertices[1].x, m.vertices[1].y, m.vertices[3].x - m.vertices[1].x, m.vertices[1].y - m.vertices[0].y);
	}

	public static string Int2(this int i)
	{
		return $"{i:00}";
	}

	public static string Pretty<T>(this IList<T> xs)
	{
		StringBuilder stringBuilder = new StringBuilder("{" + ((xs.Count <= 0) ? string.Empty : xs[0].ToString()));
		for (int i = 1; i < xs.Count; i++)
		{
			stringBuilder.Append(", ").Append(xs[i].ToString());
		}
		return stringBuilder.Append("}").ToString();
	}

	public static string Pretty(this DateTimeOffset dto)
	{
		return dto.ToString("H:mm:ss.fff");
	}

	public static string Pretty(this IEnumerable<char> chars)
	{
		return new string(chars.ToArray());
	}

	public static TimeSpan Sec(this double sec)
	{
		return TimeSpan.FromSeconds(sec);
	}

	public static TimeSpan Sec(this int sec)
	{
		return TimeSpan.FromSeconds(sec);
	}

	public static bool Eqv(this float a, float b)
	{
		return Mathf.Approximately(a, b);
	}

	public static bool Eqv(this Rect r1, Rect r2)
	{
		return r1.x.Eqv(r2.x) && r1.y.Eqv(r2.y) && r1.width.Eqv(r2.width) && r1.height.Eqv(r2.height);
	}

	public static bool Eqv(this Vector4 a, Vector4 b)
	{
		return a.x.Eqv(b.x) && a.y.Eqv(b.y) && a.z.Eqv(b.z) && a.w.Eqv(b.w);
	}

	public static bool Eqv(this Color c1, Color c2)
	{
		return c1.r.Eqv(c2.r) && c1.g.Eqv(c2.g) && c1.b.Eqv(c2.b) && c1.a.Eqv(c2.a);
	}

	public static bool StructuralEquals<T>(this T[] first, T[] second)
	{
		if (first == second)
		{
			return true;
		}
		if (first == null || second == null)
		{
			return false;
		}
		if (first.Length != second.Length)
		{
			return false;
		}
		IEqualityComparer equalityComparer = EqualityComparer<T>.Default;
		for (int i = 0; i < first.Length; i++)
		{
			if (!equalityComparer.Equals(first[i], second[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static byte[] ToRgba(this Color c)
	{
		return new byte[4]
		{
			(byte)(Mathf.Clamp01(c.r) * 255f).RoundToInt(),
			(byte)(Mathf.Clamp01(c.g) * 255f).RoundToInt(),
			(byte)(Mathf.Clamp01(c.b) * 255f).RoundToInt(),
			(byte)(Mathf.Clamp01(c.a) * 255f).RoundToInt()
		};
	}

	public static Color FromRgba(byte r, byte g, byte b, byte a)
	{
		return new Color((float)(int)r / 255f, (float)(int)g / 255f, (float)(int)b / 255f, (float)(int)a / 255f);
	}

	public static Vector4 ToHsl(this Color c)
	{
		float r = c.r;
		float g = c.g;
		float b = c.b;
		float num = Mathf.Max(r, g, b);
		float num2 = Mathf.Min(r, g, b);
		float x = 0f;
		float y = 0f;
		float num3 = (num + num2) / 2f;
		if (num == num2)
		{
			return new Vector4(x, y, num3, c.a);
		}
		x = ((num == r) ? (1f / 6f * (g - b) / (num - num2)) : ((num != g) ? (1f / 6f * (r - g) / (num - num2) + 2f / 3f) : (1f / 6f * (b - r) / (num - num2) + 1f / 3f)));
		if (x < 0f)
		{
			x += 1f;
		}
		else if (x > 1f)
		{
			x -= 1f;
		}
		y = ((!(num3 <= 0.5f)) ? ((num - num2) / (2f - 2f * num3)) : ((num - num2) / (2f * num3)));
		return new Vector4(360f * x, y, num3, c.a);
	}

	public static Color FromHsl(this Vector4 hsl)
	{
		float num = hsl.x / 360f;
		float y = hsl.y;
		float z = hsl.z;
		float num2 = ((!(z < 0.5f)) ? (z + y - z * y) : (z * (1f + y)));
		float num3 = 2f * z - num2;
		float[] array = new float[3]
		{
			num + 1f / 3f,
			num,
			num - 1f / 3f
		};
		for (int i = 0; i < 3; i++)
		{
			if (array[i] < 0f)
			{
				array[i] += 1f;
			}
			else if (array[i] > 1f)
			{
				array[i] -= 1f;
			}
			if (array[i] < 1f / 6f)
			{
				array[i] = num3 + (num2 - num3) * 6f * array[i];
			}
			else if (array[i] < 0.5f)
			{
				array[i] = num2;
			}
			else if (array[i] < 2f / 3f)
			{
				array[i] = num3 + (num2 - num3) * 6f * (2f / 3f - array[i]);
			}
			else
			{
				array[i] = num3;
			}
		}
		return new Color(array[0], array[1], array[2], hsl.w);
	}

	public static Vector3 RoundToInt(this Vector3 v3)
	{
		return new Vector3(Mathf.RoundToInt(v3.x), Mathf.RoundToInt(v3.y), Mathf.RoundToInt(v3.z));
	}

	public static int RoundToInt(this float x)
	{
		return Mathf.RoundToInt(x);
	}

	public static int RoundToEven(this float x)
	{
		int num = Mathf.RoundToInt(x);
		return (num + 1) & -2;
	}

	public static int CeilToInt(this float x)
	{
		return Mathf.CeilToInt(x);
	}

	public static int FloorToInt(this float x)
	{
		return Mathf.FloorToInt(x);
	}

	public static Mesh GetMesh(this Transform transform)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		return transform.GetComponent<MeshFilter>().mesh;
	}

	public static Mesh GetSharedMesh(this Transform transform)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		return transform.GetComponent<MeshFilter>().sharedMesh;
	}

	public static bool In<T>(this T source, params T[] list)
	{
		return list.Contains(source);
	}

	public static int Mod(this int x)
	{
		return (x >= 0) ? x : (-x);
	}

	public static bool Between<T>(this T actual, T lower, T upper) where T : IComparable<T>
	{
		return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
	}

	public static string Fmt(this string format, params object[] args)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		return string.Format(format, args);
	}

	public static string FmtEx(this string format, params object[] args)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		string text = "[{0}@{1} ah: {2}]".Fmt(Time.frameCount, Time.time.ToString("f2"), UnityApi.GetMonoHeap());
		return "{0} {1}".Fmt(text, format.Fmt(args));
	}

	public static bool NotNullOrEmpty(this string str)
	{
		return !string.IsNullOrEmpty(str);
	}

	public static bool IsNullOrEmpty(this string s)
	{
		return string.IsNullOrEmpty(s);
	}

	public static IList<T> ShuffleInPlace<T>(this IList<T> @this)
	{
		if (@this == null)
		{
			throw new ArgumentNullException("this");
		}
		for (int num = @this.Count - 1; num > 0; num--)
		{
			int index = UnityEngine.Random.Range(0, num + 1);
			T value = @this[num];
			@this[num] = @this[index];
			@this[index] = value;
		}
		return @this;
	}

	public static T[] Shuffle<T>(this IEnumerable<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		T[] array = list.ToArray();
		for (int num = array.Length - 1; num > 0; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num + 1);
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
		}
		return array;
	}

	public static IOrderedEnumerable<T> ShuffleEx<T>(this IEnumerable<T> seq)
	{
		return seq.OrderBy((T _) => Guid.NewGuid());
	}

	public static V GetOrElse<K, V>(this Dictionary<K, V> dict, K k, Func<V> f)
	{
		if (dict == null)
		{
			throw new ArgumentNullException("dict");
		}
		if (f == null)
		{
			throw new ArgumentNullException("f");
		}
		try
		{
			V value;
			return (!dict.TryGetValue(k, out value)) ? f() : value;
		}
		catch (ArgumentNullException)
		{
			return f();
		}
	}

	public static string Atom(this string s)
	{
		return string.Intern(s);
	}

	public static bool IsMatch(this string source, string rx)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (rx == null)
		{
			throw new ArgumentNullException("rx");
		}
		return new Regex(rx).IsMatch(source);
	}

	public static string SearchReplaceRegexp(this string source, string matchPattern, string replaceStr)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (matchPattern == null)
		{
			throw new ArgumentNullException("matchPattern");
		}
		if (replaceStr == null)
		{
			throw new ArgumentNullException("replaceStr");
		}
		return new Regex(matchPattern).Replace(source, replaceStr);
	}

	public static void Eliminate(this UnityEngine.Object o)
	{
		GameObject gameObject = o as GameObject;
		if (gameObject != null)
		{
			gameObject.transform.parent = null;
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		Component component = o as Component;
		if (component != null)
		{
			component.transform.parent = null;
			UnityEngine.Object.Destroy(component.gameObject);
		}
	}

	public static void GoToHell(this Transform transform)
	{
		transform.localPosition = new Vector3(5000f, 5000f, 0f);
	}

	public static bool IsInHell(this Transform t)
	{
		Vector3 localPosition = t.localPosition;
		return Math.Abs(localPosition.x) > 4500f || Mathf.Abs(localPosition.y) > 4500f;
	}

	public static Vector3 ToVector3(this Vector2 v2)
	{
		return new Vector3(v2.x, v2.y);
	}

	public static Vector3 ToVector3(this Vector2 v2, float z)
	{
		return new Vector3(v2.x, v2.y, z);
	}

	public static float Duration(this AnimationCurve curve)
	{
		if (curve.length > 0)
		{
			return curve.keys[curve.length - 1].time;
		}
		return 0f;
	}

	internal static void Push<T>(this List<T> list, T item)
	{
		list.Add(item);
	}

	internal static T Pop<T>(this List<T> list)
	{
		int index = list.Count - 1;
		T result = list[index];
		list.RemoveAt(index);
		return result;
	}

	internal static T Peek<T>(this List<T> list)
	{
		int index = list.Count - 1;
		return list[index];
	}

	public static void SetPutonTrue(this ServerData.Item item)
	{
		item.PutOn = true;
		Messenger.Invoke(Globals.MsgItemPuton, item);
	}

	public static bool IsElixirType(this ServerData.Item item)
	{
		return item.ElixirType != ServerData.Item.ElixirTypeE.None;
	}

	public static bool IsBattleElixir(this ServerData.Item item)
	{
		ServerData.Item.ElixirTypeE elixirType = item.ElixirType;
		return elixirType == ServerData.Item.ElixirTypeE.Critical || elixirType == ServerData.Item.ElixirTypeE.Heal || elixirType == ServerData.Item.ElixirTypeE.Poison;
	}

	public static Tuple<string, int> GetItemDescription(this ServerData.Item item)
	{
		if (item.ElixirType == ServerData.Item.ElixirTypeE.None)
		{
			int num = ((item.Skills != null) ? Math.Min(3, item.Skills.Length) : 0);
			for (int i = 0; i < num; i++)
			{
				if (item.Skills != null)
				{
					ServerData.SkillInfo skillInfo = item.Skills[i];
					if (skillInfo != null && skillInfo.Skill != null)
					{
						return Tuple.Create($"{skillInfo.GetSkillIcon()} {skillInfo.Skill.Title} ", skillInfo.Current);
					}
				}
			}
			return _emptyDescription;
		}
		return _emptyDescription;
	}

	public static Tuple<ServerData.Skill.TypeE, string, int> GetItemBonus(this ServerData.Item item)
	{
		ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
		return (itemSkillInfo != null) ? Tuple.Create(itemSkillInfo.Skill.Type, itemSkillInfo.Skill.Title, itemSkillInfo.Current) : null;
	}

	private static Tuple<string, int> GetItemSkillDigits(this ServerData.Item item)
	{
		ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
		return (itemSkillInfo != null) ? Tuple.Create(itemSkillInfo.GetSkillIcon(), itemSkillInfo.Current) : null;
	}

	public static bool IsItemSet(this ServerData.Item it)
	{
		return it.Slot.SlotId == ServerData.Slot.TypeE.EpicSet || it.Slot.SlotId == ServerData.Slot.TypeE.RareSet || it.Slot.SlotId == ServerData.Slot.TypeE.HalfEpicSet || it.Slot.SlotId == ServerData.Slot.TypeE.AlchemySet;
	}

	public static bool IsEpicShopGood(this ServerData.ShopGood shopGood)
	{
		return shopGood.Item.Set == 3 && shopGood.GetPrice(ServerData.MoneyType.TypeE.Gold) > 0;
	}

	public static bool Free(this Dictionary<ServerData.MoneyType, int> price)
	{
		int num = 0;
		foreach (int value in price.Values)
		{
			num = Mathf.Max(value, num);
		}
		return num == 0;
	}

	public static bool ItemStructuralEqualTo(this ServerData.Item it1, ServerData.Item it2)
	{
		if (it1.IsItemSet() || it2.IsItemSet())
		{
			return it1.Id == it2.Id;
		}
		return it1.Id == it2.Id && it1.GetItemSkillInfo().Current == it2.GetItemSkillInfo().Current;
	}

	public static Tuple<string, int> GetItemMaxDescription(this ServerData.Item item)
	{
		if (item.ElixirType == ServerData.Item.ElixirTypeE.None)
		{
			int num = ((item.Skills != null) ? Math.Min(3, item.Skills.Length) : 0);
			for (int i = 0; i < num; i++)
			{
				if (item.Skills != null)
				{
					ServerData.SkillInfo skillInfo = item.Skills[i];
					if (skillInfo != null && skillInfo.Skill != null)
					{
						return Tuple.Create($"{skillInfo.GetSkillIcon()} {skillInfo.Skill.Title} ", skillInfo.Max);
					}
				}
			}
			return _emptyDescription;
		}
		return _emptyDescription;
	}

	public static string GetSkillIcon(this ServerData.SkillInfo skillInfo)
	{
		return skillInfo.Skill.Type switch
		{
			ServerData.Skill.TypeE.Unknown => string.Empty, 
			ServerData.Skill.TypeE.Strength => Globals.CharIconStrength, 
			ServerData.Skill.TypeE.Vitality => Globals.CharIconVitality, 
			ServerData.Skill.TypeE.Rage => Globals.CharIconRage, 
			ServerData.Skill.TypeE.Magic => Globals.CharIconMagic, 
			ServerData.Skill.TypeE.MagicIce => Globals.CharIconIce, 
			ServerData.Skill.TypeE.MagicFire => Globals.CharIconFire, 
			ServerData.Skill.TypeE.MagicDark => Globals.CharIconDark, 
			ServerData.Skill.TypeE.MagicElectro => Globals.CharIconElectro, 
			ServerData.Skill.TypeE.BonusRage => Globals.CharIconRageBonus, 
			ServerData.Skill.TypeE.BonusMana => Globals.CharIconManaBonus, 
			ServerData.Skill.TypeE.BonusExp => Globals.CharIconExp, 
			ServerData.Skill.TypeE.BonusMoney => Globals.CharIconGoldBonus, 
			ServerData.Skill.TypeE.FullMana => Globals.CharIconFullMana, 
			ServerData.Skill.TypeE.FullRage => Globals.CharIconFullRage, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static ServerData.SkillInfo GetItemSkillInfo(this ServerData.Item item)
	{
		if (item == null)
		{
			return null;
		}
		if (item.ElixirType == ServerData.Item.ElixirTypeE.None)
		{
			string[] array = new string[(item.Skills != null) ? Math.Min(3, item.Skills.Length) : 0];
			for (int i = 0; i < array.Length; i++)
			{
				if (item.Skills != null)
				{
					ServerData.SkillInfo skillInfo = item.Skills[i];
					if (skillInfo != null && skillInfo.Skill != null)
					{
						return skillInfo;
					}
				}
			}
		}
		return null;
	}

	public static Tuple<ServerData.MoneyType.TypeE, int, string> GetItemBuyPrice(this ServerData.ShopGood shopGood)
	{
		ServerData.MoneyType.TypeE typeE = ServerData.MoneyType.TypeE.Gold;
		int num = 0;
		foreach (KeyValuePair<ServerData.MoneyType, int> item in (shopGood.Price.Count <= 0) ? shopGood.Item.SellPrice : shopGood.Price)
		{
			typeE = item.Key.Type;
			num = item.Value;
		}
		if (shopGood.Discount > 0)
		{
			num = CalculateDiscount(num, shopGood.Discount);
		}
		return Tuple.Create(typeE, num, GetValueCharIcon(typeE));
	}

	private static int CalculateDiscount(int amount, int percent)
	{
		int num = Mathf.Min(100, percent);
		return ((1f - (float)num / 100f) * (float)amount).RoundToInt();
	}

	public static Tuple<ServerData.MoneyType.TypeE, int, string> GetPrice(this Dictionary<ServerData.MoneyType, int> prices)
	{
		ServerData.MoneyType.TypeE typeE = ServerData.MoneyType.TypeE.Gold;
		int item = 0;
		foreach (KeyValuePair<ServerData.MoneyType, int> price in prices)
		{
			typeE = price.Key.Type;
			item = price.Value;
		}
		return Tuple.Create(typeE, item, GetValueCharIcon(typeE));
	}

	public static bool IsCountable(this ServerData.Item item)
	{
		return item.ElixirType != ServerData.Item.ElixirTypeE.None;
	}

	public static bool IsMoney(this ServerData.Item item)
	{
		ServerData.Item.ElixirTypeE elixirType = item.ElixirType;
		return elixirType == ServerData.Item.ElixirTypeE.Gold || elixirType == ServerData.Item.ElixirTypeE.Diamond || elixirType == ServerData.Item.ElixirTypeE.Key || elixirType == ServerData.Item.ElixirTypeE.Skull || elixirType == ServerData.Item.ElixirTypeE.Scarab || elixirType == ServerData.Item.ElixirTypeE.Star;
	}

	private static string GetValueCharIcon(ServerData.MoneyType.TypeE currency)
	{
		return currency switch
		{
			ServerData.MoneyType.TypeE.Gold => Globals.CharIconGold, 
			ServerData.MoneyType.TypeE.Diamond => Globals.CharIconDiamonds, 
			ServerData.MoneyType.TypeE.Key => Globals.CharIconKey, 
			_ => string.Empty, 
		};
	}

	public static Tuple<ServerData.MoneyType.TypeE, int, string> GetItemSellPrice(this ServerData.Item item)
	{
		Pair<ServerData.MoneyType, int> sellPrice = SingletonT<ServerData>.I.GetSellPrice(item);
		return Tuple.Create(sellPrice.Key.Type, sellPrice.Value, GetValueCharIcon(sellPrice.Key.Type));
	}

	public static int GetPlayerFundsCount(this ServerData.MoneyType.TypeE type)
	{
		return type switch
		{
			ServerData.MoneyType.TypeE.Gold => SingletonT<ServerData>.I.PlayerParams.MoneyGoldCount, 
			ServerData.MoneyType.TypeE.Diamond => SingletonT<ServerData>.I.PlayerParams.MoneyDiamondCount, 
			ServerData.MoneyType.TypeE.Key => SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount, 
			ServerData.MoneyType.TypeE.Skull => SingletonT<ServerData>.I.PlayerParams.MoneySkullsCount, 
			ServerData.MoneyType.TypeE.Scarab => SingletonT<ServerData>.I.PlayerParams.MoneyScarabCount, 
			ServerData.MoneyType.TypeE.Star => SingletonT<ServerData>.I.PlayerParams.MoneyStarsCount, 
			_ => throw new ArgumentOutOfRangeException("type"), 
		};
	}

	public static ServerData.MoneyType.TypeE GetMoneyTypeFromItem(this ServerData.Item item)
	{
		return item.ElixirType switch
		{
			ServerData.Item.ElixirTypeE.Key => ServerData.MoneyType.TypeE.Key, 
			ServerData.Item.ElixirTypeE.Skull => ServerData.MoneyType.TypeE.Skull, 
			ServerData.Item.ElixirTypeE.Scarab => ServerData.MoneyType.TypeE.Scarab, 
			ServerData.Item.ElixirTypeE.Gold => ServerData.MoneyType.TypeE.Gold, 
			ServerData.Item.ElixirTypeE.Diamond => ServerData.MoneyType.TypeE.Diamond, 
			ServerData.Item.ElixirTypeE.Star => ServerData.MoneyType.TypeE.Star, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static int ChangePlayerFundsCount(this ServerData.MoneyType.TypeE type, int delta)
	{
		switch (type)
		{
		case ServerData.MoneyType.TypeE.Gold:
			if (delta < 0)
			{
				Metrics.OnSpendGold(-delta);
			}
			return SingletonT<ServerData>.I.PlayerParams.MoneyGoldCount += delta;
		case ServerData.MoneyType.TypeE.Diamond:
			if (delta < 0)
			{
				Metrics.OnSpendDiamond(-delta);
			}
			return SingletonT<ServerData>.I.PlayerParams.MoneyDiamondCount += delta;
		case ServerData.MoneyType.TypeE.Key:
			return SingletonT<ServerData>.I.PlayerParams.MoneyKeysCount += delta;
		case ServerData.MoneyType.TypeE.Skull:
			if (delta < 0)
			{
				Metrics.OnSpendSkull(-delta);
			}
			return SingletonT<ServerData>.I.PlayerParams.MoneySkullsCount += delta;
		case ServerData.MoneyType.TypeE.Scarab:
			return SingletonT<ServerData>.I.PlayerParams.MoneyScarabCount += delta;
		case ServerData.MoneyType.TypeE.Star:
			return SingletonT<ServerData>.I.PlayerParams.MoneyStarsCount += delta;
		default:
			throw new ArgumentOutOfRangeException("type");
		}
	}

	public static Tuple<int, int> GetPlayerSkill(this ServerData.Skill.TypeE skillType)
	{
		int num = 0;
		switch (skillType)
		{
		case ServerData.Skill.TypeE.Strength:
			num += SingletonT<ServerData>.I.PlayerParams.Strength;
			break;
		case ServerData.Skill.TypeE.Vitality:
			num += SingletonT<ServerData>.I.PlayerParams.HP;
			break;
		case ServerData.Skill.TypeE.Rage:
			num += SingletonT<ServerData>.I.PlayerParams.Rage;
			break;
		case ServerData.Skill.TypeE.Magic:
			num += SingletonT<ServerData>.I.PlayerParams.Magic;
			break;
		default:
			throw new ArgumentOutOfRangeException("skillType");
		case ServerData.Skill.TypeE.Unknown:
		case ServerData.Skill.TypeE.MagicIce:
		case ServerData.Skill.TypeE.MagicFire:
		case ServerData.Skill.TypeE.MagicDark:
		case ServerData.Skill.TypeE.MagicElectro:
		case ServerData.Skill.TypeE.BonusRage:
		case ServerData.Skill.TypeE.BonusMana:
		case ServerData.Skill.TypeE.BonusExp:
		case ServerData.Skill.TypeE.BonusMoney:
			break;
		}
		int bagBonus = 0;
		SingletonT<ServerData>.I.ForeachInBag(delegate(ServerData.Item item)
		{
			if (item.PutOn && item.Skills != null && item.Skills.Length != 0)
			{
				ServerData.SkillInfo[] skills = item.Skills;
				foreach (ServerData.SkillInfo skillInfo in skills)
				{
					if (skillInfo.Skill.Type == skillType)
					{
						bagBonus += skillInfo.Current;
					}
				}
			}
		});
		return Tuple.Create(num, bagBonus);
	}

	public static int GetPlayerSkillPoints()
	{
		return SingletonT<ServerData>.I.PlayerParams.SkillPoints;
	}

	public static void SetPlayerSkillPoints(int newvalue)
	{
		SingletonT<ServerData>.I.PlayerParams._skillPoints = newvalue;
		Messenger.Invoke(Globals.MsgPlayerSkillPointsChanged);
	}

	public static int AddToSkill(this ServerData.Skill.TypeE skillType, int value)
	{
		return skillType switch
		{
			ServerData.Skill.TypeE.Strength => SingletonT<ServerData>.I.PlayerParams.Strength += value, 
			ServerData.Skill.TypeE.Vitality => SingletonT<ServerData>.I.PlayerParams.HP += value, 
			ServerData.Skill.TypeE.Rage => SingletonT<ServerData>.I.PlayerParams.Rage += value, 
			ServerData.Skill.TypeE.Magic => SingletonT<ServerData>.I.PlayerParams.Magic += value, 
			_ => throw new ArgumentOutOfRangeException("skillType"), 
		};
	}

	public static void PutArmorOn(this ServerData.Item item)
	{
		ServerData.Slot.TypeE slotId = item.Slot.SlotId;
		if (slotId == ServerData.Slot.TypeE.Weapon)
		{
			SingletonT<ServerData>.I.MyWeapon = item;
		}
		GameObject playerGameObject = Globals.PlayerGameObject;
		if (!(playerGameObject == null))
		{
			PersonArmor component = playerGameObject.GetComponent<PersonArmor>();
			if (!(component == null))
			{
				component.ChangeArmor(SingletonT<ServerData>.I.PlayerServerPersData.ModelId, item, item.Slot.SlotId, null);
			}
		}
	}

	public static void RemoveArmor(this ServerData.Item item)
	{
		GameObject playerGameObject = Globals.PlayerGameObject;
		if (!(playerGameObject == null))
		{
			PersonArmor component = playerGameObject.GetComponent<PersonArmor>();
			if (!(component == null))
			{
				component.ChangeArmor(SingletonT<ServerData>.I.PlayerServerPersData.ModelId, null, item.Slot.SlotId, null);
			}
		}
	}

	public static ServerData.Item GetPuppetItem(this ServerData.Item item)
	{
		ServerData.Slot.TypeE slotType = item.Slot.SlotId;
		return SingletonT<ServerData>.I.FindInBag((ServerData.Item bagItem) => bagItem.PutOn && bagItem.Slot.SlotId == slotType);
	}

	public static bool ItemSkillIsBonus(this ServerData.Item item)
	{
		if (item == null)
		{
			return false;
		}
		ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
		if (itemSkillInfo == null)
		{
			return false;
		}
		switch (itemSkillInfo.Skill.Type)
		{
		case ServerData.Skill.TypeE.Unknown:
		case ServerData.Skill.TypeE.Strength:
		case ServerData.Skill.TypeE.Vitality:
		case ServerData.Skill.TypeE.Rage:
		case ServerData.Skill.TypeE.Magic:
			return false;
		case ServerData.Skill.TypeE.FullMana:
		case ServerData.Skill.TypeE.FullRage:
			return true;
		case ServerData.Skill.TypeE.MagicIce:
		case ServerData.Skill.TypeE.MagicFire:
		case ServerData.Skill.TypeE.MagicDark:
		case ServerData.Skill.TypeE.MagicElectro:
		case ServerData.Skill.TypeE.BonusRage:
		case ServerData.Skill.TypeE.BonusMana:
		case ServerData.Skill.TypeE.BonusExp:
		case ServerData.Skill.TypeE.BonusMoney:
			return true;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static int GetElixirCount(this ServerData.Item.ElixirTypeE type)
	{
		return SingletonT<ServerData>.I.GetPlayerElixirsCount(type);
	}

	public static bool PuppetHasIt(this ServerData.ShopGood shopGood)
	{
		ServerData.Item puppetItem = shopGood.Item.GetPuppetItem();
		if (puppetItem == null)
		{
			return false;
		}
		Tuple<string, int> itemDescription = puppetItem.GetItemDescription();
		Tuple<string, int> itemDescription2 = shopGood.Item.GetItemDescription();
		return puppetItem.Id == shopGood.Item.Id && itemDescription2.Item2 <= itemDescription.Item2;
	}

	public static bool PlayerHasIt(this ServerData.ShopGood shopGood)
	{
		ServerData.Item shopItem = shopGood.Item;
		ServerData.Item item = SingletonT<ServerData>.I.FindInBag((ServerData.Item it) => it.ItemStructuralEqualTo(shopItem));
		return item != null;
	}

	public static bool PlayerHasBetter(this ServerData.ShopGood shopGood)
	{
		ServerData.Item puppetItem = shopGood.Item.GetPuppetItem();
		if (puppetItem == null)
		{
			return false;
		}
		Tuple<string, int> itemDescription = puppetItem.GetItemDescription();
		Tuple<string, int> itemDescription2 = shopGood.Item.GetItemDescription();
		if (Globals.IsDebugBuild)
		{
			Debug.Log("=== inbag:{0} inshop:{1} HAS:{2}===".Fmt(itemDescription, itemDescription2, itemDescription2.Item2 <= itemDescription.Item2));
		}
		return itemDescription.Item1 == itemDescription2.Item1 && itemDescription2.Item2 <= itemDescription.Item2;
	}

	public static Tuple<string, bool, string, bool> GetChangeStatsDigits(ServerData.Item newItem, ServerData.Item oldItem)
	{
		Func<bool, string> func = (bool b) => (!b) ? "-" : "+";
		Tuple<string, int> itemSkillDigits = newItem.GetItemSkillDigits();
		Tuple<string, int> itemSkillDigits2 = oldItem.GetItemSkillDigits();
		if (itemSkillDigits == null && itemSkillDigits2 == null)
		{
			return EmptyDigits;
		}
		if (itemSkillDigits == null)
		{
			return Tuple.Create(itemSkillDigits2.Item1 + func(arg1: false) + itemSkillDigits2.Item2, item2: false, string.Empty, item4: false);
		}
		if (itemSkillDigits2 == null)
		{
			return Tuple.Create(itemSkillDigits.Item1 + func(arg1: true) + itemSkillDigits.Item2, item2: true, string.Empty, item4: false);
		}
		if (itemSkillDigits.Item1 == itemSkillDigits2.Item1)
		{
			int num = itemSkillDigits.Item2 - itemSkillDigits2.Item2;
			if (num == 0)
			{
				return EmptyDigits;
			}
			bool flag = num > 0;
			return Tuple.Create(itemSkillDigits2.Item1 + func(flag) + Math.Abs(num), flag, string.Empty, item4: false);
		}
		return Tuple.Create(itemSkillDigits2.Item1 + func(arg1: false) + itemSkillDigits2.Item2, item2: false, itemSkillDigits.Item1 + func(arg1: true) + itemSkillDigits.Item2, item4: true);
	}
}
