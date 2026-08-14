using UnityEngine;
using Yarx;

public class AnkhHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Sprite Ankh;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int>.AddListener(Globals.MsgResurrectionCountChanged, OnResurrectionCountChanged));
	}

	private void OnResurrectionCountChanged(int count)
	{
		Vector3 localPosition = Ankh.transform.localPosition;
		int num = ((count <= 0) ? 1024 : 0);
		Ankh.transform.localPosition = new Vector3(localPosition.x, num, localPosition.z);
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}
}
