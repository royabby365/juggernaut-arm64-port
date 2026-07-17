using System.Collections;
using Yarx;

public class FightScreenFightButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	private void Awake()
	{
		Init(24, 124);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
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
