using UnityEngine;

public class VertArrow : MonoBehaviour
{
	public Transform up;

	public Transform down;

	public SpriteText digits;

	public Material red;

	public Material green;

	private void Awake()
	{
	}

	public void Compare(int delta)
	{
		if (delta < 0)
		{
			up.GetComponent<MeshRenderer>().enabled = false;
			down.GetComponent<MeshRenderer>().enabled = true;
			digits.SetColor(FontManager.ColorE.CompareRed);
			digits.Text_ = "- " + -delta;
		}
		else if (delta == 0)
		{
			up.GetComponent<MeshRenderer>().enabled = false;
			down.GetComponent<MeshRenderer>().enabled = false;
			digits.Text_ = string.Empty;
		}
		else
		{
			up.GetComponent<MeshRenderer>().enabled = true;
			down.GetComponent<MeshRenderer>().enabled = false;
			digits.SetColor(FontManager.ColorE.CompareGreen);
			digits.Text_ = "+" + delta;
		}
	}
}
