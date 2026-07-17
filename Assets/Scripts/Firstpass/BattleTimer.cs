using UnityEngine;

public class BattleTimer : MonoBehaviour
{
	public Vector3 activePosition = new Vector3(-512f, -384f, 0f);

	public Transform empty;

	public Transform full;

	public Vector3 passivePosition = new Vector3(-512f, 1000f, 0f);

	public float startSwipes;

	public void SetExecution(int swipes, int maxswipes)
	{
		float stripe = (float)swipes / (float)maxswipes;
		SetStripe(stripe);
	}

	public void HideBattleTimer()
	{
		base.transform.localPosition = passivePosition;
	}

	public void ShowBattleTimer()
	{
		base.transform.localPosition = activePosition;
	}

	public void SetStripe(float progress)
	{
		progress = Mathf.Clamp01(progress);
		float x = empty.localScale.x;
		empty.renderer.material.mainTextureScale = new Vector2(x, 1f);
		float x2 = x * progress / 1f;
		full.localScale = new Vector3(x2, 1f, 1f);
		full.renderer.material.mainTextureScale = new Vector2(x2, 1f);
	}

	private void Awake()
	{
	}

	private void Start()
	{
		SetStripe(startSwipes);
	}
}
