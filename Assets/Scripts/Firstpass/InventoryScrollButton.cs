using UnityEngine;

public class InventoryScrollButton : SpriteButton
{
	public Transform normal;

	public Transform over;

	public float inactiveAlpha;

	public override void SetActive()
	{
		base.SetActive();
		normal.GetComponent<MeshRenderer>().enabled = true;
	}

	public override void SetInactive()
	{
		base.SetInactive();
		normal.GetComponent<MeshRenderer>().enabled = false;
		over.gameObject.SetActive(false);
	}

	private void Awake()
	{
		Init();
	}

	public override void Left()
	{
		base.Left();
		normal.GetComponent<MeshRenderer>().enabled = true;
		over.gameObject.SetActive(false);
	}

	public override void Entered()
	{
		base.Entered();
		normal.GetComponent<MeshRenderer>().enabled = false;
		over.gameObject.SetActive(true);
	}
}
