using UnityEngine;

public class ShiftedSprite : MonoBehaviour
{
	private void Start()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x = -(Camera2D.ScreenWidth - 1024) / 2;
		base.transform.localPosition = localPosition;
	}
}
