using System.Collections.Generic;
using Gesture.CustomGestures;
using UnityEngine;

namespace Gesture
{

public class Recognizer
{
	private readonly List<CustomGesture> _gestures = new List<CustomGesture>();

	public Recognizer AddGesture(CustomGesture gesture)
	{
		_gestures.Add(gesture);
		return this;
	}

	public IEnumerable<string> Recognize(IList<Vector3> stroke)
	{
		Statistics stats = new Statistics(stroke);
		List<string> list = new List<string>();
		foreach (CustomGesture gesture in _gestures)
		{
			if (gesture.IsMatch(stats))
			{
				list.Add(gesture.Name);
			}
		}
		return list;
	}
}
}
