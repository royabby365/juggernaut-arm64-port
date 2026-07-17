using UnityEngine;

public class ToggleSliderThumb : SpriteButton, IDraggable
{
	internal ToggleSlider Slider;

	private void Awake()
	{
		Init(10, 4);
	}

	private void Start()
	{
		SetActive();
	}

	private void OnEnable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.DragEnd += Instance_DragEnd;
		}
	}

	private void OnDisable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.DragEnd -= Instance_DragEnd;
		}
	}

	private void Instance_DragEnd(Vector3 pos)
	{
		if (HudMk1.Instance != null && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.Options)
		{
			Slider.DragEnd(pos);
		}
	}

	public void Drag(Vector3 from, Vector3 to)
	{
		Slider.Drag(from, to);
	}

	public override void Released()
	{
		base.Released();
		Slider.Value = !Slider.Value;
	}
}
