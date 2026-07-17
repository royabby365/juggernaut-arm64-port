using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Yarx;

public class GameEvents : IDisposable
{
	public class Event : IDisposable
	{
		internal ServerData.Achievement Achievement;

		private int _Progress;

		internal int MaxProgress = 1;

		protected CompositeDisposable _listeners = new CompositeDisposable();

		protected bool _lazySendChanged;

		protected bool _lazyProgressChanged;

		internal int Progress => _Progress;

		internal double ProgressDouble => (Progress >= MaxProgress) ? 100.0 : ((double)Progress / (double)MaxProgress * 100.0);

		internal void SetProgress(int value, string reason)
		{
			if (value != _Progress)
			{
				Utils.Log("GAMEEVENT.SetProgress", reason, value, this);
				_Progress = value;
			}
		}

		protected void ProgressChanged()
		{
			Utils.Log("ProgressChanged", this);
			if (Progress == MaxProgress && Achievement != null)
			{
				Metrics.OnAchievement(Achievement);
			}
			Messenger.Invoke(Globals.MsgGameEventProgressChanged, this, "ProgressChanged");
		}

		protected void IncProgress(int count)
		{
			if (Progress < MaxProgress)
			{
				int num = Progress + count;
				if (num > MaxProgress)
				{
					num = MaxProgress;
				}
				SetProgress(num, "IncProgres");
				if (!_lazySendChanged)
				{
					ProgressChanged();
				}
				else
				{
					_lazyProgressChanged = true;
				}
			}
		}

		protected void IncProgress()
		{
			IncProgress(1);
		}

		public void Dispose()
		{
			Achievement = null;
			Utils.Dispose(ref _listeners);
		}

		public override string ToString()
		{
			return "<{0} {1} {2} {3} {4} {5}>".Fmt(GetType().Name, (Achievement != null) ? Achievement.Id.ToString() : "NULL", Progress, MaxProgress, _lazySendChanged, _lazyProgressChanged);
		}

		internal bool SetPercentCompleted(double p)
		{
			if (Achievement != null)
			{
				Utils.Log("SetPercentCompleted", p, Achievement.Id, Progress, MaxProgress);
			}
			else
			{
				Utils.LogForce("~SetPercentCompleted no achiv", p, Progress, MaxProgress);
			}
			int num = (int)(p / 100.0 * (double)MaxProgress);
			Utils.Log("CHECK", p, num, Progress, MaxProgress, num > Progress, Progress != MaxProgress);
			if (num > Progress && Progress != MaxProgress)
			{
				SetProgress((num <= MaxProgress) ? num : MaxProgress, "SetPercentCompleted");
				Messenger.Invoke(Globals.MsgGameEventProgressChanged, this, "SetPercentCompleted");
				return true;
			}
			return false;
		}
	}

	internal class BaseFightEvent : Event
	{
		protected bool _inBattle;

		protected readonly bool _sendOnSuccess;

		private int _progressOnStart = -1;

		protected BaseFightEvent(bool sendOnSucess)
		{
			_sendOnSuccess = sendOnSucess;
			_lazySendChanged = true;
			_listeners.Add(Messenger.AddListener(Globals.MsgFightStarted, OnFightStart_));
			_listeners.Add(Messenger.AddListener(Globals.MsgFightBreak, OnFightBreak_));
			_listeners.Add(Messenger<Person>.AddListener(Globals.MsgPersonDie, OnPersonDie_));
		}

		private void OnFightBreak_()
		{
			if (_progressOnStart != -1)
			{
				_inBattle = false;
				_lazyProgressChanged = false;
				SetProgress(_progressOnStart, "OnFightBreak_");
				OnFightBreak();
			}
		}

		private void OnFightStart_()
		{
			if (base.Progress >= MaxProgress)
			{
				_progressOnStart = -1;
				return;
			}
			_inBattle = true;
			_lazyProgressChanged = false;
			_progressOnStart = base.Progress;
			OnFightStart();
		}

		private void OnPersonDie_(Person person)
		{
			if (_progressOnStart == -1 || !_inBattle)
			{
				return;
			}
			if (person.Equals(Globals.Enemy))
			{
				OnEnemyDie();
				if (_lazyProgressChanged && _sendOnSuccess)
				{
					ProgressChanged();
				}
			}
			else
			{
				SetProgress(_progressOnStart, "OnPersonDie_ person");
			}
			_inBattle = false;
			_lazyProgressChanged = false;
		}

		protected virtual void OnEnemyDie()
		{
		}

		protected virtual void OnFightBreak()
		{
		}

		protected virtual void OnFightStart()
		{
		}
	}

	internal class ChapterDoneEvent : Event
	{
		private readonly int _chapterId;

		internal ChapterDoneEvent(int chapterId)
		{
			_chapterId = chapterId;
			_listeners.Add(Messenger<ServerData.Location>.AddListener(Globals.MsgZachistkaDone, OnDone));
		}

		private void OnDone(ServerData.Location location)
		{
			if (location.Id == _chapterId)
			{
				IncProgress();
			}
		}
	}

	internal class ChapterOpenEvent : Event
	{
		private readonly int _chapterId;

		internal ChapterOpenEvent(int chapterId)
		{
			_chapterId = chapterId;
			_listeners.Add(Messenger<ServerData.Location, OpenCondition.DoneReasonE>.AddListener(Globals.Msg_LocationOpenConditionDone, OnDone));
		}

		private void OnDone(ServerData.Location location, OpenCondition.DoneReasonE reason)
		{
			if (location.Id == _chapterId)
			{
				IncProgress();
			}
		}
	}

	internal class PlayerGotLevelEvent : Event
	{
		private readonly int _level;

		internal PlayerGotLevelEvent(int level)
		{
			_level = level;
			_listeners.Add(Messenger<int, int, string>.AddListener(Globals.MsgPlayerLevelChanged, OnDone));
		}

		private void OnDone(int old, int @new, string reason)
		{
			if (reason == "AddPlayerExperience" && @new == _level)
			{
				IncProgress();
			}
		}
	}

	internal class LearnMagicSchoolsEvent : Event
	{
		private readonly int _count;

		internal LearnMagicSchoolsEvent(int count)
		{
			_count = count;
			_listeners.Add(Messenger<ServerData.Spell>.AddListener(Globals.MsgPlayerSpellBuyed, OnDone));
		}

		private void OnDone(ServerData.Spell spell)
		{
			if (_count == SingletonT<ServerData>.I.GetMySpellsSchoolsCount())
			{
				IncProgress();
			}
		}
	}

	internal class LearnAllMagicSchoolsEvent : Event
	{
		internal LearnAllMagicSchoolsEvent()
		{
			_listeners.Add(Messenger<ServerData.Spell>.AddListener(Globals.MsgPlayerSpellBuyed, OnDone));
		}

		private void OnDone(ServerData.Spell spell)
		{
			if (SingletonT<ServerData>.I.GetMySpellsSchoolsCount() == SingletonT<ServerData>.I.GetAllSpellsSchoolsCount())
			{
				IncProgress();
			}
		}
	}

	internal class LearnMaxMagicSchoolEvent : Event
	{
		internal LearnMaxMagicSchoolEvent()
		{
			_listeners.Add(Messenger<ServerData.Spell>.AddListener(Globals.MsgPlayerSpellBuyed, OnDone));
		}

		private void OnDone(ServerData.Spell spell)
		{
			if (spell.NextSpell == null)
			{
				IncProgress();
			}
		}
	}

	internal class CollectRageBallsEvent : BaseFightEvent
	{
		internal CollectRageBallsEvent(int count)
			: base(sendOnSucess: true)
		{
			MaxProgress = ((count <= 1) ? 1 : count);
			_listeners.Add(Messenger.AddListener(Globals.MsgFightRageBallClicked, base.IncProgress));
		}
	}

	internal class CollectManaBallsEvent : BaseFightEvent
	{
		internal CollectManaBallsEvent(int count)
			: base(sendOnSucess: true)
		{
			MaxProgress = ((count <= 1) ? 1 : count);
			_listeners.Add(Messenger.AddListener(Globals.MsgFightManaBallClicked, base.IncProgress));
		}
	}

	internal class EyeClickedEvent : BaseFightEvent
	{
		internal EyeClickedEvent(int count)
			: base(sendOnSucess: true)
		{
			MaxProgress = ((count <= 1) ? 1 : count);
			_listeners.Add(Messenger.AddListener(Globals.MsgFightEyeDieByClick, base.IncProgress));
		}
	}

	internal class ChestOpenedEvent : Event
	{
		private readonly int _chestType;

		internal ChestOpenedEvent(string chestType, int maxCount)
		{
			MaxProgress = maxCount;
			if (chestType == "easy")
			{
				_chestType = 1;
			}
			if (chestType == "normal")
			{
				_chestType = 2;
			}
			if (chestType == "hard")
			{
				_chestType = 3;
			}
			_listeners.Add(Messenger<int, int>.AddListener(Globals.Msg_ChestOpened, delegate(int type, int _)
			{
				if (type == _chestType)
				{
					IncProgress();
				}
			}));
		}
	}

	internal class ChestOpenedNoKeysEvent : Event
	{
		private readonly int _chestType;

		internal ChestOpenedNoKeysEvent(string chestType, int count)
		{
			MaxProgress = ((count <= 1) ? 1 : count);
			if (chestType == "easy")
			{
				_chestType = 1;
			}
			if (chestType == "normal")
			{
				_chestType = 2;
			}
			if (chestType == "hard")
			{
				_chestType = 3;
			}
			_listeners.Add(Messenger<int, int>.AddListener(Globals.Msg_ChestOpened, delegate(int type, int _)
			{
				if (type == _chestType && _ == 0)
				{
					IncProgress();
				}
			}));
		}
	}

	internal class DodgeMagicEvent : BaseFightEvent
	{
		private readonly string _type;

		internal DodgeMagicEvent(string type, int count)
			: base(sendOnSucess: true)
		{
			MaxProgress = ((count <= 1) ? 1 : count);
			_type = type;
			_listeners.Add(Messenger<string, ReactE>.AddListener(Globals.MsgPlayerSpellReact, delegate(string spellType, ReactE _)
			{
				if (_ == ReactE.Dodge && _type == spellType)
				{
					IncProgress();
				}
			}));
		}
	}

	internal class UseDifferentMagicEvent : BaseFightEvent
	{
		private MagicTypeE _lastMagic;

		internal UseDifferentMagicEvent()
			: base(sendOnSucess: true)
		{
			_listeners.Add(Messenger<MagicTypeE>.AddListener(Globals.MsgPlayerCastSpell, delegate(MagicTypeE _)
			{
				if (_inBattle)
				{
					if (_lastMagic != MagicTypeE.None && _lastMagic != _)
					{
						_inBattle = false;
						IncProgress();
					}
					else
					{
						_lastMagic = _;
					}
				}
			}));
		}

		protected override void OnFightStart()
		{
			_lastMagic = MagicTypeE.None;
		}
	}

	internal class UseRageEvent : BaseFightEvent
	{
		private readonly string _type = string.Empty;

		internal UseRageEvent(string type)
			: base(sendOnSucess: true)
		{
			UseRageEvent useRageEvent = this;
			_type = type;
			_listeners.Add(Messenger<string>.AddListener(Globals.MsgPlayerUseRage, delegate(string _)
			{
				if (type == _)
				{
					useRageEvent.IncProgress();
				}
			}));
		}
	}

	internal class UsePoisonEvent : BaseFightEvent
	{
		internal UsePoisonEvent()
			: base(sendOnSucess: true)
		{
			_listeners.Add(Messenger.AddListener(Globals.MsgPlayerUsePoison, base.IncProgress));
		}
	}

	internal class WinWithHealthEvent : BaseFightEvent
	{
		private readonly int _health;

		private readonly bool _isLess;

		internal WinWithHealthEvent(bool isLess, int health)
			: base(sendOnSucess: true)
		{
			_health = health;
			_isLess = isLess;
		}

		protected override void OnEnemyDie()
		{
			Player player = Globals.Player;
			float num = (float)player.Health / (float)player.MaxHealth * 100f;
			if (_isLess)
			{
				if (num <= (float)_health)
				{
					IncProgress();
				}
			}
			else if (num >= (float)_health)
			{
				IncProgress();
			}
		}
	}

	internal class CollectGoldEvent : Event
	{
		private readonly int _count;

		internal CollectGoldEvent(int count)
		{
			CollectGoldEvent collectGoldEvent = this;
			_count = count;
			_listeners.Add(Messenger<ServerData.MoneyType.TypeE, string>.AddListener(Globals.MsgPlayerFundsChanged, delegate(ServerData.MoneyType.TypeE _, string reason)
			{
				if (_ == ServerData.MoneyType.TypeE.Gold && SingletonT<ServerData>.I.PlayerParams.GoldCount >= count && reason == "real_change")
				{
					collectGoldEvent.IncProgress();
				}
			}));
		}
	}

	internal class KillsEnemiesFromLocationEvent : BaseFightEvent
	{
		internal KillsEnemiesFromLocationEvent()
			: base(sendOnSucess: true)
		{
		}

		protected override void OnEnemyDie()
		{
			if (Globals.Enemy.FromLocation)
			{
				IncProgress();
			}
		}
	}

	internal class BuyInShopEvent : Event
	{
		private readonly int _itemId;

		internal BuyInShopEvent(int itemId)
		{
			_itemId = itemId;
			_listeners.Add(Messenger<ServerData.Item, ServerData.MoneyType.TypeE>.AddListener(Globals.MsgItemBuyInShop, delegate(ServerData.Item item, ServerData.MoneyType.TypeE moneyType)
			{
				if (item.Id == _itemId)
				{
					IncProgress();
				}
			}));
		}
	}

	internal class ChestsOnLocationWasFoundEvent : Event
	{
		internal ChestsOnLocationWasFoundEvent()
		{
			_listeners.Add(Messenger<LocationLogic>.AddListener(Globals.Msg_ChestOnLocationWasFound, delegate
			{
				IncProgress();
			}));
		}
	}

	internal class FoundInDropEvent : Event
	{
		private readonly int _itemId;

		internal FoundInDropEvent(int itemId, int count)
		{
			MaxProgress = ((count <= 1) ? 1 : count);
			_itemId = itemId;
			_listeners.Add(Messenger<ServerData.Item>.AddListener(Globals.MsgItemFoundInDrop, delegate(ServerData.Item item)
			{
				if (item.Id == _itemId)
				{
					IncProgress();
				}
			}));
		}
	}

	internal class KillsElfsEvent : BaseFightEvent
	{
		internal KillsElfsEvent()
			: base(sendOnSucess: true)
		{
		}

		protected override void OnEnemyDie()
		{
			if (SingletonT<ServerData>.I.GameSettings.Elfs == null)
			{
				return;
			}
			ServerData.Location.BotLocationInfo[] elfs = SingletonT<ServerData>.I.GameSettings.Elfs;
			foreach (ServerData.Location.BotLocationInfo botLocationInfo in elfs)
			{
				if (botLocationInfo.Bot.Id == Globals.Enemy.ServerBotInfo.Id)
				{
					IncProgress();
					break;
				}
			}
		}
	}

	internal class FatalityExecutedEvent : Event
	{
		internal FatalityExecutedEvent()
		{
			_listeners.Add(Messenger.AddListener(Globals.MsgFatalityExecuted, base.IncProgress));
		}
	}

	internal class MissesInFightEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _misses;

		internal MissesInFightEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			_listeners.Add(Messenger<ReactE>.AddListener(Globals.MsgPlayerReact, delegate(ReactE _)
			{
				if (_inBattle && _ == ReactE.Dodge)
				{
					_misses++;
				}
			}));
		}

		protected override void OnFightStart()
		{
			_misses = 0;
		}

		protected override void OnFightBreak()
		{
			_misses = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_misses >= _count && _count > 0)
			{
				IncProgress();
			}
			_misses = 0;
		}
	}

	internal class UseElixirsInFightEvent : BaseFightEvent
	{
		private readonly int _count;

		private readonly ServerData.Item.ElixirTypeE _type;

		private int _uses;

		internal UseElixirsInFightEvent(ServerData.Item.ElixirTypeE type, int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			_type = type;
			Messenger<ServerData.Item.ElixirTypeE>.AddListener(Globals.MsgUseElixir, delegate(ServerData.Item.ElixirTypeE _)
			{
				if (_inBattle && _ == _type)
				{
					_uses++;
				}
			});
		}

		protected override void OnFightStart()
		{
			_uses = 0;
		}

		protected override void OnFightBreak()
		{
			_uses = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_uses >= _count && _count > 0)
			{
				IncProgress();
			}
			_uses = 0;
		}
	}

	internal class UseCombosInFightEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _used;

		internal UseCombosInFightEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			Messenger.AddListener(Globals.MsgPlayerCombo, delegate
			{
				if (_inBattle)
				{
					_used++;
				}
			});
		}

		protected override void OnFightStart()
		{
			_used = 0;
		}

		protected override void OnFightBreak()
		{
			_used = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_used >= _count && _count > 0)
			{
				IncProgress();
			}
			_used = 0;
		}
	}

	internal class FightOnlyLeftEvent : BaseFightEvent
	{
		private bool _failed = true;

		internal FightOnlyLeftEvent()
			: base(sendOnSucess: true)
		{
			Messenger.AddListener(Globals.MsgPlayerCombo, delegate
			{
				_failed = true;
			});
			Messenger<AttackE>.AddListener(Globals.MsgPlayerAttack, delegate(AttackE _)
			{
				if (_ != AttackE.Right)
				{
					_failed = true;
				}
			});
			Messenger<MagicTypeE>.AddListener(Globals.MsgPlayerCastSpell, delegate
			{
				_failed = true;
			});
		}

		protected override void OnFightStart()
		{
			base.OnFightStart();
			_failed = false;
		}

		protected override void OnFightBreak()
		{
			base.OnFightBreak();
			_failed = true;
		}

		protected override void OnEnemyDie()
		{
			if (!_failed)
			{
				IncProgress();
			}
			_failed = true;
		}
	}

	internal class FightSpawnManaEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _spawned;

		internal FightSpawnManaEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			Messenger.AddListener(Globals.MsgSpawnManaBubbleFromEnemy, delegate
			{
				if (_inBattle)
				{
					_spawned++;
				}
			});
			Messenger.AddListener(Globals.MsgFightManaBallClicked, delegate
			{
				if (_inBattle)
				{
					_spawned--;
				}
			});
		}

		protected override void OnFightStart()
		{
			_spawned = 0;
		}

		protected override void OnFightBreak()
		{
			_spawned = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_count > 0 && _spawned >= _count)
			{
				IncProgress();
			}
			_spawned = 0;
		}
	}

	internal class FightSpawnRageEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _spawned;

		internal FightSpawnRageEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			Messenger.AddListener(Globals.MsgSpawnRageBubbleFromEnemy, delegate
			{
				if (_inBattle)
				{
					_spawned++;
				}
			});
			Messenger.AddListener(Globals.MsgFightRageBallClicked, delegate
			{
				if (_inBattle)
				{
					_spawned--;
				}
			});
		}

		protected override void OnFightStart()
		{
			_spawned = 0;
		}

		protected override void OnFightBreak()
		{
			_spawned = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_count > 0 && _spawned >= _count)
			{
				IncProgress();
			}
			_spawned = 0;
		}
	}

	internal class FullManaAndRageEvent : BaseFightEvent
	{
		private bool _failed = true;

		internal FullManaAndRageEvent()
			: base(sendOnSucess: true)
		{
			Messenger<MagicTypeE>.AddListener(Globals.MsgPlayerCastSpell, delegate
			{
				_failed = true;
			});
			Messenger<int>.AddListener(Globals.MsgPlayerUseRage, delegate
			{
				_failed = true;
			});
		}

		protected override void OnFightStart()
		{
			ServerData.PlayerParamsData playerParams = SingletonT<ServerData>.I.PlayerParams;
			ServerData.Settings gameSettings = SingletonT<ServerData>.I.GameSettings;
			_failed = playerParams._mana != gameSettings.MaxMana || playerParams._rageSpheresCount != gameSettings.MaxRage;
		}

		protected override void OnFightBreak()
		{
			_failed = true;
		}

		protected override void OnEnemyDie()
		{
			if (!_failed)
			{
				IncProgress();
			}
			_failed = true;
		}
	}

	internal class FightFullManaCollectMoreEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _colletedMore;

		internal FightFullManaCollectMoreEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			Messenger.AddListener(Globals.MsgFightManaBallClicked, delegate
			{
				if (_inBattle && SingletonT<ServerData>.I.PlayerParams._mana >= SingletonT<ServerData>.I.GameSettings.MaxMana)
				{
					_colletedMore++;
				}
			});
		}

		protected override void OnFightStart()
		{
			_colletedMore = 0;
		}

		protected override void OnFightBreak()
		{
			_colletedMore = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_colletedMore >= _count)
			{
				IncProgress();
			}
			_colletedMore = 0;
		}
	}

	internal class FightFullRageCollectMoreEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _colletedMore;

		internal FightFullRageCollectMoreEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			Messenger.AddListener(Globals.MsgFightRageBallClicked, delegate
			{
				if (_inBattle && SingletonT<ServerData>.I.PlayerParams.RageSpheresCount >= SingletonT<ServerData>.I.GameSettings.MaxRage)
				{
					_colletedMore++;
				}
			});
		}

		protected override void OnFightStart()
		{
			_colletedMore = 0;
		}

		protected override void OnFightBreak()
		{
			_colletedMore = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_colletedMore >= _count)
			{
				IncProgress();
			}
			_colletedMore = 0;
		}
	}

	internal class FightUseAllRagesEvent : BaseFightEvent
	{
		private bool _used1;

		private bool _used2;

		private bool _used3;

		internal FightUseAllRagesEvent()
			: base(sendOnSucess: true)
		{
			Messenger<string>.AddListener(Globals.MsgPlayerUseRage, delegate(string _)
			{
				if (_inBattle)
				{
					if (_ == Battle.RageAbilityNameBlock)
					{
						_used1 = true;
					}
					if (_ == Battle.RageAbilityNameCrit)
					{
						_used2 = true;
					}
					if (_ == Battle.RageAbilityNameStrong)
					{
						_used3 = true;
					}
				}
			});
		}

		private void Reset()
		{
			_used1 = false;
			_used2 = false;
			_used3 = false;
		}

		protected override void OnFightStart()
		{
			Reset();
		}

		protected override void OnFightBreak()
		{
			Reset();
		}

		protected override void OnEnemyDie()
		{
			if (_used1 && _used2 && _used3)
			{
				IncProgress();
			}
			Reset();
		}
	}

	internal class FightKillWithPoisonEvent : BaseFightEvent
	{
		private bool _isKilledByPoison;

		internal FightKillWithPoisonEvent()
			: base(sendOnSucess: true)
		{
			Messenger.AddListener(Globals.MsgEnemyKilledByPoison, delegate
			{
				if (_inBattle)
				{
					_isKilledByPoison = true;
				}
			});
		}

		protected override void OnFightStart()
		{
			_isKilledByPoison = false;
		}

		protected override void OnFightBreak()
		{
			_isKilledByPoison = false;
		}

		protected override void OnEnemyDie()
		{
			if (_isKilledByPoison)
			{
				IncProgress();
			}
			_isKilledByPoison = false;
		}
	}

	internal class ZombieEvent : Event
	{
		internal ZombieEvent()
		{
			Messenger<FightResultStats>.AddListener(Globals.MsgFightResult, delegate(FightResultStats _)
			{
				if (_.Fatality == Battle.FatalityStateE.Undone)
				{
					IncProgress();
				}
			});
		}
	}

	internal class LetEyeEatEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _eaten;

		internal LetEyeEatEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			Messenger.AddListener(Globals.MsgEyeEatRage, delegate
			{
				if (_inBattle)
				{
					_eaten++;
				}
			});
		}

		protected override void OnFightStart()
		{
			_eaten = 0;
		}

		protected override void OnFightBreak()
		{
			_eaten = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_eaten >= _count)
			{
				IncProgress();
			}
			_eaten = 0;
		}
	}

	internal class LeaveChestsEvent : Event
	{
		private readonly int _count;

		internal LeaveChestsEvent(int count)
		{
			_count = count;
			Messenger<int, int>.AddListener(Globals.Msg_LocationChestsWasFoundChanged, delegate(int wasFoundOnLocation, int wasFoundTotal)
			{
				if (wasFoundTotal >= _count && _count > 0)
				{
					IncProgress();
				}
			});
		}
	}

	internal class FightKillEyeEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _killed;

		internal FightKillEyeEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			_listeners.Add(Messenger.AddListener(Globals.MsgFightEyeDieByClick, delegate
			{
				if (_inBattle)
				{
					_killed++;
				}
			}));
		}

		protected override void OnFightStart()
		{
			_killed = 0;
		}

		protected override void OnFightBreak()
		{
			_killed = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_killed >= _count)
			{
				IncProgress();
			}
			_killed = 0;
		}
	}

	internal class FightBlocksEvent : BaseFightEvent
	{
		private readonly int _count;

		private int _blocks;

		internal FightBlocksEvent(int count)
			: base(sendOnSucess: true)
		{
			_count = count;
			Messenger<string>.AddListener(Globals.MsgPlayerUseRage, delegate(string _)
			{
				if (_inBattle && _ == Battle.RageAbilityNameBlock)
				{
					_blocks++;
				}
			});
		}

		protected override void OnFightStart()
		{
			_blocks = 0;
		}

		protected override void OnFightBreak()
		{
			_blocks = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_blocks >= _count)
			{
				IncProgress();
			}
			_blocks = 0;
		}
	}

	internal class FightKilledByMagicEvent : Event
	{
		private readonly string _type;

		internal FightKilledByMagicEvent(string type)
		{
			_type = type;
			Messenger<string>.AddListener(Globals.MsgPlayerKilledByMagic, delegate(string _)
			{
				if (_ == _type)
				{
					IncProgress();
				}
			});
		}
	}

	internal class TotalCastesInFightEvent : BaseFightEvent
	{
		private int _casts;

		internal TotalCastesInFightEvent(int count)
			: base(sendOnSucess: true)
		{
			MaxProgress = count;
			_listeners.Add(Messenger<MagicTypeE>.AddListener(Globals.MsgPlayerCastSpell, delegate
			{
				if (_inBattle)
				{
					_casts++;
				}
			}));
		}

		protected override void OnFightStart()
		{
			_casts = 0;
		}

		protected override void OnFightBreak()
		{
			_casts = 0;
		}

		protected override void OnEnemyDie()
		{
			if (_casts > 0)
			{
				IncProgress(_casts);
			}
			_casts = 0;
		}
	}

	internal class KillsEnemiesEvent : BaseFightEvent
	{
		internal KillsEnemiesEvent(int count)
			: base(sendOnSucess: true)
		{
			MaxProgress = count;
		}

		protected override void OnEnemyDie()
		{
			IncProgress();
		}
	}

	internal class TotalMakeEpicItemEvent : Event
	{
		internal TotalMakeEpicItemEvent()
		{
			Messenger.AddListener(Globals.MsgItemUpgrade, delegate
			{
				IncProgress();
			});
		}
	}

	internal class FightRating : Event
	{
		private readonly int _count;

		internal FightRating(int count)
		{
			_count = count;
			Messenger<FightResultStats>.AddListener(Globals.MsgFightResult, delegate(FightResultStats _)
			{
				if (_.FightRating == _count)
				{
					IncProgress();
				}
			});
		}
	}

	internal class FightMinigameDropOnce : Event
	{
		private readonly int _count;

		internal FightMinigameDropOnce(int count)
		{
			_count = count;
			Messenger<int>.AddListener(Globals.MsgMatch3ChainDestroyed, delegate(int _)
			{
				if (_ >= _count)
				{
					IncProgress();
				}
			});
		}
	}

	internal class FightMinigamePoints : Event
	{
		private readonly int _count;

		internal FightMinigamePoints(int count)
		{
			_count = count;
			Messenger<int>.AddListener(Globals.MsgMatch3NewRecord, delegate(int _)
			{
				if (_ >= _count)
				{
					IncProgress();
				}
			});
		}
	}

	private List<Event> _events = new List<Event>();

	private static readonly string SeparatorMac = "|||";

	private static readonly string Separator = " | ";

	internal static readonly string XCodeFileName = "achivs";

	internal static readonly string ProgressFileName = "achivs_progress";

	private IDisposable _listener;

	internal List<Event> Events => _events;

	internal string StateStringForXCode
	{
		get
		{
			if (_events == null)
			{
				return string.Empty;
			}
			if (_events.Count == 0)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(4096);
			stringBuilder.Append(NormalizeString(SingletonT<ServerData>.I.GetPhrase("AchivsTotalProgress")));
			stringBuilder.Append(SeparatorMac);
			stringBuilder.Append(NormalizeString(SingletonT<ServerData>.I.GetPhrase("AchivsAll")));
			stringBuilder.Append(SeparatorMac);
			stringBuilder.Append(NormalizeString(SingletonT<ServerData>.I.GetPhrase("AchivsDone")));
			stringBuilder.Append(SeparatorMac);
			stringBuilder.Append(NormalizeString(SingletonT<ServerData>.I.GetPhrase("AchivsUndone")));
			stringBuilder.Append(SeparatorMac);
			stringBuilder.Append(NormalizeString(SingletonT<ServerData>.I.GetPhrase("AchivsRating")));
			stringBuilder.Append("\n");
			foreach (Event @event in _events)
			{
				stringBuilder.Append(@event.Achievement.Order);
				stringBuilder.Append(SeparatorMac);
				stringBuilder.Append(@event.Achievement.Image);
				stringBuilder.Append(SeparatorMac);
				stringBuilder.Append(NormalizeString(@event.Achievement.Title));
				stringBuilder.Append(SeparatorMac);
				stringBuilder.Append(NormalizeString(@event.Achievement.Info));
				stringBuilder.Append(SeparatorMac);
				stringBuilder.Append(@event.Progress);
				stringBuilder.Append(SeparatorMac);
				stringBuilder.Append(@event.MaxProgress);
				stringBuilder.Append(SeparatorMac);
				stringBuilder.Append(@event.Achievement.Points);
				stringBuilder.Append(SeparatorMac);
				stringBuilder.Append(@event.Achievement.Id);
				stringBuilder.Append("\n");
			}
			return stringBuilder.ToString();
		}
	}

	internal int AllFinishedAchivsPoints
	{
		get
		{
			int num = 0;
			foreach (Event @event in Events)
			{
				if (@event.Progress >= @event.MaxProgress && @event.Achievement != null)
				{
					num += @event.Achievement.Points;
				}
			}
			return num;
		}
	}

	internal GameEvents()
	{
		_listener = Messenger<Event, string>.AddListener(Globals.MsgGameEventProgressChanged, delegate(Event _, string reason)
		{
			if (!(reason != "ProgressChanged"))
			{
				Utils.Log("EVENT PROGRESS CHANGED", _);
				if (!Globals.IgnoreSaveGame)
				{
					SaveProgress();
				}
			}
		});
	}

	void IDisposable.Dispose()
	{
		Utils.Dispose(ref _listener);
	}

	private static string NormalizeString(string text)
	{
		return (!string.IsNullOrEmpty(text)) ? text : " ";
	}

	internal void RegisterEvent(Event anEvent)
	{
		_events.Add(anEvent);
	}

	internal void SaveProgress()
	{
		StringBuilder stringBuilder = new StringBuilder(_events.Count * 25);
		foreach (Event @event in _events)
		{
			if (@event.Achievement != null)
			{
				stringBuilder.Append(@event.Achievement.Id);
				stringBuilder.Append(' ');
				stringBuilder.Append(@event.Progress);
				stringBuilder.Append(' ');
				stringBuilder.Append(@event.MaxProgress);
				stringBuilder.Append(' ');
			}
		}
		Utils.LogForce("ACHIVS SAVE PROGRESS");
		Utils.WriteAllText(Path.Combine(UnityApi.GetPath(), ProgressFileName), stringBuilder.ToString());
	}

	internal bool LoadAchivs()
	{
		string path = Path.Combine(UnityApi.GetPath(), ProgressFileName);
		if (!File.Exists(path))
		{
			Utils.Log("*** ACHIVS LOAD ERROR NO FILE");
			return false;
		}
		string[] array = Utils.ReadAllText(path).Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i += 3)
		{
			int result = 0;
			if (!int.TryParse(array[i], out result) || !int.TryParse(array[i + 1], out result) || !int.TryParse(array[i + 2], out result))
			{
				Utils.Log("*** ACHIVS LOAD ERROR");
				return false;
			}
		}
		Utils.LogForce("*** ACHIVS LOADING");
		for (int j = 0; j < array.Length; j += 3)
		{
			int id = int.Parse(array[j]);
			int value = int.Parse(array[j + 1]);
			int num = int.Parse(array[j + 2]);
			GetEventByServerId(id)?.SetProgress(value, "LoadAchivs");
		}
		Utils.LogForce("*** ACHIVS LOADED");
		return true;
	}

	private Event GetEventByServerId(int id)
	{
		foreach (Event @event in _events)
		{
			if (@event.Achievement != null && @event.Achievement.Id == id)
			{
				return @event;
			}
		}
		return null;
	}

	internal void Reset()
	{
		foreach (Event @event in _events)
		{
			@event.Dispose();
		}
		_events.Clear();
		foreach (KeyValuePair<int, ServerData.Achievement> achievement in SingletonT<ServerData>.I._achievements)
		{
			Event obj = null;
			Dictionary<string, object> condition = achievement.Value.Condition;
			switch (JsonUtils.JsonGet<string>("choice", condition))
			{
			case "test1":
				obj = new ChapterDoneEvent(JsonUtils.JsonGetInt("location", condition));
				break;
			case "test2":
				obj = new ChapterOpenEvent(JsonUtils.JsonGetInt("location", condition));
				break;
			case "test3":
				obj = new PlayerGotLevelEvent(JsonUtils.JsonGetInt("level", condition));
				break;
			case "test4":
				obj = new LearnMagicSchoolsEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test5":
				obj = new LearnAllMagicSchoolsEvent();
				break;
			case "test6":
				obj = new LearnMaxMagicSchoolEvent();
				break;
			case "test7":
				obj = new CollectManaBallsEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test8":
				obj = new CollectRageBallsEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test9":
				obj = new EyeClickedEvent(JsonUtils.JsonGetInt("count", condition));
				break;
			case "test10":
				obj = new ChestOpenedEvent(JsonUtils.JsonGet<string>("type", condition), JsonUtils.JsonGetInt("number", condition));
				break;
			case "test11":
				obj = new ChestOpenedNoKeysEvent(JsonUtils.JsonGet<string>("type", condition), JsonUtils.JsonGetInt("count", condition));
				break;
			case "test12":
				obj = new DodgeMagicEvent(JsonUtils.JsonGet<string>("type", condition), JsonUtils.JsonGetInt("count", condition));
				break;
			case "test13":
			{
				UseDifferentMagicEvent useDifferentMagicEvent = new UseDifferentMagicEvent();
				useDifferentMagicEvent.MaxProgress = JsonUtils.JsonGetInt("count", condition);
				obj = useDifferentMagicEvent;
				break;
			}
			case "test14":
			{
				UseRageEvent useRageEvent = new UseRageEvent(JsonUtils.JsonGet<string>("type", condition));
				useRageEvent.MaxProgress = JsonUtils.JsonGetInt("count", condition);
				obj = useRageEvent;
				break;
			}
			case "test15":
			{
				UsePoisonEvent usePoisonEvent = new UsePoisonEvent();
				usePoisonEvent.MaxProgress = JsonUtils.JsonGetInt("count", condition);
				obj = usePoisonEvent;
				break;
			}
			case "test16":
				obj = new WinWithHealthEvent(isLess: false, JsonUtils.JsonGetInt("percent", condition));
				break;
			case "test17":
				obj = new WinWithHealthEvent(isLess: true, JsonUtils.JsonGetInt("percent", condition));
				break;
			case "test18":
				obj = new CollectGoldEvent(JsonUtils.JsonGetInt("amount", condition));
				break;
			case "test19":
			{
				KillsEnemiesFromLocationEvent killsEnemiesFromLocationEvent = new KillsEnemiesFromLocationEvent();
				killsEnemiesFromLocationEvent.MaxProgress = JsonUtils.JsonGetInt("number", condition);
				obj = killsEnemiesFromLocationEvent;
				break;
			}
			case "test20":
				obj = new BuyInShopEvent(JsonUtils.JsonGetInt("item", condition));
				break;
			case "test21":
			{
				ChestsOnLocationWasFoundEvent chestsOnLocationWasFoundEvent = new ChestsOnLocationWasFoundEvent();
				chestsOnLocationWasFoundEvent.MaxProgress = JsonUtils.JsonGetInt("number", condition);
				obj = chestsOnLocationWasFoundEvent;
				break;
			}
			case "test22":
				obj = new FoundInDropEvent(JsonUtils.JsonGetInt("item", condition), JsonUtils.JsonGetInt("count", condition));
				break;
			case "test23":
			{
				KillsElfsEvent killsElfsEvent = new KillsElfsEvent();
				killsElfsEvent.MaxProgress = JsonUtils.JsonGetInt("count", condition);
				obj = killsElfsEvent;
				break;
			}
			case "test24":
			{
				FatalityExecutedEvent fatalityExecutedEvent = new FatalityExecutedEvent();
				fatalityExecutedEvent.MaxProgress = JsonUtils.JsonGetInt("number", condition);
				obj = fatalityExecutedEvent;
				break;
			}
			case "test25":
				obj = new MissesInFightEvent(JsonUtils.JsonGetInt("count", condition));
				break;
			case "test26":
				obj = new UseElixirsInFightEvent(ServerData.Item.ElixirTypeE.Heal, JsonUtils.JsonGetInt("number", condition));
				break;
			case "test27":
				obj = new UseElixirsInFightEvent(ServerData.Item.ElixirTypeE.Critical, JsonUtils.JsonGetInt("number", condition));
				break;
			case "test28":
				obj = new UseCombosInFightEvent(JsonUtils.JsonGetInt("count", condition));
				break;
			case "test29":
				obj = new FightOnlyLeftEvent();
				break;
			case "test30":
				obj = new FightSpawnManaEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test31":
				obj = new FightSpawnRageEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test32":
				obj = new FullManaAndRageEvent();
				break;
			case "test33":
				obj = new FightFullManaCollectMoreEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test34":
				obj = new FightFullRageCollectMoreEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test35":
				obj = new FightUseAllRagesEvent();
				break;
			case "test36":
				obj = new FightKillWithPoisonEvent();
				break;
			case "test37":
			{
				ZombieEvent zombieEvent = new ZombieEvent();
				zombieEvent.MaxProgress = JsonUtils.JsonGetInt("count", condition);
				obj = zombieEvent;
				break;
			}
			case "test38":
				obj = new LetEyeEatEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test39":
				obj = new FightKilledByMagicEvent("strong");
				break;
			case "test40":
				obj = new FightKilledByMagicEvent("weak");
				break;
			case "test41":
				obj = new LeaveChestsEvent(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test42":
				obj = new FightKillEyeEvent(JsonUtils.JsonGetInt("count", condition));
				break;
			case "test43":
				obj = new FightBlocksEvent(JsonUtils.JsonGetInt("count", condition));
				break;
			case "test44":
				obj = new UseElixirsInFightEvent(ServerData.Item.ElixirTypeE.Poison, JsonUtils.JsonGetInt("count", condition));
				break;
			case "test45":
				obj = new KillsEnemiesEvent(JsonUtils.JsonGetInt("count", condition));
				break;
			case "test46":
				obj = new TotalCastesInFightEvent(JsonUtils.JsonGetInt("count", condition));
				break;
			case "test47":
				obj = new TotalMakeEpicItemEvent();
				break;
			case "test48":
				obj = new FightRating(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test49":
				obj = new FightMinigameDropOnce(JsonUtils.JsonGetInt("number", condition));
				break;
			case "test50":
				obj = new FightMinigamePoints(JsonUtils.JsonGetInt("amount", condition));
				break;
			}
			if (obj != null)
			{
				obj.Achievement = achievement.Value;
				_events.Add(obj);
			}
		}
		_events.Sort(delegate(Event l, Event r)
		{
			if (l.Achievement != null && r.Achievement != null)
			{
				return l.Achievement.Order.CompareTo(r.Achievement.Order);
			}
			if (l.Achievement != null)
			{
				return 1;
			}
			return (r.Achievement != null) ? (-1) : 0;
		});
		LoadAchivs();
	}

	internal Event GetEventByAchievmentId(int id)
	{
		foreach (Event @event in _events)
		{
			if (@event.Achievement != null && @event.Achievement.Id == id)
			{
				return @event;
			}
		}
		return null;
	}
}
