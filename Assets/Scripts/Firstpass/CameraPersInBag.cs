using System;
using UnityEngine;

internal class CameraPersInBag : MonoBehaviour
{
	private Vector3 _cameraCenter;

	private Vector3 _camerCenterStartPos;

	internal GameObject player;

	private GameObject inventory_center;

	private float rotation_reference;

	private float rotatespeed = 700f;

	private float cam_rotate;

	private bool cam_zoom_to_y_changed;

	private float scroll_max = 1f;

	private float scroll_min = -1f;

	private float cam_zoom_to_y_max = 0.5f;

	private float cam_zoom_to_y_min = -0.5f;

	private float cam_zoom;

	private float cam_zoom_to_y;

	private Vector2 cam_distance;

	private Vector3 cam_last_pos;

	private Transform _cameraTransform;

	private float cam_rotate_init = 180f;

	public Rect SelectArea;

	public Vector2 InputOffset;

	public Vector2 InputPos;

	public float InputZoom;

	public Vector2 InputZoomStartPos;

	public Vector2 InputZoomStartPos1;

	public Vector2 InputZoomStartPos2;

	private bool _init = true;

	private bool IsPointIn3DArea(Vector2 point)
	{
		return point.x >= SelectArea.xMin && point.x <= SelectArea.xMax && point.y >= SelectArea.y && point.y <= SelectArea.y + SelectArea.height;
	}

	private void LateUpdate()
	{
		GameObject gameObject = player;
		if (inventory_center == null && player != null)
		{
			InitInventoryCenter();
		}
		if (!(player != null) || !(inventory_center != null))
		{
			return;
		}
		if (IsPointIn3DArea(InputPos) && InputOffset.x != 0f)
		{
			rotation_reference = InputOffset.x * ((float)Math.PI / 180f) * 2f * Time.deltaTime * (0f - rotatespeed);
			player.transform.Rotate(new Vector3(0f, 1f, 0f), rotation_reference);
			InputOffset.x = 0f;
		}
		if (cam_rotate < -45f)
		{
			cam_rotate = -45f;
		}
		if (cam_rotate > 0f)
		{
			cam_rotate = 0f;
		}
		bool flag = InputZoom != 0f && IsPointIn3DArea(InputZoomStartPos) && IsPointIn3DArea(InputZoomStartPos1) && IsPointIn3DArea(InputZoomStartPos2);
		if (cam_zoom_to_y_changed)
		{
			cam_zoom = scroll_max;
			cam_zoom_to_y = cam_zoom_to_y_max;
		}
		else if (flag)
		{
			float inputZoom = InputZoom;
			if (inputZoom != 0f)
			{
				cam_zoom += inputZoom * 3f;
				if (cam_zoom < scroll_min)
				{
					cam_zoom = scroll_min;
				}
				if (cam_zoom > scroll_max)
				{
					cam_zoom = scroll_max;
				}
				if ((double)InputZoomStartPos.y > (double)SelectArea.y + (double)SelectArea.height / 2.0)
				{
					if (inputZoom > 0f)
					{
						if (cam_zoom_to_y > cam_zoom_to_y_min)
						{
							cam_zoom_to_y -= inputZoom;
						}
					}
					else
					{
						cam_zoom_to_y -= inputZoom;
						if (cam_zoom_to_y > 0f)
						{
							cam_zoom_to_y = 0f;
						}
					}
				}
				else if (inputZoom > 0f)
				{
					if (cam_zoom_to_y < cam_zoom_to_y_max)
					{
						cam_zoom_to_y += inputZoom;
					}
				}
				else
				{
					cam_zoom_to_y += inputZoom;
					if (cam_zoom_to_y < 0f)
					{
						cam_zoom_to_y = 0f;
					}
				}
			}
		}
		cam_distance = new Vector2(1.2f, 1.2f);
		_cameraCenter = _camerCenterStartPos + new Vector3(0f, cam_zoom_to_y, cam_distance.x * 1.5f + cam_distance.y * 1.5f - cam_zoom);
		if (gameObject != player)
		{
			_cameraTransform.position = _cameraCenter;
			cam_last_pos = _cameraTransform.position;
			if (!cam_zoom_to_y_changed)
			{
				cam_zoom = 0f;
				cam_rotate = 0f;
			}
		}
		if (cam_zoom_to_y_changed)
		{
			cam_zoom_to_y_changed = false;
		}
		if (_init)
		{
			_init = false;
			cam_last_pos = _cameraCenter;
		}
		_cameraTransform.position = Vector3.Slerp(cam_last_pos, _cameraCenter, Time.time / 150f);
		cam_last_pos = _cameraTransform.position;
		_cameraTransform.RotateAround(inventory_center.transform.position, new Vector3(1f, 0f, 0f), cam_rotate);
		_cameraTransform.RotateAround(inventory_center.transform.position, new Vector3(0f, 1f, 0f), cam_rotate_init);
		_cameraTransform.LookAt(inventory_center.transform.position + new Vector3(0f, 1f + cam_zoom_to_y, 0f));
	}

	private void InitInventoryCenter()
	{
		GameObject gameObject = GameObject.Find("InventoryCam");
		gameObject.transform.parent = null;
		_cameraTransform = gameObject.transform;
		inventory_center = GameObject.Find("inventory_center");
		if (inventory_center == null)
		{
			inventory_center = GameObject.Find("arena_center");
		}
		_cameraCenter = inventory_center.transform.position + new Vector3(0f, 1f, 0f);
		_camerCenterStartPos = _cameraCenter;
		gameObject.transform.position = _cameraCenter;
		cam_last_pos = _cameraCenter;
	}
}
