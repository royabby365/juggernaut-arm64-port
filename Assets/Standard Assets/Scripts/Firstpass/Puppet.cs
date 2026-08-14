using System.Collections;
using UnityEngine;
using Yarx;

public class Puppet : MonoBehaviour
{
	private enum StateE
	{
		Visible,
		FadeIn,
		FadeOut,
		Hidden
	}

	private const float FADE_TIME = 0.5f;

	private const float COOLDOWN_TIME = 1f;

	internal const float StarFlightTime = 0.6f;

	internal const float FxTime = 0.5f;

	private float _currentFadeTime;

	private float _currentCoolDown;

	private StateE _state;

	private Vector3[] _hiddenPositions;

	private Vector3[] _startPositions;

	private Quaternion _playerStartRot;

	private CompositeDisposable _subscriptions;

	private float _currentAngle;

	private float _currentSpeed;

	private float _smoothTime = 0.1f;

	private Rect _clickRect;

	private GameObject _highlightPrefab;

	private GameObject _highlightStars;

	public PuppetSlot[] PuppetSlots;

	public SpriteText StarCount;

	public SpriteButton ActivateUpgrades;

	public int ElbowSlotIndex = 3;

	public Sprite BagTopBar;

	public Sprite BagRightBar;

	public GameObject FxStarPrefab;

	public Sprite StarIcon;

	public Transform FxRoot;

	private BagPhotoButton screenshotButton;

	private float _rot;

	private bool _puppetMove;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, BeforeSwitchGuiHandler));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPost, PostSwitchGuiHandler));
		_subscriptions.Add(Messenger<ServerData.MoneyType.TypeE, string>.AddListener(Globals.MsgPlayerFundsChanged, FundsChanged));
		_subscriptions.Add(Messenger<GameObject>.AddListener(Globals.MsgStarIncreased, StarChanged));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		screenshotButton = base.transform.GetComponentInChildren<BagPhotoButton>();
		SpriteGui spriteGui = base.transform.GetSpriteGui();
		spriteGui.Move += Gui_Move;
		spriteGui.MoveBegin += GuiOnMoveBegin;
		spriteGui.MoveEnd += GuiOnMoveEnd;
		spriteGui.Release += OnScreenShotClicked;
		_hiddenPositions = new Vector3[PuppetSlots.Length];
		_startPositions = new Vector3[PuppetSlots.Length];
		float num = PuppetSlots[0].GetComponent<Sprite>().Width;
		Vector3 vector = default(Vector3);
		PuppetSlot[] puppetSlots = PuppetSlots;
		foreach (PuppetSlot puppetSlot in puppetSlots)
		{
			vector += puppetSlot.transform.localPosition;
		}
		vector /= (float)PuppetSlots.Length;
		for (int j = 0; j < PuppetSlots.Length; j++)
		{
			Vector3 vector2 = PuppetSlots[j].transform.localPosition - vector;
			vector2.Normalize();
			ref Vector3 reference = ref _hiddenPositions[j];
			reference = vector2 * ((float)Camera2D.ScreenWidth + num) + PuppetSlots[j].transform.localPosition;
			ref Vector3 reference2 = ref _startPositions[j];
			reference2 = PuppetSlots[j].transform.localPosition;
		}
		if (screenshotButton != null)
		{
			screenshotButton.transform.localPosition = _hiddenPositions[ElbowSlotIndex];
		}
		_highlightPrefab = Util.Resource<GameObject>("_z_prefabs/combo_button_highlight");
	}

	private void Update()
	{
		if (HudMk1.Instance == null || (HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.BagItems && HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.BagStats))
		{
			return;
		}
		_currentAngle = Mathf.SmoothDampAngle(_currentAngle, _rot, ref _currentSpeed, _smoothTime);
		Globals.Player.transform.localRotation = Quaternion.Euler(new Vector3(Globals.Player.transform.localEulerAngles.x, _currentAngle, Globals.Player.transform.localEulerAngles.z));
		switch (_state)
		{
		case StateE.Visible:
			break;
		case StateE.FadeIn:
		{
			float value = _currentFadeTime / 0.5f;
			value = Mathf.Clamp01(value);
			MoveGui(1f - value);
			if (_currentFadeTime >= 0.5f)
			{
				_state = StateE.Visible;
			}
			_currentFadeTime += Time.deltaTime;
			break;
		}
		case StateE.FadeOut:
		{
			float value = _currentFadeTime / 0.5f;
			value = Mathf.Clamp01(value);
			MoveGui(value);
			if (_currentFadeTime >= 0.5f)
			{
				_state = StateE.Hidden;
				_currentCoolDown = 0f;
			}
			_currentFadeTime += Time.deltaTime;
			break;
		}
		case StateE.Hidden:
			if (_currentCoolDown >= 1f)
			{
				_state = StateE.FadeIn;
				_currentFadeTime = 0f;
				_rot = _playerStartRot.eulerAngles.y;
			}
			_currentCoolDown += Time.deltaTime;
			break;
		}
	}

	private void MoveGui(float amount)
	{
		for (int i = 0; i < PuppetSlots.Length; i++)
		{
			PuppetSlots[i].transform.localPosition = Vector3.Lerp(_startPositions[i], _hiddenPositions[i], amount);
		}
		if (screenshotButton != null)
		{
			screenshotButton.transform.localPosition = Vector3.Lerp(_hiddenPositions[ElbowSlotIndex], _startPositions[ElbowSlotIndex], amount);
		}
	}

	private void GuiOnMoveEnd(Vector3 vector3)
	{
		_puppetMove = false;
	}

	private void GuiOnMoveBegin(Vector3 vector3)
	{
		vector3 *= Camera2D.Scale;
		_puppetMove = _clickRect.Contains(vector3);
	}

	private void Gui_Move(Vector3 arg1, Vector3 arg2)
	{
		if (HudMk1.Instance == null || (HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.BagItems && HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.BagStats))
		{
			return;
		}
		arg1 *= Camera2D.Scale;
		arg2 *= Camera2D.Scale;
		if (_clickRect.Contains(arg1) && _clickRect.Contains(arg2) && _puppetMove)
		{
			_rot -= (arg2.x - arg1.x) * 0.5f;
			if (_state == StateE.Visible)
			{
				_currentFadeTime = 0f;
				_state = StateE.FadeOut;
			}
			else if (_state == StateE.FadeIn)
			{
				_currentFadeTime = 0.5f - _currentFadeTime;
				_state = StateE.FadeOut;
			}
			else
			{
				_currentCoolDown = 0f;
			}
		}
	}

	private void BeforeSwitchGuiHandler(GuiRoot.GuiType oldGui, GuiRoot.GuiType newGui)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		if (oldGui != GuiRoot.GuiType.BagItems && oldGui != GuiRoot.GuiType.BagStats && (newGui == GuiRoot.GuiType.BagItems || newGui == GuiRoot.GuiType.BagStats))
		{
			_playerStartRot = Globals.Player.transform.localRotation;
		}
		_rot = _playerStartRot.eulerAngles.y;
		_currentAngle = _rot;
		if ((oldGui == GuiRoot.GuiType.BagItems || oldGui == GuiRoot.GuiType.BagStats) && newGui != GuiRoot.GuiType.BagItems && newGui != GuiRoot.GuiType.BagStats)
		{
			if (Globals.Player != null)
			{
				Globals.Player.transform.localRotation = _playerStartRot;
			}
			for (int i = 0; i < PuppetSlots.Length; i++)
			{
				PuppetSlots[i].transform.localPosition = _startPositions[i];
			}
			if (screenshotButton != null)
			{
				screenshotButton.transform.localPosition = _hiddenPositions[ElbowSlotIndex];
			}
			_state = StateE.Visible;
			_currentCoolDown = 1f;
			_currentFadeTime = 0.5f;
		}
	}

	private void PostSwitchGuiHandler(GuiRoot.GuiType oldGui, GuiRoot.GuiType newGui)
	{
		if (HudMk1.Instance == null || (newGui != GuiRoot.GuiType.BagItems && newGui != GuiRoot.GuiType.BagStats))
		{
			return;
		}
		if (_clickRect == default(Rect))
		{
			_clickRect.xMin = float.MaxValue;
			_clickRect.yMin = float.MaxValue;
			_clickRect.xMax = float.MinValue;
			_clickRect.yMax = float.MinValue;
			PuppetSlot[] puppetSlots = PuppetSlots;
			foreach (PuppetSlot puppetSlot in puppetSlots)
			{
				Vector3 vector = HudMk1.Instance.GetComponent<Camera>().WorldToScreenPoint(puppetSlot.transform.position);
				if (_clickRect.xMin > vector.x)
				{
					_clickRect.xMin = vector.x;
				}
				if (_clickRect.yMin > vector.y)
				{
					_clickRect.yMin = vector.y;
				}
				if (_clickRect.xMax < vector.x)
				{
					_clickRect.xMax = vector.x;
				}
				if (_clickRect.yMax < vector.y)
				{
					_clickRect.yMax = vector.y;
				}
			}
			float num = PuppetSlots[0].GetComponent<Sprite>().Width;
			_clickRect.yMin -= num;
			_clickRect.xMax += num;
			_clickRect.xMax = Camera2D.ScreenWidth / 2;
		}
		if (ShouldPointStar(oldGui, newGui) && SingletonT<ServerData>.I.PlayerParams.MoneyStarsCount > 0)
		{
			_highlightStars = (GameObject)Object.Instantiate(_highlightPrefab);
			_highlightStars.transform.parent = FxRoot;
			_highlightStars.transform.localPosition = new Vector3(0f, 0f, 50f);
			StartCoroutine(ShowHighlight(_highlightStars));
		}
	}

	private bool ShouldPointStar(GuiRoot.GuiType oldGui, GuiRoot.GuiType newGui)
	{
		return GuiRoot.BagTypes.Contains(newGui) && !GuiRoot.ModalTypes.Contains(oldGui);
	}

	private void FundsChanged(ServerData.MoneyType.TypeE type, string reason)
	{
		string text_ = type.GetPlayerFundsCount().ToString();
		if (type == ServerData.MoneyType.TypeE.Star)
		{
			StarCount.Text_ = text_;
		}
	}

	private void OnScreenShotClicked(SpriteButton spriteButton)
	{
		if (spriteButton != null && spriteButton.name == "slot_screen_shot")
		{
			StartCoroutine(SaveScreenshot());
		}
	}

	private IEnumerator ScaleTexture(Texture2D source, Texture2D result)
	{
		Debug.Log("ScaleTexture");
		Color[] rpixels = result.GetPixels(0);
		float incX = 1f / (float)source.width * ((float)source.width / (float)result.width);
		float incY = 1f / (float)source.height * ((float)source.height / (float)result.height);
		int quant = 30000;
		int counter = 0;
		for (int px = 0; px < rpixels.Length; px++)
		{
			rpixels[px] = source.GetPixelBilinear(incX * ((float)px % (float)result.width), incY * Mathf.Floor(px / result.width));
			counter++;
			if (counter >= quant)
			{
				counter = 0;
				yield return new WaitForEndOfFrame();
			}
		}
		result.SetPixels(rpixels, 0);
		result.Apply();
	}

	public IEnumerator SaveScreenshot()
	{
		if (screenshotButton != null)
		{
			screenshotButton.transform.localScale = Vector3.zero;
		}
		yield return new WaitForEndOfFrame();
		string path = Application.persistentDataPath + "/SavedScreen.png";
		ScreenCapture.CaptureScreenshot(path);
		yield return new WaitForSeconds(2f);
		UnityApi.PostScreenshot();
		if (screenshotButton != null)
		{
			screenshotButton.transform.localScale = Vector3.one;
		}
		Messenger<ServerData.PhrasesE>.Invoke(Globals.MsgShowAlert, ServerData.PhrasesE.PostScreenshotToFacebook);
		yield return null;
	}

	private void StarChanged(GameObject gameObject)
	{
		StartCoroutine(AnimateStars(gameObject.transform));
	}

	private IEnumerator AnimateStars(Transform destPos)
	{
		float curStarTime = 0f;
		float curFxTime = 0f;
		GameObject iconClone = (GameObject)Object.Instantiate(StarIcon.gameObject);
		GameObject fx = (GameObject)Object.Instantiate(FxStarPrefab);
		fx.transform.parent = StarIcon.transform;
		fx.transform.localPosition = new Vector3(0f, 0f, -100f);
		while (curStarTime < 0.6f)
		{
			curStarTime += Time.deltaTime;
			curFxTime += Time.deltaTime;
			if (fx != null && curFxTime >= 0.5f)
			{
				GameObject ffx = fx;
				fx = null;
				Object.Destroy(ffx);
			}
			iconClone.transform.position = Vector3.Lerp(StarIcon.transform.position, destPos.position, curStarTime / 0.6f);
			yield return null;
		}
		if (fx != null)
		{
			Object.Destroy(fx);
		}
		iconClone.transform.position = destPos.position;
		Object.Destroy(iconClone);
		fx = (GameObject)Object.Instantiate(FxStarPrefab);
		fx.transform.parent = destPos;
		fx.transform.localPosition = new Vector3(0f, 0f, -100f);
		yield return new WaitForSeconds(0.5f);
		Object.Destroy(fx);
	}

	private IEnumerator ShowHighlight(GameObject highlight)
	{
		float time = 0f;
		Vector3 scale = Vector3.one * 100f;
		while (time < 0.5f)
		{
			highlight.transform.localScale = Vector3.Lerp(Vector3.zero, scale, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		highlight.transform.localScale = scale;
		yield return new WaitForSeconds(2f);
		time = 0f;
		while (time < 0.5f)
		{
			highlight.transform.localScale = Vector3.Lerp(scale, Vector3.zero, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		highlight.transform.localScale = Vector3.zero;
		Object.Destroy(highlight);
	}
}
