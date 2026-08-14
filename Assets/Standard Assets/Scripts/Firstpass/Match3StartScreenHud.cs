using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class Match3StartScreenHud : MonoBehaviour
{
	private Match3Hud.DifficultyE _lastDifficulty;

	private bool _waitTimeout;

	public SpriteText CounterGold;

	public SpriteText CounterCrystall;

	public SpriteText CounterSkull;

	public SpriteText CounterStar;

	public SpriteText CounterScarab;

	public GameObject RootLootMined;

	public GameObject RootRating;

	public GameObject MinesRoot;

	public BagSwitchButton[] Switches;

	public RatingEntry[] Ratings;

	public GameObject WaitMessage;

	public GameObject ButtonFullRating;

	public GameObject ButtonAddFriends;

	public GameObject FrameAddFriends;

	private bool ButtonPositionsFixed;

	private CompositeDisposable _subscriptions;

	private static readonly ServerData.Item.ElixirTypeE[] BonusOrder = new ServerData.Item.ElixirTypeE[4]
	{
		ServerData.Item.ElixirTypeE.Gold,
		ServerData.Item.ElixirTypeE.Star,
		ServerData.Item.ElixirTypeE.Skull,
		ServerData.Item.ElixirTypeE.Scarab
	};

	private void Start()
	{
		WaitMessage.SetActiveRecursivelyMk1(setActive: false);
		FrameAddFriends.SetActiveRecursivelyMk1(setActive: false);
	}

	private void OnEnable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += Instance_Release;
		}
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger.AddListener(Globals.MsgObtainInMine, UpdateObtainedInMines));
	}

	private void OnDisable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release -= Instance_Release;
		}
		if (_subscriptions != null)
		{
			_subscriptions.Dispose();
			_subscriptions = null;
		}
	}

	private void Instance_Release(SpriteButton obj)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Match3StartScreen)
		{
			return;
		}
		if (obj.name.StartsWith("button_start_match3_minigame_"))
		{
			int num = -1;
			switch (obj.name)
			{
			case "button_start_match3_minigame_0":
				num = 0;
				break;
			case "button_start_match3_minigame_1":
				num = 1;
				break;
			case "button_start_match3_minigame_2":
				num = 2;
				break;
			case "button_start_match3_minigame_3":
				num = 3;
				break;
			case "button_start_match3_minigame_4":
				num = 4;
				break;
			}
			if (num < 0)
			{
				if (Globals.IsDebugBuild)
				{
					Debug.LogError("There is no such button name: " + obj.name);
				}
				return;
			}
			if (num >= SingletonT<ServerData>.I.Mines.Count)
			{
				if (Globals.IsDebugBuild)
				{
					Debug.LogError("FUCKUP -- mines number too low " + obj.name);
				}
				return;
			}
			ServerData.Mine mine = SingletonT<ServerData>.I.Mines[num];
			if (!mine.IsBuyed)
			{
				System.Tuple<ServerData.MoneyType.TypeE, int, string> price = mine.OpenPrice.GetPrice();
				if (price.Item1.GetPlayerFundsCount() < price.Item2)
				{
					if (price.Item1 == ServerData.MoneyType.TypeE.Key)
					{
						Messenger<ServerData.HintCodesE>.Invoke(Globals.MsgShowHint, ServerData.HintCodesE.vzlom);
					}
					else
					{
						Messenger.Invoke(Globals.MsgInsufficientFunds);
					}
					return;
				}
				price.Item1.ChangePlayerFundsCount(-price.Item2);
				Globals.MainMenu.SaveGame();
			}
			HudMk1.Instance.ChangeGuiTo(new HudMk1.GuiDesc(GuiRoot.GuiType.Match3, Yarx.Collections.Tuple.Create(mine)));
		}
		else if (obj.name == "match3_button_full_rating")
		{
			Messenger.Invoke(Globals.MsgOpenRatings, Globals.MainMenu.Social.GetLeaderboardCaveId());
		}
		else if (obj.name == "match3_button_add_friends")
		{
			Messenger.Invoke(Globals.MsgAddFriends);
		}
		else
		{
			if (obj as BagSwitchButton == null)
			{
				return;
			}
			string text = obj.name;
			for (int i = 0; i < Switches.Length; i++)
			{
				if (text == Switches[i].name)
				{
					ApplyFilter(i);
					break;
				}
			}
		}
	}

	public void Init(string mineTitle)
	{
		int num = 0;
		BagSwitchButton[] switches = Switches;
		foreach (BagSwitchButton bagSwitchButton in switches)
		{
			if (UnityApi.UseGameClub() && num == 1)
			{
				bagSwitchButton.gameObject.SetActiveRecursivelyMk1(setActive: false);
			}
			else
			{
				bagSwitchButton.SetActive();
			}
			num++;
		}
		ApplyFilter(2);
	}

	private void UpdateObtainedInMines()
	{
		CounterGold.Text_ = SingletonT<ServerData>.I.PlayerParams.Match3GoldMined.ToString();
		CounterCrystall.Text_ = SingletonT<ServerData>.I.PlayerParams.Match3CrystallMined.ToString();
		CounterSkull.Text_ = SingletonT<ServerData>.I.PlayerParams.Match3SkullMined.ToString();
		CounterStar.Text_ = SingletonT<ServerData>.I.PlayerParams.Match3StarMined.ToString();
		CounterScarab.Text_ = SingletonT<ServerData>.I.PlayerParams.Match3ScarabMined.ToString();
	}

	public void ApplyFilter(int i)
	{
		BagSwitchButton[] switches = Switches;
		foreach (BagSwitchButton bagSwitchButton in switches)
		{
			bagSwitchButton.SetUnselected();
			bagSwitchButton.SetActive();
		}
		if (UnityApi.UseGameClub() && i == 1)
		{
			i = 2;
		}
		Switches[i].SetSelected();
		Switches[i].SetInactive();
		switch (i)
		{
		case 0:
			RootLootMined.SetActiveRecursivelyMk1(setActive: true);
			RootRating.SetActiveRecursivelyMk1(setActive: false);
			MinesRoot.SetActiveRecursivelyMk1(setActive: false);
			UpdateObtainedInMines();
			break;
		case 1:
			RootLootMined.SetActiveRecursivelyMk1(setActive: false);
			RootRating.SetActiveRecursivelyMk1(setActive: true);
			MinesRoot.SetActiveRecursivelyMk1(setActive: false);
			ShowRatings();
			break;
		case 2:
			RootLootMined.SetActiveRecursivelyMk1(setActive: false);
			RootRating.SetActiveRecursivelyMk1(setActive: false);
			MinesRoot.SetActiveRecursivelyMk1(setActive: true);
			ShowMines();
			break;
		}
	}

	private void ShowMines()
	{
		MineRoot component = MinesRoot.GetComponent<MineRoot>();
		MinesLine[] lines = component.Lines;
		MinesLine[] array = lines;
		foreach (MinesLine minesLine in array)
		{
			minesLine.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		for (int j = 0; j < SingletonT<ServerData>.I.Mines.Count; j++)
		{
			ServerData.Mine mine = SingletonT<ServerData>.I.Mines[j];
			List<ServerData.Bonus.DropElement> drop = mine.SecondBonus.Drop;
			MinesLine minesLine2 = lines[j];
			lines[j].gameObject.SetActiveRecursivelyMk1(setActive: true);
			minesLine2.SetMine(mine);
			HashSet<ServerData.Item.ElixirTypeE> hashSet = new HashSet<ServerData.Item.ElixirTypeE>();
			Match3LootHud[] squares = minesLine2.Squares;
			foreach (Match3LootHud match3LootHud in squares)
			{
				match3LootHud.gameObject.SetActiveRecursivelyMk1(setActive: false);
			}
			if (minesLine2.Squares.Length < BonusOrder.Length && Globals.IsDebugBuild)
			{
				Debug.Log("[FUCKUP]".FmtEx());
			}
			ServerData.Item.ElixirTypeE[] bonusOrder = BonusOrder;
			foreach (ServerData.Item.ElixirTypeE elixirTypeE in bonusOrder)
			{
				ServerData.Item.ElixirTypeE e = elixirTypeE;
				int num = drop.FindIndex((ServerData.Bonus.DropElement de) => de.Item.ElixirType == e);
				if (num >= 0 && !hashSet.Contains(elixirTypeE))
				{
					int count = hashSet.Count;
					minesLine2.Squares[count].gameObject.SetActiveRecursivelyMk1(setActive: true);
					minesLine2.Squares[count].SetLoot(drop[num]);
					hashSet.Add(elixirTypeE);
				}
			}
		}
	}

	private void ShowRatings()
	{
		FrameAddFriends.SetActiveRecursivelyMk1(setActive: false);
		RatingEntry[] ratings = Ratings;
		foreach (RatingEntry ratingEntry in ratings)
		{
			ratingEntry.gameObject.SetActiveRecursively(state: false);
		}
		if (Globals.DebugGetRatingLocal)
		{
			ShowWaitMessage(4f);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(3f, delegate
			{
				_waitTimeout = false;
				SocialAspect.ScoresInfo[] array = new SocialAspect.ScoresInfo[2]
				{
					new SocialAspect.ScoresInfo(new UserProfileStub(), 100501L, isMe: false),
					new SocialAspect.ScoresInfo(new UserProfileStub(), 100502L, isMe: true)
				};
				CreateRatingEntries(array);
				FrameAddFriends.SetActiveRecursivelyMk1(array.Length == 0);
			});
			return;
		}
		if (UnityApi.UseGameClub())
		{
			CreateRatingEntries(new SocialAspect.ScoresInfo[0]);
			FrameAddFriends.SetActiveRecursivelyMk1(setActive: true);
			SpriteText componentInChildren = FrameAddFriends.GetComponentInChildren<SpriteText>();
			componentInChildren.gameObject.SetActive(false);
			if (!ButtonPositionsFixed)
			{
				Vector3 position = ButtonFullRating.transform.position;
				position.y += 300f;
				ButtonFullRating.transform.position = position;
				position = ButtonAddFriends.transform.position;
				position.y += 300f;
				ButtonAddFriends.transform.position = position;
				ButtonPositionsFixed = true;
			}
			return;
		}
		ShowWaitMessage(60f);
		Globals.MainMenu.Social.GetFriendsCaveScores(delegate(SocialAspect.ScoresInfo[] info)
		{
			_waitTimeout = false;
			CreateRatingEntries(info);
			FrameAddFriends.SetActiveRecursivelyMk1(info.Length == 0);
		}, delegate
		{
			if (_waitTimeout)
			{
				Messenger<ServerData.PhrasesE, Action>.Invoke(Globals.MsgShowAlertWithCallback, ServerData.PhrasesE.MsgGetRatingErrorAndroid, delegate
				{
					ApplyFilter(0);
				});
				_waitTimeout = false;
			}
		});
	}

	private void ShowWaitMessage(float timeout)
	{
		SpriteGui.DontReleaseButtons = true;
		WaitMessage.SetActiveRecursivelyMk1(setActive: true);
		StartCoroutine(WaitTimeout(timeout));
	}

	private void HideWaitMessage()
	{
		SpriteGui.DontReleaseButtons = false;
		WaitMessage.SetActiveRecursivelyMk1(setActive: false);
	}

	private IEnumerator WaitTimeout(float timeout)
	{
		_waitTimeout = true;
		float time = 0f;
		while (time < timeout && _waitTimeout)
		{
			time += Time.deltaTime;
			yield return null;
		}
		HideWaitMessage();
		if (_waitTimeout)
		{
			Messenger<ServerData.PhrasesE, Action>.Invoke(Globals.MsgShowAlertWithCallback, ServerData.PhrasesE.MsgRatingTimeoutAndroid, delegate
			{
				ApplyFilter(0);
			});
			_waitTimeout = false;
		}
	}

	private void CreateRatingEntries(SocialAspect.ScoresInfo[] info)
	{
		ButtonFullRating.SetActiveRecursivelyMk1(setActive: true);
		ButtonAddFriends.SetActiveRecursivelyMk1(setActive: true);
		for (int i = 0; i < Ratings.Length; i++)
		{
			RatingEntry ratingEntry = Ratings[i];
			if (i < info.Length)
			{
				SocialAspect.ScoresInfo info2 = info[i];
				ratingEntry.SetScoresInfo(i + 1, info2);
				ratingEntry.gameObject.SetActiveRecursivelyMk1(setActive: true);
			}
			else
			{
				ratingEntry.gameObject.SetActiveRecursivelyMk1(setActive: false);
			}
		}
	}
}
