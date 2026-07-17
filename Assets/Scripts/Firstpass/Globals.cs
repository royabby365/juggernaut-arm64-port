using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
	public enum DebugRenderers
	{
		DoNothing,
		EnableDisable
	}

	internal enum BuildTypeE
	{
		InnerRelease,
		Developers,
		Inner,
		ShowContent
	}

	internal enum GameScreenE
	{
		Map,
		Shop,
		Bag,
		Battle,
		Location,
		StartMenu,
		SelectPlayer,
		BookOfMagic,
		ChestGame
	}

	public const string AD_PREF_KEY = "EVO_AD_COUNTER";

	public static bool MyDebug = false;

	public static bool IsDebugInput = MyDebug;

	public static readonly string MainServerUrl = "http://mobile.ext.terrhq.ru/";

	public static int AD_COUNTER_LIMINT = 10;

	internal static readonly float CONTENT_PACK_VERSION = UnityApi.GetGameVersion();

	internal static readonly BuildTypeE BuildType = BuildTypeE.InnerRelease;

	public static DebugRenderers DebugRenderersStage = DebugRenderers.EnableDisable;

	public static bool IsDebugBuild = MyDebug;

	public static bool ForceFPS = false;

	public static bool UseJsonAdmin = true;

	public static bool UseEncryptedJsonAdmin = true;

	public static byte[] EncryptionKeyBytes = new byte[32]
	{
		101, 161, 73, 53, 117, 41, 50, 112, 104, 25,
		59, 146, 88, 34, 203, 213, 147, 131, 208, 186,
		214, 226, 255, 71, 8, 12, 168, 67, 13, 238,
		223, 66
	};

	public static string OldOpenFeintAppId = "56305";

	public static string NewOpenFeintAppId = "58337";

	private static Dictionary<string, string> _OldOpenFeintAchievmentsMap;

	private static Dictionary<string, string> _NewOpenFeintAchievmentsMap;

	public static bool IsDebugGC = false;

	public static bool IsDebugIOSMemory = false;

	public static bool IsDebugChangeGuiTo = false;

	public static readonly string GoldBigIcon = "000016006";

	public static readonly string DiamondBigIcon = "000016007";

	public static readonly string KeyBigIcon = "000016002";

	public static readonly string SkullBigIcon = "000016004";

	public static readonly string ScarabBigIcon = "000016005";

	public static readonly string HealBigIcon = "000015001";

	public static readonly string CritBigIcon = "000016001";

	public static readonly string PoisonBigIcon = "000017001";

	public static readonly string CharIconPoison = '\u000e'.ToString();

	public static readonly string CharIconDiamonds = '\u000f'.ToString();

	public static readonly string CharIconExp = '\u0010'.ToString();

	public static readonly string CharIconRage = '\u0011'.ToString();

	public static readonly string CharIconRageBonus = '\u0012'.ToString();

	public static readonly string CharIconGold = '\u0013'.ToString();

	public static readonly string CharIconGoldBonus = '\u0014'.ToString();

	public static readonly string CharIconDark = '\u0015'.ToString();

	public static readonly string CharIconElectro = '\u0016'.ToString();

	public static readonly string CharIconFire = '\u0017'.ToString();

	public static readonly string CharIconIce = '\u0018'.ToString();

	public static readonly string CharIconMagic = '\u0019'.ToString();

	public static readonly string CharIconManaBonus = '\u001a'.ToString();

	public static readonly string CharIconStrength = '\u001b'.ToString();

	public static readonly string CharIconVitality = '\u001c'.ToString();

	public static readonly string CharIconFullMana = '\u001d'.ToString();

	public static readonly string CharIconFullRage = '\u001e'.ToString();

	public static readonly string CharIconKey = '\u001f'.ToString();

	public static readonly string TutorialFindChest = "done=tut_find_chest";

	public static readonly string MsgFatalityModeSlicesChanged = "MsgFatalityModeSlicesChanged";

	public static readonly string MsgGuiBattle_ShowFatalityBar = "MsgGuiBattle_ShowFatalityBar";

	public static readonly string MsgFatalityStarted = "MsgFatalityStarted";

	public static readonly string MsgFatalityExecuted = "MsgFatalityExecuted";

	public static readonly string MsgFatalitiesSpheresCountChanged = "MsgFatalitiesSpheresCountChanged";

	public static readonly string MsgPersonManaChanged = "MsgPersonManaChanged";

	public static readonly string MsgPlayerAttackFinished = "MsgPlayerAttackFinished";

	public static readonly string MsgPlayerAngerChanged = "MsgPlayerAngerChanged";

	public static readonly string MsgPlayerLevelChanged = "MsgPlayerLevelChanged";

	public static readonly string MsgPlayerFundsChanged = "MsgPlayerFundsChanged";

	public static readonly string MsgPlayerExpChanged = "MsgPlayerExpChanged";

	public static readonly string MsgPlayerHealthChanged = "MsgPlayerHealthChanged";

	public static readonly string MsgPlayerSkillChanged = "MsgPlayerSkillChanged";

	public static readonly string MsgPlayerWeaponChanged = "MsgPlayerWeaponChanged";

	public static readonly string MsgPlayerSkillPointsChanged = "MsgPlayerSkillPointsChanged";

	public static readonly string MsgPlayerAttackSpawnOneMagicBall = "MsgPlayerAttackSpawnOneMagicBall";

	public static readonly string MsgPlayerSpellBuyed = "MsgPlayerSpellBuyed";

	public static readonly string MsgPlayerCastSpell = "MsgPlayerCastSpell";

	public static readonly string MsgPlayerSpellReact = "MsgPlayerSpellReact";

	public static readonly string MsgPlayerUseRage = "MsgPlayerUseRage";

	public static readonly string MsgPlayerUsePoison = "MsgPlayerUsePoison";

	public static readonly string MsgPlayerDefeated = "MsgPlayerDefeated";

	public static readonly string MsgPlayerReact = "MsgPlayerReact";

	public static readonly string MsgPlayerAttack = "MsgPlayerAttack";

	public static readonly string MsgPlayerCombo = "MsgPlayerCombo";

	public static readonly string MsgPlayerItemsChanged = "MsgPlayerItemsChanged";

	public static readonly string MsgEnemyHealthChanged = "MsgEnemyHealthChanged";

	public static readonly string MsgEnemyImmunityShown = "MsgEnemyImmunityShown";

	public static readonly string MsgPersonReact = "MsgPersonReact";

	public static readonly string MsgPersonDie = "MsgPersonDie";

	public static readonly string MsgPersonDamaged = "MsgPersonDamaged";

	public static readonly string MsgPersonAttackFinished = "MsgPersonAttackFinished";

	public static readonly string MsgElixirCooldownChanged = "MsgElixirCooldownChanged";

	public static readonly string MsgElixirApplyPoisonOnEnemy = "MsgElixirApplyPoisonOnEnemy";

	public static readonly string MsgElixirCountChanged = "MsgElixirCountChanged";

	public static readonly string MsgResurrectionCountChanged = "MsgResurrectionCountChanged";

	public static readonly string MsgRageSpheresCountChanged = "MsgRageSpheresCountChanged";

	public static readonly string MsgRageSphereUse = "MsgRageSphereUse";

	public static readonly string MsgResurrectionSpawned = "MsgResurrectionSpawned";

	public static readonly string MsgUseElixir = "MsgUseElixir";

	public static readonly string MsgZachistkaProgressChanged = "MsgZachistkaProgressChanged";

	public static readonly string MsgZachistkaDone = "MsgZachistkaDone";

	public static readonly string MsgLocationMobsAdded = "MsgLocationMobsAdded";

	public static readonly string MsgLocationMobsRemoved = "MsgLocationMobsRemoved";

	public static readonly string MsgLocationMoneyChanged = "MsgLocationMoneyChanged";

	public static readonly string MsgLocationPopulationChanged = "MsgLocationPopulationChanged";

	public static readonly string Msg_ChestOnLocationAdded = "Msg_ChestOnLocationAdded";

	public static readonly string Msg_ChestOnLocationRemoved = "Msg_ChestOnLocationRemoved";

	public static readonly string Msg_ChestOnLocationWasFound = "Msg_ChestOnLocationWasFound";

	public static readonly string Msg_ChestOpened = "Msg_ChestOpened";

	public static readonly string Msg_GotoChestGameFromLocation = "Msg_GotoChestGameFromLocation";

	public static readonly string MsgAllChestsUnlocked = "MsgAllChestsUnlocked";

	public static readonly string Msg_LocationOpenConditionDone = "Msg_LocationOpenConditionDone";

	public static readonly string Msg_LocationOpenConditionProgressChanged = "Msg_LocationOpenConditionProgressChanged";

	public static readonly string Msg_LocationChestsWasFoundChanged = "Msg_LocationChestsWasFoundChanged";

	public static readonly string MsgShowElf = "MsgShowElf";

	public static readonly string MsgAttackElf = "MsgAttackElf";

	public static readonly string MsgFightResult = "MsgFightResult";

	public static readonly string MsgFightBreak = "MsgFightBreak";

	public static readonly string MsgFightPause = "MsgFightPause";

	public static readonly string MsgFightStarted = "MsgFightStarted";

	public static readonly string MsgBattleStateChanged = "MsgBattleStateChanged";

	public static readonly string MsgFightResult_Continue = "MsgFightResult_Continue";

	public static readonly string MsgFightResult_GoToBag = "MsgFightResult_GoToBag";

	public static readonly string MsgFightShowEndDialog = "MsgFightShowEndDialog";

	public static readonly string MsgBattleReactFx = "MsgBattleReactFx";

	public static readonly string MsgSpawnBubblesFromPlayer = "MsgSpawnBubblesFromPlayer";

	public static readonly string MsgSpawnManaBubbleFromEnemy = "MsgSpawnManaBubbleFromEnemy";

	public static readonly string MsgSpawnRageBubbleFromEnemy = "MsgSpawnRageBubbleFromEnemy";

	public static readonly string MsgEnemyKilledByPoison = "MsgEnemyKilledByPoison";

	public static readonly string MsgPlayerKilledByMagic = "MsgPlayerKilledByMagic";

	public static readonly string MsgBubblesDestroySelf = "MsgBubblesDestroySelf";

	public static readonly string MsgMagicModeDisabled = "MsgMagicModeDisabled";

	public static readonly string Msg_MagicGame_Show = "Msg_MagicGame_Show";

	public static readonly string Msg_MagicGame_Finished = "Msg_MagicGame_Finished";

	public static readonly string Msg_MagicGame2_Show = "Msg_MagicGame2_Show";

	public static readonly string MsgFightViewSectorClicked = "MsgFightViewSectorClicked";

	public static readonly string MsgEnemyStrongMagic = "MsgEnemyStrongMagic";

	public static readonly string MsgEnemyWeakMagic = "MsgEnemyWeakMagic";

	public static readonly string MsgFightEyeShown = "MsgFightEyeShown";

	public static readonly string MsgFightEyeDie = "MsgFightEyeDie";

	public static readonly string MsgFightEyeDieByClick = "MsgFightEyeDieByClick";

	public static readonly string MsgFightManaBallClicked = "MsgFightManaBallClicked";

	public static readonly string MsgFightRageBallClicked = "MsgFightRageBallClicked";

	public static readonly string MsgEyeEatRage = "MsgEyeEatRage";

	public static readonly string MsgFallingStarClicked = "MsgFallingStarClicked";

	public static readonly string MsgFallingStarSpawn = "MsgFallingStarSpawn";

	public static readonly string MsgShopResetTimeChanged = "MsgShopResetTimeChanged";

	public static readonly string MsgShopGoodsReseted = "MsgShopGoodsReseted";

	public static readonly string MsgShopGoodsNeedStateRefresh = "MsgShopGoodsNeedStateRefresh";

	public static readonly string MsgPlayerCompareItem = "MsgPlayerCompareItem";

	public static readonly string MsgPlayerRemoveItem = "MsgPlayerRemoveItem";

	public static readonly string MsgPlayerCompareShopGood = "MsgPlayerCompareShopGood";

	public static readonly string MsgShopFilterChanged = "MsgShopFilterChanged";

	public static readonly string MsgShopRecommendationsChanged = "MsgShopRecomendationsChanged";

	public static readonly string MsgShopScarabInsufficient = "MsgShopScarabInsufficient";

	public static readonly string MsgShopSkullInsufficient = "MsgShopSkullInsufficient";

	public static readonly string MsgShopStarsInsufficient = "MsgShopStarsInsufficient";

	public static readonly string MsgShopEpicsInsufficient = "MsgShopEpicsInsufficient";

	public static readonly string MsgShopChangeFilter = "MsgShopChangeFilter";

	public static readonly string MsgShopChangePointer = "MsgShopChangePointer";

	public static readonly string MsgGoToMainMenu = "MsgGoToMainMenu";

	public static readonly string MsgInternalGameSaveLoaded = "MsgInternalGameSaveLoaded";

	public static readonly string MsgGameScreenChanged = "MsgGameScreenChanged";

	public static readonly string Msg_StartIntro_Finished = "Msg_StartIntro_Finished";

	public static readonly string MsgGameEventProgressChanged = "MsgGameEventProgressChanged";

	public static readonly string MsgAppsfireNotification = "MsgAppsfireNotification";

	public static readonly string MsgSelectScreenDataLoaded = "MsgSelectScreenDataLoaded";

	public static readonly string MsgSelectScreenFinished = "MsgSelectScreenFinished";

	public static readonly string MsgLoadingScreenHided = "MsgLoadingScreenHided";

	public static readonly string Msg_ExitFromLocation = "Msg_ExitFromLocation";

	public static readonly string MsgGuiButtonPressed = "MsgGuiButtonPressed";

	public static readonly string MsgGuiSwitchToBefore = "MsgGuiSwitchToBefore";

	public static readonly string MsgGuiSwitchToPre = "MsgGuiSwitchToPre";

	public static readonly string MsgGuiSwitchToPost = "MsgGuiSwitchToPost";

	public static string MsgGuiBattle_HideYouLose = "MsgGuiBattle_HideYouLose";

	public static string MsgGuiBattle_HideYouWin = "MsgGuiBattle_HideYouWin";

	public static string MsgGuiBattle_PlayerLevel = "MsgGuiBattle_PlayerLevel";

	public static string MsgGuiBattle_EnemyLevel = "MsgGuiBattle_EnemyLevel";

	public static string MsgGuiBattle_EnemyAvatar = "MsgGuiBattle_EnemyAvatar";

	public static string MsgGuiBattle_PlayerAvatar = "MsgGuiBattle_PlayerAvatar";

	public static string MsgGuiBattle_ShowItemsBar = "MsgGuiBattle_ShowItemsBar";

	public static string MsgGuiBattle_ShowYouWin = "MsgGuiBattle_ShowYouWin";

	public static string MsgGuiBattle_ShowYouLose = "MsgGuiBattle_ShowYouLose";

	public static string MsgGuiBattle_ShowWeakMagicProtectionBar = "MsgGuiBattle_ShowWeakMagicProtectionBar";

	public static string MsgGuiBattle_SetMagicBarVisible = "MsgGuiBattle_SetMagicBarVisible";

	public static string MsgGuiBattle_SetFadePlate = "MsgGuiBattle_SetFadePlate";

	public static string MsgGuiBattle_ShowMagicSuccess = "MsgGuiBattle_ShowMagicSuccess";

	public static string MsgGuiBattle_ShowText = "MsgGuiBattle_ShowText";

	public static string MsgGuiBattle_ShowPhrase = "MsgGuiBattle_ShowPharse";

	public static string MsgGuiBattle_FlashPhrase = "MsgGuiBattle_FlashPhrase";

	public static string MsgGuiBattle_HidePhrase = "MsgGuiBattle_HidePhrase";

	public static string MsgGuiBattle_HideText = "MsgGuiBattle_HideText";

	public static string MsgGuiBattle_HideStrongMagic = "MsgGuiBattle_HideStrongMagic";

	public static string MsgGuiBattle_NextCombo = "MsgGuiBattle_NextCombo";

	public static string MsgGuiBattle_ComboAllowed = "MsgGuiBattle_ComboAllowed";

	public static string MsgGuiBattle_MagicCasts = "MsgGuiBattle_MagicCasts";

	public static string MsgGuiBattle_Timer = "MsgGuiBattle_Timer";

	public static string MsgGuiBattle_CastGesture = "MsgGuiBattle_CastGesture";

	public static string MsgGuiExitExtraChapter = "MsgGuiExitExtraChapter";

	public static string MsgGuiExitAchievments = "MsgGuiExitAchievments";

	public static string MsgGuiExitSkill = "MsgGuiExitSkill";

	public static string MsgGestureStarted = "MsgGestureStarted";

	public static string MsgCompareDropElement = "MsgCompareDropElement";

	public static string MsgCompareExtraDropElement = "MsgCompareExtraDropElement";

	public static string MsgTutorialFullScreenInfoHided = "MsgTutorialFullScreenInfoHided";

	public static string MsgScreenshotAlertShowing = "MsgScreenshotAlertShowing";

	public static string MsgMatch3ChainDestroyed = "MsgMatch3ChainDestroyed";

	public static string MsgMatch3NewRecord = "MsgMatch3NewRecord";

	public static readonly string MsgBagNeedRefresh = "MsgBagNeedRefresh";

	public static readonly string MsgBagRefreshFinished = "MsgBagRefreshFinished";

	public static readonly string MsgSpellUsedCountChanged = "MsgSpellUsedCountChanged";

	public static readonly string MsgPlayerBagChanged = "MsgPlayerBagChanged";

	public static readonly string MsgPlayerBagRearrange = "MsgBagRearrange";

	public static readonly string MsgBagItemAdded = "MsgBagItemAdded";

	public static readonly string MsgStarIncreased = "MsgStarIncreased";

	public static readonly string MsgMagicBookNeedMoreUsings = "MsgMagicBookNeedMoreUsings";

	public static readonly string MsgMagicBookNeedMoreSkulls = "MsgMagicBookNeedMoreSkulls";

	public static readonly string MsgInsufficientFunds = "MsgInsufficientFunds";

	public static readonly string MsgNewPersInited = "MsgNewPersInited";

	public static readonly string MsgNewSaveSlotIndex = "MsgNewSaveSlotIndex";

	public static readonly string MsgNewGame = "MsgNewGame";

	public static readonly string MsgLoadThenContinueGame = "MsgLoadThenContinueGame";

	public static string MsgShowStorylineDialog = "MsgShowStorylineDialog";

	public static string MsgTutorialInfo = "MsgTutorialInfo";

	public static string MsgLocationMoneyPilesAdded = "MsgLocationMoneyPilesAdded";

	public static string MsgLocationMoneyPileClicked = "MsgLocationMoneyPileClicked";

	public static string MsgLocationMobAttack = "MsgLocationMobAttack";

	public static string MsgItemPuton = "MsgItemPuton";

	public static string MsgPopup2ButtonYesHandler = "MsgPopup2ButtonYesHandler";

	public static string MsgPopup2ButtonYesHandlerCustomMessage = "MsgPopup2ButtonYesHandlerCustomMessage";

	public static string MsgShowAlert = "MsgShowAlert";

	public static string MsgShowAlertWithCallback = "MsgShowAlertWithCallback";

	public static string MsgShowHint = "MsgShowHint";

	public static string MsgObtainInMine = "MsgObtainInMine";

	public static string MsgMineGameStats = "MsgMineGameStats";

	public static string ChapterPrizeHandler = "ChapterPrizeHandler";

	public static string MsgSoundsVolumeChanged = "MsgSoundsVolumeChanged";

	public static string MsgItemBuyInShop = "MsgItemBuyInShop";

	public static string MsgItemFoundInDrop = "MsgItemFoundInDrop";

	public static string MsgDropChestSelected = "MsgDropChestSelected";

	public static string MsgItemUpgrade = "MsgItemUpgraded";

	public static string MsgOpenRatings = "MsgOpenRatings";

	public static string MsgAddFriends = "MsgAddFriends";

	public static string MsgRatingSocialMessageCulldown = "MsgRatingSocialMessageCulldown";

	public static string MsgRatingSocialMessageClick = "MsgRatingSocialMessageClick";

	public static string MsgTwitterNoInternet = "MsgTwitterNoInternet";

	public static string MsgTwitterPostSucceded = "MsgTwitterPostSucceded";

	public static string MsgTwitterCanceled = "MsgTwitterCanceled";

	public static string MsgFacebookPostSucceded = "MsgFacebookPostSucceded";

	public static string MsgFacebookNotPosted = "MsgFacebookNotPosted";

	public static string MsgBankBuyDoneSuccessful = "MsgBankBuyDoneSuccessful";

	public static string MsgBankBuyDoneFail = "MsgBankBuyDoneFail";

	public static string MsgShamansCountChanged = "MsgShamansCountChanged";

	public static readonly string LocationGameObjectMainMenu = "__main_menu";

	public static readonly string LocationGameObjectLocationName = "__location";

	public static readonly string LocationGameObjectStartMenuName = "__start_menu";

	public static readonly string LocationGameObjectSceneGeomName = "__scene_object";

	public static readonly string LocationGameObjectChoosePlayerMenuName = "choose_char_hud";

	public static readonly string LocationGameObjectStartIntroName = "__start_intro";

	public static readonly string LocationGameObjectChestGameName = "__chest_game";

	public static readonly string LocationGameObjectBattleCamera = "camera";

	internal static readonly string PlayerName = "__player";

	public static readonly string AppDirName = "jug_sovering";

	internal static bool DebugPlayerOneHitKill = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugPlayerLargeDamage = ((BuildType != BuildTypeE.InnerRelease) ? MyDebug : MyDebug);

	internal static readonly bool DebugPlayerAttackAlwaysCrit = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugPlayerAttackNoCrit = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugPlayerAlwaysBlock = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugPlayerAlwaysDodge = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugNoDamageOnPlayer = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugEnemyNoMagic = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugEnemyAlwaysWeakMagic = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugEnemyAlwaysStrongMagic = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugEnemyAlwaysCrit = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugEnemyAlwaysBlock = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugEnemyDoAlwayStep = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugEnemyOneHitKill = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugEnemyLargeDamage = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugNoDamageOnEnemy = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugSmallDamageOnEnemy = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugFullMana = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugFatality = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugFatalitySphereAlways = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugFatalityNoDark = BuildType != BuildTypeE.InnerRelease || true;

	internal static readonly bool DebugFastCombo = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugPoisonKills = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugPoisonLargeDamage = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugAlwaysRageBubble = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugDontDecRageBubble = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool ShowEyeInBattle = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugLocationsFastChest = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugLocationsFastMobs = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugShortPeriods = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugFrequentFallingStars = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugDontAddLevel = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugAlwaysAddLevel = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugKeysInfinite = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugSvetlyaki = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugFxOptimizerOn = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool UseFatalityWhiteSpheresMode = BuildType != BuildTypeE.InnerRelease || true;

	internal static readonly bool LocationLogicNoOfflineTime = BuildType != BuildTypeE.InnerRelease || true;

	internal static readonly bool CacheFxs = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool IsShadowEnabled = BuildType != BuildTypeE.InnerRelease || true;

	internal static readonly bool ShowIntro = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool NewBerserkMode = BuildType != BuildTypeE.InnerRelease || true;

	internal static readonly bool UseOnlyIdle = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool UseGameEvents = BuildType != BuildTypeE.InnerRelease || true;

	internal static readonly int DebugInitRageBubblesCount = ((BuildType != BuildTypeE.InnerRelease) ? (-1) : (-1));

	internal static readonly bool DebugShowAlwaysAchivsButtons = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugNoLevelLimitForRage = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugNoLevelLimitForMana = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugShopBuyLocal = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugAlwaysGenerateElf = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugGetRatingLocal = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugIgnoreSocialPostPeriod = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugShowReturnInStartLocation = BuildType != BuildTypeE.InnerRelease && false;

	internal static readonly bool DebugUseLocalSocialPosting = BuildType != BuildTypeE.InnerRelease && false;

	public static readonly bool DebugStartMenuSimple = false;

	public static readonly bool DebugLoadSoundsABOnly = false;

	public static readonly bool DebugDontLoadAndPlaySounds = false;

	public static readonly bool DebugDontLoadAndPlayMusic = false;

	public static readonly bool DebugDontLoadPlayer = false;

	public static readonly bool DebugDontLoadEnemy = false;

	public static readonly bool DebugDontGoToBag = false;

	public static readonly bool DebugMinMapMax = false;

	public static readonly bool DebugDontLoadConfig = false;

	public static readonly bool DebugDontLoadSave = false;

	public static readonly bool DebugDontLoadAnimation = false;

	public static readonly bool DebugForceLoadAllSounds = false;

	public static readonly bool ShowSocial = false;

	public static readonly string DebugFakeTexturePath = "__atlases/test2x2";

	public static readonly bool DebugDoNotLoadAtlasTextures = false;

	public static readonly bool DebugDoNotLoadAtlases = false;

	public static readonly bool IgnoreHud = false;

	public static readonly bool IgnoreSaveGame = false;

	public static readonly bool DebugDontUnloadAssetsBundles = false;

	public static readonly bool DebugFakeServerDataLoad = false;

	public static bool ShowTutorials = true;

	public static bool ViewSectorMoveForce = true;

	public static bool PlayerAttackSpawnOneMagicBall = false;

	public static readonly bool NoCameraMoveTo = false;

	internal static Player Player;

	internal static GameObject PlayerGameObject;

	internal static Enemy Enemy;

	internal static Battle Battle;

	internal static MainMenu MainMenu;

	internal static MonoBehaviour ABLoader;

	internal static int LastLoadedSceneServerId = -1;

	internal static bool ForceShowAllLocationsOnMap = false;

	internal static bool ForceShowAllLocations = false;

	internal static bool ForceDontClickRageBubbles = false;

	internal static bool ForceDontClickManaBubbles = false;

	internal static bool ForceDontClickRessurectionBubbles = false;

	internal static bool ForceDontSpawnManaBalls = false;

	internal static bool ForceDontSpawnRageBalls = false;

	internal static bool ForceSpawnRageBalls = false;

	internal static bool ForcePauseStrongMagic = false;

	internal static bool ForceWeakMagicNoTimeLimit = false;

	internal static bool ForceFatalityNoTimeLimit = false;

	internal static bool ForceDontHitEye = false;

	internal static bool ForceDontSpawnResurrection = false;

	internal static bool ForceDontClickViewSector = false;

	internal static AttackE ForceWantedAttack = AttackE.None;

	internal static bool ForceDontProcessSliceAttack = false;

	internal static bool ForceDontProcessRageButtons = false;

	internal static bool ForceEnemyDontCastMagic = false;

	internal static bool ForceDontClickFallingStars = false;

	internal static bool ForceDontClickManaButton = false;

	internal static string PlayerPrefSoundVolume = "SoundVolume";

	internal static string PlayerPrefMusicVolume = "MusicVolume";

	internal static string PlayerPrefLast24hMetricsDate = "Last24hMetricsDate";

	internal static string PlayerPrefSlotUUIDs = "SlotUUIDs";

	internal static readonly bool LoadImageConfigsOnly = false;

	internal static readonly int DefaultFPS = 60;

	internal static readonly int DefaultLowFPS = 30;

	public static bool IsPlayMusic = true;

	public static bool IsPlaySound = true;

	internal static bool GameDataLoaded = false;

	private static GameScreenE _gameScreen = GameScreenE.StartMenu;

	private static GameScreenE _prevGameScreen = GameScreenE.Map;

	internal static readonly string MagicDarkness = "Dark";

	internal static readonly string MagicFire = "Fire";

	internal static readonly string MagicIce = "Ice";

	internal static readonly string MagicElectro = "Lightning";

	internal static bool DrawBattleGUI = true;

	internal static float FxTimeout = 15f;

	internal static readonly int EnemyAttackRandomStepMax = 20;

	internal static readonly float ReactWaitTime = 0.5f;

	internal static readonly int ComboSize = 7;

	internal static bool LastLoadGameSuccessed = false;

	internal static int LevelAtStartFight = 1;

	internal static bool ForceVideoPlayback = BuildType != BuildTypeE.InnerRelease && false;

	internal static bool IsSovereighnFinalPlayed = false;

	internal static bool IsFinalScreenShowed = false;

	internal static bool IsUnaIntroPlayed = false;

	internal static bool IsSwampIntroPlayed = false;

	internal static bool IsPaused = false;

	internal static bool InFight = false;

	private static bool _Nav0GoToZachistka = false;

	internal static readonly string ReactHeal = "react_heal";

	internal static readonly string ReactDeath = "react_death";

	internal static readonly string ReactDeathForced = "react_deathf";

	internal static readonly string ReactDamage = "react_dmg";

	internal static readonly string ReactDamageForced = "react_dmgf";

	internal static readonly string ReactBlock = "react_block";

	internal static readonly string ReactDodge = "react_dodge";

	internal static readonly string ShopIdleAnimationName = "_shop_idle_";

	internal static readonly string VictoryAnimationName = "_victory_idle_";

	internal static readonly float DefaultTimeScale = 1f;

	private static int _isIPad1 = 0;

	internal static float DistanceBetweenPersons = 1.6f;

	internal static readonly float CastAngleDeviation = 45f;

	private static LoadingScreen _loadingScreen;

	internal static GameScreenE PrevGameScreen => _prevGameScreen;

	internal static GameScreenE GameScreen
	{
		get
		{
			return _gameScreen;
		}
		set
		{
			if (_gameScreen != value)
			{
				_prevGameScreen = _gameScreen;
				_gameScreen = value;
				Messenger.Invoke(MsgGameScreenChanged);
			}
		}
	}

	internal static bool Nav0GoToZachistka
	{
		get
		{
			return _Nav0GoToZachistka;
		}
		set
		{
			_Nav0GoToZachistka = value;
		}
	}

	internal static bool IsIPad1 => true;

	internal static bool IsTouchscreen => false;

	internal static int CastPixelDeviation => (int)(0.09765625f * (float)Screen.height);

	internal static int CastDarknessEndYDeviation => Screen.height / 4;

	public static float OpenFeintBankRealToCoins(float count)
	{
		return (float)Math.Ceiling((double)count / 0.013);
	}

	public static float IDreamSkyBankRealToCoins(float count)
	{
		return (float)Math.Round(count, 2);
	}

	public static float GameClubBankRealToCoins(float count)
	{
		return (float)Math.Round(count, 2);
	}

	public static string GetIDreamSkyLeaderboardId()
	{
		return UnityApi.GetPackageName() + ".Main";
	}

	public static string GetIDreamSkyLeaderboardCaveId()
	{
		return UnityApi.GetPackageName() + ".Cave";
	}

	public static string GetIDreamSkyAchievmentId(string id)
	{
		return UnityApi.GetPackageName() + "." + id;
	}

	public static string GetOpenFeintLeaderboardId()
	{
		string openFeintAppId = UnityApi.GetOpenFeintAppId();
		if (openFeintAppId == OldOpenFeintAppId)
		{
			return "1180267";
		}
		if (openFeintAppId == NewOpenFeintAppId)
		{
			return "10001553";
		}
		return string.Empty;
	}

	public static string GetOpenFeintLeaderboardCaveId()
	{
		string openFeintAppId = UnityApi.GetOpenFeintAppId();
		if (openFeintAppId == OldOpenFeintAppId)
		{
			return "1200447";
		}
		if (openFeintAppId == NewOpenFeintAppId)
		{
			return "10001554";
		}
		return string.Empty;
	}

	public static Dictionary<string, string> GetOpenFeintAchievmentsMap()
	{
		string openFeintAppId = UnityApi.GetOpenFeintAppId();
		if (openFeintAppId == OldOpenFeintAppId)
		{
			return GetOldOpenFeintAchievmentsMap();
		}
		if (openFeintAppId == NewOpenFeintAppId)
		{
			return GetNewOpenFeintAchievmentsMap();
		}
		return null;
	}

	public static Dictionary<string, string> GetOldOpenFeintAchievmentsMap()
	{
		if (_OldOpenFeintAchievmentsMap == null)
		{
			_OldOpenFeintAchievmentsMap = new Dictionary<string, string>();
			_OldOpenFeintAchievmentsMap.Add("847", "1686272");
			_OldOpenFeintAchievmentsMap.Add("850", "1686282");
			_OldOpenFeintAchievmentsMap.Add("851", "1686292");
			_OldOpenFeintAchievmentsMap.Add("852", "1686302");
			_OldOpenFeintAchievmentsMap.Add("853", "1686312");
			_OldOpenFeintAchievmentsMap.Add("854", "1686322");
			_OldOpenFeintAchievmentsMap.Add("855", "1686332");
			_OldOpenFeintAchievmentsMap.Add("856", "1686342");
			_OldOpenFeintAchievmentsMap.Add("857", "1686352");
			_OldOpenFeintAchievmentsMap.Add("858", "1686362");
			_OldOpenFeintAchievmentsMap.Add("859", "1686372");
			_OldOpenFeintAchievmentsMap.Add("860", "1686382");
			_OldOpenFeintAchievmentsMap.Add("861", "1686392");
			_OldOpenFeintAchievmentsMap.Add("862", "1686402");
			_OldOpenFeintAchievmentsMap.Add("863", "1686412");
			_OldOpenFeintAchievmentsMap.Add("864", "1686422");
			_OldOpenFeintAchievmentsMap.Add("865", "1686432");
			_OldOpenFeintAchievmentsMap.Add("866", "1686442");
			_OldOpenFeintAchievmentsMap.Add("867", "1686452");
			_OldOpenFeintAchievmentsMap.Add("868", "1686462");
			_OldOpenFeintAchievmentsMap.Add("869", "1686472");
			_OldOpenFeintAchievmentsMap.Add("870", "1686482");
			_OldOpenFeintAchievmentsMap.Add("871", "1686492");
			_OldOpenFeintAchievmentsMap.Add("872", "1686502");
			_OldOpenFeintAchievmentsMap.Add("873", "1686512");
			_OldOpenFeintAchievmentsMap.Add("874", "1686522");
			_OldOpenFeintAchievmentsMap.Add("875", "1686532");
			_OldOpenFeintAchievmentsMap.Add("876", "1686542");
			_OldOpenFeintAchievmentsMap.Add("877", "1686552");
			_OldOpenFeintAchievmentsMap.Add("878", "1686562");
			_OldOpenFeintAchievmentsMap.Add("879", "1686572");
			_OldOpenFeintAchievmentsMap.Add("880", "1686582");
			_OldOpenFeintAchievmentsMap.Add("881", "1686592");
			_OldOpenFeintAchievmentsMap.Add("882", "1686602");
			_OldOpenFeintAchievmentsMap.Add("883", "1686612");
			_OldOpenFeintAchievmentsMap.Add("884", "1686622");
			_OldOpenFeintAchievmentsMap.Add("885", "1686632");
			_OldOpenFeintAchievmentsMap.Add("886", "1686642");
			_OldOpenFeintAchievmentsMap.Add("887", "1686652");
			_OldOpenFeintAchievmentsMap.Add("888", "1686662");
			_OldOpenFeintAchievmentsMap.Add("889", "1686672");
			_OldOpenFeintAchievmentsMap.Add("890", "1686682");
			_OldOpenFeintAchievmentsMap.Add("891", "1686692");
			_OldOpenFeintAchievmentsMap.Add("892", "1686702");
			_OldOpenFeintAchievmentsMap.Add("893", "1686712");
			_OldOpenFeintAchievmentsMap.Add("894", "1686722");
			_OldOpenFeintAchievmentsMap.Add("895", "1686732");
			_OldOpenFeintAchievmentsMap.Add("896", "1686742");
			_OldOpenFeintAchievmentsMap.Add("897", "1686752");
			_OldOpenFeintAchievmentsMap.Add("898", "1686762");
			_OldOpenFeintAchievmentsMap.Add("899", "1686782");
			_OldOpenFeintAchievmentsMap.Add("900", "1686792");
			_OldOpenFeintAchievmentsMap.Add("901", "1686802");
			_OldOpenFeintAchievmentsMap.Add("902", "1686812");
			_OldOpenFeintAchievmentsMap.Add("903", "1686822");
			_OldOpenFeintAchievmentsMap.Add("904", "1686832");
			_OldOpenFeintAchievmentsMap.Add("905", "1686842");
			_OldOpenFeintAchievmentsMap.Add("906", "1686852");
			_OldOpenFeintAchievmentsMap.Add("999", "1686872");
			_OldOpenFeintAchievmentsMap.Add("1000", "1686882");
			_OldOpenFeintAchievmentsMap.Add("1001", "1686892");
			_OldOpenFeintAchievmentsMap.Add("1002", "1686902");
			_OldOpenFeintAchievmentsMap.Add("1003", "1686912");
			_OldOpenFeintAchievmentsMap.Add("1004", "1686922");
			_OldOpenFeintAchievmentsMap.Add("1005", "1686932");
			_OldOpenFeintAchievmentsMap.Add("1006", "1686942");
			_OldOpenFeintAchievmentsMap.Add("1007", "1686952");
			_OldOpenFeintAchievmentsMap.Add("1008", "1686962");
			_OldOpenFeintAchievmentsMap.Add("1009", "1686972");
			_OldOpenFeintAchievmentsMap.Add("1010", "1686982");
			_OldOpenFeintAchievmentsMap.Add("1011", "1687002");
			_OldOpenFeintAchievmentsMap.Add("1012", "1687012");
			_OldOpenFeintAchievmentsMap.Add("1013", "1687022");
			_OldOpenFeintAchievmentsMap.Add("1014", "1687032");
			_OldOpenFeintAchievmentsMap.Add("1015", "1687042");
			_OldOpenFeintAchievmentsMap.Add("1016", "1687062");
			_OldOpenFeintAchievmentsMap.Add("1017", "1687072");
			_OldOpenFeintAchievmentsMap.Add("1018", "1687082");
			_OldOpenFeintAchievmentsMap.Add("1019", "1687092");
			_OldOpenFeintAchievmentsMap.Add("1109", "1687102");
			_OldOpenFeintAchievmentsMap.Add("1110", "1687112");
			_OldOpenFeintAchievmentsMap.Add("1111", "1687122");
			_OldOpenFeintAchievmentsMap.Add("1112", "1687142");
			_OldOpenFeintAchievmentsMap.Add("1239", "1687152");
			_OldOpenFeintAchievmentsMap.Add("1240", "1687162");
			_OldOpenFeintAchievmentsMap.Add("1241", "1687172");
			_OldOpenFeintAchievmentsMap.Add("1242", "1687182");
			_OldOpenFeintAchievmentsMap.Add("1265", "1687192");
			_OldOpenFeintAchievmentsMap.Add("1266", "1687202");
			_OldOpenFeintAchievmentsMap.Add("1935", string.Empty);
			_OldOpenFeintAchievmentsMap.Add("1936", string.Empty);
			_OldOpenFeintAchievmentsMap.Add("1937", string.Empty);
			_OldOpenFeintAchievmentsMap.Add("1938", string.Empty);
			_OldOpenFeintAchievmentsMap.Add("1939", string.Empty);
		}
		return _OldOpenFeintAchievmentsMap;
	}

	public static Dictionary<string, string> GetNewOpenFeintAchievmentsMap()
	{
		if (_NewOpenFeintAchievmentsMap == null)
		{
			_NewOpenFeintAchievmentsMap = new Dictionary<string, string>();
			_NewOpenFeintAchievmentsMap.Add(string.Empty, string.Empty);
			_NewOpenFeintAchievmentsMap.Add("847", "10003891");
			_NewOpenFeintAchievmentsMap.Add("850", "10003893");
			_NewOpenFeintAchievmentsMap.Add("851", "10003916");
			_NewOpenFeintAchievmentsMap.Add("852", "10003917");
			_NewOpenFeintAchievmentsMap.Add("853", "10003918");
			_NewOpenFeintAchievmentsMap.Add("854", "10003919");
			_NewOpenFeintAchievmentsMap.Add("855", "10003920");
			_NewOpenFeintAchievmentsMap.Add("856", "10003921");
			_NewOpenFeintAchievmentsMap.Add("857", "10003922");
			_NewOpenFeintAchievmentsMap.Add("858", "10003923");
			_NewOpenFeintAchievmentsMap.Add("859", "10003924");
			_NewOpenFeintAchievmentsMap.Add("860", "10003925");
			_NewOpenFeintAchievmentsMap.Add("861", "10003926");
			_NewOpenFeintAchievmentsMap.Add("862", "10003927");
			_NewOpenFeintAchievmentsMap.Add("863", "10003928");
			_NewOpenFeintAchievmentsMap.Add("864", "10003929");
			_NewOpenFeintAchievmentsMap.Add("865", "10003930");
			_NewOpenFeintAchievmentsMap.Add("866", "10003931");
			_NewOpenFeintAchievmentsMap.Add("867", "10003932");
			_NewOpenFeintAchievmentsMap.Add("868", "10003933");
			_NewOpenFeintAchievmentsMap.Add("869", "10003934");
			_NewOpenFeintAchievmentsMap.Add("870", "10003935");
			_NewOpenFeintAchievmentsMap.Add("871", "10003936");
			_NewOpenFeintAchievmentsMap.Add("872", "10003937");
			_NewOpenFeintAchievmentsMap.Add("873", "10003938");
			_NewOpenFeintAchievmentsMap.Add("874", "10003939");
			_NewOpenFeintAchievmentsMap.Add("875", "10003940");
			_NewOpenFeintAchievmentsMap.Add("876", "10003941");
			_NewOpenFeintAchievmentsMap.Add("877", "10003942");
			_NewOpenFeintAchievmentsMap.Add("878", "10003943");
			_NewOpenFeintAchievmentsMap.Add("879", "10003944");
			_NewOpenFeintAchievmentsMap.Add("880", "10003945");
			_NewOpenFeintAchievmentsMap.Add("881", "10003946");
			_NewOpenFeintAchievmentsMap.Add("882", "10003947");
			_NewOpenFeintAchievmentsMap.Add("883", "10003948");
			_NewOpenFeintAchievmentsMap.Add("884", "10003949");
			_NewOpenFeintAchievmentsMap.Add("885", "10003950");
			_NewOpenFeintAchievmentsMap.Add("886", "10003951");
			_NewOpenFeintAchievmentsMap.Add("887", "10003952");
			_NewOpenFeintAchievmentsMap.Add("888", "10003953");
			_NewOpenFeintAchievmentsMap.Add("889", "10003954");
			_NewOpenFeintAchievmentsMap.Add("890", "10003955");
			_NewOpenFeintAchievmentsMap.Add("891", "10003956");
			_NewOpenFeintAchievmentsMap.Add("892", "10003957");
			_NewOpenFeintAchievmentsMap.Add("893", "10003958");
			_NewOpenFeintAchievmentsMap.Add("894", "10003959");
			_NewOpenFeintAchievmentsMap.Add("895", "10003960");
			_NewOpenFeintAchievmentsMap.Add("896", "10003961");
			_NewOpenFeintAchievmentsMap.Add("897", "10003962");
			_NewOpenFeintAchievmentsMap.Add("898", "10003963");
			_NewOpenFeintAchievmentsMap.Add("899", "10003964");
			_NewOpenFeintAchievmentsMap.Add("900", "10003965");
			_NewOpenFeintAchievmentsMap.Add("901", "10003966");
			_NewOpenFeintAchievmentsMap.Add("902", "10003967");
			_NewOpenFeintAchievmentsMap.Add("903", "10003968");
			_NewOpenFeintAchievmentsMap.Add("904", "10003969");
			_NewOpenFeintAchievmentsMap.Add("905", "10003970");
			_NewOpenFeintAchievmentsMap.Add("906", "10003971");
			_NewOpenFeintAchievmentsMap.Add("999", "10003972");
			_NewOpenFeintAchievmentsMap.Add("1000", "10003973");
			_NewOpenFeintAchievmentsMap.Add("1001", "10003974");
			_NewOpenFeintAchievmentsMap.Add("1002", "10003975");
			_NewOpenFeintAchievmentsMap.Add("1003", "10003976");
			_NewOpenFeintAchievmentsMap.Add("1004", "10003977");
			_NewOpenFeintAchievmentsMap.Add("1005", "10003978");
			_NewOpenFeintAchievmentsMap.Add("1006", "10003979");
			_NewOpenFeintAchievmentsMap.Add("1007", "10003980");
			_NewOpenFeintAchievmentsMap.Add("1008", "10003981");
			_NewOpenFeintAchievmentsMap.Add("1009", "10003982");
			_NewOpenFeintAchievmentsMap.Add("1010", "10003983");
			_NewOpenFeintAchievmentsMap.Add("1011", "10003984");
			_NewOpenFeintAchievmentsMap.Add("1012", "10003985");
			_NewOpenFeintAchievmentsMap.Add("1013", "10003986");
			_NewOpenFeintAchievmentsMap.Add("1014", "10003987");
			_NewOpenFeintAchievmentsMap.Add("1015", "10003988");
			_NewOpenFeintAchievmentsMap.Add("1016", "10003989");
			_NewOpenFeintAchievmentsMap.Add("1017", "10003990");
			_NewOpenFeintAchievmentsMap.Add("1018", "10003991");
			_NewOpenFeintAchievmentsMap.Add("1019", "10003992");
			_NewOpenFeintAchievmentsMap.Add("1109", "10003993");
			_NewOpenFeintAchievmentsMap.Add("1110", "10003994");
			_NewOpenFeintAchievmentsMap.Add("1111", "10003995");
			_NewOpenFeintAchievmentsMap.Add("1112", "10003996");
			_NewOpenFeintAchievmentsMap.Add("1239", "10003997");
			_NewOpenFeintAchievmentsMap.Add("1240", "10003998");
			_NewOpenFeintAchievmentsMap.Add("1241", "10003999");
			_NewOpenFeintAchievmentsMap.Add("1242", "10004000");
			_NewOpenFeintAchievmentsMap.Add("1265", "10004001");
			_NewOpenFeintAchievmentsMap.Add("1266", "10004002");
			_NewOpenFeintAchievmentsMap.Add("1935", "10004003");
			_NewOpenFeintAchievmentsMap.Add("1936", "10004004");
			_NewOpenFeintAchievmentsMap.Add("1937", "10004005");
			_NewOpenFeintAchievmentsMap.Add("1938", "10004006");
			_NewOpenFeintAchievmentsMap.Add("1939", "10004007");
		}
		return _NewOpenFeintAchievmentsMap;
	}

	public static void Reload(MonoBehaviour caller)
	{
		UnityApi.SetMetricController(null);
		BagInventory.Instance = null;
		ChooseCharHudGui.Instance = null;
		HudMk1.Instance = null;
		FontManager.Instance.Clear();
		FontManager.Instance.Shutdown();
		SingletonT<AtlasManager>.I.Clear();
		SingletonT<Fxs>.I.CleanFxCache();
		SingletonT<SoundManager>.I.UnloadAllSounds();
		SingletonT<ResourcesManager>.I.UnloadUnusedAssets(caller, delegate
		{
			SceneManager.LoadScene(0);
		});
	}

	public static void MemDebugPrint(string what)
	{
	}

	internal static void ResetTutorialsFlags()
	{
		ForceShowAllLocationsOnMap = false;
		ForceShowAllLocations = false;
		ForceDontClickRageBubbles = false;
		ForceDontClickManaBubbles = false;
		ForceDontClickRessurectionBubbles = false;
		ForceDontSpawnManaBalls = false;
		ForceDontSpawnRageBalls = false;
		ForceSpawnRageBalls = false;
		ForcePauseStrongMagic = false;
		ForceWeakMagicNoTimeLimit = false;
		ForceFatalityNoTimeLimit = false;
		ForceDontHitEye = false;
		ForceDontSpawnResurrection = false;
		ForceDontClickViewSector = false;
		ForceWantedAttack = AttackE.None;
		ForceDontProcessSliceAttack = false;
		ForceDontProcessRageButtons = false;
		ForceEnemyDontCastMagic = false;
		ForceDontClickFallingStars = false;
		ForceDontClickManaButton = false;
	}

	public static bool CanAttack(AttackE attack)
	{
		return ForceWantedAttack == AttackE.None || ForceWantedAttack == attack;
	}

	public static string DefaultSetName(ServerData.Slot.TypeE slot)
	{
		if (IgnoreHud)
		{
			if (slot == ServerData.Slot.TypeE.Weapon)
			{
				return "12";
			}
			return "6";
		}
		if (slot == ServerData.Slot.TypeE.Weapon)
		{
			if (SingletonT<ServerData>.I.PlayerServerPersData != null)
			{
				if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassRed)
				{
					return SingletonT<ServerData>.I.GameSettings.DefaultRedWeapon;
				}
				if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassGreen)
				{
					return SingletonT<ServerData>.I.GameSettings.DefaultGreenWeapon;
				}
				if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassBlue)
				{
					return SingletonT<ServerData>.I.GameSettings.DefaultBlueWeapon;
				}
			}
			return "12";
		}
		if (SingletonT<ServerData>.I.PlayerServerPersData != null)
		{
			if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassRed)
			{
				return "6";
			}
			if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassGreen)
			{
				return "5";
			}
			if (SingletonT<ServerData>.I.PlayerServerPersData.IsClassBlue)
			{
				return "4";
			}
		}
		return "6";
	}

	public static string PrefabName(this ServerData.Slot.TypeE slot, string setName)
	{
		return slot switch
		{
			ServerData.Slot.TypeE.Boots => "8_boots", 
			ServerData.Slot.TypeE.Torso => "4_torso", 
			ServerData.Slot.TypeE.Pelvis => "7_pelvis", 
			ServerData.Slot.TypeE.Belt => "6_belt", 
			ServerData.Slot.TypeE.HandLeft => "2_hand_l", 
			ServerData.Slot.TypeE.HandRight => "3_hand_r", 
			ServerData.Slot.TypeE.Helm => "1_helm", 
			ServerData.Slot.TypeE.Shoulder => "5_shoulderstrap", 
			ServerData.Slot.TypeE.Weapon => setName, 
			_ => null, 
		};
	}

	internal static int AttackDamage(Player player, float mult)
	{
		return ((float)UnityEngine.Random.Range(10, 15) * mult).RoundToInt();
	}

	internal static void Pause()
	{
		if (!IsPaused)
		{
			IsPaused = true;
			Utils.LogForce("PAUSE");
			Time.timeScale = 0f;
			SpriteGui.DontReleaseButtons = true;
			if (HudMk1.Instance != null)
			{
				HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.Pause);
			}
		}
	}

	internal static void Resume()
	{
		if (IsPaused)
		{
			IsPaused = false;
			SpriteGui.DontReleaseButtons = false;
			Time.timeScale = DefaultTimeScale;
			if (HudMk1.Instance != null)
			{
				HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.Pause);
			}
		}
	}

	internal static void TogglePauseResume()
	{
		if (!IsPaused)
		{
			Pause();
		}
		else
		{
			Resume();
		}
	}

	internal static ITouchscreen CreateTouchscreen()
	{
		if (IsTouchscreen)
		{
			return new TouchscreenIPod();
		}
		return new TouchscreenMouse();
	}

	internal static void SetColorAsBotsColor(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		GameObject gameObject = GameObject.Find(LocationGameObjectSceneGeomName);
		if (!(gameObject != null))
		{
			return;
		}
		Component component = gameObject.GetComponent<scene_parameters>();
		if (component != null)
		{
			object value = Utils.GetValue(component, "botsColor");
			if (value != null)
			{
				SetColorAs(go, (Color)value);
			}
		}
	}

	internal static void SetColorAs(GameObject go, Color color)
	{
		if (go == null)
		{
			return;
		}
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] materials = GetComponent<Renderer>()materials;
			foreach (Material material in materials)
			{
				material.color = color;
			}
		}
	}

	internal static void PlayBattleMusic(MonoBehaviour caller, bool wasSuccess)
	{
		AreaData current = AreaData.Current;
		Utils.Log("PlayBattleMusic", current);
		try
		{
			if (current.Mobs != null && current.Location.Logic.ZachistkaMobsKilled < current.Mobs.Length && current.Mobs[current.Location.Logic.ZachistkaMobsKilled].IsBoss)
			{
				SingletonT<SoundManager>.I.PlayInBattleBossMusic();
			}
			else if (!wasSuccess)
			{
				SingletonT<SoundManager>.I.PlayBattleMusic();
			}
		}
		catch (Exception)
		{
			Utils.Log("PlayBattleMusic failed");
		}
		Utils.Log("PlayBattleMusic ok");
	}

	internal static void RefreshLoadingScreen(ActionD action)
	{
		if (_loadingScreen == null)
		{
			_loadingScreen = (LoadingScreen)UnityEngine.Object.FindObjectOfType(typeof(LoadingScreen));
		}
		if (_loadingScreen != null)
		{
			if (IsDebugBuild)
			{
				Debug.Log("REFRESH LOADING SCREEN");
			}
			_loadingScreen.RefreshLoadingScreen(action);
		}
		else
		{
			action?.Invoke();
		}
	}

	internal static void ShowLoadingScreen(ActionD action)
	{
		if (_loadingScreen == null)
		{
			_loadingScreen = (LoadingScreen)UnityEngine.Object.FindObjectOfType(typeof(LoadingScreen));
		}
		if (_loadingScreen != null)
		{
			if (IsDebugBuild)
			{
				Debug.Log("SHOW LOADING SCREEN");
			}
			UnityApi.AcquireLoadingWakeLock();
			_loadingScreen.ShowLoadingScreen(action);
		}
		else
		{
			action?.Invoke();
		}
	}

	internal static void HideLoadingScreen()
	{
		if (_loadingScreen == null)
		{
			_loadingScreen = (LoadingScreen)UnityEngine.Object.FindObjectOfType(typeof(LoadingScreen));
		}
		if (_loadingScreen != null)
		{
			if (IsDebugBuild)
			{
				Debug.Log("HIDE LOADING SCREEN");
			}
			_loadingScreen.HideLoadingScreen();
			UnityApi.ReleaseLoadingWakeLock();
		}
	}

	internal static string EnemyName(string prototypeId)
	{
		return "__enemy_" + prototypeId.ToString();
	}

	internal static void HideDebugButtons()
	{
		if (MainMenu == null)
		{
			return;
		}
		Transform transform = MainMenu.transform.FindChildByName("debug_buttons");
		if (!(transform == null))
		{
			Transform transform2 = transform.FindChildByName("button_load_config");
			if (transform2 != null)
			{
				transform2.GoToHell();
			}
			transform2 = transform.FindChildByName("button_reset");
			if (transform2 != null)
			{
				transform2.GoToHell();
			}
			transform2 = transform.FindChildByName("button_save");
			if (transform2 != null)
			{
				transform2.GoToHell();
			}
		}
	}
}
