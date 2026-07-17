using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class SidebarNav : MonoBehaviour
{
	private const int Shift = -80;

	private CompositeDisposable _subscriptions;

	public Transform[] ButtonBgs;

	public SidebarButton[] NavButtons;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		SidebarButton nav0 = NavButtons[0];
		SidebarButton nav1 = NavButtons[1];
		SidebarButton nav2 = NavButtons[2];
		SidebarButton sidebarButton = NavButtons[3];
		SidebarButton sidebarButton2 = NavButtons[4];
		_subscriptions.Add(Messenger<ServerData.Location, int>.AddListener(Globals.MsgLocationMobsAdded, delegate
		{
			nav0.TurnOnFlashing();
		}));
		_subscriptions.Add(Messenger<ServerData.Location, int, int>.AddListener(Globals.MsgLocationMoneyChanged, OnLocationMoneyChanged));
		_subscriptions.Add(Messenger<LocationLogic>.AddListener(Globals.Msg_ChestOnLocationAdded, delegate
		{
			nav0.TurnOnFlashing();
		}));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgAppsfireNotification, sidebarButton2.TurnOnFlashing));
		_subscriptions.Add(Messenger<ServerData.Item>.AddListener(Globals.MsgBagItemAdded, delegate(ServerData.Item item)
		{
			if (!item.IsElixirType())
			{
				nav1.TurnOnFlashing();
			}
		}));
		_subscriptions.Add(Messenger<int, int, string>.AddListener(Globals.MsgPlayerLevelChanged, delegate(int old, int @new, string reason)
		{
			if (@new > 1 && !HudMk1.Instance.IsLoadingPlayerStats)
			{
				nav1.TurnOnFlashing();
			}
		}));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, delegate(GuiRoot.GuiType old, GuiRoot.GuiType @new)
		{
			if (@new == GuiRoot.GuiType.BagItems || @new == GuiRoot.GuiType.BagStats)
			{
				nav1.TurnOffFlashing();
			}
			if (@new == GuiRoot.GuiType.Fight)
			{
				ServerData.Location startGameLocation = SingletonT<ServerData>.I.GetStartGameLocation();
				if (startGameLocation != null && startGameLocation.IsOpened)
				{
					nav0.SetActive();
				}
				else
				{
					nav0.SetInactive();
				}
			}
			if (Globals.DebugShowReturnInStartLocation)
			{
				nav0.SetActive();
			}
			if (@new == GuiRoot.GuiType.MainMap || @new == GuiRoot.GuiType.Location)
			{
				nav0.SetActive();
			}
		}));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgShopRecommendationsChanged, delegate
		{
			if (SingletonT<ServerData>.I.PlayerParams.Level > 1 && !HudMk1.Instance.IsLoadingPlayerStats)
			{
				nav2.TurnOnFlashing();
			}
		}));
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

	private void OnLocationMoneyChanged(ServerData.Location location, int wasMoney, int newMoney)
	{
		float num = (float)wasMoney / (float)location.MoneyMax;
		float num2 = (float)newMoney / (float)location.MoneyMax;
		if (num < 70f && num2 >= 70f)
		{
			NavButtons[0].TurnOnFlashing();
		}
	}

	public void Rearrange()
	{
		List<SidebarButton> list = new List<SidebarButton>();
		SidebarButton[] navButtons = NavButtons;
		foreach (SidebarButton sidebarButton in navButtons)
		{
			if (sidebarButton.Active)
			{
				list.Add(sidebarButton);
			}
			else
			{
				sidebarButton.transform.GoToHell();
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			SidebarButton sidebarButton2 = list[j];
			sidebarButton2.transform.localPosition = new Vector3(-4f, j * -80, -100f);
		}
		RearrangeBgs(list.Count);
	}

	private void RearrangeBgs(int count)
	{
		count -= 2;
		base.transform.localPosition = new Vector3(0f, 150f + (float)(80 * count) / 2f, -350f);
		for (int i = 0; i < ButtonBgs.Length; i++)
		{
			if (i < count)
			{
				ButtonBgs[i].localPosition = new Vector3(0f, -125 - i * 80, 0f);
			}
			else
			{
				ButtonBgs[i].GoToHell();
			}
		}
		ButtonBgs[ButtonBgs.Length - 1].localPosition = new Vector3(0f, -125 - count * 80, 0f);
	}
}
