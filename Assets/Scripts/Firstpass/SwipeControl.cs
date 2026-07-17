using UnityEngine;
using Yarx;

public class SwipeControl : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

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
