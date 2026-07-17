using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AesLib;
using Assets.Plugins.GameCode.Data;
using Common;
using Compression;
using MiniJSON;
using ProtoBuf;
using UnityEngine;
using Yarx.Collections;

public class ServerData : SingletonT<ServerData>
{
	public enum StateE
	{
		None,
		Loading,
		Loaded,
		Failed,
		LoadedEqual
	}

	public class Achievement
	{
		public int Id = -1;

		public string Title;

		public string Info;

		public string Image;

		public int Points;

		public int Order;

		public Dictionary<string, object> Condition = new Dictionary<string, object>();
	}

	public class Condition
	{
		public int Id;

		public int Count = -1;

		public string Type;

		public string OpenPhrase;

		public Dictionary<string, object> Params = new Dictionary<string, object>();
	}

	[ProtoContract]
	public class Skill
	{
		public enum TypeE
		{
			Unknown,
			Strength,
			Vitality,
			Rage,
			Magic,
			MagicIce,
			MagicFire,
			MagicDark,
			MagicElectro,
			BonusRage,
			BonusMana,
			BonusExp,
			BonusMoney,
			FullMana,
			FullRage
		}

		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public TypeE Type;

		[ProtoMember(3)]
		public string Title;

		public int Weight;

		public int SkillPoint;

		public Skill()
		{
			Weight = 1;
			SkillPoint = 1;
		}

		public Skill(int id, string type, string title)
		{
			Id = id;
			Type = GetType(type);
			Title = title;
			Weight = 1;
			SkillPoint = 1;
		}

		public static TypeE GetType(string text)
		{
			switch (text)
			{
			case "STR":
				return TypeE.Strength;
			case "VIT":
				return TypeE.Vitality;
			case "RAG":
				return TypeE.Rage;
			case "SOR":
				return TypeE.Magic;
			case "ICE":
				return TypeE.MagicIce;
			case "FIRE":
				return TypeE.MagicFire;
			case "LIGHT":
				return TypeE.MagicElectro;
			case "DARK":
				return TypeE.MagicDark;
			case "BE":
				return TypeE.BonusExp;
			case "BM":
				return TypeE.BonusMana;
			case "BR":
				return TypeE.BonusRage;
			case "BMn":
				return TypeE.BonusMoney;
			case "FullMana":
				return TypeE.FullMana;
			case "FullRage":
				return TypeE.FullRage;
			default:
				Utils.LogFrom("ServerData", "Unknwon skill type", text);
				return TypeE.Unknown;
			}
		}

		public override string ToString()
		{
			return $"Id: {Id}, Type: {Type}, Title: {Title}, Weight: {Weight}, SkillPoint: {SkillPoint}";
		}
	}

	[ProtoContract]
	public class SkillInfo
	{
		[ProtoMember(1)]
		public Skill Skill;

		[ProtoMember(2)]
		public int Min;

		[ProtoMember(3)]
		public int Max;

		public int Current => Min;

		public SkillInfo Clone()
		{
			SkillInfo skillInfo = new SkillInfo();
			skillInfo.Skill = Skill;
			skillInfo.Min = Min;
			skillInfo.Max = Max;
			return skillInfo;
		}

		public override string ToString()
		{
			return $"Skill: {Skill}, Min: {Min}, Max: {Max}";
		}
	}

	[ProtoContract]
	public class Location
	{
		public class BotLocationInfo
		{
			public BotInfo Bot;

			public int Level;

			public bool IsBoss;
		}

		public class ChestLocationInfo
		{
			public Chest Chest;

			public int Prob;
		}

		public readonly float MoneyShowTreshold = 0.7f;

		private static int Counter;

		private int _counter;

		[ProtoMember(1)]
		public int Id;

		internal int MapId;

		internal int MapModel;

		internal string Title = string.Empty;

		internal BotLocationInfo[] Bots;

		internal ChestLocationInfo[] Chests;

		internal Vector2 IconMobsCoord;

		internal Vector2 IconZachistkaCoord;

		internal Vector2 IconMoneyCoord;

		internal Vector2 IconChestCoord;

		internal int RespawnPeriodSeconds;

		internal int RespawnProbability;

		internal int RespawnMax;

		internal int RespawnKill;

		internal int RespawnKillPeriodSeconds;

		internal int PopulationPeriodSeconds;

		internal int PopulationPointsUp;

		internal int PopulationMax;

		internal int MoneyMax;

		internal int MoneyPeriod;

		internal int MoneyPerPerson;

		internal Location OpenAfter;

		internal Location OpenAfterAlt;

		internal bool IsMiniBoss;

		internal bool IsAltPath;

		internal Bonus Bonus;

		internal string Description;

		internal string IconPers;

		internal Condition Condition;

		internal Dictionary<MoneyType, int> OpenPrice = new Dictionary<MoneyType, int>();

		internal bool Elfs;

		[ProtoMember(2)]
		public LocationLogic Logic;

		public bool IsCave;

		public string CaveName = string.Empty;

		public string CaveDiff = string.Empty;

		public Vector2 CaveGlobalCoord;

		public Vector2 CaveLocationCoord;

		public bool lockShaman;

		internal bool IsShowMobs => IsOpened && Logic._mobs.Count > 0;

		internal bool IsShowMoney => IsOpened && (float)Logic._money / (float)MoneyMax >= MoneyShowTreshold;

		internal bool IsShowChests
		{
			get
			{
				int num = 0;
				foreach (LocationLogic.ChestOnLocation item in Logic.ChestsOnLocation)
				{
					if (item.WasFound)
					{
						num++;
					}
				}
				return IsOpened && num > 0;
			}
		}

		internal bool IsZachistkaOpened
		{
			get
			{
				if (!IsOpened)
				{
					if (OpenAfter != null && !OpenAfter.IsOpened)
					{
						return false;
					}
					if (OpenAfterAlt != null && !OpenAfterAlt.IsOpened)
					{
						return false;
					}
					return true;
				}
				return false;
			}
		}

		internal bool IsOpened => Logic.IsOpened;

		internal string OpenInfo => SingletonT<ServerData>.I.GetPhrase(Condition.OpenPhrase);

		public Location()
		{
			_counter = ++Counter;
			Logic = new LocationLogic(this);
		}

		internal void InsertBot(int index, BotInfo bot)
		{
			List<BotLocationInfo> list = new List<BotLocationInfo>(Bots);
			list.Insert(index, new BotLocationInfo
			{
				Bot = bot,
				Level = 1
			});
			Bots = list.ToArray();
		}

		public override string ToString()
		{
			return Utils.ParamsToString("{_" + _counter, "Id=", Id, Title, "MapId=", MapId, "Bots[", (Bots != null) ? Bots.Length : 0, "]", "Opened=", IsOpened, "ZOpened=", IsZachistkaOpened, "Logic=", Logic, "}");
		}

		internal void CreateCondition()
		{
			throw new NotImplementedException();
		}
	}

	[ProtoContract]
	public class MoneyType
	{
		public enum TypeE
		{
			Gold = 1,
			Diamond,
			Key,
			Skull,
			Scarab,
			Star
		}

		public static MoneyType ZeroGold = new MoneyType
		{
			Title = string.Empty,
			Code = 1
		};

		public static MoneyType ZeroDiamond = new MoneyType
		{
			Title = string.Empty,
			Code = 2
		};

		public static MoneyType ZeroSkull = new MoneyType
		{
			Title = string.Empty,
			Code = 4
		};

		public static MoneyType ZeroKey = new MoneyType
		{
			Title = string.Empty,
			Code = 3
		};

		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public string Title;

		[ProtoMember(3)]
		public int Code;

		public TypeE Type
		{
			get
			{
				if (Code == 1)
				{
					return TypeE.Gold;
				}
				if (Code == 2)
				{
					return TypeE.Diamond;
				}
				if (Code == 3)
				{
					return TypeE.Key;
				}
				if (Code == 4)
				{
					return TypeE.Skull;
				}
				if (Code == 5)
				{
					return TypeE.Scarab;
				}
				if (Code == 6)
				{
					return TypeE.Star;
				}
				Invs.Inv(Code >= 7 || Code < 1, "Unknown MoneyType", Code);
				return TypeE.Star;
			}
		}

		public bool Equals(MoneyType other)
		{
			if (object.ReferenceEquals(null, other))
			{
				return false;
			}
			if (object.ReferenceEquals(this, other))
			{
				return true;
			}
			return other.Id == Id && other.Code == Code;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(MoneyType))
			{
				return false;
			}
			return Equals((MoneyType)obj);
		}

		public override int GetHashCode()
		{
			return (Id * 397) ^ Code;
		}

		public override string ToString()
		{
			return "<<{0} {1}>>".Fmt(Id, Title);
		}

		public static bool operator ==(MoneyType left, MoneyType right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(MoneyType left, MoneyType right)
		{
			return !object.Equals(left, right);
		}
	}

	public class Bonus
	{
		public class DropElement
		{
			public int Probability;

			public int Type;

			public int Count;

			public Item Item;

			public Bonus Bonus;

			public bool IsItem => Type == 1;

			public bool IsBonus => Type == 3;

			public bool IsExp => Type == 4;

			public override string ToString()
			{
				if (IsItem)
				{
					return "Bonus.DropElement item " + Item.Id + " " + Item.ToString();
				}
				if (IsBonus)
				{
					return $"Bonus.DropElement bonus {Count} {Bonus.Id}";
				}
				if (IsExp)
				{
					return $"Bonus.DropElement exp {Count}";
				}
				return "Bonus.DropElement empty";
			}

			public DropElement MakeDrop()
			{
				DropElement dropElement = new DropElement();
				dropElement.Type = Type;
				dropElement.Count = Count;
				DropElement dropElement2 = dropElement;
				Invs.Inv(!dropElement2.IsBonus, "!r.IsBonus");
				if (dropElement2.IsItem)
				{
					dropElement2.Item = Item.MakeRealItem(forShop: false, dropElement2.Count);
				}
				return dropElement2;
			}
		}

		public int Id;

		public string Title;

		public bool AllItems;

		public int Count;

		public List<DropElement> Drop = new List<DropElement>();

		public List<DropElement> GetRandomDrop()
		{
			return GetRandomDrop(Count);
		}

		private List<DropElement> GetRandomDrop(int count)
		{
			List<DropElement> r = new List<DropElement>();
			if (Drop.Count > 0)
			{
				Utils.Random(Drop, (DropElement _) => _.Probability, count, allowDuplicates: false, delegate(int i, int j)
				{
					DropElement dropElement = Drop[j].MakeDrop();
					Utils.Log("dropItem", dropElement);
					r.Add(dropElement);
				});
			}
			return r;
		}

		public void GetRandomDrop(List<DropElement> inChest)
		{
			GetRandomDrop(Count, inChest);
		}

		public void GetRandomDrop(int count, List<DropElement> inChest)
		{
			if (Drop.Count > 0)
			{
				List<int> added = new List<int>();
				Utils.Random(Drop, (DropElement _) => _.Probability, count, allowDuplicates: false, delegate(int i, int j)
				{
					DropElement dropElement = Drop[j].MakeDrop();
					Utils.Log("dropItem", dropElement);
					inChest.Add(dropElement);
					added.Add(j);
				});
			}
		}
	}

	[ProtoContract]
	public class BotLevel
	{
		[ProtoMember(1)]
		public int Level;

		[ProtoMember(2)]
		public string Title;

		[ProtoMember(3)]
		public Bonus WinBonus;

		[ProtoMember(4)]
		public int WinExp;

		[ProtoMember(5)]
		public int TotalWeight;

		public Bonus LossBonus;

		[ProtoMember(6)]
		public SkillInfo[] Skills;

		[ProtoMember(7)]
		public bool ShowSectorControl;

		[ProtoMember(8)]
		public float SpeedMoveSectorControl;

		[ProtoMember(9)]
		public float SectorAngle;

		[ProtoMember(10)]
		public int WeakMagicP;

		[ProtoMember(11)]
		public int StrongMagicP;

		[DefaultValue(1)]
		[ProtoMember(12)]
		public int DifMagicgame = 1;

		[ProtoMember(13)]
		public int ChangeViewDirPeriod;

		[ProtoMember(14)]
		public int ChangeViewDirProb;

		[ProtoMember(15)]
		public int ZoneSize;

		public override string ToString()
		{
			return "<{0} {1} {2}>".Fmt(Level, Title, WinBonus);
		}

		public SkillInfo GetSkill(Skill.TypeE type)
		{
			for (int i = 0; i < Skills.Length; i++)
			{
				if (Skills[i].Skill.Type == type)
				{
					return Skills[i];
				}
			}
			return null;
		}

		internal void GetWinBonus(out int exp)
		{
			exp = WinExp;
		}
	}

	[ProtoContract]
	public class BankItem
	{
		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public int Number;

		[ProtoMember(3)]
		public MoneyType CountType;

		[ProtoMember(4)]
		public int Count;

		[ProtoMember(5)]
		public MoneyType BonusType;

		[ProtoMember(6)]
		public int Bonus;

		[ProtoMember(7)]
		public float Real;

		[ProtoMember(8)]
		[DefaultValue("")]
		public string PurchaseId = string.Empty;

		[DefaultValue(false)]
		[ProtoMember(9)]
		public bool Selected;

		public int BonusPercent;

		public override string ToString()
		{
			return "{0} {1} {2} {3} {4} {5} {6} {7}".Fmt(Id, Number, CountType, Count, BonusPercent, Real, PurchaseId, Selected);
		}
	}

	public enum PhrasesE
	{
		Custom,
		PersClassAssassin,
		PersClassDestroyer,
		PersClassDefender,
		PlayerExperience,
		PlayerAnger,
		BagEquipment,
		BagBag,
		BagParams,
		PlayerRage,
		PlayerHealth,
		PlayerMagic,
		PlayerStrength,
		BagResources,
		BagElixirs,
		BagPoints,
		BagIce,
		BagFire,
		BagLighting,
		BagDark,
		Shop,
		ShopBuy,
		ShopResetButton,
		ShopResetTime,
		BattleAttack,
		ShopItemsCount,
		ExecutionHelp,
		MagicProtectionHelp,
		CastGestureHelp,
		GotLevel,
		GotSkillPoints,
		FightResultsTurns,
		FightResultsAddAnger,
		FightResultsAddExp,
		IntroductionText,
		ButtonOk,
		ButtonCancel,
		ButtonBuy,
		ButtonBuyAndPuton,
		ButtonSell,
		ButtonFight,
		ButtonStartOver,
		ButtonContinue,
		ButtonYes,
		ButtonNo,
		CompareChosen,
		ComparePuton,
		ButtonImprove,
		MagicBookHelp,
		LabelImmunity,
		LabelPersMagicPreferenses,
		ButtonPuton,
		MessageNoItems,
		FightResultsLabel,
		ExecutionLabel,
		ChoosePrize,
		FightResultsDamage,
		ButtonGoToBag,
		FightResultsTurnsLabel,
		ButtonMax,
		IntroPlace01,
		IntroTitle01,
		Intro01Text,
		Intro02Center,
		IntroPlace03,
		IntroTitle03,
		Intro03Text,
		Intro04Center,
		IntroPlace05,
		IntroTitle05,
		Intro05Text,
		WeakMagicMessage,
		LockpickingHelpTop,
		LockpickingHelpBottom,
		LockpickingFindAMatch,
		LockpickingWrongMatch,
		LockpickingMatchFound,
		LockpickingNoMoreLockpicks,
		LockpickingDifficultyLow,
		LockpickingDifficultyMedium,
		LockpickingDifficultyHigh,
		LockpickingPrize,
		LockpickingSecBeforeGame,
		InBattleNextBlock,
		InBattleNextCrit,
		InBattleBerserk,
		InBattleFightWith,
		InBattleChapter,
		FightResultsBlock,
		FightResultsDodge,
		FightResultsImmunity,
		FightResultsFatality,
		MagicIce,
		MagicFire,
		MagicLighting,
		MagicDark,
		MBookPower,
		MBookSkill,
		MBookNeedMoreSkulls,
		MBookNeedMoreSkill,
		OpsSoundsVolume,
		OpsMusicVolume,
		OpsRestartLabel,
		OpsRestartButton,
		OpsTeaserLabel,
		OpsTeaserButton,
		ExOpenConditionLabel,
		ExOpenButton,
		ExLootLabel,
		ButtonClose,
		SovHeader,
		SovText,
		ShopItemsUnlocked,
		ConditionNoMisses,
		ConditionBlocks,
		ConditionWinWithHP,
		ConditionFatalities,
		ConditionCritsInRow,
		ConditionKills,
		NewExecutionLabel,
		ButtonGoToShop,
		ShopDivision01,
		ShopDivision02,
		ShopDivision03,
		ShopDivision04,
		InsufficientFunds,
		ShopDivision05,
		ShopLevelTooLow,
		ShopAlreadyPurchased,
		ExCongratText1,
		ExCongratText2,
		Bank,
		BankCount,
		BankBonus,
		BankCost,
		LocationPopulationAtacked,
		LocationPopulationPeace,
		Tut1_1,
		Tut1_2,
		Tut2_1,
		Tut4_1,
		Tut4_2,
		Tut6_1,
		TutEye,
		TutStrongMagic,
		TutEye_2,
		Tut2_2,
		ShowTutorial,
		Tut4_1_1,
		Tut4_2_2,
		Tut6_1_2,
		ShopAlreadyPuton,
		ScarabFind,
		ScarabDig,
		TutResurrection,
		TutSearchTreasure,
		ChooseCharPreferences,
		ChooseCharBeginGame,
		Bonuses,
		BagSwitchParameters,
		BagSwitchItems,
		ScarabInsufficient,
		LocationPopulationMax,
		TutImmBattle,
		TutImmZ,
		PauseButtonResume,
		PauseButtonRestart,
		PauseButtonExit,
		OpsTutorialToggle,
		BattleLostStartOver,
		TutShop,
		TutBattleTurn,
		TutEndChests,
		TutFirstMoneyPile,
		TutPlayerDefeat,
		TutLocationScarab,
		TutBagRage,
		TutBagMana,
		TutBagExp,
		TutBagMoney,
		TutBagMagic,
		TutShopFilter2,
		TutShopFilter3,
		TutShopFilter4,
		TutShopFilter5,
		TutGotLevel,
		LockpickingButtonShowCells,
		AchievHeader,
		AchievDataHeader,
		AchievSocialLabel,
		ChooseCharFeature,
		LocationChestCountLabel,
		ItemQuantity,
		AchievementsProgress,
		AchievementsAll,
		AchievementsLocked,
		AchievementsUnlocked,
		AchievementsRating,
		TutFirstMobAttack,
		TutFirstMagicBook,
		CompareSellItem,
		ChoosePrizeExecution,
		SkillHeader,
		SkillComboName,
		SkillComboDesc,
		SkillMagicName,
		SkillMagicDesc,
		SkillRageName,
		SkillRageDesc,
		DefeatGoodsHeader,
		TutMagicBookButton,
		BankWaitMessage,
		BankBuyDoneSuccessfull,
		BankBuyDoneFail,
		BankTimeout,
		TutSliceAttack1,
		TutSliceAttack2,
		TutSliceAttack3,
		FinalText,
		TutAchievmentButton,
		MainMenuLoadGame,
		MainMenuSave,
		MainMenuNewGame,
		MainMenuNewGameAlert,
		MentorManName,
		MentorWomanName,
		DarkElfName,
		ElfPhrase1,
		ElfPhrase2,
		MsgGetRatingError,
		MsgRatingTimeout,
		ButtonFullRating,
		ButtonAddFriends,
		AddFriendsMessage,
		RatingSocialMessage,
		AlertNoInternet,
		RatingSocialMessageFacebookNotPosted,
		SocialAchivMessageTwitter,
		SocialAchivMessageFacebook,
		RatingSocialMessageTwitter,
		RatingSocialMessageFacebook,
		RatingSocialMessageFacebook341Error,
		BattleResultReal,
		BattleResultEtalon,
		BattleResultRating,
		BattleResultRatingName0,
		BattleResultRatingName1,
		BattleResultRatingName2,
		BattleResultRatingName3,
		BattleResultRatingName4,
		BattleResultTurnsCount,
		PostScreenshotToFacebook,
		ButtonPutoff,
		NoEpicItems,
		StarUpgradeHelp,
		NeedMoreStars,
		TutEpicItemPuton,
		TutFallingStarPhrase1,
		TutFallingStarPhrase2,
		EpicItemInfo,
		OpsFaqPage,
		Match3Title,
		Match3HelpBottom,
		Match3ButtonUseKeys,
		Match3BlockerText,
		Match3ButtonStart,
		Match3LootHeader,
		Match3DifficultyNormal,
		Match3DifficultyEasy,
		Match3Mined,
		Match3Rating,
		Match3Tutorial,
		Match3Mines,
		AvailableForMining,
		BuyFor,
		DealOfTheDay,
		Match3PointsCounter,
		Match3RecordCounter,
		Match3HelpRight,
		Match3ResultsPointsLabel,
		Match3ResultsRecordLabel,
		Match3ResultsButtonRestart,
		supportXCodeText,
		supportXCodeEMail,
		supportXCodeTheme,
		supportXCodeSend,
		supportXCodeClose,
		supportXCodeError,
		supportXCodeEnterEmail,
		supportXCodeEnterSubject,
		supportXCodeEnterDesc,
		supportXCodeAlertClose,
		supportXCodeInvEmail,
		supportXCodePosted,
		MsgGetRatingErrorAndroid,
		MsgRatingTimeoutAndroid,
		AddFriendsMessageAndroid
	}

	[ProtoContract]
	public class BotInfo
	{
		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2, IsRequired = false)]
		public int Level;

		[ProtoMember(3, IsRequired = false)]
		public int PeopleKilledForLevelUp;

		[ProtoMember(4, IsRequired = false)]
		public int MaxLevel;

		internal string Model;

		internal string Armor;

		internal string Picture;

		internal int Eyes = -1;

		internal string Title;

		internal float Scale;

		internal int _magic;

		internal MagicTypeE[] _magicImmunity;

		internal string[] ClosedActions;

		internal string skinColor;

		internal MagicTypeE Magic => (MagicTypeE)_magic;

		internal DamageTypeE MagicDamageType => Magic switch
		{
			MagicTypeE.Darkness => DamageTypeE.DarkMagic, 
			MagicTypeE.Fire => DamageTypeE.FireMagic, 
			MagicTypeE.Ice => DamageTypeE.IceMagic, 
			MagicTypeE.Lighting => DamageTypeE.LightingMagic, 
			_ => DamageTypeE.DarkMagic, 
		};

		public override string ToString()
		{
			return "<{0} {1} {2}>".Fmt(Id, Model, Title);
		}

		internal bool HasMagicImmunity(MagicTypeE type)
		{
			return _magicImmunity != null && _magicImmunity.IndexOf(type) >= 0;
		}
	}

	[ProtoContract]
	public class Slot
	{
		public enum TypeE
		{
			Helm = 1,
			Shoulder = 2,
			Boots = 3,
			Belt = 4,
			HandRight = 5,
			HandLeft = 6,
			Pelvis = 7,
			Torso = 8,
			Weapon = 9,
			Regalia = 10,
			Artefact = 11,
			Ring = 12,
			Earring = 13,
			Amulet = 14,
			Rashod1 = 15,
			Rashod2 = 16,
			Rashod3 = 17,
			Eyes = 18,
			RareSet = 20,
			EpicSet = 21,
			HalfEpicSet = 22,
			AlchemySet = 23
		}

		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public string Title;

		[ProtoMember(3)]
		public TypeE SlotId;

		public Slot()
		{
		}

		public Slot(int id, string title, int slotId)
		{
			Id = id;
			Title = title;
			SlotId = (TypeE)(int)Enum.ToObject(typeof(TypeE), slotId);
		}

		public Slot(TypeE id)
		{
			SlotId = id;
			Title = string.Empty;
		}

		public Slot(TypeE id, string title)
		{
			Id = (int)id;
			Title = title;
		}

		public bool Equals(Slot other)
		{
			if (object.ReferenceEquals(null, other))
			{
				return false;
			}
			if (object.ReferenceEquals(this, other))
			{
				return true;
			}
			return other.Id == Id;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(Slot))
			{
				return false;
			}
			return Equals((Slot)obj);
		}

		public override int GetHashCode()
		{
			return Id;
		}

		public static bool operator ==(Slot left, Slot right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(Slot left, Slot right)
		{
			return !object.Equals(left, right);
		}
	}

	[ProtoContract]
	public class Item
	{
		public enum ElixirTypeE
		{
			None,
			Heal,
			Critical,
			Poison,
			Key,
			Skull,
			Scarab,
			Gold,
			Diamond,
			Star
		}

		[ProtoMember(1)]
		public int Id;

		public string Title;

		public string TitleBlue;

		public string TitleGreen;

		public string Description;

		public Slot Slot;

		public int Class;

		public string Color;

		[ProtoMember(7)]
		public Dictionary<MoneyType, int> SellPrice = new Dictionary<MoneyType, int>();

		public int Sex;

		public int TotalWeight;

		public int LifeTime;

		public int DeathTime;

		public string Model;

		public string ModelBlue;

		public string ModelGreen;

		public string Picture;

		[ProtoMember(16)]
		public DateTime CreateTime;

		public int Set;

		[ProtoMember(18)]
		public SkillInfo[] Skills;

		[ProtoMember(19)]
		public bool PutOn;

		[ProtoMember(20)]
		public bool New;

		public int RashodkaEffect;

		public int RashodkaPower;

		public int RashodkaNextTurns;

		public int RashodkaEffectTurns;

		[ProtoMember(25)]
		public int RealItemsCount;

		public Fatality FatalityScenarioName;

		public int TotalWeightMax;

		public int GiveSkillsCount;

		public string PictureInBattle;

		internal int _maxStars;

		[ProtoMember(31, IsRequired = false)]
		public int CurrentStars;

		private Item c;

		public string TitleString
		{
			get
			{
				int num = ((SingletonT<ServerData>.I.PlayerServerPersData == null) ? (-1) : SingletonT<ServerData>.I.PlayerServerPersData.Class);
				if (num == 2 && !string.IsNullOrEmpty(TitleGreen))
				{
					return TitleGreen;
				}
				if (num == 3 && !string.IsNullOrEmpty(TitleBlue))
				{
					return TitleBlue;
				}
				return Title;
			}
		}

		public int MaxStars
		{
			get
			{
				foreach (ItemRelation itemRelation in SingletonT<ServerData>.I._itemRelations)
				{
					if (itemRelation.From.Id == Id)
					{
						return itemRelation.Count;
					}
				}
				return 0;
			}
			set
			{
				_maxStars = value;
			}
		}

		public ElixirTypeE ElixirType => (ElixirTypeE)RashodkaEffect;

		public bool IsArmorOrWeapon => IsArmor || IsWeapon;

		public bool IsAmulet
		{
			get
			{
				if (ElixirType != ElixirTypeE.None)
				{
					return false;
				}
				if (Slot == null)
				{
					return false;
				}
				Slot.TypeE slotId = Slot.SlotId;
				return slotId == Slot.TypeE.Artefact || slotId == Slot.TypeE.Amulet || slotId == Slot.TypeE.Earring || slotId == Slot.TypeE.Eyes || slotId == Slot.TypeE.Regalia || slotId == Slot.TypeE.Ring;
			}
		}

		public bool IsArmor
		{
			get
			{
				if (Slot == null)
				{
					return false;
				}
				Slot.TypeE slotId = Slot.SlotId;
				return slotId == Slot.TypeE.Belt || slotId == Slot.TypeE.Boots || slotId == Slot.TypeE.HandLeft || slotId == Slot.TypeE.HandRight || slotId == Slot.TypeE.Helm || slotId == Slot.TypeE.Pelvis || slotId == Slot.TypeE.Shoulder || slotId == Slot.TypeE.Torso;
			}
		}

		public bool IsWeapon => Slot != null && Slot.SlotId == Slot.TypeE.Weapon;

		public ShopGood ShopGood
		{
			get
			{
				foreach (KeyValuePair<int, ShopGood> shopGood in SingletonT<ServerData>.I._shopGoods)
				{
					if (shopGood.Value.Item.Id == Id)
					{
						return shopGood.Value;
					}
				}
				return null;
			}
		}

		public Item()
		{
			Id = 0;
		}

		public Item(int id)
		{
			Id = id;
		}

		public Item(Item loaded, Item fromServer)
		{
			Utils.Log("ITEM loaded=", loaded, "fromServer=", fromServer);
			Id = fromServer.Id;
			Title = fromServer.Title;
			TitleBlue = fromServer.TitleBlue;
			TitleGreen = fromServer.TitleGreen;
			Description = fromServer.Description;
			Slot = fromServer.Slot;
			Class = fromServer.Class;
			Color = fromServer.Color;
			Sex = fromServer.Sex;
			TotalWeight = fromServer.TotalWeight;
			LifeTime = fromServer.LifeTime;
			DeathTime = fromServer.DeathTime;
			Model = fromServer.Model;
			ModelBlue = fromServer.ModelBlue;
			ModelGreen = fromServer.ModelGreen;
			Picture = fromServer.Picture;
			Set = fromServer.Set;
			RashodkaEffect = fromServer.RashodkaEffect;
			RashodkaPower = fromServer.RashodkaPower;
			RashodkaNextTurns = fromServer.RashodkaNextTurns;
			RashodkaEffectTurns = fromServer.RashodkaEffectTurns;
			FatalityScenarioName = fromServer.FatalityScenarioName;
			TotalWeightMax = fromServer.TotalWeightMax;
			GiveSkillsCount = fromServer.GiveSkillsCount;
			PictureInBattle = fromServer.PictureInBattle;
			SellPrice = loaded.SellPrice;
			CreateTime = loaded.CreateTime;
			Skills = loaded.Skills;
			MaxStars = fromServer.MaxStars;
			CurrentStars = loaded.CurrentStars;
			if (Skills == null)
			{
				Skills = new SkillInfo[0];
			}
			else
			{
				List<SkillInfo> list = new List<SkillInfo>();
				SkillInfo[] skills = Skills;
				foreach (SkillInfo skillInfo in skills)
				{
					if (skillInfo == null)
					{
						Utils.Log("ITEM LOAD WRONG DATA", Id);
						continue;
					}
					if (skillInfo.Skill == null)
					{
						Utils.Log("ITEM LOAD WRONG DATA2", Id);
						continue;
					}
					Skill skill = SingletonT<ServerData>.I.GetSkill(skillInfo.Skill.Type);
					if (skill != null)
					{
						skillInfo.Skill.Title = skill.Title;
						list.Add(skillInfo);
					}
					else
					{
						Utils.Log("ITEM LOAD WRONG DATA3", Id);
					}
				}
				Skills = list.ToArray();
			}
			PutOn = loaded.PutOn;
			New = loaded.New;
			RealItemsCount = loaded.RealItemsCount;
		}

		internal string GetSuffix(int personClass)
		{
			string text = Model;
			if (personClass == 2)
			{
				text = ModelGreen;
			}
			if (personClass == 3)
			{
				text = ModelBlue;
			}
			try
			{
				string text2 = ((text == null) ? string.Empty : text);
				int num = text.IndexOf("#");
				if (num < 0 && PictureInBattle != null)
				{
					num = PictureInBattle.IndexOf("#");
					text2 = PictureInBattle;
				}
				if (num < 0 && Picture != null)
				{
					num = Picture.IndexOf("#");
					text2 = Picture;
				}
				text2 = text2.Substring(num + 1);
				if (text2.Length == 0)
				{
					Utils.Log("SUFFIX IS EMPTY", Id, text);
				}
				return text2;
			}
			catch (Exception ex)
			{
				Utils.Log("SUFFIX GET FAILED", this, text, ex);
				return string.Empty;
			}
		}

		public string Get3DModel()
		{
			PersData playerServerPersData = SingletonT<ServerData>.I.PlayerServerPersData;
			string text = Model;
			try
			{
				if (playerServerPersData.Class != 0)
				{
					if (playerServerPersData.Class == 2 && ModelGreen != null)
					{
						text = ModelGreen;
					}
					if (playerServerPersData.Class == 3 && ModelBlue != null)
					{
						text = ModelBlue;
					}
					text = text.Substring(0, text.IndexOf("#"));
				}
			}
			catch (Exception ex)
			{
				Utils.Log("GET3DMODEL EX", Model, text, ex.Message);
				throw;
			}
			return text;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("skills={");
			if (Skills != null)
			{
				SkillInfo[] skills = Skills;
				foreach (SkillInfo skillInfo in skills)
				{
					stringBuilder.Append(skillInfo.Skill.Title + "=" + skillInfo.Min + "; ");
				}
			}
			stringBuilder.Append("}");
			return Utils.ParamsToString("Item [", Id, Title, ElixirType, stringBuilder, Model, ElixirType, RealItemsCount, "]");
		}

		private List<SkillInfo> GenSkillsToGen()
		{
			if (Skills == null)
			{
				return new List<SkillInfo>();
			}
			int num = ((Skills.Length <= GiveSkillsCount) ? Skills.Length : GiveSkillsCount);
			List<SkillInfo> skillsToGen = new List<SkillInfo>(num);
			Utils.Random(Skills, (SkillInfo _) => 1, num, allowDuplicates: false, delegate(int number, int index)
			{
				Invs.Inv(!skillsToGen.Contains(Skills[index]), "!skillsToGen.Contains(Skills[index])");
				skillsToGen.Add(Skills[index]);
			});
			return skillsToGen;
		}

		public Item MakeRealItem(bool forShop, int count)
		{
			Item item = new Item(Id);
			try
			{
				item.CreateTime = DateTime.Now;
				item.RashodkaPower = RashodkaPower;
				item.RashodkaNextTurns = RashodkaNextTurns;
				item.RashodkaEffectTurns = RashodkaEffectTurns;
				item.RashodkaEffect = RashodkaEffect;
				item.Title = Title;
				item.TitleGreen = TitleGreen;
				item.TitleBlue = TitleBlue;
				item.Description = Description;
				item.Slot = Slot;
				item.Class = Class;
				item.Color = Color;
				item.SellPrice = SellPrice;
				item.Sex = Sex;
				item.LifeTime = LifeTime;
				item.DeathTime = DeathTime;
				item.Model = Model;
				item.ModelGreen = ModelGreen;
				item.ModelBlue = ModelBlue;
				item.Picture = Picture;
				item.Set = Set;
				item.PictureInBattle = PictureInBattle;
				item.PutOn = false;
				item.New = true;
				item.RealItemsCount = count;
				item.FatalityScenarioName = FatalityScenarioName;
				item.MaxStars = MaxStars;
				List<SkillInfo> list = new List<SkillInfo>();
				if (Skills == null)
				{
					Skills = new SkillInfo[0];
				}
				SkillInfo[] skills = Skills;
				foreach (SkillInfo skillInfo in skills)
				{
					list.Add(skillInfo.Clone());
				}
				for (int j = 0; j < list.Count; j++)
				{
					list[j].Min = ((!forShop) ? UnityEngine.Random.Range(Skills[j].Min, Skills[j].Max + 1) : Skills[j].Max);
				}
				item.Skills = list.ToArray();
				return item;
			}
			catch (Exception e)
			{
				Utils.LogForce("MAKEREALITEM failed", e.MessageAndStacktraceWithInners(string.Empty, "\n", string.Empty), this);
				throw;
			}
		}

		public bool HasSkill(Skill.TypeE type)
		{
			if (Skills == null)
			{
				return false;
			}
			SkillInfo[] skills = Skills;
			foreach (SkillInfo skillInfo in skills)
			{
				if (skillInfo.Skill != null && skillInfo.Skill.Type == type)
				{
					return true;
				}
			}
			return false;
		}

		public int GetSkill(Skill.TypeE type, int defaultValue)
		{
			if (Skills == null)
			{
				return defaultValue;
			}
			SkillInfo[] skills = Skills;
			foreach (SkillInfo skillInfo in skills)
			{
				if (skillInfo.Skill != null && skillInfo.Skill.Type == type)
				{
					return skillInfo.Min;
				}
			}
			return defaultValue;
		}
	}

	[ProtoContract]
	public class ShopGood
	{
		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public Item Item;

		[ProtoMember(3)]
		public int Probability;

		[ProtoMember(4)]
		public int LevelMin;

		[ProtoMember(5)]
		public int LevelMax;

		[ProtoMember(6)]
		public int Count;

		public int Discount;

		public bool Relict;

		public Dictionary<MoneyType, int> Price = new Dictionary<MoneyType, int>();

		internal int GetPrice(MoneyType.TypeE type)
		{
			foreach (MoneyType key in Price.Keys)
			{
				if (key.Type == type)
				{
					return Price[key];
				}
			}
			return -1;
		}

		internal ShopGood MakeRealItem(bool forShop)
		{
			Invs.Inv(Item != null, "Item != null");
			ShopGood shopGood = new ShopGood();
			shopGood.Id = Id;
			shopGood.Item = Item.MakeRealItem(forShop, Count);
			shopGood.LevelMin = LevelMin;
			shopGood.LevelMax = LevelMax;
			shopGood.Count = Count;
			shopGood.Price = Price;
			shopGood.Relict = Relict;
			shopGood.Discount = Discount;
			return shopGood;
		}

		public override string ToString()
		{
			return Utils.ParamsToString("Id=", Id, "Item=", Item, "LevelMin=", LevelMin, "LevelMax=", LevelMax, "Relict=", Relict, "Discount=", Discount);
		}
	}

	[ProtoContract]
	public class Subtitle
	{
		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public string Video;

		[ProtoMember(3)]
		public string Text;

		[ProtoMember(4)]
		public int StartTime;

		[ProtoMember(5)]
		public int EndTime;
	}

	public class PersData
	{
		public int Id;

		public string Title;

		public string Description;

		public string Feature;

		public int Class;

		public int Sex;

		public string Cloth;

		public Dictionary<MoneyType, int> Money = new Dictionary<MoneyType, int>();

		public SkillInfo[] Skills;

		public string SelectSet = "1";

		public string SelectWeapon = "1";

		public string SelectHair;

		public int SelectHairColor = 1;

		public Spell StartSpell;

		public Item Bonus;

		public int SkillsPoints;

		public bool IsClassRed => Class == 1;

		public bool IsClassGreen => Class == 2;

		public bool IsClassBlue => Class == 3;

		public bool IsMan => Sex == 1;

		public string ModelId => (!IsMan) ? "2" : "1";

		public int ArmorSet => -1;

		public int GetSkill(Skill.TypeE type, int def)
		{
			for (int i = 0; i < Skills.Length; i++)
			{
				if (Skills[i].Skill.Type == type)
				{
					return Skills[i].Min;
				}
			}
			return def;
		}
	}

	public class Spell
	{
		public int Id;

		public string Title;

		public string Description;

		public int Level;

		public string SchoolName;

		public Dictionary<MoneyType, int> Price = new Dictionary<MoneyType, int>();

		public string IconName;

		public string EffectName;

		public bool InMagicBook;

		public string UpdateId;

		public float PowerK;

		public Spell NextSpell;

		public int Points;

		public Skill.TypeE SkillType
		{
			get
			{
				if (SchoolName == "1")
				{
					return Skill.TypeE.MagicDark;
				}
				if (SchoolName == "2")
				{
					return Skill.TypeE.MagicFire;
				}
				if (SchoolName == "3")
				{
					return Skill.TypeE.MagicIce;
				}
				if (SchoolName == "4")
				{
					return Skill.TypeE.MagicElectro;
				}
				return Skill.TypeE.Unknown;
			}
		}

		public bool IsFire => SchoolName == "2";

		public bool Equals(Spell other)
		{
			if (object.ReferenceEquals(null, other))
			{
				return false;
			}
			if (object.ReferenceEquals(this, other))
			{
				return true;
			}
			return other.Id == Id && other.PowerK.Equals(PowerK) && other.Level == Level && object.Equals(other.Title, Title) && object.Equals(other.SchoolName, SchoolName);
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(Spell))
			{
				return false;
			}
			return Equals((Spell)obj);
		}

		public override int GetHashCode()
		{
			int id = Id;
			id = (id * 397) ^ PowerK.GetHashCode();
			id = (id * 397) ^ Level;
			id = (id * 397) ^ ((Title != null) ? Title.GetHashCode() : 0);
			return (id * 397) ^ ((SchoolName != null) ? SchoolName.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return $"Id: {Id}, Title: {Title}, Level: {Level}, SchoolName: {SchoolName}, PowerK: {PowerK}";
		}

		public static bool operator ==(Spell left, Spell right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(Spell left, Spell right)
		{
			return !object.Equals(left, right);
		}
	}

	[ProtoContract]
	public class Fatality
	{
		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public string Title;

		[ProtoMember(3)]
		public string Scenario;

		[ProtoMember(4)]
		public string WeaponString;
	}

	[ProtoContract]
	public class LevelData
	{
		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public string Title;

		[ProtoMember(3)]
		public int Level;

		[ProtoMember(4)]
		public int SkillPoints;

		[ProtoMember(5)]
		public int Exp;
	}

	public enum HintCodesE
	{
		none = 9999,
		money = 0,
		cristals = 1,
		sculls = 2,
		vzlom = 3,
		healpotion = 4,
		powerpotion = 5,
		damagepotion = 6,
		stathp = 7,
		statfury = 8,
		statstrength = 9,
		statmagic = 10,
		bonusfire = 11,
		bonusice = 12,
		bonusdark = 13,
		bonuslightning = 14,
		expa = 15,
		bonusexp = 16,
		bonusmoney = 17,
		bonusmana = 18,
		bonusrage = 19,
		scarab = 20,
		map = 21,
		bonusarhimagia = 22,
		bonusarhifury = 23,
		socialbutton = 24,
		achivmentprogressbar = 25,
		legendarypoints = 26
	}

	[ProtoContract]
	public struct HintData
	{
		[ProtoMember(1)]
		public string Name;

		[ProtoMember(2)]
		public string Text;
	}

	[ProtoContract]
	public class ServerColor
	{
		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public string Title;

		[ProtoMember(3)]
		public string Code;
	}

	public class Chest
	{
		internal int Id;

		internal int PicturesCount;

		internal int Type;

		internal Bonus Bonus;

		internal string Index;

		internal int ElfProbability;

		internal int ElfLevelDifference;

		internal int ElfModel;
	}

	[ProtoContract]
	[ProtoInclude(10000, typeof(PlayerParamsData))]
	public class PersonParams
	{
		[ProtoMember(601)]
		public int Magic = 60;

		[ProtoMember(602)]
		public int HP = 100;

		[ProtoMember(603)]
		public int Strength = 120;

		[ProtoMember(604)]
		public int Rage = 80;

		public int BonusExp;

		public int BonusMana;

		public int BonusRage;

		public int BonusMoney;

		public Dictionary<Skill.TypeE, float> SkillsK = new Dictionary<Skill.TypeE, float>();

		[ProtoMember(1)]
		public int Level = 1;

		public override string ToString()
		{
			return Utils.ParamsToString("Magic=", Magic, "HP=", HP, "Strength=", Strength, "Rage=", Rage);
		}
	}

	[ProtoContract]
	public class PlayerParamsData : PersonParams
	{
		[ProtoMember(2)]
		public int _experience;

		[ProtoMember(3)]
		public int GoldCount;

		[ProtoMember(4)]
		public int DiamondCount;

		[ProtoMember(5)]
		public int SkullsCount;

		[ProtoMember(7)]
		public int KeysCount;

		[ProtoMember(8)]
		public int _skillPoints;

		[ProtoMember(9)]
		public int _anger;

		[ProtoMember(10)]
		public DateTime LastBerserkTime = DateTime.MinValue;

		private int _fatalitiesSpheresCount;

		[ProtoMember(12, IsRequired = false)]
		public int _rageSpheresCount;

		[ProtoMember(13, IsRequired = false)]
		public int _mana;

		[ProtoMember(14, IsRequired = false)]
		public Dictionary<Skill.TypeE, int> SpellsUsedCount = new Dictionary<Skill.TypeE, int>();

		[ProtoMember(15)]
		public int ScarabCount;

		[ProtoMember(16, IsRequired = false)]
		public int StarsCount;

		[ProtoMember(17, IsRequired = false)]
		public int BonusStarsForOldSave;

		[ProtoMember(18, IsRequired = false)]
		public int Match3Record;

		[ProtoMember(19, IsRequired = false)]
		public int Match3GoldMined;

		[ProtoMember(20, IsRequired = false)]
		public int Match3CrystallMined;

		[ProtoMember(21, IsRequired = false)]
		public int Match3SkullMined;

		[ProtoMember(22, IsRequired = false)]
		public int Match3StarMined;

		[ProtoMember(23, IsRequired = false)]
		public int Match3ScarabMined;

		public int Experience => _experience;

		public int MoneyGoldCount
		{
			get
			{
				return GoldCount;
			}
			set
			{
				if (GoldCount != value)
				{
					GoldCount = value;
					Messenger.Invoke(Globals.MsgPlayerFundsChanged, MoneyType.TypeE.Gold, "real_change");
				}
			}
		}

		public int MoneyDiamondCount
		{
			get
			{
				return DiamondCount;
			}
			set
			{
				if (DiamondCount != value)
				{
					DiamondCount = value;
					Messenger.Invoke(Globals.MsgPlayerFundsChanged, MoneyType.TypeE.Diamond, "real_change");
				}
			}
		}

		public int MoneySkullsCount
		{
			get
			{
				return SkullsCount;
			}
			set
			{
				if (SkullsCount != value)
				{
					SkullsCount = value;
					Messenger.Invoke(Globals.MsgPlayerFundsChanged, MoneyType.TypeE.Skull, "real_change");
				}
			}
		}

		public int MoneyKeysCount
		{
			get
			{
				return KeysCount;
			}
			set
			{
				if (KeysCount != value)
				{
					KeysCount = value;
					Messenger.Invoke(Globals.MsgPlayerFundsChanged, MoneyType.TypeE.Key, "real_change");
				}
			}
		}

		public int SkillPoints => _skillPoints;

		public int Anger
		{
			get
			{
				return _anger;
			}
			set
			{
				if (!Globals.NewBerserkMode && _anger != value)
				{
					_anger = ((value <= 100) ? value : 100);
					Utils.Log("ANGER", value);
					if (_anger == 100)
					{
						LastBerserkTime = DateTime.Now;
					}
					Messenger<int>.Invoke(Globals.MsgPlayerAngerChanged, _anger);
				}
			}
		}

		internal int ResurrectionSpheresCount
		{
			get
			{
				return _fatalitiesSpheresCount;
			}
			set
			{
				if (_fatalitiesSpheresCount != value)
				{
					_fatalitiesSpheresCount = value;
					Messenger<int>.Invoke(Globals.MsgResurrectionCountChanged, value);
				}
			}
		}

		internal int RageSpheresCount
		{
			get
			{
				return _rageSpheresCount;
			}
			set
			{
				if (_rageSpheresCount != value)
				{
					if (value < 0)
					{
						value = 0;
					}
					else if (value > SingletonT<ServerData>.I.GameSettings.MaxRage)
					{
						value = SingletonT<ServerData>.I.GameSettings.MaxRage;
					}
					_rageSpheresCount = value;
					Utils.Log("RageSpheresCount changed pre msg", value);
					Messenger<int>.Invoke(Globals.MsgRageSpheresCountChanged, value);
					Utils.Log("RageSpheresCount changed post msg", value);
				}
			}
		}

		public int MoneyScarabCount
		{
			get
			{
				return ScarabCount;
			}
			set
			{
				if (ScarabCount != value)
				{
					ScarabCount = value;
					Messenger.Invoke(Globals.MsgPlayerFundsChanged, MoneyType.TypeE.Scarab, "real_change");
				}
			}
		}

		public int MoneyStarsCount
		{
			get
			{
				return StarsCount;
			}
			set
			{
				if (StarsCount != value)
				{
					StarsCount = value;
					Messenger.Invoke(Globals.MsgPlayerFundsChanged, MoneyType.TypeE.Star, "real_change");
				}
			}
		}

		public void SetSpellsUsedCount(Dictionary<Skill.TypeE, int> list)
		{
			if (SpellsUsedCount != null)
			{
				foreach (KeyValuePair<Skill.TypeE, int> item in SpellsUsedCount)
				{
					if (!list.ContainsKey(item.Key))
					{
						Messenger.Invoke(Globals.MsgSpellUsedCountChanged, item.Key, 0);
					}
				}
			}
			SpellsUsedCount = list;
			foreach (KeyValuePair<Skill.TypeE, int> item2 in list)
			{
				Messenger.Invoke(Globals.MsgSpellUsedCountChanged, item2.Key, item2.Value);
			}
		}

		internal void Reset(PersData persData)
		{
			Level = 1;
			_experience = 0;
			MoneyGoldCount = 0;
			MoneyDiamondCount = 0;
			MoneySkullsCount = 0;
			MoneyKeysCount = 0;
			MoneyScarabCount = 0;
			MoneyStarsCount = 0;
			_anger = 0;
			_skillPoints = 0;
			_anger = 0;
			_rageSpheresCount = 0;
			_mana = 0;
			SetSpellsUsedCount(new Dictionary<Skill.TypeE, int>());
			LastBerserkTime = DateTime.MinValue;
			foreach (KeyValuePair<MoneyType, int> item in persData.Money)
			{
				AddMoney(item.Key.Type, item.Value);
			}
			Utils.Log("RESETPLAYER", persData);
			SkillInfo[] skills = persData.Skills;
			foreach (SkillInfo skillInfo in skills)
			{
				Utils.Log("INITSKILL", skillInfo.Skill.Type, skillInfo.Min);
				if (skillInfo.Skill.Type == Skill.TypeE.Magic)
				{
					Magic = skillInfo.Min;
				}
				else if (skillInfo.Skill.Type == Skill.TypeE.Rage)
				{
					Rage = skillInfo.Min;
				}
				else if (skillInfo.Skill.Type == Skill.TypeE.Strength)
				{
					Strength = skillInfo.Min;
				}
				else if (skillInfo.Skill.Type == Skill.TypeE.Vitality)
				{
					HP = skillInfo.Min;
				}
			}
			Messenger.Invoke(Globals.MsgRageSpheresCountChanged, 0);
			Messenger.Invoke(Globals.MsgPlayerExpChanged, 0);
			Messenger.Invoke(Globals.MsgPlayerSkillPointsChanged);
			Messenger.Invoke(Globals.MsgPlayerLevelChanged, 1, 1, "reset");
		}

		internal void AddMoney(MoneyType.TypeE type, int count)
		{
			if (count > 0)
			{
				switch (type)
				{
				case MoneyType.TypeE.Diamond:
					MoneyDiamondCount += count;
					break;
				case MoneyType.TypeE.Gold:
					MoneyGoldCount += count;
					break;
				case MoneyType.TypeE.Key:
					MoneyKeysCount += count;
					break;
				case MoneyType.TypeE.Skull:
					MoneySkullsCount += count;
					break;
				case MoneyType.TypeE.Scarab:
					MoneyScarabCount += count;
					break;
				case MoneyType.TypeE.Star:
					MoneyStarsCount += count;
					break;
				}
			}
		}

		internal void LoadFrom(PlayerParamsData data)
		{
			Utils.Log(data.Level, data._experience, data.MoneyGoldCount, data.MoneyDiamondCount, data.MoneySkullsCount, data.MoneyKeysCount, data._skillPoints, data._anger);
			Level = data.Level;
			_experience = data._experience;
			_rageSpheresCount = data._rageSpheresCount;
			_mana = data._mana;
			MoneyGoldCount = data.MoneyGoldCount;
			MoneyDiamondCount = data.MoneyDiamondCount;
			MoneySkullsCount = data.MoneySkullsCount;
			MoneyKeysCount = data.MoneyKeysCount;
			MoneyScarabCount = data.MoneyScarabCount;
			MoneyStarsCount = data.MoneyStarsCount;
			BonusStarsForOldSave = data.BonusStarsForOldSave;
			Match3Record = data.Match3Record;
			Match3GoldMined = data.Match3GoldMined;
			Match3CrystallMined = data.Match3CrystallMined;
			Match3SkullMined = data.Match3SkullMined;
			Match3StarMined = data.Match3StarMined;
			Match3ScarabMined = data.Match3ScarabMined;
			_skillPoints = data._skillPoints;
			_anger = data._anger;
			Utils.Log("LoadFrom ", data.Rage, data.Magic, data.HP, data.Strength);
			Rage = data.Rage;
			Magic = data.Magic;
			HP = data.HP;
			Strength = data.Strength;
			LastBerserkTime = data.LastBerserkTime;
			SetSpellsUsedCount(data.SpellsUsedCount);
		}

		internal void SpellCasted(Spell spell)
		{
			Skill.TypeE skillType = spell.SkillType;
			if (!SpellsUsedCount.ContainsKey(skillType))
			{
				SpellsUsedCount.Add(skillType, 0);
			}
			if (SpellsUsedCount[skillType] < spell.Points)
			{
				Dictionary<Skill.TypeE, int> spellsUsedCount;
				Dictionary<Skill.TypeE, int> dictionary = (spellsUsedCount = SpellsUsedCount);
				Skill.TypeE key2;
				Skill.TypeE key = (key2 = skillType);
				int num = spellsUsedCount[key2];
				num = (dictionary[key] = num + 1);
				int arg = num;
				Messenger.Invoke(Globals.MsgSpellUsedCountChanged, spell.SkillType, arg);
			}
		}
	}

	public class MobParamsData : PersonParams
	{
		internal AreaData.MobData MobData;
	}

	[ProtoContract]
	public class BattleParams
	{
		[ProtoMember(1)]
		[DefaultValue(5)]
		public double ManaC = 5.0;

		[DefaultValue(60)]
		[ProtoMember(2)]
		public double magSec0 = 60.0;

		[ProtoMember(3)]
		[DefaultValue(1.083)]
		public double manaK = 1.083;

		[DefaultValue(1.083)]
		[ProtoMember(4)]
		public double critK = 1.083;

		[DefaultValue(1.083)]
		[ProtoMember(5)]
		public double magK = 1.083;

		[ProtoMember(6)]
		public double persMR;

		[ProtoMember(7)]
		public double persCR;

		[ProtoMember(8)]
		[DefaultValue(0.3)]
		public double pCrit = 0.3;

		[ProtoMember(9)]
		[DefaultValue(0.1996)]
		public double paCrit = 0.1996;

		[DefaultValue(80)]
		[ProtoMember(10)]
		public double critSec0 = 80.0;

		[ProtoMember(11)]
		[DefaultValue(1.125)]
		public double pMag = 1.125;

		[DefaultValue(0.2456)]
		[ProtoMember(12)]
		public double paMag = 0.2456;

		[ProtoMember(13)]
		[DefaultValue(60)]
		public double manaSec0 = 60.0;

		public override string ToString()
		{
			return Utils.ParamsToString("ManaC", ManaC, "sec0", magSec0, "c", critK, "persMR", persMR, "persCR", persCR, "pCrit", pCrit, "paCrit", paCrit, "critSec0", critSec0, "pMag", pMag, "paMag", paMag);
		}
	}

	public class Mine
	{
		internal int Id = -1;

		internal int Order;

		internal Bonus FirstBonus;

		internal Bonus SecondBonus;

		internal int Difficulty_;

		public Dictionary<MoneyType, int> Price = new Dictionary<MoneyType, int>();

		public Dictionary<MoneyType, int> OpenPrice = new Dictionary<MoneyType, int>();

		public Dictionary<MoneyType, int> ContinuePrice = new Dictionary<MoneyType, int>();

		public Match3Hud.DifficultyE Difficulty
		{
			get
			{
				if (Difficulty_ == 1)
				{
					return Match3Hud.DifficultyE.Easy;
				}
				return Match3Hud.DifficultyE.Normal;
			}
		}

		public bool IsBuyed => BuyedMines.Contains(Id) || Price.Free();

		public void Buy()
		{
			BuyMine(this);
		}
	}

	internal class ItemRelation
	{
		internal Item From;

		internal Item To;

		internal int Type;

		internal int Count;
	}

	[ProtoContract]
	public struct MobRespProb
	{
		[ProtoMember(1)]
		public int Count;

		[ProtoMember(2)]
		public int Prob;
	}

	public class Settings
	{
		internal int BerserkLength;

		internal int BerserkDamage;

		internal int BerserkMaxHealth;

		internal int AngerDownSpeed;

		internal int AngerPerBattle;

		internal int AngerPerFatality;

		internal int SlicesForFatality;

		internal int FatalityTime;

		internal int SellPrice;

		internal float ComboK;

		internal float CritK;

		internal float UpdateShopPeriodSeconds;

		internal int LocationMobLevel;

		internal int LocationMobLevelOffset;

		internal int LocationMobLevelMax;

		internal int LocationMobLevelUpCost = 1;

		internal float TurnTime = 5f;

		internal float MobMagicDamageWeakK;

		internal float MobMagicDamageStrongK;

		internal int mgBM1;

		internal int mgBM2;

		internal int mgBM3;

		internal int mgBM4;

		internal int mg2BM1;

		internal int mg2BM2;

		internal int mg2BM3;

		internal int mg2BM4;

		internal int mg2Time;

		internal int ChestPeriod;

		internal int ChestLifeTime;

		internal float TimeTickMagicProtection;

		internal int MagicProtectionTime1;

		internal int MagicProtectionTime2;

		internal int MagicProtectionTime3;

		internal int Crits2StarsPercent;

		internal int Crits3StarsPercent;

		internal int Crits4StarsPercent;

		internal int Crits5StarsPercent;

		internal int FallingStarCooldown = 60;

		internal int FallingStarProb;

		internal int FallingStarTime = 900;

		public int Match3TimerNormal;

		public int Match3TimerEasy;

		public int Match3TimeKeysCostNormal;

		public int Match3TimeKeysCostEasy;

		public int Match3BonusProbNormal;

		public int Match3BonusProbEasy;

		internal bool ShowEvolutionPromoInWorld;

		internal bool ShowEvolutionPromoInRu;

		internal string UrlEvolution = string.Empty;

		internal string UrlEvolutionAndroid = string.Empty;

		private float _musicVolume = -1f;

		private float _soundsVolume = -1f;

		internal float MusicChangeTime = 2f;

		internal float IntroFadeInOutTime = 1f;

		internal float IntroReadingTime = 1f;

		internal float IntroDayPassTime = 1f;

		internal int ChestsMaxCount;

		internal int ProbFatality;

		internal int CritOnPersonManaBallsCount = 2;

		internal int RageShereProbSimple;

		internal int RageShereProbCrit;

		internal int EyePeriod;

		internal int EyeProb1;

		internal int EyeProb2;

		internal int EyeProb3;

		internal int manaInBall;

		internal int critMBCount;

		internal int critMBCountFromRages;

		internal int manaOCount;

		internal int manaOProb;

		internal int manaOInitProb;

		internal int manaBonusProb;

		internal int rageBonusProb;

		internal float czRadius;

		internal float czInterpolationSpeed = 1.8f;

		internal float MagicRes = 1f;

		internal float IdlePeriod = 5f;

		internal int BloodScreenHealthTreshold = 50;

		internal int BloodScreenHealthTreshold2 = 50;

		internal int BloodScreenHealthTreshold3 = 50;

		internal int ComboManaBalls = 4;

		internal string DefaultRedWeapon = "12";

		internal string DefaultGreenWeapon = "12";

		internal string DefaultBlueWeapon = "12";

		internal Fatality DefaultFatality;

		internal string TeaserPage = "http://juggermobile.com/";

		internal string FaqPage = "http://juggermobile.com/faq.php";

		internal string FacebookCommunityUrl = ((!UnityApi.UseGameClub()) ? "http://www.facebook.com/JuggerMobileGame?ref=hl" : "http://cafe.naver.com/gameclubmini");

		internal string FacebookCommunityUrlNew = "http://www.facebook.com/pages/Juggerlive/455504447837571?fref=ts";

		internal Location.BotLocationInfo[] Elfs;

		internal int ElfPeriod;

		internal int ElfProb;

		internal int Idle2Prob = 50;

		internal Bonus ElfBonus;

		internal Bonus MonsterFromLocationBonus;

		internal int AchievmentSharingMoneyBonus = 30;

		internal int RatingSocialCulldown = 360;

		internal int RatingSharingMoneyBonus = 1;

		internal int LevelCheckNotifs = 7;

		internal int SkillComboLevel = 2;

		internal int SkillMagicLevel = 3;

		internal int SkillRageLevel = 4;

		internal readonly int MaxRage = 10;

		internal MobRespProb[] MobsRespawnProbs;

		internal int MobsRespCooldown = 60;

		internal bool ShowFps;

		internal string BankFree = string.Empty;

		internal int sovLevS0;

		internal int sovLevS1;

		internal int sovLevS2;

		internal int MaxMana => 10;

		internal float MusicVolume
		{
			get
			{
				if (_musicVolume < 0f)
				{
					_musicVolume = 1f;
					if (PlayerPrefs.HasKey(Globals.PlayerPrefMusicVolume))
					{
						_musicVolume = PlayerPrefs.GetFloat(Globals.PlayerPrefMusicVolume);
					}
				}
				return _musicVolume;
			}
			set
			{
				if (value < 0f)
				{
					value = 0f;
				}
				if (_musicVolume != value)
				{
					_musicVolume = value;
					PlayerPrefs.SetFloat(Globals.PlayerPrefMusicVolume, _musicVolume);
				}
			}
		}

		internal float SoundsVolume
		{
			get
			{
				if (_soundsVolume < 0f)
				{
					_soundsVolume = 1f;
					if (PlayerPrefs.HasKey(Globals.PlayerPrefSoundVolume))
					{
						_soundsVolume = PlayerPrefs.GetFloat(Globals.PlayerPrefSoundVolume);
					}
				}
				return _soundsVolume;
			}
			set
			{
				if (value < 0f)
				{
					value = 0f;
				}
				if (_soundsVolume != value)
				{
					_soundsVolume = value;
					PlayerPrefs.SetFloat(Globals.PlayerPrefSoundVolume, _soundsVolume);
					Messenger<float>.Invoke(Globals.MsgSoundsVolumeChanged, value);
				}
			}
		}

		internal bool IsElf(BotInfo bot)
		{
			Location.BotLocationInfo[] elfs = Elfs;
			foreach (Location.BotLocationInfo botLocationInfo in elfs)
			{
				if (botLocationInfo.Bot.Id == bot.Id)
				{
					return true;
				}
			}
			return false;
		}
	}

	public class StorylineDialog
	{
		internal int Id;

		internal string Title;

		internal Tuple<int, int, int> LocationBot;

		internal List<DialogPhrase> Dialogs = new List<DialogPhrase>();
	}

	[ProtoContract]
	public class DialogPhrase
	{
		[ProtoMember(1)]
		[DefaultValue(-1)]
		public int Npc = -1;

		[ProtoMember(2)]
		public string FemaleText;

		[ProtoMember(3)]
		public string MaleText;

		[ProtoMember(4)]
		public string Origin;

		internal string Text
		{
			get
			{
				PersData playerServerPersData = SingletonT<ServerData>.I.PlayerServerPersData;
				if (playerServerPersData != null && !string.IsNullOrEmpty(FemaleText) && !playerServerPersData.IsMan)
				{
					return FemaleText;
				}
				return MaleText;
			}
		}
	}

	[ProtoContract]
	public class Npc
	{
		[ProtoMember(1)]
		public int Id;

		[ProtoMember(2)]
		public string Title;

		[ProtoMember(3)]
		public string Picture;
	}

	public class FileInfo
	{
		public string Path;

		public string Name;

		public long Time;

		public byte[] OriginalBytes;

		public string OriginalText;

		public string OriginalText2;

		public bool Changed;

		public override string ToString()
		{
			return $"{Path} : {Name}";
		}
	}

	public delegate void LoadingTipsReadyHandler(ServerData data);

	public const int ArmorGoldPriority = 1000000;

	public const int RecommendedOnlyPriority = 2000000;

	public const int OtherGoodsPriority = 3000000;

	public const int DiamondSetsPriority = 4000000;

	public const int OtherDiamondPriority = 5000000;

	public const int RelicPriority = 100000000;

	private static readonly float Timeout = 5f;

	private static string _RemoteBasePath = null;

	private static readonly string ImageSaveBasePath = Application.dataPath + "/resources/_server_data_last_loaded/";

	private static readonly string OriginalTextSaveBasePath = Application.persistentDataPath + "/_server_data_last_loaded/";

	private static readonly string OriginalTextSaveBasePath2 = Application.persistentDataPath + "/_server_data_last_loaded2/";

	private static readonly string ImageBasePath = "_server_data_last_loaded/";

	private static readonly string DeviceBasePath = Application.persistentDataPath + "/" + Globals.AppDirName + "/";

	public Dictionary<int, Achievement> _achievements = new Dictionary<int, Achievement>();

	public Dictionary<int, Condition> _conditions = new Dictionary<int, Condition>();

	private List<Item> _Bag = new List<Item>();

	public Item MyWeapon;

	internal Dictionary<string, Skill> _skills = new Dictionary<string, Skill>();

	internal Dictionary<int, Location> _locations = new Dictionary<int, Location>();

	private float _lastUpdateAllLocationsLogic;

	internal static readonly float LocationLogicMinTimeStep = 0.1f;

	internal Dictionary<int, MoneyType> _moneyTypes = new Dictionary<int, MoneyType>();

	public Dictionary<int, Bonus> _bonuses = new Dictionary<int, Bonus>();

	internal Dictionary<int, BotLevel> _bossLevels = new Dictionary<int, BotLevel>();

	internal Dictionary<int, BotLevel> _botLevels = new Dictionary<int, BotLevel>();

	public List<BankItem> _bankItems = new List<BankItem>();

	public static readonly string PhrasePlayerExperience = "PlayerExperience";

	public static readonly string PhrasePlayerAnger = "PlayerAnger";

	public static readonly string PhrasePlayerRage = "PlayerRage";

	public static readonly string PhrasePlayerHealth = "PlayerHealth";

	public static readonly string PhrasePlayerMagic = "PlayerMagic";

	public static readonly string PhrasePlayerStrength = "PlayerStrength";

	public static readonly string PhraseBagEquipment = "BagEquipment";

	public static readonly string PhraseBagBag = "BagBag";

	public static readonly string PhraseBagParams = "BagParams";

	public static readonly string PhraseBagResources = "BagResources";

	public static readonly string PhraseBagElixirs = "BagElixirs";

	public static readonly string PhraseBagPoints = "BagPoints";

	public static readonly string PhraseBagIce = "BagIce";

	public static readonly string PhraseBagFire = "BagFire";

	public static readonly string PhraseBagLighting = "BagLighting";

	public static readonly string PhraseBagDark = "BagDark";

	public static readonly string PhraseShop = "Shop";

	public static readonly string PhraseShopBuy = "ShopBuy";

	public static readonly string PhraseShopResetButton = "ShopResetButton";

	public static readonly string PhraseShopResetTime = "ShopResetTime";

	public static readonly string PhraseShopItemsCount = "ShopItemsCount";

	public static readonly string PhraseBattleAttack = "BattleAttack";

	public static readonly string PhraseExecutionHelp = "ExecutionHelp";

	public static readonly string PhraseMagicProtectionHelp = "MagicProtectionHelp";

	public static readonly string PhraseCastGestureHelp = "CastGestureHelp";

	public static readonly string PhraseGotLevel = "GotLevel";

	public static readonly string PhraseGotSkillPoints = "GotSkillPoints";

	public static readonly string PhraseFightResultsTurns = "FightResultsTurns";

	public static readonly string PhraseFightResultsAddAnger = "FightResultsAddAnger";

	public static readonly string PhraseFightResultsAddExp = "FightResultsAddExp";

	public static readonly string PhraseFightResultsBlock = "FightResultsBlock";

	public static readonly string PhraseFightResultsDodge = "FightResultsDodge";

	public static readonly string PhraseFightResultsImmunity = "FightResultsImmunity";

	public static readonly string PhraseFightResultsFatality = "FightResultsFatality";

	public static readonly string PhraseIntroductionText = "IntroductionText";

	public static readonly string PhraseButtonOk = "ButtonOk";

	public static readonly string PhraseButtonCancel = "ButtonCancel";

	public static readonly string PhraseButtonBuy = "ButtonBuy";

	public static readonly string PhraseButtonBuyAndPuton = "ButtonBuyAndPuton";

	public static readonly string PhraseButtonSell = "ButtonSell";

	public static readonly string PhraseButtonFight = "ButtonFight";

	public static readonly string PhraseButtonStartOver = "ButtonStartOver";

	public static readonly string PhraseButtonContinue = "ButtonContinue";

	public static readonly string PhraseButtonYes = "ButtonYes";

	public static readonly string PhraseButtonNo = "ButtonNo";

	public static readonly string PhraseCompareChosen = "CompareChosen";

	public static readonly string PhraseComparePuton = "ComparePuton";

	public static readonly string PhraseMagicBookHelp = "MagicBookHelp";

	public static readonly string PhraseInBattleFightWith = "InBattleFightWith";

	public static readonly string PhraseInBattleChapter = "InBattleChapter";

	public static readonly string PhraseOpsSoundsVolume = "OpsSoundsVolume";

	public static readonly string PhraseOpsMusicVolume = "OpsMusicVolume";

	public static readonly string PhraseOpsRestartLabel = "OpsRestartLabel";

	public static readonly string PhraseOpsRestartButton = "OpsRestartButton";

	public static readonly string PhraseOpsTeaserLabel = "OpsTeaserLabel";

	public static readonly string PhraseOpsTeaserButton = "OpsTeaserButton";

	public static readonly string PhraseOpsTutorialToggle = "OpsTutorialToggle";

	public static readonly string PhraseExOpenConditionLabel = "ExOpenConditionLabel";

	public static readonly string PhraseExOpenButton = "ExOpenButton";

	public static readonly string PhraseExLootLabel = "ExLootLabel";

	public static readonly string PhraseExCongratText1 = "ExCongratText1";

	public static readonly string PhraseExCongratText2 = "ExCongratText2";

	public static readonly string PhraseButtonClose = "ButtonClose";

	public static readonly string PhraseSovHeader = "SovHeader";

	public static readonly string PhraseSovText = "SovText";

	public static readonly string PhraseBank = "Bank";

	public static readonly string PhraseBankCount = "BankCount";

	public static readonly string PhraseBankBonus = "BankBonus";

	public static readonly string PhraseBankCost = "BankCost";

	public static readonly string PhraseLocationPopulationAtacked = "LocationPopulationAtacked";

	public static readonly string PhraseLocationPopulationPeace = "LocationPopulationPeace";

	public static readonly string PhraseLocationPopulationMax = "LocationPopulationMax";

	public static readonly string PhraseScarabFind = "ScarabFind";

	public static readonly string PhraseScarabDig = "ScarabDig";

	public static readonly string PhraseScarabInsufficient = "ScarabInsufficient";

	public static readonly string PhrasePauseButtonResume = "PauseButtonResume";

	public static readonly string PhrasePauseButtonRestart = "PauseButtonRestart";

	public static readonly string PhrasePauseButtonExit = "PauseButtonExit";

	public static readonly string PhraseBankBuyError = "BankBuyError";

	internal Dictionary<string, string> _phrases = new Dictionary<string, string>();

	private Dictionary<string, string> _bigTexts = new Dictionary<string, string>();

	internal Dictionary<int, BotInfo> _bots = new Dictionary<int, BotInfo>();

	internal Dictionary<int, Slot> _slots = new Dictionary<int, Slot>();

	internal Dictionary<int, Item> _items = new Dictionary<int, Item>();

	public List<ShopGood> InShopGoods = new List<ShopGood>();

	public float InShopGoodsUpdateSeconds;

	internal Dictionary<int, ShopGood> _shopGoods = new Dictionary<int, ShopGood>();

	public Dictionary<int, Subtitle> _subtitles = new Dictionary<int, Subtitle>();

	public List<PersData> _persData = new List<PersData>();

	private PersData _playerServerPersData;

	public Dictionary<int, Spell> _spells = new Dictionary<int, Spell>();

	public List<int> MySpells = new List<int>();

	public Dictionary<int, Fatality> _fatalities = new Dictionary<int, Fatality>();

	internal Dictionary<int, LevelData> _levels = new Dictionary<int, LevelData>();

	internal Dictionary<string, HintData> _hints = new Dictionary<string, HintData>();

	internal List<ServerColor> Colors = new List<ServerColor>();

	public Dictionary<int, Chest> Chests = new Dictionary<int, Chest>();

	private float _anger_time;

	public readonly PersonParams PlayerInBattleParams = new PersonParams();

	public readonly PlayerParamsData PlayerParams = new PlayerParamsData();

	public readonly MobParamsData EnemyParams = new MobParamsData();

	public BattleParams BattleMathParams = new BattleParams();

	public float BattleStartTime;

	private int BattleHealCooldown;

	private int BattlePoisonTurns;

	private int BattlePoisonCooldown;

	private int BattleCriticalCooldown;

	private int BattleHealCooldownMax;

	private int BattlePoisonCooldownMax;

	private int BattleCriticalCooldownMax;

	private List<Item> _removedElixirs = new List<Item>();

	internal static List<int> BuyedMines = new List<int>();

	internal List<Mine> Mines = new List<Mine>();

	internal List<ItemRelation> _itemRelations = new List<ItemRelation>();

	public Settings GameSettings;

	internal Dictionary<Tuple<int, int, int>, StorylineDialog> _storylineDialogs = new Dictionary<Tuple<int, int, int>, StorylineDialog>();

	internal Dictionary<int, Npc> _npcs = new Dictionary<int, Npc>();

	private bool _parallelLoad;

	private List<ActionD> _postLoadAllActions = new List<ActionD>();

	public List<FileInfo> _loadedFilesInfo = new List<FileInfo>();

	private bool _inTryLoadRemoteData;

	internal static bool _inInitConfigsFromRemoteData = false;

	private bool _saveLoadedRemoteFiles = true;

	public bool _inBerserkMode;

	public bool ShopRegenSet;

	public int Version => 1;

	private static string RemoteBasePath
	{
		get
		{
			if (_RemoteBasePath == null)
			{
				_RemoteBasePath = UnityApi.GetServerUrl();
				Utils.LogForce("REMOTE BASE PATH", _RemoteBasePath);
			}
			return _RemoteBasePath;
		}
	}

	public StateE LoadState { get; private set; }

	public IEnumerable<Slot> Slots => _slots.Values;

	private IEnumerable<ShopGood> ShopGoods => _shopGoods.Values;

	public PersData PlayerServerPersData
	{
		get
		{
			return _playerServerPersData;
		}
		set
		{
			_playerServerPersData = value;
		}
	}

	public List<PersData> StartPersData => _persData;

	public List<string> LoadingTips { get; set; }

	public bool IsComboOpened => GameSettings != null && PlayerParams.Level >= GameSettings.SkillComboLevel;

	public bool IsMagicOpened => GameSettings != null && PlayerParams.Level >= GameSettings.SkillMagicLevel;

	public bool IsRageOpened => GameSettings != null && PlayerParams.Level >= GameSettings.SkillRageLevel;

	public bool IsBerserkMode
	{
		get
		{
			double totalMinutes = DateTime.Now.Subtract(PlayerParams.LastBerserkTime).TotalMinutes;
			return totalMinutes < (double)GameSettings.BerserkLength;
		}
	}

	internal bool IsLoading { get; private set; }

	internal int LiveShamansCount
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<int, Location> location in _locations)
			{
				if (location.Value.lockShaman && !location.Value.Logic.IsOpened)
				{
					num++;
				}
			}
			return num;
		}
	}

	[method: MethodImpl((MethodImplOptions)32)]
	public static event LoadingTipsReadyHandler OnLoadingTipsReady;

	public ServerData()
	{
		LoadState = StateE.None;
	}

	private void Loaded()
	{
		Messenger.Invoke(Globals.MsgInternalGameSaveLoaded);
	}

	public void LoadData(MonoBehaviour component)
	{
		Utils.LogForce("***---------******LOADDATA");
		LoadDataLoc(component, UnityApi.GetLanguage());
	}

	private static int GetPrice(MoneyType.TypeE moneyType, Dictionary<MoneyType, int> price, int ifNo)
	{
		foreach (KeyValuePair<MoneyType, int> item in price)
		{
			if (item.Key.Type == moneyType)
			{
				return item.Value;
			}
		}
		return ifNo;
	}

	private bool Buy(MoneyType.TypeE moneyType, Dictionary<MoneyType, int> price, ref int moneyCount)
	{
		int price2 = GetPrice(moneyType, price, -1);
		if (price2 >= 0 && price2 <= moneyCount)
		{
			moneyCount -= price2;
			return true;
		}
		return false;
	}

	private bool Buy(Dictionary<MoneyType, int> price)
	{
		int moneyCount = PlayerParams.MoneyDiamondCount;
		if (Buy(MoneyType.TypeE.Diamond, price, ref moneyCount))
		{
			PlayerParams.MoneyDiamondCount = moneyCount;
			return true;
		}
		moneyCount = PlayerParams.MoneyGoldCount;
		if (Buy(MoneyType.TypeE.Gold, price, ref moneyCount))
		{
			PlayerParams.MoneyGoldCount = moneyCount;
			return true;
		}
		moneyCount = PlayerParams.MoneyKeysCount;
		if (Buy(MoneyType.TypeE.Key, price, ref moneyCount))
		{
			PlayerParams.MoneyKeysCount = moneyCount;
			return true;
		}
		moneyCount = PlayerParams.MoneySkullsCount;
		if (Buy(MoneyType.TypeE.Skull, price, ref moneyCount))
		{
			PlayerParams.MoneySkullsCount = moneyCount;
			return true;
		}
		moneyCount = PlayerParams.MoneyStarsCount;
		if (Buy(MoneyType.TypeE.Star, price, ref moneyCount))
		{
			PlayerParams.MoneyStarsCount = moneyCount;
			return true;
		}
		return false;
	}

	internal bool BuyLocationOpenCondition(Location location)
	{
		if (location.Logic.OpenCondition == null)
		{
			return false;
		}
		if (location.Logic.OpenCondition.Done)
		{
			return true;
		}
		if (location.OpenPrice == null)
		{
			return false;
		}
		bool flag = Buy(location.OpenPrice);
		if (flag)
		{
			location.Logic.OpenCondition.SetDone(OpenCondition.DoneReasonE.Buyed);
		}
		return flag;
	}

	public Pair<MoneyType, int> GetSellPrice(Item item)
	{
		foreach (KeyValuePair<MoneyType, int> item2 in item.SellPrice)
		{
			if (item2.Value > 0)
			{
				return new Pair<MoneyType, int>(item2.Key, item2.Value * GameSettings.SellPrice / 100);
			}
		}
		return new Pair<MoneyType, int>(MoneyType.ZeroGold, 0);
	}

	public void SellItem(Item item)
	{
		if (_Bag.Contains(item))
		{
			Pair<MoneyType, int> sellPrice = GetSellPrice(item);
			switch (sellPrice.Key.Type)
			{
			case MoneyType.TypeE.Gold:
				PlayerParams.MoneyGoldCount += sellPrice.Value;
				break;
			case MoneyType.TypeE.Diamond:
				PlayerParams.MoneyDiamondCount += sellPrice.Value;
				break;
			case MoneyType.TypeE.Key:
				PlayerParams.MoneyKeysCount += sellPrice.Value;
				break;
			case MoneyType.TypeE.Skull:
				PlayerParams.MoneySkullsCount += sellPrice.Value;
				break;
			case MoneyType.TypeE.Scarab:
				PlayerParams.MoneyScarabCount += sellPrice.Value;
				break;
			case MoneyType.TypeE.Star:
				PlayerParams.MoneyStarsCount += sellPrice.Value;
				break;
			}
			item.PutOn = false;
			_Bag.Remove(item);
			if (item.IsElixirType())
			{
				RefreshElixirsCount();
			}
		}
	}

	private void ParseAchievements(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Achievement achievement = new Achievement();
			g.Get(out achievement.Id, "id").Get(out achievement.Title, "title").Get(out achievement.Info, "info")
				.Get(out achievement.Order, "order", 0)
				.Get(out achievement.Points, "points", 0)
				.Get(out achievement.Image, "image")
				.Get(out achievement.Condition, "condition");
			if (string.IsNullOrEmpty(achievement.Image))
			{
				achievement.Image = achievement.Id.ToString();
			}
			_achievements.Add(achievement.Id, achievement);
		});
	}

	private void ParseConditions(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Condition condition = new Condition();
			g.Get(out condition.Id, "id").Get(out condition.OpenPhrase, "info").Get(out condition.Count, "count", 1);
			Dictionary<string, object> dictionary = JsonUtils.JsonGetHashtable("test", g.Hash);
			foreach (string key in dictionary.Keys)
			{
				if (key == "choice")
				{
					condition.Type = (string)dictionary[key];
				}
				else if (dictionary[key] is long)
				{
					condition.Params.Add(key, (int)(long)dictionary[key]);
				}
				else
				{
					condition.Params.Add(key, dictionary[key]);
				}
			}
			if (condition.Count < 1)
			{
				condition.Count = 1;
			}
			_conditions.Add(condition.Id, condition);
		});
	}

	public void RemoveFromBag(Item it)
	{
		_Bag.Remove(it);
	}

	private void AddToBag(int serverId)
	{
		AddToBag(_items[serverId].MakeRealItem(forShop: false, 1));
	}

	private void AddToBag(int serverId, int count)
	{
		AddToBag(_items[serverId].MakeRealItem(forShop: false, count));
	}

	internal void AddToBag(Bonus.DropElement loot)
	{
		if (loot.IsExp)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogWarning("==== WE CANNOT USE EXP AS LOOT! -- {0} ====".Fmt(loot));
			}
			AddPlayerExperience(loot.Count);
		}
		else if (loot.Item.IsMoney())
		{
			switch (loot.Item.GetMoneyTypeFromItem())
			{
			case MoneyType.TypeE.Gold:
				PlayerParams.MoneyGoldCount += loot.Count;
				break;
			case MoneyType.TypeE.Diamond:
				PlayerParams.MoneyDiamondCount += loot.Count;
				break;
			case MoneyType.TypeE.Key:
				PlayerParams.MoneyKeysCount += loot.Count;
				break;
			case MoneyType.TypeE.Skull:
				PlayerParams.MoneySkullsCount += loot.Count;
				break;
			case MoneyType.TypeE.Scarab:
				PlayerParams.MoneyScarabCount += loot.Count;
				break;
			case MoneyType.TypeE.Star:
				PlayerParams.MoneyStarsCount += loot.Count;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		else if (loot.IsItem)
		{
			if (loot.Item.ElixirType == Item.ElixirTypeE.Key)
			{
				PlayerParams.MoneyKeysCount += loot.Count;
			}
			else
			{
				AddToBag(loot.Item);
			}
		}
	}

	internal void PrintBag()
	{
		Utils.Log("******** BAG");
		foreach (Item item in _Bag)
		{
			Utils.Log(item.ToString());
		}
	}

	public void AddToBag(Item item)
	{
		Utils.Log("**AddToBag", item);
		_Bag.Add(item);
		if (item.IsElixirType())
		{
			RefreshElixirsCount();
		}
		Messenger.Invoke(Globals.MsgBagNeedRefresh);
		if (!IsLoading)
		{
			Messenger<Item>.Invoke(Globals.MsgBagItemAdded, item);
		}
	}

	public List<Item> GetAllPutOn()
	{
		return new List<Item>(_Bag.Where((Item _) => _?.PutOn ?? false));
	}

	public void ForeachInBag(ActionD<Item> action)
	{
		foreach (Item item in _Bag)
		{
			action(item);
		}
	}

	public Item FindInBag(Predicate<Item> pred)
	{
		return _Bag.Find(pred);
	}

	public List<Item> GetAllInBag(Func<Item, bool> pred)
	{
		return new List<Item>(_Bag.Where(pred));
	}

	public Item[] BagAsArray()
	{
		return _Bag.ToArray();
	}

	public bool HasPutonWithSuperEffect(Skill.TypeE type)
	{
		foreach (Item item in _Bag)
		{
			if (item.PutOn && item.HasSkill(type))
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveAllPlayerArmor()
	{
		MyWeapon = null;
	}

	public Skill GetSkill(Skill.TypeE type)
	{
		foreach (Skill value in _skills.Values)
		{
			if (value.Type == type)
			{
				return value;
			}
		}
		Invs.Inv(false, "No skill", type);
		return null;
	}

	private JsonUtils.GetValues GetSkillsLazy(JsonUtils.GetValues json, out SkillInfo[] skills, string name)
	{
		JsonUtils.GetValues.MyPair<SkillInfo> myPair = new JsonUtils.GetValues.MyPair<SkillInfo>();
		json.Get(out skills, name, delegate(Dictionary<string, object> _)
		{
			SkillInfo data = new SkillInfo
			{
				Min = JsonUtils.JsonGetInt("minValue", _),
				Max = JsonUtils.JsonGetInt("maxValue", _)
			};
			if (data.Max < data.Min)
			{
				data.Max = data.Min;
			}
			if (data.Max == 0)
			{
				data.Max = int.MaxValue;
			}
			string skillName = JsonUtils.JsonTryGet<string>("typeId", _, null);
			if (!string.IsNullOrEmpty(skillName))
			{
				_postLoadAllActions.Add(delegate
				{
					try
					{
						data.Skill = _skills[skillName];
						if (data.Skill == null && Globals.IsDebugBuild)
						{
							Debug.LogWarning("Can't find skill " + skillName);
						}
					}
					catch (Exception ex)
					{
						Utils.Log("Can't find skill", skillName, ex.Message);
					}
				});
			}
			return data;
		});
		SkillInfo[] mySkills = skills;
		_postLoadAllActions.Add(delegate
		{
			int num = mySkills.Length;
			for (int i = 0; i < num; i++)
			{
				if (mySkills[i].Skill == null)
				{
					num--;
					mySkills[i] = mySkills[num];
				}
			}
			if (mySkills.Length != num)
			{
				Array.Resize(ref mySkills, num);
			}
		});
		return json;
	}

	private void ParseSkillArtikuls(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Skill skill = new Skill();
			g.Get(out skill.Id, "id").Get(out string value, "code").Get(out skill.Title, "title")
				.Get(out skill.SkillPoint, "skillPoints")
				.Get(out skill.Weight, "weight");
			skill.Type = Skill.GetType(value);
			_skills.Add(value, skill);
		});
	}

	internal void UpdateAllLocationsLogic()
	{
		float time = Time.time;
		if (!(time - _lastUpdateAllLocationsLogic > LocationLogicMinTimeStep))
		{
			return;
		}
		float deltaTime = time - _lastUpdateAllLocationsLogic;
		_lastUpdateAllLocationsLogic = time;
		foreach (KeyValuePair<int, Location> location in _locations)
		{
			location.Value.Logic.Update(deltaTime, DateTime.Now);
		}
	}

	public Location GetLocation(int id)
	{
		foreach (Location value in _locations.Values)
		{
			if (value.MapId == id)
			{
				return value;
			}
		}
		Invs.Inv(false, "GetLocation", id);
		return null;
	}

	internal Location GetLocationByServerId(int id)
	{
		foreach (Location value in _locations.Values)
		{
			if (value.Id == id)
			{
				return value;
			}
		}
		return null;
	}

	internal Location GetStartGameLocation()
	{
		foreach (Location value in _locations.Values)
		{
			if (value.OpenAfter == null)
			{
				return value;
			}
		}
		return null;
	}

	internal Location GetLastGameLocation()
	{
		Location location = null;
		if (Globals.LastLoadedSceneServerId > 0)
		{
			location = GetLocationByServerId(Globals.LastLoadedSceneServerId);
			if (location != null && location.IsZachistkaOpened)
			{
				return location;
			}
		}
		foreach (Location value in _locations.Values)
		{
			if (value.IsZachistkaOpened && value.Logic.OpenCondition == null)
			{
				return value;
			}
		}
		foreach (Location value2 in _locations.Values)
		{
			if (value2.IsZachistkaOpened && value2.Logic.OpenCondition.Done)
			{
				location = value2;
			}
		}
		if (location == null)
		{
			location = GetStartGameLocation();
		}
		return location;
	}

	private Location GetLastProgressLocation()
	{
		foreach (Location value in _locations.Values)
		{
			if (value.Condition != null || value.OpenAfter == null)
			{
				continue;
			}
			bool flag = false;
			foreach (Location value2 in _locations.Values)
			{
				if (value2.OpenAfter != null && value2.OpenAfter.Id == value.Id)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			return value;
		}
		return null;
	}

	internal Location GetProgressLocation()
	{
		foreach (Location value in _locations.Values)
		{
			if (value.IsZachistkaOpened && value.Condition == null)
			{
				return value;
			}
		}
		Location lastProgressLocation = GetLastProgressLocation();
		return (lastProgressLocation == null) ? GetStartGameLocation() : lastProgressLocation;
	}

	internal void TakeMoneyFromLocation(Location location, int amount)
	{
		location.Logic.Money -= amount;
		PlayerParams.MoneyGoldCount += amount;
	}

	public int GetLocationProgress(Location location)
	{
		return (location.Logic != null) ? location.Logic.ZachistkaMobsKilled : 0;
	}

	public void ResetLocationProgress(Location location)
	{
		foreach (KeyValuePair<int, Location> location2 in _locations)
		{
			location2.Value.Logic.ZachistkaMobsKilled = 0;
		}
	}

	public void IncLocationProgress(Location location)
	{
		location.Logic.IncZachistkaMobsKilled();
	}

	public void ResetLocationsProgress()
	{
		foreach (Location value in _locations.Values)
		{
			value.Logic.Reset();
		}
	}

	private void ParseLocations(Dictionary<string, object> hashtable)
	{
		JsonUtils.GetValues.MyPair<Location.BotLocationInfo> myPair = new JsonUtils.GetValues.MyPair<Location.BotLocationInfo>();
		JsonUtils.GetValues.MyPair<Location.ChestLocationInfo> myPair2 = new JsonUtils.GetValues.MyPair<Location.ChestLocationInfo>();
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Location data = new Location();
			g.Get(out data.Id, "id").Get(out data.Title, "title").Get(out data.MapId, "mapId")
				.Get(out data.MapModel, "3dMap")
				.Get(out data.Elfs, "elfs", defaultValue: false)
				.Get(out data.Description, "description")
				.Get(out data.IconPers, "iconPers")
				.Get(out data.IconMoneyCoord, "coordIconM")
				.Get(out data.IconMobsCoord, "coordIconMobs")
				.Get(out data.IconChestCoord, "coordIconChest")
				.Get(out data.IconZachistkaCoord, "coordIconZ")
				.Get(out data.RespawnPeriodSeconds, "periodRespawn", 0)
				.Get(out data.RespawnProbability, "probRespawn", 0)
				.Get(out data.RespawnMax, "respawnMax", 0)
				.Get(out data.RespawnKill, "killsCount", 0)
				.Get(out data.RespawnKillPeriodSeconds, "killPeriod", 0)
				.Get(out data.PopulationPeriodSeconds, "populationPeriod", 0)
				.Get(out data.PopulationPointsUp, "populationPointsUp", 0)
				.Get(out data.PopulationMax, "populationMax", 0)
				.Get(out data.MoneyMax, "moneyMax", 0)
				.Get(out data.MoneyPerPerson, "moneyPerPerson", 1)
				.Get(out data.lockShaman, "lockShaman", defaultValue: false)
				.Get(out data.IsCave, "isCave", defaultValue: false)
				.Get(out data.MoneyPeriod, "moneyPeriod", 0)
				.Get(out data.IsMiniBoss, "miniBoss", defaultValue: false)
				.Get(out data.IsAltPath, "superBoss", defaultValue: false)
				.Get(out data.Bots, "bots", delegate(Dictionary<string, object> hash)
				{
					if (!IsValidVersion(hash))
					{
						return (Location.BotLocationInfo)null;
					}
					Location.BotLocationInfo bi = new Location.BotLocationInfo();
					int value = -1;
					JsonUtils.GetValues getValues = new JsonUtils.GetValues(_postLoadAllActions, hash);
					getValues.Get(out bi.Level, "level").Get(out bi.IsBoss, "isBoss").Get(out value, "botId", -1);
					if (value == -1)
					{
						return (Location.BotLocationInfo)null;
					}
					getValues.GetAlwaysLazyIntFrom("botId", -1, _bots, delegate(BotInfo _)
					{
						bi.Bot = _;
					});
					return bi;
				})
				.Get(out data.Chests, "chests", delegate(Dictionary<string, object> hash)
				{
					Location.ChestLocationInfo ci = new Location.ChestLocationInfo();
					int value = -1;
					JsonUtils.GetValues getValues = new JsonUtils.GetValues(_postLoadAllActions, hash);
					getValues.Get(out ci.Prob, "prob").Get(out value, "chest", -1);
					if (value == -1)
					{
						return (Location.ChestLocationInfo)null;
					}
					getValues.GetAlwaysLazyIntFrom("chest", -1, Chests, delegate(Chest _)
					{
						ci.Chest = _;
					});
					return ci;
				});
			if (data.IsCave)
			{
				g.Get(out data.CaveName, "caveName").Get(out data.CaveGlobalCoord, "caveGCoord").Get(out data.CaveLocationCoord, "caveLCoord")
					.Get(out data.CaveDiff, "caveD");
			}
			if (data.IconChestCoord == Vector2.zero)
			{
				Vector2 vector = ((!(data.IconMoneyCoord.x > data.IconMobsCoord.x)) ? data.IconMobsCoord : data.IconMoneyCoord);
				data.IconChestCoord = new Vector2(vector.x + 80f, vector.y);
			}
			GetMoneyLazy(g, data.OpenPrice, "openPrice");
			if (Globals.DebugShortPeriods)
			{
				data.PopulationPeriodSeconds = 25;
				data.PopulationPointsUp = 5;
				data.MoneyPeriod = 30;
				data.MoneyPerPerson = 1;
			}
			if (Globals.DebugSvetlyaki && data.MapId == 2)
			{
				_postLoadAllActions.Add(delegate
				{
					data.InsertBot(2, GetBotInfoByServerId(238));
				});
				_postLoadAllActions.Add(delegate
				{
					data.InsertBot(2, GetBotInfoByServerId(359));
				});
				_postLoadAllActions.Add(delegate
				{
					data.InsertBot(2, GetBotInfoByServerId(491));
				});
			}
			if (Globals.DebugLocationsFastMobs)
			{
				data.RespawnPeriodSeconds = 1;
			}
			g.GetLazyIntFrom("openCond", -1, _conditions, delegate(Condition _)
			{
				data.Condition = _;
				if (data.Condition != null)
				{
					data.Logic.OpenCondition = OpenCondition.Create(data.Condition);
					if (data.Logic.OpenCondition != null)
					{
						data.Logic.OpenCondition.Location = data;
						data.Logic.OpenCondition.Enable();
					}
				}
			});
			g.GetLazyIntFrom("bonus", -1, _bonuses, delegate(Bonus _)
			{
				data.Bonus = _;
			});
			g.GetLazyIntFrom("openAfter", -1, _locations, delegate(Location _)
			{
				data.OpenAfter = _;
			});
			g.GetLazyIntFrom("openAfterAlt", -1, _locations, delegate(Location _)
			{
				data.OpenAfterAlt = _;
			});
			_locations.Add(data.Id, data);
		});
	}

	private MoneyType GetMoneyTypeByServerId(int id)
	{
		foreach (MoneyType value in _moneyTypes.Values)
		{
			if (value.Id == id)
			{
				return value;
			}
		}
		return null;
	}

	private JsonUtils.GetValues GetMoneyLazy(JsonUtils.GetValues json, Dictionary<MoneyType, int> money, string name)
	{
		json.ForeachLazy(name, delegate(string moneyType, int count)
		{
			MoneyType key = _moneyTypes[int.Parse(moneyType)];
			money[key] = count;
		});
		return json;
	}

	private void ParseMoneyTypes(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			MoneyType moneyType = new MoneyType();
			g.Get(out moneyType.Id, "id").Get(out moneyType.Title, "title").Get(out moneyType.Code, "code");
			_moneyTypes.Add(moneyType.Code, moneyType);
		});
	}

	private void ParseBonuses(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Bonus data = new Bonus();
			g.Get(out data.Id, "id").Get(out data.Title, "title").Get(out data.AllItems, "allItems")
				.Get(out data.Count, "count", 0);
			g.Foreach("drop", delegate(string id, Dictionary<string, object> dropHashtable)
			{
				JsonUtils.GetValues getValues = new JsonUtils.GetValues(_postLoadAllActions, dropHashtable);
				Bonus.DropElement dropElement = new Bonus.DropElement();
				getValues.Get(out dropElement.Probability, "probability").Get(out dropElement.Count, "count", 1).Get(out dropElement.Type, "type");
				if (dropElement.Type == 2)
				{
					dropElement.Type = 1;
				}
				if (dropElement.Type == 1)
				{
					getValues.GetLazyIntFrom("item", -1, _items, delegate(Item _)
					{
						dropElement.Item = _;
					});
				}
				if (dropElement.Type == 3)
				{
					getValues.GetLazyIntFrom("bonus", -1, _bonuses, delegate(Bonus _)
					{
						dropElement.Bonus = _;
					});
				}
				data.Drop.Add(dropElement);
			});
			if (data.Drop.TrueForAll((Bonus.DropElement _) => _.Probability <= 0))
			{
				data.Drop.ForEach(delegate(Bonus.DropElement _)
				{
					_.Probability = 1;
				});
			}
			_bonuses.Add(data.Id, data);
		});
	}

	internal BotLevel GetBotLevel(AreaData.MobData data)
	{
		BotLevel value = null;
		Dictionary<int, BotLevel> dictionary = ((!data.IsBoss) ? _botLevels : _bossLevels);
		int num = data.Level;
		while (!dictionary.TryGetValue(num, out value))
		{
			num--;
			Invs.Inv(num > 0, "GetBotLevel level > 0", data.Level);
		}
		return value;
	}

	internal int GetMinigameCount(AreaData.MobData data)
	{
		BotLevel botLevel = GetBotLevel(data);
		if (botLevel.DifMagicgame == 4)
		{
			return GameSettings.mgBM4;
		}
		if (botLevel.DifMagicgame == 3)
		{
			return GameSettings.mgBM3;
		}
		if (botLevel.DifMagicgame == 2)
		{
			return GameSettings.mgBM2;
		}
		return GameSettings.mgBM1;
	}

	internal int GetMinigame2Count(AreaData.MobData data)
	{
		BotLevel botLevel = GetBotLevel(data);
		if (botLevel.DifMagicgame == 3)
		{
			return GameSettings.mg2BM3;
		}
		if (botLevel.DifMagicgame == 2)
		{
			return GameSettings.mg2BM2;
		}
		if (botLevel.DifMagicgame == 4)
		{
			return GameSettings.mg2BM4;
		}
		return GameSettings.mg2BM1;
	}

	private void ParseBotLevels(Dictionary<int, BotLevel> dict, Dictionary<string, object> hashtable)
	{
		JsonUtils.GetValues getValues = new JsonUtils.GetValues(_postLoadAllActions, null);
		foreach (string key in hashtable.Keys)
		{
			getValues.Hash = JsonUtils.JsonGetHashtable(key, hashtable);
			BotLevel data = new BotLevel
			{
				Level = JsonUtils.JsonGetInt("level", getValues.Hash)
			};
			getValues.Get(out data.Title, "title");
			getValues.Get(out data.WinExp, "winExpMin").Get(out data.TotalWeight, "totalWeight").Get(out data.ShowSectorControl, "sectorControl")
				.Get(out data.SpeedMoveSectorControl, "speedSectorControl", 0f)
				.Get(out data.SectorAngle, "rotateSectorControl", 0f)
				.Get(out data.WeakMagicP, "weakMagicP", 0)
				.Get(out data.StrongMagicP, "strongMagicP", 0)
				.Get(out data.DifMagicgame, "difMagicgame", 1)
				.Get(out data.ChangeViewDirPeriod, "changeViewDirPeriod", 0)
				.Get(out data.ChangeViewDirProb, "changeViewDirProb", 0)
				.Get(out data.ZoneSize, "zoneSize", 0)
				.GetLazyIntFrom("lossBonus", -1, _bonuses, delegate(Bonus _)
				{
					data.LossBonus = _;
				})
				.GetLazyIntFrom("winBonus", -1, _bonuses, delegate(Bonus _)
				{
					data.WinBonus = _;
				});
			GetSkillsLazy(getValues, out data.Skills, "skills");
			if (data.SpeedMoveSectorControl == 0f)
			{
				data.SpeedMoveSectorControl = 20f;
			}
			if (data.SectorAngle == 0f)
			{
				data.SectorAngle = 30f;
			}
			dict.Add(data.Level, data);
		}
	}

	private void GetLazyBankMoney(JsonUtils.GetValues g, string name, ActionD<MoneyType> action)
	{
		int index = -1;
		g.Get(out index, name, -1);
		if (index <= 0)
		{
			return;
		}
		_postLoadAllActions.Add(delegate
		{
			MoneyType moneyTypeByServerId = GetMoneyTypeByServerId(index);
			if (moneyTypeByServerId != null)
			{
				action(moneyTypeByServerId);
			}
		});
	}

	private void ParseBankItems(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			BankItem data = new BankItem();
			g.Get(out data.Id, "id").Get(out data.PurchaseId, "purchaseId").Get(out data.Number, "number")
				.Get(out data.Count, "count")
				.Get(out data.Bonus, "bonus", 0)
				.Get(out data.BonusPercent, "bonusP", 0)
				.Get(out data.Real, "real")
				.Get(out data.Selected, "selected", defaultValue: false);
			GetLazyBankMoney(g, "countMoneyType", delegate(MoneyType _)
			{
				data.CountType = _;
			});
			GetLazyBankMoney(g, "bonusMoneyType", delegate(MoneyType _)
			{
				data.BonusType = _;
			});
			if (data.Bonus < 0)
			{
				data.Bonus = 0;
			}
			_bankItems.Add(data);
		});
		_bankItems.Sort(SortBankItems);
	}

	private static int SortBankItems(BankItem p1, BankItem p2)
	{
		return p1.Number.CompareTo(p2.Number);
	}

	private void ParseNewsTypes(Dictionary<string, object> hashtable)
	{
	}

	internal string GetBigText(string id)
	{
		string value = string.Empty;
		_bigTexts.TryGetValue(id, out value);
		return value;
	}

	internal string GetPhrase(string id)
	{
		string value = string.Empty;
		_phrases.TryGetValue(id, out value);
		return value;
	}

	internal string GetPhrase(PhrasesE key)
	{
		string phrase = GetPhrase(Enum.GetName(typeof(PhrasesE), key));
		if (phrase == null && Globals.IsDebugBuild)
		{
			Debug.LogError("Cannot get Phrase: {0}".Fmt(key));
		}
		return phrase ?? ("<" + key.ToString() + ">");
	}

	private void ParsePhrases(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			g.Get(out string value, "code").Get(out string value2, "text");
			_phrases.Add(value, value2);
		});
	}

	private void ParseBigTexts(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			g.Get(out string value, "code").Get(out string value2, "text");
			_bigTexts.Add(value, value2);
		});
	}

	internal BotInfo GetBotInfoByServerId(int id)
	{
		BotInfo value = null;
		_bots.TryGetValue(id, out value);
		return value;
	}

	private void ParseBots(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			BotInfo botInfo = new BotInfo();
			try
			{
				g.Get(out botInfo.Id, "id").Get(out botInfo.Model, "3dModel").Get(out botInfo.Picture, "3dPicture")
					.Get(out botInfo.Eyes, "eyes", -1)
					.Get(out botInfo.Armor, "3dArmor")
					.Get(out botInfo._magic, "magic", 0)
					.Get(out botInfo.skinColor, "skinColor", null)
					.Get(out botInfo._magicImmunity, null, "immunity", delegate(string _)
					{
						if (!string.IsNullOrEmpty(_))
						{
							int result = -1;
							if (int.TryParse(_, out result))
							{
								switch (result)
								{
								case 1:
									return MagicTypeE.Darkness;
								case 2:
									return MagicTypeE.Fire;
								case 3:
									return MagicTypeE.Ice;
								case 4:
									return MagicTypeE.Lighting;
								}
							}
						}
						return MagicTypeE.None;
					})
					.Get(out botInfo.Scale, "scale", 1f)
					.Get(out botInfo.Title, "title");
				string[] array = JsonUtils.JsonGet("closedActions", g.Hash, string.Empty).Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length > 0)
				{
					botInfo.ClosedActions = array;
				}
			}
			catch (Exception)
			{
				Utils.Log("PARSE BOTS failed", botInfo.Id);
				throw;
			}
			if (botInfo.Scale == 0f)
			{
				botInfo.Scale = 1f;
			}
			_bots.Add(botInfo.Id, botInfo);
		});
	}

	private void ParseSlots(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			g.Get(out int value, "id").Get(out string value2, "title").Get(out int value3, "slotId");
			_slots.Add(value3, new Slot(value, value2, value3));
		});
	}

	private static List<SkillInfo> GenSkillsToGen(SkillInfo[] Skills, int GiveSkillsCount)
	{
		if (Skills == null)
		{
			return new List<SkillInfo>();
		}
		int num = ((Skills.Length <= GiveSkillsCount) ? Skills.Length : GiveSkillsCount);
		List<SkillInfo> skillsToGen = new List<SkillInfo>(num);
		Utils.Random(Skills, (SkillInfo _) => 1, num, allowDuplicates: false, delegate(int number, int index)
		{
			skillsToGen.Add(Skills[index]);
		});
		return skillsToGen;
	}

	internal static int GetSkill(SkillInfo[] Skills, Skill.TypeE type, int defaultValue)
	{
		Invs.Inv(Skills != null, "Skills != null", type);
		foreach (SkillInfo skillInfo in Skills)
		{
			if (skillInfo.Skill != null && skillInfo.Skill.Type == type)
			{
				return skillInfo.Min;
			}
		}
		return defaultValue;
	}

	internal Item GetItemByServerId(int id)
	{
		Item value = null;
		_items.TryGetValue(id, out value);
		return value;
	}

	internal static SkillInfo[] AddRandomSkills(SkillInfo[] Skills, int GiveSkillsCount, int TotalWeight)
	{
		if (Skills == null)
		{
			return null;
		}
		List<SkillInfo> list = GenSkillsToGen(Skills, GiveSkillsCount);
		SkillInfo[] rSkills = new SkillInfo[list.Count];
		List<SkillInfo> list2 = new List<SkillInfo>();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && list[i].Skill != null)
			{
				list2.Add(new SkillInfo
				{
					Skill = list[i].Skill,
					Min = list[i].Min,
					Max = list[i].Max
				});
			}
		}
		rSkills = list2.ToArray();
		List<int> probability = list2.ConvertAll((SkillInfo _) => _.Skill.Weight);
		int totalWeight = TotalWeight;
		while (totalWeight > 0)
		{
			bool flag = false;
			SkillInfo[] array = rSkills;
			foreach (SkillInfo skillInfo in array)
			{
				if (skillInfo.Min + 1 <= skillInfo.Max && skillInfo.Skill.SkillPoint <= totalWeight)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
			Utils.Random(probability, 1, allowDuplicates: false, delegate(int __, int j)
			{
				int skillPoint = rSkills[j].Skill.SkillPoint;
				SkillInfo skillInfo2 = rSkills[j];
				if (skillPoint <= totalWeight && skillInfo2.Min + 1 <= skillInfo2.Max)
				{
					skillInfo2.Min++;
					totalWeight -= skillPoint;
				}
			});
		}
		return rSkills;
	}

	private void ParseItems(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Item data = new Item(JsonUtils.JsonGetInt("id", g.Hash));
			_items.Add(data.Id, data);
			g.Get(out data.Title, "title").Get(out data.TitleBlue, "titleBlue").Get(out data.TitleGreen, "titleGreen")
				.Get(out data.Description, "description")
				.Get(out data.Class, "class")
				.Get(out data.Color, "color")
				.Get(out data.Sex, "sex", 0)
				.Get(out data.TotalWeight, "totalWeight")
				.Get(out data.TotalWeightMax, "totalWeightMax")
				.Get(out data.GiveSkillsCount, "giveSkillsCount")
				.Get(out data.Model, "3dModel")
				.Get(out data.ModelGreen, "3dModelG", data.Model)
				.Get(out data.ModelBlue, "3dModelB", data.Model)
				.Get(out data.Picture, "3dPicture")
				.Get(out data.LifeTime, "lifeTime")
				.Get(out data.DeathTime, "deathTime")
				.Get(out data.Set, "set", 0)
				.Get(out data.RashodkaEffect, "rashodkaEffect", 0)
				.Get(out data.RashodkaPower, "rashodkaPoints", 0)
				.Get(out data.RashodkaNextTurns, "rashodkaNextTurns", 0)
				.Get(out data.RashodkaEffectTurns, "rashodkaEffectTurns", 0)
				.Get(out data.PictureInBattle, "pictureInBattle")
				.Get(out data._maxStars, "maxStars", 0)
				.GetLazyIntFrom("slot", -1, _slots, delegate(Slot _)
				{
					data.Slot = _;
				})
				.GetLazyIntFrom("fatality", -1, _fatalities, delegate(Fatality _)
				{
					data.FatalityScenarioName = _;
				});
			GetMoneyLazy(g, data.SellPrice, "price");
			GetSkillsLazy(g, out data.Skills, "skills");
		});
	}

	public void ResetInShopGoodsUpdateSeconds()
	{
		if (GameSettings != null)
		{
			InShopGoodsUpdateSeconds = GameSettings.UpdateShopPeriodSeconds;
		}
	}

	internal static bool RecommendedOnly(ShopGood shopGood)
	{
		return shopGood.Item.IsBattleElixir() && shopGood.GetPrice(MoneyType.TypeE.Gold) > 0;
	}

	internal static int ShopGoodPriority(ShopGood shopGood)
	{
		if (shopGood.Item != null)
		{
			if (shopGood.Relict)
			{
				int num = 80000;
				Slot slot = shopGood.Item.Slot;
				switch (slot.SlotId)
				{
				case Slot.TypeE.Weapon:
					num /= 4;
					break;
				case Slot.TypeE.Regalia:
					num *= 2;
					break;
				}
				if (shopGood.Item.IsArmor)
				{
					num /= 2;
				}
				return 100000000 + num + shopGood.Item.GetItemMaxDescription().Item2;
			}
			if (RecommendedOnly(shopGood))
			{
				Tuple<MoneyType.TypeE, int, string> itemBuyPrice = shopGood.GetItemBuyPrice();
				return 2000000 + ((itemBuyPrice.Item1 != MoneyType.TypeE.Diamond) ? (0 + itemBuyPrice.Item2) : 1000);
			}
			if (shopGood.Item.ElixirType == Item.ElixirTypeE.Key || shopGood.Item.ElixirType == Item.ElixirTypeE.Skull || shopGood.Item.ElixirType == Item.ElixirTypeE.Scarab || shopGood.Item.ElixirType == Item.ElixirTypeE.Star)
			{
				return 3000000 + shopGood.GetItemBuyPrice().Item2;
			}
			if (shopGood.Item.IsItemSet())
			{
				return 4000000 + shopGood.GetPrice(MoneyType.TypeE.Diamond);
			}
			if (shopGood.Item.IsArmor)
			{
				if (shopGood.GetPrice(MoneyType.TypeE.Diamond) > 0)
				{
					return 1000000 + 1000 * shopGood.LevelMin + shopGood.GetPrice(MoneyType.TypeE.Diamond);
				}
				if (shopGood.GetPrice(MoneyType.TypeE.Gold) > 0)
				{
					return 1000000 + shopGood.GetPrice(MoneyType.TypeE.Gold);
				}
			}
			else if (shopGood.GetPrice(MoneyType.TypeE.Diamond) > 0)
			{
				int num2 = ((shopGood.Item.Slot.SlotId == Slot.TypeE.Artefact) ? 100000 : 0);
				return 5000000 + num2 * shopGood.LevelMin + 10000 * (int)shopGood.Item.Slot.SlotId + num2 + shopGood.GetPrice(MoneyType.TypeE.Diamond);
			}
		}
		return -1;
	}

	public void RegenShopGoods()
	{
		if (GameSettings == null)
		{
			InShopGoods = new List<ShopGood>();
			Messenger.Invoke(Globals.MsgShopGoodsReseted);
			return;
		}
		InShopGoodsUpdateSeconds = GameSettings.UpdateShopPeriodSeconds;
		List<ShopGood> list = new List<ShopGood>(GetShopGoodsForLevelNotLower(PlayerParams.Level));
		if (list.Count != 0)
		{
			InShopGoods = new List<ShopGood>(list);
			InShopGoods.Sort((ShopGood x, ShopGood y) => -ShopGoodPriority(x).CompareTo(ShopGoodPriority(y)));
			Messenger.Invoke(Globals.MsgShopGoodsReseted);
		}
	}

	private void PrintShop()
	{
		PrintBag();
	}

	public IEnumerable<ShopGood> GetShopGoodsNew(int minLevel, int maxLevel)
	{
		foreach (ShopGood c in InShopGoods)
		{
			if (minLevel >= c.LevelMin && maxLevel <= c.LevelMin)
			{
				yield return c;
			}
		}
	}

	public IEnumerable<ShopGood> GetShopGoodsForLevelNotLower(int level)
	{
		foreach (KeyValuePair<int, ShopGood> c in _shopGoods)
		{
			if (level <= c.Value.LevelMax)
			{
				if (c.Value.Item.IsElixirType())
				{
					yield return c.Value;
				}
				else
				{
					yield return c.Value.MakeRealItem(forShop: true);
				}
			}
		}
	}

	private IEnumerable<ShopGood> GetShopGoods(Slot.TypeE slot)
	{
		foreach (KeyValuePair<int, ShopGood> c in _shopGoods)
		{
			if (c.Value.Item.Slot.SlotId == slot)
			{
				yield return c.Value;
			}
		}
	}

	private void ParseStoreGoods(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			ShopGood data = new ShopGood
			{
				Id = JsonUtils.JsonGetInt("id", g.Hash)
			};
			g.Get(out data.Probability, "probability").Get(out data.LevelMin, "levelMin", 0).Get(out data.Count, "count", 1)
				.Get(out data.Discount, "discount", 0)
				.Get(out data.Relict, "relict", defaultValue: false)
				.Get(out data.LevelMax, "levelMax", int.MaxValue)
				.GetLazyIntFrom("item", -1, _items, delegate(Item _)
				{
					data.Item = _;
				}, data.Id);
			GetMoneyLazy(g, data.Price, "price");
			if (data.Count == 0)
			{
				data.Count = 1;
			}
			_shopGoods.Add(data.Id, data);
		});
	}

	private void ParseSubtitles(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Subtitle subtitle = new Subtitle();
			g.Get(out subtitle.Id, "id").Get(out subtitle.Video, "video").Get(out subtitle.Text, "text")
				.Get(out subtitle.StartTime, "startTime")
				.Get(out subtitle.EndTime, "endTime");
			_subtitles.Add(subtitle.Id, subtitle);
		});
	}

	public PersData GetPersDataByServerId(int id)
	{
		foreach (PersData persDatum in _persData)
		{
			if (persDatum.Id == id)
			{
				return persDatum;
			}
		}
		return null;
	}

	private void ParsePersArticuls(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			PersData data = new PersData();
			g.Get(out data.Id, "id").Get(out data.Title, "title").Get(out data.Description, "description")
				.Get(out data.Feature, "feature")
				.Get(out data.Class, "class")
				.Get(out data.Sex, "sex")
				.Get(out data.Cloth, "clothingOptions")
				.Get(out data.SelectWeapon, "selectWeapon")
				.Get(out data.SelectSet, "selectSet")
				.GetLazyIntFrom("startSpell", -1, _spells, delegate(Spell _)
				{
					data.StartSpell = _;
				})
				.GetLazyIntFrom("bonus", -1, _items, delegate(Item _)
				{
					data.Bonus = _;
				});
			string[] array = JsonUtils.JsonGet("selectHair", g.Hash, string.Empty).Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 2 && int.TryParse(array[1], out data.SelectHairColor))
			{
				data.SelectHair = array[0];
			}
			GetMoneyLazy(g, data.Money, "money");
			GetSkillsLazy(g, out data.Skills, "skills");
			_persData.Add(data);
		});
	}

	internal bool IsHasSpell(Spell spell)
	{
		foreach (int mySpell in MySpells)
		{
			Utils.Log("IsHasSpell", spell.Id, mySpell);
			if (spell.Id == mySpell)
			{
				return true;
			}
		}
		return false;
	}

	internal void ForeachMySpell(ActionD<Spell> action)
	{
		foreach (int mySpell in MySpells)
		{
			Spell value = null;
			if (_spells.TryGetValue(mySpell, out value) && value != null)
			{
				action(value);
			}
		}
	}

	internal int GetAllSpellsSchoolsCount()
	{
		List<string> list = new List<string>();
		foreach (Spell value in _spells.Values)
		{
			if (!list.Contains(value.SchoolName))
			{
				list.Add(value.SchoolName);
			}
		}
		return list.Count;
	}

	internal int GetMySpellsSchoolsCount()
	{
		List<string> names = new List<string>();
		ForeachMySpell(delegate(Spell _)
		{
			if (!names.Contains(_.SchoolName))
			{
				names.Add(_.SchoolName);
			}
		});
		return names.Count;
	}

	internal Spell GetMyMaxSpell(DamageTypeE skill)
	{
		return skill switch
		{
			DamageTypeE.FireMagic => GetMyMaxSpell(Skill.TypeE.MagicFire), 
			DamageTypeE.IceMagic => GetMyMaxSpell(Skill.TypeE.MagicIce), 
			DamageTypeE.DarkMagic => GetMyMaxSpell(Skill.TypeE.MagicDark), 
			DamageTypeE.LightingMagic => GetMyMaxSpell(Skill.TypeE.MagicElectro), 
			_ => null, 
		};
	}

	internal Spell GetMyMaxSpell(Skill.TypeE skill)
	{
		Spell spell = null;
		foreach (int mySpell in MySpells)
		{
			Spell value = null;
			if (_spells.TryGetValue(mySpell, out value) && value.SkillType == skill)
			{
				if (spell == null)
				{
					spell = value;
				}
				else if (value.Level > spell.Level)
				{
					spell = value;
				}
			}
		}
		return spell;
	}

	internal Spell GetMySpell(int level, Skill.TypeE skill)
	{
		Spell spell = null;
		foreach (int mySpell in MySpells)
		{
			Spell value = null;
			if (_spells.TryGetValue(mySpell, out value) && value.Level == level && value.SkillType == skill)
			{
				if (spell == null)
				{
					spell = value;
				}
				else if (spell.PowerK < value.PowerK)
				{
					spell = value;
				}
			}
		}
		return spell;
	}

	internal bool IsHasSpell(int level, Skill.TypeE skill)
	{
		foreach (int mySpell in MySpells)
		{
			Spell value = null;
			if (_spells.TryGetValue(mySpell, out value) && value.Level >= level && value.SkillType == skill)
			{
				return true;
			}
		}
		return false;
	}

	internal int MySpellsMaxLevel()
	{
		int num = -1;
		foreach (int mySpell in MySpells)
		{
			Spell value = null;
			if (_spells.TryGetValue(mySpell, out value) && value.Level > num)
			{
				num = value.Level;
			}
		}
		return num;
	}

	private void ParseSpells(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Spell data = new Spell();
			float value = 0f;
			g.Get(out data.Id, "id").Get(out data.Title, "title").Get(out data.Description, "description")
				.Get(out data.Level, "level")
				.Get(out data.SchoolName, "magicSchool")
				.Get(out data.IconName, "3dPicture")
				.Get(out data.EffectName, "3dEffect")
				.Get(out data.InMagicBook, "magicBook")
				.Get(out value, "power", 100f)
				.Get(out data.UpdateId, "updateId")
				.Get(out data.Points, "points", 100)
				.GetLazyIntFrom("prevSpell", -1, _spells, delegate(Spell _)
				{
					if (data.NextSpell == null)
					{
						data.NextSpell = _;
					}
				})
				.GetLazyIntFrom("prevSpell2", -1, _spells, delegate(Spell _)
				{
					if (data.NextSpell == null)
					{
						data.NextSpell = _;
					}
				});
			GetMoneyLazy(g, data.Price, "price");
			data.PowerK = value / 100f;
			_spells.Add(data.Id, data);
		});
	}

	private void ParseExecutions(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Fatality fatality = new Fatality();
			g.Get(out fatality.Id, "id").Get(out fatality.Title, "title").Get(out fatality.Scenario, "scenario")
				.Get(out fatality.WeaponString, "weaponTitle");
			_fatalities.Add(fatality.Id, fatality);
		});
	}

	internal LevelData GetLevelData(int level)
	{
		return _levels.GetInv(level, "GetLevelData");
	}

	private void ParseLevels(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			LevelData levelData = new LevelData();
			g.Get(out levelData.Id, "id").Get(out levelData.Title, "title").Get(out levelData.Level, "level")
				.Get(out levelData.SkillPoints, "skills")
				.Get(out levelData.Exp, "exp");
			_levels.Add(levelData.Level, levelData);
		});
	}

	public string GetHint(HintCodesE hintCode)
	{
		return GetHint(Enum.GetName(typeof(HintCodesE), hintCode)).Text;
	}

	internal HintData GetHint(string id)
	{
		HintData value = default(HintData);
		_hints.TryGetValue(id, out value);
		return value;
	}

	private void ParseHints(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			g.Get(out string value, "code").Get(out string value2, "title").Get(out string value3, "description");
			_hints.Add(value, new HintData
			{
				Name = value2,
				Text = value3
			});
		});
	}

	private void ParseLoadingTips(Dictionary<string, object> hashtable)
	{
		LoadingTips = new List<string>();
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			g.Get(out string value, "description");
			LoadingTips.Add(value);
			if (ServerData.OnLoadingTipsReady != null)
			{
				ServerData.OnLoadingTipsReady(this);
			}
		});
	}

	private void ParseColors(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			ServerColor serverColor = new ServerColor();
			g.Get(out serverColor.Id, "id").Get(out serverColor.Title, "title").Get(out serverColor.Code, "code");
			Colors.Add(serverColor);
		});
	}

	private void ParseChests(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Chest data = new Chest();
			g.Get(out data.Id, "id").Get(out data.Type, "type").Get(out data.PicturesCount, "count")
				.Get(out data.Index, "index")
				.Get(out data.ElfProbability, "elfProbability")
				.Get(out data.ElfLevelDifference, "elfLevelDifference")
				.Get(out data.ElfModel, "elfModel")
				.GetLazyIntFrom("bonus", -1, _bonuses, delegate(Bonus _)
				{
					data.Bonus = _;
				});
			Chests.Add(data.Id, data);
		});
	}

	public int GetPlayerExperiencePercentToNextLevel()
	{
		LevelData value = null;
		LevelData value2 = null;
		int num = 0;
		if (_levels.TryGetValue(PlayerParams.Level, out value))
		{
			num = value.Exp;
		}
		if (_levels.TryGetValue(PlayerParams.Level + 1, out value2))
		{
			return (int)(100f * ((float)(value2.Exp - PlayerParams.Experience) / (float)(value2.Exp - num)));
		}
		return 0;
	}

	public void AddPlayerExperience(int exp)
	{
		if ((!Globals.DebugAlwaysAddLevel && Globals.DebugDontAddLevel) || exp == 0)
		{
			return;
		}
		if (Globals.DebugAlwaysAddLevel)
		{
			LevelData value = null;
			if (!_levels.TryGetValue(PlayerParams.Level + 1, out value))
			{
				PlayerParams.Level++;
				return;
			}
			exp = value.Exp - PlayerParams._experience;
		}
		Utils.Log("AADDPLAYEREXP", PlayerParams.Experience, exp);
		while (exp > 0)
		{
			LevelData value2 = null;
			if (_levels.TryGetValue(PlayerParams.Level + 1, out value2))
			{
				if (value2.Exp <= PlayerParams._experience + exp)
				{
					PlayerParams.Level++;
					Metrics.OnNewLevel();
					PlayerParams._skillPoints += GetSkillPoint(PlayerParams.Level);
					exp -= value2.Exp - PlayerParams.Experience;
					PlayerParams._experience = value2.Exp;
					Messenger<int, int, string>.Invoke(Globals.MsgPlayerLevelChanged, PlayerParams.Level - 1, PlayerParams.Level, "AddPlayerExperience");
					continue;
				}
				PlayerParams._experience += exp;
				break;
			}
			PlayerParams._experience += exp;
			break;
		}
		Messenger<int>.Invoke(Globals.MsgPlayerExpChanged, 100 - SingletonT<ServerData>.I.GetPlayerExperiencePercentToNextLevel());
		Messenger.Invoke(Globals.MsgPlayerSkillPointsChanged);
	}

	public int GetSkillPoint(int level)
	{
		LevelData levelData = null;
		foreach (LevelData value in _levels.Values)
		{
			if (value.Level == level)
			{
				levelData = value;
				break;
			}
		}
		return levelData?.SkillPoints ?? 0;
	}

	private void AddPlayerSkill(Skill.TypeE skillType, int count, ref int skill)
	{
		Utils.Log("ADDPPLAYERSKILL", skillType, count, skill);
		int num = GetSkill(skillType).SkillPoint * Mathf.Abs(count);
		if (PlayerParams.SkillPoints >= num)
		{
			PlayerParams._skillPoints -= num;
			skill += count;
		}
	}

	internal void AddPlayerSkillHealth(int p)
	{
		AddPlayerSkill(Skill.TypeE.Vitality, p, ref PlayerParams.HP);
	}

	internal void AddPlayerSkillMagic(int p)
	{
		AddPlayerSkill(Skill.TypeE.Magic, p, ref PlayerParams.Magic);
	}

	internal void AddPlayerSkillRage(int p)
	{
		AddPlayerSkill(Skill.TypeE.Rage, p, ref PlayerParams.Rage);
	}

	internal void AddPlayerSkillStrength(int p)
	{
		AddPlayerSkill(Skill.TypeE.Strength, p, ref PlayerParams.Strength);
	}

	internal void PreparePlayerSkillDataForFight()
	{
		Dictionary<Skill.TypeE, float> dictionary = new Dictionary<Skill.TypeE, float>();
		Dictionary<Skill.TypeE, int> dict = AccumArmorsSkills();
		dictionary.Add(Skill.TypeE.MagicDark, 1f + (float)dict.Get(Skill.TypeE.MagicDark, 0) / 100f);
		dictionary.Add(Skill.TypeE.MagicElectro, 1f + (float)dict.Get(Skill.TypeE.MagicElectro, 0) / 100f);
		dictionary.Add(Skill.TypeE.MagicFire, 1f + (float)dict.Get(Skill.TypeE.MagicFire, 0) / 100f);
		dictionary.Add(Skill.TypeE.MagicIce, 1f + (float)dict.Get(Skill.TypeE.MagicIce, 0) / 100f);
		PlayerInBattleParams.SkillsK = dictionary;
	}

	internal Dictionary<Skill.TypeE, int> AccumArmorsSkills()
	{
		Dictionary<Skill.TypeE, int> r = new Dictionary<Skill.TypeE, int>();
		ForeachInBag(delegate(Item c)
		{
			if (c.PutOn && c.Skills != null)
			{
				SkillInfo[] skills = c.Skills;
				foreach (SkillInfo skillInfo in skills)
				{
					Skill.TypeE type = skillInfo.Skill.Type;
					int value = 0;
					if (r.TryGetValue(type, out value))
					{
						Dictionary<Skill.TypeE, int> dictionary2;
						Dictionary<Skill.TypeE, int> dictionary = (dictionary2 = r);
						Skill.TypeE key2;
						Skill.TypeE key = (key2 = type);
						int num = dictionary2[key2];
						dictionary[key] = num + skillInfo.Current;
					}
					else
					{
						r.Add(type, skillInfo.Current);
					}
				}
			}
		});
		return r;
	}

	private void ParseBattleFormula(Dictionary<string, object> hashtable)
	{
		JsonUtils.GetValues getValues = new JsonUtils.GetValues(_postLoadAllActions, hashtable);
		BattleParams battleParams = new BattleParams();
		getValues.Get(out battleParams.pCrit, "pCrit").Get(out battleParams.paCrit, "paCrit0").Get(out battleParams.persCR, "persCR")
			.Get(out battleParams.critK, "critK")
			.Get(out battleParams.critSec0, "critSec0")
			.Get(out battleParams.pMag, "pMAG")
			.Get(out battleParams.paMag, "paMAG")
			.Get(out battleParams.magSec0, "magSec0")
			.Get(out battleParams.magK, "magK")
			.Get(out battleParams.persMR, "persMR")
			.Get(out battleParams.ManaC, "manaC")
			.Get(out battleParams.manaK, "manaK")
			.Get(out battleParams.manaSec0, "manaSec0");
		BattleMathParams = battleParams;
	}

	internal void Update(float dt)
	{
		if (PlayerParams != null)
		{
			_anger_time += dt;
			if (GameSettings != null && PlayerParams._anger > 0 && GameSettings != null && GameSettings.AngerDownSpeed > 0)
			{
				while (_anger_time > (float)GameSettings.AngerDownSpeed)
				{
					_anger_time -= GameSettings.AngerDownSpeed;
					PlayerParams.Anger--;
				}
				if (PlayerParams._anger <= 0)
				{
					_anger_time = 0f;
				}
			}
		}
		if (GameSettings != null)
		{
			float num = InShopGoodsUpdateSeconds - dt;
			if (num < 0f)
			{
				ResetInShopGoodsUpdateSeconds();
				num = GameSettings.UpdateShopPeriodSeconds;
			}
			else
			{
				InShopGoodsUpdateSeconds = num;
			}
			int num2 = Mathf.FloorToInt(num);
			int arg = num2 % 60;
			int arg2 = num2 / 60;
			int arg3 = num2 / 60 / 60;
			Messenger<int, int, int>.Invoke(Globals.MsgShopResetTimeChanged, arg3, arg2, arg);
		}
	}

	public void UpdatePlayerArmorParams()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		foreach (Item item in GetAllPutOn())
		{
			num += item.GetSkill(Skill.TypeE.Vitality, 0);
			num2 += item.GetSkill(Skill.TypeE.Magic, 0);
			num3 += item.GetSkill(Skill.TypeE.Strength, 0);
			num4 += item.GetSkill(Skill.TypeE.Rage, 0);
			num7 += item.GetSkill(Skill.TypeE.BonusExp, 0);
			num6 += item.GetSkill(Skill.TypeE.BonusMana, 0);
			num5 += item.GetSkill(Skill.TypeE.BonusRage, 0);
			num8 += item.GetSkill(Skill.TypeE.BonusMoney, 0);
		}
		Utils.Log("UpdatePlayerArmorParams", "HP", PlayerParams.HP, num, "Magic", PlayerParams.Magic, num2, "Strength", PlayerParams.Strength, num3, "Rage", PlayerParams.Rage, num4);
		PlayerInBattleParams.HP = PlayerParams.HP + num;
		PlayerInBattleParams.Magic = PlayerParams.Magic + num2;
		PlayerInBattleParams.Strength = PlayerParams.Strength + num3;
		PlayerInBattleParams.Rage = PlayerParams.Rage + num4;
		PlayerInBattleParams.BonusExp = num7;
		PlayerInBattleParams.BonusMana = num6;
		PlayerInBattleParams.BonusRage = num5;
		PlayerInBattleParams.BonusMoney = num8;
	}

	public void GetMana(PersonParams personParams, BattleParams battleParams, bool critical, bool fromRages, bool isCombo, out int manaBallsCount, out int manaPerBall)
	{
		bool flag = false;
		manaPerBall = GameSettings.manaInBall;
		if (isCombo)
		{
			manaBallsCount = GameSettings.ComboManaBalls;
		}
		else if (critical)
		{
			manaBallsCount = ((!fromRages) ? GameSettings.critMBCount : GameSettings.critMBCountFromRages);
			if (!fromRages)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
			if (UnityEngine.Random.Range(0, 100) < GameSettings.manaOInitProb)
			{
				manaBallsCount = Utils.GetNRand(GameSettings.manaOCount, GameSettings.manaOProb);
			}
			else
			{
				manaBallsCount = 0;
			}
		}
		if (flag && manaBallsCount > 0 && PlayerInBattleParams.BonusMana > 0)
		{
			manaBallsCount += Utils.GetNRand(PlayerInBattleParams.BonusMana, GameSettings.manaBonusProb);
		}
	}

	public void GetDamage(PersonParams personParams, PersonParams enemyParams, bool critAllowed, bool forceCrit, int addDamagePercent, out bool isCrit, out int damage)
	{
		BattleParams battleMathParams = BattleMathParams;
		double num = Math.Max(0.05, Math.Min(0.7, battleMathParams.pCrit * (double)personParams.Rage / (double)enemyParams.Rage));
		int num2 = UnityEngine.Random.Range(0, 100);
		double num3 = num * 100.0;
		if (forceCrit && critAllowed)
		{
			isCrit = true;
		}
		else
		{
			isCrit = critAllowed && (double)num2 < num3;
		}
		if (addDamagePercent > 0)
		{
			isCrit = true;
		}
		float num4 = (float)((double)personParams.Strength / 10.0 * ((!isCrit) ? 1.0 : ((double)GameSettings.CritK)));
		if (_inBerserkMode && GameSettings.BerserkDamage > 0)
		{
			num4 *= (100f + (float)GameSettings.BerserkDamage) / 100f;
		}
		num4 *= UnityEngine.Random.Range(0.8f, 1.2f);
		damage = Mathf.FloorToInt(num4);
		if (damage == 0)
		{
			damage = 1;
		}
	}

	public int GetEnemyMagicDamage(bool isWeak)
	{
		MobParamsData enemyParams = EnemyParams;
		int num = Mathf.RoundToInt((float)enemyParams.Strength * 0.1f * (1f + ((!isWeak) ? GameSettings.MobMagicDamageStrongK : GameSettings.MobMagicDamageWeakK)));
		if (num < 1)
		{
			num = 1;
		}
		return num;
	}

	public int GetPlayerMagicDamage(Spell spell)
	{
		float num = PlayerInBattleParams.Strength;
		float num2 = 100f;
		float num3 = 50f;
		float num4 = 10f;
		float p = 0.256f;
		float num5 = 9f;
		float num6 = PlayerInBattleParams.Magic;
		float a = num4 * Mathf.Pow(num6 / num3, p) - num5;
		a = Mathf.Max(0.2f, Mathf.Min(a, 20f));
		float powerK = spell.PowerK;
		float num7 = PlayerInBattleParams.SkillsK.Get(spell.SkillType, 1f);
		int num8 = Mathf.FloorToInt(num7 * powerK * (num + num2 * a));
		num8 /= 10;
		return Mathf.Max(1, num8);
	}

	private void CooldownTurn(ref int counter, int max, Item.ElixirTypeE type)
	{
		if (counter > 0)
		{
			counter--;
			if (counter > 0)
			{
				Messenger.Invoke(Globals.MsgElixirCooldownChanged, type, counter, max);
			}
			else
			{
				Messenger.Invoke(Globals.MsgElixirCooldownChanged, type, 1, 1);
			}
		}
	}

	internal void BattleNewEnemyTurn()
	{
		if (BattlePoisonTurns > 0)
		{
			BattlePoisonTurns--;
			Messenger.Invoke(Globals.MsgElixirApplyPoisonOnEnemy);
		}
	}

	internal void BattleNewPlayerTurn()
	{
		CooldownTurn(ref BattleHealCooldown, BattleHealCooldownMax, Item.ElixirTypeE.Heal);
		CooldownTurn(ref BattleCriticalCooldown, BattleCriticalCooldownMax, Item.ElixirTypeE.Critical);
		CooldownTurn(ref BattlePoisonCooldown, BattlePoisonCooldownMax, Item.ElixirTypeE.Poison);
	}

	internal void NewBattle()
	{
		BattlePoisonTurns = 0;
		BattlePoisonCooldown = 0;
		BattleHealCooldown = 0;
		BattleCriticalCooldown = 0;
	}

	public int GetPlayerElixirsCount(Item.ElixirTypeE type)
	{
		return Utils.Sum(0, _Bag, (Item _) => (_.ElixirType == type && _.RealItemsCount > 0) ? _.RealItemsCount : 0);
	}

	public bool PlayerApplyPoison(out int damage, out int turns)
	{
		damage = 0;
		turns = 0;
		if (BattlePoisonCooldown > 0)
		{
			return false;
		}
		Item item = PlayerEatXXX(Item.ElixirTypeE.Poison);
		if (item != null)
		{
			damage = ((!Globals.DebugPoisonKills) ? item.RashodkaPower : 10000);
			if (Globals.DebugPoisonLargeDamage)
			{
				damage = Globals.Enemy.MaxHealth / 2;
			}
			turns = item.RashodkaEffectTurns;
			BattlePoisonTurns = turns;
			BattlePoisonCooldown = item.RashodkaNextTurns;
			BattlePoisonCooldownMax = item.RashodkaNextTurns;
			Messenger.Invoke(Globals.MsgElixirCooldownChanged, Item.ElixirTypeE.Poison, 0, BattlePoisonCooldownMax);
			Messenger.Invoke(Globals.MsgUseElixir, Item.ElixirTypeE.Poison);
			Metrics.OnPlayerUseElixirPoison();
		}
		return item != null;
	}

	public bool PlayerEatHealthElixir()
	{
		if (BattleHealCooldown > 0)
		{
			return false;
		}
		Item item = PlayerEatXXX(Item.ElixirTypeE.Heal);
		if (item != null)
		{
			int rashodkaPower = item.RashodkaPower;
			Globals.Player.Health = Globals.Player.Health + rashodkaPower;
			Globals.Player.ShowTextOnHpBar("+" + rashodkaPower, Person.TextTimeScreenDigits, new Vector3(0f, -20f, -50f), Person.TextSpeedScreenDigits, FontManager.ColorE.HealthPotion);
			BattleHealCooldown = item.RashodkaNextTurns;
			BattleHealCooldownMax = item.RashodkaNextTurns;
			Messenger.Invoke(Globals.MsgElixirCooldownChanged, Item.ElixirTypeE.Heal, 0, BattleHealCooldownMax);
			Messenger.Invoke(Globals.MsgUseElixir, Item.ElixirTypeE.Heal);
			Metrics.OnPlayerUseElixirHealth();
		}
		return item != null;
	}

	public int PlayerEatCriticalElixir()
	{
		if (BattleCriticalCooldown > 0)
		{
			return 0;
		}
		Item item = PlayerEatXXX(Item.ElixirTypeE.Critical);
		if (item == null)
		{
			return 0;
		}
		BattleCriticalCooldown = item.RashodkaNextTurns;
		BattleCriticalCooldownMax = item.RashodkaNextTurns;
		Messenger.Invoke(Globals.MsgElixirCooldownChanged, Item.ElixirTypeE.Critical, 0, BattleCriticalCooldownMax);
		Messenger.Invoke(Globals.MsgUseElixir, Item.ElixirTypeE.Critical);
		Metrics.OnPlayerUseElixirCrit();
		return item.RashodkaPower;
	}

	private Item PlayerEatXXX(Item.ElixirTypeE type)
	{
		Item item = null;
		int num = Utils.Sum(0, _Bag, delegate(Item _)
		{
			if (_.ElixirType != type || _.RealItemsCount <= 0)
			{
				return 0;
			}
			if (item == null)
			{
				item = _;
			}
			return _.RealItemsCount;
		});
		if (item == null)
		{
			return null;
		}
		item.RealItemsCount--;
		if (item.RealItemsCount == 0)
		{
			_Bag.Remove(item);
		}
		_removedElixirs.Add(item);
		Messenger.Invoke(Globals.MsgElixirCountChanged, item.ElixirType, SingletonT<ServerData>.I.GetPlayerElixirsCount(item.ElixirType));
		return item;
	}

	internal void RefreshElixirsCount()
	{
		Messenger.Invoke(Globals.MsgElixirCountChanged, Item.ElixirTypeE.Critical, GetPlayerElixirsCount(Item.ElixirTypeE.Critical));
		Messenger.Invoke(Globals.MsgElixirCountChanged, Item.ElixirTypeE.Heal, GetPlayerElixirsCount(Item.ElixirTypeE.Heal));
		Messenger.Invoke(Globals.MsgElixirCountChanged, Item.ElixirTypeE.Poison, GetPlayerElixirsCount(Item.ElixirTypeE.Poison));
	}

	internal void BattleFinishedWithWin(int mob, float time)
	{
		_removedElixirs.Clear();
		BattleStats battleStats = (BattleStats)UnityEngine.Object.FindObjectOfType(typeof(BattleStats));
		if (battleStats != null)
		{
			battleStats.AddBattleResult(mob, time - BattleStartTime);
		}
	}

	internal void BattleBreaked()
	{
		foreach (Item removedElixir in _removedElixirs)
		{
			Item item = null;
			foreach (Item item2 in _Bag)
			{
				if (item2.Id == removedElixir.Id)
				{
					item = item2;
					break;
				}
			}
			if (item == null)
			{
				item = removedElixir;
				AddToBag(item);
				item.RealItemsCount = 0;
			}
			item.RealItemsCount++;
			int num = Utils.Sum(0, _Bag, (Item _) => (_.ElixirType == item.ElixirType && _.RealItemsCount > 0) ? _.RealItemsCount : 0);
			Messenger.Invoke(Globals.MsgElixirCountChanged, item.ElixirType, SingletonT<ServerData>.I.GetPlayerElixirsCount(item.ElixirType));
		}
		_removedElixirs.Clear();
	}

	internal void BattleFinishedWithLose(int mob, float time)
	{
		BattleStats battleStats = (BattleStats)UnityEngine.Object.FindObjectOfType(typeof(BattleStats));
		if (battleStats != null)
		{
			battleStats.AddBattleResult(mob, time - BattleStartTime);
		}
		BattleBreaked();
	}

	internal static void BuyMine(Mine mine)
	{
		if (!BuyedMines.Contains(mine.Id))
		{
			BuyedMines.Add(mine.Id);
			Metrics.OnBuyMine(mine);
		}
	}

	private void ParseMines(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Mine data = new Mine();
			g.Get(out data.Id, "id").Get(out data.Order, "order").Get(out data.Difficulty_, "dif")
				.GetLazyIntFrom("bonusF", -1, _bonuses, delegate(Bonus _)
				{
					data.FirstBonus = _;
				})
				.GetLazyIntFrom("bonus", -1, _bonuses, delegate(Bonus _)
				{
					data.SecondBonus = _;
				});
			GetMoneyLazy(g, data.Price, "price");
			GetMoneyLazy(g, data.OpenPrice, "openPrice");
			GetMoneyLazy(g, data.ContinuePrice, "contPrice");
			Mines.Add(data);
		});
		Mines.Sort((Mine x, Mine y) => x.Order.CompareTo(y.Order));
	}

	private void ParseItemRelations(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			ItemRelation data = new ItemRelation();
			g.Get(out data.Type, "type").Get(out data.Count, "count").GetLazyIntFrom("first", -1, _items, delegate(Item _)
			{
				data.From = _;
			})
				.GetLazyIntFrom("second", -1, _items, delegate(Item _)
				{
					data.To = _;
				});
			_itemRelations.Add(data);
		});
	}

	private void ParseGameSettings(Dictionary<string, object> hashtable)
	{
		JsonUtils.GetValues getValues = new JsonUtils.GetValues(_postLoadAllActions, hashtable);
		Invs.Inv(GameSettings == null, "GameSettings == null");
		Settings data = new Settings();
		int value = 0;
		int value2 = 0;
		bool value3 = false;
		JsonUtils.GetValues.MyPair<Location.BotLocationInfo> myPair = new JsonUtils.GetValues.MyPair<Location.BotLocationInfo>();
		JsonUtils.GetValues.MyPair<MobRespProb> myPair2 = new JsonUtils.GetValues.MyPair<MobRespProb>();
		getValues.Get(out data.BerserkLength, "berserkLength", 15).Get(out data.BerserkDamage, "berserkDamage", 0).Get(out data.BerserkMaxHealth, "berserkMaxHealth", 0)
			.Get(out data.AngerDownSpeed, "angerDownSpeed", 1)
			.Get(out data.FatalityTime, "fatalityTime")
			.Get(out data.SellPrice, "sellPrice")
			.Get(out data.ChestPeriod, "chestPeriod")
			.Get(out data.ChestLifeTime, "chestLifeTime", -1)
			.Get(out data.ChestsMaxCount, "chestsMaxCount")
			.Get(out data.ProbFatality, "probFatality", 0)
			.Get(out data.mgBM1, "mgBM1", 4)
			.Get(out data.mgBM2, "mgBM2", 4)
			.Get(out data.mgBM3, "mgBM3", 4)
			.Get(out data.mgBM4, "mgBM4", 4)
			.Get(out data.mg2BM1, "mg2BM1", 10)
			.Get(out data.mg2BM2, "mg2BM2", 10)
			.Get(out data.mg2BM3, "mg2BM3", 10)
			.Get(out data.mg2BM4, "mg2BM4", 10)
			.Get(out data.mg2Time, "mg2Time", 3)
			.Get(out data.ComboManaBalls, "comboManaBalls", 4)
			.Get(out value, "comboK", 100)
			.Get(out data.TurnTime, "turnTime", 6f)
			.Get(out data.MobMagicDamageWeakK, "mobWeakMagicK", 1f)
			.Get(out data.MobMagicDamageStrongK, "mobStrMagicK", 1f)
			.Get(out var value4, "criticalK", 100)
			.Get(out data.SlicesForFatality, "slicesForFatality")
			.Get(out data.UpdateShopPeriodSeconds, "shopUP")
			.Get(out data.LocationMobLevel, "locationMobLevel", 0)
			.Get(out data.LocationMobLevelOffset, "locationMobLevelOffset", 0)
			.Get(out data.LocationMobLevelMax, "locationMobLevelMax", 0)
			.Get(out data.LocationMobLevelUpCost, "locationMobLevelUpCost", 1)
			.Get(out data.MagicProtectionTime1, "mobMGame1", 1)
			.Get(out data.MagicProtectionTime2, "mobMGame2", 2)
			.Get(out data.MagicProtectionTime3, "mobMGame3", 3)
			.Get(out data.Crits2StarsPercent, "crits2Stars", 0)
			.Get(out data.Crits3StarsPercent, "crits3Stars", 0)
			.Get(out data.Crits4StarsPercent, "crits4Stars", 0)
			.Get(out data.Crits5StarsPercent, "crits5Stars", 0)
			.Get(out data.FallingStarCooldown, "fallingStarCooldown", 60)
			.Get(out data.FallingStarProb, "fallingStarProb", 0)
			.Get(out data.FallingStarTime, "fallingStarTime", 900)
			.Get(out data.MusicChangeTime, "musicChangeTime", 2f)
			.Get(out data.TimeTickMagicProtection, "timeTickMagicProtection", 1.1f)
			.Get(out data.IntroFadeInOutTime, "introFadeInOutTime", 1f)
			.Get(out data.IntroReadingTime, "introReadingTime", 1f)
			.Get(out data.IntroDayPassTime, "introFadeInOutTime", 1f)
			.Get(out data.RageShereProbSimple, "rageSPSimple", 25)
			.Get(out data.RageShereProbCrit, "rageSPCrit", 100)
			.Get(out data.EyePeriod, "eyePeriod", 5)
			.Get(out data.EyeProb1, "eyeP1", 10)
			.Get(out data.EyeProb2, "eyeP2", 20)
			.Get(out data.EyeProb3, "eyeP3", 35)
			.Get(out data.manaInBall, "manaInBall")
			.Get(out data.critMBCount, "critMBCount")
			.Get(out data.critMBCountFromRages, "critMBCountRages", data.critMBCount)
			.Get(out data.manaOCount, "manaOCount")
			.Get(out data.manaOProb, "manaOProb")
			.Get(out data.manaOInitProb, "manaOInitProb")
			.Get(out data.manaBonusProb, "manaBonusProb")
			.Get(out data.rageBonusProb, "rageBonusProb")
			.Get(out value2, "magicRes", 0)
			.Get(out value3, "showfps", defaultValue: false)
			.Get(out data.IdlePeriod, "idlePeriod", 5f)
			.Get(out data.DefaultRedWeapon, "defaultRedWeapon", "12")
			.Get(out data.DefaultGreenWeapon, "defaultGreenWeapon", "12")
			.Get(out data.DefaultBlueWeapon, "defaultBlueWeapon", "12")
			.Get(out data.BloodScreenHealthTreshold, "bloodScreenHealthTreshold", 50)
			.Get(out data.BloodScreenHealthTreshold2, "bloodScreenHealthTreshold2", data.BloodScreenHealthTreshold)
			.Get(out data.BloodScreenHealthTreshold3, "bloodScreenHealthTreshold3", data.BloodScreenHealthTreshold)
			.Get(out data.TeaserPage, "teaserPage", "http://juggermobile.com/")
			.Get(out data.FaqPage, "faqPage", "http://juggermobile.com/faq.php")
			.Get(out data.FacebookCommunityUrl, "facebookCommunityUrl", "http://cafe.naver.com/gameclubmini")
			.Get(out data.FacebookCommunityUrlNew, "facebookCommunityUrlNew", "http://www.facebook.com/pages/Juggerlive/455504447837571?fref=ts")
			.GetLazyIntFrom("defaultFatality", -1, _fatalities, delegate(Fatality _)
			{
				data.DefaultFatality = _;
			})
			.Get(out data.ElfPeriod, "elfPeriod", 0)
			.Get(out data.ElfProb, "elfProb", 0)
			.Get(out data.Idle2Prob, "idle2Prob", 50)
			.Get(out data.AchievmentSharingMoneyBonus, "achievmentSharingMoneyBonus", 30)
			.Get(out data.SkillComboLevel, "skillComboLevel", 2)
			.Get(out data.SkillMagicLevel, "skillMagicLevel", 3)
			.Get(out data.SkillRageLevel, "skillRageLevel", 4)
			.Get(out data.RatingSocialCulldown, "ratingSocialCulldown", 360)
			.Get(out data.RatingSharingMoneyBonus, "ratingSharingMoneyBonus", 1)
			.Get(out data.MobsRespCooldown, "mobsRespCooldown", 60)
			.Get(out data.LevelCheckNotifs, "levelCheckNotifs", 7)
			.Get(out data.BankFree, "bankFree", string.Empty)
			.Get(out data.sovLevS0, "sovLevS0", 0)
			.Get(out data.sovLevS1, "sovLevS1", 0)
			.Get(out data.sovLevS2, "sovLevS2", 0)
			.GetLazyIntFrom("elfBonus", -1, _bonuses, delegate(Bonus _)
			{
				data.ElfBonus = _;
			})
			.GetLazyIntFrom("lmBonus", -1, _bonuses, delegate(Bonus _)
			{
				data.MonsterFromLocationBonus = _;
			})
			.Get(out data.Elfs, "elfs", delegate(Dictionary<string, object> hash)
			{
				Location.BotLocationInfo bi = new Location.BotLocationInfo();
				int value5 = -1;
				JsonUtils.GetValues getValues2 = new JsonUtils.GetValues(_postLoadAllActions, hash);
				getValues2.Get(out bi.Level, "level").Get(out bi.IsBoss, "isBoss").Get(out value5, "botId", -1);
				if (value5 == -1)
				{
					return (Location.BotLocationInfo)null;
				}
				getValues2.GetAlwaysLazyIntFrom("botId", -1, _bots, delegate(BotInfo _)
				{
					bi.Bot = _;
				});
				return bi;
			})
			.Get(out data.MobsRespawnProbs, "mobsRespProbs", delegate(Dictionary<string, object> hash)
			{
				MobRespProb result = default(MobRespProb);
				JsonUtils.GetValues getValues2 = new JsonUtils.GetValues(_postLoadAllActions, hash);
				getValues2.Get(out result.Count, "currentBobsCount").Get(out result.Prob, "respProb");
				return result;
			})
			.Get(out data.Match3TimerNormal, "match3TimerNormal", 60)
			.Get(out data.Match3TimerEasy, "match3TimerEasy", 90)
			.Get(out data.Match3TimeKeysCostNormal, "match3TimeKeysCostNormal", 2)
			.Get(out data.Match3TimeKeysCostEasy, "match3TimeKeysCostEasy", 2)
			.Get(out data.Match3BonusProbNormal, "match3BonusProbNormal", 10)
			.Get(out data.Match3BonusProbEasy, "match3BonusProbEasy", 10)
			.Get(out data.UrlEvolution, "UrlEvolution", string.Empty)
			.Get(out data.UrlEvolutionAndroid, "UrlEvolutionAndroid", string.Empty)
			.Get(out data.ShowEvolutionPromoInRu, "ShowEvolutionPromoInRu", defaultValue: false)
			.Get(out data.ShowEvolutionPromoInWorld, "showEvolutionPromoInWorld", defaultValue: false);
		if (Globals.DebugNoLevelLimitForRage)
		{
			data.SkillRageLevel = 0;
		}
		if (Globals.DebugNoLevelLimitForMana)
		{
			data.SkillMagicLevel = 0;
		}
		if (Globals.DebugShortPeriods)
		{
			data.ChestPeriod = 5;
		}
		if (value2 > 0)
		{
			data.MagicRes = (float)(100 - value2) / 100f;
		}
		Globals.ForceFPS = value3;
		data.ComboK = (float)value / 100f;
		data.CritK = (float)value4 / 100f;
		if (Globals.DebugLocationsFastChest)
		{
			data.ChestPeriod = 1;
		}
		GameSettings = data;
	}

	internal Dictionary<int, Npc> GetNpcs()
	{
		return _npcs;
	}

	private void ParseStorylineDialogs(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			StorylineDialog storylineDialog = new StorylineDialog();
			bool flag = true;
			try
			{
				g.Get(out storylineDialog.Id, "id").Get(out storylineDialog.Title, "title").Get(out string value, "bot")
					.Get(out Dictionary<string, object> value2, "dialogs");
				string[] array = value.Split('_');
				int.TryParse(array[0], out var result);
				int.TryParse(array[1], out var result2);
				int.TryParse(array[2], out var result3);
				storylineDialog.LocationBot = Tuple.Create(result, result2, result3);
				ParseDialogPhrases(value2, storylineDialog.Dialogs);
			}
			catch (Exception ex)
			{
				Utils.LogForce("PARSE STORYLINE failed", storylineDialog.Id, ex.Message);
				flag = true;
			}
			if (flag)
			{
				if (!_storylineDialogs.ContainsKey(storylineDialog.LocationBot))
				{
					_storylineDialogs.Add(storylineDialog.LocationBot, storylineDialog);
				}
				else
				{
					Utils.LogForce("STORYLINE already exist", storylineDialog.Id, storylineDialog.LocationBot, "already have", _storylineDialogs[storylineDialog.LocationBot].Id);
				}
			}
		});
	}

	private void ParseDialogPhrases(Dictionary<string, object> dialogs, List<DialogPhrase> list)
	{
		foreach (KeyValuePair<string, object> dialog in dialogs)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)dialog.Value;
			DialogPhrase dialogPhrase = new DialogPhrase();
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				switch (item.Key)
				{
				case "npc":
					int.TryParse((string)item.Value, out dialogPhrase.Npc);
					break;
				case "maleText":
					dialogPhrase.MaleText = (string)item.Value;
					break;
				case "femaleText":
					dialogPhrase.FemaleText = (string)item.Value;
					break;
				case "location":
					dialogPhrase.Origin = (string)item.Value;
					break;
				}
			}
			if (dialogPhrase.Npc != -1)
			{
				list.Add(dialogPhrase);
			}
		}
	}

	private void ParseNpcs(Dictionary<string, object> hashtable)
	{
		ParseAll(hashtable, delegate(JsonUtils.GetValues g)
		{
			Npc npc = new Npc();
			try
			{
				g.Get(out npc.Id, "id").Get(out npc.Title, "title").Get(out npc.Picture, "picture");
			}
			catch
			{
				Utils.Log("PARSE NPCS failed", npc.Id);
				throw;
			}
			_npcs.Add(npc.Id, npc);
		});
	}

	private bool IsValidVersion(Dictionary<string, object> hash)
	{
		float num = JsonUtils.JsonGetFloat("minVersion", hash, 1f);
		float num2 = JsonUtils.JsonGetFloat("maxVersion", hash, float.MaxValue);
		return Globals.CONTENT_PACK_VERSION >= num && Globals.CONTENT_PACK_VERSION <= num2;
	}

	private Dictionary<string, object> JsonGetTable(string name, Dictionary<string, object> hashtable)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> dictionary2 = JsonUtils.JsonGetHashtable(name, hashtable);
		foreach (KeyValuePair<string, object> item in dictionary2)
		{
			Dictionary<string, object> dictionary3 = JsonUtils.JsonTryGet<Dictionary<string, object>>(item.Key, dictionary2, null);
			if (dictionary3 == null || IsValidVersion(dictionary3))
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		return dictionary;
	}

	private void ParseDataFile(string key, Dictionary<string, object> hashtable)
	{
		switch (key)
		{
		case "botLevels":
			ParseBotLevels(_botLevels, JsonGetTable("botLevels", hashtable));
			break;
		case "bossLevels":
			ParseBotLevels(_bossLevels, JsonGetTable("bossLevels", hashtable));
			break;
		case "persArtikuls":
			ParsePersArticuls(JsonGetTable("persArtikuls", hashtable));
			break;
		case "storeGoods":
			ParseStoreGoods(JsonGetTable("storeGoods", hashtable));
			break;
		case "bots":
			ParseBots(JsonGetTable("bots", hashtable));
			break;
		case "phrases":
			ParsePhrases(JsonGetTable("phrases", hashtable));
			break;
		case "bigTexts":
			ParseBigTexts(JsonGetTable("bigTexts", hashtable));
			break;
		case "newsTypes":
			ParseNewsTypes(JsonGetTable("newsTypes", hashtable));
			break;
		case "items":
			ParseItems(JsonGetTable("items", hashtable));
			break;
		case "slots":
			ParseSlots(JsonGetTable("slots", hashtable));
			break;
		case "skillArtikuls":
			ParseSkillArtikuls(JsonGetTable("skillArtikuls", hashtable));
			break;
		case "colors":
			ParseColors(JsonGetTable("colors", hashtable));
			break;
		case "locations":
			ParseLocations(JsonGetTable("locations", hashtable));
			break;
		case "settings":
			ParseGameSettings(JsonGetTable("settings", hashtable));
			break;
		case "moneyTypes":
			ParseMoneyTypes(JsonGetTable("moneyTypes", hashtable));
			break;
		case "levels":
			ParseLevels(JsonGetTable("levels", hashtable));
			break;
		case "spells":
			ParseSpells(JsonGetTable("spells", hashtable));
			break;
		case "news":
			ParseNews(JsonGetTable("news", hashtable));
			break;
		case "bonuses":
			ParseBonuses(JsonGetTable("bonuses", hashtable));
			break;
		case "sets":
			ParseSets(JsonGetTable("sets", hashtable));
			break;
		case "hints":
			ParseHints(JsonGetTable("hints", hashtable));
			break;
		case "loadingTips":
			ParseLoadingTips(JsonGetTable("loadingTips", hashtable));
			break;
		case "battleFormula":
			ParseBattleFormula(JsonGetTable("battleFormula", hashtable));
			break;
		case "executions":
			ParseExecutions(JsonGetTable("executions", hashtable));
			break;
		case "conditions":
			ParseConditions(JsonGetTable("conditions", hashtable));
			break;
		case "achievements":
			ParseAchievements(JsonGetTable("achievements", hashtable));
			break;
		case "chest":
			ParseChests(JsonGetTable("chest", hashtable));
			break;
		case "bank":
			if (!UnityApi.UseGameClub())
			{
				ParseBankItems(JsonGetTable("bank", hashtable));
			}
			break;
		case "bankAndroid":
			if (UnityApi.UseGameClub())
			{
				ParseBankItems(JsonGetTable("bankAndroid", hashtable));
			}
			break;
		case "dialogs":
			ParseStorylineDialogs(JsonGetTable("dialogs", hashtable));
			break;
		case "npc":
			ParseNpcs(JsonGetTable("npc", hashtable));
			break;
		case "gameSettings":
			break;
		case "subtitles":
			ParseSubtitles(JsonGetTable("subtitles", hashtable));
			break;
		case "mines":
			ParseMines(JsonGetTable("mines", hashtable));
			break;
		case "relations":
			ParseItemRelations(JsonGetTable("relations", hashtable));
			break;
		default:
			Utils.Log("Unknown data file", key);
			break;
		}
	}

	private void ParseSets(Dictionary<string, object> hashtable)
	{
	}

	private void ParseNews(Dictionary<string, object> hashtable)
	{
	}

	private void ParseAll(Dictionary<string, object> hashtable, ActionD<JsonUtils.GetValues> getElement)
	{
		JsonUtils.GetValues getValues = new JsonUtils.GetValues(_postLoadAllActions, null);
		foreach (string key in hashtable.Keys)
		{
			getValues.Hash = JsonUtils.JsonGetHashtable(key, hashtable);
			getElement(getValues);
		}
	}

	private static Dictionary<string, object> ParseJsonText(string text, string debugInfo)
	{
		bool success = true;
		Dictionary<string, object> dictionary = ParseJson(text, ref success);
		Invs.Inv(dictionary != null && success, "ParseJsonText failed", debugInfo);
		return dictionary;
	}

	private void ParseMainFile(MonoBehaviour component, bool storeFile, List<FileInfo> filesList, FuncD<string, string, string> getLoadPath, ActionD<MonoBehaviour, string, ActionD<byte[]>, ActionD<string>> loadFile, ActionD onFinished)
	{
		if (filesList.Count == 0)
		{
			onFinished();
			LoadFinished(StateE.Loaded);
		}
		else if (_parallelLoad)
		{
			ParseMainFilePara(component, storeFile, filesList, getLoadPath, loadFile, onFinished);
		}
		else
		{
			ParseMainFileRec(component, storeFile, filesList.GetEnumerator(), getLoadPath, loadFile, onFinished);
		}
	}

	private void ParseMainFileRec(MonoBehaviour component, bool storeFile, IEnumerator<FileInfo> filesEnum, FuncD<string, string, string> getLoadPath, ActionD<MonoBehaviour, string, ActionD<byte[]>, ActionD<string>> loadFile, ActionD onFinished)
	{
		if (!filesEnum.MoveNext())
		{
			ProcessPostLoadActions();
			LoadFinished(StateE.Loaded);
			onFinished();
			return;
		}
		FileInfo current = filesEnum.Current;
		string path = current.Path;
		string keyName = current.Name;
		string filePath = getLoadPath(keyName, path);
		FileInfo cc = current;
		loadFile(component, filePath, delegate(byte[] fileBytes)
		{
			try
			{
				if (storeFile)
				{
					Invs.Inv(cc.OriginalBytes == null, "cc.OriginalBytes == null");
					cc.OriginalBytes = fileBytes;
				}
				string text = ((!Globals.UseEncryptedJsonAdmin) ? Encoding.UTF8.GetString(fileBytes) : Decompress(Decrypt(fileBytes)));
				ParseDataFile(keyName, ParseJsonText(text, path));
				ParseMainFileRec(component, storeFile, filesEnum, getLoadPath, loadFile, onFinished);
			}
			catch (Exception ex)
			{
				Utils.LogForce("Parse file failed", filePath, ex.Message);
				ClearPostLoadActions();
				LoadFinished(StateE.Failed);
				onFinished();
			}
		}, delegate(string errorMessage)
		{
			Utils.LogForce("Load data file failed", filePath, errorMessage);
			ClearPostLoadActions();
			LoadFinished(StateE.Failed);
			onFinished();
		});
	}

	private void ParseMainFilePara(MonoBehaviour component, bool storeFile, List<FileInfo> filesList, FuncD<string, string, string> getLoadPath, ActionD<MonoBehaviour, string, ActionD<byte[]>, ActionD<string>> loadFile, ActionD onFinished)
	{
		string t = Time.frameCount.ToString();
		if (filesList.Count == 0)
		{
			onFinished();
			LoadFinished(StateE.Loaded);
			return;
		}
		int loaded = 0;
		bool error = false;
		foreach (FileInfo files in filesList)
		{
			if (error)
			{
				break;
			}
			string path = files.Path;
			string keyName = files.Name;
			string filePath = getLoadPath(keyName, path);
			FileInfo cc = files;
			loadFile(component, filePath, delegate(byte[] fileBytes)
			{
				if (error)
				{
					return;
				}
				try
				{
					if (storeFile)
					{
						Invs.Inv(cc.OriginalBytes == null, "cc.OriginalBytes == null");
						cc.OriginalBytes = fileBytes;
					}
					string text = ((!Globals.UseEncryptedJsonAdmin) ? Encoding.UTF8.GetString(fileBytes) : Decompress(Decrypt(fileBytes)));
					ParseDataFile(keyName, ParseJsonText(text, path));
					loaded++;
					if (loaded == filesList.Count)
					{
						ProcessPostLoadActions();
						LoadFinished(StateE.Loaded);
						onFinished();
					}
				}
				catch (Exception ex)
				{
					Utils.LogForce("Parse file failed", filePath, ex.Message);
					if (!error)
					{
						error = true;
						ClearPostLoadActions();
						LoadFinished(StateE.Failed);
						onFinished();
					}
				}
			}, delegate(string errorMessage)
			{
				Utils.LogForce("Load data file failed", t, filePath, errorMessage);
				if (!error)
				{
					error = true;
					ClearPostLoadActions();
					LoadFinished(StateE.Failed);
					onFinished();
				}
			});
		}
	}

	private static Dictionary<string, object> ParseJson(string text, ref bool success)
	{
		success = true;
		return (Dictionary<string, object>)Json.Deserialize(text);
	}

	private static byte[] Decrypt(byte[] cipherBytes)
	{
		Aes cipher = new Aes(Aes.KeySize.Bits256, Globals.EncryptionKeyBytes);
		return AesLib.Utility.Decrypt(cipher, cipherBytes);
	}

	private static string Decompress(byte[] compressedBytes)
	{
		return Compression.Utility.Decompress(compressedBytes);
	}

	private void LoadTemplateFromResources(string filePath, ActionD<string> onLoad, ActionD<string> onError)
	{
		if (Globals.UseEncryptedJsonAdmin)
		{
			SingletonT<ResourcesManager>.I.LoadBytes(filePath, delegate(byte[] fileBytes)
			{
				onLoad(Decompress(Decrypt(fileBytes)));
			}, onError);
		}
		else
		{
			SingletonT<ResourcesManager>.I.LoadText(filePath, onLoad, onError);
		}
	}

	private void ParseMainFileInResources(MonoBehaviour component, string text, string basePath, FuncD<string, string> getLoadPath, FuncD<string, string> getSavePath)
	{
		bool success = false;
		Dictionary<string, object> t = ParseJson(text, ref success);
		Dictionary<string, object> templates = JsonUtils.JsonGetHashtable("templates", JsonUtils.JsonGetHashtable("settings", t));
		int loaded = 0;
		bool error = false;
		foreach (string key in templates.Keys)
		{
			if (error)
			{
				break;
			}
			Dictionary<string, object> dictionary = (Dictionary<string, object>)templates[key];
			Invs.Inv(dictionary.ContainsKey("path"), "templates.ContainsKey('path')", key);
			string path = (string)dictionary["path"];
			string keyName = key;
			string filePath = basePath + getLoadPath(path);
			LoadTemplateFromResources(filePath, delegate(string fileText)
			{
				if (!error)
				{
					ParseDataFile(keyName, ParseJsonText(fileText, path));
					loaded++;
					if (loaded == templates.Count)
					{
						ProcessPostLoadActions();
						LoadFinished(StateE.Loaded);
					}
				}
			}, delegate(string errorMessage)
			{
				Utils.LogForce("Load data file failed", filePath, errorMessage);
				if (!error)
				{
					error = true;
					ClearPostLoadActions();
					LoadFinished(StateE.Failed);
				}
			});
		}
	}

	private void ProcessPostLoadActions()
	{
		foreach (ActionD postLoadAllAction in _postLoadAllActions)
		{
			postLoadAllAction();
		}
		ClearPostLoadActions();
	}

	private void ClearPostLoadActions()
	{
		_postLoadAllActions.Clear();
	}

	private Dictionary<string, Dictionary<string, object>> GetConfigFilesList(string text)
	{
		Dictionary<string, Dictionary<string, object>> dictionary = new Dictionary<string, Dictionary<string, object>>();
		if (string.IsNullOrEmpty(text))
		{
			return dictionary;
		}
		bool success = false;
		Dictionary<string, object> t = ParseJson(text, ref success);
		Invs.Inv(success, "!success");
		Dictionary<string, object> dictionary2 = JsonUtils.JsonGetHashtable("templates", JsonUtils.JsonGetHashtable("settings", t));
		foreach (string key2 in dictionary2.Keys)
		{
			Dictionary<string, object> dictionary3 = (Dictionary<string, object>)dictionary2[key2];
			string key = (string)dictionary3["path"];
			dictionary.Add(key, dictionary3);
		}
		return dictionary;
	}

	private static List<FileInfo> GetFilesList(string text)
	{
		List<FileInfo> list = new List<FileInfo>();
		if (string.IsNullOrEmpty(text))
		{
			return list;
		}
		bool success = false;
		Dictionary<string, object> t = ParseJson(text, ref success);
		Invs.Inv(success, "!success");
		Dictionary<string, object> dictionary = JsonUtils.JsonGetHashtable("templates", JsonUtils.JsonGetHashtable("settings", t));
		foreach (string key in dictionary.Keys)
		{
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary[key];
			string path = (string)dictionary2["path"];
			list.Add(new FileInfo
			{
				Name = key,
				Path = path,
				Time = JsonUtils.JsonGetInt("mtime", dictionary2)
			});
		}
		return list;
	}

	private static void InitConfigsFromImageData(MonoBehaviour component, string localization, ActionD<ServerData> loaded, ActionD<string> onError)
	{
		if (Globals.UseJsonAdmin)
		{
			InitConfigsFromImageDataJson(component, localization, loaded, onError);
		}
		else
		{
			InitConfigsFromImageDataProto(component, localization, loaded, onError);
		}
	}

	private static void InitConfigsFromImageDataProto(MonoBehaviour component, string localization, ActionD<ServerData> loaded, ActionD<string> onError)
	{
		try
		{
			string path = ImageBasePath + "data_" + localization + ".dat";
			SingletonT<ResourcesManager>.I.LoadBytes(path, delegate(byte[] bytes)
			{
				ServerData serverData = CreateServerData(bytes);
				if (serverData == null)
				{
					onError("CreateServerData(bytes) failed");
				}
				else
				{
					loaded(serverData);
				}
			}, onError);
		}
		catch (Exception ex)
		{
			onError("InitConfigsFromLocalData failed: " + ex.Message);
		}
	}

	private static void InitConfigsFromImageDataJson(MonoBehaviour component, string localization, ActionD<ServerData> loaded, ActionD<string> onError)
	{
		try
		{
			ServerData serverData = new ServerData();
			string path = ImageBasePath + "config_" + localization + ".json";
			SingletonT<ResourcesManager>.I.LoadText(path, delegate(string text)
			{
				serverData._loadedFilesInfo = GetFilesList(text);
				serverData.ParseMainFileInResources(component, text, ImageBasePath, (string inPath) => localization + "/" + Path.GetFileNameWithoutExtension(inPath) + ".json", null);
				loaded(serverData);
			}, onError);
		}
		catch (Exception ex)
		{
			onError("InitConfigsFromLocalData failed: " + ex.Message);
		}
	}

	private static string LocalConfigFilePath(string localization, string x)
	{
		return DeviceBasePath + localization + "/" + x + ".json";
	}

	private static void InitConfigsFromDeviceData(MonoBehaviour component, string localization, ActionD<ServerData> loaded, ActionD<string> onError)
	{
		if (Globals.UseJsonAdmin)
		{
			InitConfigsFromDeviceDataJson(component, localization, loaded, onError);
		}
		else
		{
			InitConfigsFromDeviceDataProto(component, localization, loaded, onError);
		}
	}

	private static void InitConfigsFromDeviceDataProto(MonoBehaviour component, string localization, ActionD<ServerData> loaded, ActionD<string> onError)
	{
		try
		{
			string text = "data_" + localization + ".dat";
			Utils.Log("DEVICEPATH", DeviceBasePath + text);
			byte[] bytes = Utils.ReadAllBytes(DeviceBasePath + text);
			ServerData serverData = CreateServerData(bytes);
			if (serverData == null)
			{
				onError("CreateServerData(bytes)");
			}
			else
			{
				loaded(serverData);
			}
		}
		catch (Exception ex)
		{
			onError("InitConfigsFromDeviceData failed:" + ex.Message);
		}
	}

	private static void InitConfigsFromDeviceDataJson(MonoBehaviour component, string localization, ActionD<ServerData> loaded, ActionD<string> onError)
	{
		try
		{
			string text = "config_" + localization;
			Utils.Log("DEVICEPATH", DeviceBasePath + text + ".json");
			string text2 = Utils.ReadAllText(DeviceBasePath + text + ".json");
			ServerData serverData = new ServerData();
			serverData._loadedFilesInfo = GetFilesList(text2);
			serverData.ParseMainFile(component, storeFile: true, serverData._loadedFilesInfo, (string x, string _) => LocalConfigFilePath(localization, x), LoadSystemIOFile, delegate
			{
				if (serverData.LoadState == StateE.Loaded)
				{
					loaded(serverData);
				}
				else
				{
					onError("InitConfigsFromDeviceData failed:" + serverData.LoadState);
				}
			});
		}
		catch (Exception ex)
		{
			onError("InitConfigsFromDeviceData failed:" + ex.Message);
		}
	}

	private void SetupData(ServerData data)
	{
		Utils.LogForce("======SETUPSERVERDATA");
		_bonuses = data._bonuses;
		_bossLevels = data._bossLevels;
		_botLevels = data._botLevels;
		_bots = data._bots;
		_items = data._items;
		_levels = data._levels;
		foreach (KeyValuePair<int, Location> location in _locations)
		{
			Location locationByServerId = data.GetLocationByServerId(location.Value.Id);
			if (locationByServerId != null)
			{
				locationByServerId.Logic = location.Value.Logic;
			}
		}
		_bankItems = data._bankItems;
		_locations = data._locations;
		_achievements = data._achievements;
		_conditions = data._conditions;
		_moneyTypes = data._moneyTypes;
		_persData = data._persData;
		_phrases = data._phrases;
		_bigTexts = data._bigTexts;
		_shopGoods = data._shopGoods;
		_skills = data._skills;
		_slots = data._slots;
		_spells = data._spells;
		_subtitles = data._subtitles;
		_hints = data._hints;
		LoadingTips = data.LoadingTips;
		_loadedFilesInfo = data._loadedFilesInfo;
		_fatalities = data._fatalities;
		_npcs = data._npcs;
		_storylineDialogs = data._storylineDialogs;
		Chests = data.Chests;
		Mines = data.Mines;
		_itemRelations = data._itemRelations;
		GameSettings = data.GameSettings;
		BattleMathParams = data.BattleMathParams;
	}

	private static void TryLoadRemoteData(MonoBehaviour component, ServerData baseServerData, string localization, ActionD<ServerData> onLoad, ActionD<string> onError)
	{
		if (Globals.UseJsonAdmin)
		{
			TryLoadRemoteDataJson(component, baseServerData, localization, onLoad, onError);
		}
		else
		{
			TryLoadRemoteDataProto(component, baseServerData, localization, onLoad, onError);
		}
	}

	private static void TryLoadRemoteDataJson(MonoBehaviour component, ServerData baseServerData, string localization, ActionD<ServerData> onLoad, ActionD<string> onError)
	{
		ServerData remoteServerData = new ServerData();
		remoteServerData.InitConfigsFromRemoteData(baseServerData, component, localization, delegate
		{
			Utils.LogForce("LOAD REMOTE FILES SUCCESSED", remoteServerData.LoadState);
			onLoad(remoteServerData);
		}, onError);
	}

	private static void TryLoadRemoteDataProto(MonoBehaviour component, ServerData baseServerData, string localization, ActionD<ServerData> onLoad, ActionD<string> onError)
	{
		string path = RemoteBasePath + "shr/u/data_" + localization + ".dat";
		_inInitConfigsFromRemoteData = true;
		component.StartCoroutine(Utils.WWWLoad(path, Timeout, delegate(string _, byte[] wwwBytes)
		{
			ServerData serverData = CreateServerData(wwwBytes);
			if (serverData == null)
			{
				_inInitConfigsFromRemoteData = false;
				onError("CreateServerData(wwwBytes) failed " + path);
			}
			else
			{
				Utils.LogForce("LOAD REMOTE FILE", path);
				SaveLoadedFiles("data_" + localization + ".dat", localization, wwwBytes);
				onLoad(CreateServerData(wwwBytes));
			}
		}, delegate(string _, string __)
		{
			_inInitConfigsFromRemoteData = false;
			onError(__);
		}));
	}

	private static ServerData CreateServerData(byte[] bytes)
	{
		if (!(Globals.MainMenu.GetComponent<SaveLoadProtobuf>() is ISaveLoadData<ServerDataData> saveLoadData))
		{
			return null;
		}
		ServerDataData data = saveLoadData.LoadData(bytes);
		return ServerDataData.CreateServerData(data);
	}

	private void LoadDataLoc(MonoBehaviour component, string localization)
	{
		Utils.LogForce("LOAD SERVER DATA2", localization);
		InitConfigsFromDeviceData(component, localization, delegate(ServerData deviceServerData)
		{
			Utils.LogForce("LOAD DEVICE CONFIG SUCCESS", localization);
			SetupData(deviceServerData);
			Loaded();
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(0.001f, delegate
			{
				TryLoadRemoteData(component);
			});
		}, delegate(string _)
		{
			Utils.LogForce("LOAD DEVICE CONFIG FAILED:", _);
			TryLoadRemoteData(component, null, localization, delegate(ServerData remoteServerData)
			{
				Utils.LogForce("LOAD REMOTE CONFIG SUCCESS", localization);
				SetupData(remoteServerData);
				Loaded();
			}, delegate(string remoteError)
			{
				Utils.LogForce("LOAD REMOTE CONFIG FAILED:", remoteError);
				Loaded();
			});
		});
	}

	public void LoadFromImage(MonoBehaviour component, ActionD onLoaded)
	{
		LoadFromImage(component, UnityApi.GetLanguage(), onLoaded);
	}

	private void LoadFromImage(MonoBehaviour component, string localization, ActionD onLoaded)
	{
		InitConfigsFromImageData(component, localization, delegate(ServerData serverData)
		{
			Utils.LogForce("LOAD IMAGE CONFIG SUCCESS", localization);
			SetupData(serverData);
			onLoaded();
		}, delegate(string imageError)
		{
			Invs.Inv(false, "LOAD IMAGE CONFIG FAILED:", imageError);
		});
	}

	private bool FindChanges(List<FileInfo> my, List<FileInfo> compareWith)
	{
		Utils.Log("FINDCHANGES");
		bool result = false;
		FileInfo c;
		foreach (FileInfo item in my)
		{
			c = item;
			int num = compareWith.FindIndex((FileInfo _) => c.Name == _.Name && c.Time != _.Time);
			if (num >= 0)
			{
				Utils.LogForce("CHANGED TIME", c.Name, c.Time, compareWith[num].Time);
				c.Changed = true;
				result = true;
			}
			else if (compareWith.FindIndex((FileInfo _) => c.Name == _.Name) < 0)
			{
				Utils.LogForce("DONT EXIST", c.Name);
				c.Changed = true;
				result = true;
			}
		}
		return result;
	}

	public void TryLoadRemoteData(MonoBehaviour component)
	{
		if (_inTryLoadRemoteData)
		{
			return;
		}
		_inTryLoadRemoteData = true;
		TryLoadRemoteData(component, this, UnityApi.GetLanguage(), delegate(ServerData remoteServerData)
		{
			Utils.LogForce("LOAD REMOTE CONFIG SUCCESS", remoteServerData.LoadState, remoteServerData.LoadState);
			if (remoteServerData.LoadState == StateE.Loaded)
			{
				if (Globals.BuildType == Globals.BuildTypeE.InnerRelease)
				{
					MainMenu mainMenu = Globals.MainMenu;
					if (mainMenu == null || (mainMenu != null && mainMenu._hud == null) || (mainMenu != null && mainMenu._hud != null && mainMenu._hud.CurrentGui.Type == GuiRoot.GuiType.StartMenu))
					{
						SetupData(remoteServerData);
					}
				}
				else
				{
					SetupData(remoteServerData);
				}
			}
			_inTryLoadRemoteData = false;
		}, delegate(string remoteError)
		{
			Utils.Log("LOAD REMOTE CONFIG FAILED:", remoteError);
			_inTryLoadRemoteData = false;
		});
	}

	public static void LoadRemoteData(MonoBehaviour component, string language, ActionD<ServerData> onSuccess, ActionD<string> onError)
	{
		ServerData sd = new ServerData();
		sd._saveLoadedRemoteFiles = false;
		sd.InitConfigsFromRemoteData(null, component, language, delegate
		{
			onSuccess(sd);
		}, onError);
	}

	private void InitConfigsFromRemoteData(ServerData baseServerData, MonoBehaviour component, string localization, ActionD onSuccess, ActionD<string> onError)
	{
		string mainConfigFileName = "config_" + localization;
		_inInitConfigsFromRemoteData = true;
		string text = ((!Globals.UseEncryptedJsonAdmin) ? "shr/j/" : "shr/je/");
		string text2 = RemoteBasePath + text + mainConfigFileName + ".js";
		Utils.LogForce("InitConfigsFromRemoteData", text2);
		try
		{
			component.StartCoroutine(Utils.WWWLoad(text2, Timeout, delegate(string _, string wwwText)
			{
				_loadedFilesInfo = GetFilesList(wwwText);
				Utils.LogForce(localization, Utils.ParamsToString(_loadedFilesInfo.ToArray()));
				if (baseServerData == null)
				{
					foreach (FileInfo item in _loadedFilesInfo)
					{
						item.Changed = true;
					}
				}
				else if (baseServerData != null && !FindChanges(_loadedFilesInfo, baseServerData._loadedFilesInfo))
				{
					Utils.LogForce("configs equal");
					LoadFinished(StateE.LoadedEqual);
					onSuccess();
					_inInitConfigsFromRemoteData = false;
					return;
				}
				FuncD<string, bool> isLocalFile = delegate(string lfn)
				{
					int index = _loadedFilesInfo.FindIndex((FileInfo _f) => _f.Name == lfn);
					return !_loadedFilesInfo[index].Changed;
				};
				ParseMainFile(component, storeFile: true, _loadedFilesInfo, (string _x, string x) => (!isLocalFile(_x)) ? (RemoteBasePath + x) : x, delegate(MonoBehaviour component2, string path2, ActionD<byte[]> onLoad2, ActionD<string> onError2)
				{
					if (path2.StartsWith(RemoteBasePath))
					{
						Utils.LogForce("LOAD REMOTE FILE", path2, path2.StartsWith(RemoteBasePath));
						LoadWWWFile(component2, path2, onLoad2, onError2);
					}
					else
					{
						LoadSystemIOFile(component2, LocalConfigFilePath(localization, Path.GetFileNameWithoutExtension(path2)), onLoad2, onError2);
					}
				}, delegate
				{
					if (LoadState == StateE.Loaded)
					{
						Debug.Log("LLLLLL " + _saveLoadedRemoteFiles);
						if (_saveLoadedRemoteFiles)
						{
							SaveLoadedFiles(wwwText, mainConfigFileName, localization, _loadedFilesInfo);
						}
						Utils.LogForce("SAVE REMOTE FILES");
						_inInitConfigsFromRemoteData = false;
						onSuccess();
					}
					else
					{
						_inInitConfigsFromRemoteData = false;
						onError("FAILED " + LoadState);
					}
				});
			}, delegate(string _, string __)
			{
				_inInitConfigsFromRemoteData = false;
				onError(__);
			}));
		}
		catch (Exception ex)
		{
			Utils.LogForce("InitConfigsFromRemoteData exception", ex.Message);
			onError(ex.Message);
		}
	}

	private void SaveConfigFile(string path, string text)
	{
		Utils.LogForce("SaveConfigFile", path);
		Utils.WriteAllText(path, text);
	}

	private void SaveConfigFile(string path, byte[] bytes)
	{
		Utils.LogForce("SaveConfigFile", path);
		Utils.WriteAllBytes(path, bytes);
	}

	private static void SaveLoadedFiles(string mainConfigFileName, string localization, byte[] fileData)
	{
		string text = DeviceBasePath + mainConfigFileName;
		Utils.LogForce("SAVE CONFIGS FILES TO DEVICE", text);
		if (!Directory.Exists(DeviceBasePath))
		{
			Directory.CreateDirectory(DeviceBasePath);
		}
		string path = DeviceBasePath + localization + "/";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		Utils.WriteAllBytes(text, fileData);
	}

	private void SaveLoadedFilesOriginalBytes(string dirName, string ext, List<FileInfo> loadedFilesInfo)
	{
		if (!Directory.Exists(dirName))
		{
			Directory.CreateDirectory(dirName);
		}
		foreach (FileInfo item in loadedFilesInfo)
		{
			if (item.Changed)
			{
				SaveConfigFile(dirName + "/" + Path.GetFileNameWithoutExtension(item.Name) + ext, item.OriginalBytes);
			}
		}
	}

	private void SaveLoadedFilesOriginalText(string dirName, string ext, List<FileInfo> loadedFilesInfo)
	{
		if (!Directory.Exists(dirName))
		{
			Directory.CreateDirectory(dirName);
		}
		foreach (FileInfo item in loadedFilesInfo)
		{
			if (item.Changed)
			{
				SaveConfigFile(dirName + "/" + Path.GetFileNameWithoutExtension(item.Name) + ext, item.OriginalText);
			}
		}
	}

	private void SaveLoadedFilesOriginalText2(string dirName, string ext, List<FileInfo> loadedFilesInfo)
	{
		if (!Directory.Exists(dirName))
		{
			Directory.CreateDirectory(dirName);
		}
		foreach (FileInfo item in loadedFilesInfo)
		{
			if (item.Changed)
			{
				SaveConfigFile(dirName + "/" + Path.GetFileNameWithoutExtension(item.Name) + ext, item.OriginalText2);
			}
		}
	}

	private void SaveLoadedFiles(string configFileText, string mainConfigFileName, string localization, List<FileInfo> loadedFilesInfo)
	{
		Utils.LogForce("SAVE CONFIGS FILES TO DEVICE");
		if (!Directory.Exists(DeviceBasePath))
		{
			Directory.CreateDirectory(DeviceBasePath);
		}
		string text = DeviceBasePath + localization + "/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		foreach (FileInfo item in loadedFilesInfo)
		{
			string path = text + item.Name + ".json";
			if (item.Changed)
			{
				SaveConfigFile(path, item.OriginalBytes);
			}
			else if (!File.Exists(path))
			{
				SaveConfigFile(path, item.OriginalBytes);
			}
		}
		SaveConfigFile(DeviceBasePath + mainConfigFileName + ".json", configFileText);
	}

	private static void LoadWWWFile(MonoBehaviour component, string path, ActionD<byte[]> onLoad, ActionD<string> onError)
	{
		try
		{
			component.StartCoroutine(Utils.WWWLoad(path, Timeout, delegate(string _, byte[] wwwBytes)
			{
				onLoad(wwwBytes);
			}, delegate(string _, string __)
			{
				onError(__);
			}));
		}
		catch (Exception ex)
		{
			Utils.LogForce("LoadWWWFile exception", ex.Message);
			onError(ex.Message);
		}
	}

	private static void LoadSystemIOFile(MonoBehaviour component, string path, ActionD<byte[]> onLoad, ActionD<string> onError)
	{
		try
		{
			byte[] v = Utils.ReadAllBytes(path);
			onLoad(v);
		}
		catch (Exception ex)
		{
			onError("LoadSystemIOFile failed: " + ex.Message + " [[[ " + ex.StackTrace + "]]] ");
		}
	}

	private void LoadFinished(StateE state)
	{
		Utils.LogFrom("ServerData", "Load finished", UnityApi.GetLanguage(), state, _Bag.Count);
		LoadState = state;
		Loaded();
	}

	internal void NewGame()
	{
		BuyedMines = new List<int>();
		Globals.LastLoadedSceneServerId = -1;
		_Bag = new List<Item>();
		RefreshElixirsCount();
		MySpells = new List<int>();
		RemoveAllPlayerArmor();
		ResetLocationsProgress();
		PlayerParams.Reset(PlayerServerPersData);
		if (PlayerServerPersData.StartSpell != null)
		{
			Utils.Log("NEWGAMESPELL", PlayerServerPersData.StartSpell.Id);
			MySpells.Clear();
			MySpells.Add(PlayerServerPersData.StartSpell.Id);
		}
		foreach (KeyValuePair<int, Location> location in _locations)
		{
			if (location.Value.Logic != null)
			{
				location.Value.Logic.RecreateOpenCondition();
			}
		}
		if (Globals.MainMenu != null)
		{
			ISaveLoad<PlayerState> saveLoad = Globals.MainMenu.GetComponent<SaveLoadProtobuf>() as ISaveLoad<PlayerState>;
			Utils.Log("SAVE", saveLoad);
		}
	}

	internal float GetEnemyMagicResist(DamageTypeE damageType)
	{
		AreaData.MobData mobData = EnemyParams.MobData;
		BotInfo serverInfo = mobData.ServerInfo;
		if (!serverInfo.HasMagicImmunity(damageType.AsMagic()))
		{
			return 1f;
		}
		BotLevel botLevel = GetBotLevel(mobData);
		if (botLevel == null)
		{
			return 1f;
		}
		return GameSettings.MagicRes;
	}

	internal void LoadFrom(PlayerState state)
	{
		IsLoading = true;
		Utils.Log("****LOADFROM****", state);
		if (state == null)
		{
			MySpells = new List<int>();
			_Bag = new List<Item>();
			return;
		}
		PersData persDataByServerId = GetPersDataByServerId(state.PlayerPersDataId);
		if (persDataByServerId == null)
		{
			LoadFrom(null);
			return;
		}
		PlayerParams.LoadFrom(state.PlayerParams);
		ResetLocationsProgress();
		if (state.Locations != null)
		{
			foreach (Location location in state.Locations)
			{
				GetLocationByServerId(location.Id)?.Logic.CopyFromLoadedGame(location.Logic);
			}
			foreach (KeyValuePair<int, Location> location2 in _locations)
			{
				LocationLogic logic = location2.Value.Logic;
				if (logic.ZachistkaMobsKilled >= location2.Value.Bots.Length && !logic.IsOpened)
				{
					logic.ZachistkaMobsKilled = location2.Value.Bots.Length - 1;
				}
				foreach (BotInfo mob in logic._mobs)
				{
					if (mob.Level <= 5 || mob.MaxLevel <= 0)
					{
						LocationLogic.GenerateMobLevel(mob);
					}
				}
			}
		}
		List<int> list = new List<int>();
		if (state.Spells != null)
		{
			foreach (int spell in state.Spells)
			{
				if (_spells.ContainsKey(spell))
				{
					list.Add(spell);
				}
			}
		}
		MySpells = list;
		_playerServerPersData = persDataByServerId;
		_Bag = ((state.Inventory != null) ? new List<Item>(state.Inventory) : new List<Item>());
		_Bag = new List<Item>();
		if (state.Inventory != null)
		{
			Item[] inventory = state.Inventory;
			foreach (Item item in inventory)
			{
				Item value = null;
				if (_items.TryGetValue(item.Id, out value) && value != null)
				{
					Item item2 = new Item(item, value);
					AddToBag(item2);
				}
				else
				{
					Utils.Log("**LOAD ITEM NO", item);
				}
			}
		}
		Utils.Log("*******LOAD FINISHED***************");
		IsLoading = false;
	}

	public bool IsHelm(Item item)
	{
		Item value = null;
		if (_items.TryGetValue(item.Id, out value) && value != null)
		{
			return value.Slot != null && value.Slot.SlotId == Slot.TypeE.Helm;
		}
		return false;
	}

	public string GetItemImageName(Item item)
	{
		PersData playerServerPersData = PlayerServerPersData;
		string text = item.Class.Int2();
		if (item.IsArmor)
		{
			text = playerServerPersData.Class.Int2();
		}
		string text2 = ((!item.IsWeapon) ? item.Set.Int2() : "00");
		string text3 = ((!item.IsArmorOrWeapon) ? item.GetSuffix(0) : item.GetSuffix(playerServerPersData.Class));
		return text2 + text + ((int)item.Slot.SlotId).Int2() + text3;
	}

	private void ZachistkaDone(Location location, Location opened, Location openAfter)
	{
		if (openAfter != null)
		{
			openAfter = GetLocationByServerId(openAfter.Id);
			if (openAfter != null && openAfter.Id == location.Id && opened.Logic.OpenCondition != null)
			{
				opened.Logic.OpenCondition.CheckProgress = true;
				Utils.Log("******CHECKPROGRESS", opened, opened.Logic.OpenCondition);
			}
		}
	}

	internal void ZachistkaDone(Location location)
	{
		foreach (KeyValuePair<int, Location> location2 in _locations)
		{
			ZachistkaDone(location, location2.Value, location2.Value.OpenAfter);
			ZachistkaDone(location, location2.Value, location2.Value.OpenAfterAlt);
		}
	}

	internal void UpdateLiveShamansCount()
	{
		Messenger.Invoke(Globals.MsgShamansCountChanged, LiveShamansCount);
	}

	internal void GiveMoney(MoneyType.TypeE type, string amountText)
	{
		int result = 0;
		int result2 = -1;
		string[] array = amountText.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1 && PlayerParams != null && int.TryParse(array[0], out result) && result > 0)
		{
			PlayerParams.AddMoney(type, result);
			Globals.MainMenu.SaveGame();
		}
		else
		{
			if (array.Length != 2 || !int.TryParse(array[0], out result) || result <= 0 || !int.TryParse(array[1], out result2) || result2 < 1 || result2 > 4)
			{
				return;
			}
			result2--;
			if (Globals.MainMenu != null && Globals.MainMenu.SaveSlotIndex == result2 && PlayerParams != null)
			{
				PlayerParams.AddMoney(type, result);
				Globals.MainMenu.SaveGame();
				return;
			}
			PlayerState playerState = Globals.MainMenu.TryLoadSave(result2);
			if (playerState != null && result > 0)
			{
				switch (type)
				{
				case MoneyType.TypeE.Gold:
					playerState.PlayerParams.MoneyGoldCount += result;
					break;
				case MoneyType.TypeE.Diamond:
					playerState.PlayerParams.MoneyDiamondCount += result;
					break;
				}
				if (Globals.MainMenu.GetComponent<SaveLoadProtobuf>() is ISaveLoad<PlayerState> saveLoad)
				{
					saveLoad.Save(result2, playerState);
				}
			}
		}
	}

	internal void BankBuySuccess(string id)
	{
		foreach (BankItem bankItem in _bankItems)
		{
			if (bankItem.PurchaseId == id)
			{
				BankBuy(bankItem);
				break;
			}
		}
	}

	internal void BankBuy(BankItem bankItem)
	{
		Utils.Log("    BankBuy done", bankItem.Id, bankItem.PurchaseId);
		if (bankItem != null)
		{
			if (bankItem.CountType != null)
			{
				InBunkBuyedSuccessed(bankItem.CountType.Type, bankItem.Count + bankItem.Bonus);
			}
			Metrics.OnBuyInBunk(bankItem);
		}
	}

	internal int GameProgress()
	{
		if (_locations == null)
		{
			return 0;
		}
		float num = 0f;
		float num2 = 0f;
		foreach (Location value in _locations.Values)
		{
			num2 += (float)value.Bots.Length;
			num += (float)Mathf.Min(value.Logic.ZachistkaMobsKilled, value.Bots.Length);
		}
		float num3 = num * 100f / num2;
		if (num3 < 0f)
		{
			num3 = 0f;
		}
		if (num3 > 100f)
		{
			num3 = 100f;
		}
		return num3.RoundToInt();
	}

	internal int GetAllExp()
	{
		if (PlayerParams == null)
		{
			return 0;
		}
		int num = PlayerParams.Experience;
		for (int num2 = PlayerParams.Level; num2 > 1; num2--)
		{
			num += _levels[num2].Exp;
		}
		return num;
	}

	internal void InBunkBuyedSuccessed(MoneyType.TypeE moneyType, int count)
	{
		Utils.Log("InBunkBuyedSuccessed", moneyType, count);
		if (moneyType == MoneyType.TypeE.Diamond && count > 0)
		{
			PlayerParams.MoneyDiamondCount += count;
		}
		if (moneyType == MoneyType.TypeE.Gold && count > 0)
		{
			PlayerParams.MoneyGoldCount += count;
		}
	}

	internal Bonus GetWinBonus(Enemy enemy, AreaData.MobData mob)
	{
		int num = GameSettings.Elfs.IndexOf((Location.BotLocationInfo _) => _.Bot.Id == mob.ServerInfo.Id);
		BotLevel botLevel = SingletonT<ServerData>.I.GetBotLevel(mob);
		if (num < 0)
		{
			if (enemy.FromLocation)
			{
				return GameSettings.MonsterFromLocationBonus;
			}
			return botLevel?.WinBonus;
		}
		return (GameSettings.ElfBonus != null) ? GameSettings.ElfBonus : botLevel?.WinBonus;
	}
}
