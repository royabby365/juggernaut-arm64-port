using UnityEngine;

public class MapButton : SpriteButton
{
	private Vector3 _startScale;

	public Animation CustomAnimation;

	public float EnteredScaleX = 1.2f;

	public float EnteredScaleY = 1.2f;

	private void Start()
	{
		_startScale = base.transform.localScale;
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
}
