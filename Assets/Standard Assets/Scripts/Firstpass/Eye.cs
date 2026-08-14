using System.Collections;
using UnityEngine;

internal class Eye : MonoBehaviour
{
	public GameObject SpawnDieEffect;

	private GameObject _currentEffect;

	private float _fadeTime = 0.2f;

	private float _explosionTime = 0.7f;

	private float _currentTime;

	private Transform _eye;

	private readonly int _maxHits = 5;

	private int _currentHits;

	private bool _Hited;

	private Camera _camera;

	private TimePeriod _timePeriod;

	internal bool Pause
	{
		set
		{
			if (_timePeriod != null)
			{
				_timePeriod.Pause = value;
			}
		}
	}

	public bool Hited
	{
		get
		{
			return _Hited;
		}
		set
		{
			if (_Hited != value)
			{
				_Hited = value;
			}
		}
	}

	private void Start()
	{
		_eye = base.transform.Find("eye");
		_eye.localScale = Vector3.zero;
		_camera = GetComponentInChildren<Camera>();
		_timePeriod = new TimePeriod(SingletonT<ServerData>.I.GameSettings.EyePeriod);
		_currentTime = _fadeTime;
		StartCoroutine("FadeInCoroutine");
	}

	private void SpawnNewEffect()
	{
		if (SpawnDieEffect != null)
		{
			_currentEffect = (GameObject)Object.Instantiate(SpawnDieEffect);
			_currentEffect.transform.parent = base.transform;
			_currentEffect.transform.localPosition = new Vector3(0f, 0f, 0.5f);
			_currentEffect.transform.localRotation = Quaternion.identity;
		}
	}

	private IEnumerator FadeInCoroutine()
	{
		SingletonT<SoundManager>.I.PlayGlobalSound("eye_show");
		while (_currentTime > 0f)
		{
			_currentTime -= Time.deltaTime;
			float scale = 1f - _currentTime / _fadeTime;
			scale = Mathf.Clamp01(scale);
			_eye.localScale = new Vector3(scale, scale, scale);
			yield return null;
		}
		Object.Destroy(_currentEffect);
	}

	private void Update()
	{
		if (_timePeriod == null || Hited)
		{
			return;
		}
		if (!Globals.Battle.Pause)
		{
			_timePeriod.Update();
			if (_timePeriod.Fired && !_timePeriod.Pause)
			{
				if (SingletonT<ServerData>.I.PlayerParams.RageSpheresCount > 0)
				{
					SingletonT<ServerData>.I.PlayerParams.RageSpheresCount--;
					Messenger.Invoke(Globals.MsgEyeEatRage);
					Metrics.OnEyeEatRage();
				}
				_timePeriod.Reset();
			}
		}
		if (SingletonT<ServerData>.I.PlayerParams.RageSpheresCount <= 0)
		{
			Die();
		}
		else
		{
			if (Globals.ForceDontHitEye || Globals.IsPaused || !Input.GetMouseButtonDown(0))
			{
				return;
			}
			Vector3 position = _camera.ScreenToViewportPoint(Input.mousePosition);
			Ray ray = _camera.ViewportPointToRay(position);
			RaycastHit[] array = Physics.RaycastAll(ray, 10000f, 67108864);
			foreach (RaycastHit raycastHit in array)
			{
				if (raycastHit.collider.gameObject.transform.root.Equals(base.transform.root) && _currentHits < _maxHits)
				{
					SingletonT<SoundManager>.I.PlayGlobalSound("eye_tap");
					_currentHits++;
					float num = (float)(_maxHits - _currentHits) / (float)_maxHits;
					_eye.localScale = new Vector3(num, num, num);
					if (_currentHits == _maxHits)
					{
						Die();
						Messenger.Invoke(Globals.MsgFightEyeDieByClick);
					}
				}
			}
		}
	}

	internal void Die()
	{
		Messenger.Invoke(Globals.MsgFightEyeDie);
		_timePeriod = null;
		_currentTime = _fadeTime;
		StartCoroutine("DestroyMe");
	}

	private IEnumerator DestroyMe()
	{
		_eye.localScale = default(Vector3);
		SingletonT<SoundManager>.I.PlayGlobalSound("eye_die");
		SpawnNewEffect();
		yield return new WaitForSeconds(_explosionTime);
		Object.Destroy(base.transform.root.gameObject);
	}
}
