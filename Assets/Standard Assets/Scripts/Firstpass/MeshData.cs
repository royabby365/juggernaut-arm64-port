using UnityEngine;

public struct MeshData
{
	public Vector3[] vertices;

	public int[] triangles;

	public Vector2[] uv;

	public Color[] colors;

	public Color SetTint(Color c)
	{
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = c;
		}
		return c;
	}
}
