using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class AchievementScroll : MonoBehaviour
{
	private const float DefaultFriction = 4000f;

	private const float UnpenetrateTime = 0.3f;

	private CompositeDisposable _subscriptions;

	public Transform ScrollRoot;

	public BagSwitchButton[] Switches;

	public RatingEntry[] Ratings;

	public Transform RatingsRoot;

	public AchievementBigProgressBar ProgressBar;

	public GameObject AchievProgress;

	public GameObject Achiev;

	public Camera Viewport01;

	public BoxCollider Collider;

	public Sprite VerticalSliderBg;

	public Sprite VerticalSliderIndicator;

	public AnimationCurve DecelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public GameObject WaitMessage;

	public GameObject ButtonFullRating;

	public GameObject ButtonAddFriends;

	public GameObject FrameAddFriends;

	public SpriteText RatingShareBonusCount;

	private SpriteButton _fullRating;

	private SpriteButton _addFriends;

	private List<Tuple<AchievementEntry, bool, GameEvents.Event>> _entries = new List<Tuple<AchievementEntry, bool, GameEvents.Event>>();

	private int _sliderIndicatorMin;

	private int _sliderIndicatorMax;

	private int _minScrollLoc;

	private int _maxScrollLoc;

	private float _scrollSpeed;

	private Vector3 _scrollBegin;

	private bool _deceleration;

	private float _startTime;

	private float _stopTime;

	private Vector3 _startPos;

	private Vector3 _stopPos;

	private bool _waitTimeout;

	private bool ButtonPositionsFixed;

	private void Awake()
	{
		_fullRating = ButtonFullRating.GetComponentInChildren<SpriteButton>();
		_addFriends = ButtonAddFriends.GetComponentInChildren<SpriteButton>();
		_fullRating.ResetScale();
		_addFriends.ResetScale();
		_fullRating.SetActive();
		_addFriends.SetActive();
		ButtonFullRating.SetActiveRecursivelyMk1(setActive: false);
		ButtonAddFriends.SetActiveRecursivelyMk1(setActive: false);
		FrameAddFriends.SetActiveRecursivelyMk1(setActive: false);
		if (SingletonT<ServerData>.I.GameSettings != null)
		{
			RatingShareBonusCount.Text_ = "+" + SingletonT<ServerData>.I.GameSettings.RatingSharingMoneyBonus;
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnCloseGui));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnEnterGui));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgNewPersInited, InitAchievements));
		_subscriptions.Add(Messenger<GameEvents.Event, string>.AddListener(Globals.MsgGameEventProgressChanged, OnGameEventProgressChanged));
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += ProcessButtons;
			HudMk1.Instance.MoveBegin += GuiOnMoveBegin;
			HudMk1.Instance.MoveEnd += GuiOnMoveEnd;
			HudMk1.Instance.Move += GuiOnMove;
		}
		WaitMessage.SetActiveRecursivelyMk1(setActive: false);
	}

	private void OnDisable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release -= ProcessButtons;
			HudMk1.Instance.MoveBegin -= GuiOnMoveBegin;
			HudMk1.Instance.MoveEnd -= GuiOnMoveEnd;
			HudMk1.Instance.Move -= GuiOnMove;
		}
		_subscriptions.Dispose();
	}

	private void InitAchievements()
	{
		for (int i = 0; i < MainMenu.GameEvents.Events.Count; i++)
		{
			GameEvents.Event obj = MainMenu.GameEvents.Events[i];
			if (obj.Achievement != null)
			{
				GameObject gameObject = ((obj.MaxProgress != 1) ? ((GameObject)UnityEngine.Object.Instantiate(AchievProgress)) : ((GameObject)UnityEngine.Object.Instantiate(Achiev)));
				gameObject.transform.parent = ScrollRoot;
				gameObject.transform.localPosition = Vector3.zero;
				AchievementEntry component = gameObject.GetComponent<AchievementEntry>();
				_entries.Add(Tuple.Create(component, item2: true, obj));
				component.name = component.name + "_" + i;
			}
		}
		int num = 0;
		BagSwitchButton[] switches = Switches;
		foreach (BagSwitchButton bagSwitchButton in switches)
		{
			if (UnityApi.UseGameClub() && num == 3)
			{
				bagSwitchButton.gameObject.SetActiveRecursivelyMk1(setActive: false);
			}
			else
			{
				bagSwitchButton.SetActive();
			}
			num++;
		}
		ApplyFilter(0);
	}

	private void Update()
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.AchievementsScroll && _deceleration)
		{
			float time = Time.time;
			if (time <= _stopTime)
			{
				float num = _stopTime - _startTime;
				float num2 = time - _startTime;
				ScrollRoot.localPosition = Vector3.Lerp(_startPos, _stopPos, DecelerationCurve.Evaluate(num2 / num));
			}
			else
			{
				ScrollRoot.localPosition = _stopPos;
				_deceleration = false;
			}
			UpdateScrollIndicator();
		}
	}

	private void OnEnterGui(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.AchievementsScroll)
		{
			ApplyFilter(3);
		}
	}

	private void OnCloseGui(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (old == GuiRoot.GuiType.AchievementsScroll && old != @new)
		{
			Viewport01.enabled = false;
		}
	}

	private void GuiOnMove(Vector3 begin, Vector3 end)
	{
		HudMk1 instance = HudMk1.Instance;
		if (!(instance == null) && instance.CheckCollider(Collider, begin))
		{
			float y = (end - begin).y;
			y *= Camera2D.Scale;
			_scrollSpeed = y / Time.deltaTime;
			Vector3 localPosition = ScrollRoot.localPosition;
			ScrollRoot.localPosition = new Vector3(localPosition.x, localPosition.y + (float)y.RoundToInt(), localPosition.z);
			UpdateScrollIndicator();
		}
	}

	private void UpdateScrollIndicator()
	{
		float num = ScrollRoot.localPosition.y;
		if (num < (float)_minScrollLoc)
		{
			num = _minScrollLoc;
		}
		else if (num > (float)_maxScrollLoc)
		{
			num = _maxScrollLoc;
		}
		float num2 = (num - (float)_minScrollLoc) / (float)(_maxScrollLoc - _minScrollLoc);
		int num3 = _sliderIndicatorMin + (num2 * (float)(_sliderIndicatorMax - _sliderIndicatorMin)).RoundToInt();
		Vector3 localPosition = VerticalSliderIndicator.transform.localPosition;
		VerticalSliderIndicator.transform.localPosition = new Vector3(num3, localPosition.y, localPosition.z);
	}

	private void GuiOnMoveEnd(Vector3 vector3)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.AchievementsScroll)
		{
			return;
		}
		Vector3 localPosition = ScrollRoot.localPosition;
		_startTime = Time.time;
		_startPos = localPosition;
		_deceleration = true;
		if (localPosition.y < (float)_minScrollLoc)
		{
			_stopPos = new Vector3(localPosition.x, _minScrollLoc, localPosition.z);
			_stopTime = _startTime + 0.3f;
		}
		else if (localPosition.y > (float)_maxScrollLoc)
		{
			_stopPos = new Vector3(localPosition.x, _maxScrollLoc, localPosition.z);
			_stopTime = _startTime + 0.3f;
		}
		else if (!_scrollSpeed.Eqv(0f))
		{
			float a = Mathf.Abs(_scrollSpeed);
			float num = Mathf.Sign(_scrollSpeed);
			a = Mathf.Min(a, 4000f);
			float num2 = ((!(num < 0f)) ? Mathf.Abs((float)_maxScrollLoc - localPosition.y) : Mathf.Abs(localPosition.y - (float)_minScrollLoc));
			float num3 = a / 4000f;
			float num4 = Mathf.Round(a * num3 - 4000f * num3 * num3 / 2f);
			if (num4 > num2)
			{
				num3 /= num4 / num2;
				num4 = num2;
			}
			_stopTime = _startTime + num3;
			_stopPos = localPosition + new Vector3(0f, num * num4, 0f);
		}
		else
		{
			_deceleration = false;
		}
		Debug.Log("======== min:{0} max:{1} speed:{2} start:{3} stop:{4}".Fmt(_minScrollLoc, _maxScrollLoc, _scrollSpeed, _startPos, _stopPos));
		_scrollSpeed = 0f;
		UpdateScrollIndicator();
	}

	private void GuiOnMoveBegin(Vector3 begin)
	{
		HudMk1 instance = HudMk1.Instance;
		if (!(instance == null) && instance.CheckCollider(Collider, begin))
		{
			_deceleration = false;
		}
	}

	private int GetTopWorldMargin()
	{
		return 250;
	}

	private float GetTopMargin()
	{
		return (float)GetTopWorldMargin() / 768f * (float)Screen.height;
	}

	private void InitViewport()
	{
		int num = (int)(Camera2D.GetScale() * (float)Screen.height);
		_minScrollLoc = (num / 2 - 340 + 208 - 2) / 2 - 10;
		float num2 = (float)Screen.height - GetTopMargin().DivideBy2DScale();
		Viewport01.pixelRect = new Rect(0f, 0f, Screen.width, num2);
		Vector3 localPosition = VerticalSliderBg.transform.localPosition;
		localPosition.x = Camera2D.ScreenWidth - 52;
		VerticalSliderBg.transform.localPosition = localPosition;
		VerticalSliderBg.Width = (int)((num2.MultiplyBy2DScale() - 32f) * (float)Camera2D.ScreenHeight / (float)Screen.height);
		VerticalSliderBg.Refresh();
	}

	public void Init(Camera viewport01)
	{
		viewport01.enabled = false;
		Viewport01 = viewport01;
		InitViewport();
		InitAchievements();
		InitProgress();
	}

	private void ProcessButtons(SpriteButton spriteButton)
	{
		if (spriteButton.name == "button_full_rating")
		{
			Messenger.Invoke(Globals.MsgOpenRatings, Globals.MainMenu.Social.GetLeaderboardId());
		}
		else if (spriteButton.name == "button_add_friends")
		{
			Messenger.Invoke(Globals.MsgAddFriends);
		}
		else
		{
			if (spriteButton as BagSwitchButton == null)
			{
				return;
			}
			string text = spriteButton.name;
			for (int i = 0; i < Switches.Length; i++)
			{
				if (text == Switches[i].name)
				{
					ApplyFilter(i);
					break;
				}
			}
		}
	}

	private void ShowRatings()
	{
		if (Globals.DebugGetRatingLocal)
		{
			ShowWaitMessage(4f);
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(5f, delegate
			{
				_waitTimeout = false;
				SocialAspect.ScoresInfo[] array = new SocialAspect.ScoresInfo[2]
				{
					new SocialAspect.ScoresInfo(new UserProfileStub(), 100501L, isMe: false),
					new SocialAspect.ScoresInfo(new UserProfileStub(), 100502L, isMe: true)
				};
				CreateRatingEntries(array);
				FrameAddFriends.SetActiveRecursivelyMk1(array.Length == 0);
			});
			return;
		}
		if (UnityApi.UseGameClub())
		{
			CreateRatingEntries(new SocialAspect.ScoresInfo[0]);
			FrameAddFriends.SetActiveRecursivelyMk1(setActive: true);
			SpriteText componentInChildren = FrameAddFriends.GetComponentInChildren<SpriteText>();
			componentInChildren.gameObject.SetActive(false);
			if (!ButtonPositionsFixed)
			{
				Vector3 position = ButtonFullRating.transform.position;
				position.y += 300f;
				ButtonFullRating.transform.position = position;
				position = ButtonAddFriends.transform.position;
				position.y += 300f;
				ButtonAddFriends.transform.position = position;
				ButtonPositionsFixed = true;
			}
			return;
		}
		ShowWaitMessage(60f);
		Globals.MainMenu.Social.GetFriendsScores(delegate(SocialAspect.ScoresInfo[] info)
		{
			_waitTimeout = false;
			CreateRatingEntries(info);
			FrameAddFriends.SetActiveRecursivelyMk1(info.Length == 0);
		}, delegate
		{
			if (false)
			{
				_waitTimeout = false;
				CreateRatingEntries(new SocialAspect.ScoresInfo[0]);
				FrameAddFriends.SetActiveRecursivelyMk1(setActive: true);
			}
			else if (_waitTimeout)
			{
				Messenger<ServerData.PhrasesE, Action>.Invoke(Globals.MsgShowAlertWithCallback, ServerData.PhrasesE.MsgGetRatingErrorAndroid, delegate
				{
					ApplyFilter(0);
				});
				_waitTimeout = false;
			}
		});
	}

	private void ShowWaitMessage(float timeout)
	{
		SpriteGui.DontReleaseButtons = true;
		WaitMessage.SetActiveRecursivelyMk1(setActive: true);
		Viewport01.enabled = false;
		StartCoroutine(WaitTimeout(timeout));
	}

	private IEnumerator WaitTimeout(float timeout)
	{
		_waitTimeout = true;
		float time = 0f;
		while (time < timeout && _waitTimeout)
		{
			time += Time.deltaTime;
			yield return null;
		}
		HideWaitMessage();
		if (_waitTimeout)
		{
			Messenger<ServerData.PhrasesE, Action>.Invoke(Globals.MsgShowAlertWithCallback, ServerData.PhrasesE.MsgRatingTimeoutAndroid, delegate
			{
				ApplyFilter(0);
			});
			_waitTimeout = false;
		}
	}

	private void DestroyRatingEntries()
	{
		RatingsRoot.GoToHell();
		ButtonFullRating.SetActiveRecursivelyMk1(setActive: false);
		ButtonAddFriends.SetActiveRecursivelyMk1(setActive: false);
		FrameAddFriends.SetActiveRecursivelyMk1(setActive: false);
	}

	private void HideWaitMessage()
	{
		SpriteGui.DontReleaseButtons = false;
		WaitMessage.SetActiveRecursivelyMk1(setActive: false);
	}

	private void CreateRatingEntries(SocialAspect.ScoresInfo[] info)
	{
		if (info.Length > 0)
		{
			RatingsRoot.localPosition = Vector3.zero;
		}
		else
		{
			RatingsRoot.GoToHell();
		}
		ButtonFullRating.SetActiveRecursivelyMk1(setActive: true);
		ButtonAddFriends.SetActiveRecursivelyMk1(setActive: true);
		for (int i = 0; i < Ratings.Length; i++)
		{
			RatingEntry ratingEntry = Ratings[i];
			if (i < info.Length)
			{
				SocialAspect.ScoresInfo info2 = info[i];
				ratingEntry.SetScoresInfo(i + 1, info2);
				ratingEntry.transform.localPosition = new Vector3(0f, -78 * i, 0f);
			}
			else
			{
				ratingEntry.transform.GoToHell();
			}
		}
	}

	private void ApplyFilter(int i)
	{
		BagSwitchButton[] switches = Switches;
		foreach (BagSwitchButton bagSwitchButton in switches)
		{
			bagSwitchButton.SetUnselected();
			bagSwitchButton.SetActive();
		}
		if (UnityApi.UseGameClub() && i == 3)
		{
			i = 0;
		}
		Switches[i].SetSelected();
		Switches[i].SetInactive();
		Viewport01.enabled = i != 3;
		if (i == 3)
		{
			VerticalSliderBg.ShowOrHideMethod(show: false);
			VerticalSliderIndicator.ShowOrHideMethod(show: false);
			ShowRatings();
			return;
		}
		DestroyRatingEntries();
		for (int k = 0; k < _entries.Count; k++)
		{
			Tuple<AchievementEntry, bool, GameEvents.Event> tuple = _entries[k];
			bool item = true;
			switch (i)
			{
			case 1:
				item = tuple.Item3.Progress >= tuple.Item3.MaxProgress;
				break;
			case 2:
				item = tuple.Item3.Progress < tuple.Item3.MaxProgress;
				break;
			case 3:
				item = tuple.Item3.MaxProgress > 1;
				break;
			}
			_entries[k] = Tuple.Create(tuple.Item1, item, tuple.Item3);
		}
		RearrangeEntries();
	}

	private void RearrangeEntries()
	{
		_deceleration = false;
		int num = 0;
		int num2 = 0;
		Vector3 localPosition = ScrollRoot.localPosition;
		ScrollRoot.localPosition = new Vector3(localPosition.x, _minScrollLoc, localPosition.z);
		int num3 = 0;
		foreach (Tuple<AchievementEntry, bool, GameEvents.Event> entry in _entries)
		{
			AchievementEntry item = entry.Item1;
			item.SetEvent(entry.Item3);
			num += entry.Item3.Achievement.Points;
			num2 += ((entry.Item3.Progress >= entry.Item3.MaxProgress) ? entry.Item3.Achievement.Points : 0);
			if (entry.Item2)
			{
				int num4 = ((!(item.ProgressBar == null)) ? 160 : 110);
				item.transform.localPosition = new Vector3(0f, num3, 0f);
				num3 -= num4;
			}
			else
			{
				item.transform.GoToHell();
			}
		}
		ProgressBar.SetIndicator(num2, num);
		float num5 = Viewport01.pixelRect.height.MultiplyBy2DScale();
		_maxScrollLoc = -num3 + _minScrollLoc - num5.RoundToInt() + 8;
		if (_maxScrollLoc < _minScrollLoc)
		{
			_maxScrollLoc = _minScrollLoc;
			VerticalSliderBg.ShowOrHideMethod(show: false);
			VerticalSliderIndicator.ShowOrHideMethod(show: false);
			return;
		}
		VerticalSliderBg.ShowOrHideMethod(show: true);
		VerticalSliderIndicator.ShowOrHideMethod(show: true);
		int num6 = ((float)VerticalSliderBg.Width * (num5 / (float)(-num3))).RoundToInt();
		if (num6 % 2 != 0)
		{
			num6++;
		}
		num6 = Mathf.Max(18, num6);
		VerticalSliderIndicator.Width = num6;
		VerticalSliderIndicator.Refresh();
		_sliderIndicatorMin = num6 / 2 + 2;
		_sliderIndicatorMax = VerticalSliderBg.Width - num6 / 2 - 2;
		Vector3 localPosition2 = VerticalSliderIndicator.transform.localPosition;
		VerticalSliderIndicator.transform.localPosition = new Vector3(_sliderIndicatorMin, localPosition2.y, localPosition2.z);
	}

	private void OnGameEventProgressChanged(GameEvents.Event @event, string reason)
	{
		InitProgress();
	}

	private void InitProgress()
	{
		if (ProgressBar == null || _entries == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (Tuple<AchievementEntry, bool, GameEvents.Event> entry in _entries)
		{
			if (entry.Item3 != null && entry.Item3.Achievement != null)
			{
				num += entry.Item3.Achievement.Points;
				num2 += ((entry.Item3.Progress >= entry.Item3.MaxProgress) ? entry.Item3.Achievement.Points : 0);
			}
		}
		ProgressBar.SetIndicator(num2, num);
	}
}
