using UnityEngine;

internal class TimePeriod
{
	private float _lastTime;

	private float _pauseTime = -1f;

	private float _accum;

	private bool _fired;

	private readonly float _period;

	public bool Pause
	{
		get
		{
			return _pauseTime > 0f;
		}
		set
		{
			if (value)
			{
				if (_pauseTime == -1f)
				{
					_pauseTime = Time.time;
				}
			}
			else
			{
				_pauseTime = -1f;
				_lastTime = Time.time;
			}
		}
	}

	public bool Fired => _fired;

	public TimePeriod(float period)
	{
		_period = period;
		_accum = 0f;
		_lastTime = Time.time;
	}

	public void Update()
	{
		if (_pauseTime == -1f && !_fired)
		{
			_accum += Time.time - _lastTime;
			_lastTime = Time.time;
			if (_accum >= _period)
			{
				_fired = true;
			}
		}
	}

	public override string ToString()
	{
		return "<{0} {1} {2} {3}>".Fmt(_fired, _period, _pauseTime, _lastTime);
	}

	internal void Reset()
	{
		_fired = false;
		_accum = 0f;
		_pauseTime = -1f;
		_lastTime = Time.time;
	}
}
