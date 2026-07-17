using UnityEngine;
using Yarx;

public class BagStat : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public ServerData.Skill.TypeE SkillType;

	public SpriteText SkillCount;

	public SpriteText SkillPointsCount;

	public SpriteButton AddToSkill;

	private int _addToSkillCount;

	private static readonly Vector3 ActivePosition = new Vector3(13f, -22f, 0f);

	private static readonly Vector3 InactivePosition = new Vector3(68f, -22f, 0f);

	public int AddToSkillCount
	{
		get
		{
			return _addToSkillCount;
		}
		set
		{
			_addToSkillCount = value;
			SkillPointsCount.Text_ = $"+{value}";
		}
	}

	public void SetActive()
	{
		AddToSkill.SetActive();
		AddToSkill.ShowOrHide(show: true);
		SkillPointsCount.ShowOrHide(show: true);
		SkillCount.Anchor_ = TextAnchor.MiddleLeft;
		SkillCount.transform.localPosition = ActivePosition;
	}

	public void SetInactive()
	{
		AddToSkill.SetInactive();
		AddToSkill.ShowOrHide(show: false);
		SkillPointsCount.ShowOrHide(show: false);
		SkillCount.Anchor_ = TextAnchor.MiddleCenter;
		SkillCount.transform.localPosition = InactivePosition;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		AddToSkillCount = 0;
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}
}
