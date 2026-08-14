using UnityEngine;

public class OneSpriteButton : SpriteButton
{
	public Color darkTint = new Color(0.5f, 0.5f, 0.5f);

	public Color lightTint = new Color(0.7f, 0.7f, 0.7f, 1f);

	public Color neutralTint = new Color32(128, 128, 128, byte.MaxValue);

	public Vector3 overScale = new Vector3(1.1f, 1.1f, 1f);

	private Vector3 _localPos;

	public override void SetActive()
	{
		base.SetActive();
		SetColor(neutralTint);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		SetColor(darkTint);
	}

	private void Awake()
	{
	}

	public override void Left()
	{
		base.Left();
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		SetColor(neutralTint);
	}

	public override void Entered()
	{
		base.Entered();
		base.transform.localScale = overScale;
		SetColor(lightTint);
	}
}
