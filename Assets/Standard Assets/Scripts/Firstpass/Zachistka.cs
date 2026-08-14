public class Zachistka : SpriteGui
{
	public InfoFrame info;

	public void HideInfoFrame()
	{
		info.gameObject.SetActiveRecursivelyMk1(setActive: false);
	}

	public void SetInfoFrameAt(string buttonName)
	{
		SpriteButton button = GetButton(buttonName);
		if (button == null)
		{
			return;
		}
		FrameButton frameButton = button as FrameButton;
		if (!(frameButton == null))
		{
			if (!info.gameObject.active)
			{
				info.gameObject.SetActiveRecursivelyMk1(setActive: true);
			}
			info.SetPosition(frameButton.transform);
			info.SetInfo(frameButton.id, frameButton.life, frameButton.str, frameButton.will, frameButton.fire, frameButton.lightning);
		}
	}

	public void SetInfoFrameAt(string buttonName, string id, int life, int strength, int will, bool fire, bool lighting, bool dark, bool ice)
	{
		SpriteButton spriteButton = ((!_buttons.ContainsKey(buttonName)) ? null : _buttons[buttonName]);
		if (spriteButton == null)
		{
			return;
		}
		FrameButton frameButton = spriteButton as FrameButton;
		if (!(frameButton == null))
		{
			frameButton.life = life;
			frameButton.id = id;
			frameButton.str = strength;
			frameButton.will = will;
			frameButton.fire = fire;
			frameButton.lightning = lighting;
			if (!info.gameObject.active)
			{
				info.gameObject.SetActiveRecursivelyMk1(setActive: true);
			}
			info.SetPosition(frameButton.transform);
			info.SetInfo(frameButton.id, frameButton.life, frameButton.str, frameButton.will, frameButton.fire, frameButton.lightning);
		}
	}

	public override void SetButtonActive(string buttonName)
	{
		base.SetButtonActive(buttonName);
	}

	public void HideButton(string buttonName)
	{
		SpriteButton value = null;
		if (_buttons.TryGetValue(buttonName, out value))
		{
			value.gameObject.SetActiveRecursivelyMk1(setActive: false);
			return;
		}
		Utils.Log("HideButton failed: can't find button", buttonName);
	}

	public void ShowButton(string buttonName)
	{
		SpriteButton value = null;
		if (_buttons.TryGetValue(buttonName, out value))
		{
			value.gameObject.SetActiveRecursivelyMk1(setActive: true);
			return;
		}
		Utils.Log("ShowButton failed: can't find button", buttonName);
	}

	private void Start()
	{
	}
}
