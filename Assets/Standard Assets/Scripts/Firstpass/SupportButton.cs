using UnityEngine;
using Yarx;

public class SupportButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public Vector3 PushScale = new Vector3(1f, 1.1f, 1f);

	public Color OverColor = new Color(0.7f, 0.7f, 0.7f, 1f);

	public Sprite Button;

	private void Awake()
	{
		Init();
		if (UnityApi.UseGameClub())
		{
			base.gameObject.SetActiveRecursively(state: false);
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		if (_subscriptions != null)
		{
			_subscriptions.Dispose();
		}
	}

	public override void Entered()
	{
		base.Entered();
		Button.Tint_ = OverColor;
		base.transform.localScale = PushScale;
	}

	public override void Left()
	{
		base.Left();
		Button.Tint_ = Color.gray;
		base.transform.localScale = Vector3.one;
	}
}
