using UnityEngine;
using Yarx.Collections;

internal class MainMapOnGui : MonoBehaviour
{
	private MainMapHud _parent;

	private void Start()
	{
		_parent = base.gameObject.GetComponentInChildren<MainMapHud>();
	}

	private void OnGUI()
	{
		if (!(_parent == null) && !(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.MainMap)
		{
			int num = 60;
			if (GUI.Button(new Rect(10f, Screen.height - 70 - num, 180f, 30f), (!Globals.ForceShowAllLocationsOnMap) ? "Show all zones" : "Hide zones"))
			{
				Globals.ForceShowAllLocationsOnMap = !Globals.ForceShowAllLocationsOnMap;
				_parent.RefreshLocationButtons();
			}
			if (GUI.Button(new Rect(10f, Screen.height - 100 - num, 180f, 30f), (!Globals.ForceShowAllLocations) ? "Show all locations" : "Hide locations"))
			{
				Globals.ForceShowAllLocations = !Globals.ForceShowAllLocations;
				_parent.RefreshLocationButtons();
			}
			if (GUI.Button(new Rect(10f, Screen.height - 130 - num, 180f, 30f), "Show skill"))
			{
				SkillBonusHud componentInChildren = HudMk1.Instance.GetComponentInChildren<SkillBonusHud>();
				componentInChildren.Init(SkillBonusHud.SkillBonusTypeE.Combo);
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.SkillBonus);
			}
			if (GUI.Button(new Rect(10f, Screen.height - 160 - num, 180f, 30f), "Show final"))
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Final);
			}
			if (GUI.Button(new Rect(10f, Screen.height - 190 - num, 180f, 30f), "Show achievement"))
			{
				AchievmentsHud componentInChildren2 = HudMk1.Instance.GetComponentInChildren<AchievmentsHud>();
				componentInChildren2.Init(MainMenu.GameEvents.Events[0]);
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Achievments);
			}
			if (GUI.Button(new Rect(10f, Screen.height - 220 - num, 180f, 30f), "Show match3 easy"))
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Match3StartScreen, Tuple.Create("???????????????? ????????????????"));
			}
			if (GUI.Button(new Rect(10f, Screen.height - 250 - num, 180f, 30f), "Show match3 normal"))
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Match3StartScreen, Tuple.Create("???????????????? ????????????????"));
			}
		}
	}
}
