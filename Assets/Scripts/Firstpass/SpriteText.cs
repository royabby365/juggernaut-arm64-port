using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteText : RendererHandler, IRendererHandler
{
	public enum AdvanceOutlineMode
	{
		No,
		Half,
		Full
	}

	public enum CapitalizeMode
	{
		Non,
		First,
		All,
		Upcase
	}

	private const bool ExFont = true;

	public string Text = "Hello There!";

	public ServerData.PhrasesE Phrase;

	public int FillColumn = -1;

	public float TextAlpha = 1f;

	public FontManager.ColorE NamedColorE;

	public FontManager.FontFamilyE FontFamily;

	public int PixSize = 26;

	public bool Bold;

	public bool Italic;

	public bool Outline = true;

	public int LineSpacing;

	public CapitalizeMode capitalizeMode;

	public int Tracking;

	public AdvanceOutlineMode AdvanceOutline;

	public Color TopColor = new Color(0.99f, 0.99f, 0.99f, 1f);

	public Color BottomColor = new Color(0.99f, 0.99f, 0.99f, 1f);

	private Color TopColor0 = new Color(0.99f, 0.99f, 0.99f, 1f);

	private Color BottomColor0 = new Color(0.99f, 0.99f, 0.99f, 1f);

	public bool propagateChanges;

	public TextAnchor Anchor = TextAnchor.LowerLeft;

	private Material fontMaterial;

	private BmFont bmfont;

	private readonly Vector3[] _quadVerts = new Vector3[4]
	{
		new Vector3(0f, 0f),
		new Vector3(0f, 1f),
		new Vector3(1f, 1f),
		new Vector3(1f, 0f)
	};

	private readonly bool[] _vcolors = new bool[4] { false, true, true, false };

	private readonly Vector2[] _quadUvs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f)
	};

	private readonly int[] _quadTriangles = new int[6] { 0, 1, 2, 2, 3, 0 };

	private Bounds _bounds;

	private static readonly string[] CapitalizeExceptions = new string[1] { "and" };

	public string Text_
	{
		get
		{
			return Text;
		}
		set
		{
			if (Text != value)
			{
				Text = value;
				Refresh();
			}
		}
	}

	public ServerData.PhrasesE Phrase_
	{
		get
		{
			return Phrase;
		}
		set
		{
			if (Phrase != value)
			{
				Phrase = value;
				Refresh();
			}
		}
	}

	public int FillColumn_
	{
		get
		{
			return FillColumn;
		}
		set
		{
			if (FillColumn != value)
			{
				FillColumn = value;
				Refresh();
			}
		}
	}

	public float TextAlpha_
	{
		get
		{
			return TextAlpha;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (!TextAlpha.Eqv(value))
			{
				TextAlpha = value;
				SetTextAlpha();
			}
		}
	}

	public FontManager.ColorE NamedColorE_
	{
		get
		{
			return NamedColorE;
		}
		set
		{
			if (NamedColorE != value)
			{
				NamedColorE = value;
				Refresh();
			}
		}
	}

	public FontManager.FontFamilyE FontFamily_
	{
		get
		{
			return FontFamily;
		}
		set
		{
			if (FontFamily != value)
			{
				FontFamily = value;
				bmfont = null;
				Refresh();
			}
		}
	}

	public int PixSize_
	{
		get
		{
			return PixSize;
		}
		set
		{
			if (PixSize != value)
			{
				PixSize = value;
				bmfont = null;
				Refresh();
			}
		}
	}

	public bool Bold_
	{
		get
		{
			return Bold;
		}
		set
		{
			if (Bold != value)
			{
				Bold = value;
				bmfont = null;
				Refresh();
			}
		}
	}

	public bool Italic_
	{
		get
		{
			return Italic;
		}
		set
		{
			if (Italic != value)
			{
				Italic = value;
				bmfont = null;
				Refresh();
			}
		}
	}

	public bool Outline_
	{
		get
		{
			return Outline;
		}
		set
		{
			if (Outline != value)
			{
				Outline = value;
				bmfont = null;
				Refresh();
			}
		}
	}

	public int LineSpacing_
	{
		get
		{
			return LineSpacing;
		}
		set
		{
			if (LineSpacing != value)
			{
				LineSpacing = value;
				Refresh();
			}
		}
	}

	public CapitalizeMode capitalizeMode_
	{
		get
		{
			return capitalizeMode;
		}
		set
		{
			if (capitalizeMode != value)
			{
				capitalizeMode = value;
				Refresh();
			}
		}
	}

	public int Tracking_
	{
		get
		{
			return Tracking;
		}
		set
		{
			if (Tracking != value)
			{
				Tracking = value;
				Refresh();
			}
		}
	}

	public AdvanceOutlineMode AdvanceOutline_
	{
		get
		{
			return AdvanceOutline;
		}
		set
		{
			if (AdvanceOutline != value)
			{
				AdvanceOutline = value;
				Refresh();
			}
		}
	}

	public TextAnchor Anchor_
	{
		get
		{
			return Anchor;
		}
		set
		{
			if (Anchor != value)
			{
				Anchor = value;
				Refresh();
			}
		}
	}

	public void SetColor(Color top, Color bottom)
	{
		TopColor = top;
		BottomColor = bottom;
	}

	public void SetColor(string colorName)
	{
		FontColor namedColor = FontManager.Instance.GetNamedColor(colorName);
		SetColor(namedColor.TopColor, namedColor.BottomColor);
	}

	public void SetColor(FontManager.ColorE colorE)
	{
		if (colorE != FontManager.ColorE.None)
		{
			NamedColorE = colorE;
		}
	}

	private void SetAlpha(float alpha)
	{
		Color topColor = TopColor;
		Color bottomColor = BottomColor;
		float a = Mathf.Min(topColor.a, alpha);
		float a2 = Mathf.Min(bottomColor.a, alpha);
		topColor = new Color(topColor.r, topColor.g, topColor.b, a);
		bottomColor = new Color(bottomColor.r, bottomColor.g, bottomColor.b, a2);
		Color color = new Color(1f, 1f, 1f, alpha);
		Color[] colors = base.transform.GetComponent<MeshFilter>().sharedMesh.colors;
		for (int i = 0; i < colors.Length; i++)
		{
			if (colors[i].r == 1f && colors[i].g == 1f && colors[i].b == 1f)
			{
				colors[i] = color;
				continue;
			}
			ref Color reference = ref colors[i];
			reference = ((!_vcolors[i % 4]) ? bottomColor : topColor);
		}
		base.transform.GetComponent<MeshFilter>().sharedMesh.colors = colors;
	}

	public void TurnOnRenderer()
	{
		DoTurnOnRenderer();
	}

	public void TurnOffRenderer()
	{
		DoTurnOffRenderer();
	}

	private void Awake()
	{
		RegenerateSprite();
		base.tag = TexturePacker.doNotPutInAtlasTag;
		Refresh();
	}

	private void SetTextAlpha()
	{
		TextAlpha = Mathf.Clamp01(TextAlpha_);
		SetAlpha(TextAlpha);
	}

	private void Refresh()
	{
		if (MeshFilter == null)
		{
			return;
		}
		if (bmfont == null)
		{
			if (FontManager.Instance != null)
			{
				bmfont = GetBmFontResource();
			}
			if (bmfont == null)
			{
				return;
			}
			fontMaterial = bmfont.Atlas;
		}
		if (NamedColorE_ != FontManager.ColorE.None)
		{
			SetColor(Enum.GetName(typeof(FontManager.ColorE), NamedColorE_));
		}
		if (Phrase_ != ServerData.PhrasesE.Custom)
		{
			Text = SingletonT<ServerData>.I.GetPhrase(Phrase_);
		}
		Text = Capitalize(Text);
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (FillColumn_ > 0)
		{
			Text = Text_.ForceTextIntoMultipleLines((int)((float)FillColumn_ / UnityApi.CharWidth));
		}
		Vector2 vector = MeasureString(Text);
		if (Anchor_ == TextAnchor.LowerCenter || Anchor_ == TextAnchor.LowerLeft || Anchor_ == TextAnchor.LowerRight)
		{
			zero2.y = 0f - vector.y;
		}
		if (Anchor_ == TextAnchor.UpperCenter || Anchor_ == TextAnchor.UpperLeft || Anchor_ == TextAnchor.UpperRight)
		{
			zero2.y = 0f;
		}
		if (Anchor_ == TextAnchor.MiddleCenter || Anchor_ == TextAnchor.MiddleLeft || Anchor_ == TextAnchor.MiddleRight)
		{
			zero2.y = (0f - vector.y) / 2f;
		}
		if (Anchor_ == TextAnchor.UpperRight || Anchor_ == TextAnchor.MiddleRight || Anchor_ == TextAnchor.LowerRight)
		{
			zero2.x = vector.x;
		}
		if (Anchor_ == TextAnchor.UpperCenter || Anchor_ == TextAnchor.MiddleCenter || Anchor_ == TextAnchor.LowerCenter)
		{
			zero2.x = vector.x / 2f;
		}
		zero2.x = zero2.x.RoundToInt();
		zero2.y = zero2.y.RoundToInt();
		MeshFilter component = GetComponent<MeshFilter>();
		MeshData meshData = GenerateTextMesh(zero - zero2, Text_);
		Mesh mesh = component.mesh;
		mesh.Clear();
		mesh.vertices = meshData.vertices;
		mesh.triangles = meshData.triangles;
		mesh.colors = meshData.colors;
		mesh.uv = meshData.uv;
		if (!(GetComponent<Renderer>() == null))
		{
			GetComponent<Renderer>().material = fontMaterial;
			_bounds = component.mesh.bounds;
			if (propagateChanges)
			{
				BroadcastMessage("MoveMe", new Rect(0f, 0f, _bounds.size.x, _bounds.size.y), SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public Bounds GetBounds()
	{
		return _bounds;
	}

	private MeshData GenerateTextMesh(Vector3 position, string str)
	{
		if (str == null)
		{
			str = string.Empty;
		}
		int num = str.Length + 16;
		List<int> list = new List<int>(num * 2);
		List<Vector3> list2 = new List<Vector3>(num * 4);
		List<Vector2> list3 = new List<Vector2>(num * 4);
		List<Color> list4 = new List<Color>(num * 4);
		float num2 = bmfont.LineHeight + (float)LineSpacing_;
		int outlineTrackingAmount = GetOutlineTrackingAmount();
		Vector3 vector = position;
		for (int i = 0; i < str.Length; i++)
		{
			char c = str[i];
			switch (c)
			{
			case '\n':
				vector.y -= num2;
				vector.x = position.x;
				continue;
			case '\r':
				continue;
			}
			BmChar bitmapChar = bmfont.GetBitmapChar(c);
			if (bitmapChar == null)
			{
				continue;
			}
			int count = list2.Count;
			Rect uVRect = bmfont.GetUVRect(bitmapChar);
			Vector2 b = new Vector2(uVRect.width, uVRect.height);
			Vector2 vector2 = new Vector2(uVRect.x, uVRect.y);
			for (int j = 0; j < 4; j++)
			{
				list3.Add(Vector2.Scale(_quadUvs[j], b) + vector2);
			}
			for (int k = 0; k < 4; k++)
			{
				if (c > '\u001f')
				{
					list4.Add((!_vcolors[k]) ? BottomColor : TopColor);
				}
				else
				{
					list4.Add(Color.white);
				}
			}
			Vector3 b2 = bitmapChar.Size;
			Vector3 vector3 = bitmapChar.Offset;
			vector3.y += b2.y;
			vector3.x *= -1f;
			for (int l = 0; l < 4; l++)
			{
				list2.Add(Vector3.Scale(_quadVerts[l], b2) + vector - vector3);
			}
			for (int m = 0; m < 6; m++)
			{
				list.Add(_quadTriangles[m] + count);
			}
			float num3 = 0f;
			if (i < str.Length - 1)
			{
				num3 = bmfont.GetKerning(c, str[i + 1]);
			}
			vector.x += bitmapChar.XAdvance + num3 + (float)Tracking_ + (float)outlineTrackingAmount;
		}
		return new MeshData
		{
			vertices = list2.ToArray(),
			uv = list3.ToArray(),
			colors = list4.ToArray(),
			triangles = list.ToArray()
		};
	}

	private BmFont GetBmFontResource()
	{
		string family = GetFamily(FontFamily_);
		family += PixSize_;
		if (Bold_)
		{
			family += "b";
		}
		if (Italic_)
		{
			family += "i";
		}
		if (Outline_)
		{
			family += "o";
		}
		BmFont font = FontManager.Instance.GetFont(family);
		if (font == null && Globals.IsDebugBuild)
		{
			Debug.LogError($"cannot load font:'{family}' for {base.name}@...{base.transform.root.name}");
		}
		return font;
	}

	private static string GetFamily(FontManager.FontFamilyE family)
	{
		return family switch
		{
			FontManager.FontFamilyE.Tahoma => "Tahoma", 
			FontManager.FontFamilyE.AndaleMono => "Andale Mono", 
			_ => throw new ArgumentOutOfRangeException("family"), 
		};
	}

	private string Capitalize(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return capitalizeMode_ switch
		{
			CapitalizeMode.Non => input, 
			CapitalizeMode.First => input.CapitalizeFirstLetter(), 
			CapitalizeMode.All => input.CapitalizeEx(2, CapitalizeExceptions), 
			CapitalizeMode.Upcase => input.Upcase(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private int GetOutlineTrackingAmount()
	{
		if (!Outline_)
		{
			return 0;
		}
		int outline = bmfont.Outline;
		return AdvanceOutline_ switch
		{
			AdvanceOutlineMode.No => 0, 
			AdvanceOutlineMode.Half => outline, 
			AdvanceOutlineMode.Full => 2 * outline, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private Vector2 MeasureString(string text)
	{
		Vector2 zero = Vector2.zero;
		if (string.IsNullOrEmpty(text))
		{
			return zero;
		}
		float num = bmfont.LineHeight + (float)LineSpacing_;
		float num2 = 0f;
		float num3 = bmfont.LineHeight;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '\n':
				num3 += num;
				num2 = 0f;
				continue;
			case '\r':
				continue;
			}
			BmChar bitmapChar = bmfont.GetBitmapChar(c);
			if (bmfont.GetBitmapChar(c) != null)
			{
				float num4 = 0f;
				if (i + 1 < text.Length)
				{
					num4 = bmfont.GetKerning(c, text[i + 1]);
				}
				num2 += num4 + bitmapChar.XAdvance + (float)Tracking_;
				if (num2 > zero.x)
				{
					zero.x = num2;
				}
			}
		}
		zero.y = num3;
		return zero;
	}
}
