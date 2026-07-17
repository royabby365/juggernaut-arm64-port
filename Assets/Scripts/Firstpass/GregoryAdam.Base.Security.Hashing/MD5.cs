using System;
using System.IO;
using GregoryAdam.Base.ExtensionMethods;

namespace GregoryAdam.Base.Security.Hashing;

public sealed class MD5
{
	private enum SS
	{
		S11 = 7,
		S12 = 12,
		S13 = 17,
		S14 = 22,
		S21 = 5,
		S22 = 9,
		S23 = 14,
		S24 = 20,
		S31 = 4,
		S32 = 11,
		S33 = 16,
		S34 = 23,
		S41 = 6,
		S42 = 10,
		S43 = 15,
		S44 = 21
	}

	private const int BufShiftBits = 6;

	private const int BufSize = 64;

	private const int BufSizeModuloMask = 63;

	private const int ReadBufSize = 8192;

	private uint State0;

	private uint State1;

	private uint State2;

	private uint State3;

	private static byte[] Padding;

	private byte[] Buffer;

	private ulong Count;

	private int BytesInBuffer => (int)Count & 0x3F;

	public MD5()
	{
		Initialize();
	}

	static MD5()
	{
		Padding = new byte[64];
		Padding[0] = 128;
	}

	public byte[] Hash(byte[] input, int index, int count)
	{
		Initialize();
		Update(input, index, count);
		return Final();
	}

	public byte[] Hash(byte[] input)
	{
		return Hash(input, input.GetLowerBound(0), input.GetUpperBound(0) - input.GetLowerBound(0) + 1);
	}

	public byte[] Hash(string input)
	{
		return Hash(input.ToUTF8());
	}

	public byte[] Hash(string input, int index, int count)
	{
		return Hash(input.ToUTF8(index, count));
	}

	public byte[] Hash(Stream stream)
	{
		byte[] array = new byte[8192];
		Initialize();
		int inputCount;
		while ((inputCount = stream.Read(array, 0, 8192)) > 0)
		{
			Update(array, 0, inputCount);
		}
		return Final();
	}

	public byte[] HashFile(string FileName)
	{
		using FileStream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
		return Hash(stream);
	}

	private void Initialize()
	{
		State0 = 1732584193u;
		State1 = 4023233417u;
		State2 = 2562383102u;
		State3 = 271733878u;
		Count = 0uL;
		if (Buffer == null)
		{
			Buffer = new byte[64];
		}
	}

	private void Update(byte[] input)
	{
		Update(input, input.GetLowerBound(0), input.GetUpperBound(0) - input.GetLowerBound(0) + 1);
	}

	private void Update(byte[] input, int inputIndex, int inputCount)
	{
		int num = BytesInBuffer;
		int num2 = 64 - num;
		Count += (ulong)inputCount;
		if (num > 0 && inputCount >= num2)
		{
			Array.Copy(input, inputIndex, Buffer, num, num2);
			inputIndex += num2;
			inputCount -= num2;
			Transform(Buffer, 0);
			num = 0;
		}
		int num3 = inputCount >> 6;
		while (--num3 >= 0)
		{
			Transform(input, inputIndex);
			inputIndex += 64;
			inputCount -= 64;
		}
		if (inputCount > 0)
		{
			Array.Copy(input, inputIndex, Buffer, num, inputCount);
		}
	}

	private byte[] Final()
	{
		int bytesInBuffer = BytesInBuffer;
		byte[] array = new byte[8];
		ulong num = Count << 3;
		Encode((uint)num, array, 0);
		Encode((uint)(num >> 32), array, 4);
		int inputCount = ((bytesInBuffer >= 56) ? (120 - bytesInBuffer) : (56 - bytesInBuffer));
		Update(Padding, 0, inputCount);
		Update(array, 0, 8);
		byte[] array2 = new byte[16];
		Encode(State0, array2, 0);
		Encode(State1, array2, 4);
		Encode(State2, array2, 8);
		Encode(State3, array2, 12);
		return array2;
	}

	private uint F(uint x, uint y, uint z)
	{
		return (x & y) | (~x & z);
	}

	private uint G(uint x, uint y, uint z)
	{
		return (x & z) | (y & ~z);
	}

	private uint H(uint x, uint y, uint z)
	{
		return x ^ y ^ z;
	}

	private uint I(uint x, uint y, uint z)
	{
		return y ^ (x | ~z);
	}

	private void FF(ref uint a, uint b, uint c, uint d, uint x, SS s, uint ac)
	{
		a = (a += F(b, c, d) + x + ac).RotateLeft((int)s) + b;
	}

	private void GG(ref uint a, uint b, uint c, uint d, uint x, SS s, uint ac)
	{
		a = (a += G(b, c, d) + x + ac).RotateLeft((int)s) + b;
	}

	private void HH(ref uint a, uint b, uint c, uint d, uint x, SS s, uint ac)
	{
		a = (a += H(b, c, d) + x + ac).RotateLeft((int)s) + b;
	}

	private void II(ref uint a, uint b, uint c, uint d, uint x, SS s, uint ac)
	{
		a = (a += I(b, c, d) + x + ac).RotateLeft((int)s) + b;
	}

	private void Transform(byte[] input, int index)
	{
		uint a = State0;
		uint a2 = State1;
		uint a3 = State2;
		uint a4 = State3;
		uint[] array = new uint[16];
		Decode(array, input, index, 16u);
		FF(ref a, a2, a3, a4, array[0], SS.S11, 3614090360u);
		FF(ref a4, a, a2, a3, array[1], SS.S12, 3905402710u);
		FF(ref a3, a4, a, a2, array[2], SS.S13, 606105819u);
		FF(ref a2, a3, a4, a, array[3], SS.S14, 3250441966u);
		FF(ref a, a2, a3, a4, array[4], SS.S11, 4118548399u);
		FF(ref a4, a, a2, a3, array[5], SS.S12, 1200080426u);
		FF(ref a3, a4, a, a2, array[6], SS.S13, 2821735955u);
		FF(ref a2, a3, a4, a, array[7], SS.S14, 4249261313u);
		FF(ref a, a2, a3, a4, array[8], SS.S11, 1770035416u);
		FF(ref a4, a, a2, a3, array[9], SS.S12, 2336552879u);
		FF(ref a3, a4, a, a2, array[10], SS.S13, 4294925233u);
		FF(ref a2, a3, a4, a, array[11], SS.S14, 2304563134u);
		FF(ref a, a2, a3, a4, array[12], SS.S11, 1804603682u);
		FF(ref a4, a, a2, a3, array[13], SS.S12, 4254626195u);
		FF(ref a3, a4, a, a2, array[14], SS.S13, 2792965006u);
		FF(ref a2, a3, a4, a, array[15], SS.S14, 1236535329u);
		GG(ref a, a2, a3, a4, array[1], SS.S21, 4129170786u);
		GG(ref a4, a, a2, a3, array[6], SS.S22, 3225465664u);
		GG(ref a3, a4, a, a2, array[11], SS.S23, 643717713u);
		GG(ref a2, a3, a4, a, array[0], SS.S24, 3921069994u);
		GG(ref a, a2, a3, a4, array[5], SS.S21, 3593408605u);
		GG(ref a4, a, a2, a3, array[10], SS.S22, 38016083u);
		GG(ref a3, a4, a, a2, array[15], SS.S23, 3634488961u);
		GG(ref a2, a3, a4, a, array[4], SS.S24, 3889429448u);
		GG(ref a, a2, a3, a4, array[9], SS.S21, 568446438u);
		GG(ref a4, a, a2, a3, array[14], SS.S22, 3275163606u);
		GG(ref a3, a4, a, a2, array[3], SS.S23, 4107603335u);
		GG(ref a2, a3, a4, a, array[8], SS.S24, 1163531501u);
		GG(ref a, a2, a3, a4, array[13], SS.S21, 2850285829u);
		GG(ref a4, a, a2, a3, array[2], SS.S22, 4243563512u);
		GG(ref a3, a4, a, a2, array[7], SS.S23, 1735328473u);
		GG(ref a2, a3, a4, a, array[12], SS.S24, 2368359562u);
		HH(ref a, a2, a3, a4, array[5], SS.S31, 4294588738u);
		HH(ref a4, a, a2, a3, array[8], SS.S32, 2272392833u);
		HH(ref a3, a4, a, a2, array[11], SS.S33, 1839030562u);
		HH(ref a2, a3, a4, a, array[14], SS.S34, 4259657740u);
		HH(ref a, a2, a3, a4, array[1], SS.S31, 2763975236u);
		HH(ref a4, a, a2, a3, array[4], SS.S32, 1272893353u);
		HH(ref a3, a4, a, a2, array[7], SS.S33, 4139469664u);
		HH(ref a2, a3, a4, a, array[10], SS.S34, 3200236656u);
		HH(ref a, a2, a3, a4, array[13], SS.S31, 681279174u);
		HH(ref a4, a, a2, a3, array[0], SS.S32, 3936430074u);
		HH(ref a3, a4, a, a2, array[3], SS.S33, 3572445317u);
		HH(ref a2, a3, a4, a, array[6], SS.S34, 76029189u);
		HH(ref a, a2, a3, a4, array[9], SS.S31, 3654602809u);
		HH(ref a4, a, a2, a3, array[12], SS.S32, 3873151461u);
		HH(ref a3, a4, a, a2, array[15], SS.S33, 530742520u);
		HH(ref a2, a3, a4, a, array[2], SS.S34, 3299628645u);
		II(ref a, a2, a3, a4, array[0], SS.S41, 4096336452u);
		II(ref a4, a, a2, a3, array[7], SS.S42, 1126891415u);
		II(ref a3, a4, a, a2, array[14], SS.S43, 2878612391u);
		II(ref a2, a3, a4, a, array[5], SS.S44, 4237533241u);
		II(ref a, a2, a3, a4, array[12], SS.S41, 1700485571u);
		II(ref a4, a, a2, a3, array[3], SS.S42, 2399980690u);
		II(ref a3, a4, a, a2, array[10], SS.S43, 4293915773u);
		II(ref a2, a3, a4, a, array[1], SS.S44, 2240044497u);
		II(ref a, a2, a3, a4, array[8], SS.S41, 1873313359u);
		II(ref a4, a, a2, a3, array[15], SS.S42, 4264355552u);
		II(ref a3, a4, a, a2, array[6], SS.S43, 2734768916u);
		II(ref a2, a3, a4, a, array[13], SS.S44, 1309151649u);
		II(ref a, a2, a3, a4, array[4], SS.S41, 4149444226u);
		II(ref a4, a, a2, a3, array[11], SS.S42, 3174756917u);
		II(ref a3, a4, a, a2, array[2], SS.S43, 718787259u);
		II(ref a2, a3, a4, a, array[9], SS.S44, 3951481745u);
		State0 += a;
		State1 += a2;
		State2 += a3;
		State3 += a4;
	}

	private void Decode(uint[] output, byte[] input, int inputIndex, uint count)
	{
		int num = -1;
		while (++num < count)
		{
			output[num] = (uint)(input[inputIndex++] | (input[inputIndex++] << 8) | (input[inputIndex++] << 16) | (input[inputIndex++] << 24));
		}
	}

	private void Encode(uint input, byte[] output, int outputIndex)
	{
		output[outputIndex++] = (byte)(input & 0xFF);
		output[outputIndex++] = (byte)((input >> 8) & 0xFF);
		output[outputIndex++] = (byte)((input >> 16) & 0xFF);
		output[outputIndex++] = (byte)((input >> 24) & 0xFF);
	}
}
