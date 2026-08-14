using UnityEngine;

internal class FatalityMode
{
	private static string[] _fatalityBloodFxs = new string[2] { "ifx_blood", "ifx_blood_splash" };

	private int _slicesCount;

	private int _slicesToBloodCount;

	private int _slicesToBlood = 4;

	private readonly int _slicesToExecureFatality;

	private Battle _battle;

	private readonly AttackE _attack;

	private readonly ReactE _react;

	private readonly int _damage;

	public float TimeRemains { get; private set; }

	internal FatalityMode(Battle battle, int slicesCount, float waitTime, AttackE attack, ReactE react, int damage)
	{
		_battle = battle;
		_slicesCount = 0;
		_slicesToBloodCount = 0;
		slicesCount = (int)(0.7f * (float)slicesCount);
		_slicesToExecureFatality = slicesCount;
		TimeRemains = waitTime;
		_attack = attack;
		_react = react;
		_damage = damage;
		battle.IPadTouchscreen.OnSlice += IncFatalitySlicesCount;
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Execution);
		}
	}

	internal void Destroy(ref FatalityMode self)
	{
		if (self == this && _battle != null)
		{
			_battle.IPadTouchscreen.OnSlice -= IncFatalitySlicesCount;
			_battle = null;
			self = null;
		}
	}

	internal void Update(float dt)
	{
		if (!(HudMk1.Instance == null) && !(TimeRemains < 0f) && !(_battle == null))
		{
			if (!Globals.ForceFatalityNoTimeLimit)
			{
				TimeRemains -= dt;
			}
			if (!(TimeRemains > 0f) && _battle.State == Battle.StateE.FatalityMode)
			{
				Globals.Battle._fatalityExecuted = Battle.FatalityStateE.Undone;
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BattleHud);
				Messenger.Invoke(Globals.MsgGuiBattle_ShowFatalityBar, arg1: false);
				Globals.Player.MakeVictoryIdle();
				Globals.Battle.ExecuteFatalityDelayedPlayerAttack(_attack);
				_battle.IPadTouchscreen.OnSlice -= IncFatalitySlicesCount;
			}
		}
	}

	private void IncFatalitySlicesCount(float clockAngle)
	{
		if (HudMk1.Instance == null || _battle.State != Battle.StateE.FatalityMode || _slicesCount >= _slicesToExecureFatality)
		{
			return;
		}
		_slicesCount++;
		SingletonT<SoundManager>.I.PlayGlobalSound("slicefatality" + Random.Range(1, 4));
		Messenger<int, int>.Invoke(Globals.MsgFatalityModeSlicesChanged, _slicesCount, _slicesToExecureFatality);
		_slicesToBloodCount++;
		if (_slicesToBloodCount == _slicesToBlood)
		{
			Globals.Enemy.PlayScenario(_battle, _fatalityBloodFxs[Random.Range(0, _fatalityBloodFxs.Length)]);
			_slicesToBloodCount = 0;
		}
		if (_slicesCount != _slicesToExecureFatality)
		{
			return;
		}
		TimeRemains = 0f;
		ServerData.Item item = SingletonT<ServerData>.I.FindInBag((ServerData.Item _) => _.PutOn && _.ElixirType == ServerData.Item.ElixirTypeE.None && _.Slot != null && _.Slot.SlotId == ServerData.Slot.TypeE.Weapon);
		ServerData.Fatality defaultFatality = SingletonT<ServerData>.I.GameSettings.DefaultFatality;
		string scenario = ((defaultFatality == null || string.IsNullOrEmpty(defaultFatality.Scenario)) ? "fatality1" : defaultFatality.Scenario);
		if (item != null)
		{
			if (item.FatalityScenarioName != null && !string.IsNullOrEmpty(item.FatalityScenarioName.Scenario))
			{
				scenario = item.FatalityScenarioName.Scenario;
			}
			else
			{
				ServerData.Item itemByServerId = SingletonT<ServerData>.I.GetItemByServerId(item.Id);
				if (itemByServerId != null && itemByServerId.FatalityScenarioName != null && !string.IsNullOrEmpty(itemByServerId.FatalityScenarioName.Scenario))
				{
					scenario = itemByServerId.FatalityScenarioName.Scenario;
				}
			}
		}
		HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.BattleHud);
		Messenger.Invoke(Globals.MsgFatalityExecuted);
		Globals.Player.PlayFatality(scenario, delegate
		{
			Globals.Player.AnimationPlayed("-");
			_battle.State = Battle.StateE.ShowFinishDialog;
		});
		Globals.Battle.State = Battle.StateE.FatalityModeExecute;
	}
}
