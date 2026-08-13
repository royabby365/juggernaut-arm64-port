using System;
using UnityEngine;
using Yarx;

public class LocationMoney : SpriteButton
{
	private enum PileSize
	{
		Small,
		Medium,
		Large
	}

	private CompositeDisposable _subscriptions;

	public int moneyCount;

	private PileSize _pileSize;

	private string _smallPile = "money_1";

	private string _medPile = "money_2";

	private string _largePile = "money_3";

	private Sprite _sprite;

	private readonly Color _animColor = new Color(0.7f, 0.7f, 0.7f, 1f);

	private AnimationCurve _animCurve;

	public float cellW => (_sprite != null) ? _sprite.Width : 0;

	public float cellH => (_sprite != null) ? _sprite.Height : 0;

	public void SetMoneyCount(int money)
	{
		moneyCount = money;
		if (money >= 100)
		{
			SetPileSize(PileSize.Medium);
		}
		else if (money >= 500)
		{
			SetPileSize(PileSize.Large);
		}
		else
		{
			SetPileSize(PileSize.Small);
		}
	}

	private void SetPileSize(PileSize pileSize)
	{
		switch (pileSize)
		{
		case PileSize.Small:
			_sprite.SpriteName_ = _smallPile;
			break;
		case PileSize.Medium:
			_sprite.SpriteName_ = _medPile;
			break;
		case PileSize.Large:
			_sprite.SpriteName_ = _largePile;
			break;
		default:
			throw new ArgumentOutOfRangeException("pileSize");
		}
	}

	private void Awake()
	{
		_animCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		_animCurve.postWrapMode = WrapMode.PingPong;
		_sprite = GetComponent<Sprite>();
		SetPileSize(PileSize.Large);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void Update()
	{
		if (GetComponent<Renderer>().isVisible)
		{
			_sprite.Tint_ = Color.Lerp(Color.gray, _animColor, _animCurve.Evaluate(Time.time));
		}
	}
}
