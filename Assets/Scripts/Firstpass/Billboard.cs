using UnityEngine;

public class Billboard : MonoBehaviour
{
	public bool XRot;

	public bool YRot = true;

	public bool ZRot;

	private void Start()
	{
		Update();
	}

	private void Update()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		Camera mainCamera = Camera.mainCamera;
		if (mainCamera != null)
		{
			base.transform.LookAt(mainCamera.transform);
			if (YRot)
			{
				base.transform.eulerAngles = new Vector3(mainCamera.transform.eulerAngles.x, mainCamera.transform.eulerAngles.y - 180f, mainCamera.transform.eulerAngles.z);
			}
		}
		base.transform.eulerAngles = new Vector3((!XRot) ? 0f : base.transform.eulerAngles.x, (!YRot) ? 0f : base.transform.eulerAngles.y, (!ZRot) ? 0f : base.transform.eulerAngles.z);
	}
}
