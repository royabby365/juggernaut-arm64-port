using ProtoBuf;

[ProtoContract]
public class OpenConditionWinBattleWithRating : OpenCondition
{
	private readonly int _needRating;

	public OpenConditionWinBattleWithRating()
	{
	}

	public OpenConditionWinBattleWithRating(string[] args)
		: base(args[0])
	{
		_needRating = int.Parse(args[1]);
	}

	public override void Enable()
	{
		base.Enable();
		base._listeners.Add(Messenger<FightResultStats>.AddListener(Globals.MsgFightResult, delegate(FightResultStats _)
		{
			if (_.FightRating == _needRating)
			{
				IncProgress();
			}
		}));
	}

	public override string GetInfo(string p)
	{
		return p.Fmt(base.MaxProgress, _needRating);
	}

	public override string ToString()
	{
		return base.ToString() + " " + _needRating;
	}
}
