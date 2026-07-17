using UnityEngine;
using Yarx;

public class FightTimer : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Transform empty;

	public Transform full;

	public int Shift = 108;

	private void Start()
	{
		int num = (Camera2D.ScreenWidth - 2 * Shift) / 4;
		empty.localScale = new Vector3(num, 1f, 1f);
		base.transform.localPosition = new Vector3(-2 * num, -14f, 100f);
		SetStripe(1f);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<float>.AddListener(Globals.MsgGuiBattle_Timer, OnBattleTimerChanged));
	}

	private void OnBattleTimerChanged(float f)
	{
		SetStripe(f);
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void SetStripe(float progress)
	{
		progress = Mathf.Clamp01(progress);
		if (progress < 0.02f)
		{
			progress = 0f;
		}
		float x = empty.localScale.x;
		if (empty.renderer != null)
		{
			empty.renderer.material.mainTextureScale = new Vector2(x, 1f);
		}
		float x2 = x * progress / 1f;
		full.localScale = new Vector3(x2, 1f, 1f);
		if (full.renderer != null)
		{
			full.renderer.material.mainTextureScale = new Vector2(x2, 1f);
		}
	}
}
