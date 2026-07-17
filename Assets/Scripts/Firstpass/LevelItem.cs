using UnityEngine;
using Yarx;
using Yarx.Collections;

public class LevelItem : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public CompareOneStat Compare1;

	public CompareOneStat Compare2;

	public SpriteText NonItemDescription;

	public Sprite ItemFrame;

	public Sprite ItemIcon;

	public SpriteText ItemName;

	private readonly Color _darkTint = new Color32(128, 128, 128, 96);

	private ServerData.ShopGood _shopGood;

	public ServerData.ShopGood ShopGood
	{
		get
		{
			return _shopGood;
		}
		set
		{
			_shopGood = value;
			SetItemLook(value);
			UpdateCompare();
		}
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void SetItemLook(ServerData.ShopGood shopGood)
	{
		ServerData.Item item = shopGood.Item;
		FontManager.ColorE colorE = item.DecodeColor();
		Color bottomColor = FontManager.Instance.GetNamedColor(colorE).BottomColor;
		ItemFrame.Tint_ = bottomColor;
		ItemIcon.SpriteName_ = SingletonT<ServerData>.I.GetItemImageName(item);
		ItemName.Text_ = item.TitleString;
		ItemName.NamedColorE_ = colorE;
		bool flag = shopGood.Count > 1;
	}

	public void UpdateCompare()
	{
		ServerData.Item puppetItem = ShopGood.Item.GetPuppetItem();
		SetItemCompare(ShopGood.Item, puppetItem);
	}

	private void SetItemCompare(ServerData.Item item, ServerData.Item opposite)
	{
		if (item == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("[FUCKUP]");
			}
			return;
		}
		ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
		NonItemDescription.ShowOrHide(show: false);
		if (itemSkillInfo == null || itemSkillInfo.Skill.Type == ServerData.Skill.TypeE.FullRage || itemSkillInfo.Skill.Type == ServerData.Skill.TypeE.FullMana)
		{
			Compare1.SetCompareEmpty();
			Compare2.SetCompareEmpty();
			NonItemDescription.Text_ = ((!item.Description.IsNullOrEmpty()) ? item.Description : "<empty>");
			NonItemDescription.ShowOrHide(show: true);
			return;
		}
		if (opposite == null)
		{
			Compare2.SetCompareEmpty();
			int current = itemSkillInfo.Current;
			Tuple<string, int> itemDescription = item.GetItemDescription();
			Compare1.SetCompare(itemDescription, current, oppositeState: false);
			return;
		}
		ServerData.SkillInfo itemSkillInfo2 = opposite.GetItemSkillInfo();
		if (itemSkillInfo2.Skill.Type == itemSkillInfo.Skill.Type)
		{
			Compare2.SetCompareEmpty();
			int delta = itemSkillInfo.Current - itemSkillInfo2.Current;
			Tuple<string, int> itemDescription2 = item.GetItemDescription();
			Compare1.SetCompare(itemDescription2, delta, oppositeState: false);
		}
		else
		{
			int current2 = itemSkillInfo.Current;
			int current3 = itemSkillInfo2.Current;
			Tuple<string, int> itemDescription3 = item.GetItemDescription();
			Tuple<string, int> itemDescription4 = opposite.GetItemDescription();
			Compare1.SetCompare(itemDescription3, current2, oppositeState: false);
			Compare2.SetCompare(itemDescription4, current3, oppositeState: true);
		}
	}
}
