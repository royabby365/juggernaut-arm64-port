using System;

[Obsolete]
public class MainMenu2D : SpriteGui
{
	private void Start()
	{
		foreach (SpriteButton value in _buttons.Values)
		{
			value.SetActive();
		}
		base.Release += ProcessButtons;
	}

	private void Update()
	{
		ProcessRayCast();
	}

	private void ProcessButtons(SpriteButton button)
	{
	}
}
