using UnityEngine;

public class ProgressBar : MonoBehaviour
{
	public Transform empty;

	public Transform full;

	public Vector3 activePosition;

	public Vector3 passivePosition;

	public float startState;

	public void SetStripe(float fraction)
	{
		if (fraction.Eqv(0f))
		{
			Hide();
		}
		else
		{
			Show();
		}
		fraction = Mathf.Clamp01(fraction);
		startState = fraction;
		float x = empty.localScale.x;
		float x2 = x * fraction;
		full.localScale = new Vector3(x2, 1f, 1f);
		full.renderer.material.mainTextureScale = new Vector2(x2, 1f);
	}

	public void Hide()
	{
		base.transform.parent.localPosition = passivePosition;
	}

	public void Show()
	{
		base.transform.parent.localPosition = activePosition;
	}

	private void Awake()
	{
	}

	private void Start()
	{
		SetStripe(startState);
	}
}
