using UnityEngine;
using Yarx;

[RequireComponent(typeof(Sprite))]
public class LocationChest : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public LocationLogic.ChestOnLocation ChestonLocation;

	public bool Animate = true;

	private Sprite _sprite;

	private readonly Color _animColor = new Color(0.7f, 0.7f, 0.7f, 1f);

	private AnimationCurve _animCurve;

	public float cellW => (_sprite != null) ? _sprite.Width : 0;

	public float cellH => (_sprite != null) ? _sprite.Height : 0;

	private void Awake()
	{
		_animCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		_animCurve.postWrapMode = WrapMode.PingPong;
		_sprite = GetComponent<Sprite>();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Update()
	{
		if (Animate && base.renderer.isVisible)
		{
			_sprite.Tint_ = Color.Lerp(Color.gray, _animColor, _animCurve.Evaluate(Time.time));
		}
	}
}
