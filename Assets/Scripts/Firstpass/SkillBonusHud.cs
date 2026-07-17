using UnityEngine;
using Yarx;

public class SkillBonusHud : MonoBehaviour
{
	public enum SkillBonusTypeE
	{
		Combo,
		Magic,
		Rage
	}

	private CompositeDisposable _subscriptions;

	public Sprite Icon;

	public SpriteText SkillName;

	public SpriteText SkillDescription;

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += Hud_Release;
			HudMk1.Instance.DragEndWithButton += Hud_Release;
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Hud_Release(SpriteButton obj)
	{
		if (obj.name == "button_skill_bonus_continue")
		{
			Messenger.Invoke(Globals.MsgGuiExitSkill);
		}
	}

	public void Init(SkillBonusTypeE skillType)
	{
		switch (skillType)
		{
		case SkillBonusTypeE.Combo:
			Icon.SpriteName_ = "crit_abil";
			Icon.transform.localPosition = new Vector3(-39f, Icon.transform.localPosition.y, Icon.transform.localPosition.z);
			SkillName.Phrase_ = ServerData.PhrasesE.SkillComboName;
			SkillDescription.Phrase_ = ServerData.PhrasesE.SkillComboDesc;
			break;
		case SkillBonusTypeE.Magic:
			Icon.SpriteName_ = "magic_abil";
			Icon.transform.localPosition = new Vector3(10f, Icon.transform.localPosition.y, Icon.transform.localPosition.z);
			SkillName.Phrase_ = ServerData.PhrasesE.SkillMagicName;
			SkillDescription.Phrase_ = ServerData.PhrasesE.SkillMagicDesc;
			break;
		case SkillBonusTypeE.Rage:
			Icon.SpriteName_ = "rage_abil";
			Icon.transform.localPosition = new Vector3(4f, Icon.transform.localPosition.y, Icon.transform.localPosition.z);
			SkillName.Phrase_ = ServerData.PhrasesE.SkillRageName;
			SkillDescription.Phrase_ = ServerData.PhrasesE.SkillRageDesc;
			break;
		}
	}
}
