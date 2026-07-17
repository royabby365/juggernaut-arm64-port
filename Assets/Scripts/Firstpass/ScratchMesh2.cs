using UnityEngine;
using Yarx;

[ExecuteInEditMode]
public class ScratchMesh2 : MonoBehaviour
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

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawIcon(base.transform.position, "cross.png");
	}

	private void SetSharedMesh()
	{
		Mesh mesh = base.transform.GetComponent<MeshFilter>().mesh;
		if (!(mesh == null))
		{
			mesh.SetTint(TopLeft);
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
		SetSharedMesh();
		needRefresh = false;
	}
}
