using UnityEngine;
using Yarx.Collections;

public class CompareOneStat : MonoBehaviour
{
	public SpriteText Stat;

	public SpriteText Delta;

	public Transform More;

	public Transform Less;

	public bool ShowDelta;

	private FontManager.ColorE _redColor = FontManager.ColorE.CompareRed;

	private FontManager.ColorE _greenColor = FontManager.ColorE.CompareGreen;

	private void ShowCompare(bool show)
	{
		Stat.ShowOrHideMethod(show);
		Delta.ShowOrHideMethod(show && ShowDelta);
		More.ShowOrHide(show && ShowDelta);
		Less.ShowOrHide(show && ShowDelta);
	}

	public void SetCompareEmpty()
	{
		ShowCompare(show: false);
	}

	public void SetCompare(System.Tuple<string, int> stat, int delta, bool oppositeState)
	{
		ShowCompare(show: true);
		if (oppositeState)
		{
			delta = -delta;
		}
		if (delta == 0 || !ShowDelta)
		{
			Stat.Text_ = stat.Item1 + ((!oppositeState) ? stat.Item2 : 0);
		}
		else
		{
			Stat.Text_ = stat.Item1;
		}
		if (ShowDelta)
		{
			Delta.Text_ = ((delta == 0) ? string.Empty : Mathf.Abs(delta).ToString());
			if (delta == 0)
			{
				More.ShowOrHide(show: false);
				Less.ShowOrHide(show: false);
			}
			else if (delta < 0)
			{
				More.ShowOrHide(show: false);
				Delta.NamedColorE_ = _redColor;
			}
			else
			{
				Less.ShowOrHide(show: false);
				Delta.NamedColorE_ = _greenColor;
			}
		}
	}
}
