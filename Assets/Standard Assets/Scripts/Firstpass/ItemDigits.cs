using UnityEngine;

public class ItemDigits : MonoBehaviour
{
	private Rect _uvrect;

	private bool _inited;

	public int digitWidth = 11;

	public Transform one;

	public Transform ten;

	public Transform handred;

	public void SetItemDigits(int current)
	{
		if (!_inited)
		{
			Init();
		}
		int num = current / 100;
		int num2 = current % 100 / 10;
		int d = current % 10;
		int num3 = 0;
		if (num == 0)
		{
			handred.gameObject.SetActive(false);
			num3 += digitWidth / 2;
		}
		else
		{
			handred.gameObject.SetActive(true);
			SetDigit(handred, num);
			num3 += digitWidth;
		}
		if (num == 0 && num2 == 0)
		{
			ten.gameObject.SetActive(false);
			num3 += digitWidth / 2;
		}
		else
		{
			ten.gameObject.SetActive(true);
			ten.localPosition = new Vector3(num3, 0f, 0f);
			num3 += digitWidth;
			SetDigit(ten, num2);
		}
		one.localPosition = new Vector3(num3, 0f, 0f);
		SetDigit(one, d);
	}

	private void Init()
	{
		Vector2[] uv = one.gameObject.GetComponent<MeshFilter>().mesh.uv;
		Vector2 vector = uv[0];
		Vector2 vector2 = uv[3];
		_uvrect = new Rect(vector.x, vector2.y, vector2.x - vector.x, vector2.y - vector.y);
		_inited = true;
	}

	private void SetDigit(Transform t, int d)
	{
		Rect r = DigitFromRect(_uvrect, d);
		t.GetComponent<MeshFilter>().mesh.SetUV(r);
	}

	private Rect DigitFromRect(Rect rect, int d)
	{
		float num = rect.width / 11f;
		return new Rect(rect.x + num * (float)d, 1f - rect.y, num, rect.height);
	}
}
