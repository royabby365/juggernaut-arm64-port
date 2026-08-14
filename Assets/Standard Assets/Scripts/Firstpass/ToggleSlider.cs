using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ToggleSlider : MonoBehaviour
{
	private bool _isUpdating;

	private bool _value;

	private int _prevWidth;

	private Camera _camera;

	public Sprite Background;

	public ToggleSliderThumb Thumb;

	public Sprite TrueValueSprite;

	public bool Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			UpdateGUI();
		}
	}

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<bool> ValueChanged;

	private void Start()
	{
		_camera = base.transform.GetSpriteGui().GetComponent<Camera>();
		Thumb.Slider = this;
		UpdateGUI();
	}

	private void OnEnable()
	{
		_isUpdating = false;
	}

	private void OnDisable()
	{
		StopCoroutine("UpdateGUICoro");
		_isUpdating = false;
	}

	internal void Drag(Vector3 from, Vector3 to)
	{
		if (!_isUpdating)
		{
			float num = to.x - from.x;
			num *= Camera2D.Scale;
			Vector3 vector = _camera.WorldToScreenPoint(Thumb.transform.position);
			if ((!(num < 0f) || !(to.x > vector.x)) && (!(num > 0f) || !(to.x < vector.x)))
			{
				float value = Thumb.transform.localPosition.x + num;
				value = Mathf.Clamp(value, GetLeftLimit(), GetRightLimit());
				value = Mathf.RoundToInt(value);
				Thumb.transform.localPosition = new Vector3(value, Thumb.transform.localPosition.y, Thumb.transform.localPosition.z);
				TrueValueSprite.ClipHorizontalLocal(-2000f, value + (float)(Background.Width / 2));
			}
		}
	}

	internal void DragEnd(Vector3 pos)
	{
		float x = Thumb.transform.localPosition.x;
		Value = x > 0f;
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
		StartCoroutine(UpdateGUICoro());
	}

	private IEnumerator UpdateGUICoro()
	{
		_isUpdating = true;
		float time = 0.2f;
		float startX = Thumb.transform.localPosition.x;
		float finishX = ((!Value) ? GetLeftLimit() : GetRightLimit());
		float timeScale = (Mathf.Max(startX, finishX) - Mathf.Min(startX, finishX)) / (GetRightLimit() - GetLeftLimit());
		time *= timeScale;
		float t = 0f;
		while (t < time)
		{
			float x = Mathf.Lerp(startX, finishX, t / time);
			Thumb.transform.localPosition = new Vector3(x, Thumb.transform.localPosition.y, Thumb.transform.localPosition.z);
			TrueValueSprite.ClipHorizontalLocal(-2000f, x + (float)(Background.Width / 2));
			t += Time.deltaTime;
			yield return null;
		}
		Thumb.transform.localPosition = new Vector3(finishX, Thumb.transform.localPosition.y, Thumb.transform.localPosition.z);
		TrueValueSprite.ClipHorizontalLocal(-2000f, finishX + (float)(Background.Width / 2));
		_isUpdating = false;
		OnValueChanged();
	}

	private float GetRightLimit()
	{
		Sprite component = Thumb.GetComponent<Sprite>();
		return (float)Background.Width - (float)component.Width * 0.5f - (float)Background.Width * 0.5f;
	}

	private float GetLeftLimit()
	{
		Sprite component = Thumb.GetComponent<Sprite>();
		return (float)component.Width * 0.5f - (float)Background.Width * 0.5f;
	}
}
