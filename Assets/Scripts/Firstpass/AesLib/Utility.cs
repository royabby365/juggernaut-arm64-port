using System;

namespace AesLib;

public class Utility
{
	public static byte[] Decrypt(Aes cipher, byte[] cipherBytes)
	{
		byte[] array = new byte[cipherBytes.Length];
		long num = Aes.BlockSize();
		if (cipherBytes.Length % num != 0L)
		{
			throw new ArgumentException("Input data does not match AES block size");
		}
		for (long num2 = 0L; num2 < cipherBytes.Length; num2 += num)
		{
			byte[] array2 = new byte[num];
			Array.Copy(cipherBytes, num2, array2, 0L, num);
			byte[] array3 = new byte[num];
			cipher.InvCipher(array2, array3);
			Array.Copy(array3, 0L, array, num2, num);
		}
		return array;
	}
}
