using Yarx;

public class AltButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public ServerData.HintCodesE HintCode;

	private void Awake()
	{
		base.name = base.name + "_alt_" + SpriteGui.UniqueId;
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
		Init();
		SetActive();
	}
}
