using UnityEngine;

public class UnityConsoleEchoListener : IDebugEchoListner
{
	private const string DEBUG_CONSOLE_MARKER = "@-> ";

	public void Echo(DebugCommandMessage messageType, string text)
	{
		switch (messageType)
		{
		case DebugCommandMessage.Standard:
			Debug.Log("@-> " + text);
			break;
		case DebugCommandMessage.Error:
			Debug.LogError("@-> " + text);
			break;
		case DebugCommandMessage.Warning:
			Debug.LogWarning("@-> " + text);
			break;
		}
	}
}
