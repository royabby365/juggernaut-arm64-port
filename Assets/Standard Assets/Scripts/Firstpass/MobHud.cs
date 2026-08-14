using System.IO;
using UnityEngine;
using Yarx;

public class MobHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private float _target;

	private float _current;

	private float _max;

	private float _animationTime;

	private float _animationDuration;

	public SpriteText HpDigits;

	public Sprite HpBar;

	public Sprite DeltaBar;

	public int HpBarWidth = 275;

	public Sprite MobIcon;

	public SpriteText Level;

	public AnimationCurve DeltaCurve;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int, int>.AddListener(Globals.MsgEnemyHealthChanged, SetMobHealth));
		_subscriptions.Add(Messenger<string>.AddListener(Globals.MsgGuiBattle_EnemyAvatar, SetMobIcon));
		_subscriptions.Add(Messenger<int>.AddListener(Globals.MsgGuiBattle_EnemyLevel, SetMobLevel));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		_animationDuration = ((DeltaCurve.length <= 0) ? 0f : DeltaCurve.keys[DeltaCurve.length - 1].time);
		_animationTime = _animationDuration;
	}

	private void Update()
	{
		if (_animationTime < _animationDuration && _max > 0f && _current > _target)
		{
			_animationTime += Time.deltaTime;
			SetDelta(Mathf.Lerp(_current, _target, DeltaCurve.Evaluate(_animationTime)), _max);
		}
	}

	private void SetMobHealth(int current, int max)
	{
		_current = Mathf.Lerp(_current, _target, DeltaCurve.Evaluate(_animationTime));
		_target = current;
		_animationTime = 0f;
		_max = max;
		if (_current < _target)
		{
			_current = _target;
			_animationTime = _animationDuration;
		}
		HpDigits.Text_ = "{0}/{1}".Fmt(current, max);
		float num = 1f - Mathf.Clamp01((float)current / (float)max);
		int num2 = (num * (float)HpBarWidth).RoundToInt();
		Vector3 localPosition = HpBar.transform.localPosition;
		HpBar.transform.localPosition = new Vector3(num2, localPosition.y, localPosition.z);
		HpBar.ClipHorizontalLocal(-1024f, -num2);
	}

	private void SetDelta(float current, float max)
	{
		float num = 1f - Mathf.Clamp01(current / max);
		float num2 = num * (float)HpBarWidth;
		Vector3 localPosition = DeltaBar.transform.localPosition;
		DeltaBar.transform.localPosition = new Vector3(num2, localPosition.y, localPosition.z);
		DeltaBar.ClipHorizontalLocal(-1024f, 0f - num2);
	}

	private void SetMobIcon(string iconName)
	{
		iconName = Path.GetFileNameWithoutExtension(iconName);
		MobIcon.SpriteName_ = iconName;
	}

	private void SetMobLevel(int level)
	{
		Level.Text_ = level.ToString();
	}
}
