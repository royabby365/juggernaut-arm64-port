using UnityEngine;
using Yarx;

public class MainMapProgressBar : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public Transform empty;

	public Transform full;

	public int StartScale = 150;

	public Transform Thumb;

	public Transform SoveringIcon;

	public SpriteText ProgressCount;

	public void SetIndicator(int current, int max)
	{
		ProgressCount.Text_ = "{0}%".Fmt(current);
		SetStripe((float)current / (float)max);
	}

	private void Awake()
	{
		empty.localScale = new Vector3(StartScale, 1f, 1f);
		SetStripe(1f);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnGuiChanged));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
	}

	private void OnGuiChanged(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.MainMap)
		{
			SetIndicator(SingletonT<ServerData>.I.GameProgress(), 100);
		}
	}

	private void SetStripe(float progress)
	{
		progress = Mathf.Clamp01(progress);
		if (progress < 0.02f)
		{
			progress = 0f;
		}
		float x = empty.localScale.x;
		empty.renderer.material.mainTextureScale = new Vector2(x, 1f);
		float num = x * progress / 1f;
		full.localScale = new Vector3(num, 1f, 1f);
		full.renderer.material.mainTextureScale = new Vector2(num, 1f);
		Thumb.localPosition = new Vector3(full.localPosition.x + full.parent.localPosition.x + num * 4f, Thumb.localPosition.y, Thumb.localPosition.z);
		SoveringIcon.localPosition = new Vector3(empty.localPosition.x + empty.parent.localPosition.x + x * 4f, SoveringIcon.localPosition.y, SoveringIcon.localPosition.z);
	}
}
