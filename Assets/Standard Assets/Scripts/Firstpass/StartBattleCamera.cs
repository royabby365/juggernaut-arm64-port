using UnityEngine;

public class StartBattleCamera : MonoBehaviour
{
	private GameObject _camera;

	private Vector3 _target;

	private float _radius;

	private float _height;

	private float _dir;

	private Vector3 _speed;

	private void Start()
	{
		_camera = GameObject.Find(Globals.LocationGameObjectBattleCamera);
		GameObject gameObject = GameObject.Find("arena_center");
		_target = gameObject != null ? gameObject.transform.position : Vector3.zero;
		_dir = ((Random.value > 0.5f) ? 1 : (-1));
	}

	private void Update()
	{
		if (_camera == null) return;
		Quaternion quaternion = Quaternion.Euler(_speed * Time.time * _dir);
		_camera.transform.rotation = quaternion;
		_camera.transform.position = quaternion * (_target + new Vector3(0f, _height, 0f - _radius));
		_camera.transform.LookAt(_target);
	}

	public void SetParams(float radius, float height, float speed)
	{
		_radius = radius;
		_height = height;
		_speed = new Vector3(0f, speed, 0f);
	}
}
