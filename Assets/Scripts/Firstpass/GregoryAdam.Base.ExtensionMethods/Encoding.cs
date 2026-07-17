using System.Text;

namespace GregoryAdam.Base.ExtensionMethods;

public static class Encoding
{
	public static byte[] ToUTF8(this char c)
	{
		return System.Text.Encoding.UTF8.GetBytes(new char[1] { c });
	}

	public static byte[] ToUTF8(this char[] c)
	{
		return System.Text.Encoding.UTF8.GetBytes(c);
	}

	public static byte[] ToUTF8(this char[] c, int index, int count)
	{
		return System.Text.Encoding.UTF8.GetBytes(c, index, count);
	}

	public static byte[] ToUTF8(this string s)
	{
		return System.Text.Encoding.UTF8.GetBytes(s);
	}

	public static byte[] ToUTF8(this string s, int index, int count)
	{
		return System.Text.Encoding.UTF8.GetBytes(s.Substring(index, count));
	}

	public static string FromUTF8(this byte[] b)
	{
		return System.Text.Encoding.UTF8.GetString(b);
	}

	public static string FromUTF8(this byte[] b, int index, int count)
	{
		return System.Text.Encoding.UTF8.GetString(b, index, count);
	}
}
