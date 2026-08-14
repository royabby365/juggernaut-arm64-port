using UnityEngine;
using Yarx;

public class GestureIcons : MonoBehaviour
{
	private const int iIce = 0;

	private const int iDark = 1;

	private const int iFire = 2;

	private const int iLightning = 3;

	private CompositeDisposable _subscriptions;

	public Sprite[] Icons;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<bool, bool, bool, bool>.AddListener(Globals.MsgGuiBattle_MagicCasts, OnMagicCasts));
	}

	private void OnMagicCasts(bool dark, bool fire, bool ice, bool lightning)
	{
		LayoutIcons(dark, fire, ice, lightning);
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void LayoutIcons(bool dark, bool fire, bool ice, bool lightning)
	{
		int num = Icons.Length;
		if (!ice)
		{
			num--;
		}
		if (!dark)
		{
			num--;
		}
		if (!fire)
		{
			num--;
		}
		if (!lightning)
		{
			num--;
		}
		Icons[0].gameObject.SetActiveRecursivelyMk1(ice);
		Icons[1].gameObject.SetActiveRecursivelyMk1(dark);
		Icons[2].gameObject.SetActiveRecursivelyMk1(fire);
		Icons[3].gameObject.SetActiveRecursivelyMk1(lightning);
		int num2 = -(num * 128) / 2;
		int num3 = 0;
		Sprite[] icons = Icons;
		foreach (Sprite sprite in icons)
		{
			if (sprite.gameObject.active)
			{
				int num4 = 64 + num2 + 128 * num3++;
				sprite.transform.localPosition = new Vector3(num4, 0f, 0f);
			}
		}
	}
}
