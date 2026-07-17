using UnityEngine;

public class FightScreenFightButtonAnimate : MonoBehaviour
{
	public Transform FightButton;

	public Transform FightButtonLabel;

	public float RotationSpeed = 0.01f;

	public AnimationCurve WobbleButton;

	public float WobbleButtonK = 0.2f;

	public AnimationCurve WobbleLabel;

	private bool _stopAnimation;

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void OnBecameVisible()
	{
		_stopAnimation = false;
	}

	private void OnBecameInvisible()
	{
		_stopAnimation = true;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!_stopAnimation)
		{
			FightButton.RotateAround(Vector3.forward, RotationSpeed);
			float num = WobbleButton.Evaluate(Time.time);
			FightButton.localScale = new Vector3(1.1f + WobbleButtonK * num, 1.1f + WobbleButtonK * num, 1f);
			float num2 = WobbleLabel.Evaluate(Time.time + 0.5f);
			FightButtonLabel.localScale = new Vector3(num2, num2, 0f);
			FightButtonLabel.localScale *= WobbleButtonK;
			FightButtonLabel.localScale += new Vector3(1f, 1f, 1f);
		}
	}
}
