using UnityEngine;

public class AchievmentProgressBar : MonoBehaviour
{
	public SpriteText ProgressText;

	public Sprite ProgressScale;

	public void SetProgress(int progress, int maxValue)
	{
		if (maxValue < progress)
		{
			maxValue = progress;
		}
		maxValue = ((maxValue == 0) ? 1 : maxValue);
		float num = (float)progress / (float)maxValue;
		ProgressText.Text_ = $"{progress}/{maxValue}";
		float num2 = (float)ProgressScale.Width * num;
		if (num2 == 0f)
		{
			num2 = float.Epsilon;
		}
		else if (num2 > 1f)
		{
			num2 = num2.RoundToInt();
		}
		ProgressScale.ClipHorizontalLocal(-2000f, num2);
	}
}
