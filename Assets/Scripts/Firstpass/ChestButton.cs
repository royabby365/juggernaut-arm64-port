using UnityEngine;

public class ChestButton : SpriteButton
{
	public Vector3 lootIconPlace = new Vector3(57f, -50f, -50f);

	public Transform closedBox;

	public Transform openBox;

	public GameObject lootProto;

	public GameObject itemButtonProto;

	private GameObject _loot;

	private ServerData.Bonus.DropElement _lootItem;

	private int _lootCount;

	private InventoryItemButton _myItem;

	private void Awake()
	{
		Init();
	}

	public void SetLoot(ServerData.Bonus.DropElement loot)
	{
		_lootItem = loot;
		_lootCount = loot.Count;
	}

	public override void SetActive()
	{
		base.SetActive();
		openBox.gameObject.active = false;
		closedBox.gameObject.active = true;
	}

	public override void SetSelected()
	{
		base.SetSelected();
		openBox.gameObject.active = true;
		closedBox.gameObject.active = false;
		if (!_lootItem.IsItem)
		{
			_loot = (GameObject)Object.Instantiate(lootProto);
			if (_lootItem != null)
			{
				Transform transform = _loot.transform.FindChildByName("resource_ico", includeInactive: true);
				if (transform != null)
				{
					transform.GetComponent<MeshRenderer>().material.mainTexture = SingletonT<ResourcesManager>.I.LoadItemIcon(_lootItem);
				}
			}
			Transform transform2 = _loot.transform.FindChildByName("count_bg", includeInactive: true);
			transform2.gameObject.SetActiveRecursivelyMk1(_lootCount != 1);
			if (transform2.gameObject.active)
			{
				Transform transform3 = transform2.FindChildByName("count", includeInactive: true);
				SpriteText component = transform3.GetComponent<SpriteText>();
				component.Text_ = _lootCount.ToString();
			}
			_loot.transform.parent = openBox;
			_loot.transform.localPosition = lootIconPlace;
		}
		else
		{
			_loot = (GameObject)Object.Instantiate(itemButtonProto);
			InventoryItemButton component2 = _loot.transform.Find("bag_item").GetComponent<InventoryItemButton>();
			component2.name = "shop_good_" + SpriteGui.UniqueId;
			_loot.transform.parent = openBox;
			_loot.transform.SetLayerRecursively(openBox);
			_loot.transform.localPosition = lootIconPlace;
			component2.shopItem = _lootItem.Item;
			component2.renderer.material.mainTexture = SingletonT<ResourcesManager>.I.LoadItemIcon(_lootItem.Item);
			component2.RemoveNew();
			component2.Init();
			component2.SetActive();
			_myItem = component2;
		}
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
		if (_loot != null)
		{
			_loot.transform.parent = null;
			_loot.active = false;
			GameObject loot = _loot;
			_loot = null;
			Object.Destroy(loot);
		}
	}

	public ServerData.Item GetItem()
	{
		return (!(_myItem != null) || !_myItem.Active) ? null : _myItem.shopItem;
	}
}
