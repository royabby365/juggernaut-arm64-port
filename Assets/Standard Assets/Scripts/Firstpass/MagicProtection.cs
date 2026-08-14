using System.Collections;
using System.Linq;
using UnityEngine;
using Yarx;

public class MagicProtection : MonoBehaviour
{
	public Vector3 ActivePosition;

	public int Extent = 160;

	public Transform[] FootsOrdered;

	public Vector3 InactivePosition;

	public int TicksFinal = 3;

	public int TicksToShowOne = 2;

	public int TicksToInputForOne = 2;

	private float _tick;

	private CompositeDisposable _subscriptions;

	private int _current;

	private Transform[] _foots;

	private int _max;

	private float _timeToInput;

	private bool _youLose;

	private void Awake()
	{
		for (int i = 0; i < FootsOrdered.Length; i++)
		{
			Transform transform = FootsOrdered[i];
			Quaternion quaternion = Quaternion.AngleAxis(45 * i, Vector3.back);
			transform.localPosition = (Extent * (quaternion * Vector3.up)).RoundToInt();
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int>.AddListener(Globals.Msg_MagicGame_Show, StartGame));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgFightBreak, OnMsgFightBreak));
	}

	private void OnMsgFightBreak()
	{
		StopAllCoroutines();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		_subscriptions.Dispose();
	}

	private void Start()
	{
		SpriteGui spriteGui = base.transform.GetSpriteGui();
		if (spriteGui != null)
		{
			spriteGui.Release += ProcessFoots;
		}
	}

	private void ProcessFoots(SpriteButton button)
	{
		Foot foot = button as Foot;
		if (!(foot == null))
		{
			foot.SetState(Foot.State.InputShow);
			Transform transform = _foots[_current++];
			if (foot.name != transform.name)
			{
				_youLose = true;
			}
			foot.SetInactive();
		}
	}

	internal void StartGame(int max)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.StrongMagicMiniGame);
		if (SingletonT<ServerData>.I != null && SingletonT<ServerData>.I.GameSettings != null)
		{
			_tick = SingletonT<ServerData>.I.GameSettings.TimeTickMagicProtection;
			if (Globals.IsDebugBuild)
			{
				Debug.Log("ServerData.I.GameSettings.TimeTickMagicProtection:" + _tick);
			}
		}
		_max = Mathf.Clamp(max, 1, 8);
		_current = 0;
		_youLose = false;
		TicksFinal = SingletonT<ServerData>.I.GameSettings.MagicProtectionTime3;
		TicksToShowOne = SingletonT<ServerData>.I.GameSettings.MagicProtectionTime2;
		TicksToInputForOne = SingletonT<ServerData>.I.GameSettings.MagicProtectionTime1;
		_timeToInput = _tick * (float)TicksToInputForOne * (float)_max;
		_foots = FootsOrdered.Shuffle();
		foreach (Foot item in FootsOrdered.Select((Transform ft) => ft.GetComponent<Foot>()))
		{
			item.SetState(Foot.State.Start);
			item.SetInactive();
		}
		StartCoroutine("PhaseOne");
	}

	private void SetGameInactive()
	{
		if (!(HudMk1.Instance == null))
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.EnemyTurn);
		}
	}

	private IEnumerator PhaseOne()
	{
		yield return new WaitForSeconds(((HudMk1)base.transform.GetSpriteGui()).GetMaxTime(GuiRoot.State.On, GuiRoot.GuiType.StrongMagicMiniGame));
		for (int i = 0; i < _max; i++)
		{
			Foot foot = _foots[i].GetComponent<Foot>();
			foot.SetState(Foot.State.Show);
			yield return new WaitForSeconds(_tick * (float)TicksToShowOne);
			foot.SetState(Foot.State.Start);
		}
		StartCoroutine("PhaseTwo");
	}

	private IEnumerator PhaseTwo()
	{
		foreach (Foot foot in _foots.Select((Transform ft) => ft.GetComponent<Foot>()))
		{
			foot.SetState(Foot.State.Input);
			foot.SetActive();
		}
		yield return null;
		StartCoroutine("PhaseThree");
	}

	private IEnumerator PhaseThree()
	{
		float begin = Time.time;
		float curTime = begin;
		Battle.StateE oldState = Globals.Battle.State;
		Globals.Battle.State = Battle.StateE.WaitEnemyMiniGameEnd;
		Globals.Battle.TimeRemainsTillTheEndOfState = _timeToInput;
		while (curTime < begin + _timeToInput && _current < _max)
		{
			curTime += Time.deltaTime;
			yield return null;
		}
		Globals.Battle.State = oldState;
		StartCoroutine((_current != _max || _youLose) ? "PhaseLose" : "PhaseWin");
	}

	private IEnumerator PhaseWin()
	{
		foreach (Foot foot in _foots.Select((Transform ft) => ft.GetComponent<Foot>()))
		{
			foot.SetState(Foot.State.Win);
		}
		yield return new WaitForSeconds(_tick * (float)TicksFinal);
		Messenger.Invoke(Globals.Msg_MagicGame_Finished, "strong", arg2: true);
		SetGameInactive();
	}

	private IEnumerator PhaseLose()
	{
		SingletonT<SoundManager>.I.PlayFailSound();
		foreach (Foot foot in _foots.Select((Transform ft) => ft.GetComponent<Foot>()))
		{
			foot.SetState(Foot.State.Lose);
		}
		GetComponent<Animation>().Play();
		yield return new WaitForSeconds(_tick * (float)TicksFinal);
		Messenger.Invoke(Globals.Msg_MagicGame_Finished, "strong", arg2: false);
		SetGameInactive();
	}
}
