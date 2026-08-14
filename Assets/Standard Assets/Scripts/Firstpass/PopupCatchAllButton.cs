using UnityEngine;

public class PopupCatchAllButton : SpriteButton, IDraggable
{
	private void Start()
	{
		Init();
		SetActive();
	}

	public void Drag(Vector3 from, Vector3 to)
	{
	}
}
