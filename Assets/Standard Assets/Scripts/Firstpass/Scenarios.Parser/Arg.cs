namespace Scenarios.Parser
{

public abstract class Arg
{
	private readonly object _val;

	public object Val => _val;

	protected Arg(object o)
	{
		_val = o;
	}
}
}
