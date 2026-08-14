using System;
using UnityEngine;

internal class LinesCast : Cast
{
	internal class FollowTheDirection
	{
		private bool _checkAngle;

		private readonly float _maxAngleDeviation;

		private readonly float _deviation;

		private readonly Vector2 _direction;

		internal bool IsValid { get; private set; }

		internal bool IsFailed { get; private set; }

		internal Vector2 LastValidPosition { get; private set; }

		internal Vector2 StartPosition { get; private set; }

		internal FollowTheDirection(string name, Vector2 direction, float rangeAngle, float deviation, bool checkAngle)
		{
			_direction = direction;
			_maxAngleDeviation = rangeAngle;
			_deviation = deviation;
			_checkAngle = checkAngle;
		}

		internal void Start(Vector2 position)
		{
			StartPosition = position;
			LastValidPosition = position;
			IsValid = true;
			IsFailed = false;
		}

		internal bool Move(Vector2 position)
		{
			if (!IsValid)
			{
				return false;
			}
			Vector2 vector = position - StartPosition;
			float magnitude = vector.magnitude;
			float num = Vector2.Dot(vector.normalized, _direction);
			float f = 57.29578f * Mathf.Acos(num);
			float num2 = num * magnitude;
			float f2 = Mathf.Sqrt(magnitude * magnitude - num2 * num2);
			if (Mathf.Abs(num2) > _deviation)
			{
				if (_checkAngle)
				{
					if (Mathf.Abs(f) < _maxAngleDeviation)
					{
						IsValid = Mathf.Abs(f2) <= _deviation;
					}
					else if (IsValid)
					{
						IsValid = false;
						IsFailed = true;
					}
				}
				else
				{
					IsValid = Mathf.Abs(f2) <= _deviation;
				}
			}
			else if (Mathf.Abs(f2) > _deviation)
			{
				IsValid = false;
				IsFailed = true;
			}
			if (IsValid)
			{
				LastValidPosition = position;
			}
			return IsValid;
		}
	}

	private TrailEffect _trailEffects;

	private Battle _battle;

	private readonly FollowTheDirection[] _lines;

	private readonly Func<FollowTheDirection, FollowTheDirection, bool> _validation;

	private FollowTheDirection _currentDir;

	private bool _failed;

	private bool Failed
	{
		get
		{
			return _failed;
		}
		set
		{
			_failed = value;
			if (value)
			{
				DestroyFx();
			}
		}
	}

	internal LinesCast(string name, Func<GameObject> fx, Battle battle, Func<FollowTheDirection, FollowTheDirection, bool> validation, params FollowTheDirection[] lines)
	{
		_trailEffects = new TrailEffect(fx);
		_battle = battle;
		base.Name = name;
		_lines = lines;
		_validation = validation;
	}

	private void DestroyFx()
	{
		_trailEffects.Destroy();
	}

	public override void Start(Vector2 point)
	{
		if (_battle.MagicAllowed)
		{
			_currentDir = _lines[0];
			_currentDir.Start(point);
			Failed = false;
			Vector3 wp = Utils.WP(point);
			_trailEffects.Start(wp);
		}
	}

	public override void Reset()
	{
		DestroyFx();
		_currentDir = null;
	}

	public override void End(Vector2 point)
	{
		DestroyFx();
		if (!_battle.MagicAllowed || Failed)
		{
			return;
		}
		FollowTheDirection followTheDirection = _lines[_lines.Length - 1];
		if (_currentDir == followTheDirection)
		{
			_currentDir.Move(point);
			if (_currentDir.IsValid)
			{
				if (_validation != null)
				{
					Failed = !_validation(_lines[0], followTheDirection);
				}
				if (Casted != null && !Failed)
				{
					Casted(this);
				}
			}
		}
		_currentDir = null;
	}

	public override void Move(Vector2 pos)
	{
		if (!_battle.MagicAllowed || _currentDir == null || _currentDir.IsFailed)
		{
			return;
		}
		Vector3 wp = Utils.WP(pos);
		_trailEffects.Move(wp);
		if (_currentDir.IsValid)
		{
			_currentDir.Move(pos);
		}
		if (!_currentDir.IsFailed && !_currentDir.IsValid)
		{
			int num = _lines.IndexOf(_currentDir);
			num++;
			if (num >= 1 && num < _lines.Length)
			{
				_currentDir = _lines[num];
				_currentDir.Start(pos);
			}
			else
			{
				Failed = true;
			}
		}
	}
}
