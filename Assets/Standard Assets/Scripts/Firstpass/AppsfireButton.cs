using System.Collections;
using UnityEngine;
using Yarx;

public class AppsfireButton : SpriteButton
{
	private const float FlashPeriodSec = 1f;

	private CompositeDisposable _subscriptions;

	public Vector3 PushScale = new Vector3(1f, 1.1f, 1f);

	public Color OverColor = new Color(0.7f, 0.7f, 0.7f, 1f);

	public Sprite Button;

	private bool _flashing;

	private AnimationCurve _flashingCurve;

	private void Awake()
	{
		_flashingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		_flashingCurve.postWrapMode = WrapMode.PingPong;
		Init();
		if (UnityApi.UseGameClub())
		{
			base.gameObject.SetActiveRecursively(state: false);
		}
	}

	private void Start()
	{
		if (!UnityApi.UseGameClub())
		{
			StartCoroutine("CheckAppsfire");
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		if (_subscriptions != null)
		{
			_subscriptions.Dispose();
		}
		StopAllCoroutines();
	}

	public override void Entered()
	{
		base.Entered();
		Button.Tint_ = OverColor;
		base.transform.localScale = PushScale;
	}

	public override void Left()
	{
		base.Left();
		Button.Tint_ = Color.gray;
		base.transform.localScale = Vector3.one;
	}

	public override void Released()
	{
		base.Released();
		_flashing = false;
		UnityApi.AFPresentNotifications();
	}

	private IEnumerator FlashMe()
	{
		float startTime = Time.time;
		while (_flashing)
		{
			yield return null;
			float delta = Time.time - startTime;
			float dt = _flashingCurve.Evaluate(delta / 1f);
			base.transform.localScale = Vector3.Lerp(Vector3.one, PushScale, dt);
		}
		base.transform.localScale = Vector3.one;
	}

	private IEnumerator CheckAppsfire()
	{
		while (true)
		{
			yield return new WaitForSeconds(20f);
			if (_flashing)
			{
				if (UnityApi.GetNumberOfPendingNotifications() <= 0)
				{
					_flashing = false;
				}
			}
			else if (SingletonT<ServerData>.I.PlayerParams != null && SingletonT<ServerData>.I.GameSettings != null && SingletonT<ServerData>.I.PlayerParams.Level >= SingletonT<ServerData>.I.GameSettings.LevelCheckNotifs)
			{
				int n = UnityApi.GetNumberOfPendingNotifications();
				if (n > 0)
				{
					StartFlashMe();
				}
			}
		}
	}

	private void StartFlashMe()
	{
		_flashing = true;
		StartCoroutine("FlashMe");
		Messenger.Invoke(Globals.MsgAppsfireNotification);
	}
}
