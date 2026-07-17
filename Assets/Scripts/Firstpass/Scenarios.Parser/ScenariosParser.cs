using System.Collections.Generic;

namespace Scenarios.Parser;

public class ScenariosParser
{
	private readonly IGlobals _globals;

	public ScenariosParser(IGlobals globals)
	{
		_globals = globals;
	}

	public void DumpTokenStream(IEnumerator<Token> ts)
	{
		while (ts.MoveNext())
		{
			ErrorReporter.Info(ts.Current);
		}
	}

	public Script Parse(IEnumerator<Token> ts)
	{
		Script script = new Script();
		ts.MoveNext();
		while (ts.Current.Kind != Kind.Eof)
		{
			switch (ts.Current.Kind)
			{
			case Kind.Dollar:
				ts.MoveNext();
				script.AddScenario(ParseScenario(ts));
				break;
			case Kind.Var:
				ts.MoveNext();
				ParseVar(ts);
				break;
			default:
				ErrorReporter.InParser("error at {0}", ts.Current);
				break;
			}
		}
		return script;
	}

	private Scenario ParseScenario(IEnumerator<Token> ts)
	{
		Expect(ts, Kind.Id);
		string sval = ts.Current.Sval;
		ts.MoveNext();
		Expect(ts, Kind.BraceOpen);
		ts.MoveNext();
		List<Statement> statements = ParseStatementsList(ts);
		Expect(ts, Kind.BraceClose);
		ts.MoveNext();
		return new Scenario(sval, statements);
	}

	private void ParseVar(IEnumerator<Token> ts)
	{
		Expect(ts, Kind.Id);
		string sval = ts.Current.Sval;
		ts.MoveNext();
		Arg arg = ParseArg(ts);
		Expect(ts, Kind.Semi);
		ts.MoveNext();
		_globals.AddGlobal(sval, arg);
	}

	private Command ParseCommand(IEnumerator<Token> ts)
	{
		Expect(ts, Kind.Id);
		string sval = ts.Current.Sval;
		ts.MoveNext();
		Expect(ts, Kind.ParenOpen);
		ts.MoveNext();
		IEnumerable<Arg> args = ParseArgs(ts);
		Expect(ts, Kind.ParenClose);
		ts.MoveNext();
		Expect(ts, Kind.Semi);
		ts.MoveNext();
		return new Command(sval, args);
	}

	private Condition ParseCondition(IEnumerator<Token> ts)
	{
		Expect(ts, Kind.HashIdOpen);
		string sval = ts.Current.Sval;
		ts.MoveNext();
		Expect(ts, Kind.ParenOpen);
		ts.MoveNext();
		IEnumerable<Arg> args = ParseArgs(ts);
		Expect(ts, Kind.ParenClose);
		ts.MoveNext();
		Expect(ts, Kind.Semi);
		ts.MoveNext();
		IList<Statement> stmts = ParseStatementsList(ts);
		Expect(ts, Kind.HashIdClose);
		string sval2 = ts.Current.Sval;
		ts.MoveNext();
		Expect(ts, Kind.Semi);
		ts.MoveNext();
		if (sval != sval2)
		{
			ErrorReporter.InParser("IDs '{0}' and '{1}' must be equal at {2}", sval, sval2, ts.Current);
		}
		return new Condition(sval, args, stmts);
	}

	private void AddLookupArgToList(ICollection<Arg> args, Arg arg)
	{
		Arg arg2 = arg;
		if (arg2 is ArgString argString)
		{
			Arg arg3 = _globals.LookupVar((string)argString.Val);
			if (arg3 != null)
			{
				arg2 = arg3;
			}
		}
		args.Add(arg2);
	}

	private IEnumerable<Arg> ParseArgs(IEnumerator<Token> ts)
	{
		List<Arg> list = new List<Arg>();
		Arg arg = ParseArg(ts);
		if (arg == null)
		{
			return list;
		}
		AddLookupArgToList(list, arg);
		while (ts.Current.Kind == Kind.Comma)
		{
			ts.MoveNext();
			arg = ParseArg(ts);
			if (arg != null)
			{
				AddLookupArgToList(list, arg);
			}
		}
		return list;
	}

	private static Arg ParseArg(IEnumerator<Token> ts)
	{
		switch (ts.Current.Kind)
		{
		case Kind.Minus:
			ts.MoveNext();
			return ParseNumArg(ts, -1);
		case Kind.Plus:
			ts.MoveNext();
			return ParseNumArg(ts, 1);
		case Kind.Int:
		case Kind.Float:
			return ParseNumArg(ts, 1);
		case Kind.Id:
		{
			Arg result = new ArgString(ts.Current.Sval);
			ts.MoveNext();
			return result;
		}
		case Kind.ParenClose:
			return null;
		default:
			ErrorReporter.InParser("incorrect Arg: {0}", ts.Current);
			return null;
		}
	}

	private static Arg ParseNumArg(IEnumerator<Token> ts, int sign)
	{
		Arg result = null;
		switch (ts.Current.Kind)
		{
		case Kind.Float:
			result = new ArgFloat((float)sign * ts.Current.Fval);
			ts.MoveNext();
			return result;
		case Kind.Int:
			result = new ArgInt(sign * ts.Current.Ival);
			ts.MoveNext();
			return result;
		default:
			ErrorReporter.InParser("incorrect Arg: {0}", ts.Current);
			return result;
		}
	}

	private List<Statement> ParseStatementsList(IEnumerator<Token> ts)
	{
		List<Statement> list = new List<Statement>();
		while (ts.Current.Kind == Kind.Id || ts.Current.Kind == Kind.HashIdOpen)
		{
			switch (ts.Current.Kind)
			{
			case Kind.Id:
			{
				Command item2 = ParseCommand(ts);
				list.Add(item2);
				break;
			}
			case Kind.HashIdOpen:
			{
				Condition item = ParseCondition(ts);
				list.Add(item);
				break;
			}
			}
		}
		return list;
	}

	private static void Expect(IEnumerator<Token> ts, Kind what)
	{
		if (ts.Current.Kind != what)
		{
			ErrorReporter.InParser("'{0}' expected, got {1}", what, ts.Current);
		}
	}
}
