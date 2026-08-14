using UnityEngine;

public class FlashButton3part : SpriteButton
{
	public int HorizontalPadding = 22;

	public Transform normal;

	public Transform normalLeft;

	public Transform normalRight;

	public Transform over;

	public Transform overLeft;

	public Transform overRight;

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
		Init(HorizontalPadding, 0);
		over.ShowOrHide(show: false);
		overLeft.ShowOrHide(show: false);
		overRight.ShowOrHide(show: false);
	}

	public override void Left()
	{
		base.Left();
		normal.ShowOrHide(show: true);
		normalRight.ShowOrHide(show: true);
		over.ShowOrHide(show: false);
		overLeft.ShowOrHide(show: false);
		overRight.ShowOrHide(show: false);
	}

	public override void Entered()
	{
		base.Entered();
		normal.ShowOrHide(show: false);
		normalLeft.ShowOrHide(show: false);
		normalRight.ShowOrHide(show: false);
		over.ShowOrHide(show: true);
		overLeft.ShowOrHide(show: true);
		overRight.ShowOrHide(show: true);
	}
}
