using UnityEngine;

public class ExtraItemPreview : SpriteButton
{
	public const string ShowExtraItemStats = "show_extra_item_stats_";

	public Sprite LootItemIcon;

	public Sprite LootItemCountBg;

	public SpriteText LootItemCountDigits;

	public Sprite ItemFrame;

	private ServerData.Bonus.DropElement _lootItem;

	private int _lootCount;

	private void Awake()
	{
		base.name = "show_extra_item_stats_" + SpriteGui.UniqueId;
	}

	private void OnDisable()
	{
		UnregisterMe();
	}

	public void SetLoot(ServerData.Bonus.DropElement loot)
	{
		_lootItem = loot;
		if (loot != null)
		{
			_lootCount = loot.Count;
		}
		SetLook();
		if (!base.Active)
		{
			Init();
			SetActive();
		}
	}

	private void SetLook()
	{
		ServerData.Item item = _lootItem.Item;
		LootItemIcon.SpriteName_ = SingletonT<ServerData>.I.GetItemImageName(item);
		UseCount(item.IsCountable());
	}

	private void UseCount(bool useit)
	{
		LootItemCountBg.ShowOrHide(useit);
		LootItemCountDigits.ShowOrHide(useit);
		if (useit)
		{
			LootItemIcon.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
			ItemFrame.Tint_ = Color.gray;
			LootItemCountDigits.Text_ = _lootCount.ToString();
		}
		else
		{
			LootItemIcon.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
			ServerData.Item item = _lootItem.Item;
			FontManager.ColorE color = item.DecodeColor();
			Color bottomColor = FontManager.Instance.GetNamedColor(color).BottomColor;
			ItemFrame.Tint_ = bottomColor;
		}
	}

	public override void Released()
	{
		if (_lootItem != null)
		{
			Messenger<ServerData.Bonus.DropElement>.Invoke(Globals.MsgCompareExtraDropElement, _lootItem);
		}
	}
}
