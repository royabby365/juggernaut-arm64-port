namespace Scenarios.Parser;

public struct Lineinfo
{
	public readonly int Line;

	public Lineinfo(int line)
	{
		Line = line;
	}

	public override string ToString()
	{
		return $"line: {Line + 1}";
	}
}
