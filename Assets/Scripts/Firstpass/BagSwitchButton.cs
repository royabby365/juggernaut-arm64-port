using UnityEngine;
using Yarx;

public class BagSwitchButton : SpriteButton
{
	private const float TextSelectedAlpha = 1f;

	private const float TextUnselectedAlpha = 0.4f;

	private CompositeDisposable _subscriptions;

	public SpriteText Label;

	public Sprite Button;

	public int ColliderBorderX;

	public int ColliderBorderY;

	public float EnteredScaleX = 1.2f;

	public float EnteredScaleY = 1.2f;

	public ServerData.Slot.TypeE[] WhatTypes;

	public ServerData.PhrasesE Prase;

	public FontManager.ColorE TextEnteredTint;

	public FontManager.ColorE TextLeavedTint;

	private readonly Color LeavedTint = new Color(0.3f, 0.3f, 0.3f, 1f);

	private readonly Color SelectedTint = new Color(0.6f, 0.6f, 0.6f, 1f);

	private Vector3 _startScale;

	private void Awake()
	{
		_startScale = base.transform.localScale;
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

	public override void Entered()
	{
		base.Entered();
		if (!EnteredScaleX.Eqv(1f) || !EnteredScaleY.Eqv(1f))
		{
			base.transform.localScale = new Vector3(EnteredScaleX, EnteredScaleY, 1f);
		}
	}

	public override void Left()
	{
		base.Left();
		base.transform.localScale = _startScale;
	}

	public override void SetSelected()
	{
		base.SetSelected();
		DoSetTint(SelectedTint);
		Label.TextAlpha_ = 1f;
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
		DoSetTint(LeavedTint);
		Label.TextAlpha_ = 0.4f;
	}

	private void DoSetTint(Color tint)
	{
		Button.Tint_ = tint;
	}
}
