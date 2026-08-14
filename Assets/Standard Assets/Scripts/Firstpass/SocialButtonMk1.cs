using UnityEngine;
using Yarx;

public class SocialButtonMk1 : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public Vector3 OverScale = new Vector3(1.1f, 1.1f, 1f);

	public Color OverTint = new Color32(200, 200, 200, byte.MaxValue);

	public Sprite MySprite;

	public ServerData.PhrasesE SocialMessage;

	public SocialPoster.MessageType MessageType;

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
	}

	public override void SetInactive()
	{
		base.SetInactive();
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
