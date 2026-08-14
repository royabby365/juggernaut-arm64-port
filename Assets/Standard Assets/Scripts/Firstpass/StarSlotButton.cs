using Yarx;

public class StarSlotButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public Sprite Bg;

	public Sprite Star;

	public SpriteText Count;

	public PuppetSlot MySlot { private get; set; }

	private void Awake()
	{
		base.name = base.name + "#" + SpriteGui.UniqueId;
		Init(0, 30);
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

	private void Update()
	{
	}

	public override void SetActive()
	{
		base.SetActive();
		ShowHide(show: true);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		ShowHide(show: false);
	}

	private void ShowHide(bool show)
	{
		Bg.ShowOrHide(show);
		Star.ShowOrHide(show);
		Count.ShowOrHide(show);
	}

	public void SetCount(int currentStars, int maxStars)
	{
		Count.Text_ = "{0}/{1}".Fmt(currentStars, maxStars);
	}

	public override void Released()
	{
		base.Released();
		MySlot.UpgradeStars();
	}
}
