using UnityEngine;
using Yarx;

public class NewGuiButtonMk2 : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public SpriteText Label;

	public Sprite Button;

	public Sprite Highlight;

	public int ColliderBorderX;

	public int ColliderBorderY;

	public float EnteredScaleX = 1.2f;

	public float EnteredScaleY = 1.2f;

	private Color EnteredTint = Color.white;

	private Color LeavedTint = new Color(0.91f, 0.91f, 0.91f, 1f);

	private Color InactiveTint = new Color(0.3f, 0.3f, 0.3f, 0.7f);

	private Color EnteredTintHl = new Color(0.75f, 0.75f, 0.75f, 1f);

	private Color LeavedTintHl = new Color(0.58f, 0.58f, 0.58f, 1f);

	private FontManager.ColorE TextLeavedTint = FontManager.ColorE.ButtonGold;

	private Vector3 _startScale;

	private void Awake()
	{
		Init(ColliderBorderX, ColliderBorderY);
		_startScale = base.transform.localScale;
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
		Button.Tint_ = LeavedTint;
		if (Highlight != null)
		{
			Highlight.Tint_ = LeavedTintHl;
		}
		if (Label != null && TextLeavedTint != FontManager.ColorE.None)
		{
			Label.SetColor(TextLeavedTint);
		}
	}

	public override void Entered()
	{
		base.Entered();
		Button.Tint_ = EnteredTint;
		if (Highlight != null)
		{
			Highlight.Tint_ = EnteredTintHl;
		}
		if (!EnteredScaleX.Eqv(1f) || !EnteredScaleY.Eqv(1f))
		{
			base.transform.localScale = new Vector3(EnteredScaleX, EnteredScaleY, 1f);
		}
	}

	public override void Left()
	{
		base.Left();
		Button.Tint_ = LeavedTint;
		if (Highlight != null)
		{
			Highlight.Tint_ = LeavedTintHl;
		}
		base.transform.localScale = _startScale;
	}

	public override void SetActive()
	{
		base.SetActive();
		Button.Tint_ = LeavedTint;
		if (Highlight != null)
		{
			Highlight.Tint_ = LeavedTintHl;
		}
		if (Label != null)
		{
			Label.TextAlpha_ = 1f;
		}
		base.transform.localScale = _startScale;
	}

	public override void SetInactive()
	{
		base.SetInactive();
		Button.Tint_ = InactiveTint;
		if (Highlight != null)
		{
			Highlight.Tint_ = InactiveTint;
		}
		if (Label != null)
		{
			Label.TextAlpha_ = 0.3f;
		}
		base.transform.localScale = _startScale;
	}
}
