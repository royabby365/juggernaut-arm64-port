namespace GregoryAdam.Base.ExtensionMethods
{

public static class BitRotate
{
	public static uint RotateLeft(this uint x, int nBits)
	{
		nBits &= 0x1F;
		return (x << nBits) | (x >> 32 - nBits);
	}

	public static int RotateLeft(this int x, int nBits)
	{
		return (int)((uint)x).RotateLeft(nBits);
	}

	public static ulong RotateLeft(this ulong x, int nBits)
	{
		nBits &= 0x3F;
		return (x << nBits) | (x >> 64 - nBits);
	}

	public static long RotateLeft(this long x, int nBits)
	{
		return (long)((ulong)x).RotateLeft(nBits);
	}

	public static uint RotateRight(this uint x, int nBits)
	{
		nBits &= 0x1F;
		return (x >> nBits) | (x << 32 - nBits);
	}

	public static int RotateRight(this int x, int nBits)
	{
		return (int)((uint)x).RotateLeft(nBits);
	}

	public static ulong RotateRight(this ulong x, int nBits)
	{
		nBits &= 0x3F;
		return (x >> nBits) | (x << 64 - nBits);
	}

	public static long RotateRight(this long x, int nBits)
	{
		return (long)((ulong)x).RotateLeft(nBits);
	}
}
}
