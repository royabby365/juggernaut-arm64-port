using UnityEngine;

internal class PrototypeAnims : MonoBehaviour
{
	private float pressedTime;

	private float maxPressedTime = 5f;

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			pressedTime += Time.deltaTime;
			return;
		}
		pressedTime -= Time.deltaTime;
		if (pressedTime < 0f)
		{
			pressedTime = 0f;
		}
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, 10f, 100f, 50f), pressedTime.ToString());
	}
}
