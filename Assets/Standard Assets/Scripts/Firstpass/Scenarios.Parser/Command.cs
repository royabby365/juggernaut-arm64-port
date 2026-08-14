using System.Collections.Generic;

namespace Scenarios.Parser
{

public class Command : Statement
{
	public Command(string id, IEnumerable<Arg> args)
		: base(id, new List<Arg>(args))
	{
	}
}
}
