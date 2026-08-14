using UnityEngine;
using Yarx;

[ExecuteInEditMode]
public class ScratchAnyMesh : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public bool needRefresh;

	public Color VerticesColor = Color.white;

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawIcon(base.transform.position, "cross.png");
	}

	private void SetSharedMesh()
	{
		Mesh sharedMesh = base.transform.GetComponent<MeshFilter>().sharedMesh;
		if (!(sharedMesh == null))
		{
			sharedMesh.SetTint(VerticesColor);
			base.name.Trace("set mesh");
		}
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
