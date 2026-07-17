using System;
using UnityEngine;

[Serializable]
public class inventory_rotator : MonoBehaviour
{
	public Vector3 rotation_direction;

	public float rotation_speed;

	private object inventory;

	private object rotation_reference;

	public inventory_rotator()
	{
		rotation_direction = new Vector3(0f, 1f, 0f);
		rotation_speed = 1f;
	}

	public virtual void Update()
	{
	}

	public virtual void Main()
	{
	}
}
