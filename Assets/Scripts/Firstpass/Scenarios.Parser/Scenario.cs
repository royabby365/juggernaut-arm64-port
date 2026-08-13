using System.Collections.Generic;

namespace Scenarios.Parser
{

public class Scenario
{
	public readonly string Id;

	public readonly List<Statement> Statements;

	public Scenario(string id, List<Statement> statements)
	{
		Id = id;
		Statements = statements;
	}

	public override string ToString()
	{
		return $"Scenario[name:{Id}, statements:{Statements.Count}]";
	}
}
}
