using ProtoBuf;

[ProtoContract]
public class OpenConditionCritsInRowNTimes : OpenCondition
{
	private bool _fightStarted;

	private int _count;

	[ProtoMember(3)]
	public int _crits;

	public OpenConditionCritsInRowNTimes()
	{
	}

	public OpenConditionCritsInRowNTimes(string[] args)
		: base(args[0])
	{
		_crits = int.Parse(args[1]);
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
			if (person == Globals.Enemy && _fightStarted && _count >= _crits)
			{
				IncProgress();
			}
			_fightStarted = false;
		}));
		base._listeners.Add(Messenger<Person, AttackE, ReactE, DamageTypeE>.AddListener(Globals.MsgPersonReact, delegate(Person person, AttackE attack, ReactE reaction, DamageTypeE damageType)
		{
			if (_fightStarted && person.Equals(Globals.Enemy))
			{
				if (attack == AttackE.Combo || (reaction.HasFlag(ReactE.Critical) && damageType == DamageTypeE.Natural))
				{
					_count++;
				}
				else if (_count < _crits && damageType != DamageTypeE.AcidMagic)
				{
					_count = 0;
				}
			}
		}));
	}

	public override string GetInfo(string p)
	{
		return p.Fmt(base.MaxProgress, _crits);
	}

	public override string ToString()
	{
		return base.ToString() + " " + _crits;
	}
}
