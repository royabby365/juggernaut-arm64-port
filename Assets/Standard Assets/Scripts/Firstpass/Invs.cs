using System;
using System.Text;

internal static class Invs
{
	public static void Inv(bool condition, params object[] args)
	{
		if (condition)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("Invariant failed [" + Utils.Version + "]: ");
		if (args.Length > 0)
		{
			stringBuilder.Append(args[0].ToString());
			for (int i = 1; i < args.Length; i++)
			{
				stringBuilder.Append(" ");
				stringBuilder.Append((args[i] == null) ? "NULL" : args[i].ToString());
			}
		}
		Utils.Log("EXCEPTION", stringBuilder.ToString());
		throw new Exception(stringBuilder.ToString());
	}
}
