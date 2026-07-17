using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class GamePrototype : MonoBehaviour
{
	public enum States
	{
		Idle,
		GoingUp,
		GoingDown
	}

	public const string Idle = "idle";

	public const string Idle2 = "idle2";

	public const string AttackForce = "attack_force";

	public const string AttackForceFail = "attack_force_fail";

	public const string AttackIdle = "attack_idle";

	public const string AttackNormal = "attack_normal";

	public const string AttackSuper = "attack_super";

	public const string AttackWeak = "attack_weak";

	public const string AttackWarm = "warm";

	public const string AttackSuperFail = "attack_super_fail";

	public const string DefaultConfig = "time 3\n30 weak\n50 normal\n60 super\n70 normal\n95 fail\n100 warm\n";

	private const string SaveCommandsKey = "commands";

	private CompositeDisposable _subscriptions;

	public AnimationCurve IndicatorCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Animation Anim;

	public Transform Arrow;

	public PrototypeFightButton FightButton;

	public Transform IndicatorRoot;

	public Rect TextFieldRect = new Rect(480f, 100f, 440f, 800f);

	public Rect ResetButton = new Rect(100f, 200f, 200f, 100f);

	public Rect RefreshButton = new Rect(100f, 200f, 200f, 100f);

	public Rect SaveButton = new Rect(100f, 100f, 100f, 100f);

	public Rect LoadButton = new Rect(100f, 100f, 100f, 100f);

	public UnityEngine.Object CellPrefab;

	public Color WarmColor;

	public Color WeakColor;

	public Color NormalColor;

	public Color SuperColor;

	public Color SuperFailColor;

	private float _time = 2f;

	private float _timeToDown = 1f;

	private readonly Dictionary<int, Tuple<string, Color>> _cells = new Dictionary<int, Tuple<string, Color>>();

	private States _state;

	private States _prev;

	private float _d;

	private float _maxD;

	private float _startToGoingUp;

	private float _startToGoingDown;

	private float _startPosToGoingDown;

	private string _stringToEdit;

	private float _accum;

	private void Awake()
	{
		_state = States.Idle;
		Anim.CrossFade("idle2");
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		LoadCommands();
		LoadConfig();
		FightButton.Press += FightButtonOnPress;
		FightButton.Release += FightButtonOnRelease;
		_d = 0f;
		MoveArrow();
	}

	private void FightButtonOnRelease()
	{
		if (_state == States.GoingUp)
		{
			_prev = States.GoingUp;
			_state = States.GoingDown;
		}
	}

	private void FightButtonOnPress()
	{
		if (_state == States.Idle)
		{
			_prev = States.Idle;
			_state = States.GoingUp;
		}
	}

	private void OnApplicationQuit()
	{
		SaveCommands();
		RemoveOldCells();
	}

	private void OnGUI()
	{
		_stringToEdit = GUI.TextArea(TextFieldRect, _stringToEdit);
		if (GUI.Button(RefreshButton, "Refresh"))
		{
			LoadConfig();
		}
		if (GUI.Button(ResetButton, "Default"))
		{
			_stringToEdit = "time 3\n30 weak\n50 normal\n60 super\n70 normal\n95 fail\n100 warm\n";
			LoadConfig();
		}
		if (GUI.Button(SaveButton, "Save To File"))
		{
			SaveCommands();
		}
		if (GUI.Button(LoadButton, "Load From File"))
		{
			LoadCommands();
			LoadConfig();
		}
	}

	private void LateUpdate()
	{
		_accum += Time.deltaTime;
		switch (_state)
		{
		case States.Idle:
			if (!Anim.isPlaying)
			{
				PlayAnim("idle2");
			}
			_accum = 0f;
			_d = 0f;
			_maxD = 0f;
			break;
		case States.GoingUp:
			if (_prev == States.Idle)
			{
				_prev = States.GoingUp;
				_accum = 0f;
				PlayAnim("attack_idle");
			}
			_d = IndicatorCurve.Evaluate(_accum / _time);
			if (_d >= 1f)
			{
				_state = States.GoingDown;
			}
			break;
		case States.GoingDown:
			if (_prev == States.GoingUp)
			{
				_prev = States.GoingDown;
				_accum = Time.deltaTime;
				Hit((_d * 100f).RoundToInt());
				_maxD = _d;
			}
			_d = IndicatorCurve.Evaluate(_maxD - _accum / _timeToDown);
			if (_d <= 0f)
			{
				_state = States.Idle;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		MoveArrow();
	}

	private void PlayAnim(string state)
	{
		Anim.wrapMode = ((!state.Contains("idle")) ? WrapMode.Once : WrapMode.Loop);
		Anim.CrossFade(state, 0.5f);
		Debug.Log("[c:{0} state:{1} mode:{2}]".Fmt(Time.frameCount, state, Anim.wrapMode));
	}

	private void MoveArrow()
	{
		int num = (_d * 600f).RoundToInt() - 300;
		Vector3 localPosition = Arrow.localPosition;
		Arrow.localPosition = new Vector3(localPosition.x, num, localPosition.z);
	}

	private void Hit(int i)
	{
		i = Mathf.Min(i, 100);
		i = Mathf.Max(0, i);
		IOrderedEnumerable<KeyValuePair<int, Tuple<string, Color>>> orderedEnumerable = _cells.OrderBy((KeyValuePair<int, Tuple<string, Color>> kv) => kv.Key);
		foreach (KeyValuePair<int, Tuple<string, Color>> item2 in orderedEnumerable)
		{
			if (i > item2.Key)
			{
				continue;
			}
			string item = item2.Value.Item1;
			PlayAnim(item);
			break;
		}
	}

	private void SaveCommands()
	{
		PlayerPrefs.SetString("commands", _stringToEdit);
	}

	private void LoadCommands()
	{
		_stringToEdit = PlayerPrefs.GetString("commands", "time 3\n30 weak\n50 normal\n60 super\n70 normal\n95 fail\n100 warm\n");
	}

	private void LoadConfig()
	{
		string[] array = _stringToEdit.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length <= 0)
		{
			return;
		}
		_cells.Clear();
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text.ToLower();
			string[] array3 = text2.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array3.Length < 2)
			{
				continue;
			}
			if (array3.Length == 2 && array3[0] == "time")
			{
				float result = 2f;
				float.TryParse(array3[1], out result);
				_time = result;
			}
			else if (array3.Length >= 2)
			{
				int result2 = 10;
				int.TryParse(array3[0], out result2);
				result2 = Mathf.Min(Mathf.Max(0, result2), 100);
				string item = "warm";
				Color item2 = Color.black;
				switch (array3[1])
				{
				case "weak":
					item = "attack_weak";
					item2 = WeakColor;
					break;
				case "normal":
					item = "attack_normal";
					item2 = NormalColor;
					break;
				case "super":
					item = "attack_super";
					item2 = SuperColor;
					break;
				case "warm":
					item = "warm";
					item2 = WarmColor;
					break;
				case "fail":
					item = "attack_super_fail";
					item2 = SuperFailColor;
					break;
				}
				_cells[result2] = Tuple.Create(item, item2);
				RefreshCells();
			}
		}
	}

	private void RefreshCells()
	{
		if (_cells.Count == 0)
		{
			_cells[100] = Tuple.Create("attack_normal", Color.green);
		}
		IOrderedEnumerable<KeyValuePair<int, Tuple<string, Color>>> orderedEnumerable = _cells.OrderBy((KeyValuePair<int, Tuple<string, Color>> kv) => kv.Key);
		KeyValuePair<int, Tuple<string, Color>>[] array = orderedEnumerable.Select(delegate(KeyValuePair<int, Tuple<string, Color>> kv)
		{
			Debug.Log("key: {0} attack:{1} color:{2}".Fmt(kv.Key, kv.Value.Item1, kv.Value.Item2));
			return kv;
		}).ToArray();
		RemoveOldCells();
		int num = 0;
		foreach (KeyValuePair<int, Tuple<string, Color>> item in orderedEnumerable)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(CellPrefab);
			gameObject.transform.parent = IndicatorRoot;
			gameObject.transform.localPosition = new Vector3(0f, 6 * num, 0f);
			int height = 6 * (item.Key - num);
			num = item.Key;
			Sprite component = gameObject.GetComponent<Sprite>();
			component.Height = height;
			component.Tint_ = item.Value.Item2;
			component.Refresh();
		}
	}

	private void RemoveOldCells()
	{
		foreach (Transform item in IndicatorRoot)
		{
			if (item.name.Contains("cell"))
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}
}
