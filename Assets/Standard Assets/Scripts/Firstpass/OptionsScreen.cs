using Yarx;

public class OptionsScreen : SpriteGui
{
	private CompositeDisposable _subscriptions;

	public string assetPrefix = "options_screen";

	public string defaultLoc = "en";

	private void Awake()
	{
		string language = UnityApi.GetLanguage();
		if (!(language != defaultLoc))
		{
		}
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
		foreach (SpriteButton value in _buttons.Values)
		{
			value.SetActive();
		}
		RegenerateAtlas();
		base.Release += ProcessButtons;
		Init();
	}

	public void Init()
	{
	}

	private void ProcessButtons(SpriteButton button)
	{
	}

	private void Update()
	{
		ProcessRayCast();
	}
}
