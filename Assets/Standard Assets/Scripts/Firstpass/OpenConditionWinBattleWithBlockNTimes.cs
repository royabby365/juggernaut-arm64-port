using ProtoBuf;

[ProtoContract]
public class OpenConditionWinBattleWithBlockNTimes : OpenCondition
{
	[ProtoMember(3)]
	public int _blocks;

	private bool _fightStarted;

	private int _count;

	public OpenConditionWinBattleWithBlockNTimes()
	{
	}

	public OpenConditionWinBattleWithBlockNTimes(string[] args)
		: base(args[0])
	{
		_blocks = int.Parse(args[1]);
	}

	public override void Enable()
	{
		base.Enable();
		base._listeners.Add(Messenger.AddListener(Globals.MsgFightStarted, delegate
		{
			if (CheckProgress)
			{
				_fightStarted = true;
				_count = 0;
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
			if (person.Equals(Globals.Enemy) && _fightStarted && _count >= _blocks)
			{
				IncProgress();
			}
			_fightStarted = false;
		}));
		base._listeners.Add(Messenger<Person, AttackE, ReactE, DamageTypeE>.AddListener(Globals.MsgPersonReact, delegate(Person person, AttackE attack, ReactE reaction, DamageTypeE damageType)
		{
			if (_fightStarted && reaction.HasFlag(ReactE.Block) && person.Equals(Globals.Player))
			{
				_count++;
			}
		}));
	}

	public override string GetInfo(string p)
	{
		return p.Fmt(base.MaxProgress, _blocks);
	}

	public override string ToString()
	{
		return base.ToString() + " " + _blocks;
	}
}
