using UnityEngine;

public class BagPhotoButton : SpriteButton
{
	public Color EnteredTint = new Color(0.8f, 0.8f, 0.8f, 1f);

	public Color DefaultTint = new Color(0.5f, 0.5f, 0.5f, 1f);

	private Sprite buttonSprite;

	private void Awake()
	{
		buttonSprite = base.transform.GetComponent<Sprite>();
		Init();
	}

	public new void Init()
	{
		base.Init();
		if (!UnityApi.ShowScreenshotButton())
		{
			base.gameObject.SetActiveRecursively(state: false);
		}
	}

	public override void Entered()
	{
		base.Entered();
		DoTint(EnteredTint);
	}

	public override void Left()
	{
		base.Left();
		DoTint(DefaultTint);
	}

	private void DoTint(Color tintColor)
	{
		if (buttonSprite != null)
		{
			buttonSprite.Tint = tintColor;
		}
	}
}
