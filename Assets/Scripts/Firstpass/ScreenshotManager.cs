using System.Collections;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
	private string TextureName = "screenshot_frame";

	public void TakeScreenshot()
	{
		string text = "resources/screenshot_frame/" + TextureName;
		SingletonT<ResourcesManager>.I.GetAssetBundleAsync(this, ResourcesManager.GetAssetBundlePath(text), delegate(string _, ResourcesManager.AssetBundleData ab, float time)
		{
			StartCoroutine(TakeScreenshotImpl(ab));
		}, delegate(string _, string errorMessage)
		{
			Utils.LogForce("ScreenshotManager.TakeScreenshotImpl", errorMessage);
		});
	}

	private IEnumerator TakeScreenshotImpl(ResourcesManager.AssetBundleData ab)
	{
		yield return new WaitForEndOfFrame();
		Texture2D frame = (Texture2D)ab.Bundle.Load(TextureName);
		float croppedHeight = ((Screen.height <= 700) ? ((float)Screen.height) : ((float)Screen.height * 0.9f));
		float croppedWidth = croppedHeight * 4f / 3f;
		Rect rect = new Rect(((float)Screen.width - croppedWidth) / 2f, ((float)Screen.height - croppedHeight) / 2f, croppedWidth, croppedHeight);
		Debug.Log(string.Concat("Taking screenshot of screen at ", rect, ", frame size (", frame.width, ", ", frame.height, ")"));
		Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, mipmap: false);
		screenShot.ReadPixels(rect, 0, 0);
		TextureScale.Bilinear(screenShot, frame.width, frame.height);
		ComposePicture(frame, screenShot);
		screenShot.Apply();
		JPGEncoder jpgEncoder = new JPGEncoder(screenShot, 70f);
		jpgEncoder.doEncoding();
		byte[] bytes = jpgEncoder.GetBytes();
		FacebookPlugin.SubmitScreenshot(bytes);
		SingletonT<ResourcesManager>.I.RemoveAssetBundleAndDestroyAll(ab.Path);
	}

	private void ComposePicture(Texture2D frame, Texture2D screenShot)
	{
		for (int i = 0; i < screenShot.height; i++)
		{
			for (int j = 0; j < screenShot.width; j++)
			{
				Color pixel = screenShot.GetPixel(j, i);
				Color pixel2 = frame.GetPixel(j, i);
				pixel = Color.Lerp(pixel, pixel2, pixel2.a);
				screenShot.SetPixel(j, i, pixel);
			}
		}
	}
}
