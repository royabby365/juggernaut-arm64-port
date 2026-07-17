using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class LocationLogic
{
	[ProtoContract]
	public class ChestOnLocation
	{
		public enum ElfStateE
		{
			NotGeneratedYet,
			GeneratedFalse,
			GeneratedTrue,
			Killed
		}

		private static int _iniqueId;

		[ProtoMember(1)]
		public int ServerId = -1;

		[ProtoMember(2)]
		public int X;

		[ProtoMember(3)]
		public int Y;

		[ProtoMember(4)]
		public bool WasFound;

		[ProtoMember(5, IsRequired = false)]
		public ElfStateE ElfState;

		public DateTime StartDate;

		public Rect RectOnLocation;

		public int InstanceId { get; private set; }

		public ServerData.Chest Chest => SingletonT<ServerData>.I.Chests[ServerId];

		public ChestOnLocation()
		{
			X = -1;
			Y = -1;
			InstanceId = _iniqueId++;
			StartDate = DateTime.Now;
		}

		public override string ToString()
		{
			return $"X: {X}, Y: {Y}, InstanceId: {InstanceId}, ServerId: {ServerId}";
		}
	}

	private static readonly DateTime NullDate = DateTime.MinValue;

	private readonly ServerData.Location _location;

	[ProtoMember(1)]
	public DateTime _lastPopulationCountUpdateDate = NullDate;

	public float _lastPopulationCountUpdateRealTime;

	[ProtoMember(2)]
	public DateTime _lastMoneyUpdateDate = NullDate;

	public float _lastMoneyUpdateRealTime;

	[ProtoMember(3)]
	public DateTime _lastMobCountUpdateDate = NullDate;

	public float _lastMobCountUpdateRealTime;

	[ProtoMember(13)]
	public DateTime _lastElfsUpdateDate = NullDate;

	public float _lastElfsUpdateRealTime;

	[ProtoMember(4)]
	public DateTime _lastMobKillDate = NullDate;

	public float _lastMobKillRealTime;

	[ProtoMember(5)]
	public int _money;

	[ProtoMember(6)]
	public int _population;

	[ProtoMember(7)]
	public bool IsOpened;

	[ProtoMember(8)]
	public int ZachistkaMobsKilled;

	[ProtoMember(9)]
	public List<ServerData.BotInfo> _mobs = new List<ServerData.BotInfo>();

	public int MobInFight = -1;

	[ProtoMember(10)]
	public DateTime _lastChestDate = NullDate;

	public float _lastChestRealTime;

	[ProtoMember(11)]
	public List<ChestOnLocation> ChestsOnLocation = new List<ChestOnLocation>();

	[ProtoMember(12)]
	public OpenCondition OpenCondition;

	internal static ServerData.Bonus.DropElement _chapterBonus;

	private static int Counter = 0;

	private int _counter;

	private static bool _debug1 = false;

	public ServerData.Location MyLocation => _location;

	public int Money
	{
		get
		{
			return _money;
		}
		set
		{
			int money = _money;
			_money = value;
			Messenger<ServerData.Location, int, int>.Invoke(Globals.MsgLocationMoneyChanged, _location, money, value);
		}
	}

	public int Population
	{
		get
		{
			return _population;
		}
		set
		{
			_population = value;
			Messenger<ServerData.Location, int, bool>.Invoke(Globals.MsgLocationPopulationChanged, _location, value, _population > value);
		}
	}

	public LocationLogic(ServerData.Location location)
	{
		_location = location;
		_counter = ++Counter;
	}

	public LocationLogic()
	{
		_counter = ++Counter;
	}

	private void AddMob(ServerData.BotInfo bot)
	{
		_mobs.Add(bot);
		Messenger<ServerData.Location, int>.Invoke(Globals.MsgLocationMobsAdded, _location, _mobs.Count - 1);
	}

	internal void RecreateOpenCondition()
	{
		if (OpenCondition != null)
		{
			OpenCondition.Disable();
			OpenCondition = null;
		}
		ServerData.Location locationByServerId = SingletonT<ServerData>.I.GetLocationByServerId(_location.Id);
		if (locationByServerId != null && locationByServerId.Condition != null)
		{
			OpenCondition openCondition = OpenCondition.Create(locationByServerId.Condition);
			if (openCondition != null)
			{
				openCondition.Location = locationByServerId;
				openCondition.Enable();
				OpenCondition = openCondition;
			}
		}
	}

	internal void CopyFromLoadedGame(LocationLogic loaded)
	{
		_lastMobCountUpdateDate = loaded._lastMobCountUpdateDate;
		_lastMobKillDate = loaded._lastMobKillDate;
		_lastMoneyUpdateDate = loaded._lastMoneyUpdateDate;
		_lastPopulationCountUpdateDate = loaded._lastPopulationCountUpdateDate;
		_lastChestDate = loaded._lastChestDate;
		ChestsOnLocation = loaded.ChestsOnLocation;
		if (ChestsOnLocation == null)
		{
			ChestsOnLocation = new List<ChestOnLocation>();
		}
		Money = loaded._money;
		Population = loaded._population;
		if (OpenCondition != null)
		{
			OpenCondition.Disable();
		}
		if (OpenCondition != null && loaded.OpenCondition != null && OpenCondition.ServerId == loaded.OpenCondition.ServerId)
		{
			OpenCondition.Disable();
			OpenCondition.CopyProgressFrom(loaded.OpenCondition);
		}
		if (OpenCondition != null)
		{
			OpenCondition.Location = loaded._location;
			OpenCondition.Enable();
		}
		IsOpened = loaded.IsOpened;
		ZachistkaMobsKilled = loaded.ZachistkaMobsKilled;
		if (ZachistkaMobsKilled < 0)
		{
			ZachistkaMobsKilled = 0;
		}
		_mobs = new List<ServerData.BotInfo>();
		if (loaded._mobs == null)
		{
			return;
		}
		foreach (ServerData.BotInfo mob in loaded._mobs)
		{
			ServerData.BotInfo botInfoByServerId = SingletonT<ServerData>.I.GetBotInfoByServerId(mob.Id);
			if (botInfoByServerId != null)
			{
				botInfoByServerId.Level = mob.Level;
				botInfoByServerId.PeopleKilledForLevelUp = mob.PeopleKilledForLevelUp;
				botInfoByServerId.MaxLevel = mob.MaxLevel;
				AddMob(botInfoByServerId);
			}
		}
	}

	internal void MobFromFightDie()
	{
		Utils.Log("MobFromFightDie", MobInFight, _mobs.Count);
		if (MobInFight >= 0 && MobInFight < _mobs.Count)
		{
			_mobs.RemoveAt(MobInFight);
			if (GetElf() == null)
			{
				foreach (ChestOnLocation item in ChestsOnLocation)
				{
					if (item.WasFound && item.ElfState == ChestOnLocation.ElfStateE.GeneratedTrue)
					{
						item.ElfState = ChestOnLocation.ElfStateE.Killed;
					}
				}
			}
			Messenger<ServerData.Location>.Invoke(Globals.MsgLocationMobsRemoved, _location);
		}
		MobInFight = -1;
	}

	public ServerData.BotInfo GetElf()
	{
		if (ChestsOnLocation.Count > 0)
		{
			int elfModel = ChestsOnLocation[0].Chest.ElfModel;
			foreach (ServerData.BotInfo mob in _mobs)
			{
				if (mob.Id == SingletonT<ServerData>.I.GameSettings.Elfs[elfModel].Bot.Id)
				{
					return mob;
				}
			}
		}
		return null;
	}

	public void IncZachistkaMobsKilled()
	{
		ZachistkaMobsKilled++;
		Utils.LogForce("~~~~~~~~~~~~IncZachistkaMobsKilled");
		bool flag = false;
		if (ZachistkaMobsKilled >= _location.Bots.Length)
		{
			if (!IsOpened)
			{
				GenChests(SingletonT<ServerData>.I.GameSettings.ChestsMaxCount);
				SingletonT<ServerData>.I.ZachistkaDone(_location);
				Population = 2;
				flag = true;
				if (_location != null && _location.Bonus != null && _location.Bonus.Drop != null && _location.Bonus.Drop.Count > 0)
				{
					_chapterBonus = _location.Bonus.Drop[0].MakeDrop();
				}
			}
			IsOpened = true;
			SingletonT<ServerData>.I.UpdateLiveShamansCount();
			_lastMobKillDate = (_lastMobCountUpdateDate = (_lastMoneyUpdateDate = (_lastPopulationCountUpdateDate = DateTime.Now)));
		}
		if (flag)
		{
			Messenger.Invoke(Globals.MsgZachistkaDone, _location);
		}
		Messenger.Invoke(Globals.MsgZachistkaProgressChanged, _location);
	}

	internal void Reset()
	{
		IsOpened = false;
		ZachistkaMobsKilled = 0;
		Population = 0;
		_mobs.Clear();
		Money = 0;
		ChestsOnLocation = new List<ChestOnLocation>();
		if (OpenCondition != null)
		{
			OpenCondition.Reset();
		}
		_lastChestDate = NullDate;
		_lastMobKillDate = NullDate;
		_lastMoneyUpdateDate = NullDate;
		_lastMoneyUpdateDate = NullDate;
		_lastMobCountUpdateDate = NullDate;
	}

	public void Update(float deltaTime, DateTime now)
	{
		if (_location == null || !IsOpened || (Globals.LocationLogicNoOfflineTime && (Globals.GameScreen == Globals.GameScreenE.StartMenu || Globals.GameScreen == Globals.GameScreenE.SelectPlayer)))
		{
			return;
		}
		UpdateTimer("MONEY", _location.MoneyPeriod, ref _lastMoneyUpdateRealTime, deltaTime, now, ref _lastMoneyUpdateDate, delegate(int count)
		{
			int money = Money;
			if (_location.MoneyMax > 10000 && !_debug1)
			{
				_debug1 = true;
				Debug.Log("!!!!!!!!!!!!!! MMMMMMMMMONEY " + _location.MoneyMax);
			}
			Money = Math.Min(_location.MoneyMax, _money + _population * count * _location.MoneyPerPerson);
			if (money != Money)
			{
				Utils.LogFrom("GROW MONEY", _location.Id, _location.MoneyPeriod, money, "->", Money, "Count", count, "PerPerson", _location.MoneyPerPerson, "Perses", _population);
			}
		});
		UpdateTimer("POPSCOUNT", _location.PopulationPeriodSeconds, ref _lastPopulationCountUpdateRealTime, deltaTime, now, ref _lastPopulationCountUpdateDate, delegate(int count)
		{
			int population = Population;
			Population = Math.Min(_population + count * _location.PopulationPointsUp, _location.PopulationMax);
			if (population != Population)
			{
				Utils.LogFrom("LocationLogic", "Grow pops", now, population, "->", Population, _location.PopulationPeriodSeconds, "Cycles", count, "Up", _location.PopulationPointsUp, "Max", _location.PopulationMax);
			}
		});
		ServerData.Settings gameSettings = SingletonT<ServerData>.I.GameSettings;
		if (gameSettings == null || _location.IsOpened)
		{
		}
		UpdateTimer("MOBSKILL", _location.RespawnKillPeriodSeconds, ref _lastMobKillRealTime, deltaTime, now, ref _lastMobKillDate, delegate(int count)
		{
			if (_mobs.Count != 0)
			{
				int population = Population;
				Population = Math.Max(_population - _location.RespawnKill * _mobs.Count * count, 0);
				if (population != Population)
				{
					foreach (ServerData.BotInfo mob in _mobs)
					{
						if (!SingletonT<ServerData>.I.GameSettings.IsElf(mob) && mob.Level < SingletonT<ServerData>.I.GameSettings.LocationMobLevelMax)
						{
							mob.PeopleKilledForLevelUp += _location.RespawnKill * count;
							while (mob.PeopleKilledForLevelUp >= SingletonT<ServerData>.I.GameSettings.LocationMobLevelUpCost)
							{
								mob.PeopleKilledForLevelUp -= SingletonT<ServerData>.I.GameSettings.LocationMobLevelUpCost;
								mob.Level++;
								int num = Mathf.Min(SingletonT<ServerData>.I.GameSettings.LocationMobLevelMax, mob.MaxLevel);
								if (mob.Level > num)
								{
									mob.Level = num;
								}
							}
						}
					}
					Utils.LogFrom("LocationLogic", "Mob kill", now, population, "->", Population, _location.RespawnKillPeriodSeconds, "OneKills", _location.RespawnKill, "MobsCount", _mobs.Count, "Cycles", count);
				}
			}
		});
	}

	private void GenChests(int count)
	{
		if (_location.Chests.Length <= 0)
		{
			return;
		}
		ServerData.Settings gameSettings = SingletonT<ServerData>.I.GameSettings;
		for (int i = 0; i < count; i++)
		{
			if (ChestsOnLocation.Count < gameSettings.ChestsMaxCount)
			{
				Utils.Random(_location.Chests, (ServerData.Location.ChestLocationInfo _) => (!Globals.DebugShortPeriods) ? _.Prob : 100, 1, allowDuplicates: true, delegate(int __, int chestI)
				{
					Utils.Log("GEN CHEST", _location.Title, _location.Chests.Length, ChestsOnLocation.Count, gameSettings.ChestsMaxCount, _location.Chests[chestI].Chest.Id);
					ChestsOnLocation.Add(new ChestOnLocation
					{
						ServerId = _location.Chests[chestI].Chest.Id
					});
					Messenger<LocationLogic>.Invoke(Globals.Msg_ChestOnLocationAdded, this);
				});
			}
		}
	}

	internal void RespawnMob()
	{
		ServerData.BotInfo randomBot = GetRandomBot();
		if (randomBot != null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("LOCATION LOGIC: RespawnMob");
			}
			AddMob(randomBot);
		}
		else
		{
			Debug.Log("[FUCKUP] -- cannog get random bot at all!");
		}
	}

	private ServerData.BotInfo GetRandomBot()
	{
		List<ServerData.BotInfo> botsWithoutBosses = new List<ServerData.BotInfo>(_location.Bots.Length);
		ServerData.Location.BotLocationInfo[] bots = _location.Bots;
		foreach (ServerData.Location.BotLocationInfo botLocationInfo in bots)
		{
			if (!botLocationInfo.IsBoss)
			{
				botsWithoutBosses.Add(botLocationInfo.Bot);
			}
		}
		if (botsWithoutBosses.Count == 0)
		{
			return null;
		}
		List<int> list = new List<int>(botsWithoutBosses.Count);
		foreach (ServerData.BotInfo item in botsWithoutBosses)
		{
			if (_mobs.Contains(item))
			{
				list.Add(1);
			}
			else
			{
				list.Add(50);
			}
		}
		ServerData.BotInfo r = null;
		Utils.Random(list, 1, allowDuplicates: false, delegate(int __, int index)
		{
			r = botsWithoutBosses[index];
		});
		GenerateMobLevel(r);
		return r;
	}

	public static void GenerateMobLevel(ServerData.BotInfo bot)
	{
		int num = 5;
		int num2 = 5;
		foreach (KeyValuePair<int, ServerData.Location> location in SingletonT<ServerData>.I._locations)
		{
			if (!location.Value.Logic.IsOpened && location.Value.Logic.ZachistkaMobsKilled <= 0)
			{
				continue;
			}
			int num3 = Mathf.Min(location.Value.Logic.ZachistkaMobsKilled, location.Value.Bots.Length);
			for (int i = 0; i < num3; i++)
			{
				ServerData.Location.BotLocationInfo botLocationInfo = location.Value.Bots[i];
				if (!botLocationInfo.IsBoss && botLocationInfo.Level > num2)
				{
					num2 = botLocationInfo.Level;
				}
			}
		}
		int b = num2 - SingletonT<ServerData>.I.GameSettings.LocationMobLevel;
		num = Mathf.Max(5, b);
		if (num > SingletonT<ServerData>.I.GameSettings.LocationMobLevelMax)
		{
			num = SingletonT<ServerData>.I.GameSettings.LocationMobLevelMax;
		}
		bot.Level = num;
		int a = num2 - SingletonT<ServerData>.I.GameSettings.LocationMobLevelOffset;
		a = Mathf.Max(a, num);
		bot.MaxLevel = Mathf.Min(a, SingletonT<ServerData>.I.GameSettings.LocationMobLevelMax);
	}

	private void UpdateTimer(string name, int periodSeconds, ref float realTimeAccum, float realDeltaTime, DateTime now, ref DateTime _store, ActionD<int> action)
	{
		if (periodSeconds <= 0)
		{
			return;
		}
		if (_store == NullDate)
		{
			realDeltaTime = 0f;
			realTimeAccum = 0f;
			_store = now;
		}
		if (Globals.LocationLogicNoOfflineTime)
		{
			if (realDeltaTime < 0f)
			{
				realDeltaTime = 0f;
			}
			else if (realDeltaTime > 0.5f)
			{
				realDeltaTime = 0.5f;
			}
			realTimeAccum += realDeltaTime;
			if (realTimeAccum > (float)periodSeconds)
			{
				float num = realTimeAccum / (float)periodSeconds;
				realTimeAccum -= (float)periodSeconds * num;
				action((int)num);
			}
		}
		else
		{
			int num2 = (int)now.Subtract(_store).TotalSeconds;
			if (num2 > periodSeconds)
			{
				int v = num2 / periodSeconds;
				action(v);
				_store = now.Subtract(new TimeSpan(0, 0, num2 % periodSeconds));
			}
		}
	}

	public override string ToString()
	{
		return "<<_{0} {1} OpenCond={1} Chets={2} Mobs={3}>>".Fmt(_counter, ZachistkaMobsKilled, (OpenCondition == null) ? "NULL" : OpenCondition.ToString(), Utils.ParamsToString(ChestsOnLocation.ToArray(), " [", "]"), Utils.ParamsToString(_mobs.ToArray(), " [", "]"));
	}
}
