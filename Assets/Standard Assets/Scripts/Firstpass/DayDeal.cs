using UnityEngine;
using Yarx;

public class DayDeal : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText Discount;

	private void Awake()
	{
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
