using UnityEngine;

public class InventoryItemButton : SpriteButton
{
	public Transform newBadge;

	public ServerData.Item shopItem;

	public float inactiveAlpha;

	private void Awake()
	{
	}

	public void SetNew()
	{
		newBadge.gameObject.SetActive(true);
		shopItem.New = true;
	}

	public void RemoveNew()
	{
		newBadge.gameObject.SetActive(false);
		shopItem.New = false;
	}

	public void Hide()
	{
		SetAlpha(inactiveAlpha);
		SetInactive();
	}

	public void Unhide()
	{
		SetAlpha(1f);
	}
}
