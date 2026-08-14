using UnityEngine;

internal class Bubbles
{
	public static float _unlockHudBubblesTime;

	public bool HasKilledBubbles => _unlockHudBubblesTime > 0f;

	public void SpawnBubbles(Battle battle, bool critical, bool fromRages, bool isCombo)
	{
		if (!Globals.ForceDontSpawnManaBalls && SingletonT<ServerData>.I.IsMagicOpened)
		{
			int manaBallsCount = 0;
			int manaPerBall = 0;
			SingletonT<ServerData>.I.GetMana(SingletonT<ServerData>.I.PlayerInBattleParams, SingletonT<ServerData>.I.BattleMathParams, critical, fromRages, isCombo, out manaBallsCount, out manaPerBall);
			if (Globals.PlayerAttackSpawnOneMagicBall)
			{
				Globals.PlayerAttackSpawnOneMagicBall = false;
				manaBallsCount = 1;
				Messenger.Invoke(Globals.MsgPlayerAttackSpawnOneMagicBall);
			}
			SpawnBubbles(battle, Globals.Enemy, manaBallsCount, manaPerBall, fromRages);
		}
	}

	public void SpawnBubblesFromPlayer(Battle battle, Person person, int manaBallsCount, int manaPerBall)
	{
		SpawnBubbles(battle, person, manaBallsCount, manaPerBall, fromRages: false);
	}

	private void SpawnBubbles(Battle battle, Person person, int manaBallsCount, int manaPerBall, bool fromRages)
	{
		if (Globals.ForceDontSpawnManaBalls || !SingletonT<ServerData>.I.IsMagicOpened)
		{
			return;
		}
		Utils.Log("**SpawnBubble", manaBallsCount);
		GameObject[] array = new GameObject[2] { battle.BubbleBig, battle.BubbleSmall };
		GameObject[] bubblesPrototypeSelect = new GameObject[2] { battle.BubbleBigSelect, battle.BubbleSmallSelect };
		Vector3 position = person.transform.position;
		for (int i = 0; i < manaBallsCount; i++)
		{
			int bi = Random.Range(0, array.Length);
			Utils.NewWithOffset(array[bi], position, 0f, 0f, 0f, delegate(GameObject bubble)
			{
				bubble.name = "Bubble";
				Bubble bubble2 = bubble.AddComponent<Bubble>();
				bubble2.ManaPoints = manaPerBall;
				bubble2.Battle = battle;
				bubble2.Target = person.gameObject;
				bubble2.SelectFx = bubblesPrototypeSelect[bi];
				if (!fromRages && person.Equals(Globals.Enemy))
				{
					Messenger.Invoke(Globals.MsgSpawnManaBubbleFromEnemy);
				}
			});
		}
	}

	public void SpawnRageBubble(Battle battle, Person person)
	{
		if (!Globals.ForceDontSpawnRageBalls && SingletonT<ServerData>.I.IsRageOpened)
		{
			Utils.Log("**SpawnRageBubble");
			Vector3 position = person.transform.position;
			Utils.NewWithOffset(Globals.Battle.RageBubblePrefab, position, 0f, 0f, 0f, delegate(GameObject bubble)
			{
				bubble.name = "RageBubble";
				Bubble bubble2 = bubble.AddComponent<Bubble>();
				bubble2.Type = Bubble.TypeE.Rage;
				bubble2.Battle = battle;
				bubble2.Target = person.gameObject;
				bubble2.SelectFx = Globals.Battle.RageBubblePrefabExpl;
				Messenger.Invoke(Globals.MsgSpawnRageBubbleFromEnemy);
			});
		}
	}

	public void ProcessBubbles(Battle battle)
	{
	}

	public void Update(Battle battle)
	{
		if (_unlockHudBubblesTime > 0f)
		{
			_unlockHudBubblesTime -= Time.deltaTime;
			if (_unlockHudBubblesTime <= 0f)
			{
				Messenger<bool>.Invoke(Globals.MsgGuiBattle_SetMagicBarVisible, arg1: true);
			}
		}
	}

	internal void DestroyAll()
	{
		Bubble.DestroyAll();
	}
}
