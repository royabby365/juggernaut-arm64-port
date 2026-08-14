using UnityEngine;

public class StatProgressBar : MonoBehaviour
{
	public SpriteText SkillName;

	public SpriteText ProgressText;

	public Sprite ProgressScale;

	public void SetProgress(string labelText, int progress, int maxValue)
	{
		SkillName.Text_ = labelText;
		if (maxValue < progress)
		{
			maxValue = progress;
		}
		maxValue = ((maxValue == 0) ? 1 : maxValue);
		float num = (float)progress / (float)maxValue;
		ProgressText.Text_ = $"{progress}/{maxValue}";
		ProgressScale.ClipHorizontalLocal(-2000f, ((float)ProgressScale.Width * num).RoundToInt());
	}
}
