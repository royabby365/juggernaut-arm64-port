using UnityEngine;

public class HpDigits : MonoBehaviour
{
	private Rect _uvrect;

	private bool _inited;

	public int digitWidth = 12;

	public Transform one;

	public Transform ten;

	public Transform handred;

	public Transform maxone;

	public Transform maxten;

	public Transform maxhandred;

	public void SetHpDigits(int current, int max)
	{
		if (!_inited)
		{
			Init();
		}
		int num = current / 100;
		int num2 = current % 100 / 10;
		int d = current % 10;
		if (num == 0)
		{
			handred.gameObject.active = false;
		}
		else
		{
			handred.gameObject.active = true;
			SetDigit(handred, num);
		}
		if (num == 0 && num2 == 0)
		{
			ten.gameObject.active = false;
		}
		else
		{
			ten.gameObject.active = true;
			SetDigit(ten, num2);
		}
		SetDigit(one, d);
		int num3 = 0;
		int num4 = max / 100;
		int num5 = max % 100 / 10;
		int d2 = max % 10;
		if (num4 == 0)
		{
			maxhandred.gameObject.active = false;
		}
		else
		{
			SetDigit(maxhandred, num4);
			maxhandred.gameObject.active = true;
			maxhandred.localPosition = new Vector3(num3, 0f, 0f);
			num3 += digitWidth;
		}
		if (num4 == 0 && num5 == 0)
		{
			maxten.gameObject.active = false;
		}
		else
		{
			SetDigit(maxten, num5);
			maxten.gameObject.active = true;
			maxten.localPosition = new Vector3(num3, 0f, 0f);
			num3 += digitWidth;
		}
		maxone.localPosition = new Vector3(num3, 0f, 0f);
		SetDigit(maxone, d2);
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
