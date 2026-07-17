using UnityEngine;
using Yarx;
using Yarx.Collections;

public class CompareItem : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public CompareOneStat Compare1;

	public CompareOneStat Compare2;

	public Sprite ItemFrame;

	public Sprite ItemIcon;

	public SpriteText ItemName;

	public Transform NewExecution;

	public SpriteText NewExecutionLine1;

	public SpriteText NewExecutionLine2;

	private ServerData.Item _item;

	private ServerData.Item _oppsiteItem;

	public ServerData.Item Item
	{
		get
		{
			return _item;
		}
		set
		{
			_item = value;
			if (value != null)
			{
				SetItemLook(value);
			}
		}
	}

	public ServerData.Item OppsiteItem
	{
		get
		{
			return _oppsiteItem;
		}
		set
		{
			_oppsiteItem = value;
			if (_item != null)
			{
				SetItemCompare(_item, value);
			}
		}
	}

	private void Awake()
	{
		NewExecution.gameObject.SetActiveRecursivelyMk1(setActive: false);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
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
		if (Globals.IsDebugBuild)
		{
			Debug.Log("===== ITEM: {0} =====".Fmt(item.Title));
		}
		ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
		if (opposite == null)
		{
			Compare2.SetCompareEmpty();
			int current = itemSkillInfo.Current;
			Tuple<string, int> itemDescription = item.GetItemDescription();
			Compare1.SetCompare(itemDescription, current, oppositeState: false);
		}
		else
		{
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
		if (item.FatalityScenarioName != null)
		{
			NewExecution.gameObject.SetActiveRecursivelyMk1(setActive: true);
			NewExecutionLine2.Text_ = Item.FatalityScenarioName.WeaponString;
		}
		else
		{
			NewExecution.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
	}

	private void SetItemLook(ServerData.Item item)
	{
		FontManager.ColorE colorE = item.DecodeColor();
		Color bottomColor = FontManager.Instance.GetNamedColor(colorE).BottomColor;
		ItemFrame.Tint_ = bottomColor;
		ItemIcon.SpriteName_ = SingletonT<ServerData>.I.GetItemImageName(item);
		ItemName.Text_ = item.TitleString;
		ItemName.NamedColorE_ = colorE;
	}
}
