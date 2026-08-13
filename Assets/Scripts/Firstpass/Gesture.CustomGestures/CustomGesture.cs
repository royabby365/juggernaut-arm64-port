namespace Gesture.CustomGestures
{

public abstract class CustomGesture
{
	private const float MinExtent = 100f;

	public const float ReductionTolerance = 0.2f;

	public string Name { get; protected set; }

	public virtual bool IsMatch(Statistics stats)
	{
		return stats.Extent >= 100f && stats.Total >= 1;
	}

	public static bool UpDirection(Direction dir)
	{
		if (dir == Direction.N || dir == Direction.NE || dir == Direction.NW)
		{
			return true;
		}
		return false;
	}

	public static bool DownDirection(Direction dir)
	{
		switch (dir)
		{
		case Direction.SE:
		case Direction.S:
		case Direction.SW:
			return true;
		default:
			return false;
		}
	}

	protected static bool IsVertical(Direction dir)
	{
		return UpDirection(dir) || DownDirection(dir);
	}

	protected static bool IsVerticalAndOpposite(Direction dir1, Direction dir2)
	{
		return (UpDirection(dir1) && DownDirection(dir2)) || (DownDirection(dir1) && UpDirection(dir2));
	}
}
}
