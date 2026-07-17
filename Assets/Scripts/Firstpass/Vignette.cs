using System.Collections;
using UnityEngine;
using Yarx;

public class Vignette : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public AnimationCurve AlphaCurve;

	public float howLong = 2f;

	public Color ClearColor = new Color(0f, 0f, 0f, 0f);

	public Color Tint = new Color(0.5f, 0f, 0f, 0f);

	private Mesh _mesh;

	private Color _color;

	public void StartFx()
	{
		StartCoroutine("AnimateVignette");
	}

	private void Awake()
	{
		_color = ClearColor;
		_mesh = base.transform.GetComponent<MeshFilter>().mesh;
		_mesh.SetTint(_color);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		base.transform.GetComponent<MeshRenderer>().enabled = false;
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
		base.transform.GetComponent<MeshRenderer>().enabled = false;
	}

	private void Start()
	{
	}

	private IEnumerator OnBecameVisible()
	{
		for (int i = 0; i < 2; i++)
		{
			yield return null;
			if ((bool)_mesh)
			{
				_mesh.SetTint(_color);
			}
		}
	}

	private void OnBecameInvisible()
	{
		base.transform.GetComponent<MeshRenderer>().enabled = false;
	}

	private IEnumerator AnimateVignette()
	{
		float start = Time.time;
		base.transform.GetComponent<MeshRenderer>().enabled = true;
		while (Time.time - start < howLong)
		{
			float dt = Mathf.Clamp01((Time.time - start) / howLong);
			_color = new Color(Tint.r, Tint.g, Tint.b, Mathf.Clamp01(AlphaCurve.Evaluate(dt)));
			_mesh.SetTint(_color);
			yield return null;
		}
		_color = new Color(Tint.r, Tint.g, Tint.b, 0f);
		base.transform.GetComponent<MeshRenderer>().enabled = false;
	}
}
