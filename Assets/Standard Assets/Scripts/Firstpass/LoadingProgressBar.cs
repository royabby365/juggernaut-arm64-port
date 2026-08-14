using UnityEngine;

public class LoadingProgressBar : MonoBehaviour
{
	public Transform Wheel;

	private void Update()
	{
		Wheel.localRotation *= Quaternion.Euler(0f, 0f, -90f * Time.deltaTime);
	}
}
