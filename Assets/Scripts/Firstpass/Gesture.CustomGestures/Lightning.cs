namespace Gesture.CustomGestures
{

public class Lightning : CustomGesture
{
	public Lightning()
	{
		base.Name = "Lightning";
	}

	public override bool IsMatch(Statistics stats)
	{
		if (stats.Directions.Count > 3)
		{
			return false;
		}
		return base.IsMatch(stats) && stats.Proximity > 0.6f && CustomGesture.DownDirection(stats.Directions[0]);
	}
}
}
