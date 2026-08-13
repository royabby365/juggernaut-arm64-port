using System.Collections;
using UnityEngine;
using Yarx;

public class AchievementButton : SpriteButton
{
	private const float FlashPeriodSec = 1f;

	private CompositeDisposable _subscriptions;

	public SpriteText Count;

	public Vector3 EnteredScale = new Vector3(1.1f, 1.1f, 1f);

	public Color EnteredTint = new Color(0.7f, 0.7f, 0.7f, 1f);

	public Sprite Icon;

	private Sprite _sprite;

	private bool _flashing;

	private AnimationCurve _flashingCurve;

	private static readonly Vector3 MediumLocalScale = new Vector3(1.1f, 1.1f, 1f);

	private static readonly Vector3 SmallLocalScale = new Vector3(0.8f, 0.8f, 1f);

	private Color _add = new Color(0.19f, 0.19f, 0.19f, 1f);

	private void Awake()
	{
		_sprite = GetComponent<Sprite>();
		Init();
		_flashingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		_flashingCurve.postWrapMode = WrapMode.PingPong;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GameEvents.Event, string>.AddListener(Globals.MsgGameEventProgressChanged, Handler));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgRatingSocialMessageCulldown, OnMsgRatingSocialMessageCulldown));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgRatingSocialMessageClick, OnMsgRatingSocialMessageClick));
	}

	private void OnMsgRatingSocialMessageClick()
	{
	}

	private void OnMsgRatingSocialMessageCulldown()
	{
		_flashing = true;
		StartCoroutine(FlashMe());
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.AchievementsScroll)
		{
			_flashing = false;
		}
	}

	private void Handler(GameEvents.Event @event, string reason)
	{
		if (@event.Progress >= @event.MaxProgress)
		{
			_flashing = true;
			StartCoroutine(FlashMe());
			UpdateCount();
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		UpdateCount();
	}

	private void Update()
	{
	}

	private void UpdateCount()
	{
		int num = 0;
		if (MainMenu.GameEvents == null && !Globals.DebugShowAlwaysAchivsButtons)
		{
			SetInactive();
			return;
		}
		num = MainMenu.GameEvents.AllFinishedAchivsPoints;
		if (num > 0 || Globals.DebugShowAlwaysAchivsButtons)
		{
			SetActive();
			Count.Text_ = num.ToString();
		}
		else
		{
			SetInactive();
		}
	}

	public override void SetActive()
	{
		base.SetActive();
		GetComponent<Renderer>().ShowOrHide(show: true);
		Count.ShowOrHideMethod(show: true);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		GetComponent<Renderer>().ShowOrHide(show: false);
		Count.ShowOrHideMethod(show: false);
	}

	public override void Entered()
	{
		base.Entered();
		base.transform.localScale = EnteredScale;
		_sprite.Tint_ = EnteredTint;
	}

	public override void Left()
	{
		base.Left();
		base.transform.localScale = Vector3.one;
		_sprite.Tint_ = Color.gray;
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
}
