using UnityEngine;
using Yarx;

public class AchievementBigProgressBar : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Transform empty;

	public Transform full;

	public int StartScale = 140;

	public SpriteText ProgressCount;

	public void SetIndicator(int current, int max)
	{
		ProgressCount.Text_ = "{0}/{1}".Fmt(current, max);
		SetStripe((max <= 0) ? 0f : ((float)current / (float)max));
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
		float x2 = x * progress / 1f;
		full.localScale = new Vector3(x2, 1f, 1f);
		full.GetComponent<Renderer>().material.mainTextureScale = new Vector2(x2, 1f);
	}
}
