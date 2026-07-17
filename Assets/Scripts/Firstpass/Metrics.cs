using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx.Collections;

internal class Metrics
{
	private static Dictionary<int, int> _fightSpellsUsed;

	private static int _fightCombos;

	private static int _fightRageBlocks;

	private static int _fightRageCrits;

	private static int _fightRageBerserk;

	private static int _fightUsedElixirHealth;

	private static int _fightUsedElixirForce;

	private static int _fightUsedElixirPoison;

	private static int _fightEyeEatsRage;

	private static int _fightAttackBlocks;

	private static int _fightAttackDodge;

	private static int _fightAttackDone;

	private static int PlayerLevel
	{
		get
		{
			if (SingletonT<ServerData>.I.PlayerParams != null)
			{
				return SingletonT<ServerData>.I.PlayerParams.Level;
			}
			return 0;
		}
	}

	static Metrics()
	{
		_fightSpellsUsed = new Dictionary<int, int>();
		_fightCombos = 0;
		_fightRageBlocks = 0;
		_fightRageCrits = 0;
		_fightRageBerserk = 0;
		_fightUsedElixirHealth = 0;
		_fightUsedElixirForce = 0;
		_fightUsedElixirPoison = 0;
		_fightEyeEatsRage = 0;
		_fightAttackBlocks = 0;
		_fightAttackDodge = 0;
		_fightAttackDone = 0;
		Messenger<int, int>.AddListener(Globals.Msg_ChestOpened, delegate(int type, int used)
		{
			OnChestUsedLockpicks(used, type);
		});
		Messenger<int>.AddListener(Globals.Msg_GotoChestGameFromLocation, delegate(int type)
		{
			OnChestOpened(type);
		});
		Messenger<int, float, int>.AddListener(Globals.MsgMineGameStats, delegate(int mineId, float gameTime, int lockpicksUsed)
		{
			if (gameTime > 0f)
			{
				UnityApi.AddMetric(155, (int)gameTime, SingletonT<ServerData>.I.PlayerParams.Level, mineId);
				if (lockpicksUsed > 0)
				{
					UnityApi.AddMetric(156, lockpicksUsed, SingletonT<ServerData>.I.PlayerParams.Level, mineId);
				}
			}
		});
		Messenger<ServerData.Item>.AddListener(Globals.MsgItemFoundInDrop, delegate(ServerData.Item _)
		{
			if (_.ElixirType == ServerData.Item.ElixirTypeE.Gold)
			{
				UnityApi.AddMetric(149, _.RealItemsCount, SingletonT<ServerData>.I.PlayerParams.Level, 1);
			}
			else if (_.ElixirType == ServerData.Item.ElixirTypeE.Diamond)
			{
				UnityApi.AddMetric(149, _.RealItemsCount, SingletonT<ServerData>.I.PlayerParams.Level, 2);
			}
			else
			{
				UnityApi.AddMetric(150, 1, SingletonT<ServerData>.I.PlayerParams.Level, _.Id);
			}
		});
		Messenger.AddListener(Globals.MsgFightStarted, ResetFightState);
		Messenger<ServerData.Item, ServerData.MoneyType.TypeE>.AddListener(Globals.MsgItemBuyInShop, OnBuyInShop);
		Messenger<ServerData.Location>.AddListener(Globals.MsgZachistkaDone, OnLocationDone);
		Messenger<ServerData.Location, OpenCondition.DoneReasonE>.AddListener(Globals.Msg_LocationOpenConditionDone, delegate(ServerData.Location location, OpenCondition.DoneReasonE _)
		{
			OnUnlockChapter(location, _);
		});
		Messenger<int>.AddListener(Globals.MsgDropChestSelected, delegate(int _)
		{
			UnityApi.AddMetric(147, 1, SingletonT<ServerData>.I.PlayerParams.Level, _);
		});
		Messenger<ServerData.Spell>.AddListener(Globals.MsgPlayerSpellBuyed, delegate(ServerData.Spell spell)
		{
			UnityApi.AddMetric(101, 1, SingletonT<ServerData>.I.PlayerParams.Level, spell.Id);
		});
	}

	public static void TrySendDayMetrics()
	{
	}

	private static IEnumerator AddMetricsCoro(Stack<Tuple<int, int, int, int>> metrics)
	{
		float period = 0.5f;
		float currentTime = period;
		while (metrics.Count > 0)
		{
			if (currentTime >= period)
			{
				currentTime = 0f;
				Tuple<int, int, int, int> metric = metrics.Pop();
				UnityApi.AddMetric(metric.Item1, metric.Item2, metric.Item3, metric.Item4);
			}
			currentTime += Time.deltaTime;
			yield return null;
		}
	}

	private static void AddInt(Dictionary<int, int> dict, int count)
	{
		if (count > 0)
		{
			int playerLevel = PlayerLevel;
			int value = 0;
			if (dict.TryGetValue(playerLevel, out value))
			{
				dict[playerLevel] = value + count;
			}
			else
			{
				dict.Add(playerLevel, count);
			}
		}
	}

	internal static void OnSpendGold(int count)
	{
		UnityApi.AddMetric(90, count, PlayerLevel, 0);
	}

	internal static void OnSpendDiamond(int count)
	{
		UnityApi.AddMetric(91, count, PlayerLevel, 0);
	}

	internal static void OnSpendSkull(int count)
	{
		UnityApi.AddMetric(105, count, SingletonT<ServerData>.I.PlayerParams.Level, 0);
	}

	private static void ResetFightState()
	{
		_fightSpellsUsed.Clear();
		_fightCombos = 0;
		_fightRageBlocks = 0;
		_fightRageCrits = 0;
		_fightRageBerserk = 0;
		_fightUsedElixirHealth = 0;
		_fightUsedElixirForce = 0;
		_fightUsedElixirPoison = 0;
		_fightEyeEatsRage = 0;
		_fightAttackBlocks = 0;
		_fightAttackDodge = 0;
		_fightAttackDone = 0;
	}

	public static void OnFightFinish(bool victory, int playerLevel, ServerData.BotInfo bot, bool onLocation, Battle.FatalityStateE fatality, int steps)
	{
		Utils.Log("********", victory, playerLevel, bot, onLocation, fatality, steps);
		UnityApi.AddMetric((!onLocation) ? 109 : 110, victory ? 1 : 0, playerLevel, bot.Id);
		if (victory)
		{
			UnityApi.AddMetric(114, steps, playerLevel, bot.Id);
		}
		if (fatality != Battle.FatalityStateE.None)
		{
			UnityApi.AddMetric(113, (fatality == Battle.FatalityStateE.Executed) ? 1 : 0, playerLevel, bot.Id);
		}
		if (_fightCombos > 0)
		{
			UnityApi.AddMetric(112, _fightCombos, playerLevel, bot.Id);
		}
		if (_fightRageBlocks > 0)
		{
			UnityApi.AddMetric(117, _fightRageBlocks, playerLevel, bot.Id);
		}
		if (_fightRageCrits > 0)
		{
			UnityApi.AddMetric(118, _fightRageCrits, playerLevel, bot.Id);
		}
		if (_fightRageBerserk > 0)
		{
			UnityApi.AddMetric(119, _fightRageBerserk, playerLevel, bot.Id);
		}
		if (_fightUsedElixirHealth > 0)
		{
			UnityApi.AddMetric(120, _fightUsedElixirHealth, playerLevel, bot.Id);
		}
		if (_fightUsedElixirForce > 0)
		{
			UnityApi.AddMetric(121, _fightUsedElixirForce, playerLevel, bot.Id);
		}
		if (_fightUsedElixirPoison > 0)
		{
			UnityApi.AddMetric(122, _fightUsedElixirPoison, playerLevel, bot.Id);
		}
		if (_fightEyeEatsRage > 0)
		{
			UnityApi.AddMetric(123, _fightEyeEatsRage, playerLevel, bot.Id);
		}
		if (_fightAttackBlocks > 0)
		{
			UnityApi.AddMetric(146, _fightAttackBlocks, 2, bot.Id);
		}
		if (_fightAttackDodge > 0)
		{
			UnityApi.AddMetric(146, _fightAttackDodge, 3, bot.Id);
		}
		if (_fightAttackDone > 0)
		{
			UnityApi.AddMetric(146, _fightAttackDone, 1, bot.Id);
		}
		foreach (KeyValuePair<int, int> item in _fightSpellsUsed)
		{
			if (item.Value > 0)
			{
				UnityApi.AddMetric(104, item.Value, playerLevel, item.Key);
			}
		}
	}

	public static void OnBuyMine(ServerData.Mine mine)
	{
		UnityApi.AddMetric(154, 1, SingletonT<ServerData>.I.PlayerParams.Level, mine.Id);
	}

	public static void OnNewLevel()
	{
		int level = SingletonT<ServerData>.I.PlayerParams.Level;
		UnityApi.AddMetric(1, 1, level, level);
	}

	public static void OnNewGame()
	{
		UnityApi.AddMetric(3, 1, 1, SingletonT<ServerData>.I.PlayerServerPersData.Id);
		UnityApi.AddMetric(1, 1, 1, 1);
	}

	public static void OnLocationDone(ServerData.Location location)
	{
		UnityApi.AddMetric(92, 1, SingletonT<ServerData>.I.PlayerParams.Level, location.Id);
	}

	public static void OnUnlockChapter(ServerData.Location location, OpenCondition.DoneReasonE reason)
	{
		switch (reason)
		{
		case OpenCondition.DoneReasonE.ByCondition:
			UnityApi.AddMetric(93, 1, SingletonT<ServerData>.I.PlayerParams.Level, location.Id);
			break;
		case OpenCondition.DoneReasonE.Buyed:
			UnityApi.AddMetric(151, 1, SingletonT<ServerData>.I.PlayerParams.Level, location.Id);
			break;
		}
	}

	public static void OnBuyInShop(ServerData.Item item, ServerData.MoneyType.TypeE type)
	{
		switch (type)
		{
		case ServerData.MoneyType.TypeE.Gold:
			UnityApi.AddMetric(153, 1, SingletonT<ServerData>.I.PlayerParams.Level, item.Id);
			break;
		case ServerData.MoneyType.TypeE.Diamond:
			UnityApi.AddMetric(152, 1, SingletonT<ServerData>.I.PlayerParams.Level, item.Id);
			break;
		default:
			UnityApi.AddMetric(95, 1, SingletonT<ServerData>.I.PlayerParams.Level, item.Id);
			break;
		}
	}

	public static void OnBuyInBunk(ServerData.BankItem item)
	{
		if (item != null)
		{
			UnityApi.AddMetric(96, Mathf.RoundToInt(item.Real * 100f), SingletonT<ServerData>.I.PlayerParams.Level, item.Id);
		}
	}

	public static void OnPlayerCastSpell(ServerData.Spell spell)
	{
		int id = spell.Id;
		if (!_fightSpellsUsed.ContainsKey(id))
		{
			_fightSpellsUsed[id] = 1;
		}
		else
		{
			_fightSpellsUsed[id] += 1;
		}
	}

	public static void OnPlayerCombo()
	{
		_fightCombos++;
	}

	public static void OnPlayerRageBlock()
	{
		_fightRageBlocks++;
	}

	public static void OnPlayerRageCrit()
	{
		_fightRageCrits++;
	}

	public static void OnPlayerRageBerserk()
	{
		_fightRageBerserk++;
	}

	public static void OnPlayerUseElixirHealth()
	{
		_fightUsedElixirHealth++;
	}

	public static void OnPlayerUseElixirCrit()
	{
		_fightUsedElixirForce++;
	}

	public static void OnPlayerUseElixirPoison()
	{
		_fightUsedElixirPoison++;
	}

	public static void OnEyeEatRage()
	{
		_fightEyeEatsRage++;
	}

	public static void OnScarabUsed()
	{
		UnityApi.AddMetric(130, 1, SingletonT<ServerData>.I.PlayerParams.Level, 0);
	}

	public static void OnChestUsedLockpicks(int count, int diff)
	{
		UnityApi.AddMetric(132, count, SingletonT<ServerData>.I.PlayerParams.Level, diff);
	}

	public static void OnChestOpened(int type)
	{
		UnityApi.AddMetric(133, 1, SingletonT<ServerData>.I.PlayerParams.Level, type);
	}

	public static void OnAchievement(ServerData.Achievement achivement)
	{
		UnityApi.AddMetric(136, 1, SingletonT<ServerData>.I.PlayerParams.Level, achivement.Id);
	}

	public static void OnTapJoy()
	{
		UnityApi.AddMetric(142, 1, SingletonT<ServerData>.I.PlayerParams.Level, 0);
	}

	public static void OnPostToSocialClicked()
	{
		UnityApi.AddMetric(143, 1, SingletonT<ServerData>.I.PlayerParams.Level, 0);
	}

	public static void OnPostToFacebookDone()
	{
		UnityApi.AddMetric(143, 1, SingletonT<ServerData>.I.PlayerParams.Level, 1);
	}

	public static void OnPostToTwitterDone()
	{
		UnityApi.AddMetric(143, 1, SingletonT<ServerData>.I.PlayerParams.Level, 2);
	}

	public static void OnAttack(ReactE react)
	{
		if (SingletonT<ServerData>.I.PlayerParams.Level <= 2)
		{
			if (react == ReactE.Block)
			{
				_fightAttackBlocks++;
			}
			if (react == ReactE.Dodge)
			{
				_fightAttackDodge++;
			}
			if (react == ReactE.Damage)
			{
				_fightAttackDone++;
			}
		}
	}
}
