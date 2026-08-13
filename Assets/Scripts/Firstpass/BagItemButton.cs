using System.Collections;
using UnityEngine;
using Yarx.Collections;

public class BagItemButton : SpriteButton, IDraggable
{
	public Transform newBadge;

	public ServerData.Item item;

	public Sprite Frame;

	public Sprite UpgradeProgress;

	private Sprite _icon;

	private Sprite _new;

	private readonly Color _inactiveColor = new Color32(96, 96, 96, 96);

	private void Awake()
	{
		_icon = GetComponent<Sprite>();
		_new = newBadge.GetComponent<Sprite>();
		ClearUpdateProgress();
	}

	public void ClipWorld(float leftX, float rightX)
	{
		bool flag = _icon.ClipHorizontalWorld(leftX, rightX);
		Frame.ClipHorizontalWorld(leftX, rightX);
		UpgradeProgress.ClipHorizontalWorld(leftX, rightX);
		if (newBadge.gameObject.active)
		{
			_new.ClipHorizontalWorld(leftX, rightX);
		}
		if (flag)
		{
			SetInactive();
		}
		else
		{
			SetActive();
		}
	}

	public void RemoveNew()
	{
		newBadge.gameObject.SetActive(false);
		item.New = false;
	}

	public void SetUpgradeProgress(int current, int max)
	{
		float y = UpgradeProgress.transform.localScale.y;
		float item = ((max != 0) ? Mathf.Clamp01((float)current / (float)max) : 0f);
		StartCoroutine("UpgradeProgressCoro", Tuple.Create(y, item));
	}

	private void ClearUpdateProgress()
	{
		UpgradeProgress.transform.localScale = new Vector3(1f, 0f, 1f);
	}

	private IEnumerator UpgradeProgressCoro(System.Tuple<float, float> fromTo)
	{
		yield return new WaitForSeconds(1.1f);
		UpgradeProgress.transform.localScale = new Vector3(1f, fromTo.Item2, 1f);
	}

	public void Hide()
	{
		_icon.Tint_ = _inactiveColor;
		UpgradeProgress.Tint_ = _inactiveColor;
	}

	public void Unhide()
	{
		_icon.Tint_ = Color.gray;
		UpgradeProgress.Tint_ = Color.gray;
	}

	public void Refresh()
	{
		string itemImageName = SingletonT<ServerData>.I.GetItemImageName(item);
		_icon.SpriteName_ = itemImageName;
		if (!item.New)
		{
			RemoveNew();
		}
		FontManager.ColorE color = item.DecodeColor();
		Color bottomColor = FontManager.Instance.GetNamedColor(color).BottomColor;
		Frame.Tint_ = bottomColor;
		SetUpgradeProgress(item.CurrentStars, item.MaxStars);
	}

	public override string ToString()
	{
		return $"{base.name}, Item: {item}";
	}

	public void Drag(Vector3 from, Vector3 to)
	{
	}

	public void UpgradeItem()
	{
		ServerData.Item item = this.item;
		this.item = null;
		int upgradeId = GetUpgradeId(item);
		if (upgradeId < 0)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogWarning("NO UPGRADE FOR: {0}".Fmt(item));
			}
			return;
		}
		this.item = SingletonT<ServerData>.I.GetItemByServerId(upgradeId);
		if (this.item == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogWarning("CANNOT CREATE ITEM WITH ID: {0}".Fmt(upgradeId));
			}
			return;
		}
		this.item = this.item.MakeRealItem(forShop: true, 1);
		this.item.PutOn = item.PutOn;
		ClearUpdateProgress();
		SingletonT<ServerData>.I.AddToBag(this.item);
		SingletonT<ServerData>.I.RemoveFromBag(item);
		Refresh();
		BagInventory.Instance.RearrangeBag();
		System.Tuple<string, bool, string, bool> changeStatsDigits = Extensions.GetChangeStatsDigits(this.item, item);
		Messenger.Invoke(Globals.MsgItemUpgrade);
		Messenger.Invoke(Globals.MsgPlayerItemsChanged, changeStatsDigits);
	}

	public static int GetUpgradeId(ServerData.Item old)
	{
		int result = -1;
		foreach (ServerData.ItemRelation itemRelation in SingletonT<ServerData>.I._itemRelations)
		{
			if (itemRelation.From.Id == old.Id)
			{
				result = itemRelation.To.Id;
				break;
			}
		}
		return result;
	}
}
