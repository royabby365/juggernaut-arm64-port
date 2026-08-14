using System;
using UnityEngine;

[Serializable]
public class FontColor
{
	public string Name;

	public Color TopColor;

	public Color BottomColor;

	public static FontColor Create(FontManager.ColorE name, Color top, Color bottom)
	{
		FontColor fontColor = new FontColor();
		fontColor.Name = FontManager.ColorToKey(name);
		fontColor.TopColor = top;
		fontColor.BottomColor = bottom;
		return fontColor;
	}
}
