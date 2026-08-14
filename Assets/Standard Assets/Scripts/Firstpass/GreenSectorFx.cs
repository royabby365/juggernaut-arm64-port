using UnityEngine;

public class GreenSectorFx : MonoBehaviour
{
	public float howLong = 3f;

	public float deltaY = 0.05f;

	public AnimationCurve curve;

	private float _startTime;

	private float _endTime;

	private bool _end;

	private void Start()
	{
		if (howLong <= 0f && Globals.IsDebugBuild)
		{
			Debug.LogError("howLong must be > 0f");
		}
	}

	private void Update()
	{
		if (Time.time < _endTime)
		{
			float num = curve.Evaluate((Time.time - _startTime) / howLong);
			num *= deltaY;
			Transform transform = base.transform;
			transform.localPosition = new Vector3(transform.localPosition.x, num, transform.localPosition.z);
		}
		else if (!_end)
		{
			Transform transform2 = base.transform;
			transform2.localPosition = new Vector3(transform2.localPosition.x, 0f, transform2.localPosition.z);
			_end = true;
		}
	}

	public void Animate()
	{
		_startTime = Time.time;
		_endTime = Time.time + howLong;
		_end = false;
	}
}
