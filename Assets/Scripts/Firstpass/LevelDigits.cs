using UnityEngine;

public class LevelDigits : MonoBehaviour
{
	private Rect _uvrect;

	private bool _inited;

	public int level = 29;

	public int digitWidth = 13;

	public Transform one;

	public Transform ten;

	public Transform crown;

	public void SetLevelDigits(int current)
	{
		if (!_inited)
		{
			Init();
		}
		if (current > 99)
		{
			ten.gameObject.active = false;
			one.gameObject.active = false;
			if (crown != null)
			{
				crown.gameObject.active = true;
			}
			return;
		}
		one.gameObject.active = true;
		if (crown != null)
		{
			crown.gameObject.active = false;
		}
		int num = current / 10;
		int d = current % 10;
		int num2 = digitWidth;
		if (num == 0)
		{
			ten.gameObject.active = false;
			num2 /= 2;
		}
		else
		{
			ten.gameObject.active = true;
			SetDigit(ten, num);
		}
		one.localPosition = new Vector3(num2 - 1, 0f, 0f);
		SetDigit(one, d);
	}

	private void Start()
	{
		SetLevelDigits(level);
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
