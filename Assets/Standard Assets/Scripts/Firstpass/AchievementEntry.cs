using UnityEngine;
using Yarx;

public class AchievementEntry : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Sprite Icon;

	public SpriteText LevelDigit;

	public Sprite Shild;

	public SpriteText Title;

	public SpriteText Description;

	public AchievmentProgressBar ProgressBar;

	private Color DarkTint = new Color(0.2f, 0.2f, 0.2f, 0.8f);

	private MeshFilter _filter;

	internal void SetEvent(GameEvents.Event ge)
	{
		bool flag = ge.Progress >= ge.MaxProgress;
		Icon.SpriteName_ = ((!flag) ? (ge.Achievement.Image + "_sepia") : ge.Achievement.Image);
		Shild.Tint_ = ((!flag) ? DarkTint : Color.gray);
		if (_filter != null)
		{
			_filter.mesh.SetTint((!flag) ? DarkTint : Color.white);
		}
		Title.Text_ = ge.Achievement.Title;
		Description.Text_ = ge.Achievement.Info;
		if (ProgressBar != null)
		{
			ProgressBar.SetProgress(ge.Progress, ge.MaxProgress);
		}
		LevelDigit.Text_ = ge.Achievement.Points.ToString();
	}

	private void Awake()
	{
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
		_filter = LevelDigit.GetComponent<MeshFilter>();
	}
}
