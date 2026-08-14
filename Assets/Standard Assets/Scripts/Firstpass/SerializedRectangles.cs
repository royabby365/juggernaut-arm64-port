using UnityEngine;

public class SerializedRectangles : MonoBehaviour
{
	public int LocationWidth;

	public int LocationHeight;

	public Rect[] Rectangles;

	public Rect[] SmallRectangles;

	public Rect[] BigRectangles;

	private void OnEnable()
	{
		Start();
	}

	private void Start()
	{
		if ((SmallRectangles.Length < 12 || BigRectangles.Length < 8) && Globals.IsDebugBuild)
		{
			Debug.LogError("INVARIANT FILED: SMALL:{0} BIG:{1}".Fmt(SmallRectangles.Length, BigRectangles.Length));
		}
	}
}
