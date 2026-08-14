using System;
using System.Collections;
using UnityEngine;

public class ScreenshotCapturer : MonoBehaviour
{
	public SpriteButton ButtonScreenshot;

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += Instance_Release;
		}
	}

	private void Instance_Release(SpriteButton obj)
	{
		if (!(HudMk1.Instance == null) && obj == ButtonScreenshot)
		{
			StartCoroutine(SaveScreenshot());
		}
	}

	public IEnumerator SaveScreenshot()
	{
		ButtonScreenshot.transform.localScale = Vector3.zero;
		Messenger.Invoke(Globals.MsgScreenshotAlertShowing, arg1: true);
		yield return new WaitForEndOfFrame();
		string path = Application.persistentDataPath + "/SavedScreen.png";
		ScreenCapture.CaptureScreenshot(path);
		yield return new WaitForSeconds(2f);
		UnityApi.PostScreenshot();
		ButtonScreenshot.transform.localScale = Vector3.one;
		Messenger<ServerData.PhrasesE, Action>.Invoke(Globals.MsgShowAlertWithCallback, ServerData.PhrasesE.PostScreenshotToFacebook, delegate
		{
			Messenger.Invoke(Globals.MsgScreenshotAlertShowing, arg1: false);
		});
	}
}
