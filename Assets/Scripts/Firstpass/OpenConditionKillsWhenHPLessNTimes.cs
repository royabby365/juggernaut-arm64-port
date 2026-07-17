using ProtoBuf;

[ProtoContract]
public class OpenConditionKillsWhenHPLessNTimes : OpenCondition
{
	private bool _fightStarted;

	private int _lastEnemyHealth = -1;

	[ProtoMember(3)]
	public int _health;

	public OpenConditionKillsWhenHPLessNTimes()
	{
	}

	public OpenConditionKillsWhenHPLessNTimes(string[] args)
		: base(args[0])
	{
		_health = int.Parse(args[1]);
	}

	public override void Enable()
	{
		base.Enable();
		base._listeners.Add(Messenger.AddListener(Globals.MsgFightStarted, delegate
		{
			if (CheckProgress)
			{
				_fightStarted = true;
			}
		}));
		base._listeners.Add(Messenger.AddListener(Globals.MsgFightBreak, delegate
		{
			if (CheckProgress)
			{
				_fightStarted = false;
			}
		}));
		base._listeners.Add(Messenger<Person>.AddListener(Globals.MsgPersonDie, delegate(Person person)
		{
			if (person.Equals(Globals.Enemy) && _fightStarted && _lastEnemyHealth > 0 && _lastEnemyHealth <= _health)
			{
				IncProgress();
			}
			_fightStarted = false;
		}));
		base._listeners.Add(Messenger<int, int>.AddListener(Globals.MsgEnemyHealthChanged, delegate(int current, int max)
		{
			if (_fightStarted && current > 0)
			{
				_lastEnemyHealth = current;
			}
		}));
	}

	public override string GetInfo(string p)
	{
		return p.Fmt(base.MaxProgress, _health);
	}

	public override string ToString()
	{
		return base.ToString() + " " + _health;
	}
}
