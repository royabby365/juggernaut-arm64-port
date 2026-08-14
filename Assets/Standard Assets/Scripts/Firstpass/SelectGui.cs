using System;
using UnityEngine;

public class SelectGui : MonoBehaviour
{
	internal enum ModeE
	{
		InGame,
		SelectAny
	}

	internal AreaData _last;

	private GUIContent[] _contentCache;

	internal int _activeIndex;

	internal ModeE Mode;

	public Zachistka Hud;

	private Action<SpriteButton> _listenerGuiClick;

	internal FightScreen _guiBattleHud;

	private bool _generatedAtlas = true;

	private void Start()
	{
		if (Globals.BuildType != Globals.BuildTypeE.InnerRelease)
		{
			Utils.DestroyComponentThenAddNew<SelectGuiOnGUI>(base.gameObject);
		}
		if (Hud != null)
		{
			Hud.SetButtonActive("close_button");
		}
	}

	private void _gui_Click(SpriteButton button)
	{
		if (button.Name.StartsWith("frame_"))
		{
			SetActive(int.Parse(button.Name.Substring(6)));
		}
		if (button.Name == "attack_button")
		{
			Globals.Battle.StartFight(_last.Mobs[_activeIndex]);
		}
	}

	internal void SetActive(int i)
	{
		_activeIndex = i;
		if (i == -1)
		{
			if (Hud != null)
			{
				Hud.HideInfoFrame();
			}
			Globals.HideLoadingScreen();
		}
		else if (_last != null)
		{
			AreaData.MobData mobData = _last.Mobs[_activeIndex];
			if (Hud != null)
			{
				Hud.SetInfoFrameAt("frame_" + i, mobData.ServerInfo.Title, mobData.MaxHealth, mobData.Strength, mobData.Rage, mobData.Fire, mobData.Lighting, mobData.Darkness, mobData.Ice);
			}
			Globals.Battle.ChangeEnemy(mobData, delegate
			{
				Globals.HideLoadingScreen();
			});
		}
	}

	private void OnEnable()
	{
		if (!Globals.IgnoreHud)
		{
			if (HudMk1.Instance != null)
			{
				_guiBattleHud = HudMk1.Instance.GetComponent<FightScreen>();
				if (_guiBattleHud == null)
				{
					_guiBattleHud = Utils.DestroyComponentThenAddNew<FightScreen>(HudMk1.Instance.gameObject);
				}
			}
		}
		else if (_guiBattleHud == null)
		{
			GameObject gameObject = new GameObject();
			_guiBattleHud = Utils.DestroyComponentThenAddNew<FightScreen>(gameObject);
		}
		_activeIndex = SingletonT<ServerData>.I.GetLocationProgress(AreaData.Current.Location);
		if (_activeIndex >= AreaData.Current.Location.Bots.Length)
		{
			Mode = ModeE.SelectAny;
		}
		_last = null;
	}

	private void OnDisable()
	{
		if (_guiBattleHud != null)
		{
			_guiBattleHud.HideHud();
		}
	}

	private void Update()
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Fight && base.gameObject.active && !(Globals.Battle == null))
		{
			AreaData current = AreaData.Current;
			if (current != null)
			{
				DoUpdate(current, Time.deltaTime);
			}
		}
	}

	private void DoUpdate(AreaData areaData, float deltaTime)
	{
		if (_last != areaData && areaData.Mobs != null && areaData.Mobs.Length > 0)
		{
			_last = areaData;
			int locationProgress = SingletonT<ServerData>.I.GetLocationProgress(AreaData.Current.Location);
			locationProgress = ((locationProgress < AreaData.Current.Mobs.Length) ? locationProgress : (-1));
			SetActive(locationProgress);
			_guiBattleHud.InitLocationView(locationProgress, _last);
			_guiBattleHud.UnhideHud();
		}
	}

	internal void UpdateSelected()
	{
		int locationProgress = SingletonT<ServerData>.I.GetLocationProgress(AreaData.Current.Location);
		_guiBattleHud.InitLocationView(_activeIndex, AreaData.Current);
	}

	private void DestroyAllCollidersOnHud()
	{
		HudMk1 hudMk = (HudMk1)UnityEngine.Object.FindObjectOfType(typeof(HudMk1));
		if (!(hudMk == null))
		{
			Collider[] componentsInChildren = hudMk.GetComponentsInChildren<Collider>();
			Utils.Log("****JNJJJ", componentsInChildren.Length);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Collider obj = componentsInChildren[i];
				componentsInChildren[i] = null;
				UnityEngine.Object.Destroy(obj);
			}
			SingletonT<ResourcesManager>.I.UnloadUnusedAssets(this, delegate
			{
			});
		}
	}
}
