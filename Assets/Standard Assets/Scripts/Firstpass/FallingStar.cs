using UnityEngine;

public class FallingStar : MonoBehaviour
{
	private SpriteButton _button;

	private AudioSource _sound;

	public GameObject ClickEffectPrefab;

	private void Start()
	{
		_button = base.gameObject.AddComponent<SimplestButtonNoInit>();
		_button.name = "button_falling_star" + GetInstanceID();
		_button.Init(16, 16);
		_button.SetActive();
		_sound = base.gameObject.AddComponent<AudioSource>();
		_sound.volume = SingletonT<ServerData>.I.GameSettings.SoundsVolume;
		_sound.loop = true;
		_sound.clip = SingletonT<SoundManager>.I.GetSound(null, "falling_star");
		_sound.Play();
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Click += Instance_Click;
		}
	}

	private void Instance_Click(SpriteButton obj)
	{
		if (!Globals.ForceDontClickFallingStars && obj == _button)
		{
			Messenger.Invoke(Globals.MsgFallingStarClicked);
			GameObject gameObject = (GameObject)Object.Instantiate(ClickEffectPrefab);
			gameObject.transform.parent = base.gameObject.transform.parent;
			gameObject.transform.localPosition = base.gameObject.transform.localPosition;
			Suicidal suicidal = gameObject.AddComponent<Suicidal>();
			suicidal.SuicideTime = 0.5f;
			_button.UnregisterMe();
			Object.Destroy(base.gameObject);
			SingletonT<SoundManager>.I.PlayGlobalSound("star");
		}
	}

	private void Update()
	{
		if (!Globals.Battle.Pause)
		{
			float num = (float)SingletonT<ServerData>.I.GameSettings.FallingStarTime / 1000f;
			float num2 = (float)Camera2D.ScreenHeight / num;
			base.transform.localPosition += new Vector3(0f, (0f - num2) * Time.deltaTime, 0f);
		}
		if (base.transform.localPosition.y < (float)(-Camera2D.ScreenHeight / 2))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
