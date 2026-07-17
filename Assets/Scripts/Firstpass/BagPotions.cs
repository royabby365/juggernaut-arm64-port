using System.Collections;
using UnityEngine;
using Yarx;

public class BagPotions : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText HealsCount;

	public SpriteText DoubleDamageCount;

	public SpriteText PoisonsCount;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.Item.ElixirTypeE, int>.AddListener(Globals.MsgElixirCountChanged, OnItemChanged));
	}

	private void OnItemChanged(ServerData.Item.ElixirTypeE unused1, int count)
	{
		int playerElixirsCount = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Heal);
		int playerElixirsCount2 = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Critical);
		int playerElixirsCount3 = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Poison);
		HealsCount.Text_ = playerElixirsCount.ToString();
		DoubleDamageCount.Text_ = playerElixirsCount2.ToString();
		PoisonsCount.Text_ = playerElixirsCount3.ToString();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private IEnumerator OnBecameVisible()
	{
		for (int i = 0; i < 2; i++)
		{
			yield return null;
		}
	}

	private void OnBecameInvisible()
	{
	}
}
