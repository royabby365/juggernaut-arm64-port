using UnityEngine;

public class AchievmentView : MonoBehaviour
{
	private bool _isDescriptionEnabled;

	public Sprite Icon;

	public SpriteText Title;

	public AchievmentProgressBar ProgressBar;

	public SpriteText Count;

	public SpriteText Description;

	public GameObject[] DescriptionViewObjects;

	public GameObject[] NonDescriptionViewObjects;

	public AchievmentButton DescriptionButton;

	public SpriteText AchievmentDigit;

	private void Start()
	{
		if (DescriptionButton != null)
		{
			DescriptionButton.name = "button_achievment_" + Mathf.Abs(GetInstanceID());
			DescriptionButton.Click += DescriptionButton_Click;
			DescriptionButton.Init();
		}
		UpdateState();
	}

	private void OnEnable()
	{
		UpdateState();
	}

	internal void SetAchievment(GameEvents.Event achievment)
	{
		_isDescriptionEnabled = false;
		if (Icon != null)
		{
			if (achievment.Progress >= achievment.MaxProgress)
			{
				Icon.SpriteName_ = achievment.Achievement.Image;
			}
			else
			{
				Icon.SpriteName_ = achievment.Achievement.Image + "_sepia";
			}
		}
		if (Title != null)
		{
			Title.Text_ = achievment.Achievement.Title;
		}
		if (ProgressBar != null)
		{
			ProgressBar.SetProgress(achievment.Progress, achievment.MaxProgress);
		}
		if (Count != null)
		{
			Count.Text_ = "+" + achievment.Achievement.Points;
		}
		if (Description != null)
		{
			Description.Text_ = achievment.Achievement.Info;
		}
		if (DescriptionButton != null)
		{
			DescriptionButton.SetActive();
		}
		if (AchievmentDigit != null)
		{
			AchievmentDigit.Text_ = achievment.Achievement.Points.ToString();
		}
		UpdateState();
	}

	private void DescriptionButton_Click()
	{
		_isDescriptionEnabled = !_isDescriptionEnabled;
		UpdateState();
	}

	private void UpdateState()
	{
		if (DescriptionViewObjects.Length > 0 && NonDescriptionViewObjects.Length > 0)
		{
			GameObject[] descriptionViewObjects = DescriptionViewObjects;
			foreach (GameObject gameObject in descriptionViewObjects)
			{
				gameObject.SetActiveRecursivelyMk1(_isDescriptionEnabled);
			}
			GameObject[] nonDescriptionViewObjects = NonDescriptionViewObjects;
			foreach (GameObject gameObject2 in nonDescriptionViewObjects)
			{
				gameObject2.SetActiveRecursivelyMk1(!_isDescriptionEnabled);
			}
		}
	}
}
