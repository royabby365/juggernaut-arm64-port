using UnityEngine;
using Yarx;

public class BagItemSlot : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Sprite Frame;

	public Sprite Inner;

	private Color _inactiveColor;

	public void ClipWorld(float leftX, float rightX)
	{
		Frame.ClipHorizontalWorld(leftX, rightX);
		Inner.ClipHorizontalWorld(leftX, rightX);
	}

	private void Awake()
	{
		_inactiveColor = Frame.Tint;
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

	public void TurnBgOff(bool off)
	{
		Inner.ShowOrHide(!off);
	}

	public void SetItemColor(ServerData.Item item)
	{
		if (item == null)
		{
			Frame.Tint_ = _inactiveColor;
			return;
		}
		FontManager.ColorE color = item.DecodeColor();
		Color bottomColor = FontManager.Instance.GetNamedColor(color).BottomColor;
		Frame.Tint_ = bottomColor;
	}
}
