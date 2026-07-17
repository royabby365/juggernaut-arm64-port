using ProtoBuf;

[ProtoContract]
public class OpenConditionCastsNTimes : OpenCondition
{
	private string _op;

	private readonly int _maxCount;

	[ProtoMember(3)]
	public int _blocks;

	private bool _fightStarted;

	private int _count;

	public OpenConditionCastsNTimes()
	{
	}

	public OpenConditionCastsNTimes(string[] args)
		: base(args[0])
	{
		_maxCount = int.Parse(args[1]);
		_op = args[2];
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
			if (person.Equals(Globals.Enemy) && _fightStarted)
			{
				bool flag = false;
				if (_op == "l" && _count < _maxCount)
				{
					flag = true;
				}
				else if (_op == "g" && _count >= _maxCount)
				{
					flag = true;
				}
				else if (_op == "eq" && _count == _maxCount)
				{
					flag = true;
				}
				if (flag)
				{
					IncProgress();
				}
			}
			_fightStarted = false;
		}));
		base._listeners.Add(Messenger<MagicTypeE>.AddListener(Globals.MsgPlayerCastSpell, delegate
		{
			if (_fightStarted)
			{
				_count++;
			}
		}));
	}

	public override string GetInfo(string p)
	{
		return p.Fmt(base.MaxProgress, _maxCount);
	}

	public override string ToString()
	{
		return base.ToString() + " " + _maxCount;
	}
}
