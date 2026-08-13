using UnityEngine;

public class HpBar : MonoBehaviour
{
	private const int FRAME_SUB = 23;

	private int _fullhp;

	public LeftOrRight leftOrRight;

	public int digitsFromSide = 170;

	public int digitsFromTop = -8;

	public Transform cap;

	public Transform frame;

	public Transform empty;

	public Transform hpbar;

	public Transform triangle;

	public Transform hpdigits;

	private int _count;

	private void Start()
	{
		int num = ((leftOrRight == LeftOrRight.Left) ? 1 : (-1));
		hpdigits.localPosition = new Vector3(num * digitsFromSide, digitsFromTop, hpdigits.localPosition.z);
	}

	private void Update()
	{
	}

	private void HideTriangle()
	{
		triangle.gameObject.SetActive(false);
	}

	private void UnhideTriangle(int scale)
	{
		triangle.gameObject.SetActive(true);
		float x = hpbar.localPosition.x;
		float x2 = x + 2f * Mathf.Sign(x) * (float)scale;
		Vector3 localPosition = triangle.localPosition;
		triangle.localPosition = new Vector3(x2, localPosition.y, localPosition.z);
	}

	public void SetHp(int hp, int maxhp)
	{
		float hpStripe = (float)hp / (float)maxhp;
		SetHpStripe(hpStripe);
		if (hpdigits != null)
		{
			hpdigits.GetComponent<HpDigits>().SetHpDigits(hp, maxhp);
		}
	}

	private void SetHpStripe(float hp)
	{
		hp = Mathf.Clamp01(hp);
		int num = ((float)_fullhp * hp).FloorToInt();
		int num2 = num - 5;
		int num3;
		if (num2 < 0 || _fullhp - num < 5)
		{
			HideTriangle();
			num3 = num;
		}
		else
		{
			num3 = num2;
			UnhideTriangle(num3);
		}
		hpbar.localScale = new Vector3(num3, 1f, 1f);
	}

	internal void SetWidth(int width)
	{
		int num = empty.localPosition.x.FloorToInt().Mod();
		int num2 = (width - num) / 2;
		int num3 = num2 - 23;
		empty.localScale = new Vector3(num2, 1f, 1f);
		frame.localScale = new Vector3(num3, 1f, 1f);
		Vector3 localPosition = cap.localPosition;
		cap.localPosition = new Vector3(localPosition.x + 2f * Mathf.Sign(localPosition.x) * (float)(num3 - 1), localPosition.y, localPosition.z);
		_fullhp = num2 - 1;
	}
}
