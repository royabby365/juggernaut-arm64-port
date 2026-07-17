using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Yarx;

public class TutorialContinueButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public SpriteText Label;

	public Sprite Button;

	public Sprite Highlight;

	public int ColliderBorderX;

	public int ColliderBorderY;

	public float EnteredScaleX = 1.2f;

	public float EnteredScaleY = 1.2f;

	public ServerData.PhrasesE Prase;

	public Color EnteredTint = new Color(0.8f, 0.8f, 0.8f, 1f);

	public Color LeavedTint = new Color(0.5f, 0.5f, 0.5f, 1f);

	public Color InactiveTint = new Color(0.3f, 0.3f, 0.3f, 1f);

	public FontManager.ColorE TextEnteredTint;

	public FontManager.ColorE TextLeavedTint;

	public FontManager.ColorE TextInactiveTint;

	private Vector3 _startScale;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action Click;

	public void OnClick()
	{
		this.Click?.Invoke();
	}

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

	public override void Entered()
	{
		base.Entered();
		DoSetTint(EnteredTint);
		if (Label != null && TextEnteredTint != FontManager.ColorE.None)
		{
			Label.NamedColorE_ = TextEnteredTint;
		}
		if (!EnteredScaleX.Eqv(1f) || !EnteredScaleY.Eqv(1f))
		{
			base.transform.localScale = new Vector3(EnteredScaleX, EnteredScaleY, 1f);
		}
	}

	public override void Left()
	{
		base.Left();
		DoSetTint(LeavedTint);
		if (Label != null && TextLeavedTint != FontManager.ColorE.None)
		{
			Label.NamedColorE_ = TextLeavedTint;
		}
		base.transform.localScale = _startScale;
	}

	public override void SetActive()
	{
		base.SetActive();
		DoSetTint(LeavedTint);
		if (Label != null && TextLeavedTint != FontManager.ColorE.None)
		{
			Label.NamedColorE_ = TextLeavedTint;
		}
		base.transform.localScale = _startScale;
	}

	public override void SetInactive()
	{
		base.SetInactive();
		DoSetTint(InactiveTint);
		if (Label != null && TextInactiveTint != FontManager.ColorE.None)
		{
			Label.NamedColorE_ = TextLeavedTint;
		}
		base.transform.localScale = _startScale;
	}

	public override void Released()
	{
		base.Released();
		OnClick();
	}

	private void DoSetTint(Color tint)
	{
		Button.Tint_ = tint;
		if (Highlight != null)
		{
			Highlight.Tint_ = tint;
		}
	}
}
