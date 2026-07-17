using UnityEngine;

public class CenteredSprite : MonoBehaviour
{
	public int Width;

	private void Start()
	{
		Vector3 localPosition = base.transform.localPosition;
		if (Width > 0)
		{
			localPosition.x = (Camera2D.ScreenWidth - Width) / 2;
		}
		else
		{
			localPosition.x = Camera2D.ScreenWidth / 2;
		}
		base.transform.localPosition = localPosition;
	}
}
