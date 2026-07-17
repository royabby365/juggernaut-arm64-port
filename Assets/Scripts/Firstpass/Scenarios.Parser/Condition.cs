using System.Collections.Generic;

namespace Scenarios.Parser;

public class Condition : Statement
{
	public readonly List<Statement> statements;

	public Condition(string id, IEnumerable<Arg> args, IEnumerable<Statement> stmts)
		: base(id, new List<Arg>(args))
	{
		statements = new List<Statement>(stmts);
	}
}
