using System.Collections.Generic;
using UnityEngine;

internal class TouchscreenIPod : TouchscreenBase
{
	private class FingerData
	{
		public int Id;

		public Vector2 Start;

		public Vector2 Last;
	}

	private List<FingerData> _fingers = new List<FingerData>();

	private FingerData Remove(Touch touch)
	{
		FingerData fingerData = Get(touch);
		if (fingerData != null)
		{
			_fingers.Remove(fingerData);
		}
		return fingerData;
	}

	private FingerData Get(Touch touch)
	{
		return _fingers.Find((FingerData _) => _.Id == touch.fingerId);
	}

	public override void Clear()
	{
		base.Clear();
		_fingers.Clear();
	}

	public override void Update()
	{
		Touch[] touches = Input.touches;
		Touch[] array = touches;
		for (int i = 0; i < array.Length; i++)
		{
			Touch touch = array[i];
			switch (touch.phase)
			{
			case TouchPhase.Began:
			{
				FingerData fingerData = Get(touch);
				if (fingerData == null)
				{
					FingerData fingerData2 = new FingerData();
					fingerData2.Id = touch.fingerId;
					fingerData2.Start = touch.position;
					fingerData2.Last = touch.position;
					fingerData = fingerData2;
					_fingers.Add(fingerData);
				}
				TouchStarted(touch.position);
				break;
			}
			case TouchPhase.Moved:
				ProcessMove(touch);
				break;
			case TouchPhase.Ended:
			case TouchPhase.Canceled:
				if (Remove(touch) != null)
				{
					TouchEnd(touch.position);
				}
				break;
			}
		}
	}

	private void ProcessMove(Touch touch)
	{
		FingerData fingerData = Get(touch);
		if (fingerData != null)
		{
			if (_fingers.Count == 1)
			{
				TouchMoved(touch.deltaPosition, touch.position);
			}
			else if (_fingers.Count == 2 && Mathf.Abs(_fingers[0].Start.y - _fingers[1].Start.y) > 50f)
			{
				float num = Mathf.Abs(_fingers[0].Last.y - _fingers[1].Last.y);
				fingerData.Last = touch.position;
				float num2 = Mathf.Abs(_fingers[0].Last.y - _fingers[1].Last.y);
				float num3 = 0.02f * (num - num2);
				Zoom(0f - num3, Utils.Midpoint(_fingers[0].Start, _fingers[1].Start), _fingers[0].Start, _fingers[1].Start);
			}
		}
	}
}
