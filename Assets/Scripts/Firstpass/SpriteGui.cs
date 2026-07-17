using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(Camera2D))]
public class SpriteGui : MonoBehaviour
{
	protected enum ActiveState
	{
		ActiveEntered,
		ActiveLeft,
		DragState,
		MovingDown,
		InactiveDown,
		InactiveUp
	}

	private static bool _dontReleaseButtons;

	private static float _blockReleaseUntil;

	private static int _uniqueId;

	private SpriteButton _activeButton;

	protected ActiveState _activeState = ActiveState.InactiveUp;

	private float _activeTimer;

	private Vector3 _activeMousePos;

	private bool _atlasRegenerated;

	protected Dictionary<string, SpriteButton> _buttons = new Dictionary<string, SpriteButton>();

	protected Camera _camera2d;

	protected bool _hidden;

	protected int _mask;

	protected int _gpidx;

	protected Vector3[] _gesture_points;

	protected bool _dontInitCamera2d;

	protected static float _colliderDistance = 2400f;

	public static bool DontReleaseButtons
	{
		private get
		{
			return _dontReleaseButtons;
		}
		set
		{
			Utils.LogForce("DontReleaseButtons", _dontReleaseButtons, value);
			_dontReleaseButtons = value;
		}
	}

	public static bool IsGuiActive => !DontReleaseButtons && Time.time > _blockReleaseUntil;

	public SpriteButton ActiveButton => _activeButton;

	public bool IsLocked { get; private set; }

	public static int UniqueId => _uniqueId++;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<SpriteButton> Click;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<SpriteButton> Release;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<SpriteButton, Vector2> ReleaseWithMousePosition;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<SpriteButton> LongPress;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<Vector3, Vector3> Move;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<Vector3> MoveTouch;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<Vector3> MoveBegin;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<Vector3> MoveEnd;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<Vector3, Vector3> Drag;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<Vector3> DragBegin;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<Vector3> DragEnd;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<SpriteButton> DragEndWithButton;

	public static void BlockReleaseUntil(float time)
	{
		_blockReleaseUntil = Mathf.Max(_blockReleaseUntil, time);
	}

	public void ResetGui()
	{
		_activeState = ActiveState.InactiveUp;
	}

	public virtual void RegenerateAtlas()
	{
		if (_atlasRegenerated)
		{
			return;
		}
		TexturePacker component = base.transform.GetComponent<TexturePacker>();
		if (component == null)
		{
			Transform transform = base.transform.FindChildByName("left_top");
			if (transform != null)
			{
				component = transform.transform.GetComponent<TexturePacker>();
			}
		}
		if (component != null)
		{
			component.RegenerateAtlas();
			_atlasRegenerated = true;
		}
	}

	public virtual void RegisterButton(SpriteButton btn)
	{
		if (_buttons.ContainsKey(btn.name) && Globals.IsDebugBuild)
		{
			Debug.LogError($"button name collision: {btn.name}");
		}
		_buttons[btn.name] = btn;
	}

	public virtual void UnregisterButton(SpriteButton btn)
	{
		if (_buttons.ContainsKey(btn.name))
		{
			_buttons.Remove(btn.name);
		}
	}

	public virtual void UnselectButton(string buttonName)
	{
		SpriteButton button = GetButton(buttonName);
		if (button != null)
		{
			button.SetUnselected();
		}
	}

	public virtual void SelectButton(string buttonName)
	{
		SpriteButton button = GetButton(buttonName);
		if (button != null)
		{
			button.SetSelected();
		}
	}

	public virtual void SetButtonActive(string buttonName)
	{
		SpriteButton button = GetButton(buttonName);
		if (button != null)
		{
			button.SetActive();
			return;
		}
		Utils.Log("NO BUTTON", buttonName);
	}

	public virtual void SetButtonInactive(string buttonName)
	{
		SpriteButton button = GetButton(buttonName);
		if (button != null)
		{
			button.SetInactive();
		}
	}

	public virtual void LockHud()
	{
		IsLocked = true;
	}

	public virtual void UnlockHud()
	{
		IsLocked = false;
	}

	public virtual void HideHud()
	{
		_hidden = true;
		if (_camera2d == null)
		{
			InitCamera2D();
		}
		if (_camera2d != null)
		{
			_camera2d.enabled = false;
		}
		GetComponent<Camera>().enabled = false;
	}

	public virtual void UnhideHud()
	{
		_hidden = false;
		if (_camera2d == null)
		{
			InitCamera2D();
		}
		if (_camera2d != null)
		{
			_camera2d.enabled = true;
		}
	}

	public bool CheckCollider(Collider collider, Vector3 screenPos)
	{
		if (_camera2d == null || collider == null)
		{
			return false;
		}
		RaycastHit hitInfo;
		return GetComponent<Collider>()Raycast(_camera2d.ScreenPointToRay(screenPos), out hitInfo, _colliderDistance);
	}

	protected virtual void ProcessRayCast()
	{
		if (_camera2d == null)
		{
			InitCamera2D();
		}
		if (!(_camera2d != null) || IsLocked || _hidden || !_camera2d.enabled)
		{
			return;
		}
		bool mouseButton = Input.GetMouseButton(0);
		if (!Input.GetMouseButtonUp(0) && !mouseButton)
		{
			return;
		}
		SpriteButton button = null;
		Vector3 mousePosition = Input.mousePosition;
		Ray ray = _camera2d.ScreenPointToRay(mousePosition);
		RaycastHit[] array = Physics.RaycastAll(ray, _colliderDistance, _mask);
		Array.Sort(array, (RaycastHit left, RaycastHit right) => left.distance.CompareTo(right.distance));
		RaycastHit[] array2 = array;
		foreach (RaycastHit raycastHit in array2)
		{
			SpriteButton button2 = GetButton(raycastHit.transform.name);
			if (button2 != null && button2.Active)
			{
				button = button2;
				break;
			}
		}
		ActiveState activeState = _activeState;
		_activeState = ChangeActiveState(mouseButton, button, mousePosition);
	}

	protected ActiveState ChangeActiveState(bool down, SpriteButton button, Vector3 mousePosition)
	{
		if (_activeButton != null && !_activeButton.Active && _activeState != ActiveState.DragState)
		{
			_activeButton = null;
			_activeState = ((!down) ? ActiveState.InactiveUp : ActiveState.InactiveDown);
		}
		if (down)
		{
			IDraggable draggable = _activeButton as IDraggable;
			switch (_activeState)
			{
			case ActiveState.ActiveEntered:
			{
				if (button != _activeButton && draggable == null)
				{
					if (_activeButton != null)
					{
						_activeButton.Left();
					}
					return ActiveState.ActiveLeft;
				}
				float time = Time.time;
				if (button != null && time - _activeTimer >= button.LongPressInterval)
				{
					OnButtonLongPress(button);
					if (_activeButton != null)
					{
						_activeButton.Left();
					}
					_activeButton = null;
					return _activeState = ActiveState.InactiveDown;
				}
				if (draggable != null)
				{
					Vector3 activeMousePos3 = _activeMousePos;
					_activeMousePos = mousePosition;
					if ((activeMousePos3 - mousePosition).sqrMagnitude >= 1f)
					{
						OnMoveBegin(mousePosition);
						OnDragBegin(mousePosition);
						return ActiveState.DragState;
					}
				}
				return _activeState;
			}
			case ActiveState.ActiveLeft:
				if (button == _activeButton)
				{
					if (_activeButton != null)
					{
						_activeButton.Entered();
					}
					_activeTimer = Time.time;
					_activeMousePos = mousePosition;
					return ActiveState.ActiveEntered;
				}
				return _activeState;
			case ActiveState.DragState:
			{
				Vector3 activeMousePos = _activeMousePos;
				_activeMousePos = mousePosition;
				if (draggable != null)
				{
					draggable.Drag(activeMousePos, mousePosition);
					OnMove(activeMousePos, mousePosition);
					OnDrag(activeMousePos, mousePosition);
				}
				return _activeState;
			}
			case ActiveState.InactiveUp:
				if (button == null)
				{
					_activeMousePos = mousePosition;
					OnMoveBegin(_activeMousePos);
					Messenger.Invoke(Globals.MsgGestureStarted);
					return ActiveState.MovingDown;
				}
				_activeButton = button;
				_activeTimer = Time.time;
				_activeMousePos = mousePosition;
				if (IsGuiActive)
				{
					_activeButton.Entered();
					_activeButton.Clicked();
				}
				OnButtonClick(button);
				OnMoveTouch(mousePosition);
				return ActiveState.ActiveEntered;
			case ActiveState.InactiveDown:
				return ActiveState.InactiveDown;
			case ActiveState.MovingDown:
			{
				Vector3 activeMousePos2 = _activeMousePos;
				_activeMousePos = mousePosition;
				OnMove(activeMousePos2, mousePosition);
				return ActiveState.MovingDown;
			}
			default:
				if (Globals.IsDebugBuild)
				{
					Debug.LogError("cannot be here! " + _activeState);
				}
				return _activeState;
			}
		}
		switch (_activeState)
		{
		case ActiveState.InactiveUp:
			return ActiveState.InactiveUp;
		case ActiveState.InactiveDown:
			return ActiveState.InactiveUp;
		case ActiveState.MovingDown:
			OnMoveEnd(mousePosition);
			return ActiveState.InactiveUp;
		case ActiveState.ActiveEntered:
			if (_activeButton != null)
			{
				_activeButton.Left();
				_activeButton.Released();
				OnButtonRelease(_activeButton);
				OnReleaseWithMousePosition(_activeButton, mousePosition);
			}
			_activeButton = null;
			return _activeState = ActiveState.InactiveUp;
		case ActiveState.DragState:
			OnMoveEnd(mousePosition);
			OnDragEnd(mousePosition);
			OnDragEndWithButton(_activeButton);
			_activeButton = null;
			return _activeState = ActiveState.InactiveUp;
		case ActiveState.ActiveLeft:
			_activeButton = null;
			return _activeState = ActiveState.InactiveUp;
		default:
			if (Globals.IsDebugBuild)
			{
				Debug.LogError("cannot be here! " + _activeState);
			}
			return _activeState;
		}
	}

	protected void OnMoveTouch(Vector3 pos)
	{
		this.MoveTouch?.Invoke(pos);
	}

	protected void OnMoveBegin(Vector3 begin)
	{
		this.MoveBegin?.Invoke(begin);
	}

	protected void OnMoveEnd(Vector3 end)
	{
		this.MoveEnd?.Invoke(end);
	}

	protected void OnMove(Vector3 begin, Vector3 end)
	{
		this.Move?.Invoke(begin, end);
	}

	protected void OnDragBegin(Vector3 begin)
	{
		this.DragBegin?.Invoke(begin);
	}

	protected void OnDragEnd(Vector3 end)
	{
		this.DragEnd?.Invoke(end);
	}

	protected void OnDragEndWithButton(SpriteButton button)
	{
		Action<SpriteButton> dragEndWithButton = this.DragEndWithButton;
		if (dragEndWithButton != null && IsGuiActive)
		{
			dragEndWithButton(button);
		}
	}

	protected void OnDrag(Vector3 begin, Vector3 end)
	{
		this.Drag?.Invoke(begin, end);
	}

	protected void OnButtonClick(SpriteButton button)
	{
		this.Click?.Invoke(button);
	}

	protected void OnButtonRelease(SpriteButton button)
	{
		Action<SpriteButton> release = this.Release;
		if (release != null && IsGuiActive)
		{
			release(button);
		}
	}

	protected void OnReleaseWithMousePosition(SpriteButton button, Vector2 arg2)
	{
		this.ReleaseWithMousePosition?.Invoke(button, arg2);
	}

	protected void OnButtonLongPress(SpriteButton button)
	{
		this.LongPress?.Invoke(button);
	}

	public void ForeachButton(ActionD<SpriteButton> action)
	{
		foreach (KeyValuePair<string, SpriteButton> button in _buttons)
		{
			action(button.Value);
		}
	}

	public void ForeachButton(FuncD<string, bool> cond, ActionD<SpriteButton> action)
	{
		foreach (KeyValuePair<string, SpriteButton> button in _buttons)
		{
			if (cond(button.Key))
			{
				action(button.Value);
			}
		}
	}

	public SpriteButton GetButton(string buttonName)
	{
		if (_buttons.ContainsKey(buttonName))
		{
			return _buttons[buttonName];
		}
		return null;
	}

	protected void InitCamera2D()
	{
		if (!_dontInitCamera2d)
		{
			_camera2d = base.transform.GetComponent<Camera>();
			_mask = 1 << base.gameObject.layer;
		}
	}
}
