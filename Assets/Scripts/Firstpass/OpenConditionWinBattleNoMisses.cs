using ProtoBuf;

[ProtoContract]
public class OpenConditionWinBattleNoMisses : OpenCondition
{
	private bool _fightStarted;

	private bool _fightFailed;

	public OpenConditionWinBattleNoMisses()
	{
	}

	public OpenConditionWinBattleNoMisses(string[] args)
		: base(args[0])
	{
	}

	public override void Enable()
	{
		base.Enable();
		base._listeners.Add(Messenger.AddListener(Globals.MsgFightStarted, delegate
		{
			if (CheckProgress)
			{
				_fightStarted = true;
				_fightFailed = false;
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
			if (person.Equals(Globals.Enemy) && !_fightFailed && _fightStarted)
			{
				IncProgress();
			}
			_fightStarted = false;
		}));
		base._listeners.Add(Messenger<Person, AttackE, ReactE, DamageTypeE>.AddListener(Globals.MsgPersonReact, delegate(Person person, AttackE attack, ReactE reaction, DamageTypeE damageType)
		{
			if (_fightStarted && (reaction.HasFlag(ReactE.Block) || reaction.HasFlag(ReactE.Dodge)) && person.Equals(Globals.Enemy))
			{
				_fightFailed = true;
			}
		}));
	}
}
