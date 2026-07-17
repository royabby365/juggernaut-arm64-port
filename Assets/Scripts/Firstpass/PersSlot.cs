using UnityEngine;

public class PersSlot : SpriteButton
{
	private const float LONG_PRESS_ENABLED = 1.2f;

	private const float LONG_PRESS_DISABLED = 9999f;

	public Transform selected;

	public ServerData.Slot.TypeE Slot;

	private void Awake()
	{
		Init();
		SetUnselected();
		base.LongPressInterval = 9999f;
	}

	public override void SetSelected()
	{
		base.SetSelected();
		selected.gameObject.active = true;
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
		selected.gameObject.active = false;
	}

	public void EnableLongPress(bool enable)
	{
		base.LongPressInterval = ((!enable) ? 9999f : 1.2f);
	}
}
