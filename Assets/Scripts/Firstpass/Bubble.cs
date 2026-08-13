using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class Bubble : MonoBehaviour
{
	internal enum TypeE
	{
		Mana,
		Rage
	}

	private static List<bool> _matrixFlags;

	private static readonly List<Vector2> _matrix;

	private static readonly float _farPlane;

	private float _spawnTime;

	private readonly float _flightDuration = 3f;

	private int _matrixIndex;

	private Vector3 _startPos;

	internal TypeE Type;

	internal GameObject Target;

	internal GameObject SelectFx;

	private bool _goUp = true;

	private Vector3 _targetPos = Vector3.zero;

	private int _turnCounter = 3;

	private bool _destroyed;

	internal Battle Battle;

	public int ManaPoints;

	private CompositeDisposable _subscriptions;

	static Bubble()
	{
		_matrixFlags = new List<bool>();
		_matrix = new List<Vector2>();
		_farPlane = 6f;
		float num = (float)Screen.width / 2f;
		float num2 = 0.8f * (float)Screen.width;
		int num3 = (int)num2 / 16;
		num2 = 16 * num3;
		float num4 = ((float)Screen.width - num2) / 2f;
		for (int i = 0; i < 16; i++)
		{
			_matrix.Add(new Vector2(num4 + (float)(i * num3), 2 * num3 + num3 * (i % 2)));
		}
		_matrixFlags = new List<bool>(_matrix.Count);
		for (int j = 0; j < _matrix.Count; j++)
		{
			Vector2 vector = _matrix[j];
			_matrixFlags.Add(item: false);
		}
		Messenger.AddListener(Globals.MsgFightStarted, delegate
		{
			for (int k = 0; k < _matrixFlags.Count; k++)
			{
				_matrixFlags[k] = false;
			}
		});
	}

	internal static void DestroyAll()
	{
		Object[] array = Object.FindSceneObjectsOfType(typeof(Bubble));
		for (int i = 0; i < array.Length; i++)
		{
			Bubble bubble = (Bubble)array[i];
			if (bubble != null)
			{
				bubble.DestroyWithoutExplosion();
			}
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<Battle.StateE>.AddListener(Globals.MsgBattleStateChanged, Bubble_TurnStateChanged));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
		_destroyed = true;
	}

	private void Start()
	{
		Camera mainCamera = Camera.main;
		Vector3 vector = mainCamera.WorldToScreenPoint(base.transform.position);
		vector.z = _farPlane;
		_startPos = mainCamera.transform.InverseTransformPoint(base.transform.position);
		_matrixIndex = -1;
		bool flag = false;
		int num = 0;
		Vector3 vector2 = GetVector(0);
		do
		{
			_matrixIndex = Random.Range(0, _matrix.Count);
			if (!_matrixFlags[_matrixIndex])
			{
				flag = true;
			}
			num++;
			if (num > 5)
			{
				vector2 = GetVector(_matrixIndex);
				_matrixIndex = -1;
				break;
			}
		}
		while (!flag);
		if (_matrixIndex > -1)
		{
			vector2 = GetVector(_matrixIndex);
			_matrixFlags[_matrixIndex] = true;
		}
		_targetPos = mainCamera.ScreenToWorldPoint(vector2);
		_targetPos = mainCamera.transform.InverseTransformPoint(_targetPos);
		base.transform.parent = mainCamera.transform;
		base.transform.localPosition = _startPos;
		_spawnTime = Time.time;
	}

	private Vector3 GetVector(int index)
	{
		Vector2 vector = _matrix[index];
		return new Vector3(vector.x, Camera.main.pixelHeight - vector.y, _farPlane);
	}

	private void Bubble_TurnStateChanged(Battle.StateE obj)
	{
		if (!_destroyed)
		{
			if (obj == Battle.StateE.PlayerTime)
			{
				_turnCounter--;
			}
			if (_turnCounter == 0)
			{
				Messenger.Invoke(Globals.MsgBubblesDestroySelf, this);
				Object.Destroy(base.gameObject);
				_destroyed = true;
			}
		}
	}

	private void Update()
	{
		if (_goUp)
		{
			float num = Time.time - _spawnTime;
			num /= _flightDuration;
			if (num > 1f)
			{
				num = 1f;
				_goUp = false;
			}
			base.transform.localPosition = Vector3.Lerp(_startPos, _targetPos, num);
		}
	}

	private void OnDestroy()
	{
		if (_matrixIndex > -1)
		{
			_matrixFlags[_matrixIndex] = false;
		}
	}

	internal void DestroyWithoutExplosion()
	{
		if (!_destroyed)
		{
			_destroyed = true;
			Object.Destroy(base.gameObject, 0.02f);
		}
	}

	internal void Destroy()
	{
		if (!_destroyed)
		{
			_destroyed = true;
			Object.Destroy(base.gameObject, 0.02f);
			if (SelectFx != null)
			{
				Object.Destroy(Object.Instantiate(SelectFx, base.transform.position, base.transform.rotation), 10f);
			}
		}
	}
}
