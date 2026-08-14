using UnityEngine;

public class PopupOne : MonoBehaviour
{
	public Vector2 OnPos = new Vector2(40f, -200f);

	public Vector2 OffPos = new Vector2(40f, -2000f);

	public Transform oldIcon;

	public Transform newIcon;

	public SpriteText oldName;

	public SpriteText oldLife;

	public SpriteText oldStrength;

	public SpriteText oldAnger;

	public SpriteText oldVamp;

	public VertArrow lifeArrow;

	public VertArrow strengthArrow;

	public VertArrow angerArrow;

	public VertArrow vampArrow;

	public SpriteText newName;

	public SpriteText newLife;

	public SpriteText newStrength;

	public SpriteText newAnger;

	public SpriteText newVamp;

	public bool IsOn { get; private set; }

	private void Awake()
	{
		Off();
	}

	public void On()
	{
		IsOn = true;
		base.transform.localPosition = new Vector3(OnPos.x, OnPos.y, base.transform.localPosition.z);
	}

	public void Off()
	{
		IsOn = false;
		base.transform.localPosition = new Vector3(OffPos.x, OffPos.y, base.transform.localPosition.z);
	}

	public void Compare(InventoryItemButton old, InventoryItemButton current)
	{
		On();
		Vector2[] uv = old.transform.GetComponent<MeshFilter>().mesh.uv;
		Vector2[] uv2 = current.transform.GetComponent<MeshFilter>().mesh.uv;
		oldIcon.GetComponent<MeshFilter>().mesh.uv = uv;
		newIcon.GetComponent<MeshFilter>().mesh.uv = uv2;
		ServerData.Item shopItem = old.shopItem;
		int skill = shopItem.GetSkill(ServerData.Skill.TypeE.Vitality, 0);
		int skill2 = shopItem.GetSkill(ServerData.Skill.TypeE.Strength, 0);
		int skill3 = shopItem.GetSkill(ServerData.Skill.TypeE.Rage, 0);
		oldName.Text_ = shopItem.TitleString;
		oldLife.Text_ = skill.ToString();
		oldStrength.Text_ = skill2.ToString();
		oldAnger.Text_ = skill3.ToString();
		oldVamp.Text_ = 0.ToString();
		ServerData.Item shopItem2 = current.shopItem;
		int skill4 = shopItem2.GetSkill(ServerData.Skill.TypeE.Vitality, 0);
		int skill5 = shopItem2.GetSkill(ServerData.Skill.TypeE.Strength, 0);
		int skill6 = shopItem2.GetSkill(ServerData.Skill.TypeE.Rage, 0);
		newName.Text_ = shopItem2.TitleString;
		newLife.Text_ = skill4.ToString();
		newStrength.Text_ = skill5.ToString();
		newAnger.Text_ = skill6.ToString();
		newVamp.Text_ = 0.ToString();
		lifeArrow.Compare(skill4 - skill);
		strengthArrow.Compare(skill5 - skill2);
		angerArrow.Compare(skill6 - skill3);
		vampArrow.Compare(0);
	}
}
