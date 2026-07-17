using UnityEngine;

public class FlashButton : SpriteButton
{
	public Transform normal;

	public Transform over;

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
		over.ShowOrHide(show: false);
	}

	public override void Left()
	{
		base.Left();
		normal.GetComponent<MeshRenderer>().enabled = true;
		over.ShowOrHide(show: false);
	}

	public override void Entered()
	{
		base.Entered();
		normal.GetComponent<MeshRenderer>().enabled = false;
		over.ShowOrHide(show: true);
	}
}
