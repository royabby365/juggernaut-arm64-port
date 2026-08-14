using ProtoBuf;

[ProtoContract]
public class OpenConditionFatalitiesNTimes : OpenCondition
{
	private bool _fightStarted;

	public OpenConditionFatalitiesNTimes()
	{
	}

	public OpenConditionFatalitiesNTimes(string[] args)
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
			}
		}));
		base._listeners.Add(Messenger.AddListener(Globals.MsgFightBreak, delegate
		{
			if (CheckProgress)
			{
				_fightStarted = false;
			}
		}));
		base._listeners.Add(Messenger.AddListener(Globals.MsgFatalityExecuted, delegate
		{
			if (_fightStarted)
			{
				IncProgress();
			}
			_fightStarted = false;
		}));
	}
}
