using UnityEngine;

public class ComboButton : SpriteButton
{
	public Transform backlit;

	public Transform cooldown;

	public override void SetActive()
	{
		base.SetActive();
		backlit.gameObject.active = true;
		cooldown.gameObject.active = false;
	}

	public override void SetInactive()
	{
		base.SetInactive();
		backlit.gameObject.active = false;
		cooldown.gameObject.active = true;
	}

	private void Awake()
	{
		Init();
	}
}
