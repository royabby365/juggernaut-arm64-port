using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
public class Slider : MonoBehaviour
{
	private float _value;

	private int _prevWidth;

	private Camera _camera;

	public SliderThumb Thumb;

	public Sprite ScaleBackground;

	public Sprite ScaleForeground;

	public int PixelWidth = 100;

	public float Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value != _value)
			{
				_value = value;
				OnValueChanged();
				UpdateGUI();
			}
		}
	}

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<float> ValueChanged;

	private void Start()
	{
		_camera = base.transform.GetSpriteGui().GetComponent<Camera>();
		Thumb.Slider = this;
		UpdateGUI();
	}

	private void Update()
	{
		if (_prevWidth != PixelWidth)
		{
			UpdateGUI();
		}
		_prevWidth = PixelWidth;
	}

	private void OnValueChanged()
	{
		if (this.ValueChanged != null)
		{
			this.ValueChanged(_value);
		}
	}

	private void UpdateGUI()
	{
		int num = Mathf.RoundToInt((float)PixelWidth * _value);
		ScaleForeground.Width = num;
		ScaleForeground.Refresh();
		ScaleBackground.Width = PixelWidth;
		ScaleBackground.Refresh();
		Thumb.transform.localPosition = new Vector3(num, Thumb.transform.localPosition.y, Thumb.transform.localPosition.z);
	}

	internal void Drag(Vector3 from, Vector3 to)
	{
		float num = to.x - from.x;
		num *= Camera2D.Scale;
		Vector3 vector = _camera.WorldToScreenPoint(Thumb.transform.position);
		if ((!(num < 0f) || !(to.x > vector.x)) && (!(num > 0f) || !(to.x < vector.x)))
		{
			float num2 = PixelWidth;
			float value = Thumb.transform.localPosition.x + num;
			value = Mathf.Clamp(value, 0f, num2);
			Value = value / num2;
		}
	}
}
