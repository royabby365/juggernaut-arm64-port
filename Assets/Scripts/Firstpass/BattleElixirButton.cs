using UnityEngine;
using Yarx;

public class BattleElixirButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	public override void Entered()
	{
		base.Entered();
		base.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
	}

	public override void Left()
	{
		base.Left();
		base.transform.localScale = Vector3.one;
	}
}
