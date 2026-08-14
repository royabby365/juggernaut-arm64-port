using System;
using System.Collections;
using UnityEngine;

[Serializable]
[AddComponentMenu("Parameters/Scene")]
public class scene_parameters : MonoBehaviour
{
	public Color ambient;
	public bool fog;
	public Color fogColor;
	public float fogDensity;
	public Color botsColor;
	public Light mainlight;
	private float mainlight_intensity;
	private bool mainligh_on;
	public float glowIntensity;

	public scene_parameters()
	{
	}

	public virtual void Start()
	{
		ambient = RenderSettings.ambientLight;
		fog = RenderSettings.fog;
		fogColor = RenderSettings.fogColor;
		fogDensity = RenderSettings.fogDensity;
		mainlight_intensity = ((mainlight != null) ? mainlight.intensity : 0f);
		mainligh_on = (mainlight_intensity > 0f);
	}

	public virtual void UpdateGlowIntensity()
	{
	}

	public virtual IEnumerator MainLightOn(float speed, float wait)
	{
		yield return new WaitForSeconds(wait);
		if (mainlight != null)
		{
			mainlight.intensity = mainlight_intensity;
		}
		mainligh_on = true;
	}

	public virtual IEnumerator MainLightOff(float speed, float wait)
	{
		yield return new WaitForSeconds(wait);
		if (mainlight != null)
		{
			mainlight.intensity = 0f;
		}
		mainligh_on = false;
	}

	public virtual void Main()
	{
	}
}
