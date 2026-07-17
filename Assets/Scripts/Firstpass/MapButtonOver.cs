using UnityEngine;

public class MapButtonOver : SpriteButton
{
	public Transform Normal;

	public Transform Over;

	public Color darkTint = new Color(0.5f, 0.5f, 0.5f);

	private void Awake()
	{
		Over.gameObject.active = false;
	}

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

	public override void Left()
	{
		base.Left();
		Normal.GetComponent<MeshRenderer>().enabled = true;
		Over.gameObject.active = false;
	}

	public override void Entered()
	{
		base.Entered();
		Normal.GetComponent<MeshRenderer>().enabled = false;
		Over.gameObject.active = true;
	}
}
