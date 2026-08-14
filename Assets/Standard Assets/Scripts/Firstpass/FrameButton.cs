using UnityEngine;

public class FrameButton : SpriteButton
{
	public string id;

	public int life;

	public int str;

	public int will;

	public bool fire;

	public bool lightning;

	public Transform frame;

	public Color activeTint = Color.white;

	public Color inactiveTint = new Color(0.5f, 0.5f, 0.5f);

	public void SetAvatar(Texture2D tex)
	{
		Transform transform = base.transform.Find("avatar");
		transform.GetComponent<Renderer>().material.mainTexture = tex;
	}

	public void SetMeBoss()
	{
		Object original = Util.Resource<Object>("zachistka/prefabs/boss_bage");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		gameObject.transform.parent = base.transform;
		gameObject.transform.SetLayerRecursively(base.transform);
		gameObject.transform.localPosition = new Vector3(6f, -62f, -50f);
		if (!base.Active)
		{
			SetInactive();
		}
	}

	public override void SetActive()
	{
		base.SetActive();
		foreach (Transform item in base.transform)
		{
			Mesh mesh = item.GetComponent<MeshFilter>().mesh;
			if (mesh != null)
			{
				mesh.SetTint(activeTint);
			}
		}
	}

	public override void SetInactive()
	{
		base.SetInactive();
		foreach (Transform item in base.transform)
		{
			Mesh mesh = item.GetComponent<MeshFilter>().mesh;
			if (mesh != null)
			{
				mesh.SetTint(inactiveTint);
			}
		}
	}

	private void Awake()
	{
		Init();
	}

	public override void Clicked()
	{
		base.Clicked();
	}
}
