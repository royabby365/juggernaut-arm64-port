using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Yarx;

public class FightScreenMk1 : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText ChapterText;

	public SpriteText MobText;

	public FightScreenMobIcon[] MobIcons;

	private SpriteGui _gui;

	private Dictionary<string, SpriteButton> _mobsButtons = new Dictionary<string, SpriteButton>();

	internal int _activeIndex;

	private AreaData _area;

	private static readonly string MobButtonName = "mob_icon_";

	private bool _allowSelectMob;

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

	private static int MobButtonIndex(string name)
	{
		return int.Parse(name.Substring(MobButtonName.Length));
	}

	internal void InitLocationView(int activeIndex, AreaData areaData)
	{
		_area = areaData;
		for (int i = ((!IsHideAll) ? areaData.Mobs.Length : 0); i < 10; i++)
		{
			MobIcons[i].SetState(FightScreenMobIcon.State.OutOfGui);
		}
		SetSelectedMob(activeIndex, loadMob: false);
		ChapterText.Text_ = areaData.Location.Title;
	}

	private void Awake()
	{
	}

	private void NonStart()
	{
		_gui = base.transform.GetSpriteGui();
		_gui.Release += ProcessButtons;
		FightScreenMobIcon[] mobIcons = MobIcons;
		foreach (FightScreenMobIcon fightScreenMobIcon in mobIcons)
		{
			_mobsButtons.Add(fightScreenMobIcon.name, fightScreenMobIcon);
			fightScreenMobIcon.SetInactive();
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void ProcessButtons(SpriteButton button)
	{
		string text = button.name;
		if (text.StartsWith(MobButtonName))
		{
			SetSelectedMob(MobButtonIndex(text), loadMob: true);
		}
		else if (text == "_fight_button")
		{
			Globals.Battle.StartFight(_area.Mobs[_activeIndex]);
		}
	}

	private void SetSelectedMob(int i, bool loadMob)
	{
		_activeIndex = i;
		bool isHideAll = IsHideAll;
		if (!isHideAll)
		{
			ChangeUnselectedMobIconsView();
		}
		AreaData.MobData mobData = _area.Mobs[_activeIndex];
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
				}
				else
				{
					fightScreenMobIcon.ResetBoss();
				}
				fightScreenMobIcon.SetIcon(Path.GetFileNameWithoutExtension(mobData.ServerInfo.Picture));
				if (i != _activeIndex)
				{
					fightScreenMobIcon.SetState((i >= _activeIndex) ? FightScreenMobIcon.State.Inactive : FightScreenMobIcon.State.Skull);
				}
			}
		}
	}
}
