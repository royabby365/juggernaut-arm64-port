using ProtoBuf;

[ProtoContract]
public class OpenConditionWinBattleWithHPFinished : OpenCondition
{
	private bool _fightStarted;

	[ProtoMember(3)]
	public int _health;

	public OpenConditionWinBattleWithHPFinished()
	{
	}

	public OpenConditionWinBattleWithHPFinished(string[] args)
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
			ServerData.Location openAfter = base.Location.OpenAfter;
			if ((openAfter == null || (openAfter != null && openAfter.Logic != null && openAfter.Logic.IsOpened)) && _fightStarted && person.Equals(Globals.Enemy))
			{
				Player player = Globals.Player;
				float num = (float)player.Health / (float)player.MaxHealth * 100f;
				if (num >= (float)_health)
				{
					IncProgress();
				}
			}
			_fightStarted = false;
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
