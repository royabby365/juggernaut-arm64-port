using System;
using UnityEngine;
using Yarx.Collections;

[Obsolete]
public class ExtraItem : MonoBehaviour
{
	public Sprite LootItemIcon;

	public Sprite LootItemCountBg;

	public SpriteText LootItemCountDigits;

	public Sprite ItemFrame;

	public CompareOneStat Compare1;

	public CompareOneStat Compare2;

	private ServerData.Bonus.DropElement _lootItem;

	private int _lootCount;

	private void Awake()
	{
	}

	public void SetLoot(ServerData.Bonus.DropElement loot)
	{
		_lootItem = loot;
		if (loot != null)
		{
			_lootCount = loot.Count;
		}
		SetLook();
	}

	private void SetLook()
	{
		ServerData.Item item = _lootItem.Item;
		bool flag = item.IsCountable();
		LootItemIcon.SpriteName_ = SingletonT<ServerData>.I.GetItemImageName(item);
		UseCount(flag);
		UseCompare(!flag);
	}

	private void UseCompare(bool useit)
	{
		if (useit)
		{
			LootItemIcon.transform.localScale = Vector3.one;
			ServerData.Item item = _lootItem.Item;
			FontManager.ColorE color = item.DecodeColor();
			Color bottomColor = FontManager.Instance.GetNamedColor(color).BottomColor;
			ItemFrame.Tint_ = bottomColor;
			ServerData.Item puppetItem = item.GetPuppetItem();
			SetItemCompare(item, puppetItem);
		}
		else
		{
			Compare1.SetCompareEmpty();
			Compare2.SetCompareEmpty();
		}
	}

	private void UseCount(bool useit)
	{
		LootItemCountBg.ShowOrHide(useit);
		LootItemCountDigits.ShowOrHide(useit);
		if (useit)
		{
			LootItemIcon.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			ItemFrame.Tint_ = Color.gray;
			LootItemCountDigits.Text_ = _lootCount.ToString();
		}
	}

	private void SetItemCompare(ServerData.Item item, ServerData.Item opposite)
	{
		ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
		if (itemSkillInfo == null)
		{
			Compare1.SetCompareEmpty();
			Compare2.SetCompareEmpty();
			return;
		}
		if (opposite == null)
		{
			Compare2.SetCompareEmpty();
			int current = itemSkillInfo.Current;
			System.Tuple<string, int> itemDescription = item.GetItemDescription();
			Compare1.SetCompare(itemDescription, current, oppositeState: false);
			return;
		}
		ServerData.SkillInfo itemSkillInfo2 = opposite.GetItemSkillInfo();
		if (itemSkillInfo2.Skill.Type == itemSkillInfo.Skill.Type)
		{
			Compare2.SetCompareEmpty();
			int delta = itemSkillInfo.Current - itemSkillInfo2.Current;
			System.Tuple<string, int> itemDescription2 = item.GetItemDescription();
			Compare1.SetCompare(itemDescription2, delta, oppositeState: false);
		}
		else
		{
			int current2 = itemSkillInfo.Current;
			int current3 = itemSkillInfo2.Current;
			System.Tuple<string, int> itemDescription3 = item.GetItemDescription();
			System.Tuple<string, int> itemDescription4 = opposite.GetItemDescription();
			Compare1.SetCompare(itemDescription3, current2, oppositeState: false);
			Compare2.SetCompare(itemDescription4, current3, oppositeState: true);
		}
	}
}
