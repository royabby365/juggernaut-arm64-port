using UnityEngine;

public class MagicSuccess : MonoBehaviour
{
	public Transform ice;

	public Transform dark;

	public Transform fire;

	public Transform lightning;

	public AnimationCurve alphaCurve;

	public AnimationCurve scaleCurve;

	public float howLong = 3f;

	private Transform _active;

	private float _start;

	private float _end;

	private void Awake()
	{
		ice.gameObject.SetActive(false);
		dark.gameObject.SetActive(false);
		fire.gameObject.SetActive(false);
		lightning.gameObject.SetActive(false);
	}

	private void Start()
	{
		ice.gameObject.SetActive(false);
		dark.gameObject.SetActive(false);
		fire.gameObject.SetActive(false);
		lightning.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (!(_active == null))
		{
			if (Time.time >= _end)
			{
				_active.gameObject.SetActive(false);
				_active = null;
				return;
			}
			float time = (Time.time - _start) / howLong;
			float num = alphaCurve.Evaluate(time);
			_active.GetMesh().SetTint(new Color(1f, 1f, 1f, (!(num > 1f)) ? num : 1f));
			float num2 = scaleCurve.Evaluate(time);
			base.transform.localScale = new Vector3(num2, num2, 1f);
		}
	}

	public void ShowMagicSuccess(string school)
	{
		if (school == Globals.MagicDarkness)
		{
			ShowSchool(dark);
		}
		else if (school == Globals.MagicElectro)
		{
			ShowSchool(lightning);
		}
		else if (school == Globals.MagicFire)
		{
			ShowSchool(fire);
		}
		else if (school == Globals.MagicIce)
		{
			ShowSchool(ice);
		}
		else if (Globals.IsDebugBuild)
		{
			Debug.LogError("wrong magic school: " + school);
		}
	}

	private void ShowSchool(Transform school)
	{
		if (!(_active != null))
		{
			school.gameObject.SetActive(true);
			_active = school;
			_start = Time.time;
			_end = _start + howLong;
		}
	}
}
