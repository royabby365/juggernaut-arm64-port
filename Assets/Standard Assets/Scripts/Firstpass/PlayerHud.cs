using System.IO;
using UnityEngine;
using Yarx;

public class PlayerHud : MonoBehaviour
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

	public SpriteText Level;

	public Sprite PlayerIcon;

	public AnimationCurve DeltaCurve;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int, int>.AddListener(Globals.MsgPlayerHealthChanged, OnPlayerHealthChanged));
		_subscriptions.Add(Messenger<string>.AddListener(Globals.MsgGuiBattle_PlayerAvatar, OnPlayerAvatarChanged));
		_subscriptions.Add(Messenger<int, int, string>.AddListener(Globals.MsgPlayerLevelChanged, OnPlayerChangeLevel));
	}

	private void OnPlayerChangeLevel(int prev, int current, string reason)
	{
		SetPlayerLevel(current);
	}

	private void OnPlayerAvatarChanged(string unusedParam)
	{
		ServerData.Item item = SingletonT<ServerData>.I.FindInBag((ServerData.Item item2) => item2.PutOn && item2.Slot.SlotId == ServerData.Slot.TypeE.Helm);
		ServerData.PersData playerServerPersData = SingletonT<ServerData>.I.PlayerServerPersData;
		string text = ((!(Globals.Player != null) || !Globals.Player.IsFemale) ? "m" : "f");
		int num = playerServerPersData?.Class ?? 1;
		string text2 = DecodeItemColor(item);
		string path = "{0}_set_{1}_{2}".Fmt(text2, num, text);
		path = Path.GetFileNameWithoutExtension(path);
		SetPlayerIcon(path);
	}

	internal static string DecodeItemColor(ServerData.Item item)
	{
		if (item == null)
		{
			return "default";
		}
		if (item.Color.Contains("gray"))
		{
			return "gray";
		}
		if (item.Color.Contains("green"))
		{
			return "green";
		}
		if (item.Color.Contains("blue"))
		{
			return "blue";
		}
		if (item.Color.Contains("purple"))
		{
			return "violet";
		}
		return "gray";
	}

	private void OnPlayerHealthChanged(int current, int max)
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
		SetHealth(current, max);
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

	private void SetHealth(int current, int max)
	{
		HpDigits.Text_ = "{0}/{1}".Fmt(current, max);
		float num = 1f - Mathf.Clamp01((float)current / (float)max);
		int num2 = -(num * (float)HpBarWidth).RoundToInt();
		Vector3 localPosition = HpBar.transform.localPosition;
		HpBar.transform.localPosition = new Vector3(num2, localPosition.y, localPosition.z);
		HpBar.ClipHorizontalLocal(-num2, 1024f);
	}

	private void SetDelta(float current, float max)
	{
		float num = 1f - Mathf.Clamp01(current / max);
		float num2 = 0f - num * (float)HpBarWidth;
		Vector3 localPosition = DeltaBar.transform.localPosition;
		DeltaBar.transform.localPosition = new Vector3(num2, localPosition.y, localPosition.z);
		DeltaBar.ClipHorizontalLocal(0f - num2, 1024f);
	}

	private void SetPlayerIcon(string iconName)
	{
		PlayerIcon.SpriteName_ = iconName;
	}

	private void SetPlayerLevel(int level)
	{
		Level.Text_ = level.ToString();
	}
}
