using System;

public class ClickEventArgs : EventArgs
{
	private SpriteButton _button;

	public SpriteButton Button => _button;

	public ClickEventArgs(SpriteButton button)
	{
		_button = button;
	}
}
