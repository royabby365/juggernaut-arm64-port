using System;
using UnityEngine;

[Serializable]
public class asset_animated_texture : MonoBehaviour
{
	public int uvAnimationTileX;

	public int uvAnimationTileY;

	public float framesPerSecond;

	public asset_animated_texture()
	{
		uvAnimationTileX = 4;
		uvAnimationTileY = 4;
		framesPerSecond = 16f;
	}

	public virtual void Update()
	{
	}

	public virtual void Main()
	{
	}
}
