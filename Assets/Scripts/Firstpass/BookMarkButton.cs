using UnityEngine;

public class BookMarkButton : SpriteButton
{
	public int VerticalPadding = -24;

	public Transform activeLayer;

	public Transform passiveLayer;

	public Color overColor = new Color(1f, 0.9f, 0.9f);

	public override void SetActive()
	{
		base.SetActive();
		passiveLayer.gameObject.SetActive(true);
		activeLayer.GetComponent<MeshRenderer>().enabled = false;
	}

	public override void SetInactive()
	{
		base.SetInactive();
		passiveLayer.gameObject.SetActive(false);
		activeLayer.GetComponent<MeshRenderer>().enabled = true;
	}

	private void Awake()
	{
		Init(0, VerticalPadding);
	}

	public override void Left()
	{
		base.Left();
		SetColor(Color.white);
	}

	public override void Entered()
	{
		base.Entered();
		SetColor(overColor);
	}
}
