using UnityEngine;

public class InfoFrame : MonoBehaviour
{
	public SpriteText id;

	public SpriteText life;

	public SpriteText str;

	public SpriteText will;

	public Transform arrow;

	public Transform fire;

	public Transform lightning;

	public string leftmostFrame = "frame_0";

	public string rightmostFrame = "frame_9";

	private int _arrowWidth;

	private int _width;

	private void Start()
	{
		_arrowWidth = (int)arrow.GetComponent<MeshFilter>().mesh.bounds.size.x;
		_width = (int)GetComponent<MeshFilter>().mesh.bounds.size.x;
	}

	public void SetInfo(string id, int life, int str, int will, bool fire, bool lightning)
	{
		this.id.Text_ = id;
		this.life.Text_ = life.ToString();
		this.str.Text_ = str.ToString();
		this.will.Text_ = will.ToString();
		if (fire)
		{
			this.fire.gameObject.SetActive(true);
		}
		else
		{
			this.fire.gameObject.SetActive(false);
		}
		if (lightning)
		{
			this.lightning.gameObject.SetActive(true);
		}
		else
		{
			this.lightning.gameObject.SetActive(false);
		}
	}

	public void SetPosition(Transform t)
	{
		string text = t.gameObject.name;
		int num = (int)t.GetComponent<MeshFilter>().mesh.bounds.size.x;
		Vector3 position = arrow.position;
		arrow.position = new Vector3(t.position.x + (float)(num / 2) - (float)(_arrowWidth / 2), position.y, position.z);
		Vector3 position2 = base.transform.position;
		if (text == leftmostFrame)
		{
			base.transform.position = new Vector3(t.position.x, position2.y, position2.z);
		}
		else if (text == rightmostFrame)
		{
			base.transform.position = new Vector3(t.position.x + (float)num - (float)_width, position2.y, position2.z);
		}
		else
		{
			base.transform.position = new Vector3(t.position.x + (float)(num / 2) - (float)(_width / 2), position2.y, position2.z);
		}
	}
}
