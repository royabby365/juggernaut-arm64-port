using Scenarios.Parser;

namespace Scenarios;

public interface IGlobals
{
	Arg LookupVar(string id);

	void AddGlobal(string id, Arg arg);
}
