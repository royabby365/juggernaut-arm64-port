namespace Scenarios.Parser;

public class ArgString : Arg
{
	public ArgString(string name)
		: base(name)
	{
	}

	public override string ToString()
	{
		return $"{base.Val}:String";
	}
}
