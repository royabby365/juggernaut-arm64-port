using UnityEngine;

internal class IPadCameraMove
{
	private ITouchscreen _touchscreen;

	private Vector3 _rotatePoint;

	private Camera _camera;

	private Battle _battle;

	private bool _enabled;

	internal IPadCameraMove(Battle battle)
	{
		_battle = battle;
		battle.CameraMoveModeEnabled += delegate(bool _)
		{
			_enabled = _;
			_camera = Camera.main;
			_rotatePoint = Utils.Midpoint(Globals.Player.transform.position, Globals.Enemy.transform.position);
			_touchscreen = Globals.CreateTouchscreen();
			if (_)
			{
				_battle.FreeCamera();
				_touchscreen.OnTouchMove += Touchscreen_OnTouchMove;
			}
			else
			{
				_touchscreen.OnTouchMove -= Touchscreen_OnTouchMove;
			}
		};
	}

	private void Touchscreen_OnTouchMove(Vector2 offset, Vector2 pos)
	{
		if (_enabled)
		{
			ProcessMovement(offset.x);
		}
	}

	private void ProcessMovement(float offset)
	{
		if (offset != 0f)
		{
			_camera.transform.RotateAround(_rotatePoint, Vector3.up, Time.deltaTime * 45f * (float)((!(offset < 0f)) ? 1 : (-1)));
		}
	}
}
