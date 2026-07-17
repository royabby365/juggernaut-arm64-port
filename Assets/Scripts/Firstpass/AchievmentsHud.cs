using UnityEngine;

public class AchievmentsHud : MonoBehaviour
{
	private AchievmentView _currentAchievmentView;

	private AchievmentView[] _nextAchievmentViews;

	private SocialButton _twitterButton;

	private SocialButton _facebookButton;

	public Transform CurrentAchievmentRoot;

	public Transform[] NextAchievmentRoots;

	public GameObject CurrentAchievmentPrefab;

	public GameObject NextAchievmentPrefab;

	public SpriteText SharingMoneyBonus;

	internal ServerData.Achievement Achievement;

	private void Awake()
	{
		GameObject gameObject = (GameObject)Object.Instantiate(CurrentAchievmentPrefab);
		_currentAchievmentView = gameObject.GetComponentInChildren<AchievmentView>();
		gameObject.transform.parent = CurrentAchievmentRoot;
		gameObject.transform.localPosition = default(Vector3);
		if (SingletonT<ServerData>.I.GameSettings != null)
		{
			SharingMoneyBonus.Text_ = "+" + SingletonT<ServerData>.I.GameSettings.AchievmentSharingMoneyBonus;
		}
		_nextAchievmentViews = new AchievmentView[NextAchievmentRoots.Length];
		for (int i = 0; i < NextAchievmentRoots.Length; i++)
		{
			GameObject gameObject2 = (GameObject)Object.Instantiate(NextAchievmentPrefab);
			gameObject2.transform.parent = NextAchievmentRoots[i];
			gameObject2.transform.localPosition = default(Vector3);
			_nextAchievmentViews[i] = gameObject2.GetComponentInChildren<AchievmentView>();
		}
	}

	internal void Init(GameEvents.Event achievment)
	{
		if (_currentAchievmentView == null)
		{
			return;
		}
		SingletonT<SoundManager>.I.PlayGlobalSound("Jug_achievment");
		Achievement = achievment.Achievement;
		_currentAchievmentView.SetAchievment(achievment);
		MainMenu.GameEvents.Events.Sort(SortByProgress);
		Transform[] nextAchievmentRoots = NextAchievmentRoots;
		foreach (Transform transform in nextAchievmentRoots)
		{
			transform.gameObject.SetActiveRecursivelyMk1(setActive: false);
		}
		int num = 0;
		int num2 = 0;
		while (num < _nextAchievmentViews.Length)
		{
			GameEvents.Event obj = MainMenu.GameEvents.Events[num2];
			if (obj.MaxProgress > 1 && obj.Progress < obj.MaxProgress && obj != achievment)
			{
				_nextAchievmentViews[num].SetAchievment(obj);
				NextAchievmentRoots[num].gameObject.SetActiveRecursivelyMk1(setActive: true);
				num++;
			}
			num2++;
		}
		if (_twitterButton == null)
		{
			Transform transform2 = base.transform.FindChildByName("button_twitter");
			if (transform2 != null)
			{
				_twitterButton = transform2.GetComponentInChildren<SocialButton>();
			}
		}
		if (_facebookButton == null)
		{
			Transform transform3 = base.transform.FindChildByName("button_facebook");
			if (transform3 != null)
			{
				_facebookButton = transform3.GetComponentInChildren<SocialButton>();
			}
		}
		Utils.Log("TWITTER", _twitterButton, UnityApi.IsTwitterEnabled());
		if (_twitterButton != null)
		{
			if (!UnityApi.IsTwitterEnabled())
			{
				Utils.Log("TWITTER.SetInactive");
				_twitterButton.SetInactive();
			}
			else
			{
				Utils.Log("TWITTER.SetActive");
				_twitterButton.SetActive();
			}
		}
		if (_facebookButton != null)
		{
			_facebookButton.SetActive();
		}
	}

	private void OnEnable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += ProcessButtons;
			HudMk1.Instance.DragEndWithButton += ProcessButtons;
		}
	}

	private void OnDisable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release -= ProcessButtons;
			HudMk1.Instance.DragEndWithButton -= ProcessButtons;
		}
	}

	private int SortByProgress(GameEvents.Event e1, GameEvents.Event e2)
	{
		float num = (float)e1.Progress / (float)e1.MaxProgress;
		float num2 = (float)e2.Progress / (float)e2.MaxProgress;
		float num3 = num2 - num;
		if (num3 > 0f)
		{
			return 1;
		}
		if (num3 == 0f)
		{
			return 0;
		}
		return -1;
	}

	private void ProcessButtons(SpriteButton obj)
	{
		switch (obj.name)
		{
		case "button_achievment_continue":
			Messenger.Invoke(Globals.MsgGuiExitAchievments);
			break;
		}
	}
}
