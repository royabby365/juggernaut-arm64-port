using System.Collections.Generic;
using UnityEngine;

namespace Gesture
{

public class BoundsXY
{
	private float _minx;

	private float _miny;

	private float _maxx;

	private float _maxy;

	public float Distance => Vector2.Distance(new Vector2(_minx, _miny), new Vector2(_maxx, _maxy));

	public BoundsXY(Vector3 center)
	{
		_minx = (_maxx = center.x);
		_miny = (_maxy = center.y);
	}

	public static BoundsXY Create(IList<Vector3> points)
	{
		if (points == null || points.Count == 0)
		{
			return new BoundsXY(Vector3.zero);
		}
		BoundsXY boundsXY = new BoundsXY(points[0]);
		for (int i = 1; i < points.Count; i++)
		{
			boundsXY.AddPoint(points[i]);
		}
		return boundsXY;
	}

	public BoundsXY AddPoint(Vector3 point)
	{
		if (point.x < _minx)
		{
			_minx = point.x;
		}
		if (point.x > _maxx)
		{
			_maxx = point.x;
		}
		if (point.y < _miny)
		{
			_miny = point.y;
		}
		if (point.y > _maxy)
		{
			_maxy = point.y;
		}
		return this;
	}
}
}
