using UnityEngine;
using Yarx;

public class Match3ProgressBar : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Transform empty;

	public Transform full;

	public int StartScale = 150;

	public void SetIndicator(int current, int max)
	{
		SetStripe((float)current / (float)max);
	}

	private void Awake()
	{
		empty.localScale = new Vector3(StartScale, 1f, 1f);
		SetStripe(1f);
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

	private void SetStripe(float progress)
	{
		progress = Mathf.Clamp01(progress);
		if (progress < 0.02f)
		{
			progress = 0f;
		}
		float x = empty.localScale.x;
		empty.GetComponent<Renderer>().material.mainTextureScale = new Vector2(x, 1f);
		float num = x * progress / 1f;
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		full.localScale = new Vector3(num, 1f, 1f);
		full.GetComponent<Renderer>().material.mainTextureScale = new Vector2(num, 1f);
	}
}
