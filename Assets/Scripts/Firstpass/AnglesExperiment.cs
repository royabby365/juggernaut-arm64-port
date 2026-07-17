using UnityEngine;

public class AnglesExperiment : MonoBehaviour
{
	private Vector3 _p1;

	private void OnGUI()
	{
		float f = _p1.y / _p1.x;
		float angle = ((_p1.x == 0f) ? 0f : (Mathf.Atan(f) * 57.29578f));
		angle = ConvertAtanAngleToEuler(angle, _p1);
		GUI.Label(new Rect(0f, 0f, 100f, 100f), angle.ToString());
	}

	private float ConvertAtanAngleToEuler(float angle, Vector3 pos)
	{
		angle = ((!(pos.x >= 0f)) ? (angle + 90f) : (angle + 270f));
		return angle;
	}

	private void Update()
	{
		_p1 = new Vector3(Mathf.Cos(Time.realtimeSinceStartup) * 100f, Mathf.Sin(Time.realtimeSinceStartup) * 100f);
		Debug.DrawLine(default(Vector3), _p1);
	}
}
