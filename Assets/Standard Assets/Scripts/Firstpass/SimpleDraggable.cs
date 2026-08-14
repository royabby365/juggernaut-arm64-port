using UnityEngine;

public class SimpleDraggable : SpriteButton, IDraggable
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

	public void Drag(Vector3 oldpos, Vector3 newpos)
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x += (newpos - oldpos).x;
		base.transform.localPosition = localPosition;
	}

	private void Awake()
	{
		Init();
	}
}
