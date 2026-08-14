using UnityEngine;

public class SimpleOverButton : SpriteButton
{
	public Transform normal;

	public Transform over;

	public override void SetActive()
	{
		base.SetActive();
		normal.GetComponent<MeshRenderer>().enabled = true;
		over.ShowOrHide(show: false);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		normal.GetComponent<MeshRenderer>().enabled = false;
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

	private void Awake()
	{
		Init();
	}
}
