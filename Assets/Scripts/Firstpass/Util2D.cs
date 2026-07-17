using System;
using System.Collections.Generic;
using UnityEngine;

public static class Util2D
{
	public static Color Alpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);
	}

	public static Color Hue(this Color c, float hue)
	{
		Vector4 vector = c.ToHsl();
		return new Vector4(hue, vector.y, vector.z, vector.w).FromHsl();
	}

	public static Color Rgb(byte r, byte g, byte b, byte a)
	{
		return new Color((float)(int)r / 255f, (float)(int)g / 255f, (float)(int)b / 255f, (float)(int)a / 255f);
	}

	public static Color Rgb(byte r, byte g, byte b)
	{
		return new Color((float)(int)r / 255f, (float)(int)g / 255f, (float)(int)b / 255f);
	}

	public static Vector2 GuiToScreenPoint(this Vector2 guiPoint)
	{
		return new Vector2(guiPoint.x, (float)Camera2D.ScreenHeight - guiPoint.y);
	}

	public static Vector3 GuiToScreenSpace(this Vector2 guiPoint)
	{
		return guiPoint.GuiToScreenSpace(0f);
	}

	public static Vector3 SetXY(this Vector3 pos, Vector3 newpos)
	{
		return new Vector3(newpos.x, newpos.y, pos.z);
	}

	public static Vector3 SetX(this Vector3 p, float x)
	{
		return new Vector3(x, p.y, p.z);
	}

	public static Vector3 GuiToScreenSpace(this Vector2 guiPoint, float z)
	{
		return new Vector3(guiPoint.x, (float)Camera2D.ScreenHeight - guiPoint.y, z);
	}

	public static void SetUV(this Mesh mesh, Rect r, float side)
	{
		mesh.SetUV(r, side, 0);
	}

	public static void SetUV(this Mesh mesh, Rect r)
	{
		mesh.SetUV(r, 1f, 0);
	}

	public static Vector2[] SetUV(this Vector2[] uv, Rect r, float texWidth, float texHeight, int first)
	{
		float x = r.x;
		float y = r.y;
		float width = r.width;
		float height = r.height;
		float value = x / texWidth;
		float value2 = (width + x) / texWidth;
		float value3 = 1f - y / texHeight;
		float value4 = 1f - (y + height) / texHeight;
		uv[first][0] = value;
		uv[first][1] = value4;
		uv[first + 1][0] = value;
		uv[first + 1][1] = value3;
		uv[first + 2][0] = value2;
		uv[first + 2][1] = value4;
		uv[first + 3][0] = value2;
		uv[first + 3][1] = value3;
		return uv;
	}

	public static Vector2[] SetUV(this Vector2[] uv, Rect r, float side, int first)
	{
		return uv.SetUV(r, side, side, first);
	}

	public static void SetUV(this Mesh mesh, Rect r, float texWidth, float texHeight, int first)
	{
		if (mesh == null)
		{
			"mesh".Trace("is null");
			return;
		}
		Vector2[] uv = mesh.uv;
		mesh.uv = uv.SetUV(r, texWidth, texHeight, first);
	}

	public static void SetUV(this Mesh mesh, Rect r, float side, int first)
	{
		if (mesh == null)
		{
			"mesh".Trace("is null");
			return;
		}
		Vector2[] uv = mesh.uv;
		mesh.uv = uv.SetUV(r, side, first);
	}

	public static void SetTint(this Mesh mesh, Color color)
	{
		if (!(mesh == null))
		{
			Color[] array = new Color[mesh.vertexCount];
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				array[i] = color;
			}
			mesh.colors = array;
		}
	}

	public static void SetAlphaRecursively(this Transform root, float alpha)
	{
		root.SetAlpha(alpha);
		foreach (Transform item in root)
		{
			item.SetAlpha(alpha);
		}
	}

	public static void SetAlpha(this Transform transform, float alpha)
	{
		Mesh mesh = transform.GetComponent<MeshFilter>().mesh;
		if (mesh != null)
		{
			mesh.SetTint(new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)));
		}
	}

	public static void SetTintRecursively(this Transform root, Color color)
	{
		root.SetTint(color);
		foreach (Transform item in root)
		{
			item.SetTint(color);
		}
	}

	public static void SetTint(this Transform transform, Color color)
	{
		Mesh mesh = transform.GetComponent<MeshFilter>().mesh;
		if (mesh != null)
		{
			mesh.SetTint(color);
		}
	}

	[Obsolete]
	public static void SetTint(this Mesh mesh, Color color, int first, int count)
	{
		Color[] colors = mesh.colors;
		for (int i = first; i < first + count; i++)
		{
			colors[i] = color;
		}
		mesh.colors = colors;
	}

	[Obsolete]
	public static void SetTint(this Mesh mesh, Color color, params int[] pointers)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (pointers == null)
		{
			throw new ArgumentNullException("pointers");
		}
		Color[] colors = mesh.colors;
		foreach (int num in pointers)
		{
			for (int j = 0; j < 4; j++)
			{
				colors[num + j] = color;
			}
		}
		mesh.colors = colors;
	}

	public static Rect GetQuadBoundsXY(this Transform transform)
	{
		MeshFilter component = transform.GetComponent<MeshFilter>();
		if (component == null)
		{
			return default(Rect);
		}
		Mesh sharedMesh = component.sharedMesh;
		if (sharedMesh == null)
		{
			return default(Rect);
		}
		Bounds bounds = sharedMesh.bounds;
		return new Rect(0f, 0f, bounds.size.x, bounds.size.y);
	}

	public static Rect GetLinesBg(this GameObject proto, int count, Transform root, int w, int dy)
	{
		KeyValuePair<GameObject, Rect> keyValuePair = new KeyValuePair<GameObject, Rect>(null, new Rect(0f, 0f, 0f, dy));
		for (int i = 0; i < count; i++)
		{
			keyValuePair = proto.GetLineBg(root, w, keyValuePair.Value.height.RoundToInt());
		}
		return keyValuePair.Value;
	}

	public static KeyValuePair<GameObject, Rect> GetLineBg(this GameObject proto, Transform root, int w, int dy)
	{
		int layer = root.gameObject.layer;
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(proto);
		gameObject.layer = layer;
		gameObject.transform.parent = root;
		gameObject.transform.localPosition = new Vector3(0f, -dy, 0f);
		Rect quadBoundsXY = gameObject.transform.GetQuadBoundsXY();
		int num = quadBoundsXY.width.RoundToInt();
		Transform transform = gameObject.transform.Find("center");
		transform.gameObject.layer = layer;
		Rect quadBoundsXY2 = transform.GetQuadBoundsXY();
		int num2 = quadBoundsXY2.width.RoundToInt();
		Transform transform2 = gameObject.transform.Find("right");
		transform2.gameObject.layer = layer;
		int num3 = transform2.GetQuadBoundsXY().width.RoundToInt();
		int num4 = w - (num + num3);
		num4 = ((num4 < num2) ? 1 : (num4 / num2 + ((num4 % num2 != 0) ? 1 : 0)));
		int num5 = num + num3 + num4 * num2;
		float height = quadBoundsXY.height;
		float x = transform.localPosition.x;
		if (num4 > 1)
		{
			for (int i = 1; i < num4; i++)
			{
				GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(transform.gameObject);
				gameObject2.layer = layer;
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localPosition = new Vector3((float)i * quadBoundsXY2.width + x, 0f, 0f);
			}
		}
		Vector3 localPosition = transform2.localPosition;
		localPosition.x += (float)(num4 - 1) * quadBoundsXY2.width;
		transform2.localPosition = localPosition;
		return new KeyValuePair<GameObject, Rect>(gameObject, new Rect(0f, 0f, num5, height + (float)dy));
	}
}
