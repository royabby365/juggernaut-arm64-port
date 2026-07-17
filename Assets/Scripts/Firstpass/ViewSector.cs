using UnityEngine;

public class ViewSector : MonoBehaviour
{
	internal enum SideE
	{
		Right,
		RightCenter,
		Center,
		LeftCenter,
		Left
	}

	private class AnimData
	{
		internal Color FromColor;

		internal Color ToColor;

		private float TimeIn;

		internal readonly float MaxTime = 0.5f;

		internal Transform Renderer;

		internal Transform Renderer2;

		internal bool Update(float dt)
		{
			TimeIn += dt;
			bool result = true;
			if (TimeIn >= MaxTime)
			{
				result = false;
				TimeIn = MaxTime;
			}
			Color color = Color.Lerp(FromColor, ToColor, TimeIn / MaxTime);
			Renderer.SetTintRecursively(color);
			if (Renderer2 != null)
			{
				Renderer2.SetTintRecursively(color);
			}
			return result;
		}
	}

	internal float SpeedInAngles = 20f;

	public float RangeMin = -60f;

	public float RangeMax = 60f;

	internal float _angle;

	private float _direction = 1f;

	internal float ChangeDirPeriod;

	internal int ChangeDirProb;

	private float _changeDirPeriodRemains;

	private Transform _left;

	private Transform _center;

	private Transform _right;

	private Transform _left2;

	private Transform _center2;

	private Transform _right2;

	private Color _leftColor;

	private Color _centerColor;

	private Color _rightColor;

	public GameObject Prefab1;

	public GameObject Prefab2;

	public GameObject Prefab3;

	public float RightMinAngle = 45f;

	public float RightCenterMinAngle = 15f;

	public float CenterMinAngle = -15f;

	public float LeftCenterMinAngle = -45f;

	private AnimData _centerAnim;

	private AnimData _rightAnim;

	private AnimData _leftAnim;

	private Transform _redPlane;

	private bool _originalDisabled;

	internal int ZoneSize = 1;

	private SideE _prevSide = SideE.Center;

	private static Color FreeColor = Color.green;

	private static Color BlockedColor = Color.yellow;

	private static Color ClosedColor = new Color(0.99215686f, 0.5254902f, 0f);

	internal SideE Side
	{
		get
		{
			if (_angle > RightMinAngle)
			{
				return SideE.Right;
			}
			if (_angle > RightCenterMinAngle)
			{
				return SideE.RightCenter;
			}
			if (_angle > CenterMinAngle)
			{
				return SideE.Center;
			}
			if (_angle > LeftCenterMinAngle)
			{
				return SideE.LeftCenter;
			}
			return SideE.Left;
		}
	}

	internal void Init()
	{
		Transform transform = base.transform.FindChildByName("red_plane", includeInactive: true);
		Transform transform2 = base.transform.FindChildByName("red_plane_1.5x", includeInactive: true);
		Transform transform3 = base.transform.FindChildByName("red_plane_2x", includeInactive: true);
		transform.gameObject.SetActiveRecursivelyMk1(setActive: false);
		transform2.gameObject.SetActiveRecursivelyMk1(setActive: false);
		transform3.gameObject.SetActiveRecursivelyMk1(setActive: false);
		if (ZoneSize == 1)
		{
			RangeMin = -78f;
			RangeMax = 78f;
			RightMinAngle = 50f;
			RightCenterMinAngle = 15f;
			CenterMinAngle = -15f;
			LeftCenterMinAngle = -50f;
			_redPlane = transform;
		}
		else if (ZoneSize == 2)
		{
			RangeMin = -85f;
			RangeMax = 57f;
			RightMinAngle = 40f;
			RightCenterMinAngle = 0f;
			CenterMinAngle = -25f;
			LeftCenterMinAngle = -70f;
			_redPlane = transform2;
		}
		else if (ZoneSize == 3)
		{
			RangeMin = -88f;
			RangeMax = 60f;
			RightMinAngle = 50f;
			RightCenterMinAngle = -7f;
			CenterMinAngle = -20f;
			LeftCenterMinAngle = -78f;
			_redPlane = transform3;
		}
		if (_redPlane != null)
		{
			_redPlane.gameObject.SetActiveRecursivelyMk1(setActive: true);
			_redPlane.transform.SetTintRecursively(Color.red);
		}
	}

	private void OnEnable()
	{
		Init();
		float a = 1f;
		FreeColor = new Color(FreeColor.r, FreeColor.g, FreeColor.b, a);
		BlockedColor = new Color(BlockedColor.r, BlockedColor.g, BlockedColor.b, a);
		ClosedColor = new Color(ClosedColor.r, ClosedColor.g, ClosedColor.b, a);
		base.transform.root.GetComponent<Animation>().wrapMode = WrapMode.Loop;
		if (!_originalDisabled)
		{
			_originalDisabled = true;
			Vector3 vector = DisableIfFind("green1_light");
			Vector3 vector2 = DisableIfFind("green2_light");
			Vector3 vector3 = DisableIfFind("green3_light");
			InstanateLight(Prefab1, "green1_light");
			InstanateLight(Prefab2, "green2_light");
			InstanateLight(Prefab3, "green3_light");
			base.transform.root.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		}
	}

	private void InstanateLight(GameObject prefab, string name_)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(prefab);
		gameObject.transform.parent = base.transform;
		gameObject.transform.name = name_;
		Animation component = gameObject.GetComponent<Animation>();
		component.wrapMode = WrapMode.Loop;
	}

	private Vector3 DisableIfFind(string name)
	{
		Transform transform = base.transform.root.FindChildByName(name);
		Vector3 result = Vector3.zero;
		if (transform != null)
		{
			result = transform.gameObject.transform.position;
			Object.DestroyImmediate(transform.gameObject);
		}
		return result;
	}

	private void SetColors(Transform t1, Transform t2, bool isFirst, bool isSecond, Color first, Color second)
	{
		if (isFirst)
		{
			t1.SetTintRecursively(first);
			t2.SetTintRecursively(first);
		}
		else if (isSecond)
		{
			t1.SetTintRecursively(second);
			t2.SetTintRecursively(second);
		}
	}

	private void InitSide(out Transform t1, out Transform t2, out Color color, string id)
	{
		t1 = base.transform.Find("green" + id).gameObject.transform;
		t2 = base.transform.Find("green" + id + "_light").gameObject.transform;
		color = FreeColor;
		t1.transform.SetTintRecursively(color);
		t2.transform.SetTintRecursively(color);
	}

	private void UpdateSides()
	{
		SideE side = Side;
		bool flag = false;
		if (_right == null)
		{
			InitSide(out _right, out _right2, out _rightColor, "1");
			flag = true;
			SetColors(_right, _right2, side == SideE.Right, side == SideE.RightCenter, ClosedColor, BlockedColor);
		}
		if (_left == null)
		{
			InitSide(out _left, out _left2, out _leftColor, "3");
			flag = true;
			SetColors(_left, _left2, side == SideE.Left, side == SideE.LeftCenter, ClosedColor, BlockedColor);
		}
		if (_center == null)
		{
			InitSide(out _center, out _center2, out _centerColor, "2");
			flag = true;
			SetColors(_center, _center2, side == SideE.Center, side == SideE.RightCenter || side == SideE.LeftCenter, ClosedColor, BlockedColor);
		}
		if (flag)
		{
			_prevSide = side;
		}
		if (side != _prevSide)
		{
			if (_prevSide == SideE.Center && (side == SideE.LeftCenter || side == SideE.RightCenter))
			{
				_centerAnim = new AnimData
				{
					ToColor = BlockedColor,
					FromColor = ClosedColor,
					Renderer = _center,
					Renderer2 = _center2
				};
			}
			else if (_prevSide == SideE.RightCenter && side == SideE.Right)
			{
				_centerAnim = new AnimData
				{
					ToColor = FreeColor,
					FromColor = BlockedColor,
					Renderer = _center,
					Renderer2 = _center2
				};
			}
			else if (_prevSide == SideE.Right && side == SideE.RightCenter)
			{
				_centerAnim = new AnimData
				{
					ToColor = BlockedColor,
					FromColor = FreeColor,
					Renderer = _center,
					Renderer2 = _center2
				};
			}
			else if (_prevSide == SideE.RightCenter && side == SideE.Center)
			{
				_centerAnim = new AnimData
				{
					ToColor = ClosedColor,
					FromColor = BlockedColor,
					Renderer = _center,
					Renderer2 = _center2
				};
			}
			else if (_prevSide == SideE.LeftCenter && side == SideE.Left)
			{
				_centerAnim = new AnimData
				{
					ToColor = FreeColor,
					FromColor = BlockedColor,
					Renderer = _center,
					Renderer2 = _center2
				};
			}
			else if (_prevSide == SideE.Left && side == SideE.LeftCenter)
			{
				_centerAnim = new AnimData
				{
					ToColor = BlockedColor,
					FromColor = FreeColor,
					Renderer = _center,
					Renderer2 = _center2
				};
			}
			else if (_prevSide == SideE.LeftCenter && side == SideE.Center)
			{
				_centerAnim = new AnimData
				{
					ToColor = ClosedColor,
					FromColor = BlockedColor,
					Renderer = _center,
					Renderer2 = _center2
				};
			}
			if (_prevSide == SideE.Center && side == SideE.RightCenter)
			{
				_rightAnim = new AnimData
				{
					ToColor = BlockedColor,
					FromColor = FreeColor,
					Renderer = _right,
					Renderer2 = _right2
				};
			}
			else if (_prevSide == SideE.RightCenter && side == SideE.Right)
			{
				_rightAnim = new AnimData
				{
					ToColor = ClosedColor,
					FromColor = BlockedColor,
					Renderer = _right,
					Renderer2 = _right2
				};
			}
			else if (_prevSide == SideE.Right && side == SideE.RightCenter)
			{
				_rightAnim = new AnimData
				{
					ToColor = BlockedColor,
					FromColor = ClosedColor,
					Renderer = _right,
					Renderer2 = _right2
				};
			}
			else if (_prevSide == SideE.RightCenter && side == SideE.Center)
			{
				_rightAnim = new AnimData
				{
					ToColor = FreeColor,
					FromColor = BlockedColor,
					Renderer = _right,
					Renderer2 = _right2
				};
			}
			if (_prevSide == SideE.Center && side == SideE.LeftCenter)
			{
				_leftAnim = new AnimData
				{
					ToColor = BlockedColor,
					FromColor = FreeColor,
					Renderer = _left,
					Renderer2 = _left2
				};
			}
			else if (_prevSide == SideE.LeftCenter && side == SideE.Left)
			{
				_leftAnim = new AnimData
				{
					ToColor = ClosedColor,
					FromColor = BlockedColor,
					Renderer = _left,
					Renderer2 = _left2
				};
			}
			else if (_prevSide == SideE.Left && side == SideE.LeftCenter)
			{
				_leftAnim = new AnimData
				{
					ToColor = BlockedColor,
					FromColor = ClosedColor,
					Renderer = _left,
					Renderer2 = _left2
				};
			}
			else if (_prevSide == SideE.LeftCenter && side == SideE.Center)
			{
				_leftAnim = new AnimData
				{
					ToColor = FreeColor,
					FromColor = BlockedColor,
					Renderer = _left,
					Renderer2 = _left2
				};
			}
		}
		_prevSide = side;
	}

	private void Update()
	{
		SideE side = Side;
		UpdateSides();
		if (Globals.Battle.IsInMagicMode)
		{
			return;
		}
		if (!Globals.Battle.Pause || Globals.ViewSectorMoveForce)
		{
			float deltaTime = Time.deltaTime;
			_angle += _direction * SpeedInAngles * Time.deltaTime;
		}
		if (_angle > RangeMax)
		{
			_angle = RangeMax;
			_direction = 0f - _direction;
		}
		else if (_angle < RangeMin)
		{
			_angle = RangeMin;
			_direction = 0f - _direction;
		}
		if (_centerAnim != null && !_centerAnim.Update(Time.deltaTime))
		{
			_centerAnim = null;
		}
		if (_rightAnim != null && !_rightAnim.Update(Time.deltaTime))
		{
			_rightAnim = null;
		}
		if (_leftAnim != null && !_leftAnim.Update(Time.deltaTime))
		{
			_leftAnim = null;
		}
		Vector3 localEulerAngles = _redPlane.localEulerAngles;
		_redPlane.localEulerAngles = new Vector3(localEulerAngles.x, _angle, localEulerAngles.z);
		if (ChangeDirPeriod > 0f && ChangeDirProb > 0)
		{
			_changeDirPeriodRemains -= Time.deltaTime;
			if (_changeDirPeriodRemains <= 0f)
			{
				if (Random.Range(0, 100) <= ChangeDirProb)
				{
					_direction = 0f - _direction;
				}
				_changeDirPeriodRemains = ChangeDirPeriod;
			}
		}
		if (side == Side)
		{
		}
	}
}
