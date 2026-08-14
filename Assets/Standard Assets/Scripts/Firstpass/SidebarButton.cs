using System.Collections;
using UnityEngine;
using Yarx;

public class SidebarButton : SpriteButton
{
	private const float FlashPeriodSec = 1f;

	private CompositeDisposable _subscriptions;

	public Sprite Frame;

	public Sprite Bg;

	public Sprite Icon;

	public SidebarNav NavBar;

	private static Sprite _nav0Icon;

	private static SidebarButton _nav0Button;

	private Color _add = new Color(0.19f, 0.19f, 0.19f, 1f);

	private Color _bg;

	private bool _flashing;

	private AnimationCurve _flashingCurve;

	private static readonly Vector3 BigLocalScale = new Vector3(1.2f, 1.2f, 1f);

	private static readonly Vector3 MediumLocalScale = new Vector3(1.1f, 1.1f, 1f);

	private static readonly Vector3 SmallLocalScale = new Vector3(0.8f, 0.8f, 1f);

	private readonly Color DarkTint = new Color32(100, 100, 100, 220);

	private static bool FightState;

	public static void ChangeMapToSwords(bool change)
	{
		FightState = change;
		if (!Globals.IgnoreHud)
		{
			if (_nav0Button._flashing && FightState)
			{
				_nav0Button._flashing = false;
			}
			_nav0Icon.SpriteName_ = ((!change) ? "nav-icon-map" : "nav_icon_fight");
		}
	}

	private void Awake()
	{
		Init(0, -6);
		_bg = Bg.Tint_;
		_flashingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		_flashingCurve.postWrapMode = WrapMode.PingPong;
		if (base.name == "global_nav_0")
		{
			_nav0Icon = Icon;
			_nav0Button = this;
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	public void TurnOnFlashing()
	{
		if (!FightState)
		{
			_flashing = true;
			StartCoroutine("FlashMe");
		}
	}

	public void TurnOffFlashing()
	{
		_flashing = false;
	}

	private IEnumerator FlashMe()
	{
		float startTime = Time.time;
		while (_flashing)
		{
			yield return null;
			float delta = Time.time - startTime;
			float dt = _flashingCurve.Evaluate(delta / 1f);
			Icon.transform.localScale = Vector3.Lerp(SmallLocalScale, MediumLocalScale, dt);
			Icon.Tint_ = Color.gray + Color.Lerp(Color.black, _add, dt);
		}
		Icon.transform.localScale = Vector3.one;
		Icon.Tint_ = Color.gray;
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	public override void Entered()
	{
		base.Entered();
		Bg.Tint_ = _bg + _add;
		_flashing = false;
		Icon.transform.localScale = BigLocalScale;
	}

	public override void Left()
	{
		base.Left();
		Bg.Tint_ = _bg;
		Icon.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public override void SetActive()
	{
		base.SetActive();
		Icon.Tint_ = Color.gray;
		NavBar.Rearrange();
	}

	public override void SetInactive()
	{
		base.SetInactive();
		Icon.Tint_ = DarkTint;
		NavBar.Rearrange();
	}
}
