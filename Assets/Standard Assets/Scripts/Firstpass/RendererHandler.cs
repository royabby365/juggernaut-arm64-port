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
		if ((!_culled || !show) && GetComponent<Renderer>() != null)
		{
			GetComponent<Renderer>().enabled = show;
		}
	}

	protected void DoTurnOnRenderer()
	{
		_culled = false;
		if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = true;
		}
		if (!_hide && GetComponent<Renderer>() != null)
		{
			GetComponent<Renderer>().enabled = true;
		}
	}

	protected void DoTurnOffRenderer()
	{
		_culled = true;
		if (GetComponent<Renderer>() != null)
		{
			GetComponent<Renderer>().enabled = false;
		}
		if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = false;
		}
	}

	internal void ActiveShow()
	{
		_culled = false;
		_hide = false;
		GetComponent<Renderer>().enabled = true;
		if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = true;
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
