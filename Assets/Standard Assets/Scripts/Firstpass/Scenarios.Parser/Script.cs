using System.Collections.Generic;

namespace Scenarios.Parser
{

public class Script
{
	private readonly List<Scenario> _scenarios = new List<Scenario>();

	public List<Scenario> Scenarios => _scenarios;

	public void AddScenario(Scenario scenario)
	{
		_scenarios.Add(scenario);
	}

	public void DumpScenarios()
	{
		foreach (Scenario scenario in _scenarios)
		{
			ErrorReporter.Info(scenario.ToString());
		}
	}
}
}
