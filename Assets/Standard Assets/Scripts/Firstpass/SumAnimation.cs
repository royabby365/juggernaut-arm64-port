using UnityEngine;
using Yarx;

public class SumAnimation : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText Count;

	public Color TextColor;

	public Transform CoinIco;

	public AnimationCurve alpha;

	public AnimationCurve scale;

	public float howLong = 3f;

	private float _start;

	public void Init(string count)
	{
		Count.Text_ = "+" + count;
	}

	private void SetAlpha(float a)
	{
		a = Mathf.Clamp01(a);
		Color color = new Color(1f, 1f, 1f, a);
		Util2D.SetTint(color: new Color(TextColor.r, TextColor.g, TextColor.b, a), mesh: Count.transform.GetComponent<MeshFilter>().mesh);
		CoinIco.GetComponent<MeshFilter>().mesh.SetTint(color);
	}

	private void SetScale(float s)
	{
		Count.transform.localScale = new Vector3(s, s, s);
	}

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
		_start = Time.time;
		SetScale(scale.Evaluate(0.001f));
	}

	private void Update()
	{
		float time = Time.time;
		if (time >= _start + howLong)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		float time2 = (time - _start) / howLong;
		SetAlpha(alpha.Evaluate(time2));
		SetScale(scale.Evaluate(time2));
	}
}
