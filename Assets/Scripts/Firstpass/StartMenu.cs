using UnityEngine;
using Yarx;

public class StartMenu : SpriteGui
{
	private CompositeDisposable _subscriptions;

	public Transform muzhik;

	public Transform angelL;

	public Transform angelR;

	public SpriteButton megaBonusButton;

	public Transform jugLogo;

	public Transform buttonNewGame;

	public Transform buttonContinueGame;

	public Transform buttonRating;

	public Transform buttonSettings;

	public Transform buttonDevelopeds;

	public string assetPrefix;

	public string defaultLoc = "en";

	public Transform Alert;

	private bool _megaBonus;

	public void HideAlert()
	{
		Alert.GoToHell();
	}

	public void ShowAlert()
	{
		Alert.localPosition = new Vector3(0f, 0f, 0f);
	}

	private void Awake()
	{
		string language = UnityApi.GetLanguage();
		if (language != defaultLoc)
		{
			jugLogo.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			buttonNewGame.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			buttonContinueGame.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			buttonRating.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			buttonSettings.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			buttonDevelopeds.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
		}
		Alert.GoToHell();
	}

	private void OnEnable()
	{
		SpriteGui.DontReleaseButtons = false;
		_subscriptions = new CompositeDisposable();
		_megaBonus = Random.Range(1, 3) == 1;
		muzhik.gameObject.SetActive(!_megaBonus);
		angelL.gameObject.SetActive(_megaBonus);
		angelR.gameObject.SetActive(_megaBonus);
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
		_megaBonus = false;
	}

	private void Start()
	{
		Application.targetFrameRate = Globals.DefaultFPS;
		foreach (SpriteButton value in _buttons.Values)
		{
			value.SetActive();
		}
		if (!_megaBonus)
		{
			megaBonusButton.SetInactive();
		}
		base.transform.FindChildByName("_3_ratings").GetComponent<SimpleOverButton>().SetInactive();
		base.transform.FindChildByName("_4_options").GetComponent<SimpleOverButton>().SetInactive();
		base.transform.FindChildByName("_5_developers").GetComponent<SimpleOverButton>().SetInactive();
		RegenerateAtlas();
	}

	private void Update()
	{
		Transform transform = base.transform.FindChildByName("_2_continue");
		if (transform != null)
		{
			SimpleOverButton component = transform.GetComponent<SimpleOverButton>();
			if (component != null)
			{
				if (Globals.LastLoadGameSuccessed)
				{
					component.SetActive();
				}
				else
				{
					component.SetInactive();
				}
			}
		}
		ProcessRayCast();
	}
}
