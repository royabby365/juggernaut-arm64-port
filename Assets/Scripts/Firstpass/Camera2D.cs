using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Camera2D : MonoBehaviour
{
	private static float _scale = 1f;

	public bool AdjustAspect = true;

	public float ManualScale = 1f;

	public bool enable = true;

	public bool forceHalfPixelShift;

	public float DebugScaleFactor;

	private Camera Camera;

	public static float Scale
	{
		get
		{
			return _scale;
		}
		set
		{
			_scale = value;
		}
	}

	public static int ScreenWidth
	{
		get
		{
			GetScreenSize(out var width, out var _);
			return (int)width;
		}
	}

	public static int ScreenHeight
	{
		get
		{
			GetScreenSize(out var _, out var height);
			return (int)height;
		}
	}

	public static void GetScreenSize(Camera camera, out float width, out float height)
	{
		width = GetComponent<Camera>()pixelWidth;
		height = GetComponent<Camera>()pixelHeight;
		AdjustScreenSize(ref width, ref height);
	}

	public static void GetScreenSize(out float width, out float height)
	{
		width = Screen.width;
		height = Screen.height;
		AdjustScreenSize(ref width, ref height);
	}

	private static void AdjustScreenSize(ref float width, ref float height)
	{
		if (height < 640f)
		{
			float num = Mathf.Max(853.3f / width, 640f / height);
			width *= num;
			height *= num;
		}
	}

	private static float PixelShift(float pixels)
	{
		return Scale * 0.5f - (float)(Mathf.RoundToInt(pixels) % 2) / 2f;
	}

	private void Awake()
	{
		Camera = base.gameObject.camera;
		UpdateScaleParams();
	}

	private void UpdateScaleParams()
	{
		if (!forceHalfPixelShift)
		{
			forceHalfPixelShift = false;
			switch (Application.platform)
			{
			case RuntimePlatform.WindowsPlayer:
				forceHalfPixelShift = true;
				break;
			case RuntimePlatform.WindowsWebPlayer:
				forceHalfPixelShift = true;
				break;
			case RuntimePlatform.WindowsEditor:
				forceHalfPixelShift = true;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.OSXWebPlayer:
			case RuntimePlatform.OSXDashboardPlayer:
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.PS3:
			case RuntimePlatform.XBOX360:
			case RuntimePlatform.Android:
			case RuntimePlatform.NaCl:
				break;
			}
		}
		Scale = GetScaleFromResolution();
		DebugScaleFactor = Scale;
	}

	private float GetScaleFromResolution()
	{
		int width = Screen.width;
		int height = Screen.height;
		if (width == 2048 && height == 1536)
		{
			return 0.5f;
		}
		if (width == 480 && height == 320)
		{
			return 2f;
		}
		return 1f;
	}

	public static float GetScale()
	{
		return Mathf.Max(1f, 640f / (float)Screen.height);
	}

	private void OnPreCull()
	{
		if (enable)
		{
			float width;
			float height;
			if (AdjustAspect)
			{
				GetScreenSize(Camera, out width, out height);
			}
			else
			{
				ManualScale = GetScale();
				width = ManualScale * Camera.pixelWidth;
				height = ManualScale * Camera.pixelHeight;
			}
			Camera.orthographicSize = Mathf.Min(height, width) / 2f;
			Matrix4x4 projectionMatrix = Matrix4x4.Ortho((0f - width) / 2f, width / 2f, (0f - height) / 2f, height / 2f, Camera.nearClipPlane, Camera.farClipPlane);
			if (forceHalfPixelShift)
			{
				projectionMatrix *= Matrix4x4.TRS(new Vector3(0f - PixelShift(width), PixelShift(height)), Quaternion.identity, Vector3.one);
			}
			Camera.projectionMatrix = projectionMatrix;
		}
	}
}
