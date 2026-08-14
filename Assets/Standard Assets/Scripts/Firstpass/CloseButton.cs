public class CloseButton : SpriteButton
{
	public override void SetActive()
	{
		base.SetActive();
	}

	public override void SetInactive()
	{
		base.SetInactive();
	}

	private void Awake()
	{
		Init(20, 20);
	}

	public override void Clicked()
	{
		base.Clicked();
	}
}
