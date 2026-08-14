using UnityEngine;

public class ChooseCharButton : SpriteButton, IDraggable
{
	private void Awake()
	{
		SpriteGui spriteGui = base.transform.GetSpriteGui();
		spriteGui.RegisterButton(this);
		BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(4000f, 4000f, 0f);
		_collider = boxCollider;
	}

	public void Drag(Vector3 from, Vector3 to)
	{
	}
}
