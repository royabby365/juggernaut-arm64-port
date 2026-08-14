using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ClipQuad : MonoBehaviour
{
	public float LeftClipX;

	private Mesh _mesh;

	private Vector3[] _vxs;

	private Vector2[] _uv;

	private Vector3 _localPosition;

	private void Start()
	{
		_mesh = GetComponent<MeshFilter>().mesh;
		_vxs = _mesh.vertices;
		_uv = _mesh.uv;
		StartCoroutine("UpdateClip");
	}

	private IEnumerator UpdateClip()
	{
		while (true)
		{
			Info();
			Clip();
			yield return null;
		}
	}

	private void Info()
	{
		Vector3[] vertices = _mesh.vertices;
		ref Vector3 reference = ref vertices[0];
		reference = vertices[2] - new Vector3(10f, 0f, 0f);
		ref Vector3 reference2 = ref vertices[1];
		reference2 = vertices[3] - new Vector3(10f, 0f, 0f);
		_mesh.vertices = vertices;
		vertices = _mesh.vertices;
	}

	private void Clip()
	{
		float num = LeftClipX - base.transform.localPosition.x;
		float x = (_vxs[2] - _vxs[0]).x;
		base.transform.ShowOrHide(show: true);
		if (num <= 0f)
		{
			_mesh.vertices = _vxs;
			_mesh.uv = _uv;
			return;
		}
		if (num >= x)
		{
			base.transform.ShowOrHide(show: false);
			return;
		}
		Vector3[] vertices = _mesh.vertices;
		ref Vector3 reference = ref vertices[0];
		reference = _vxs[0] + new Vector3(num, 0f, 0f);
		ref Vector3 reference2 = ref vertices[1];
		reference2 = _vxs[1] + new Vector3(num, 0f, 0f);
		_mesh.vertices = vertices;
		Vector2[] uv = _mesh.uv;
		float num2 = num / x;
		ref Vector2 reference3 = ref uv[0];
		reference3 = _uv[0] + (_uv[2] - _uv[0]) * num2;
		ref Vector2 reference4 = ref uv[1];
		reference4 = _uv[1] + (_uv[3] - _uv[1]) * num2;
		_mesh.uv = uv;
	}
}
