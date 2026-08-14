using System;
using System.Text;
using ProtoBuf;
using Yarx;

[ProtoInclude(10700, typeof(OpenConditionWinBattleWithRating))]
[ProtoContract]
[ProtoInclude(10000, typeof(OpenConditionKillsWhenHPLessNTimes))]
[ProtoInclude(10100, typeof(OpenConditionCritsInRowNTimes))]
[ProtoInclude(10200, typeof(OpenConditionFatalitiesNTimes))]
[ProtoInclude(10300, typeof(OpenConditionWinBattleNoMisses))]
[ProtoInclude(10400, typeof(OpenConditionWinBattleWithHPFinished))]
[ProtoInclude(10500, typeof(OpenConditionWinBattleWithBlockNTimes))]
[ProtoInclude(10600, typeof(OpenConditionCastsNTimes))]
public class OpenCondition
{
	internal enum DoneReasonE
	{
		Buyed,
		ByCondition
	}

	private ServerData.Location _Location;

	private int _maxProgress = -1;

	[ProtoMember(2)]
	public bool Done;

	[ProtoMember(3)]
	public bool CheckProgress;

	[ProtoMember(4)]
	public int ServerId = -1;

	private static int Counter;

	private int _counter;

	public ServerData.Location Location
	{
		get
		{
			return _Location;
		}
		set
		{
			if (_Location != value)
			{
				if (_Location != null && _Location.Logic != null)
				{
					_Location.Logic.OpenCondition = this;
				}
				_Location = value;
			}
		}
	}

	[ProtoMember(1)]
	public int Progress { get; set; }

	public int MaxProgress
	{
		get
		{
			if (_maxProgress != -1)
			{
				return _maxProgress;
			}
			if (!SingletonT<ServerData>.I._conditions.ContainsKey(ServerId))
			{
				return -1;
			}
			return SingletonT<ServerData>.I._conditions[ServerId].Count;
		}
	}

	protected CompositeDisposable _listeners { get; private set; }

	protected OpenCondition()
	{
		Progress = 0;
		_counter = ++Counter;
	}

	protected OpenCondition(string max)
	{
		Progress = 0;
		_counter = ++Counter;
	}

	public static OpenCondition Create(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return null;
		}
		string text2 = array[0];
		string[] array2 = new string[array.Length - 1];
		if (array2.Length > 0)
		{
			Array.Copy(array, 1, array2, 0, array2.Length);
		}
		return text2 switch
		{
			"kills_when_hp_less" => new OpenConditionKillsWhenHPLessNTimes(array2), 
			"crits_in_row" => new OpenConditionCritsInRowNTimes(array2), 
			"fatalities" => new OpenConditionFatalitiesNTimes(array2), 
			"no_misses" => new OpenConditionWinBattleNoMisses(array2), 
			"win_hp_finished_more" => new OpenConditionWinBattleWithHPFinished(array2), 
			"win_with_blocks" => new OpenConditionWinBattleWithBlockNTimes(array2), 
			"casts_in_battle" => new OpenConditionCastsNTimes(array2), 
			"win_with_rating" => new OpenConditionWinBattleWithRating(array2), 
			_ => null, 
		};
	}

	internal static OpenCondition Create(ServerData.Condition condition)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (condition.Type == "test1")
		{
			stringBuilder.Append("no_misses " + condition.Count);
		}
		else if (condition.Type == "test2")
		{
			stringBuilder.Append("win_with_blocks {0} {1}".Fmt(condition.Count, condition.Params["blocksCount"]));
		}
		else if (condition.Type == "test3")
		{
			stringBuilder.Append("win_hp_finished_more {0} {1}".Fmt(condition.Count, condition.Params["hp"]));
		}
		else if (condition.Type == "test4")
		{
			stringBuilder.Append("fatalities " + condition.Count);
		}
		else if (condition.Type == "test5")
		{
			stringBuilder.Append("crits_in_row {0} {1}".Fmt(condition.Count, condition.Params["critsCount"]));
		}
		else if (condition.Type == "test6")
		{
			stringBuilder.Append("kills_when_hp_less {0} {1}".Fmt(condition.Count, condition.Params["enemyHealth"]));
		}
		else if (condition.Type == "test7")
		{
			stringBuilder.Append("casts_in_battle {0} {1} {2}".Fmt(condition.Count, condition.Params["count"], condition.Params["cond"]));
		}
		else if (condition.Type == "test8")
		{
			stringBuilder.Append("win_with_rating {0} {1}".Fmt(condition.Count, condition.Params["rating"]));
		}
		OpenCondition openCondition = Create(stringBuilder.ToString());
		if (openCondition == null)
		{
			Utils.Log("?????????", stringBuilder.ToString());
		}
		if (openCondition != null)
		{
			openCondition.ServerId = condition.Id;
		}
		return openCondition;
	}

	internal void SetDone(DoneReasonE reason)
	{
		if (!Done)
		{
			Utils.Log("CONDITION DONE", this, reason);
			Done = true;
			Messenger.Invoke(Globals.Msg_LocationOpenConditionDone, Location, reason);
		}
	}

	public override string ToString()
	{
		return base.ToString() + " _{5} {3} {0}/{1} {2} {4}".Fmt(Progress, MaxProgress, Done, ServerId, CheckProgress, _counter);
	}

	public virtual void Reset()
	{
		Done = false;
		Progress = 0;
		CheckProgress = false;
	}

	public virtual void Enable()
	{
		Utils.Log("OPENCONDITION ENABLE", Location.Title, this);
		if (_listeners == null)
		{
			_listeners = new CompositeDisposable();
		}
	}

	public virtual void Disable()
	{
		if (_listeners != null)
		{
			_listeners.Dispose();
			_listeners = null;
		}
	}

	protected void IncProgress()
	{
		if (!CheckProgress || Location == null || Done)
		{
			Utils.Log("DONTINCPROGRESS", this, Done, CheckProgress, Location);
			return;
		}
		Progress++;
		Utils.Log("INCPROGRESS", this, "Location=", Location, Progress, MaxProgress, Done);
		if (Progress >= MaxProgress)
		{
			SetDone(DoneReasonE.ByCondition);
			Messenger.Invoke(Globals.Msg_LocationOpenConditionProgressChanged, Location.Id);
		}
		else if (!Done)
		{
			Messenger.Invoke(Globals.Msg_LocationOpenConditionProgressChanged, Location.Id);
		}
	}

	protected void DebugSetMaxProgress()
	{
		CheckProgress = true;
		while (Progress < MaxProgress)
		{
			IncProgress();
		}
	}

	public bool CopyProgressFrom(OpenCondition condition)
	{
		if (condition.GetType() != GetType())
		{
			return false;
		}
		if (condition.ServerId != ServerId)
		{
			return false;
		}
		Progress = condition.Progress;
		CheckProgress = condition.CheckProgress;
		Done = condition.Done;
		if (Progress >= MaxProgress)
		{
			Progress = MaxProgress;
		}
		if (!Done && CheckProgress && Progress == MaxProgress)
		{
			Done = true;
		}
		return true;
	}

	public virtual string GetInfo(string p)
	{
		return p.Fmt(MaxProgress);
	}
}
