using UnityEngine;

public class CompareRow : MonoBehaviour
{
	public SpriteText label;

	public SpriteText digits;

	public void Set(string lbl, int count)
	{
		label.Text_ = lbl;
		digits.Text_ = count.ToString();
	}
}
