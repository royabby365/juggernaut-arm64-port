using System;
using UnityEngine;

[Serializable]
public class uvAnimation : MonoBehaviour
{
	public int uvAnimationTileX;

	public int uvAnimationTileY;

	public float framesPerSecond;

	public uvAnimation()
	{
		uvAnimationTileX = 24;
		uvAnimationTileY = 1;
		framesPerSecond = 10f;
	}

	public virtual void Update()
	{
		int num = (int)(Time.time * framesPerSecond);
		num %= uvAnimationTileX * uvAnimationTileY;
		Vector2 scale = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
		int num2 = num % uvAnimationTileX;
		int num3 = num / uvAnimationTileX;
		Vector2 offset = new Vector2((float)num2 * scale.x, 1f - scale.y - (float)num3 * scale.y);
		GetComponent<Renderer>()material.SetTextureOffset("_OffsetTex", offset);
		GetComponent<Renderer>()material.SetTextureScale("_OffsetTex", scale);
	}

	public virtual void Main()
	{
	}
}
