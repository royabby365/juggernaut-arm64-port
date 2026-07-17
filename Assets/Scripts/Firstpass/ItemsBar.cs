using UnityEngine;

public class ItemsBar : MonoBehaviour
{
	public ItemButton[] Potions;

	public Vector3 activePosition = new Vector3(0f, 280f, 0f);

	public Vector3 passivePosition = new Vector3(0f, 1000f, 0f);

	public void HideItemsBar()
	{
		base.transform.localPosition = passivePosition;
	}

	public void ShowItemsBar()
	{
		base.transform.localPosition = activePosition;
	}

	public void RearrangePotions()
	{
		int num = 0;
		ItemButton[] potions = Potions;
		foreach (ItemButton itemButton in potions)
		{
			if (!itemButton.IsInHell)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		int num2 = -90 * (num - 1) / 2;
		ItemButton[] potions2 = Potions;
		foreach (ItemButton itemButton2 in potions2)
		{
			if (!itemButton2.IsInHell)
			{
				Vector3 localPosition = itemButton2.transform.localPosition;
				itemButton2.transform.localPosition = new Vector3(num2, localPosition.y, localPosition.z);
				num2 += 90;
			}
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}
}
