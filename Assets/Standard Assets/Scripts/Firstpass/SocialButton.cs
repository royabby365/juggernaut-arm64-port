using UnityEngine;
using Yarx;

public class SocialButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public Vector3 OverScale = new Vector3(1.1f, 1.1f, 1f);

	public Color OverTint = new Color32(200, 200, 200, byte.MaxValue);

	public Sprite MySprite;

	private void Awake()
	{
		Init();
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

	private void Update()
	{
	}

	public override void SetActive()
	{
		base.SetActive();
		base.transform.localScale = Vector3.one;
	}

	public override void SetInactive()
	{
		base.SetInactive();
		base.transform.localScale = Vector3.zero;
	}

	public override void Left()
	{
		base.Left();
		base.transform.localScale = Vector3.one;
		MySprite.Tint_ = Color.gray;
	}

	public override void Entered()
	{
		base.Entered();
		base.transform.localScale = OverScale;
		MySprite.Tint_ = OverTint;
	}
}
