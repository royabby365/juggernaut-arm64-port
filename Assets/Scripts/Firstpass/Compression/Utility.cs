using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;

namespace Compression
{

public class Utility
{
	public static string Decompress(byte[] bytes)
	{
		MemoryStream baseInputStream = new MemoryStream(bytes, writable: false);
		GZipInputStream gZipInputStream = new GZipInputStream(baseInputStream);
		byte[] array = new byte[65536];
		char[] array2 = new char[65536];
		Decoder decoder = Encoding.UTF8.GetDecoder();
		StringBuilder stringBuilder = new StringBuilder();
		long num = 0L;
		try
		{
			while (true)
			{
				int num2 = gZipInputStream.Read(array, 0, array.Length);
				if (num2 == 0)
				{
					break;
				}
				num += num2;
				int chars = decoder.GetChars(array, 0, num2, array2, 0);
				stringBuilder.Append(array2, 0, chars);
			}
		}
		catch (GZipException)
		{
		}
		gZipInputStream.Close();
		return stringBuilder.ToString();
	}
}
}
