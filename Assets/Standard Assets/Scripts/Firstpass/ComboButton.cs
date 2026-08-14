using UnityEngine;

public class ComboButton : SpriteButton
{
	public Transform backlit;

	public Transform cooldown;

	public override void SetActive()
	{
		base.SetActive();
		backlit.gameObject.SetActive(true);
		cooldown.gameObject.SetActive(false);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		backlit.gameObject.SetActive(false);
		cooldown.gameObject.SetActive(true);
	}

	private void Awake()
	{
		Init();
	}
}
