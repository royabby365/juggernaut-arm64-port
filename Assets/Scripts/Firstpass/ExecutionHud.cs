using UnityEngine;
using Yarx;

public class ExecutionHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Transform empty;

	public Transform full;

	private void Awake()
	{
	}

	private void Start()
	{
		SetExecutionStripe(0f);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int, int>.AddListener(Globals.MsgFatalityModeSlicesChanged, OnFatalityCountChanged));
	}

	private void OnFatalityCountChanged(int current, int max)
	{
		SetExecutionStripe(Mathf.Clamp01((float)current / (float)max));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void SetExecutionStripe(float swipes)
	{
		swipes = Mathf.Clamp01(swipes);
		float x = empty.localScale.x;
		empty.renderer.material.mainTextureScale = new Vector2(x, 1f);
		float x2 = x * swipes / 1f;
		full.localScale = new Vector3(x2, 1f, 1f);
		full.renderer.material.mainTextureScale = new Vector2(x2, 1f);
	}
}
