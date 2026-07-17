using System;
using UnityEngine;

[Serializable]
public class asset_billboard : MonoBehaviour
{
	public bool xrot;

	public bool yrot;

	public bool zrot;

	public asset_billboard()
	{
		yrot = true;
	}

	public virtual void Update()
	{
		Vector3 vector = new Vector3
		{
			x = transform.eulerAngles.x,
			y = transform.eulerAngles.y,
			z = transform.eulerAngles.z
		};
		Camera current = Camera.current;
		if ((bool)current)
		{
			transform.LookAt(current.transform);
			if (yrot)
			{
				float y = current.transform.eulerAngles.y - 180f;
				Vector3 eulerAngles = transform.eulerAngles;
				float num = (eulerAngles.y = y);
				Vector3 vector2 = (transform.eulerAngles = eulerAngles);
			}
		}
		if (!xrot)
		{
			int num2 = 0;
			Vector3 eulerAngles2 = transform.eulerAngles;
			float num3 = (eulerAngles2.x = num2);
			Vector3 vector4 = (transform.eulerAngles = eulerAngles2);
		}
		if (!yrot)
		{
			int num4 = 0;
			Vector3 eulerAngles3 = transform.eulerAngles;
			float num5 = (eulerAngles3.y = num4);
			Vector3 vector6 = (transform.eulerAngles = eulerAngles3);
		}
		if (!zrot)
		{
			int num6 = 0;
			Vector3 eulerAngles4 = transform.eulerAngles;
			float num7 = (eulerAngles4.z = num6);
			Vector3 vector8 = (transform.eulerAngles = eulerAngles4);
		}
	}

	public virtual void Main()
	{
	}
}
