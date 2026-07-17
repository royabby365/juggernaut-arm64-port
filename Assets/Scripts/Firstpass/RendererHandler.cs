using UnityEngine;

public class RendererHandler : MonoBehaviour
{
	private bool _hide;

	private bool _culled;

	protected MeshFilter MeshFilter;

	private MeshRenderer _meshRenderer;

	private static Mesh _shared;

	internal void ShowOrHideMethod(bool show)
	{
		_hide = !show;
		if ((!_culled || !show) && base.renderer != null)
		{
			base.renderer.enabled = show;
		}
	}

	protected void DoTurnOnRenderer()
	{
		_culled = false;
		if (base.collider != null)
		{
			base.collider.enabled = true;
		}
		if (!_hide && base.renderer != null)
		{
			base.renderer.enabled = true;
		}
	}

	protected void DoTurnOffRenderer()
	{
		_culled = true;
		if (base.renderer != null)
		{
			base.renderer.enabled = false;
		}
		if (base.collider != null)
		{
			base.collider.enabled = false;
		}
	}

	internal void ActiveShow()
	{
		_culled = false;
		_hide = false;
		base.renderer.enabled = true;
		if (base.collider != null)
		{
			base.collider.enabled = true;
		}
	}

	public void RegenerateSprite()
	{
		MeshFilter = GetComponent<MeshFilter>();
		_meshRenderer = GetComponent<MeshRenderer>();
		if (MeshFilter == null)
		{
			MeshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		if (_meshRenderer == null)
		{
			_meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		}
		if (_shared == null)
		{
			_shared = Util.Resource<Mesh>("__atlases/_shared", typeof(Mesh));
		}
		if (MeshFilter.sharedMesh == null)
		{
			MeshFilter.sharedMesh = _shared;
		}
		if (_hide)
		{
			DoTurnOffRenderer();
		}
	}
}
