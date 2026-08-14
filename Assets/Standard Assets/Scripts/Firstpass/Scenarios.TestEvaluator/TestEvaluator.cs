using System;
using System.Collections.Generic;
using Scenarios.Parser;

namespace Scenarios.TestEvaluator
{

public class TestEvaluator : IGlobals
{
	public class Context
	{
		protected Context _prevContext;

		public string ContextName;

		public readonly string ScenarioName;

		public float WaitTime;

		public TestEvaluator Evaluator;

		public ActionD OnFinish;

		public int OnFinishCounter;

		public Context RootContext
		{
			get
			{
				Context prevContext = _prevContext;
				while (prevContext._prevContext != null)
				{
					prevContext = prevContext._prevContext;
				}
				return prevContext;
			}
		}

		public Context TopContext
		{
			get
			{
				Context context = this;
				while (context._prevContext != null)
				{
					context = context._prevContext;
				}
				return context;
			}
		}

		public virtual string Stacktrace => ScenarioName + ((_prevContext == null) ? string.Empty : (":" + _prevContext.Stacktrace));

		public Context(string name, string scenario)
		{
			ContextName = name;
			ScenarioName = scenario;
		}
	}

	private static readonly DelegatePlus NOOP;

	private readonly Dictionary<string, TestEvaluator> _childs = new Dictionary<string, TestEvaluator>();

	private readonly Dictionary<string, DelegatePlus> _commands = new Dictionary<string, DelegatePlus>();

	private readonly Dictionary<string, Arg> _globals;

	private readonly string _id;

	private readonly TestEvaluator _parent;

	private readonly Dictionary<string, List<Statement>> _scenarios = new Dictionary<string, List<Statement>>();

	public string Name => _id;

	public TestEvaluator(string id)
		: this(id, null)
	{
	}

	public TestEvaluator(string id, TestEvaluator parent)
	{
		_id = id;
		_parent = parent;
		_globals = ((_parent == null) ? new Dictionary<string, Arg>() : _parent._globals);
	}

	static TestEvaluator()
	{
		Action action = delegate
		{
		};
		Delegate dlg = Delegate.CreateDelegate(typeof(Action), null, action.Method);
		NOOP = new DelegatePlus(dlg);
	}

	public void VisitEachFX(string scenarioName, TestEvaluator other, ActionD<string> onFx)
	{
		Visit(scenarioName, delegate(Statement statement)
		{
			if (statement.id == "fx")
			{
				object arg = statement.GetArg(1, null);
				if (arg != null && arg is string)
				{
					onFx(arg as string);
				}
			}
			else
			{
				if (statement.id == "scenario")
				{
					return statement.GetArg(0, null) as string;
				}
				if (statement.id == "enemyscenario" && other != null)
				{
					other.VisitEachFX(statement.GetArg(0, null) as string, null, onFx);
				}
			}
			return (string)null;
		});
	}

	public void Visit(string scenarioName, FuncD<Statement, string> action)
	{
		List<Statement> value = null;
		if (_scenarios.TryGetValue(scenarioName, out value))
		{
			foreach (Statement item in value)
			{
				VisitStatement(item, action);
			}
			return;
		}
		if (_parent != null)
		{
			_parent.Visit(scenarioName, action);
		}
	}

	private void VisitStatement(Statement c, FuncD<Statement, string> action)
	{
		string text = action(c);
		if (text != null)
		{
			Visit(text, action);
		}
		if (!(c is Condition condition))
		{
			return;
		}
		foreach (Statement statement in condition.statements)
		{
			VisitStatement(statement, action);
		}
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

	public TestEvaluator AddCommand(string name, Delegate command, params object[] defaults)
	{
		DelegatePlus value = new DelegatePlus(command, defaults);
		if (_commands.ContainsKey(name))
		{
			ErrorReporter.Warning("command {0} redefined", name);
		}
		_commands[name] = value;
		return this;
	}

	public TestEvaluator AddChild(string id, TestEvaluator child)
	{
		if (_childs.ContainsKey(id))
		{
			ErrorReporter.Warning("child {0} was already set", id);
		}
		_childs[id] = child;
		return this;
	}

	public TestEvaluator AddScript(Script script)
	{
		foreach (Scenario scenario in script.Scenarios)
		{
			string id = scenario.Id;
			List<Statement> statements = scenario.Statements;
			if (_scenarios.ContainsKey(id))
			{
				ErrorReporter.Warning("scenario {0} redefined)", id);
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
		return NOOP;
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
		ErrorReporter.Warning("scenario lookup falure: {0} {1}", name, _parent != null);
		return null;
	}

	public bool HasScenario(string name)
	{
		return LookUpScenario(name) != null;
	}

	private void PlayScenario(string name, Context context)
	{
		EvalScenario(name, context, null);
	}

	public void PlayScenario(string name, Context context, ActionD onFinish)
	{
		EvalScenario(name, context, onFinish);
	}

	private void EvalScenario(string name, Context context, ActionD onFinish)
	{
		List<Statement> list = LookUpScenario(name);
		if (list != null)
		{
			context.OnFinish = onFinish;
			Eval(list, 0, context);
		}
	}

	private void Eval(List<Statement> statements, int starti, Context context)
	{
		try
		{
			while (starti < statements.Count)
			{
				Statement statement = statements[starti];
				float waitTime = context.WaitTime;
				if (waitTime > 0f)
				{
					context.WaitTime = 0f;
					context.TopContext.OnFinishCounter++;
					SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(context.ContextName, waitTime, delegate
					{
						Eval(statements, starti, context);
						Context topContext = context.TopContext;
						topContext.OnFinishCounter--;
						if (topContext.OnFinishCounter == 0 && topContext.OnFinish != null)
						{
							ActionD onFinish2 = topContext.OnFinish;
							topContext.OnFinish = null;
							onFinish2();
						}
					});
					break;
				}
				EvalStatement(statement, context);
				starti++;
			}
			if (context.TopContext.OnFinishCounter == 0 && context.OnFinish != null)
			{
				ActionD onFinish = context.OnFinish;
				context.OnFinish = null;
				onFinish();
			}
		}
		catch (Exception ex_)
		{
			Utils.HandleError(ex_, "EvalScenario failed in", context.Stacktrace);
		}
	}

	private void EvalStatement(Statement statement, Context context)
	{
		if (!(statement is Command command))
		{
			EvalCondition(statement as Condition, context);
		}
		else
		{
			EvalCommand(command, context);
		}
	}

	private void EvalCondition(Condition condition, Context context)
	{
		bool flag = true;
		if (_commands.ContainsKey(condition.id))
		{
			DelegatePlus delegatePlus = LookUpCommand(condition.id);
			if (!delegatePlus.Equals(NOOP))
			{
				object[] args = condition.GetArgs(delegatePlus.action, context, delegatePlus.arity, delegatePlus.defaults);
				flag = (bool)delegatePlus.action.DynamicInvoke(args);
			}
		}
		else
		{
			ErrorReporter.Warning("there is no condition '{0}'", condition.id);
		}
		if (flag)
		{
			Eval(condition.statements, 0, context);
		}
	}

	private void EvalCommand(Command command, Context context)
	{
		if (_commands.ContainsKey(command.id))
		{
			DelegatePlus delegatePlus = LookUpCommand(command.id);
			if (delegatePlus.Equals(NOOP))
			{
				return;
			}
			object[] args = command.GetArgs(delegatePlus.action, context, delegatePlus.arity, delegatePlus.defaults);
			object obj = null;
			try
			{
				obj = delegatePlus.action.DynamicInvoke(args);
			}
			catch (Exception ex)
			{
				Utils.Log($"EvalCommand {command.id} failed. Args {args.ConcatAsStrings()}.");
				throw new Exception("EvalCommand {0} failed. Args {1}. Ex {2}".Fmt(command.id, args.ConcatAsStrings(), ex.Message), ex);
			}
			if (obj is Context context2)
			{
				if (context2.Evaluator == null)
				{
					PlayScenario(context2.ScenarioName, context2);
				}
				else
				{
					context2.Evaluator.PlayScenario(context2.ScenarioName, context2);
				}
			}
		}
		else
		{
			ErrorReporter.Warning("there is no command '{0}'", command.id);
		}
	}

	internal void StopAll(string name)
	{
		SingletonT<TimeEventsManager>.I.StopAllWithName(name);
	}
}
}
