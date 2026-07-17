using UnityEngine;

public class UpdateLoadingProgressBar : MonoBehaviour
{
	public Transform Wheel;

	private bool _shown;

	private Vector3 _pos;

	private float _time;

	private int _exitReason;

	private Vector3 _npos = new Vector3(100000f, 100000f, 0f);

	private void Start()
	{
		_pos = base.transform.localPosition;
		base.transform.localPosition = _npos;
	}

	private void Update()
	{
		if (_time > 0f)
		{
			_time -= Time.deltaTime;
			if (_time <= 0f && _shown)
			{
				_shown = false;
				_exitReason = 1;
				base.transform.localPosition = _npos;
			}
		}
		if (ServerData._inInitConfigsFromRemoteData)
		{
			if (_exitReason != 1)
			{
				if (!_shown)
				{
					_shown = true;
					base.transform.localPosition = _pos;
					_time = 10f;
					_exitReason = 0;
				}
				Wheel.localRotation *= Quaternion.Euler(0f, 0f, -90f * Time.deltaTime);
			}
		}
		else
		{
			if (_shown)
			{
				_shown = false;
				base.transform.localPosition = _pos;
			}
			_exitReason = 0;
		}
	}
}
