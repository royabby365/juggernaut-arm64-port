using UnityEngine;

internal class TouchscreenMouse : TouchscreenBase
{
	private bool _zoomStarted;

	private Vector2 _zoomStartPos;

	private Vector2 _mousePos = Vector2.zero;

	public override void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		if (Input.GetMouseButton(0))
		{
			if (_isTouch)
			{
				Vector2 offset = new Vector2(mousePosition.x - _mousePos.x, mousePosition.y - _mousePos.y);
				TouchMoved(offset, mousePosition);
			}
			else
			{
				TouchStarted(mousePosition);
			}
		}
		else if (_isTouch)
		{
			TouchEnd(mousePosition);
		}
		_mousePos = mousePosition;
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis != 0f)
		{
			if (!_zoomStarted)
			{
				_zoomStarted = true;
				_zoomStartPos = Input.mousePosition;
			}
			Zoom(axis, _zoomStartPos, _zoomStartPos, _zoomStartPos);
		}
		else if (_zoomStarted)
		{
			_zoomStarted = false;
		}
	}
}
