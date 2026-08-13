using UnityEngine;

public class ShopItem : MonoBehaviour
{
	public string debugIconId;

	public Transform myIcon;

	public bool threeString;

	public int count;

	public SpriteText volume;

	public Transform gold;

	public Transform diamond;

	public SpriteText itemName;

	public SpriteText price;

	public SpriteText string1;

	public SpriteText string2;

	public SpriteText string3;

	public BagItem item;

	public string countSuffix;

	private int _count;

	private void Awake()
	{
		if (debugIconId == null || !(myIcon != null))
		{
			return;
		}
		Renderer renderer = myIcon.renderer;
		Texture2D texture2D = Util.Resource<Texture2D>(debugIconId);
		if (texture2D == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogError("wrong path " + debugIconId);
			}
			else
			{
				GetComponent<Renderer>()material.mainTexture = texture2D;
			}
		}
	}

	private void Start()
	{
		ItemChanged();
	}

	internal void ItemChanged()
	{
		int num = item.Description.Length;
		if (threeString)
		{
			string1.Text_ = ((num <= 0) ? string.Empty : item.Description[0]);
			string2.Text_ = ((num <= 1) ? string.Empty : item.Description[1]);
			string3.Text_ = ((num <= 2) ? string.Empty : item.Description[2]);
		}
		else
		{
			string text = ((num <= 0) ? string.Empty : item.Description[0]);
			string text2 = ((num <= 1) ? string.Empty : item.Description[1]);
			string text3 = ((num <= 2) ? string.Empty : item.Description[2]);
			string text_ = text + "\n" + text2 + "\n" + text3;
			string1.Text_ = text_;
		}
		price.Text_ = item.SellPrice.ToString();
		bool flag = item.SellCurrency == ServerData.MoneyType.TypeE.Gold;
		gold.gameObject.SetActive(flag);
		diamond.gameObject.SetActive(!flag);
		itemName.Text_ = item.Name;
	}

	private void Update()
	{
		if (count != _count)
		{
			_count = count;
			volume.Text_ = ((count > 1) ? (count + " " + countSuffix) : ((count != 1) ? "?????????" : string.Empty));
		}
	}

	public int BuyOne()
	{
		if (count < 1)
		{
			return 0;
		}
		count--;
		return item.SellPrice;
	}
}
