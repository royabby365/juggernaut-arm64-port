namespace Scenarios.Parser;

public class ArgInt : Arg
{
	public ArgInt(int val)
		: base(val)
	{
	}

	public override string ToString()
	{
		return $"{base.Val}:Int";
	}
}
