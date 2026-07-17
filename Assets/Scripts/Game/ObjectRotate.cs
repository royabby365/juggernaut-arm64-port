using UnityEngine;

[AddComponentMenu("Object Scripts/Transform Scripts/ObjectRotateXYZ")]
[ExecuteInEditMode]
public class ObjectRotate : MonoBehaviour
{
	public Vector3 mSpeed = Vector3.zero;

	private void Update()
	{
		if (Vector3.zero != mSpeed)
		{
			base.transform.Rotate(mSpeed, Space.World);
		}
	}
}
