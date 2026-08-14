using System.Runtime.CompilerServices;
using UnityEngine;

internal abstract class TouchscreenBase : ITouchscreen
{
	protected bool _isTouch;

	protected Vector2 _touchStartPosition = Vector2.zero;

	private float _startTime;

	[method: MethodImpl((MethodImplOptions)32)]
	public event TouchStartD OnTouchStart;

	[method: MethodImpl((MethodImplOptions)32)]
	public event TouchEndD OnTouchEnd;

	[method: MethodImpl((MethodImplOptions)32)]
	public event TouchMoveD OnTouchMove;

	[method: MethodImpl((MethodImplOptions)32)]
	public event ZoomD OnZoom;

	protected void TouchStarted(Vector2 position)
	{
		_isTouch = true;
		_touchStartPosition = position;
		_startTime = Time.realtimeSinceStartup;
		if (this.OnTouchStart != null)
		{
			this.OnTouchStart(position);
		}
	}

	protected void TouchMoved(Vector2 offset, Vector2 pos)
	{
		if (this.OnTouchMove != null)
		{
			this.OnTouchMove(offset, pos);
		}
	}

	protected void TouchEnd(Vector2 position)
	{
		if (_isTouch)
		{
			_isTouch = false;
			if (this.OnTouchEnd != null)
			{
				this.OnTouchEnd(_touchStartPosition, position, Time.realtimeSinceStartup - _startTime);
			}
		}
	}

	protected void Zoom(float offset, Vector2 startPos, Vector2 startPos1, Vector2 startPos2)
	{
		if (this.OnZoom != null)
		{
			this.OnZoom(offset, startPos, startPos1, startPos2);
		}
	}

	public virtual void Clear()
	{
	}

	public abstract void Update();
}
