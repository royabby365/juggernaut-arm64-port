using System;
using System.Collections.Generic;

public interface IDebugCommandHost : IDebugCommandExecutioner, IDebugEchoListner
{
	void RegisterCommand(string command, string description, Action<IDebugCommandHost, string, IList<string>> callback);

	void UnregisterCommand(string command);

	void Echo(string text);

	new void Echo(DebugCommandMessage messageType, string text);

	void EchoWarning(string text);

	void EchoError(string text);

	void RegisterEchoListner(IDebugEchoListner listner);

	void UnregisterEchoListner(IDebugEchoListner listner);

	void PushExecutioner(IDebugCommandExecutioner executioner);

	void PopExecutioner();
}
