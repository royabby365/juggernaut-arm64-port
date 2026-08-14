using UnityEngine;

public class SliderThumb : SpriteButton, IDraggable
{
	internal Slider Slider;

	public void Drag(Vector3 from, Vector3 to)
	{
		Slider.Drag(from, to);
	}

	private void Awake()
	{
		Init(10, 10);
	}

	private void Start()
	{
		SetActive();
	}
}
