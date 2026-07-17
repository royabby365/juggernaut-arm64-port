using UnityEngine;
using Yarx;

public class NewPopupMk2 : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Sprite LeftPane;

	public Sprite RightPane;

	public Sprite Ico;

	public int MinHeight = 100;

	public int DeltaHeight = 47;

	public int DebugHeight = 100;

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
		SetHeight(DebugHeight);
	}

	private void SetHeight(int height)
	{
		height = Mathf.Max(height, MinHeight);
		LeftPane.Height = height;
		LeftPane.Refresh();
		RightPane.Height = height - DeltaHeight;
		RightPane.Refresh();
	}
}
