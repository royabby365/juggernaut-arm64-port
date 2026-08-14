using System;
using System.Collections.Generic;
using System.IO;
using Common;
using UnityEngine;
using Yarx;

public class MainMenuHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public MainMenuSlot[] SaveSlots;

	private static readonly string _slotName = "_slot_";

	private static readonly string _deleteSave = "_delete_save_";

	private void Awake()
	{
		for (int i = 0; i < 4; i++)
		{
			PlayerState playerState = Globals.MainMenu.TryLoadSave(i);
			if (playerState == null)
			{
				SaveSlots[i].ShowHideInfo(show: false);
				continue;
			}
			int num = ((playerState.Inventory != null) ? playerState.Inventory.IndexOf((ServerData.Item item2) => item2.PutOn && SingletonT<ServerData>.I.IsHelm(item2)) : (-1));
			ServerData.Item item = ((num < 0) ? null : SingletonT<ServerData>.I.GetItemByServerId(playerState.Inventory[num].Id));
			ServerData.PersData persDataByServerId = SingletonT<ServerData>.I.GetPersDataByServerId(playerState.PlayerPersDataId);
			string text = ((persDataByServerId == null) ? "m" : ((!persDataByServerId.IsMan) ? "f" : "m"));
			int num2 = persDataByServerId?.Class ?? 1;
			string text2 = PlayerHud.DecodeItemColor(item);
			string path = "{0}_set_{1}_{2}".Fmt(text2, num2, text);
			path = Path.GetFileNameWithoutExtension(path);
			SaveSlots[i].SetInfo(playerState.SaveTime, path, playerState.PlayerParams.Level, playerState.PlayerParams.GoldCount, playerState.PlayerParams.DiamondCount, (persDataByServerId == null) ? string.Empty : persDataByServerId.Title);
		}
		RearrangeSlots();
	}

	private void RearrangeSlots()
	{
		List<MainMenuSlot> list = new List<MainMenuSlot>();
		list.AddRange(SaveSlots);
		list.Sort(SortBySaveTime);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].transform.localPosition = new Vector3(0f, i * 83, 0f);
		}
	}

	private static int SortBySaveTime(MainMenuSlot p1, MainMenuSlot p2)
	{
		if (!p1.HasData)
		{
			if (!p2.HasData)
			{
				return 0;
			}
			return -1;
		}
		if (!p2.HasData)
		{
			if (!p1.HasData)
			{
				return 0;
			}
			return 1;
		}
		if (p1.SaveTime > p2.SaveTime)
		{
			return 1;
		}
		if (p1.SaveTime == p2.SaveTime)
		{
			return 0;
		}
		return -1;
	}

	private void ProcessButtons(string buttonName)
	{
		if (buttonName.StartsWith(_slotName))
		{
			int slotId = -1;
			if (!int.TryParse(buttonName.Substring(_slotName.Length), out slotId))
			{
				return;
			}
			PlayerState playerState = Globals.MainMenu.TryLoadSave(slotId);
			if (playerState != null)
			{
				Globals.MainMenu.LoadGameThenContinue(slotId);
				return;
			}
			Globals.MainMenu.SaveSlotIndex = slotId;
			Globals.ShowLoadingScreen(delegate
			{
				SocialPoster.ResetCulldown(slotId);
				Globals.MainMenu.StartNewGame();
			});
		}
		else
		{
			if (!buttonName.StartsWith(_deleteSave))
			{
				return;
			}
			int slotId2 = -1;
			if (!int.TryParse(buttonName.Substring(_deleteSave.Length), out slotId2))
			{
				return;
			}
			Messenger<ServerData.PhrasesE, ServerData.PhrasesE, string, Action>.Invoke(Globals.MsgPopup2ButtonYesHandlerCustomMessage, ServerData.PhrasesE.ButtonYes, ServerData.PhrasesE.ButtonNo, SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.MainMenuNewGameAlert), delegate
			{
				string persistentDataPath = Application.persistentDataPath;
				string path = Path.Combine(persistentDataPath, GetFileName(slotId2));
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				SaveSlots[slotId2].ShowHideInfo(show: false);
				RearrangeSlots();
				SocialPoster.ResetCulldown(slotId2);
			});
		}
	}

	private string GetFileName(int index)
	{
		if (index > 0)
		{
			return "jug_savering" + index + ".jug";
		}
		return "jug_savering.jug";
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<string>.AddListener(Globals.MsgGuiButtonPressed, ProcessButtons));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
