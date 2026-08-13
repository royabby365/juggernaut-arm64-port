using UnityEngine;

public class ManaBar : MonoBehaviour
{
	public Transform empty;

	public Transform full0;

	public Transform full1;

	public Transform full2;

	public float startMana;

	public SpriteButton Spell0;

	public SpriteButton Spell1;

	public SpriteButton Spell2;

	public void SetMana(int mana, int maxmana)
	{
		float manaStripe = (float)mana / (float)maxmana;
		SetManaStripe(manaStripe);
	}

	private void SetManaStripe(float mana)
	{
		mana = Mathf.Clamp01(mana);
		float num = 1f / 3f;
		float y = empty.localScale.y;
		if (mana <= num)
		{
			float y2 = y * mana / num;
			full0.localScale = new Vector3(1f, y2, 1f);
			full0.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, y2);
			full1.localScale = new Vector3(1f, 0f, 1f);
			full1.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 0f);
			full2.localScale = new Vector3(1f, 0f, 1f);
			full2.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 0f);
		}
		else if (mana <= 2f * num)
		{
			full0.localScale = new Vector3(1f, y, 1f);
			full0.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, y);
			mana -= num;
			float y3 = y * mana / num;
			full1.localScale = new Vector3(1f, y3, 1f);
			full1.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, y3);
			full2.localScale = new Vector3(1f, 0f, 1f);
			full2.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 0f);
		}
		else
		{
			full0.localScale = new Vector3(1f, y, 1f);
			full0.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, y);
			full1.localScale = new Vector3(1f, y, 1f);
			full1.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, y);
			mana -= 2f * num;
			float y4 = y * mana / num;
			full2.localScale = new Vector3(1f, y4, 1f);
			full2.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, y4);
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
		SetManaStripe(0f);
		Spell0.SetInactive();
		Spell1.SetInactive();
		Spell2.SetInactive();
		Spell0.SetUnselected();
		Spell1.SetUnselected();
		Spell2.SetUnselected();
	}
}
