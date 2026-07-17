using System.Collections.Generic;
using UnityEngine;

namespace Gesture;

public class Statistics
{
	private readonly StrokeInfo _info;

	private readonly BoundsXY _bounds;

	public IList<Direction> Directions => _info.Directions;

	public int Total => _info.Directions.Count;

	public float Proximity { get; private set; }

	public float Extent => _bounds.Distance;

	public Statistics(IList<Vector3> points)
	{
		_bounds = BoundsXY.Create(points);
		IList<Vector3> list = Reduction.DouglasPeuckerReduction(points, _bounds.Distance * 0.2f);
		if (Globals.IsDebugBuild)
		{
			Vector3 vector = new Vector3((float)(-Screen.width) / 2f, (float)(-Screen.height) / 2f, 100f);
			for (int i = 1; i < list.Count; i++)
			{
				Debug.DrawLine(list[i - 1] + vector, list[i] + vector, Color.yellow, 5f, depthTest: false);
			}
		}
		_info = new StrokeInfo(list);
		Init();
	}

	private void Init()
	{
		Proximity = GetProximity();
	}

	private float GetProximity()
	{
		IList<Vector3> points = _info.Points;
		float distance = _bounds.Distance;
		float num = Vector3.Distance(points[0], points[points.Count - 1]);
		return (!Mathf.Approximately(num, 0f)) ? (num / distance) : 0f;
	}

	public override string ToString()
	{
		return $"Proximity: {Proximity} Extent:{Extent} Info:{_info}";
	}
}
