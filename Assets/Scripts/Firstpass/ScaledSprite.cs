using UnityEngine;

public class ScaledSprite : MonoBehaviour
{
	public bool Scale = true;

	public bool ScaleUseHeight = true;

	public bool Shift = true;

	public bool ShiftY = true;

	public bool ShiftY1024;

	public float Aspect = 1f;

	public AlignedSprite Aligned;

	private void Start()
	{
		Vector3 localScale = base.transform.localScale;
		float num = Camera2D.ScreenWidth;
		float num2 = Camera2D.ScreenHeight;
		if (Scale)
		{
			float num3 = num / 1024f;
			if (ScaleUseHeight)
			{
				num3 = Mathf.Max(num3, num2 / 768f);
			}
			base.transform.localScale = new Vector3(localScale.x * num3, localScale.y * num3 / Aspect, localScale.z);
		}
		if (Shift)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.x = (0f - localScale.x) * num / 2f;
			if (ShiftY)
			{
				localPosition.y = localScale.y * num2 / 2f;
			}
			if (ShiftY1024)
			{
				localPosition.y = 1024f * base.transform.localScale.y / 2f;
			}
			base.transform.localPosition = localPosition;
		}
		if ((bool)Aligned)
		{
			Aligned.Align(base.transform);
		}
	}
}
