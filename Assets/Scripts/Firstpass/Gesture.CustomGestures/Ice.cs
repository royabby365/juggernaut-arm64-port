namespace Gesture.CustomGestures;

public class Ice : CustomGesture
{
	public Ice()
	{
		base.Name = "Ice";
	}

	public override bool IsMatch(Statistics stats)
	{
		if (stats.Directions.Count != 3)
		{
			return false;
		}
		Direction dir = stats.Directions[0];
		Direction dir2 = stats.Directions[stats.Directions.Count - 1];
		return base.IsMatch(stats) && stats.Proximity < 0.3f && !CustomGesture.IsVerticalAndOpposite(dir, dir2);
	}
}
