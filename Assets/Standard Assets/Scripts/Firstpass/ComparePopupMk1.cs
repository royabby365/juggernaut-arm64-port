using UnityEngine;
using Yarx;
using Yarx.Collections;

public class ComparePopupMk1 : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText Count;

	public Sprite CountBg;

	public CompareOneStat Compare1;

	public CompareOneStat Compare2;

	public SpriteText NonItemDescription;

	public Sprite ItemFrame;

	public Sprite ItemIcon;

	public SpriteText ItemName;

	private Vector3 _countableScale = new Vector3(0.8f, 0.8f, 1f);

	private System.Tuple<ServerData.Item, int> _itemWithCount;

	public System.Tuple<ServerData.Item, int> ItemWithCount
	{
		get
		{
			return _itemWithCount;
		}
		set
		{
			_itemWithCount = value;
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

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += OnButtonRelease;
			HudMk1.Instance.DragEndWithButton += OnButtonRelease;
		}
	}

	private void OnButtonRelease(SpriteButton spriteButton)
	{
		if (!(HudMk1.Instance == null) && spriteButton != null && spriteButton.name == "_catch_all_compare_button")
		{
			HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.GlobalComparePopup);
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private System.Tuple<ServerData.Item, int> GetItemWithCount(ServerData.Bonus.DropElement dropElement)
	{
		return System.Tuple.Create(dropElement.Item, dropElement.Count);
	}

	public void SetDropElement(ServerData.Bonus.DropElement dropElement)
	{
		ItemWithCount = GetItemWithCount(dropElement);
	}

	private void SetItemLook(System.Tuple<ServerData.Item, int> itemWithCount)
	{
		ServerData.Item item = itemWithCount.Item1;
		FontManager.ColorE colorE = item.DecodeColor();
		Color bottomColor = FontManager.Instance.GetNamedColor(colorE).BottomColor;
		ItemFrame.Tint_ = bottomColor;
		ItemIcon.SpriteName_ = SingletonT<ServerData>.I.GetItemImageName(item);
		ItemName.Text_ = item.TitleString;
		ItemName.NamedColorE_ = colorE;
		bool flag = itemWithCount.Item2 > 1;
		Count.transform.ShowOrHide(flag);
		CountBg.transform.ShowOrHide(flag);
		ItemIcon.transform.localScale = ((!flag) ? Vector3.one : _countableScale);
		if (flag)
		{
			Count.Text_ = itemWithCount.Item2.ToString();
		}
		UpdateCompare();
	}

	private void UpdateCompare()
	{
		ServerData.Item puppetItem = ItemWithCount.Item1.GetPuppetItem();
		SetItemCompare(ItemWithCount.Item1, puppetItem);
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
		if (itemSkillInfo == null)
		{
			Compare1.SetCompareEmpty();
			Compare2.SetCompareEmpty();
			NonItemDescription.Text_ = item.Description ?? "<empty>";
			NonItemDescription.ShowOrHide(show: true);
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
