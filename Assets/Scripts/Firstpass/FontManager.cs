using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FontManager : MonoBehaviour
{
	public enum ColorE
	{
		None,
		BagSectionColor,
		BagStatColor,
		BagMoney,
		BagSkillPoints,
		CountSmall,
		ItemViolet,
		ItemBlue,
		ItemGreen,
		ItemGray,
		ItemRed,
		ButtonGold,
		ButtonBuyShop,
		Brass,
		DescriptionText,
		DescriptionsStatLabel,
		LevelDigitSteel,
		CompareGreen,
		CompareRed,
		OldTextBrown,
		LockMessageGold,
		LockMessageGreen,
		LockMessageRed,
		MagicFire,
		MagicIce,
		MagicElectro,
		MagicDark,
		Block,
		Dodge,
		DamageForced,
		Damage,
		Immunity,
		HealthPotion,
		MagicFireMonotone,
		MagicDarkMonotone,
		MagicIceMonotone,
		MagicLightningMonotone,
		MagicBookInactiveText,
		PoisonDamage,
		PureGray,
		FightResult0,
		FightResult1,
		FightResult2,
		FightResult3,
		FightResult4,
		ItemGold
	}

	public enum FontFamilyE
	{
		Tahoma,
		AndaleMono
	}

	public string AssetsPath = "bmfonts_ex";

	public string AssetSuffix = "_font2";

	public FontColor[] FontColors;

	private readonly Dictionary<string, BmFont> _loaded = new Dictionary<string, BmFont>();

	private static FontManager _instance;

	public static FontManager Instance
	{
		get
		{
			return _instance;
		}
		private set
		{
			if (_instance == null || _instance == value)
			{
				_instance = value;
				_instance.InitNamedColors();
			}
			else if (_instance != value && Globals.IsDebugBuild)
			{
				Debug.LogError($"FontManager should be a singleton");
			}
		}
	}

	public static string ColorToKey(ColorE colorE)
	{
		return Enum.GetName(typeof(ColorE), colorE);
	}

	public void Clear()
	{
		_loaded.Clear();
	}

	public void Shutdown()
	{
		_instance = null;
	}

	public BmFont GetFont(string bmfontName)
	{
		if (_loaded.ContainsKey(bmfontName))
		{
			return _loaded[bmfontName];
		}
		string language = UnityApi.GetLanguage();
		string text = AssetsPath;
		if (language.Contains("ko"))
		{
			text += "_ko";
		}
		else if (language.Contains("zh") || language.Contains("cn"))
		{
			text += "_zh";
		}
		string text2 = $"{text}/{bmfontName}{AssetSuffix}";
		GameObject gameObject = Util.Resource<GameObject>(text2);
		if (gameObject == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogWarning($"Cannot load font: {text2}");
			}
			return null;
		}
		gameObject.SetActive(false);
		BmFont component = gameObject.transform.GetComponent<BmFont>();
		component.Init();
		BmFont bmFont = component;
		_loaded[bmfontName] = bmFont;
		return bmFont;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void InitNamedColors()
	{
		List<FontColor> list = new List<FontColor>();
		list.Add(FontColor.Create(ColorE.None, Color.white, Color.white));
		list.Add(FontColor.Create(ColorE.BagSectionColor, new Color32(byte.MaxValue, 203, 105, 216), new Color32(152, 108, 62, 221)));
		list.Add(FontColor.Create(ColorE.BagStatColor, new Color32(byte.MaxValue, 216, 153, byte.MaxValue), new Color32(130, 99, 49, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.BagMoney, new Color32(byte.MaxValue, 223, 146, byte.MaxValue), new Color32(236, 137, 0, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.BagSkillPoints, new Color32(byte.MaxValue, 106, 253, byte.MaxValue), new Color32(101, 47, 96, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.CountSmall, new Color32(237, 224, 193, byte.MaxValue), new Color32(byte.MaxValue, 157, 36, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.ItemViolet, new Color32(206, 125, 214, byte.MaxValue), new Color32(116, 27, 130, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.ItemBlue, new Color32(124, 153, 206, byte.MaxValue), new Color32(22, 62, 121, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.ItemGreen, new Color32(164, 214, 127, byte.MaxValue), new Color32(62, 104, 0, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.ItemGray, new Color32(208, 202, 193, byte.MaxValue), new Color32(104, 97, 87, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.ItemRed, new Color32(214, 127, 127, byte.MaxValue), new Color32(104, 32, 32, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.ItemGold, new Color32(253, 247, 120, 240), new Color32(196, 82, 0, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.ButtonGold, new Color32(253, 247, 120, 240), new Color32(196, 82, 0, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.ButtonBuyShop, new Color32(239, 220, 75, byte.MaxValue), new Color32(236, 138, 0, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.Brass, new Color32(byte.MaxValue, 171, 68, 138), new Color32(116, 85, 32, 155)));
		list.Add(FontColor.Create(ColorE.DescriptionText, new Color32(220, 178, 68, 240), new Color32(220, 178, 68, 240)));
		list.Add(FontColor.Create(ColorE.DescriptionsStatLabel, new Color32(231, 188, 117, byte.MaxValue), new Color32(201, 161, 99, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.LevelDigitSteel, new Color32(20, 22, 44, 200), new Color32(20, 22, 44, 200)));
		list.Add(FontColor.Create(ColorE.CompareGreen, new Color32(198, 247, 65, 240), new Color32(56, 99, 22, 240)));
		list.Add(FontColor.Create(ColorE.CompareRed, new Color32(237, 32, 0, 240), new Color32(142, 19, 0, 240)));
		list.Add(FontColor.Create(ColorE.OldTextBrown, new Color32(91, 49, 1, 200), new Color32(26, 17, 6, 200)));
		list.Add(FontColor.Create(ColorE.LockMessageGold, new Color32(254, 228, 161, 240), new Color32(128, 91, 27, 240)));
		list.Add(FontColor.Create(ColorE.MagicFire, new Color32(byte.MaxValue, 250, 0, 240), new Color32(byte.MaxValue, 37, 0, 240)));
		list.Add(FontColor.Create(ColorE.MagicElectro, new Color32(158, 212, 222, 240), new Color32(0, 130, 155, 240)));
		list.Add(FontColor.Create(ColorE.MagicDark, new Color32(141, 89, 156, 240), new Color32(10, 0, 15, 240)));
		list.Add(FontColor.Create(ColorE.MagicIce, new Color32(155, 204, byte.MaxValue, 240), new Color32(6, 109, 216, 240)));
		list.Add(FontColor.Create(ColorE.Block, new Color32(63, 222, byte.MaxValue, 240), new Color32(34, 120, 166, 240)));
		list.Add(FontColor.Create(ColorE.Dodge, new Color32(167, byte.MaxValue, 39, 240), new Color32(90, 144, 21, 240)));
		list.Add(FontColor.Create(ColorE.DamageForced, new Color32(byte.MaxValue, 0, 0, 240), new Color32(byte.MaxValue, 0, 0, 240)));
		list.Add(FontColor.Create(ColorE.Damage, new Color32(byte.MaxValue, 226, 0, 240), new Color32(byte.MaxValue, 226, 0, 240)));
		list.Add(FontColor.Create(ColorE.Immunity, new Color32(179, 0, byte.MaxValue, 240), new Color32(102, 0, 179, 240)));
		list.Add(FontColor.Create(ColorE.HealthPotion, new Color32(0, byte.MaxValue, 0, 240), new Color32(0, byte.MaxValue, 0, 240)));
		list.Add(FontColor.Create(ColorE.PoisonDamage, new Color32(164, 214, 127, byte.MaxValue), new Color32(62, 104, 0, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.MagicFireMonotone, new Color32(byte.MaxValue, 97, 27, 240), new Color32(226, 72, 3, 240)));
		list.Add(FontColor.Create(ColorE.MagicLightningMonotone, new Color32(20, 155, 183, 240), new Color32(0, 134, 163, 240)));
		list.Add(FontColor.Create(ColorE.MagicIceMonotone, new Color32(38, 129, 230, 240), new Color32(19, 103, 196, 240)));
		list.Add(FontColor.Create(ColorE.MagicDarkMonotone, new Color32(121, 75, 171, 240), new Color32(90, 47, 114, 240)));
		list.Add(FontColor.Create(ColorE.MagicBookInactiveText, new Color32(176, 135, 83, 240), new Color32(176, 135, 83, 240)));
		list.Add(FontColor.Create(ColorE.PureGray, Color.gray, Color.gray));
		list.Add(FontColor.Create(ColorE.FightResult0, new Color32(208, 202, 193, 240), new Color32(104, 97, 87, 240)));
		list.Add(FontColor.Create(ColorE.FightResult1, new Color32(164, 214, 127, byte.MaxValue), new Color32(62, 104, 0, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.FightResult2, new Color32(124, 153, 206, byte.MaxValue), new Color32(22, 62, 121, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.FightResult3, new Color32(byte.MaxValue, 106, 253, byte.MaxValue), new Color32(101, 47, 96, byte.MaxValue)));
		list.Add(FontColor.Create(ColorE.FightResult4, new Color32(237, 32, 0, 240), new Color32(142, 19, 0, 240)));
		List<FontColor> list2 = list;
		FontColors = list2.ToArray();
	}

	public FontColor GetNamedColor(string colorName)
	{
		int num = Array.FindIndex(FontColors, (FontColor fc) => fc.Name == colorName);
		if (num < 0)
		{
			return null;
		}
		return FontColors[num];
	}

	public FontColor GetNamedColor(ColorE color)
	{
		return GetNamedColor(ColorToKey(color));
	}
}
