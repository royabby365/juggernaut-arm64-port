using UnityEngine;

public class ChooseCharBgSprite : MonoBehaviour
{
	public Camera Camera;

	private void Start()
	{
		float num = Camera.orthographicSize * 2f;
		float num2 = num * Camera.pixelWidth / Camera.pixelHeight;
		float a = num2 / 1024f;
		a = Mathf.Max(a, num / 768f);
		base.transform.localScale = new Vector3(a, a, base.transform.localScale.z);
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x = (0f - num2) / 2f;
		localPosition.y = num / 2f;
		base.transform.localPosition = localPosition;
	}
}
