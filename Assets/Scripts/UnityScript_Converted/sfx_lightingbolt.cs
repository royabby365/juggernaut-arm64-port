using System;
using UnityEngine;

[Serializable]
public class sfx_lightingbolt : MonoBehaviour
{
	public int zigs;

	public float speed;

	public float scale;

	public BodyPositions targetPos;

	private Transform target;

	private Perlin noise;

	private float oneOverZigs;

	private Particle[] particles;

	public sfx_lightingbolt()
	{
		zigs = 100;
		speed = 1f;
		scale = 1f;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void Main()
	{
	}
}
