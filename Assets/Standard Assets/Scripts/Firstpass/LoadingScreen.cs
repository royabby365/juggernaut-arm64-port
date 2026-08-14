using System.Collections;
using UnityEngine;
using Yarx;

public class LoadingScreen : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public MeshFilter background;

	public int screensCount = 16;

	public SpriteText versionNumber;

	public SpriteText loadingTips;

	private ActionD _action;

	public bool IsVisible => GetComponent<Camera>() != null && GetComponent<Camera>().enabled;

	public void ShowLoadingScreen(ActionD action)
	{
		if (Globals.IgnoreHud)
		{
			_action = action;
			StartCoroutine("LoadScreenAnimated2");
		}
		else if (!GetComponent<Camera>().enabled)
		{
			Utils.Log("ShowLoadingScreen");
			SpriteGui.DontReleaseButtons = true;
			_action = action;
			StartCoroutine("LoadScreenAnimated");
			GetComponent<Camera>().enabled = true;
		}
		else
		{
			action?.Invoke();
		}
	}

	public void RefreshLoadingScreen(ActionD action)
	{
		if (!GetComponent<Camera>().enabled)
		{
			ShowLoadingScreen(action);
			return;
		}
		Utils.Log("RefreshLoadingScreen");
		_action = action;
		StartCoroutine("RefreshScreenAnimated");
	}

	public IEnumerator LoadScreenAnimated2()
	{
		while (SingletonT<ServerData>.I.PlayerServerPersData == null)
		{
			yield return null;
		}
		_action();
	}

	public void HideLoadingScreen()
	{
		if (GetComponent<Camera>().enabled)
		{
			Utils.Log("HideLoadingScreen");
			Messenger.Invoke(Globals.MsgLoadingScreenHided);
			StopAllCoroutines();
			GetComponent<Camera>().enabled = false;
			SpriteGui.DontReleaseButtons = false;
			GenerateNewTip();
		}
	}

	private void Awake()
	{
		versionNumber.Text_ = " ";
		GenerateNewTip();
		ServerData.OnLoadingTipsReady += GenerateTipOnReady;
	}

	private void GenerateTipOnReady(ServerData data)
	{
		if (loadingTips.Text_ == string.Empty)
		{
			GenerateNewTip(data);
		}
	}

	private void GenerateNewTip()
	{
		GenerateNewTip(SingletonT<ServerData>.I);
	}

	private void GenerateNewTip(ServerData serverData)
	{
		if (serverData.LoadingTips != null && serverData.LoadingTips.Count > 0)
		{
			string text_ = serverData.LoadingTips[Random.Range(0, serverData.LoadingTips.Count)];
			loadingTips.Text_ = text_;
		}
		else
		{
			loadingTips.Text_ = string.Empty;
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Update()
	{
		Transform transform = background.transform;
		float num = Camera2D.ScreenWidth;
		float num2 = Camera2D.ScreenHeight;
		float num3 = Mathf.Max(num / 1024f, num2 / 768f);
		transform.localScale = new Vector3(num3, num3, 1f);
		Vector3 localPosition = transform.localPosition;
		localPosition.x = (0f - num) / 2f;
		transform.localPosition = localPosition;
	}

	private IEnumerator LoadScreenAnimated()
	{
		int n = Random.Range(1, screensCount);
		string screenName = $"jugger_iPad_Load_screens_{n:00}";
		string screenPath = "load_screen/fragments/" + screenName;
		Texture2D tex = Util.Resource<Texture2D>(screenPath);
		background.GetComponent<Renderer>().material.mainTexture = tex;
		yield return null;
		if (_action != null)
		{
			ActionD t = _action;
			_action = null;
			t();
		}
	}

	private IEnumerator RefreshScreenAnimated()
	{
		yield return null;
		if (_action != null)
		{
			ActionD t = _action;
			_action = null;
			t();
		}
	}
}
