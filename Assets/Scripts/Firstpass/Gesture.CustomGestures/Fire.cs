namespace Gesture.CustomGestures;

public class Fire : CustomGesture
{
	public Fire()
	{
		base.Name = "Fire";
	}

	public override bool IsMatch(Statistics stats)
	{
		if (stats.Directions.Count < 4)
		{
			return false;
		}
		return base.IsMatch(stats) && stats.Proximity < 0.4f && stats.Directions.Count >= 4;
	}
}
