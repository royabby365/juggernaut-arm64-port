using System;
using System.Globalization;
using System.Text;

namespace GregoryAdam.Base.ExtensionMethods;

public static class HexConversion
{
	public static string ToHex(this byte b)
	{
		return b.ToString("X2");
	}

	public static string ToHex(this byte[] b)
	{
		StringBuilder stringBuilder = new StringBuilder(b.Length << 1);
		int upperBound = b.GetUpperBound(0);
		for (int i = b.GetLowerBound(0); i <= upperBound; i++)
		{
			stringBuilder.Append(b[i].ToHex());
		}
		return stringBuilder.ToString();
	}

	public static string ToUTF8Hex(this string s)
	{
		return s.ToUTF8().ToHex();
	}

	public static byte[] FromHex(this string s)
	{
		if (s.Length % 2 != 0)
		{
			throw new ArgumentException("Length must be even");
		}
		byte[] array = new byte[s.Length >> 1];
		int num = 0;
		int num2 = 0;
		while (num < s.Length)
		{
			array[num2] = byte.Parse(s.Substring(num, 2), NumberStyles.HexNumber);
			num += 2;
			num2++;
		}
		return array;
	}

	public static string FromUTF8Hex(this string s)
	{
		return s.FromHex().FromUTF8();
	}
}
