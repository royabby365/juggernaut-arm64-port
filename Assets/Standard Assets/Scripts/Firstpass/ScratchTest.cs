using System.Collections;
using UnityEngine;
using Yarx;

public class ScratchTest : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

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
}
