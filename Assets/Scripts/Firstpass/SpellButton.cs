using UnityEngine;

public class SpellButton : SpriteButton
{
	private int frame_count;

	public Transform pressed;

	public Transform ready;

	public Transform enable;

	public Transform disable;

	public override void SetActive()
	{
		base.SetActive();
		pressed.gameObject.active = false;
		ready.gameObject.active = true;
		enable.gameObject.active = true;
		disable.gameObject.active = false;
	}

	public override void SetInactive()
	{
		base.SetInactive();
		pressed.gameObject.active = false;
		ready.gameObject.active = false;
		enable.gameObject.active = false;
		disable.gameObject.active = true;
	}

	public override void SetSelected()
	{
		base.SetSelected();
		pressed.gameObject.active = true;
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
		pressed.gameObject.active = false;
	}

	private void Awake()
	{
		Init();
	}

	private void Update()
	{
		frame_count--;
		if (frame_count == 0)
		{
			pressed.gameObject.active = false;
		}
	}
}
