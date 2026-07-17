using UnityEngine;
using Yarx;
using Yarx.Collections;

public class MinesLine : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Match3LootHud[] Squares;

	public Sprite Lock;

	public SpriteButton Button;

	public SpriteText ButtonLabel;

	public void SetMine(ServerData.Mine mine)
	{
		Button.gameObject.SetActiveRecursivelyMk1(setActive: true);
		if (mine.IsBuyed)
		{
			ButtonLabel.Text_ = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.Match3ButtonStart);
			return;
		}
		Tuple<ServerData.MoneyType.TypeE, int, string> price = mine.OpenPrice.GetPrice();
		ButtonLabel.Text_ = "{0} {1} {2}".Fmt(SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.Match3ButtonStart), price.Item2, price.Item3);
	}

	private void Awake()
	{
		ButtonLabel.Phrase_ = ServerData.PhrasesE.Custom;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
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
}
