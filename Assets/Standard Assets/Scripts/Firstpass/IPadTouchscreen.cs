using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IPadTouchscreen
{
	private Touch[] _touches;

	private Touch _startTouch;

	private bool _isStartTouch;

	public float MaxHOffset = 0.1f;

	public float MaxVOffset = 0.1f;

	public float MinMoveLength = 0.05f;

	public float _animWaitTime;

	public float _waitEnemyDamageTime;

	public string _lastAttackAnim;

	private Battle Battle;

	private readonly ITouchscreen _touchscreen;

	private Vector2 _sliceStartPos;

	private float _slicePathLenght;

	private Vector2 _prevOffset;

	private float _prevAngle;

	private TrailEffect _trailEffect;

	private Vector2 _startPos;

	private bool _eyeWasKilled;

	internal bool TrailEnabled
	{
		get
		{
			if (_trailEffect == null)
			{
				return false;
			}
			return _trailEffect.Enabled;
		}
		set
		{
			if (_trailEffect != null)
			{
				_trailEffect.Enabled = value;
			}
		}
	}

	[method: MethodImpl((MethodImplOptions)32)]
	internal event Action<float> OnAttackLeft;

	[method: MethodImpl((MethodImplOptions)32)]
	internal event Action<float> OnAttackRight;

	[method: MethodImpl((MethodImplOptions)32)]
	internal event Action<float> OnAttackForward;

	[method: MethodImpl((MethodImplOptions)32)]
	internal event Action<float> OnSlice;

	internal IPadTouchscreen(ITouchscreen touchscreen, Battle battle, Func<GameObject> fx)
	{
		_trailEffect = new TrailEffect(fx);
		_touchscreen = touchscreen;
		_touchscreen.OnTouchStart += Touchscreen_OnTouchStart;
		_touchscreen.OnTouchEnd += Touchscreen_OnTouchEnd;
		_touchscreen.OnTouchMove += Touchscreen_OnTouchMove;
		Battle = battle;
	}

	internal void Restart()
	{
		if (_trailEffect != null)
		{
			_trailEffect.Destroy();
		}
		_isStartTouch = false;
	}

	private void Touchscreen_OnTouchMove(Vector2 offset, Vector2 absolute)
	{
		if (!Globals.IsPaused)
		{
			if (_isStartTouch && !_trailEffect.Started && Vector2.Distance(absolute, _startPos) > 20f)
			{
				_trailEffect.Start(Utils.WP(absolute));
			}
			if (_trailEffect != null && _trailEffect.Started)
			{
				_trailEffect.Move(Utils.WP(absolute));
			}
			ProcessSlice(ref offset, ref absolute);
		}
	}

	private void Touchscreen_OnTouchEnd(Vector2 startPoint, Vector2 endPoint, float time)
	{
		if (!Globals.IsPaused && _isStartTouch)
		{
			ProcessMoveEnd(startPoint, endPoint, time);
		}
	}

	private void Touchscreen_OnTouchStart(Vector2 point)
	{
		if (!Globals.IsPaused)
		{
			_eyeWasKilled = false;
			_isStartTouch = true;
			_startPos = point;
			RestartSlice(ref point);
			if (Globals.Battle._eye != null)
			{
				Globals.Battle._eye.Hited = false;
			}
			if (_trailEffect != null)
			{
				_trailEffect.Start(Utils.WP(point));
			}
		}
	}

	private void ProcessMoveEnd(Vector2 startPoint, Vector2 endPoint, float time)
	{
		Vector2 offset = Vector2.zero;
		ProcessSlice(ref offset, ref endPoint);
		if (_trailEffect != null)
		{
			_trailEffect.Destroy();
		}
		float num = endPoint.x - startPoint.x;
		float num2 = endPoint.y - startPoint.y;
		float f = num2 / num;
		float angle = ((num == 0f) ? 0f : (Mathf.Atan(f) * 57.29578f));
		_isStartTouch = false;
		bool flag = Battle.State == Battle.StateE.PlayerTime || Battle.State == Battle.StateE.FatalityMode;
		if ((!flag && !(Globals.Battle._eye != null)) || _eyeWasKilled || (Mathf.Abs(num) <= 5f && Mathf.Abs(num2) <= 5f))
		{
			return;
		}
		float magnitude = new Vector2(num, num2).magnitude;
		if (magnitude <= 0.1f * (float)Screen.width || magnitude / time < 250f)
		{
			return;
		}
		float num3 = ConvertAtanAngleToEuler(angle, new Vector3(num, num2, 0f));
		if ((num3 > 135f && num3 <= 225f) || num3 > 315f || num3 <= 45f)
		{
			if (!CheckEye() && flag && this.OnAttackForward != null)
			{
				this.OnAttackForward(num3);
			}
		}
		else if (num3 > 45f && num3 <= 135f && !CheckEye() && flag && this.OnAttackLeft != null)
		{
			this.OnAttackLeft(num3);
		}
		if (num3 > 225f && num3 <= 315f && !CheckEye() && flag && this.OnAttackRight != null)
		{
			this.OnAttackRight(num3);
		}
	}

	private bool CheckEye()
	{
		return false;
	}

	private void ProcessSlice(ref Vector2 offset, ref Vector2 absolute)
	{
		float f = offset.y / offset.x;
		float num = ((offset.x == 0f) ? 0f : (Mathf.Atan(f) * 57.29578f));
		bool flag = IsDirectionChanged(ref _prevOffset, ref offset, _prevAngle, num);
		float magnitude = offset.magnitude;
		if (magnitude < 5f)
		{
			CheckSlice(ConvertAtanAngleToEuler(num, offset));
			RestartSlice(ref absolute);
		}
		else if (flag)
		{
			CheckSlice(ConvertAtanAngleToEuler(num, offset));
			RestartSlice(ref absolute);
		}
		else
		{
			_slicePathLenght += magnitude;
		}
		_prevOffset = offset;
		_prevAngle = num;
	}

	private float ConvertAtanAngleToEuler(float angle, Vector3 pos)
	{
		angle = ((!(pos.x >= 0f)) ? (angle + 90f) : (angle + 270f));
		return angle;
	}

	private bool IsDirectionChanged(ref Vector2 pos1, ref Vector2 pos2, float angle1, float angle2)
	{
		bool flag = false;
		if (pos1.x > 0f && pos2.x > 0f)
		{
			return Mathf.Abs(angle1 - angle2) > 45f;
		}
		if (pos1.x < 0f && pos2.x < 0f)
		{
			return Mathf.Abs(angle1 - angle2) > 45f;
		}
		float num = Mathf.Max(angle1, angle2);
		float num2 = Mathf.Min(angle1, angle2);
		num -= 180f;
		return Mathf.Abs(num - num2) > 45f;
	}

	private void CheckSlice(float clockAngle)
	{
		if (_slicePathLenght > 150f)
		{
			CheckEye();
			if (this.OnSlice != null)
			{
				this.OnSlice(clockAngle);
			}
		}
	}

	private void RestartSlice(ref Vector2 sliceStartPos)
	{
		_sliceStartPos = sliceStartPos;
		_slicePathLenght = 0f;
	}
}
