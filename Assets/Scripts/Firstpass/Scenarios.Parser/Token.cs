namespace Scenarios.Parser
{

public struct Token
{
	public readonly float Fval;

	public readonly int Ival;

	public readonly Kind Kind;

	public readonly Lineinfo Pos;

	public readonly string Sval;

	private Token(Kind k, Lineinfo pos)
	{
		Kind = k;
		Pos = pos;
		Fval = 0f;
		Ival = 0;
		Sval = null;
	}

	private Token(int val, Lineinfo pos)
	{
		Kind = Kind.Int;
		Fval = 0f;
		Ival = val;
		Pos = pos;
		Sval = null;
	}

	private Token(float val, Lineinfo pos)
	{
		Kind = Kind.Float;
		Fval = val;
		Pos = pos;
		Ival = 0;
		Sval = null;
	}

	private Token(Kind k, string val, Lineinfo pos)
	{
		Kind = k;
		Pos = pos;
		Fval = 0f;
		Ival = 0;
		Sval = val;
	}

	public static Token FromKind(Kind k, Lineinfo pos)
	{
		return new Token(k, pos);
	}

	public static Token Int(int val, Lineinfo pos)
	{
		return new Token(val, pos);
	}

	public static Token Float(float val, Lineinfo pos)
	{
		return new Token(val, pos);
	}

	public static Token Id(string id, Lineinfo pos)
	{
		return new Token(Kind.Id, id, pos);
	}

	public static Token HashIdOpen(string id, Lineinfo pos)
	{
		return new Token(Kind.HashIdOpen, id, pos);
	}

	public static Token HashIdClose(string id, Lineinfo pos)
	{
		return new Token(Kind.HashIdClose, id, pos);
	}

	public override string ToString()
	{
		return Kind switch
		{
			Kind.Int => $"INT({Ival}) at {Pos}", 
			Kind.Float => $"FLOAT({Fval}) at {Pos}", 
			Kind.Id => $"ID({Sval}) at {Pos}", 
			Kind.HashIdOpen => $"#{Sval} at {Pos}", 
			Kind.HashIdClose => $"{Sval}# at {Pos}", 
			_ => $"{Kind} at {Pos}", 
		};
	}
}
}
