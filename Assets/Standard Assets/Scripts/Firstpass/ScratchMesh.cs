using System;
using UnityEngine;
using Yarx;

[ExecuteInEditMode]
public class ScratchMesh : MonoBehaviour
{
	public enum GradientDirection
	{
		No,
		Mono,
		Vertical,
		Horizontal,
		All4
	}

	private CompositeDisposable _subscriptions;

	public bool needRefresh;

	public Rect rect = new Rect(0f, 0f, 32f, 32f);

	public int texWidth = 1024;

	public int texHeight = 1024;

	public bool needChangeUV;

	public GradientDirection Gradient;

	public Color TopLeft;

	public Color TopRight;

	public Color BottomLeft;

	public Color BottomRight;

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawIcon(base.transform.position, "cross.png");
	}

	private void SetSharedMesh()
	{
		Mesh sharedMesh = base.transform.GetComponent<MeshFilter>().sharedMesh;
		if (sharedMesh == null)
		{
			return;
		}
		Rect r = rect;
		if (needChangeUV)
		{
			float b = r.x / (float)texWidth;
			float b2 = (r.width + r.x) / (float)texWidth;
			float b3 = 1f - r.y / (float)texHeight;
			float b4 = 1f - (r.y + r.height) / (float)texHeight;
			Vector2[] uv = sharedMesh.uv;
			if (uv[1][0].Eqv(b) && uv[1][1].Eqv(b3) && uv[2][0].Eqv(b2) && uv[2][1].Eqv(b4))
			{
				return;
			}
			sharedMesh.SetUV(r, texWidth, texHeight, 0);
		}
		switch (Gradient)
		{
		case GradientDirection.Mono:
			sharedMesh.SetTint(TopLeft);
			break;
		case GradientDirection.Vertical:
			sharedMesh.SetTint(TopLeft, 1, 1);
			sharedMesh.SetTint(TopLeft, 3, 1);
			sharedMesh.SetTint(BottomLeft, 0, 1);
			sharedMesh.SetTint(BottomLeft, 2, 1);
			break;
		case GradientDirection.Horizontal:
			sharedMesh.SetTint(TopLeft, 0, 2);
			sharedMesh.SetTint(TopRight, 2, 2);
			break;
		case GradientDirection.All4:
			sharedMesh.SetTint(TopLeft, 1, 1);
			sharedMesh.SetTint(BottomLeft, 0, 1);
			sharedMesh.SetTint(TopRight, 3, 1);
			sharedMesh.SetTint(BottomRight, 2, 1);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case GradientDirection.No:
			break;
		}
		base.name.Trace("set mesh");
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Update()
	{
		if (needRefresh)
		{
			SetSharedMesh();
			needRefresh = false;
		}
	}
}
