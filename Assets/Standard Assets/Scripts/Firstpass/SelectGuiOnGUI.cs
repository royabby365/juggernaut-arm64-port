using System;
using System.Collections.Generic;
using UnityEngine;

internal class SelectGuiOnGUI : MonoBehaviour
{
	private SelectGui _parent;

	private void Start()
	{
		_parent = base.transform.root.GetComponent<SelectGui>();
	}

	private void OnGUI()
	{
		if (_parent == null || HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Fight || Globals.BuildType == Globals.BuildTypeE.Inner || Globals.BuildType == Globals.BuildTypeE.InnerRelease)
		{
			return;
		}
		SelectGui.ModeE mode = _parent.Mode;
		FightScreen guiBattleHud = _parent._guiBattleHud;
		int num = 90;
		float num2 = 160f;
		float num3 = 30f;
		if (GUI.Button(new Rect(10f, (float)Screen.height - num2 - (float)num, 180f, 30f), "Destroy gui"))
		{
			List<GameObject> list = new List<GameObject>();
			MonoBehaviour[] componentsInChildren = HudMk1.Instance.GetComponentsInChildren<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in componentsInChildren)
			{
				if (monoBehaviour.transform.parent != null && monoBehaviour.transform.parent.Equals(HudMk1.Instance.transform) && !monoBehaviour.name.StartsWith("battle_hud") && !list.Contains(monoBehaviour.gameObject))
				{
					list.Add(monoBehaviour.gameObject);
				}
			}
			foreach (GameObject item in list)
			{
				Utils.LogForce(item.name);
				item.transform.parent = null;
				UnityEngine.Object.Destroy(item);
			}
		}
		num2 -= num3;
		if (GUI.Button(new Rect(10f, (float)Screen.height - num2 - (float)num, 180f, 30f), "UnloadUnusedAssets"))
		{
		}
		num2 -= num3;
		if (GUI.Button(new Rect(10f, (float)Screen.height - num2 - (float)num, 180f, 30f), "GC.Collect"))
		{
			GC.GetTotalMemory(forceFullCollection: true);
		}
		num2 -= num3;
		if (GUI.Button(new Rect(10f, (float)Screen.height - num2 - (float)num, 180f, 30f), (mode != SelectGui.ModeE.SelectAny) ? "Switch to select mode" : "Switch to game mode"))
		{
			if (mode == SelectGui.ModeE.InGame)
			{
				FightScreenMobIcon[] mobIcons = guiBattleHud.MobIcons;
				foreach (FightScreenMobIcon fightScreenMobIcon in mobIcons)
				{
					fightScreenMobIcon.SetActive();
				}
				guiBattleHud.AllowSelectMob = true;
				mode = SelectGui.ModeE.SelectAny;
			}
			else
			{
				FightScreenMobIcon[] mobIcons2 = guiBattleHud.MobIcons;
				foreach (FightScreenMobIcon fightScreenMobIcon2 in mobIcons2)
				{
					fightScreenMobIcon2.SetInactive();
				}
				guiBattleHud.AllowSelectMob = false;
				mode = SelectGui.ModeE.InGame;
			}
			_parent._last = null;
		}
		num2 -= num3;
		if (GUI.Button(new Rect(10f, (float)Screen.height - num2 - (float)num, 180f, 30f), "Reset to start"))
		{
			SingletonT<ServerData>.I.ResetLocationProgress(AreaData.Current.Location);
			_parent._last = null;
		}
	}
}
