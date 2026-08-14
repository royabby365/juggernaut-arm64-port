using UnityEngine;
using Yarx;

public class ComboButtonMk1 : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public Sprite NextAttackIcon;

	public Color Light = new Color(0.8f, 0.8f, 0.8f, 1f);

	public Vector3 OverScale = new Vector3(1.1f, 1.1f, 1f);

	public override void Entered()
	{
		base.Entered();
		NextAttackIcon.Tint_ = Light;
		base.transform.localScale = OverScale;
	}

	public override void Left()
	{
		base.Left();
		NextAttackIcon.Tint_ = Color.gray;
		base.transform.localScale = Vector3.one;
	}

	private void Awake()
	{
		Init(12, 12);
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
}
