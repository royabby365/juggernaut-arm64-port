using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour
{
	private string _currentDirection = "question";

	private string _currentStatus = string.Empty;

	private Dictionary<string, Transform> _icons;

	public Transform frontActive;

	public Transform frontBig;

	public Transform frontDisabled;

	public Transform leftActive;

	public Transform leftBig;

	public Transform leftDisabled;

	public Transform rightActive;

	public Transform rightBig;

	public Transform rightDisabled;

	public Transform question;

	public void SetCombo(string direction, string status)
	{
		Transform transform = _icons[GetKey(_currentDirection, _currentStatus)];
		if (transform.gameObject.active)
		{
			FightButton component = transform.GetComponent<FightButton>();
			if (component != null)
			{
				component.SetInactive();
			}
			transform.gameObject.active = false;
		}
		_currentDirection = direction;
		_currentStatus = status;
		Transform transform2 = _icons[GetKey(direction, status)];
		transform2.gameObject.active = true;
		if (status == Combos.Big)
		{
			transform2.GetComponent<FightButton>().SetActive();
		}
	}

	private void Awake()
	{
		string text = base.name;
		Transform obj = frontBig;
		obj.name = obj.name + "@" + text;
		Transform obj2 = leftBig;
		obj2.name = obj2.name + "@" + text;
		Transform obj3 = rightBig;
		obj3.name = obj3.name + "@" + text;
		frontBig.transform.GetComponent<FightButton>().Init();
		leftBig.transform.GetComponent<FightButton>().Init();
		rightBig.transform.GetComponent<FightButton>().Init();
		_icons = new Dictionary<string, Transform>
		{
			{
				Combos.Front + Combos.Active,
				frontActive
			},
			{
				Combos.Front + Combos.Disabled,
				frontDisabled
			},
			{
				Combos.Front + Combos.Big,
				frontBig
			},
			{
				Combos.Left + Combos.Active,
				leftActive
			},
			{
				Combos.Left + Combos.Disabled,
				leftDisabled
			},
			{
				Combos.Left + Combos.Big,
				leftBig
			},
			{
				Combos.Right + Combos.Active,
				rightActive
			},
			{
				Combos.Right + Combos.Disabled,
				rightDisabled
			},
			{
				Combos.Right + Combos.Big,
				rightBig
			},
			{
				Combos.Question,
				question
			}
		};
		foreach (KeyValuePair<string, Transform> icon in _icons)
		{
			icon.Value.gameObject.active = false;
		}
	}

	private static string GetKey(string dir, string status)
	{
		return (!(dir == Combos.Question)) ? (dir + status) : Combos.Question;
	}
}
