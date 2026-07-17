using UnityEngine;

internal class ScaleForTime : MonoBehaviour
{
	public float ScaleTime;

	public float Size;

	private Person _person;

	private float _speed;

	private void Start()
	{
		_person = GetComponent<Person>();
		if (ScaleTime == 0f)
		{
			_speed = 10000f;
		}
		else
		{
			_speed = Mathf.Abs(base.transform.localScale.x / _person.InitScale.x - Size) / ScaleTime;
		}
	}

	private void Update()
	{
		float num = Time.deltaTime * _speed;
		float x = _person._size.x;
		bool flag = false;
		float num2 = 0f;
		if (x < Size)
		{
			num2 = x + num;
			if (num2 >= Size)
			{
				num2 = Size;
				flag = true;
			}
		}
		else if (x > Size)
		{
			num2 = x - num;
			if (num2 <= Size)
			{
				num2 = Size;
				flag = true;
			}
		}
		else
		{
			num2 = Size;
			flag = true;
		}
		_person.DoScale(num2, useInitScale: false);
		if (flag)
		{
			Object.Destroy(this);
		}
	}
}
