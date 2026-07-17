using UnityEngine;

public class AttackButton : SpriteButton
{
	public Transform normal;

	public Transform over;

	private int _timer;

	public override void SetActive()
	{
		base.SetActive();
		normal.GetComponent<MeshRenderer>().enabled = true;
		over.gameObject.active = false;
	}

	public override void SetInactive()
	{
		base.SetInactive();
		normal.GetComponent<MeshRenderer>().enabled = false;
		over.gameObject.active = false;
	}

	private void Awake()
	{
		Init();
	}

	private void Update()
	{
		if (--_timer == 0)
		{
			normal.GetComponent<MeshRenderer>().enabled = true;
			over.gameObject.active = false;
		}
	}

	public override void Clicked()
	{
		base.Clicked();
		normal.GetComponent<MeshRenderer>().enabled = false;
		over.gameObject.active = true;
		_timer = 30;
	}
}
