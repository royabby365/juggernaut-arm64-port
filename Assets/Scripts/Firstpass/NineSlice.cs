using System;
using UnityEngine;

public class NineSlice
{
	public static MeshData Create9SliceMesh(Quad.OriginPlace origin, int origWidth, int origHeight, Rect uvrect, int width, int height, int borderLeft, int borderRight, int borderTop, int borderBottom)
	{
		return Create9SliceMesh(origin, origWidth, origHeight, uvrect, width, height, borderLeft, borderRight, borderTop, borderBottom, Vector3.forward, Vector3.up);
	}

	public static MeshData Create9SliceMesh(Quad.OriginPlace origin, int origWidth, int origHeight, Rect uvrect, int width, int height, int borderLeft, int borderRight, int borderTop, int borderBottom, Vector3 normal, Vector3 up)
	{
		MeshData result = new MeshData
		{
			vertices = new Vector3[16],
			uv = new Vector2[16],
			colors = new Color[16],
			triangles = new int[54]
		};
		Vector3 vector = Vector3.Cross(normal, up);
		Vector3 vector2 = Vector3.zero;
		switch (origin)
		{
		case Quad.OriginPlace.UpperLeft:
			vector2 = -height * up;
			break;
		case Quad.OriginPlace.Center:
			vector2 = -(height / 2) * up + ((float)width / 2f).FloorToInt() * vector;
			break;
		case Quad.OriginPlace.UpperRight:
			vector2 = -height * up + width * vector;
			break;
		case Quad.OriginPlace.BottomRight:
			vector2 = width * vector;
			break;
		case Quad.OriginPlace.UpperCenter:
			vector2 = -height * up + ((float)width / 2f).FloorToInt() * vector;
			break;
		case Quad.OriginPlace.LeftCenter:
			vector2 = -(height / 2) * up;
			break;
		case Quad.OriginPlace.RightCenter:
			vector2 = -(height / 2) * up + width * vector;
			break;
		case Quad.OriginPlace.BottomCenter:
			vector2 = ((float)width / 2f).FloorToInt() * vector;
			break;
		default:
			throw new ArgumentOutOfRangeException("origin");
		case Quad.OriginPlace.BottomLeft:
			break;
		}
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				int num = j + i * 4;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				switch (i)
				{
				case 1:
					num2 = borderLeft.Absi();
					num4 = borderLeft.ClampNegative();
					break;
				case 2:
					num2 = width - borderRight.Absi();
					num4 = origWidth - borderRight.ClampNegative();
					break;
				case 3:
					num2 = width;
					num4 = origWidth;
					break;
				}
				switch (j)
				{
				case 1:
					num3 = borderBottom.Absi();
					num5 = borderBottom.ClampNegative();
					break;
				case 2:
					num3 = height - borderTop.Absi();
					num5 = origHeight - borderTop.ClampNegative();
					break;
				case 3:
					num3 = height;
					num5 = origHeight;
					break;
				}
				ref Vector3 reference = ref result.vertices[num];
				reference = vector2 - num2 * vector + num3 * up;
				ref Color reference2 = ref result.colors[num];
				reference2 = new Color(0.5f, 0.5f, 0.5f, 1f);
				Vector2 vector3 = new Vector2((float)num4 / (float)origWidth, (float)num5 / (float)origHeight);
				Vector2 vector4 = new Vector2(vector3.x * uvrect.width + uvrect.x, vector3.y * uvrect.height + uvrect.y);
				result.uv[num] = vector4;
			}
		}
		int num6 = 0;
		for (int k = 0; k < 3; k++)
		{
			for (int l = 0; l < 3; l++)
			{
				int num7 = l + k * 4;
				result.triangles[num6++] = num7;
				result.triangles[num6++] = num7 + 1;
				result.triangles[num6++] = num7 + 4;
				result.triangles[num6++] = num7 + 4;
				result.triangles[num6++] = num7 + 1;
				result.triangles[num6++] = num7 + 5;
			}
		}
		return result;
	}
}
