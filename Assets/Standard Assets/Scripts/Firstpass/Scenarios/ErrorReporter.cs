using UnityEngine;

namespace Scenarios
{

public static class ErrorReporter
{
	private static void Error(string where, string what)
	{
		if (Globals.IsDebugBuild)
		{
			string message = $"ERROR: in {where} {what}";
			Debug.Log(message);
		}
	}

	public static void InParser(string fmt, params object[] args)
	{
		Error("[PARSER]", string.Format(fmt, args));
	}

	public static void InScanner(string fmt, params object[] args)
	{
		Error("[SCANNER]", string.Format(fmt, args));
	}

	public static void InEval(string fmt, params object[] args)
	{
		Error("[EVAL]", string.Format(fmt, args));
	}

	public static void Warning(string fmt, params object[] args)
	{
	}

	public static void Info(string fmt, params object[] args)
	{
	}

	public static void Info(object o)
	{
	}
}
}
