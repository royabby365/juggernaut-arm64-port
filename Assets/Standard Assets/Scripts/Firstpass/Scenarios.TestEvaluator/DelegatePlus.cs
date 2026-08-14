using System;

namespace Scenarios.TestEvaluator
{

public struct DelegatePlus
{
	public readonly int arity;

	public readonly object[] defaults;

	public readonly Delegate action;

	public readonly bool IsCondition;

	public DelegatePlus(Delegate dlg, params object[] defaults)
	{
		IsCondition = dlg.Method.ReturnType == typeof(bool);
		arity = dlg.Method.GetParameters().Length;
		if (defaults == null)
		{
			defaults = new object[0];
		}
		if (defaults.Length > 0 && arity != defaults.Length)
		{
			ErrorReporter.InEval("default params count must be: {0}, was: {1}", arity, defaults.Length);
		}
		action = dlg;
		this.defaults = defaults;
	}
}
}
