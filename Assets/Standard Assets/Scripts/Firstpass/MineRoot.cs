using UnityEngine;
using Yarx;

public class MineRoot : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public MinesLine[] Lines;

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
