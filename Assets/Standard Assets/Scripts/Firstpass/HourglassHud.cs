using UnityEngine;
using Yarx;

public class HourglassHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private bool _isPlayerTime;

	private float _flickTime = 2f;

	public Sprite HourglassIcon;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<Battle.StateE>.AddListener(Globals.MsgBattleStateChanged, delegate(Battle.StateE state)
		{
			_isPlayerTime = state == Battle.StateE.PlayerTime;
			HourglassIcon.gameObject.SetActiveRecursivelyMk1(_isPlayerTime);
			HourglassIcon.Tint_ = new Color(1f, 1f, 1f, 0f);
		}));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgFightStarted, delegate
		{
			_isPlayerTime = true;
			HourglassIcon.gameObject.SetActiveRecursivelyMk1(_isPlayerTime);
			HourglassIcon.Tint_ = new Color(1f, 1f, 1f, 0f);
		}));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Update()
	{
		if (_isPlayerTime && Globals.Battle != null && HourglassIcon != null)
		{
			if (Globals.Battle.TimeRemainsTillTheEndOfState <= _flickTime)
			{
				HourglassIcon.Tint_ = new Color(1f, 1f, 1f, 0.5f * Mathf.Sin(Time.realtimeSinceStartup * 10f) + 0.5f);
			}
			else
			{
				HourglassIcon.Tint_ = new Color(1f, 1f, 1f, 0f);
			}
		}
	}
}
