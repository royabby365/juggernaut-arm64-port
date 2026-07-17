using UnityEngine;

internal class EyeCamera : MonoBehaviour
{
	private Transform _eye;

	private Camera _camera;

	private void Start()
	{
		_camera = base.gameObject.GetComponent<Camera>();
		Renderer[] componentsInChildren = base.transform.root.gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (GetComponent<Renderer>()materials != null)
			{
				Material[] materials = GetComponent<Renderer>()materials;
				foreach (Material material in materials)
				{
					material.renderQueue = 4000;
				}
			}
		}
	}

	private void Update()
	{
	}
}
