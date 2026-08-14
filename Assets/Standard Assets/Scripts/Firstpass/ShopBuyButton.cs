using UnityEngine;

public class ShopBuyButton : SpriteButton
{
	private void Awake()
	{
		GameObject obj = base.gameObject;
		obj.name = obj.name + "_" + base.transform.parent.gameObject.name;
		Init();
	}

	private void Start()
	{
	}
}
