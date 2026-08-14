namespace Scenarios.Parser
{

public class ArgFloat : Arg
{
	public ArgFloat(float val)
		: base(val)
	{
	}

	public override string ToString()
	{
		return $"{base.Val}:Float";
	}
}
}
