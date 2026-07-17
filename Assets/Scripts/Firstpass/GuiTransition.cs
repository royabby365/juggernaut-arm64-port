using System.Collections;
using UnityEngine;
using Yarx;

public class GuiTransition : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public AnimationCurve Curve;

	public float TransitionTime = 1f;

	private readonly Vector3 _offPosition = new Vector3(-3000f, 0f, 0f);

	public float ShowTransition()
	{
		StartCoroutine("TransitionCoro");
		return TransitionTime;
	}

	private IEnumerator TransitionCoro()
	{
		base.transform.localScale = Vector3.zero;
		base.transform.localPosition = Vector3.zero;
		float startTime = Time.time;
		bool stop = false;
		while (!stop)
		{
			float dt = Time.time - startTime;
			if (dt >= TransitionTime)
			{
				stop = true;
			}
			float xx = Curve.Evaluate(dt / TransitionTime);
			base.transform.localScale = new Vector3(xx, xx, 1f);
			yield return null;
		}
		base.transform.localPosition = _offPosition;
		base.transform.localScale = Vector3.zero;
	}

	private void Awake()
	{
		base.transform.localPosition = _offPosition;
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
