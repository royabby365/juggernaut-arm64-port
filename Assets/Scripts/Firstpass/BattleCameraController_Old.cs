using System.Collections;
using UnityEngine;

public class BattleCameraController_Old : MonoBehaviour
{
	private const int CAMERA_MODE = 0;

	private const float CHARACTERS_DISTANCE = 1.6f;

	public int mode;

	public Vector3 mouselook_angle;

	public Vector3 mouselook_last_angle;

	public Vector3 mouselook_last_pos;

	public bool thirdlook;

	public float distance = 2f;

	public float vibration;

	public Vector3 addOffset = new Vector3(0f, 1f, 0f);

	private bool wait_move_to;

	private bool gameover;

	private float thirdlook_timer;

	private float start_distance = -20f;

	private bool update_angle;

	private bool update_position;

	private Vector3 smooth = new Vector3(25f, 1f, 30f);

	private Vector2 distance_range = new Vector2(6f, 11.5f);

	private GameObject cam;

	private Transform cam_transform;

	public bool ignore_CameraEnemyBonesOffset;

	public GameObject player;

	public GameObject enemy;

	private PersonData player_params;

	private PersonData enemy_params;

	private Vector3 player_pos;

	private Vector3 enemy_pos;

	private Transform player_bones;

	private Transform enemy_bones;

	private Transform player_transform;

	private Transform enemy_transform;

	private Vector3 player_bone_cam_offset = new Vector3(0f, 0f, 0f);

	private Transform player_bone_cam_transform;

	private Transform enemy_bone_cam_transform;

	private float characters_distance;

	private float characters_distance_velocity;

	private Transform target_look;

	private Transform target_pos;

	private GameObject target_dummy;

	private GameObject arenacenter;

	private float arenacenter_y;

	private bool lock_move;

	private Vector3[] lock_data;

	private bool in_game_over;

	private bool startfly = true;

	private float addAngle = 60f;

	private float _addAngleChangeTime;

	private bool _seted;

	private float prev_add_height;

	private float vibration_timeout;

	private float vibration_start_time;

	private float vibration_sign = 1f;

	private float vibration_back_value;

	private bool in_vibration;

	private Transform MoveToArg1;

	private Transform MoveToArg2;

	private string MoveToArg3;

	private string MoveToArg4;

	private float MoveToArg5;

	private Vector3 MoveToArg6;

	private float MoveToArg7;

	private float vsign = -1f;

	private IEnumerator Start()
	{
		cam = GameObject.Find(Globals.LocationGameObjectBattleCamera);
		cam_transform = cam.transform;
		GameObject newobj = new GameObject("camera_target_pos");
		target_pos = newobj.transform;
		newobj = new GameObject("camera_target_look");
		target_look = newobj.transform;
		target_dummy = new GameObject("camera_target_dummy");
		arenacenter = GameObject.Find("arena_center");
		arenacenter_y = arenacenter.transform.position.y + 0.8f;
		cam_transform.position = arenacenter.transform.position;
		cam_transform.rotation = arenacenter.transform.rotation;
		yield return new WaitForSeconds(0.01f);
		cam_transform.Translate(start_distance, 2f, 0f);
		update_angle = true;
		update_position = true;
		gameover = false;
	}

	private void OnDisable()
	{
		if ((bool)target_pos)
		{
			Object.Destroy(target_pos.gameObject);
			target_pos = null;
		}
		if ((bool)target_look)
		{
			Object.Destroy(target_look.gameObject);
			target_look = null;
		}
		if ((bool)target_dummy)
		{
			Object.Destroy(target_dummy.gameObject);
			target_dummy = null;
		}
	}

	private void Update()
	{
	}

	public void LateUpdate()
	{
		if (gameover)
		{
			cam_transform.RotateAround(player_pos, Vector3.up, -20f * Time.deltaTime);
			return;
		}
		bool mouseButtonDown = Input.GetMouseButtonDown(1);
		bool mouseButtonUp = Input.GetMouseButtonUp(1);
		if (false)
		{
			return;
		}
		if (!lock_move && mouseButtonDown)
		{
			mouselook_last_pos = cam_transform.position;
			mouselook_last_angle = cam_transform.eulerAngles;
			thirdlook_timer += RealDeltaTime();
		}
		Input.GetMouseButton(1);
		mode = 0;
		if (!lock_move && (bool)player && (bool)enemy)
		{
			if (startfly)
			{
				if (smooth.x > smooth.y)
				{
					smooth.x -= RealDeltaTime() * 10f;
				}
				else
				{
					startfly = false;
				}
			}
			else if (!IsScenariosPlaying())
			{
				if (smooth.x < smooth.z)
				{
					smooth.x += RealDeltaTime() * 10f;
				}
			}
			else
			{
				smooth.x = smooth.y;
			}
			UpdateCharactersDistance();
			if (update_position)
			{
				UpdateCameraPosition();
			}
			if (update_angle)
			{
				UpdateCameraAngle();
			}
		}
		else if (!lock_move && (bool)player && !enemy)
		{
			Transform transform = target_dummy.transform;
			transform.position = player_transform.position;
			transform.eulerAngles = player_transform.eulerAngles;
			transform.Translate(0f, 0.6f, 3.2f);
			enemy = target_dummy;
			enemy_bones = enemy.transform;
			enemy_transform = enemy.transform;
		}
	}

	public void Restart()
	{
		gameover = false;
		update_position = true;
		update_angle = true;
	}

	private IEnumerator GameOverMode()
	{
		yield return new WaitForSeconds(1.5f);
		GameObject target = new GameObject("MoveToTarget");
		target.transform.position = target_look.position;
		update_angle = false;
		update_position = false;
		Vector3 playerup = player_transform.position + new Vector3(0f, 1f, 0f);
		Transform targetTransform = target.transform;
		while ((double)Vector3.Distance(targetTransform.position, playerup) > 0.1)
		{
			targetTransform.LookAt(playerup);
			targetTransform.Translate(0f, 0f, Time.deltaTime * 5f);
			cam_transform.LookAt(target.transform);
			cam_transform.Translate(0f, 0f, Time.deltaTime * 2f);
			yield return null;
		}
		yield return MoveTo2(player_transform, enemy_transform, "player", "playerup", 1.5f, new Vector3(-3.5f, 0f, 2f), 0f);
		gameover = true;
		Object.Destroy(target);
	}

	public void SetPlayer(GameObject inPlayer)
	{
		player = inPlayer;
		player_bones = inPlayer.transform.Find("bones");
		player_transform = inPlayer.transform;
		player_bone_cam_transform = player_transform.FindChildByName("bone_cam");
		player_params = Globals.Player.GetComponent<PersonData>();
	}

	public void SetEnemy(GameObject inEnemy)
	{
		enemy = inEnemy;
		enemy_bones = inEnemy.transform.Find("bones");
		enemy_transform = inEnemy.transform;
		enemy_bone_cam_transform = enemy_transform.FindChildByName("bone_cam");
		enemy_params = Globals.Enemy.GetComponent<PersonData>();
	}

	public void SetAddAngle(float angle)
	{
		_addAngleChangeTime = Time.time;
		addAngle = angle;
	}

	public void SetDistance(float v)
	{
		distance = v;
	}

	public void SetPlayerBoneCamOffset(Vector3 v)
	{
		player_bone_cam_offset = v;
	}

	public void SetAddOffset(Vector3 offset)
	{
		addOffset = offset;
	}

	private void UpdateCharactersDistance()
	{
		if (!player_transform && !enemy_transform)
		{
			return;
		}
		if (player_pos == Vector3.zero)
		{
			player_pos = player_transform.position;
		}
		if (enemy_pos == Vector3.zero)
		{
			enemy_pos = enemy_transform.position;
		}
		Vector3 to;
		Vector3 to2;
		if (mode == 0)
		{
			to = player_bones.position;
			Transform transform = player_bone_cam_transform;
			if ((bool)player_bone_cam_transform)
			{
				to = new Vector3(transform.position.x, to.y, transform.position.z);
			}
			to2 = enemy_bones.position;
			transform = enemy_bone_cam_transform;
			if ((bool)enemy_bone_cam_transform)
			{
				to2 = new Vector3(transform.position.x, to2.y, transform.position.z);
			}
		}
		else
		{
			to = player_transform.position;
			to2 = enemy_transform.position;
		}
		player_pos = Vector3.Lerp(player_pos, to, 2f);
		enemy_pos = Vector3.Lerp(enemy_pos, to2, 2f);
		if (player_pos.y < arenacenter_y)
		{
			player_pos.y = arenacenter_y;
		}
		if (enemy_pos.y < arenacenter_y)
		{
			enemy_pos.y = arenacenter_y;
		}
		Vector3 position = player_transform.position;
		player_transform.position = player_pos + player_bone_cam_offset;
		Vector3 a = player_transform.TransformPoint(new Vector3(0f, 0f, (!(player_params != null)) ? 0f : player_params.CameraBonesOffset));
		player_transform.position = position;
		position = enemy_transform.position;
		enemy_transform.position = enemy_pos;
		Vector3 b = enemy_transform.TransformPoint(new Vector3(0f, 0f, (!(enemy_params != null) || ignore_CameraEnemyBonesOffset) ? 0f : enemy_params.CameraEnemyBonesOffset));
		enemy_transform.position = position;
		float num = Vector3.Distance(a, b);
		if ((double)num < 0.1)
		{
			num = 0.1f;
		}
		if (characters_distance == 0f)
		{
			characters_distance = num;
		}
		characters_distance = Mathf.Lerp(characters_distance, num, RealDeltaTime() * 15f);
	}

	private void UpdateCameraPosition()
	{
		if (thirdlook)
		{
			if (mode == 1)
			{
				target_pos.position = player_transform.position;
				target_pos.rotation = player_transform.rotation;
				target_pos.Translate(2f, 2f, -3f);
			}
			else
			{
				target_pos.position = player_bones.position;
				target_pos.rotation = player_transform.rotation;
				target_pos.Translate(2f, 1f, -3f);
			}
		}
		else
		{
			target_pos.position = player_transform.position;
			target_pos.LookAt(enemy_transform.position);
			float num = characters_distance * distance + Mathf.Max((!(player_params != null)) ? 0f : player_params.CameraAddDistance, (!(enemy_params != null)) ? 0f : enemy_params.CameraEnemyAddDistance);
			if (num < distance_range.x)
			{
				num = distance_range.x;
			}
			if (num > distance_range.y)
			{
				num = distance_range.y;
			}
			target_pos.Translate(num, 1f - vibration, characters_distance / 2f);
		}
		if (!_seted && (bool)cam_transform && (bool)arenacenter && (bool)arenacenter.transform)
		{
			cam_transform.position = target_pos.position + addOffset;
			cam_transform.rotation = target_pos.rotation;
			cam_transform.RotateAround(arenacenter.transform.position, new Vector3(0f, 1f, 0f), addAngle);
		}
	}

	private void SetCamAtTarget()
	{
		if ((bool)player_transform)
		{
			target_pos.position = player_transform.position;
			target_pos.LookAt(enemy_transform.position);
			float num = characters_distance * distance;
			if (num < distance_range.x)
			{
				num = distance_range.x;
			}
			if (num > distance_range.y)
			{
				num = distance_range.y;
			}
			target_pos.Translate(num, 1f - vibration, characters_distance / 2f);
			cam_transform.position = target_pos.position;
		}
	}

	private void UpdateCameraAngle()
	{
		float num = ((mode != 1) ? 0.5f : 1.5f);
		Vector3 position = target_look.position;
		target_look.position = player_pos;
		target_look.LookAt(enemy_pos);
		target_look.position = target_look.TransformPoint(new Vector3(0f, 0f, (!(player_params != null)) ? 0f : player_params.CameraBonesOffset));
		target_look.LookAt(enemy_pos);
		float num2 = characters_distance / 1.9f;
		if (num2 < 0.1f)
		{
			num2 = 0.1f;
		}
		target_look.Translate(0f, num - vibration, num2);
		if (position == Vector3.zero)
		{
			position = target_look.position;
		}
		target_look.position = Vector3.Lerp(position, target_look.position, RealDeltaTime() * 8f * 20f / smooth.x);
		float num3 = ((!(player_params != null)) ? 0f : player_params.CameraAddHeight);
		if ((bool)enemy_params && (double)enemy_params.CameraEnemyAddHeight != 0.0)
		{
			num3 = (((double)num3 != 0.0) ? ((enemy_params.CameraEnemyAddHeight + num3) / 2f) : enemy_params.CameraEnemyAddHeight);
		}
		cam_transform.LookAt(AddY(target_look.position, num3));
	}

	private IEnumerator Vibration(float power, int count, float increase, float timeout)
	{
		if (in_vibration)
		{
			yield break;
		}
		in_vibration = true;
		if (mode != 1)
		{
			vibration_timeout = timeout;
			float t = Time.time;
			vibration_start_time = Time.time;
			power /= 10f;
			vibration_sign = 1f;
			for (int i = 0; i < count; i++)
			{
				smooth.x = smooth.y;
				power += increase / 10f;
				vibration_start_time = Time.time;
				yield return new WaitForSeconds(timeout);
				vibration += power;
				vibration_start_time = Time.time;
				yield return new WaitForSeconds(timeout);
				vibration_start_time = Time.time;
				vibration_back_value = vibration;
				vibration_sign = -1f;
				vibration -= power;
			}
			in_vibration = false;
		}
	}

	private IEnumerator SetOnPosition(string bodypart_name, string isparent, string lookat, Vector3 pos, float timeout, string slowmo)
	{
		Vector3 oldPos = cam_transform.position;
		Quaternion oldRot = cam_transform.rotation;
		update_angle = false;
		update_position = false;
		cam_transform.position = player_pos;
		Transform bodypart = player_transform.FindChildByName(bodypart_name);
		if ((bool)bodypart)
		{
			cam_transform.position = bodypart.transform.position;
			if (isparent == "1")
			{
				cam_transform.parent = bodypart.transform;
			}
		}
		cam_transform.Translate(pos);
		if (lookat == "center" || lookat == "0")
		{
			cam_transform.LookAt(target_look);
		}
		if ((bool)bodypart && lookat == "0")
		{
			cam_transform.rotation = bodypart.transform.rotation;
		}
		if (lookat == "enemy")
		{
			cam_transform.LookAt(enemy_pos);
		}
		if (lookat == "auto")
		{
			update_angle = true;
		}
		if (slowmo == "1")
		{
			Slowmo(timeout);
		}
		yield return new WaitForSeconds(timeout);
		if (isparent == "1")
		{
			cam_transform.parent = null;
		}
		cam_transform.position = oldPos;
		cam_transform.rotation = oldRot;
		update_angle = true;
		update_position = true;
	}

	private IEnumerator MoveTo3(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float speed, Vector3 translate, float timeout)
	{
		update_angle = false;
		update_position = false;
		Vector3 lookatpos = default(Vector3);
		GameObject target = new GameObject("MoveToTarget");
		switch (lookatname)
		{
		case "center":
		case "0":
			lookatpos = target_look.position;
			break;
		case "player":
			lookatpos = playertransform.position;
			break;
		case "playerup":
			lookatpos = playertransform.position + new Vector3(0f, 1f, 0f);
			break;
		case "enemy":
			lookatpos = enemytransform.position;
			break;
		case "auto":
			update_angle = true;
			break;
		}
		Transform bodypart = player_transform.FindChildByName(targetname);
		Vector3 targetpos = default(Vector3);
		if (targetname == "0" && (bool)bodypart)
		{
			targetpos = bodypart.transform.position;
		}
		else
		{
			switch (targetname)
			{
			case "center":
				targetpos = target_look.position;
				break;
			case "player":
				targetpos = playertransform.position;
				break;
			case "enemy":
				targetpos = enemytransform.position;
				break;
			}
		}
		target.transform.position = targetpos;
		target.transform.LookAt(lookatpos);
		target.transform.Translate(translate);
		targetpos = target.transform.position;
		target.transform.position = cam_transform.position;
		while ((double)Vector3.Distance(cam_transform.position, targetpos) > 0.1)
		{
			target.transform.LookAt(targetpos);
			target.transform.Translate(0f, 0f, Time.deltaTime * speed);
			cam_transform.position = target.transform.position;
			if (lookatpos != Vector3.zero)
			{
				cam_transform.LookAt(lookatpos);
			}
			yield return null;
		}
		Object.Destroy(target);
		if (timeout != 0f)
		{
			yield return new WaitForSeconds(timeout);
		}
		update_angle = true;
		update_position = true;
	}

	private IEnumerator MoveTo2(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float moveTime, Vector3 translate, float timeout)
	{
		update_angle = false;
		update_position = false;
		Vector3 lookatpos = default(Vector3);
		GameObject target = new GameObject("MoveToTarget");
		switch (lookatname)
		{
		case "center":
		case "0":
			lookatpos = target_look.position;
			break;
		case "player":
			lookatpos = playertransform.position;
			break;
		case "playerup":
			lookatpos = playertransform.position + new Vector3(0f, 1f, 0f);
			break;
		case "enemy":
			lookatpos = enemytransform.position;
			break;
		case "auto":
			update_angle = true;
			break;
		}
		Transform bodypart = player_transform.FindChildByName(targetname);
		Vector3 targetpos = default(Vector3);
		if (targetname == "0" && (bool)bodypart)
		{
			targetpos = bodypart.transform.position;
		}
		else
		{
			switch (targetname)
			{
			case "center":
				targetpos = target_look.position;
				break;
			case "player":
				targetpos = playertransform.position;
				break;
			case "enemy":
				targetpos = enemytransform.position;
				break;
			}
		}
		target.transform.position = targetpos;
		target.transform.LookAt(lookatpos);
		target.transform.Translate(translate);
		targetpos = target.transform.position;
		target.transform.position = cam_transform.position;
		Vector3 startPos = cam_transform.position;
		float ttime = moveTime;
		while (ttime > 0f)
		{
			float dist = Vector3.Distance(cam_transform.position, targetpos);
			if ((double)dist < 0.1)
			{
				break;
			}
			float speed = dist / ttime;
			double k = 1.0 - (double)(ttime / moveTime);
			target.transform.LookAt(targetpos);
			target.transform.Translate(0f, 0f, Time.deltaTime * speed);
			ttime -= Time.deltaTime;
			cam_transform.position = target.transform.position;
			if (lookatpos != Vector3.zero)
			{
				cam_transform.LookAt(lookatpos);
			}
			yield return null;
		}
		Object.Destroy(target);
		if (timeout != 0f)
		{
			yield return new WaitForSeconds(timeout);
		}
		update_angle = true;
		update_position = true;
	}

	private void EndMoveTo()
	{
		wait_move_to = false;
		_seted = true;
	}

	private void MoveToArray()
	{
		Transform moveToArg = MoveToArg1;
		Transform moveToArg2 = MoveToArg2;
		MoveToArg1 = null;
		MoveToArg2 = null;
		MoveTo(moveToArg, moveToArg2, MoveToArg3, MoveToArg4, MoveToArg5, MoveToArg6, MoveToArg7);
	}

	private IEnumerator MoveTo(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float moveTime, Vector3 translate, float timeout)
	{
		update_angle = false;
		update_position = false;
		Vector3 lookatpos = default(Vector3);
		Vector3 startLookAtPos = lookatpos;
		GameObject target = new GameObject("MoveToTarget");
		Transform lookAtTransform = null;
		switch (lookatname)
		{
		case "center":
		case "0":
			lookatpos = target_look.position;
			lookAtTransform = target_look;
			break;
		case "player":
			lookatpos = playertransform.position;
			lookAtTransform = playertransform;
			break;
		case "playerup":
			lookatpos = playertransform.position + new Vector3(0f, 1f, 0f);
			lookAtTransform = playertransform;
			break;
		case "enemy":
			lookatpos = enemytransform.position;
			lookAtTransform = enemytransform;
			break;
		case "auto":
			update_angle = true;
			break;
		}
		Transform bodypart = player_transform.FindChildByName(targetname);
		Vector3 targetpos = default(Vector3);
		if (targetname == "0" && (bool)bodypart)
		{
			targetpos = bodypart.transform.position;
		}
		else
		{
			switch (targetname)
			{
			case "center":
				targetpos = target_look.position;
				break;
			case "player":
				targetpos = playertransform.position;
				break;
			case "enemy":
				targetpos = enemytransform.position;
				break;
			}
		}
		target.transform.position = targetpos;
		target.transform.LookAt(lookatpos);
		target.transform.Translate(translate);
		targetpos = target.transform.position;
		target.transform.position = cam_transform.position;
		float ttime = 0f;
		float timeStart = Time.time;
		float vsign = -1f;
		Vector3 startPos = cam_transform.position;
		ttime = moveTime;
		while (ttime > 0f)
		{
			float dist = Vector3.Distance(cam_transform.position, targetpos);
			if ((double)dist < 0.1)
			{
				break;
			}
			float speed = dist / ttime;
			double k = 1.0 - (double)(ttime / moveTime);
			target.transform.LookAt(targetpos);
			target.transform.Translate(0f, 0f, Time.deltaTime * speed);
			ttime -= Time.deltaTime;
			cam_transform.position = target.transform.position;
			if (lookatpos != Vector3.zero)
			{
				cam_transform.LookAt(lookatpos);
			}
			yield return null;
		}
		Object.Destroy(target);
		if (timeout > 0f)
		{
			while (timeout > 0f)
			{
				if (in_vibration)
				{
					lookatpos = Vibrate(startLookAtPos);
					if (lookatpos != Vector3.zero)
					{
						cam_transform.LookAt(lookatpos);
					}
				}
				timeout -= Time.deltaTime;
				yield return null;
			}
		}
		if (timeout < 0f)
		{
			wait_move_to = true;
			while (wait_move_to)
			{
				if (in_vibration)
				{
					lookatpos = Vibrate(startLookAtPos);
					if (lookatpos != Vector3.zero)
					{
						cam_transform.LookAt(lookatpos);
					}
				}
				yield return null;
			}
		}
		update_angle = true;
		update_position = true;
	}

	private Vector3 Vibrate(Vector3 initPos)
	{
		float num = (Time.time - vibration_start_time) / vibration_timeout;
		if (num > 1f)
		{
			num = 1f;
		}
		float num2 = Mathf.Lerp(0f, (!(vibration_sign > 0f)) ? vibration_back_value : vibration, (!(vsign > 0f)) ? (1f - num) : num);
		if ((double)num == 1.0)
		{
			vsign = 0f - vsign;
		}
		return new Vector3(initPos.x, initPos.y + num2, initPos.z);
	}

	private IEnumerator Slowmo2(float timeout, float speed, float intime)
	{
		Time.timeScale = Globals.DefaultTimeScale;
		speed = 1f / speed;
		float inStepsCount = intime / 0.05f;
		float inScaleStep = (1f - speed) / inStepsCount;
		for (float inStepsCountCurrent = inStepsCount; inStepsCountCurrent > 0f; inStepsCountCurrent -= 1f)
		{
			Time.timeScale -= inScaleStep;
			yield return new WaitForSeconds(0.05f);
		}
		Time.timeScale = speed;
		if (timeout >= intime)
		{
			yield return new WaitForSeconds(timeout - intime);
		}
		for (float inStepsCountCurrent = inStepsCount; inStepsCountCurrent > 0f; inStepsCountCurrent -= 1f)
		{
			Time.timeScale += inScaleStep;
			yield return new WaitForSeconds(0.05f);
		}
		Time.timeScale = Globals.DefaultTimeScale;
	}

	private IEnumerator Slowmo(float timeout)
	{
		while (Time.timeScale > 0.2f)
		{
			Time.timeScale -= 0.1f;
			yield return new WaitForSeconds(0.05f);
		}
		Time.timeScale = 0.2f;
		if (timeout >= 0.8f)
		{
			yield return new WaitForSeconds(timeout - 0.8f);
		}
		while (Time.timeScale < 1f)
		{
			Time.timeScale += 0.1f;
			yield return new WaitForSeconds(0.05f);
		}
		Time.timeScale = Globals.DefaultTimeScale;
	}

	private float RealDeltaTime()
	{
		return Time.deltaTime / Time.timeScale;
	}

	private bool IsScenariosPlaying()
	{
		return false;
	}

	private Vector3 AddY(Vector3 t, float y)
	{
		return new Vector3(t.x, t.y + y, t.z);
	}
}
