using UnityEngine;

public class Suicidal : MonoBehaviour
{
	private float _startTime;

	public float SuicideTime;

	private void Start()
	{
		_startTime = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		if (!(SuicideTime <= 0f) && Time.realtimeSinceStartup - _startTime >= SuicideTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
