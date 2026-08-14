using System.Collections.Generic;
using UnityEngine;

public class BmFont : MonoBehaviour
{
	private struct KerningPair
	{
		private readonly char _left;

		private readonly char _right;

		public KerningPair(char left, char right)
		{
			_left = left;
			_right = right;
		}

		public override int GetHashCode()
		{
			return 31 * _left + _right;
		}

		public override bool Equals(object other)
		{
			if (!(other is KerningPair kerningPair))
			{
				return false;
			}
			return kerningPair._left == _left && kerningPair._right == _right;
		}
	}

	public float FontSize;

	public float LineHeight;

	public float Base;

	public float ScaleW;

	public float ScaleH;

	public int Outline;

	public BmChar[] Chars;

	public Material Atlas;

	public BmKerning[] Kernings;

	private readonly Dictionary<KerningPair, float> _kernings = new Dictionary<KerningPair, float>();

	private readonly Dictionary<int, BmChar> _bmchars = new Dictionary<int, BmChar>();

	private bool _inited;

	internal void Init()
	{
		BmKerning[] kernings = Kernings;
		foreach (BmKerning bmKerning in kernings)
		{
			KerningPair key = new KerningPair((char)bmKerning.FirstChar, (char)bmKerning.SecondChar);
			_kernings[key] = bmKerning.Amount;
		}
		BmChar[] chars = Chars;
		foreach (BmChar bmChar in chars)
		{
			_bmchars[bmChar.Id] = bmChar;
		}
		_inited = true;
	}

	public BmChar GetBitmapChar(int c)
	{
		c = SubstituteChar(c);
		if (!_inited)
		{
			Init();
		}
		if (_bmchars.ContainsKey(c))
		{
			return _bmchars[c];
		}
		return null;
	}

	private int SubstituteChar(int ch)
	{
		switch (ch)
		{
		case 1105:
			return 1077;
		case 1025:
			return 1045;
		case 8216:
		case 8217:
			return 34;
		case 8211:
			return 45;
		default:
			return ch;
		}
	}

	public Rect GetUVRect(BmChar bmChar)
	{
		Vector2 vector = new Vector2(bmChar.Size.x / ScaleW, bmChar.Size.y / ScaleH);
		Vector2 vector2 = new Vector2(bmChar.Position.x / ScaleW, bmChar.Position.y / ScaleH);
		Vector2 vector3 = new Vector2(vector2.x, 1f - (vector2.y + vector.y));
		return new Rect(vector3.x, vector3.y, vector.x, vector.y);
	}

	public float GetKerning(char first, char second)
	{
		first = (char)SubstituteChar(first);
		second = (char)SubstituteChar(second);
		KerningPair key = new KerningPair(first, second);
		if (_kernings.ContainsKey(key))
		{
			return _kernings[new KerningPair(first, second)];
		}
		return 0f;
	}
}
