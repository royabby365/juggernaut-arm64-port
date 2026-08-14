using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gesture
{

public static class Reduction
{
	public static IList<Vector3> DouglasPeuckerReduction(IList<Vector3> points, float tolerance)
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log("//////------- TOLERANCE: {0}".Fmt(tolerance));
		}
		if (points == null || points.Count < 3)
		{
			return points;
		}
		int num = points.Count - 1;
		List<int> list = new List<int>();
		list.Add(0);
		list.Add(num);
		List<int> pointIndexsToKeep = list;
		while (points[0] == points[num] && num > 2)
		{
			num--;
		}
		DouglasPeuckerReduction(points, 0, num, tolerance, ref pointIndexsToKeep);
		pointIndexsToKeep.Sort();
		return pointIndexsToKeep.Select((int index) => points[index]).ToList();
	}

	private static void DouglasPeuckerReduction(IList<Vector3> points, int firstPoint, int lastPoint, float tolerance, ref List<int> pointIndexsToKeep)
	{
		float num = 0f;
		int num2 = 0;
		for (int i = firstPoint; i < lastPoint; i++)
		{
			float num3 = PerpendicularDistance(points[firstPoint], points[lastPoint], points[i]);
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		if (num > tolerance && num2 != 0)
		{
			pointIndexsToKeep.Add(num2);
			DouglasPeuckerReduction(points, firstPoint, num2, tolerance, ref pointIndexsToKeep);
			DouglasPeuckerReduction(points, num2, lastPoint, tolerance, ref pointIndexsToKeep);
		}
	}

	private static float PerpendicularDistance(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = 0.5f * Mathf.Abs(p1.x * p2.y + p2.x * p3.y + p3.x * p1.y - p2.x * p1.y - p3.x * p2.y - p1.x * p3.y);
		float num2 = Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2f) + Mathf.Pow(p1.y - p2.y, 2f));
		return 2f * num / num2;
	}
}
}
