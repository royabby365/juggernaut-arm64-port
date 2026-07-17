using System;
using System.Collections.Generic;
using Scenarios.Parser;

namespace Scenarios.Evaluator.Scenarios.CoroutineEvaluator;

public class CoroutineEvaluator : IGlobals
{
	private static readonly DelegatePlus _noop;

	private readonly Dictionary<string, CoroutineEvaluator> _childs = new Dictionary<string, CoroutineEvaluator>();

	private readonly Dictionary<string, DelegatePlus> _commands = new Dictionary<string, DelegatePlus>();

	private readonly Dictionary<string, Arg> _globals;

	private readonly string _id;

	private readonly CoroutineEvaluator _parent;

	private readonly Dictionary<string, List<Statement>> _scenarios = new Dictionary<string, List<Statement>>();

	public string Name => _id;

	public CoroutineEvaluator(string id)
		: this(id, null)
	{
	}

	public CoroutineEvaluator(string id, CoroutineEvaluator parent)
	{
		_id = id;
		_parent = parent;
		_globals = ((_parent == null) ? new Dictionary<string, Arg>() : _parent._globals);
	}

	static CoroutineEvaluator()
	{
		Action action = delegate
		{
		};
		Delegate dlg = Delegate.CreateDelegate(typeof(Action), null, action.Method);
		_noop = new DelegatePlus(dlg);
	}

	public Arg LookupVar(string name)
	{
		return (!_globals.ContainsKey(name)) ? null : _globals[name];
	}

	public void AddGlobal(string id, Arg arg)
	{
		if (_globals.ContainsKey(id))
		{
			Arg arg2 = _globals[id];
			ErrorReporter.Warning("var {0} already set to {1}, new value: {2}", id, arg2, arg);
		}
		_globals[id] = arg;
	}

	public CoroutineEvaluator AddCommand(string name, Delegate command, params object[] defaults)
	{
		DelegatePlus value = new DelegatePlus(command, defaults);
		if (_commands.ContainsKey(name))
		{
			ErrorReporter.Warning("command {0} redefined", name);
		}
		_commands[name] = value;
		return this;
	}

	public CoroutineEvaluator AddChild(string id, CoroutineEvaluator child)
	{
		if (_childs.ContainsKey(id))
		{
			ErrorReporter.Warning("child {0} was already set", id);
		}
		_childs[id] = child;
		return this;
	}

	public CoroutineEvaluator AddScript(Script script)
	{
		foreach (Scenario scenario in script.Scenarios)
		{
			string id = scenario.Id;
			List<Statement> statements = scenario.Statements;
			if (_scenarios.ContainsKey(id))
			{
				ErrorReporter.Warning("scenario {0} redefined", id);
			}
			_scenarios[id] = statements;
		}
		return this;
	}

	private DelegatePlus LookUpCommand(string name)
	{
		if (_commands.ContainsKey(name))
		{
			return _commands[name];
		}
		if (_parent != null)
		{
			return _parent.LookUpCommand(name);
		}
		ErrorReporter.Warning("command lookup falure: {0}", name);
		return _noop;
	}

	private List<Statement> LookUpScenario(string name)
	{
		if (_scenarios.ContainsKey(name))
		{
			return _scenarios[name];
		}
		if (_parent != null)
		{
			return _parent.LookUpScenario(name);
		}
		ErrorReporter.Warning("scenario lookup falure: {0}", name);
		return null;
	}

	public void PlayScenario(string name)
	{
		EvalScenario(name);
	}

	private void EvalScenario(string name)
	{
		List<Statement> list = LookUpScenario(name);
		if (list == null)
		{
			return;
		}
		foreach (Statement item in list)
		{
			EvalStatement(item);
		}
	}

	private void EvalStatement(Statement statement)
	{
		if (!(statement is Command command))
		{
			EvalCondition(statement as Condition);
		}
		else
		{
			EvalCommand(command);
		}
	}

	private void EvalCondition(Condition condition)
	{
		if (_commands.ContainsKey(condition.id))
		{
			DelegatePlus delegatePlus = LookUpCommand(condition.id);
			if (!delegatePlus.Equals(_noop))
			{
				object[] args = condition.GetArgs(delegatePlus.arity, delegatePlus.defaults);
				delegatePlus.action.DynamicInvoke(args);
			}
		}
		else
		{
			ErrorReporter.Warning("there is no condition '{0}')", condition.id);
		}
		foreach (Statement statement in condition.statements)
		{
			EvalStatement(statement);
		}
	}

	private void EvalCommand(Command command)
	{
		if (_commands.ContainsKey(command.id))
		{
			DelegatePlus delegatePlus = LookUpCommand(command.id);
			if (!delegatePlus.Equals(_noop))
			{
				object[] args = command.GetArgs(delegatePlus.arity, delegatePlus.defaults);
				delegatePlus.action.DynamicInvoke(args);
			}
		}
		else
		{
			ErrorReporter.Warning("WARNING: there is no command '{0}')", command.id);
		}
	}
}
