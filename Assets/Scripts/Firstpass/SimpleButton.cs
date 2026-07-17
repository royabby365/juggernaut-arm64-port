using UnityEngine;

public class SimpleButton : SpriteButton
{
	public Color darkTint = new Color(0.5f, 0.5f, 0.5f);

	public override void SetActive()
	{
		base.SetActive();
		SetColor(Color.white);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		SetColor(darkTint);
	}

	private void Awake()
	{
		Init();
	}
}
