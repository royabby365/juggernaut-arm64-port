using System;
using System.Collections.Generic;
using System.Reflection;

namespace Scenarios.Parser
{

public abstract class Statement
{
	public readonly string id;

	private object[] _args;

	protected Statement(string id, IList<Arg> args)
	{
		this.id = id;
		_args = new object[args.Count];
		for (int i = 0; i < _args.Length; i++)
		{
			_args[i] = args[i].Val;
		}
	}

	public object GetArg(int i, object ifNo)
	{
		if (i < _args.Length)
		{
			return _args[i];
		}
		return ifNo;
	}

	public object[] GetArgs(Delegate method, object context, int arity, object[] defaults)
	{
		object[] array = new object[arity];
		Invs.Inv(arity > 0, "arity > 0", id);
		array[0] = context;
		if (defaults.Length > 0 && defaults.Length > _args.Length)
		{
			int num = _args.Length;
			int num2 = defaults.Length;
			for (int i = 0; i < num; i++)
			{
				array[i + 1] = _args[i];
			}
			for (int j = num + 1; j < num2; j++)
			{
				array[j] = defaults[j];
			}
		}
		else
		{
			for (int k = 0; k < _args.Length; k++)
			{
				array[k + 1] = _args[k];
			}
		}
		if (array.Length != arity)
		{
			ErrorReporter.Warning("Incorrect '{2}' args count: {0} must be: {1}", array.Length, arity, id);
		}
		ParameterInfo[] parameters = method.Method.GetParameters();
		for (int l = 0; l < parameters.Length; l++)
		{
			if (array[l] != null && array[l].GetType() == typeof(int) && parameters[l].ParameterType == typeof(string))
			{
				array[l] = array[l].ToString();
			}
		}
		return array;
	}

	public object[] GetArgs(int arity, object[] defaults)
	{
		object[] array = new object[arity];
		Invs.Inv(arity > 0, "arity > 0", id);
		if (defaults.Length > 0 && defaults.Length > _args.Length)
		{
			int num = _args.Length;
			int num2 = defaults.Length;
			for (int i = 0; i < num; i++)
			{
				array[i] = _args[i];
			}
			for (int j = num + 1; j < num2; j++)
			{
				array[j] = defaults[j];
			}
		}
		else
		{
			for (int k = 0; k < _args.Length; k++)
			{
				array[k] = _args[k];
			}
		}
		if (array.Length != arity)
		{
			ErrorReporter.Warning("Incorrect '{2}' args count: {0} must be: {1}", array.Length, arity, id);
		}
		return array;
	}

	public override string ToString()
	{
		return $"Statemant: {id}: {_args.ConcatAsStrings()}";
	}
}
}
