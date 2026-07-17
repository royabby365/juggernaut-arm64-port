namespace Scenarios.TestEvaluator;

internal static class EvaluatorExt
{
	internal static void AddCommand(this TestEvaluator ev, string name, ActionD action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T>(this TestEvaluator ev, string name, ActionD<T> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2>(this TestEvaluator ev, string name, ActionD<T1, T2> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3>(this TestEvaluator ev, string name, ActionD<T1, T2, T3> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4>(this TestEvaluator ev, string name, ActionD<T1, T2, T3, T4> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, T5>(this TestEvaluator ev, string name, ActionD<T1, T2, T3, T4, T5> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, T5, T6>(this TestEvaluator ev, string name, ActionD<T1, T2, T3, T4, T5, T6> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, T5, T6, T7>(this TestEvaluator ev, string name, ActionD<T1, T2, T3, T4, T5, T6, T7> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, T5, T6, T7, T8>(this TestEvaluator ev, string name, ActionD<T1, T2, T3, T4, T5, T6, T7, T8> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<R>(this TestEvaluator ev, string name, FuncD<R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T, R>(this TestEvaluator ev, string name, FuncD<T, R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, R>(this TestEvaluator ev, string name, FuncD<T1, T2, R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, R>(this TestEvaluator ev, string name, FuncD<T1, T2, T3, R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, R>(this TestEvaluator ev, string name, FuncD<T1, T2, T3, T4, R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, T5, R>(this TestEvaluator ev, string name, FuncD<T1, T2, T3, T4, T5, R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, T5, T6, R>(this TestEvaluator ev, string name, FuncD<T1, T2, T3, T4, T5, T6, R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, T5, T6, T7, R>(this TestEvaluator ev, string name, FuncD<T1, T2, T3, T4, T5, T6, T7, R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}

	internal static void AddCommand<T1, T2, T3, T4, T5, T6, T7, T8, R>(this TestEvaluator ev, string name, FuncD<T1, T2, T3, T4, T5, T6, T7, T8, R> action, params object[] args)
	{
		ev.AddCommand(name, action, args);
	}
}
