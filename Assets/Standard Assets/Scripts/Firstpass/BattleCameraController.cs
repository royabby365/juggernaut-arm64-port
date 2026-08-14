using System.Collections;
using UnityEngine;
using Yarx;

public class BattleCameraController : MonoBehaviour
{
	public enum BattleCameraState
	{
		Start,
		Fight,
		Win,
		Bag,
		MoveTo,
		PreFatality,
		Dialog
	}

	private CompositeDisposable _subscriptions;

	private float _startCameraFov;

	private float _cameraFov;

	private Vector3 _cameraPos;

	private Vector3 _cameraTarget;

	private Vector3 _cameraPrevTarget;

	private GameObject _player;

	private Transform _playerBones;

	private GameObject _enemy;

	private Transform _enemyBones;

	private Transform _camera;

	private BattleCameraState _prevState = BattleCameraState.Bag;

	private BattleCameraState _currentState = BattleCameraState.Bag;

	private float _charactersDistance;

	public float StartDistance = 10f;

	private float _startRotation;

	private float _prevInterpolationSpeed;

	private HudMk1.GuiDesc _prevGui;

	private float _screenshotTimer;

	private GuiRoot.GuiType _currentGui;

	private bool _isBlockedByPopup;

	private Vector3 _prevMousePos;

	public float FightDistanceFactor = 1.4f;

	public float FightMinDistance = 3.5f;

	public Vector3 BagPosOffset = new Vector3(4.17f, 1.97f, 2.76f);

	public Vector3 BagTargetOffset = new Vector3(-2.2f, 0.99f, 0.15f);

	public float BagScreenSpaceLeftOffset = 200f;

	public Vector3 PreFatalityOffset = new Vector3(0f, 0.7f, 0f);

	public float PreFatalityFov = 20f;

	public float Speed = 1.8f;

	private bool _isDontInterpolateThisFrame;

	public float _cameraPosHeight = 1.9f;

	public float _cameraTargetHeight = 1f;

	public float _k = 0.45f;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<bool>.AddListener(Globals.MsgScreenshotAlertShowing, delegate(bool showing)
		{
			_isBlockedByPopup = showing;
		}));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, delegate(GuiRoot.GuiType old, GuiRoot.GuiType @new)
		{
			_currentGui = @new;
		}));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		SetCamera();
		_startCameraFov = _camera.GetComponent<Camera>().fieldOfView;
		_cameraFov = _startCameraFov;
		Speed = SingletonT<ServerData>.I.GameSettings.czInterpolationSpeed;
		_prevInterpolationSpeed = Speed;
	}

	private void SetCamera()
	{
		if (_camera == null)
		{
			GameObject gameObject = GameObject.Find(Globals.LocationGameObjectBattleCamera);
			if (gameObject != null)
			{
				_camera = gameObject.transform;
			}
		}
	}

	private void LateUpdate()
	{
		SetCamera();
		if (_player != Globals.PlayerGameObject || _playerBones == null)
		{
			InitPlayer(Globals.PlayerGameObject);
		}
		if (Globals.Enemy != null)
		{
			if (_enemy != Globals.Enemy.gameObject || _enemyBones == null)
			{
				InitEnemy(Globals.Enemy.gameObject);
			}
		}
		else if (_enemy != null)
		{
			InitEnemy(null);
		}
		if (!(_camera == null))
		{
			if (_currentState != BattleCameraState.PreFatality)
			{
				_cameraFov = _startCameraFov;
			}
			if (_currentState != BattleCameraState.Start)
			{
				Speed = _prevInterpolationSpeed;
			}
			switch (_currentState)
			{
			case BattleCameraState.Start:
				UpdateStart();
				InterpolateCamera();
				break;
			case BattleCameraState.Fight:
				UpdateFight();
				InterpolateCamera();
				break;
			case BattleCameraState.Win:
				UpdateWin();
				InterpolateCamera();
				break;
			case BattleCameraState.Bag:
				UpdateBag();
				InterpolateCamera();
				break;
			case BattleCameraState.MoveTo:
				break;
			case BattleCameraState.PreFatality:
				UpdatePreFatality();
				InterpolateCamera();
				break;
			case BattleCameraState.Dialog:
				UpdateDialog();
				InterpolateCamera();
				break;
			}
		}
	}

	private void InterpolateCamera()
	{
		if (!(_camera == null))
		{
			if (_isDontInterpolateThisFrame)
			{
				_camera.position = _cameraPos;
				_camera.LookAt(_cameraTarget);
				_cameraPrevTarget = _cameraTarget;
				_isDontInterpolateThisFrame = false;
				_camera.GetComponent<Camera>().fieldOfView = _cameraFov;
			}
			else
			{
				_camera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_camera.GetComponent<Camera>().fieldOfView, _cameraFov, RealDeltaTime() * Speed);
				_camera.position = Vector3.Lerp(_camera.position, _cameraPos, RealDeltaTime() * Speed);
				Vector3 vector = Vector3.Lerp(_cameraPrevTarget, _cameraTarget, RealDeltaTime() * Speed);
				_camera.LookAt(vector);
				_cameraPrevTarget = vector;
			}
		}
	}

	private void UpdateStart()
	{
		if (HudMk1.Instance == null || _player == null)
		{
			return;
		}
		Vector3 vector = ((!(_playerBones == null)) ? _playerBones.position : _player.transform.position);
		Vector3 vector2;
		if (!(_enemy == null))
		{
			vector2 = ((!(_enemyBones == null)) ? _enemyBones.position : _enemy.transform.position);
		}
		else if (Globals.Battle.ArenaCenter != null)
		{
			Vector3 vector3 = Globals.Battle.ArenaCenter.transform.position - new Vector3(vector.x, Globals.Battle.ArenaCenter.transform.position.y, vector.z);
			vector2 = Globals.Battle.ArenaCenter.transform.position + vector3.normalized * Globals.DistanceBetweenPersons;
		}
		else
		{
			vector2 = vector + _player.transform.rotation * (Vector3.forward * Globals.DistanceBetweenPersons);
		}
		float y = ((!(Globals.Battle.ArenaCenter == null)) ? Globals.Battle.ArenaCenter.transform.position.y : 0f);
		Vector3 rhs = new Vector3(vector2.x, y, vector2.z) - new Vector3(vector.x, y, vector.z);
		rhs.Normalize();
		Vector3 vector4 = Vector3.Cross(Vector3.up, rhs);
		vector4.Normalize();
		Vector3 vector5 = (vector2 + vector) / 2f;
		float speed = 10f;
		if (_isBlockedByPopup)
		{
			return;
		}
		float num = 0f;
		if (Input.GetMouseButtonDown(0))
		{
			_prevMousePos = Input.mousePosition;
		}
		if (Input.GetMouseButton(0))
		{
			num = Input.mousePosition.x - _prevMousePos.x;
			if (Globals.IsDebugInput)
			{
			}
			num *= 1024f / (float)Screen.width;
			_prevMousePos = Input.mousePosition;
		}
		if (SpriteGui.IsGuiActive)
		{
			if ((_currentGui == GuiRoot.GuiType.Fight || _currentGui == GuiRoot.GuiType.FightOnLocation) && UnityApi.ShowScreenshotButton() && Mathf.Abs(num) > 20f)
			{
				_screenshotTimer = 1.5f;
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.FightScreenshot);
			}
			if (_currentGui == GuiRoot.GuiType.FightScreenshot && UnityApi.ShowScreenshotButton() && num != 0f)
			{
				_screenshotTimer = 1.5f;
			}
		}
		else if (_currentGui == GuiRoot.GuiType.FightScreenshot)
		{
			if (UnityApi.ShowScreenshotButton() && num != 0f)
			{
				_screenshotTimer = 1.5f;
			}
		}
		else
		{
			_startRotation = 0f;
			Speed = _prevInterpolationSpeed;
		}
		if (_screenshotTimer > 0f)
		{
			Speed = speed;
			_startRotation += num * 0.3f;
			Quaternion quaternion = Quaternion.AngleAxis(_startRotation, Vector3.up);
			vector4 = quaternion * vector4;
			vector4.Normalize();
			_screenshotTimer -= Time.deltaTime;
		}
		else
		{
			_startRotation = 0f;
			Speed = _prevInterpolationSpeed;
			if (_currentGui == GuiRoot.GuiType.FightScreenshot)
			{
				HudMk1.Instance.ChangeGuiTo(_prevGui);
			}
		}
		_cameraPos = vector4 * StartDistance + vector5;
		_cameraTarget = vector5;
		_cameraPos.y = _cameraPosHeight;
		_cameraTarget.y = _cameraTargetHeight;
	}

	private void UpdateFight()
	{
		if (!(_player == null))
		{
			Vector3 vector = ((!(_playerBones == null)) ? _playerBones.position : _player.transform.position);
			Vector3 b = ((_enemy == null) ? Vector3.Reflect(vector, Vector3.up) : ((!(_enemyBones == null)) ? _enemyBones.position : _enemy.transform.position));
			Vector3 vector2 = new Vector3(b.x, 0f, b.z) - new Vector3(vector.x, 0f, vector.z);
			vector2.Normalize();
			Quaternion quaternion = Quaternion.LookRotation(vector2, Vector3.up);
			_charactersDistance = Vector3.Distance(vector, b) * FightDistanceFactor;
			_charactersDistance = Mathf.Clamp(_charactersDistance, FightMinDistance + ((!SingletonT<ServerData>.I._inBerserkMode) ? 0f : 1.2f), _charactersDistance);
			_cameraPos = vector + quaternion * new Vector3(_charactersDistance, 0f, 0f - _charactersDistance);
			_cameraPos.y = _cameraPosHeight + ((!SingletonT<ServerData>.I._inBerserkMode) ? 0f : 0.5f);
			_cameraTarget = vector + vector2 * _charactersDistance * _k;
			_cameraTarget.y = _cameraTargetHeight;
		}
	}

	private void UpdateWin()
	{
		if (!(_player == null))
		{
			Vector3 vector = ((!(_playerBones == null)) ? _playerBones.position : _player.transform.position);
			Vector3 vector2 = _cameraPos - vector;
			Quaternion quaternion = Quaternion.LookRotation(vector2.normalized, Vector3.up);
			Vector3 vector3 = Quaternion.Inverse(quaternion) * vector2;
			quaternion *= Quaternion.Euler(0f, -20f * Time.deltaTime, 0f);
			vector2 = quaternion * vector3;
			_cameraPos = vector2 + vector;
			_cameraTarget = vector;
			_cameraTarget.y = _cameraTargetHeight;
		}
	}

	private void UpdatePreFatality()
	{
		if (!(_player == null))
		{
			if (_playerBones == null)
			{
				Vector3 position = _player.transform.position;
			}
			else
			{
				Vector3 position = _playerBones.position;
			}
			Vector3 vector = ((_enemy == null) ? Globals.Battle.ArenaCenter.transform.position : ((!(_enemyBones == null)) ? _enemyBones.position : _enemy.transform.position));
			_cameraFov = PreFatalityFov;
			_cameraTarget = vector + PreFatalityOffset;
		}
	}

	private void UpdateDialog()
	{
		if (!(_player == null))
		{
			Vector3 vector = ((!(_playerBones == null)) ? _playerBones.position : _player.transform.position);
			Vector3 vector2;
			if (!(_enemy == null))
			{
				vector2 = ((!(_enemyBones == null)) ? _enemyBones.position : _enemy.transform.position);
			}
			else if (Globals.Battle.ArenaCenter != null)
			{
				Vector3 vector3 = Globals.Battle.ArenaCenter.transform.position - new Vector3(vector.x, Globals.Battle.ArenaCenter.transform.position.y, vector.z);
				vector2 = Globals.Battle.ArenaCenter.transform.position + vector3.normalized * Globals.DistanceBetweenPersons;
			}
			else
			{
				vector2 = vector + _player.transform.rotation * (Vector3.forward * Globals.DistanceBetweenPersons);
			}
			float y = ((!(Globals.Battle.ArenaCenter == null)) ? Globals.Battle.ArenaCenter.transform.position.y : 0f);
			Vector3 rhs = new Vector3(vector2.x, y, vector2.z) - new Vector3(vector.x, y, vector.z);
			rhs.Normalize();
			Vector3 vector4 = Vector3.Cross(Vector3.up, rhs);
			vector4.Normalize();
			Vector3 vector5 = (vector2 + vector) / 2f;
			_cameraPos = vector4 + vector5;
			_cameraTarget = _cameraPos + vector4 * 20f;
			_cameraPos.y = _cameraPosHeight;
			_cameraTarget.y = _cameraTargetHeight;
		}
	}

	private void UpdateBag()
	{
		if (!(_player == null))
		{
			Vector3 vector = ((!(_playerBones == null)) ? _playerBones.position : _player.transform.position);
			Ray ray = _camera.GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
			float y = Vector3.Angle(_camera.GetComponent<Camera>().ScreenPointToRay(new Vector3(BagScreenSpaceLeftOffset.DivideBy2DScale(), Screen.height / 2, 0f)).direction, ray.direction);
			Vector3 vector2 = vector - _cameraPos;
			Quaternion quaternion = Quaternion.Euler(0f, y, 0f);
			_cameraTarget = quaternion * vector2 + _cameraPos;
			_cameraTarget.y = _cameraTargetHeight;
		}
	}

	private void InitEnemy(GameObject gameObject)
	{
		_enemy = gameObject;
		if (_enemy != null)
		{
			_enemyBones = _enemy.transform.FindChildByName("bone_cam");
		}
		if (_enemyBones == null)
		{
			_enemyBones = _enemy.transform.FindChildByName("bones");
		}
	}

	private void InitPlayer(GameObject gameObject)
	{
		_player = gameObject;
		if (_player != null)
		{
			_playerBones = _player.transform.FindChildByName("bones");
		}
	}

	private float RealDeltaTime()
	{
		if (Time.timeScale == 0f)
		{
			return 0f;
		}
		return Time.deltaTime / Time.timeScale;
	}

	public void GoToMoveToState(Transform playerTransform, Transform enemyTransfrom, string targetName, string lookAtName, float moveTime, Vector3 translate, float timeout)
	{
		_prevState = _currentState;
		_currentState = BattleCameraState.MoveTo;
		StartCoroutine(MoveTo(playerTransform, enemyTransfrom, targetName, lookAtName, moveTime, translate, timeout));
	}

	private IEnumerator MoveTo(Transform playerTransform, Transform enemyTransfrom, string targetName, string lookAtName, float moveTime, Vector3 translate, float timeout)
	{
		Vector3 lookatpos = _cameraTarget;
		GameObject target = new GameObject("MoveToTarget");
		Transform lookAtTransform = null;
		switch (lookAtName)
		{
		case "center":
		case "0":
			lookatpos = _cameraTarget;
			lookAtTransform = null;
			break;
		case "player":
			lookatpos = playerTransform.position;
			lookAtTransform = playerTransform;
			break;
		case "playerup":
			lookatpos = playerTransform.position + new Vector3(0f, 1f, 0f);
			lookAtTransform = playerTransform;
			break;
		case "enemy":
			lookatpos = enemyTransfrom.position;
			lookAtTransform = enemyTransfrom;
			break;
		case "auto":
			yield break;
		}
		Transform bodypart = playerTransform.FindChildByName(targetName);
		Vector3 targetpos = _camera.position;
		if (targetName == "0" && (bool)bodypart)
		{
			targetpos = bodypart.transform.position;
		}
		else
		{
			switch (targetName)
			{
			case "center":
				targetpos = _camera.position;
				break;
			case "player":
				targetpos = playerTransform.position;
				break;
			case "enemy":
				targetpos = enemyTransfrom.position;
				break;
			}
		}
		target.transform.position = targetpos;
		target.transform.LookAt(lookatpos);
		target.transform.Translate(translate);
		targetpos = target.transform.position;
		target.transform.position = _camera.transform.position;
		float ttime = 0f;
		float timeStart = Time.time;
		_cameraFov = _startCameraFov;
		Vector3 startPos = _camera.position;
		ttime = moveTime;
		while (ttime > 0f)
		{
			_camera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_camera.GetComponent<Camera>().fieldOfView, _cameraFov, RealDeltaTime() * Speed);
			float dist = Vector3.Distance(_camera.position, targetpos);
			if ((double)dist < 0.1)
			{
				break;
			}
			float speed = dist / ttime;
			double k = 1.0 - (double)(ttime / moveTime);
			target.transform.LookAt(targetpos);
			target.transform.Translate(0f, 0f, Time.deltaTime * speed);
			ttime -= Time.deltaTime;
			_camera.position = target.transform.position;
			if (lookatpos != Vector3.zero)
			{
				_camera.LookAt(lookatpos);
			}
			yield return null;
		}
		Object.Destroy(target);
		if (timeout > 0f)
		{
			yield return new WaitForSeconds(timeout);
		}
	}

	private void EndMoveTo()
	{
		_currentState = _prevState;
	}

	public void GoToDialogState()
	{
		SwitchState(BattleCameraState.Dialog);
	}

	public void GoToStartState()
	{
		if (!(HudMk1.Instance == null))
		{
			_prevGui = HudMk1.Instance.CurrentGui;
			SwitchState(BattleCameraState.Start);
		}
	}

	public void GoToFightState()
	{
		SwitchState(BattleCameraState.Fight);
	}

	public void GoToWinState()
	{
		SwitchState(BattleCameraState.Win);
	}

	public void GoToBagState()
	{
		_cameraPos = _player.transform.TransformPoint(BagPosOffset);
		SwitchState(BattleCameraState.Bag);
	}

	public void GoToPreFatalityState()
	{
		SwitchState(BattleCameraState.PreFatality);
	}

	private void SwitchState(BattleCameraState newState)
	{
		_prevState = _currentState;
		_currentState = newState;
	}

	public Vector3 GetEnemyScreenSpacePivot()
	{
		Vector3 position = ((!(_enemy != null)) ? default(Vector3) : ((!(_enemyBones == null)) ? _enemyBones.position : _enemy.transform.position));
		Camera componentInChildren = _camera.GetComponentInChildren<Camera>();
		return componentInChildren.WorldToScreenPoint(position);
	}

	public void Shake()
	{
		StartCoroutine(ShakeCoro(1f, 0.5f));
	}

	private IEnumerator ShakeCoro(float shakeTime, float radius)
	{
		float time = 0f;
		while (time < shakeTime)
		{
			if (!Globals.IsPaused)
			{
				time += Time.deltaTime;
				_camera.transform.position += Random.onUnitSphere * radius * (1f - time / shakeTime);
			}
			yield return null;
		}
	}
}
