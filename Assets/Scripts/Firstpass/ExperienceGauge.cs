using System.Collections;
using UnityEngine;
using Yarx;

public class ExperienceGauge : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText Level;

	public SpriteText Experience;

	public Sprite Gauge;

	private string _expFormatString;

	private int _gaugeWidth;

	public void SetLevel(int prevLevel, int level, string reason)
	{
		Level.Text_ = level.ToString();
	}

	public void SetExperiencePercent(int percent)
	{
		SetExperience(percent, 100);
	}

	private void SetExperience(int exp, int expMax)
	{
		expMax = ((expMax == 0) ? 1 : expMax);
		float num = (float)exp / (float)expMax;
		int num2 = (num * 100f).RoundToInt();
		Experience.Text_ = _expFormatString.Fmt(num2);
		Gauge.ClipHorizontalLocal(-2000f, (num * (float)Gauge.Width).RoundToInt());
	}

	private void Awake()
	{
		_expFormatString = SingletonT<ServerData>.I.GetPhrase(Experience.Phrase_);
		Experience.Phrase_ = ServerData.PhrasesE.Custom;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int, int, string>.AddListener(Globals.MsgPlayerLevelChanged, SetLevel));
		_subscriptions.Add(Messenger<int>.AddListener(Globals.MsgPlayerExpChanged, SetExperiencePercent));
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
