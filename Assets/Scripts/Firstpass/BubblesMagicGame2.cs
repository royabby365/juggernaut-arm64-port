using System.Collections.Generic;
using UnityEngine;

internal class BubblesMagicGame2
{
	internal List<BubbleMagicGame2> _magicGame2List;

	private float _time;

	internal int _count;

	public void DestroyAll()
	{
		if (_magicGame2List == null)
		{
			return;
		}
		foreach (BubbleMagicGame2 magicGame in _magicGame2List)
		{
			magicGame.Destroy();
		}
		_magicGame2List.Clear();
	}

	public void SpawnBubbles(int count)
	{
		Battle battle = Globals.Battle;
		_time = SingletonT<ServerData>.I.GameSettings.mg2Time;
		_count = count;
		Messenger.Invoke(Globals.MsgFatalityModeSlicesChanged, 0, count);
		battle.ResumeTurnTime("SpawnBubbles");
		battle._invokeEndMiniGame = true;
		battle.State = Battle.StateE.WaitEnemyMiniGameEnd;
		battle.TimeRemainsTillTheEndOfState = SingletonT<ServerData>.I.GameSettings.mg2Time;
		_magicGame2List = new List<BubbleMagicGame2>(count);
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(battle.MagicWeakBubblePregab, Globals.Enemy.transform.position, Quaternion.identity);
			BubbleMagicGame2 bubbleMagicGame = gameObject.AddComponent<BubbleMagicGame2>();
			bubbleMagicGame.Speed = 0.8f;
			bubbleMagicGame.A = 0.15f;
			gameObject.transform.Translate(new Vector3(Random.Range(0f, 0.5f), Random.Range(0f, 0.5f), Random.Range(0f, 0.5f)));
			gameObject.transform.eulerAngles = new Vector3(Random.Range(-75f, 75f), Random.Range(-75f, 75f), 0f);
			_magicGame2List.Add(bubbleMagicGame);
		}
	}

	private void ProcessBubbles(Battle battle)
	{
		bool flag = false;
	}

	public void Update(Battle battle)
	{
		if (!(_time > 0f))
		{
			return;
		}
		if (!Globals.ForceWeakMagicNoTimeLimit)
		{
			_time -= Time.deltaTime;
		}
		if (_time > 0f)
		{
			ProcessBubbles(battle);
		}
		else
		{
			if (_magicGame2List == null || _magicGame2List.Count <= 0)
			{
				return;
			}
			foreach (BubbleMagicGame2 magicGame in _magicGame2List)
			{
				magicGame.Destroy();
			}
			_magicGame2List = null;
		}
	}
}
