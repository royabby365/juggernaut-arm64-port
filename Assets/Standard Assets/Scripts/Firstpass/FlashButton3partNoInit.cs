using UnityEngine;

public class FlashButton3partNoInit : SpriteButton
{
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
		normal.GetComponent<MeshFilter>().mesh.SetTint(Color.white);
		normalLeft.GetComponent<MeshFilter>().mesh.SetTint(Color.white);
		normalRight.GetComponent<MeshFilter>().mesh.SetTint(Color.white);
		over.GetComponent<MeshFilter>().mesh.SetTint(Color.white);
		overLeft.GetComponent<MeshFilter>().mesh.SetTint(Color.white);
		overRight.GetComponent<MeshFilter>().mesh.SetTint(Color.white);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		normal.GetComponent<MeshFilter>().mesh.SetTint(darkTint);
		normalLeft.GetComponent<MeshFilter>().mesh.SetTint(darkTint);
		normalRight.GetComponent<MeshFilter>().mesh.SetTint(darkTint);
		over.GetComponent<MeshFilter>().mesh.SetTint(darkTint);
		overLeft.GetComponent<MeshFilter>().mesh.SetTint(darkTint);
		overRight.GetComponent<MeshFilter>().mesh.SetTint(darkTint);
	}

	private void Awake()
	{
		over.ShowOrHide(show: false);
		overLeft.ShowOrHide(show: false);
		overRight.ShowOrHide(show: false);
	}

	public override void Left()
	{
		base.Left();
		normal.ShowOrHide(show: true);
		normalLeft.ShowOrHide(show: true);
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
