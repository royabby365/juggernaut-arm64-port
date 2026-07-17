using UnityEngine;

public class BuyCurrencyButton : SpriteButton
{
	public Transform normal;

	public Transform normal1;

	public Transform normal2;

	public Transform over;

	public Transform over1;

	public Transform over2;

	public int framecount = 30;

	private int _framecount;

	private void Awake()
	{
		Init();
		_framecount = framecount;
	}

	private void Update()
	{
		if (++_framecount >= framecount)
		{
			normal.GetComponent<MeshRenderer>().enabled = true;
			normal1.GetComponent<MeshRenderer>().enabled = true;
			normal2.GetComponent<MeshRenderer>().enabled = true;
			over.gameObject.active = false;
			over1.gameObject.active = false;
			over2.gameObject.active = false;
		}
	}

	public override void Clicked()
	{
		base.Clicked();
		normal.GetComponent<MeshRenderer>().enabled = false;
		normal1.GetComponent<MeshRenderer>().enabled = false;
		normal2.GetComponent<MeshRenderer>().enabled = false;
		over.gameObject.active = true;
		over1.gameObject.active = true;
		over2.gameObject.active = true;
		_framecount = 0;
	}
}
