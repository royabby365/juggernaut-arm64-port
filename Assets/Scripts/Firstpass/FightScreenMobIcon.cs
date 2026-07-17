using System;
using System.Collections;
using UnityEngine;
using Yarx;

public class FightScreenMobIcon : SpriteButton
{
	public enum State
	{
		Active,
		Inactive,
		Skull,
		OutOfGui
	}

	private CompositeDisposable _subscriptions;

	private Vector3 originalIconPos;

	public Sprite Frame;

	public Sprite Icon;

	public Sprite Skull;

	public Sprite BossBage;

	public string BossBageName = "boss_bage";

	public string InactivenameSuffix = "_sepia";

	private static readonly Color SepiaToneFrame = new Color32(44, 44, 44, byte.MaxValue);

	private static readonly Color NeutralToneFrame = new Color32(86, 86, 86, byte.MaxValue);

	private static readonly Color BrightToneFrame = new Color32(202, 189, 160, byte.MaxValue);

	private static readonly Color SepiaToneIcon = new Color32(122, 110, 104, byte.MaxValue);

	private string _iconName = "01_10";

	private State _state = State.Inactive;

	private bool _boss;

	public Vector3 OriginalIconPos => originalIconPos;

	public void SetIcon(string iconName)
	{
		_iconName = iconName;
		SetState(_state);
	}

	public void SetMeBoss()
	{
		_boss = true;
		SetState(_state);
	}

	public void ResetBoss()
	{
		_boss = false;
		SetState(_state);
	}

	public void SetState(State state)
	{
		_state = state;
		BossBage.ShowOrHide(_boss);
		if (state != State.OutOfGui)
		{
			Vector3 localPosition = base.transform.localPosition;
			base.transform.localPosition = localPosition + new Vector3(0f, 0f - localPosition.y, 0f);
		}
		if (base.name == "mob_icon_9")
		{
			Vector3 localPosition2 = base.transform.localPosition;
			base.transform.localPosition = new Vector3(localPosition2.x, localPosition2.y - 100f, localPosition2.z);
		}
		switch (state)
		{
		case State.Active:
			Frame.Tint_ = BrightToneFrame;
			Icon.SpriteName_ = _iconName;
			Icon.Tint_ = Color.gray;
			Skull.ShowOrHide(show: false);
			BossBage.SpriteName_ = BossBageName;
			break;
		case State.Inactive:
			Frame.Tint_ = NeutralToneFrame;
			Icon.SpriteName_ = _iconName + InactivenameSuffix;
			Icon.Tint_ = SepiaToneIcon;
			Skull.ShowOrHide(show: false);
			BossBage.SpriteName_ = BossBageName + InactivenameSuffix;
			break;
		case State.Skull:
			Frame.Tint_ = SepiaToneFrame;
			Icon.SpriteName_ = _iconName + InactivenameSuffix;
			Icon.Tint_ = SepiaToneIcon;
			Skull.ShowOrHide(show: true);
			BossBage.SpriteName_ = BossBageName + InactivenameSuffix;
			break;
		case State.OutOfGui:
		{
			Vector3 localPosition3 = base.transform.localPosition;
			base.transform.localPosition = localPosition3 + new Vector3(0f, 1000f, 0f);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("state");
		}
	}

	private void Awake()
	{
		Init(-3, 3);
		originalIconPos = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z);
		SetState(State.OutOfGui);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
	}

	private IEnumerator OnBecameVisible()
	{
		for (int i = 0; i < 2; i++)
		{
			yield return null;
		}
	}

	private void OnBecameInvisible()
	{
	}
}
