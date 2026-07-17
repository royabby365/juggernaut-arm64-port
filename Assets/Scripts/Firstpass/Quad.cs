using System;
using UnityEngine;

public struct Quad
{
	public enum OriginPlace
	{
		UpperLeft,
		Center,
		UpperRight,
		BottomLeft,
		BottomRight,
		UpperCenter,
		LeftCenter,
		RightCenter,
		BottomCenter
	}

	public enum Mirror
	{
		None,
		Horizontal,
		Vertical,
		Both
	}

	public readonly int[] Triangles;

	public Vector2[] Uv;

	public readonly Vector3[] Vertices;

	public Vector3 LowerLeft;

	public Vector3 LowerRight;

	public Vector3 UpperLeft;

	public Vector3 UpperRight;

	private static readonly int[] QTriangles = new int[6] { 0, 1, 2, 2, 1, 3 };

	public Quad(Vector3 origin, Vector3 normal, Vector3 up, float width, float height, OriginPlace originPlace)
		: this(origin, normal, up, width, height, originPlace, Mirror.None)
	{
	}

	public Quad(Vector3 origin, Vector3 normal, Vector3 up, float width, float height, OriginPlace originPlace, Mirror mirror)
	{
		Vertices = new Vector3[4];
		Uv = new Vector2[4];
		Triangles = new int[6];
		Vector3 vector = Vector3.Cross(normal, up);
		switch (originPlace)
		{
		case OriginPlace.Center:
		{
			if ((int)width % 2 != 0 && Globals.IsDebugBuild)
			{
				Debug.LogWarning("width of sprite not even");
			}
			if ((int)height % 2 != 0 && Globals.IsDebugBuild)
			{
				Debug.LogWarning("height of sprite not even");
			}
			Vector3 vector2 = up * height / 2f + origin;
			UpperLeft = vector2 + vector * width / 2f;
			UpperRight = vector2 - vector * width / 2f;
			LowerLeft = UpperLeft - up * height;
			LowerRight = UpperRight - up * height;
			break;
		}
		case OriginPlace.UpperLeft:
			UpperLeft = origin;
			UpperRight = (0f - width) * vector;
			LowerLeft = UpperLeft - up * height;
			LowerRight = UpperRight - up * height;
			break;
		case OriginPlace.UpperRight:
			UpperRight = origin;
			UpperLeft = width * vector;
			LowerLeft = UpperLeft - up * height;
			LowerRight = UpperRight - up * height;
			break;
		case OriginPlace.BottomLeft:
			LowerLeft = origin;
			LowerRight = (0f - width) * vector;
			UpperLeft = LowerLeft + up * height;
			UpperRight = LowerRight + up * height;
			break;
		case OriginPlace.BottomRight:
			LowerRight = origin;
			LowerLeft = width * vector;
			UpperLeft = LowerLeft + up * height;
			UpperRight = LowerRight + up * height;
			break;
		default:
			throw new ArgumentOutOfRangeException("OriginPlace: " + originPlace);
		}
		FillVertices(mirror);
	}

	public static MeshData CreateQuad(OriginPlace origin, Mirror mirror, int width, int height, Rect uvrect, Rect clip)
	{
		MeshData result = new MeshData
		{
			vertices = new Vector3[4],
			uv = new Vector2[4],
			colors = new Color[4],
			triangles = QTriangles
		};
		Vector3 up = Vector3.up;
		Vector3 vector = Vector3.Cross(Vector3.forward, Vector3.up);
		Vector3 vector2 = Vector3.zero;
		switch (origin)
		{
		case OriginPlace.UpperLeft:
			vector2 = -height * up;
			break;
		case OriginPlace.Center:
			vector2 = -(height / 2) * up + ((float)width / 2f).FloorToInt() * vector;
			break;
		case OriginPlace.UpperRight:
			vector2 = -height * up + width * vector;
			break;
		case OriginPlace.BottomRight:
			vector2 = width * vector;
			break;
		case OriginPlace.UpperCenter:
			vector2 = -height * up + ((float)width / 2f).FloorToInt() * vector;
			break;
		case OriginPlace.LeftCenter:
			vector2 = -(height / 2) * up;
			break;
		case OriginPlace.RightCenter:
			vector2 = -(height / 2) * up + width * vector;
			break;
		case OriginPlace.BottomCenter:
			vector2 = ((float)width / 2f).FloorToInt() * vector;
			break;
		default:
			throw new ArgumentOutOfRangeException("origin");
		case OriginPlace.BottomLeft:
			break;
		}
		result.vertices[0] = vector2;
		ref Vector3 reference = ref result.vertices[1];
		reference = vector2 + height * up;
		ref Vector3 reference2 = ref result.vertices[2];
		reference2 = vector2 - width * vector;
		ref Vector3 reference3 = ref result.vertices[3];
		reference3 = result.vertices[2] + height * up;
		ref Vector2 reference4 = ref result.uv[0];
		reference4 = new Vector2(uvrect.x, uvrect.y);
		ref Vector2 reference5 = ref result.uv[1];
		reference5 = new Vector2(uvrect.x, uvrect.y + uvrect.height);
		ref Vector2 reference6 = ref result.uv[2];
		reference6 = new Vector2(uvrect.x + uvrect.width, uvrect.y);
		ref Vector2 reference7 = ref result.uv[3];
		reference7 = new Vector2(uvrect.x + uvrect.width, uvrect.y + uvrect.height);
		switch (mirror)
		{
		case Mirror.Horizontal:
		{
			Vector2 vector3 = result.uv[1];
			ref Vector2 reference12 = ref result.uv[1];
			reference12 = result.uv[3];
			result.uv[3] = vector3;
			vector3 = result.uv[0];
			ref Vector2 reference13 = ref result.uv[0];
			reference13 = result.uv[2];
			result.uv[2] = vector3;
			break;
		}
		case Mirror.Vertical:
		{
			Vector2 vector3 = result.uv[1];
			ref Vector2 reference10 = ref result.uv[1];
			reference10 = result.uv[0];
			result.uv[0] = vector3;
			vector3 = result.uv[3];
			ref Vector2 reference11 = ref result.uv[3];
			reference11 = result.uv[2];
			result.uv[2] = vector3;
			break;
		}
		case Mirror.Both:
		{
			Vector2 vector3 = result.uv[1];
			ref Vector2 reference8 = ref result.uv[1];
			reference8 = result.uv[2];
			result.uv[2] = vector3;
			vector3 = result.uv[3];
			ref Vector2 reference9 = ref result.uv[3];
			reference9 = result.uv[0];
			result.uv[0] = vector3;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("mirror");
		case Mirror.None:
			break;
		}
		return result;
	}

	private void FillVertices(Mirror mirror)
	{
		Vector2 vector = new Vector2(0f, 1f);
		Vector2 vector2 = new Vector2(1f, 1f);
		Vector2 vector3 = new Vector2(0f, 0f);
		Vector2 vector4 = new Vector2(1f, 0f);
		ref Vector3 reference = ref Vertices[0];
		reference = LowerLeft;
		Uv[0] = vector3;
		ref Vector3 reference2 = ref Vertices[1];
		reference2 = UpperLeft;
		Uv[1] = vector;
		ref Vector3 reference3 = ref Vertices[2];
		reference3 = LowerRight;
		Uv[2] = vector4;
		ref Vector3 reference4 = ref Vertices[3];
		reference4 = UpperRight;
		Uv[3] = vector2;
		switch (mirror)
		{
		case Mirror.Horizontal:
		{
			Vector2 vector5 = Uv[1];
			ref Vector2 reference9 = ref Uv[1];
			reference9 = Uv[3];
			Uv[3] = vector5;
			vector5 = Uv[0];
			ref Vector2 reference10 = ref Uv[0];
			reference10 = Uv[2];
			Uv[2] = vector5;
			break;
		}
		case Mirror.Vertical:
		{
			Vector2 vector5 = Uv[1];
			ref Vector2 reference7 = ref Uv[1];
			reference7 = Uv[0];
			Uv[0] = vector5;
			vector5 = Uv[3];
			ref Vector2 reference8 = ref Uv[3];
			reference8 = Uv[2];
			Uv[2] = vector5;
			break;
		}
		case Mirror.Both:
		{
			Vector2 vector5 = Uv[1];
			ref Vector2 reference5 = ref Uv[1];
			reference5 = Uv[2];
			Uv[2] = vector5;
			vector5 = Uv[3];
			ref Vector2 reference6 = ref Uv[3];
			reference6 = Uv[0];
			Uv[0] = vector5;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("mirror");
		case Mirror.None:
			break;
		}
		Triangles[0] = 0;
		Triangles[1] = 1;
		Triangles[2] = 2;
		Triangles[3] = 2;
		Triangles[4] = 1;
		Triangles[5] = 3;
	}
}
