using UnityEngine;

public class TextMover : MonoBehaviour
{
	public SpriteText SpriteText;

	private void MoveMe(Rect newTextBounds)
	{
		float num = -30f;
		base.transform.localPosition = new Vector3((0f - newTextBounds.width) / 2f + num, base.transform.localPosition.y, base.transform.localPosition.z);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
