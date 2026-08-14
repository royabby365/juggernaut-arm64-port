using System;
using UnityEngine;

public class FightResultPbar : MonoBehaviour
{
	public SpriteText label;

	public Transform bar;

	public SpriteText levelText;

	public int barZero = 14;

	public int barFull = 166;

	public bool supressLabel;

	private ServerData.PhrasesE _format;

	private string _formatString;

	public void SetLevel(int lvl)
	{
		if (levelText != null)
		{
			levelText.Text_ = lvl.ToString();
		}
	}

	public void SetPecentage(int percent)
	{
		int num = Math.Max(0, Math.Min(percent, 100));
		label.Text_ = ((!supressLabel) ? string.Format(_formatString, num) : string.Empty);
		float num2 = barFull - barZero;
		num2 *= (float)num / 100f;
		int num3 = barZero + num2.CeilToInt();
		bar.localScale = new Vector3(num3, 1f, 1f);
	}

	private void Awake()
	{
		_format = label.Phrase_;
		_formatString = SingletonT<ServerData>.I.GetPhrase(_format) ?? "{0}";
		label.Phrase_ = ServerData.PhrasesE.Custom;
	}
}
