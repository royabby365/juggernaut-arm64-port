using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Yarx;

public class FightScreen : MonoBehaviour
{
	private readonly List<GuiRoot> _guis = new List<GuiRoot>();

	private GuiRoot.GuiType _currentGui = GuiRoot.GuiType.None;

	private Dictionary<string, SpriteButton> _mobsButtons = new Dictionary<string, SpriteButton>();

	public SpriteText ChapterText;

	public SpriteText MobText;

	public FightScreenMobIcon[] MobIcons;

	public GameObject FightButton;

	public GameObject TextRoot;

	private bool _allowSelectMob;

	private static readonly string MobButtonName = "mob_icon_";

	private CompositeDisposable _messageHandlers;

	private int _activeIndex;

	private AreaData _area;

	public bool AllowSelectMob
	{
		get
		{
			return _allowSelectMob;
		}
		set
		{
			if (_allowSelectMob == value)
			{
				return;
			}
			_allowSelectMob = value;
			foreach (SpriteButton value2 in _mobsButtons.Values)
			{
				if (value)
				{
					value2.SetActive();
				}
				else
				{
					value2.SetInactive();
				}
			}
		}
	}

	internal bool IsHideAll
	{
		get
		{
			if (_area == null)
			{
				return true;
			}
			return _area.Mobs.Length == 1 && _area.Mobs[0].IsBoss;
		}
	}

	internal int ActiveMobIndex
	{
		get
		{
			return _activeIndex;
		}
		set
		{
			Utils.LogForce("**ACTIVE MOB INDEX", _activeIndex, "->", value);
			_activeIndex = value;
		}
	}

	internal void InitLocationView(int activeIndex, AreaData areaData)
	{
		_area = areaData;
		for (int i = ((!IsHideAll) ? areaData.Mobs.Length : 0); i < 10; i++)
		{
			if (MobIcons[i] != null)
			{
				MobIcons[i].SetState(FightScreenMobIcon.State.OutOfGui);
			}
		}
		if (IsHideAll)
		{
			Messenger.Invoke(Globals.ChapterPrizeHandler, 0);
		}
		SetSelectedMob(activeIndex, loadMob: false);
		TextRoot.SetActiveRecursivelyMk1(ActiveMobIndex > -1);
		FightButton.SetActiveRecursivelyMk1(ActiveMobIndex > -1);
		ChapterText.Text_ = areaData.Location.Title;
	}

	private static int MobButtonIndex(string name)
	{
		return int.Parse(name.Substring(MobButtonName.Length));
	}

	private void OnEnable()
	{
		_messageHandlers = new CompositeDisposable();
		_messageHandlers.Add(Messenger<string>.AddListener(Globals.MsgGuiButtonPressed, ProcessButtons));
		_messageHandlers.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
	}

	private void OnDisable()
	{
		Utils.Dispose(ref _messageHandlers);
		_area = null;
	}

	private void Start()
	{
		foreach (SpriteButton value in _mobsButtons.Values)
		{
			value.SetInactive();
		}
		if (!Globals.IgnoreHud)
		{
			MobText = HudMk1.Instance.transform.FindChildByName("mob_text").GetComponent<SpriteText>();
			ChapterText = HudMk1.Instance.transform.FindChildByName("chapter_text").GetComponent<SpriteText>();
		}
	}

	private void Update()
	{
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		TextRoot.SetActiveRecursivelyMk1(ActiveMobIndex > -1);
		FightButton.SetActiveRecursivelyMk1(ActiveMobIndex > -1);
	}

	private void SetSelectedMob(int i, bool loadMob)
	{
		if (i < 0 || i >= _area.Mobs.Length)
		{
			ActiveMobIndex = -1;
			return;
		}
		ActiveMobIndex = i;
		bool isHideAll = IsHideAll;
		if (!isHideAll)
		{
			ChangeUnselectedMobIconsView();
		}
		Utils.Log("**** SetSelectedMob", _area.Mobs.Length, _activeIndex);
		AreaData.MobData mobData = _area.Mobs[ActiveMobIndex];
		if (!isHideAll)
		{
			MobIcons[i].SetState(FightScreenMobIcon.State.Active);
		}
		StringBuilder stringBuilder = new StringBuilder(SingletonT<ServerData>.I.GetPhrase(ServerData.PhraseInBattleFightWith) + " " + _area.Mobs[i].ServerInfo.Title);
		if (mobData.Darkness || mobData.Lighting || mobData.Fire || mobData.Ice)
		{
			stringBuilder.Append(" [ ");
			if (mobData.Darkness)
			{
				stringBuilder.Append(Globals.CharIconDark);
			}
			if (mobData.Lighting)
			{
				stringBuilder.Append(Globals.CharIconElectro);
			}
			if (mobData.Fire)
			{
				stringBuilder.Append(Globals.CharIconFire);
			}
			if (mobData.Ice)
			{
				stringBuilder.Append(Globals.CharIconIce);
			}
			stringBuilder.Append(" ]");
		}
		MobText.Text_ = stringBuilder.ToString();
		if (loadMob)
		{
			Globals.Battle.ChangeEnemy(mobData, delegate
			{
				Globals.HideLoadingScreen();
			});
		}
	}

	private void ChangeUnselectedMobIconsView()
	{
		for (int i = 0; i < _area.Mobs.Length; i++)
		{
			AreaData.MobData mobData = _area.Mobs[i];
			if (i < MobIcons.Length)
			{
				FightScreenMobIcon fightScreenMobIcon = MobIcons[i];
				if (mobData.IsBoss)
				{
					fightScreenMobIcon.SetMeBoss();
					Messenger.Invoke(Globals.ChapterPrizeHandler, i);
				}
				else
				{
					fightScreenMobIcon.ResetBoss();
				}
				fightScreenMobIcon.SetIcon(Path.GetFileNameWithoutExtension(mobData.ServerInfo.Picture));
				if (i != ActiveMobIndex)
				{
					fightScreenMobIcon.SetState((i >= ActiveMobIndex) ? FightScreenMobIcon.State.Inactive : FightScreenMobIcon.State.Skull);
				}
			}
		}
	}

	private void ProcessButtons(string buttonName)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (buttonName.StartsWith(MobButtonName))
		{
			SetSelectedMob(MobButtonIndex(buttonName), loadMob: true);
		}
		else if (buttonName == "_fight_button" && Globals.Enemy != null)
		{
			FightOnLocationHud componentInChildren = HudMk1.Instance.GetComponentInChildren<FightOnLocationHud>();
			if (Globals.MainMenu._lastLocationOrMapOrZachistka == GuiRoot.GuiType.Location && componentInChildren != null && !Globals.Nav0GoToZachistka)
			{
				Globals.Battle.StartFight(componentInChildren.MobData);
				return;
			}
			Globals.Nav0GoToZachistka = false;
			Globals.Battle.StartFight(_area.Mobs[ActiveMobIndex]);
		}
	}

	public void HideHud()
	{
	}

	public void UnhideHud()
	{
	}
}
