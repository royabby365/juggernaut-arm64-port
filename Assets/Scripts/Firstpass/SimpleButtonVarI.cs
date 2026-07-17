using UnityEngine;

public class SimpleButtonVarI : MonoBehaviour
{
	private Rect _buttonRect;

	private bool _active;

	public Transform backlit;

	private int _count;

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
		SetInactive();
	}

	private void Update()
	{
		_count++;
		if (_count > Random.Range(100, 180))
		{
			_count = 0;
			if (_active)
			{
				SetInactive();
			}
			else
			{
				SetActive();
			}
		}
		if (!_active)
		{
			return;
		}
		Touch[] touches = Input.touches;
		foreach (Touch touch in touches)
		{
			if (_buttonRect.IsContainsPoint(touch.position))
			{
				break;
			}
		}
		if (Input.GetMouseButtonDown(0) && _buttonRect.IsContainsPoint(Input.mousePosition))
		{
			SetInactive();
		}
	}

	internal void RecalcRect()
	{
		Rect rect = base.transform.GetSharedMesh().ToRect();
		_buttonRect = new Rect(base.transform.position.x + (float)(Screen.width / 2), (float)Screen.height - base.transform.position.y, rect.width, rect.height);
	}

	public void SetActive()
	{
		_active = true;
		backlit.gameObject.active = true;
		RecalcRect();
	}

	public void SetInactive()
	{
		_active = false;
		backlit.gameObject.active = false;
	}
}
