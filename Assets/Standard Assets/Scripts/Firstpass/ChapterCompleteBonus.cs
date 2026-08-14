using UnityEngine;
using Yarx;

public class ChapterCompleteBonus : MonoBehaviour
{
	public GameObject LootProto;

	public FightScreenMobIcon[] MobIcons;

	private GameObject _curLoot;

	private CompositeDisposable _subscriptions;

	private void Awake()
	{
		_curLoot = (GameObject)Object.Instantiate(LootProto);
		_curLoot.transform.parent = base.transform;
		_curLoot.transform.localPosition = Vector3.zero;
		base.transform.GoToHell();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int>.AddListener(Globals.ChapterPrizeHandler, ShowCurChapterPrize));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void ShowCurChapterPrize(int bossIndex)
	{
		ServerData.Bonus.DropElement dropElement = AreaData.Current.Location.Bonus.Drop[0];
		if (LootProto == null || dropElement == null)
		{
			base.transform.GoToHell();
			return;
		}
		ExtraItemPreview componentInChildren = _curLoot.GetComponentInChildren<ExtraItemPreview>();
		componentInChildren.SetLoot(dropElement);
		Sprite itemFrame = componentInChildren.ItemFrame;
		float num = 90f / ((float)itemFrame.Width + 9f);
		componentInChildren.transform.localScale = new Vector3(num, num);
		Sprite component = componentInChildren.GetComponent<Sprite>();
		if ((bool)component)
		{
			component.ShowOrHide(show: false);
		}
		Sprite component2 = componentInChildren.transform.Find("frame_inner").gameObject.GetComponent<Sprite>();
		if (component2 != null)
		{
			component2.ShowOrHide(show: false);
		}
		if (bossIndex > 0)
		{
			bossIndex++;
		}
		if (MobIcons != null && bossIndex < MobIcons.Length)
		{
			Vector3 originalIconPos = MobIcons[bossIndex].OriginalIconPos;
			originalIconPos.x += 10f;
			base.transform.localPosition = originalIconPos;
			if (bossIndex <= 0)
			{
				originalIconPos = base.transform.position;
				originalIconPos.x = -44f;
				base.transform.position = originalIconPos;
			}
		}
		else
		{
			base.transform.GoToHell();
			if (Globals.IsDebugBuild)
			{
				Debug.LogError("ABNORMAL TERMINATION -- MobIcons != null && bossIndex < MobIcons.Length == false");
			}
		}
	}
}
