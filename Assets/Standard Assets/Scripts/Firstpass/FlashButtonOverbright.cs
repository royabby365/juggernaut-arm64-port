using UnityEngine;

public class FlashButtonOverbright : SpriteButton
{
	public Transform normal;

	public Color normTint = new Color(0.5f, 0.5f, 0.5f);

	public Color darkTint = new Color(0.7f, 0.7f, 0.7f);

	public Color liteTint = new Color(0.3f, 0.3f, 0.3f);

	public override void SetActive()
	{
		base.SetActive();
		SetColor(normTint);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		SetColor(darkTint);
	}

	private void Awake()
	{
		Init(20, 20);
	}

	public override void Left()
	{
		base.Left();
		SetColor(normTint);
	}

	public override void Entered()
	{
		base.Entered();
		SetColor(liteTint);
	}
}
