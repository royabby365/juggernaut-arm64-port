using System.Collections;
using UnityEngine;
using Yarx;

public class IntroScreen : SpriteGui
{
	private CompositeDisposable _subscriptions;

	public Transform CharFrame;

	public SpriteText Header;

	public SpriteText Title;

	public SpriteText ScenarioText;

	public Transform DaysPass4;

	public Transform DaysPass30;

	public Transform NextButton;

	public SpriteText ButtonLabel;

	public string askola2D = "intro/fragments/intro_askola";

	public string voevoda2D = "intro/fragments/intro_npc_voevoda";

	public Vector3 ActiveButtonPosition = new Vector3(460f, -680f, -50f);

	public Vector3 InactiveButtonPosition = new Vector3(460f, -1000f, -50f);

	private float _gsFadeInOut;

	private float _gsReadingTime;

	private float _gsDayPassTime;

	private bool _introEnded;

	private float _readingTime;

	private IEnumerator Start()
	{
		NextButton.GetComponent<SpriteButton>().Init(28, 0);
		ButtonLabel.Phrase_ = ServerData.PhrasesE.ButtonContinue;
		foreach (SpriteButton button in _buttons.Values)
		{
			button.SetActive();
		}
		DaysPass30.gameObject.active = false;
		DaysPass4.gameObject.active = false;
		_gsFadeInOut = SingletonT<ServerData>.I.GameSettings.IntroFadeInOutTime;
		_gsReadingTime = SingletonT<ServerData>.I.GameSettings.IntroReadingTime;
		_gsDayPassTime = SingletonT<ServerData>.I.GameSettings.IntroDayPassTime;
		base.Release += ProcessButtons;
		HideButton();
		ChooseVoevoda();
		Header.Phrase_ = ServerData.PhrasesE.IntroPlace01;
		Title.Phrase_ = ServerData.PhrasesE.IntroTitle01;
		ScenarioText.LineSpacing_ = 1;
		ScenarioText.Phrase_ = ServerData.PhrasesE.Intro01Text;
		yield return StartCoroutine("FadeOut");
		ShowButton();
		yield return StartCoroutine("WaitForReading");
		yield return StartCoroutine("FadeIn");
		CharFrame.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
		Header.Phrase_ = ServerData.PhrasesE.Custom;
		Header.Text_ = string.Empty;
		Title.Phrase_ = ServerData.PhrasesE.Custom;
		Title.Text_ = string.Empty;
		ScenarioText.Phrase_ = ServerData.PhrasesE.Custom;
		ScenarioText.Text_ = string.Empty;
		yield return StartCoroutine("FadeOutCenter", DaysPass30);
		yield return new WaitForSeconds(_gsDayPassTime);
		yield return StartCoroutine("FadeInCenter", DaysPass30);
		CharFrame.parent.gameObject.SetActiveRecursivelyMk1(setActive: true);
		ChooseAskola();
		Header.Phrase_ = ServerData.PhrasesE.IntroPlace03;
		Title.Phrase_ = ServerData.PhrasesE.IntroTitle03;
		ScenarioText.LineSpacing_ = -3;
		ScenarioText.Phrase_ = ServerData.PhrasesE.Intro03Text;
		yield return StartCoroutine("FadeOut");
		ShowButton();
		yield return StartCoroutine("WaitForReading");
		yield return StartCoroutine("FadeIn");
		CharFrame.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
		Header.Phrase_ = ServerData.PhrasesE.Custom;
		Header.Text_ = string.Empty;
		Title.Phrase_ = ServerData.PhrasesE.Custom;
		Title.Text_ = string.Empty;
		ScenarioText.Phrase_ = ServerData.PhrasesE.Custom;
		ScenarioText.Text_ = string.Empty;
		yield return StartCoroutine("FadeOutCenter", DaysPass4);
		yield return new WaitForSeconds(_gsDayPassTime);
		yield return StartCoroutine("FadeInCenter", DaysPass4);
		CharFrame.parent.gameObject.SetActiveRecursivelyMk1(setActive: true);
		ChooseVoevoda();
		Header.Phrase_ = ServerData.PhrasesE.IntroPlace05;
		Title.Phrase_ = ServerData.PhrasesE.IntroTitle05;
		ScenarioText.LineSpacing_ = 1;
		ScenarioText.Phrase_ = ServerData.PhrasesE.Intro05Text;
		yield return StartCoroutine("FadeOut");
		_introEnded = true;
		ShowButton();
	}

	private void ProcessButtons(SpriteButton button)
	{
		if (button.name.Contains("continue"))
		{
			if (_introEnded)
			{
				Messenger.Invoke(Globals.Msg_StartIntro_Finished);
				return;
			}
			_readingTime = 0f;
			HideButton();
		}
	}

	private IEnumerator WaitForReading()
	{
		_readingTime = _gsReadingTime;
		float startTime = Time.time;
		while (Time.time < startTime + _readingTime && !_readingTime.Eqv(0f))
		{
			yield return new WaitForEndOfFrame();
		}
		HideButton();
	}

	private IEnumerator FadeOut()
	{
		float startTime = Time.time;
		SetMainTextAlpha(0f, 1f);
		yield return null;
		while (Time.time < startTime + _gsFadeInOut)
		{
			SetMainTextAlpha((Time.time - startTime) / _gsFadeInOut, 2f);
			yield return new WaitForEndOfFrame();
		}
		SetMainTextAlpha(1f, 1f);
		yield return null;
	}

	private IEnumerator FadeIn()
	{
		float startTime = Time.time;
		while (Time.time < startTime + _gsFadeInOut)
		{
			SetMainTextAlpha(1f - (Time.time - startTime) / _gsFadeInOut, 1f);
			yield return new WaitForEndOfFrame();
		}
		SetMainTextAlpha(0f, 1f);
		yield return null;
	}

	private IEnumerator FadeOutCenter(Transform dayspass)
	{
		float startTime = Time.time;
		dayspass.SetAlpha(0f);
		dayspass.gameObject.active = true;
		yield return null;
		while (Time.time < startTime + _gsFadeInOut)
		{
			dayspass.SetAlpha((Time.time - startTime) / _gsFadeInOut);
			yield return new WaitForEndOfFrame();
		}
		dayspass.SetAlpha(1f);
		yield return null;
	}

	private IEnumerator FadeInCenter(Transform dayspass)
	{
		float startTime = Time.time;
		while (Time.time < startTime + _gsFadeInOut)
		{
			dayspass.SetAlpha(1f - (Time.time - startTime) / _gsFadeInOut);
			yield return new WaitForEndOfFrame();
		}
		dayspass.SetAlpha(0f);
		dayspass.gameObject.active = false;
		yield return null;
	}

	private void SetMainTextAlpha(float alpha, float kImages)
	{
		Header.TextAlpha_ = alpha;
		Title.TextAlpha_ = alpha;
		ScenarioText.TextAlpha_ = alpha;
		CharFrame.parent.SetAlphaRecursively(alpha * kImages);
	}

	private void HideButton()
	{
		NextButton.parent.localPosition = InactiveButtonPosition;
	}

	private void ShowButton()
	{
		NextButton.parent.localPosition = ActiveButtonPosition;
	}

	private void ChooseVoevoda()
	{
		CharFrame.GetComponent<MeshRenderer>().material.mainTexture = Util.Resource<Texture2D>(voevoda2D);
	}

	private void ChooseAskola()
	{
		CharFrame.GetComponent<MeshRenderer>().material.mainTexture = Util.Resource<Texture2D>(askola2D);
	}

	private void Awake()
	{
		CharFrame.GetComponent<MeshRenderer>().material.mainTexture = Util.Resource<Texture2D>(voevoda2D);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		_subscriptions.Dispose();
	}

	private void OnBecameInvisible()
	{
	}

	private void Update()
	{
		ProcessRayCast();
	}
}
