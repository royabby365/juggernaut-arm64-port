using UnityEngine;

public class AlignedSprite : MonoBehaviour
{
	public float CoverPart = 0.246f;

	public void Align(Transform reference)
	{
		Vector3 localPosition = reference.transform.localPosition;
		Vector3 localScale = reference.transform.localScale;
		float num = localPosition.y - 1024f * localScale.y * CoverPart;
		Vector3 localPosition2 = base.transform.localPosition;
		localPosition2.y = ((float)(Camera2D.ScreenHeight / 2) + num) / 2f;
		base.transform.localPosition = localPosition2;
	}
}
