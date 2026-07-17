using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gesture;

public class StrokeInfo
{
	private const float AngleE0 = 0f;

	private const float AngleNNe = 292.5f;

	private const float AngleNwN = 247.5f;

	private const float AngleWNw = 202.5f;

	private const float AngleSwW = 157.5f;

	private const float AngleSSw = 112.5f;

	private const float AngleSeS = 67.5f;

	private const float AngleESe = 22.5f;

	private const float AngleE = 360f;

	private const float AngleNeE = 337.5f;

	private readonly List<Direction> _directions = new List<Direction>();

	private readonly IList<Vector3> _points;

	public IList<Direction> Directions => _directions;

	public IList<Vector3> Points
	{
		get
		{
			object result;
			if (_points.Count > 0)
			{
				IList<Vector3> points = _points;
				result = points;
			}
			else
			{
				List<Vector3> list = new List<Vector3>();
				list.Add(Vector3.zero);
				result = list;
			}
			return (IList<Vector3>)result;
		}
	}

	public StrokeInfo(IList<Vector3> points)
	{
		if (points == null)
		{
			return;
		}
		_points = points;
		for (int i = 1; i < points.Count; i++)
		{
			Direction direction = GetDirection(points[i - 1], points[i]);
			if (_directions.Count <= 0 || _directions[_directions.Count - 1] != direction)
			{
				_directions.Add(direction);
			}
		}
	}

	private static Direction GetDirection(Vector3 a, Vector3 b)
	{
		Vector3 to = b - a;
		float num = Vector3.Angle(Vector3.right, to);
		if (Vector3.Cross(Vector3.right, b - a).z > 0f)
		{
			num = 360f - num;
		}
		if (num >= 337.5f && num < 360f)
		{
			return Direction.E;
		}
		if (num >= 0f && num < 22.5f)
		{
			return Direction.E;
		}
		if (num >= 22.5f && num < 67.5f)
		{
			return Direction.SE;
		}
		if (num >= 67.5f && num < 112.5f)
		{
			return Direction.S;
		}
		if (num >= 112.5f && num < 157.5f)
		{
			return Direction.SW;
		}
		if (num >= 157.5f && num < 202.5f)
		{
			return Direction.W;
		}
		if (num >= 202.5f && num < 247.5f)
		{
			return Direction.NW;
		}
		if (num >= 247.5f && num < 292.5f)
		{
			return Direction.N;
		}
		if (num >= 292.5f && num < 337.5f)
		{
			return Direction.NE;
		}
		if (Globals.IsDebugBuild)
		{
			Debug.LogError("illegalState: " + num);
		}
		return Direction.N;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		int count = _directions.Count;
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Append(DirectionsToString(_directions[i]));
			if (i < count - 1)
			{
				stringBuilder.Append(",");
			}
		}
		stringBuilder.Append("]");
		return $"[StrokeInfo: {stringBuilder}]";
	}

	private static string DirectionsToString(Direction direction)
	{
		return direction switch
		{
			Direction.N => "N", 
			Direction.NE => "NE", 
			Direction.E => "E", 
			Direction.SE => "SE", 
			Direction.S => "S", 
			Direction.SW => "SW", 
			Direction.W => "W", 
			Direction.NW => "NW", 
			_ => throw new ArgumentOutOfRangeException("direction"), 
		};
	}
}
