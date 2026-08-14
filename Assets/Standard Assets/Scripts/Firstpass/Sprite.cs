using System;
using UnityEngine;
using Yarx.Collections;

public class Sprite : RendererHandler, IRendererHandler
{
	public string SpriteName;

	public Quad.OriginPlace Origin;

	public Quad.Mirror QuadMirror;

	public Color Tint = new Color(0.5f, 0.5f, 0.5f, 1f);

	public bool IsNineSlice;

	public int Width = 64;

	public int Height = 64;

	public int LeftBorder = 1;

	public int RightBorder = 1;

	public int TopBorder = 1;

	public int BottomBorder = 1;

	public int HGutter;

	public int VGutter;

	public bool FillWidth;

	public bool FillHeight;

	public bool PosAsBorder = true;

	private Atlas _atlas;

	private bool _clippedHorizontal;

	private bool _clippedVertical;

	private float _clipLeft;

	private float _clipRight;

	private float _clipTop;

	private int _errorsCount;

	internal string SpriteName_
	{
		get
		{
			return SpriteName;
		}
		set
		{
			if (SpriteName != value)
			{
				SpriteName = value;
				Refresh();
			}
		}
	}

	internal Color Tint_
	{
		get
		{
			return Tint;
		}
		set
		{
			if (Tint != value)
			{
				Tint = value;
				Refresh();
			}
		}
	}

	private void Awake()
	{
		if (FillWidth)
		{
			Vector3 localPosition = base.transform.localPosition;
			Width = Camera2D.ScreenWidth;
			if (PosAsBorder)
			{
				Width -= (int)(2f * localPosition.x);
			}
		}
		if (FillHeight)
		{
			Height = Camera2D.ScreenHeight;
		}
		RegenerateSprite();
		if (SpriteName_ == "default_shop_item")
		{
			SpriteName_ = "000016004";
		}
		Refresh();
	}

	public void Refresh()
	{
		if (_clippedHorizontal)
		{
			ClipHorizontalLocal(_clipLeft, _clipRight);
		}
		else if (_clippedVertical)
		{
			ClipVertical(_clipTop);
		}
		else
		{
			RemakeMesh();
		}
	}

	private void RemakeMesh()
	{
		if (MeshFilter == null || SpriteName_.IsNullOrEmpty())
		{
			return;
		}
		System.Tuple<Atlas, int> atlasBySpriteName = SingletonT<AtlasManager>.I.GetAtlasBySpriteName(SpriteName_);
		if (atlasBySpriteName == null)
		{
			if (++_errorsCount < 5 && Globals.IsDebugBuild)
			{
				Debug.LogWarning("[FUCKUP 11'] == {0} @ {2}/.../{1}==".Fmt(SpriteName_, base.name, base.transform.root.name));
			}
			return;
		}
		_atlas = atlasBySpriteName.Item1;
		if (!(GetComponent<Renderer>() == null))
		{
			GetComponent<Renderer>().material = _atlas.Material;
			if (_atlas.Material == null && ++_errorsCount < 5 && Globals.IsDebugBuild)
			{
				Debug.LogWarning("[FACKUP 2]");
			}
			int item = atlasBySpriteName.Item2;
			Rect uvrect = _atlas.Uvs[item];
			Vector2 vector = _atlas.Dims[item];
			int num = (int)vector.x;
			int num2 = (int)vector.y;
			if (!IsNineSlice && (HGutter != 0 || VGutter != 0))
			{
				float num3 = uvrect.width / vector.x;
				float num4 = uvrect.height / vector.y;
				float num5 = (float)HGutter * num3;
				float num6 = (float)VGutter * num4;
				uvrect = new Rect(uvrect.x + num5, uvrect.y + num6, uvrect.width - 2f * num5, uvrect.height - 2f * num6);
				num -= 2 * HGutter;
				num2 -= 2 * VGutter;
			}
			MeshData meshData = ((!IsNineSlice) ? Quad.CreateQuad(Origin, QuadMirror, num, num2, uvrect, default(Rect)) : NineSlice.Create9SliceMesh(Origin, num, num2, uvrect, Width, Height, LeftBorder, RightBorder, TopBorder, BottomBorder));
			if (!IsNineSlice)
			{
				Width = (int)vector.x;
				Height = (int)vector.y;
			}
			meshData.SetTint(Tint_);
			Mesh mesh = GetComponent<MeshFilter>().mesh;
			mesh.Clear();
			mesh.vertices = meshData.vertices;
			mesh.triangles = meshData.triangles;
			mesh.colors = meshData.colors;
			mesh.uv = meshData.uv;
		}
	}

	public Vector3 GetCenter()
	{
		float num = (float)Width / 2f;
		float num2 = (float)Height / 2f;
		return Origin switch
		{
			Quad.OriginPlace.UpperLeft => -Vector3.left * num - Vector3.up * num2, 
			Quad.OriginPlace.Center => Vector3.zero, 
			Quad.OriginPlace.UpperRight => Vector3.left * num - Vector3.up * num2, 
			Quad.OriginPlace.BottomLeft => -Vector3.left * num + Vector3.up * num2, 
			Quad.OriginPlace.BottomRight => Vector3.left * num + Vector3.up * num2, 
			Quad.OriginPlace.UpperCenter => -Vector3.up * num2, 
			Quad.OriginPlace.LeftCenter => -Vector3.left * num, 
			Quad.OriginPlace.RightCenter => Vector3.left * num, 
			Quad.OriginPlace.BottomCenter => Vector3.up * num2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public bool ClipVertical(float fraction)
	{
		fraction = Mathf.Clamp01(fraction);
		_clipTop = fraction;
		if (IsNineSlice && ++_errorsCount < 5)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogWarning("clip NineSlice Sprite vertically: " + base.name);
			}
			_clippedVertical = false;
			return false;
		}
		RemakeMesh();
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		if (base.transform.GetComponent<Renderer>() != null)
		{
			base.transform.ShowOrHide(show: true);
		}
		if (fraction.Eqv(0f))
		{
			if (base.transform.GetComponent<Renderer>() != null)
			{
				base.transform.ShowOrHide(show: false);
			}
			_clippedVertical = true;
			return true;
		}
		if (fraction.Eqv(1f))
		{
			_clippedVertical = false;
			return false;
		}
		Vector2[] uv = mesh.uv;
		vertices[1].y *= fraction;
		vertices[3].y *= fraction;
		uv[1].y = mesh.uv[0].y + (mesh.uv[1].y - mesh.uv[0].y) * fraction;
		uv[3].y = mesh.uv[2].y + (mesh.uv[3].y - mesh.uv[2].y) * fraction;
		mesh.vertices = vertices;
		mesh.uv = uv;
		_clippedVertical = true;
		return true;
	}

	public bool ClipHorizontalWorld(float clipLeftX, float clipRightX)
	{
		float x = base.transform.InverseTransformPoint(clipLeftX, 0f, 0f).x;
		float x2 = base.transform.InverseTransformPoint(clipRightX, 0f, 0f).x;
		return ClipHorizontalLocal(x, x2);
	}

	public bool ClipHorizontalLocal(float lcx, float rcx)
	{
		_clipLeft = lcx;
		_clipRight = rcx;
		if (!IsNineSlice)
		{
			if (++_errorsCount < 5 && Globals.IsDebugBuild)
			{
				Debug.LogWarning("clip non NineSlice Sprite: " + base.name);
			}
			_clippedHorizontal = false;
			return false;
		}
		RemakeMesh();
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		base.transform.ShowOrHide(show: true);
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < 4; i++)
		{
			if (vertices[4 * i].x > lcx)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			base.transform.ShowOrHide(show: false);
			_clippedHorizontal = true;
			return true;
		}
		if (num > 0)
		{
			Vector2[] uv = mesh.uv;
			num2 = num - 1;
			float num3 = vertices[4 * num].x - vertices[4 * num2].x;
			float num4 = lcx - vertices[4 * num2].x;
			float num5 = num4 / num3;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					vertices[4 * j + k].x = lcx;
					uv[4 * j + k].x = mesh.uv[4 * num2 + k].x + (mesh.uv[4 * num + k].x - mesh.uv[4 * num2 + k].x) * num5;
				}
			}
			mesh.vertices = vertices;
			mesh.uv = uv;
			_clippedHorizontal = true;
			return true;
		}
		for (int num6 = 3; num6 >= 0; num6--)
		{
			if (vertices[4 * num6].x < rcx)
			{
				num2 = num6;
				break;
			}
		}
		if (num2 < 0)
		{
			base.transform.ShowOrHide(show: false);
			_clippedHorizontal = true;
			return true;
		}
		if (num2 >= 0 && num2 < 3)
		{
			Vector2[] uv2 = mesh.uv;
			num = num2 + 1;
			float num7 = vertices[4 * num].x - vertices[4 * num2].x;
			float num8 = rcx - vertices[4 * num2].x;
			float num9 = num8 / num7;
			for (int num10 = 3; num10 > num2; num10--)
			{
				for (int l = 0; l < 4; l++)
				{
					vertices[4 * num10 + l].x = rcx;
					uv2[4 * num10 + l].x = mesh.uv[4 * num2 + l].x + (mesh.uv[4 * num + l].x - mesh.uv[4 * num2 + l].x) * num9;
				}
			}
			mesh.vertices = vertices;
			mesh.uv = uv2;
			_clippedHorizontal = true;
			return true;
		}
		_clippedHorizontal = false;
		return false;
	}

	public void TurnOnRenderer()
	{
		DoTurnOnRenderer();
	}

	public void TurnOffRenderer()
	{
		DoTurnOffRenderer();
	}
}
