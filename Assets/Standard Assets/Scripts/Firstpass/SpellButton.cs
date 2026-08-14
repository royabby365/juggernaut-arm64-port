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
		pressed.gameObject.SetActive(false);
		ready.gameObject.SetActive(true);
		enable.gameObject.SetActive(true);
		disable.gameObject.SetActive(false);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		pressed.gameObject.SetActive(false);
		ready.gameObject.SetActive(false);
		enable.gameObject.SetActive(false);
		disable.gameObject.SetActive(true);
	}

	public override void SetSelected()
	{
		base.SetSelected();
		pressed.gameObject.SetActive(true);
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
		pressed.gameObject.SetActive(false);
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
			pressed.gameObject.SetActive(false);
		}
	}
}
