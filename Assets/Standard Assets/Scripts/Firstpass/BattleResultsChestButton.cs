using System.Collections;
using UnityEngine;

public class BattleResultsChestButton : SpriteButton
{
	private const string AnimationName = "Take 002";

	public Sprite LootItemIcon;

	public Sprite LootItemCountBg;

	public SpriteText LootItemCountDigits;

	public Sprite Oreol;

	public Sprite ColorFrame;

	public Animation ChestAnim;

	private GameObject _loot;

	private ServerData.Bonus.DropElement _lootItem;

	private int _lootCount;

	private InventoryItemButton _myItem;

	private static readonly Vector3 ItemIconPosition = new Vector3(0f, -70f, -150f);

	private static readonly Vector3 CountableIconPosition = new Vector3(0f, -90f, -150f);

	private static readonly Vector3 CountableIconScale = new Vector3(0.6f, 0.6f, 1f);

	private static readonly Vector3 ItemIconScale = new Vector3(0.8f, 0.8f, 1f);

	private bool _isLoot;

	public void SetLoot(ServerData.Bonus.DropElement loot)
	{
		_isLoot = true;
		_lootItem = loot;
		if (loot != null)
		{
			_lootCount = loot.Count;
		}
	}

	public ServerData.Bonus.DropElement GetLoot()
	{
		return _lootItem;
	}

	public void SetNonLoot(ServerData.Bonus.DropElement nonloot)
	{
		_isLoot = false;
		_lootItem = nonloot;
		_lootCount = nonloot.Count;
		ServerData.Item item = nonloot.Item;
		SetItemLook(item);
	}

	public override void SetSelected()
	{
		base.SetSelected();
		SingletonT<SoundManager>.I.PlayChestSound();
		ServerData.Item item = _lootItem.Item;
		SetItemLook(item);
		if (item.IsMoney())
		{
			item.GetMoneyTypeFromItem().ChangePlayerFundsCount(_lootCount);
		}
		else
		{
			ServerData.Item item2 = item.MakeRealItem(forShop: false, _lootCount);
			SingletonT<ServerData>.I.AddToBag(item2);
			Messenger.Invoke(Globals.MsgBagNeedRefresh);
		}
		Messenger<ServerData.Item>.Invoke(Globals.MsgItemFoundInDrop, item);
		if (int.TryParse(base.name.Substring(base.name.Length - 1, 1), out var result))
		{
			Messenger<int>.Invoke(Globals.MsgDropChestSelected, result + 1);
		}
	}

	private void SetItemLook(ServerData.Item item)
	{
		FontManager.ColorE color = item.DecodeColor();
		Color bottomColor = FontManager.Instance.GetNamedColor(color).BottomColor;
		Oreol.ShowOrHideMethod(_isLoot);
		LootItemIcon.SpriteName_ = SingletonT<ServerData>.I.GetItemImageName(item);
		LootItemIcon.ShowOrHide(show: true);
		UseCount(item.IsCountable());
		StartCoroutine("ShowItemIcon", bottomColor);
	}

	private IEnumerator ShowItemIcon(Color color)
	{
		Vector3 iconScale = LootItemIcon.transform.localScale;
		Vector3 oreolScale = Oreol.transform.localScale;
		LootItemIcon.transform.localScale = Vector3.zero;
		Oreol.transform.localScale = Vector3.zero;
		float length = ChestAnim["Take 002"].length;
		if (length <= 0.5f)
		{
			length = 0.5f;
		}
		if (!_isLoot)
		{
			yield return new WaitForSeconds(length);
		}
		ChestAnim.Play();
		yield return new WaitForSeconds(length * 0.66f);
		ColorFrame.Tint_ = color;
		ColorFrame.ShowOrHide(show: true);
		float endTime = Time.time + length * 0.33f;
		while (Time.time < endTime)
		{
			float dt = Mathf.Clamp01((endTime - Time.time) / length);
			LootItemIcon.transform.localScale = Vector3.Lerp(iconScale, Vector3.zero, dt);
			Oreol.transform.localScale = Vector3.Lerp(oreolScale, Vector3.zero, dt);
			yield return null;
		}
		LootItemIcon.transform.localScale = iconScale;
		Oreol.transform.localScale = oreolScale;
		yield return null;
	}

	private void UseCount(bool useit)
	{
		LootItemCountBg.ShowOrHide(useit);
		LootItemCountDigits.ShowOrHide(useit);
		if (useit)
		{
			LootItemCountDigits.Text_ = _lootCount.ToString();
		}
		LootItemIcon.transform.localScale = ((!useit) ? ItemIconScale : CountableIconScale);
		LootItemIcon.transform.localPosition = ((!useit) ? CountableIconPosition : ItemIconPosition);
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
		LootItemIcon.ShowOrHide(show: false);
		LootItemCountBg.ShowOrHide(show: false);
		LootItemCountDigits.ShowOrHide(show: false);
		Oreol.ShowOrHideMethod(show: false);
		ColorFrame.ShowOrHide(show: false);
		_lootItem = null;
		_lootCount = 0;
		ChestAnim.Rewind();
		ChestAnim["Take 002"].enabled = true;
		ChestAnim.Sample();
		ChestAnim["Take 002"].enabled = false;
	}
}
