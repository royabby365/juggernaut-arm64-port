using UnityEngine;
using Yarx;

public class BattleHudButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public Sprite Button;

	public Sprite InnerBg;

	public Color EnteredTint = new Color(0.8f, 0.8f, 0.8f, 1f);

	public string ActiveName;

	public string InactiveName;

	public string BgActiveName;

	public string BgInactiveName;

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

	public override void Entered()
	{
		base.Entered();
		if (!base.Selected)
		{
			DoSetTint(EnteredTint);
			base.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
		}
	}

	public override void Left()
	{
		base.Left();
		DoSetTint(Color.gray);
		base.transform.localScale = Vector3.one;
	}

	public override void SetActive()
	{
		base.SetActive();
		if (!base.Selected)
		{
			SetActiveLook();
		}
	}

	public override void SetInactive()
	{
		base.SetInactive();
		SetInactiveLook();
	}

	public override void SetSelected()
	{
		base.SetSelected();
		SetInactiveLook();
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
		if (base.Active)
		{
			SetActiveLook();
		}
		else
		{
			SetInactiveLook();
		}
	}

	private void SetActiveLook()
	{
		Button.SpriteName_ = ActiveName;
		InnerBg.SpriteName_ = BgActiveName;
	}

	private void SetInactiveLook()
	{
		Button.SpriteName_ = InactiveName;
		InnerBg.SpriteName_ = BgInactiveName;
	}

	private void DoSetTint(Color tint)
	{
		Button.Tint_ = tint;
	}
}
