using System;

namespace Scenarios.Evaluator.Scenarios.CoroutineEvaluator
{

public struct DelegatePlus
{
	public readonly Delegate action;

	public readonly int arity;

	public readonly object[] defaults;

	public DelegatePlus(Delegate dlg, params object[] defaults)
	{
		arity = dlg.Method.GetParameters().Length;
		if (defaults.Length > 0 && arity != defaults.Length)
		{
			ErrorReporter.InEval("default params count must be: {0}, was: {1}", arity, defaults.Length);
		}
		action = dlg;
		this.defaults = defaults;
	}
}
}
