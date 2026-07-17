using System;
using UnityEngine;

[Serializable]
public class ShowFPS : MonoBehaviour
{
	public float updateInterval;

	private float accum;

	private int frames;

	private float timeleft;

	private float fps;

	private double lastSample;

	private int gotIntervals;

	public ShowFPS()
	{
		updateInterval = 1f;
		fps = 15f;
	}

	public virtual void Start()
	{
		timeleft = updateInterval;
		lastSample = Time.realtimeSinceStartup;
	}

	public virtual float GetFPS()
	{
		return fps;
	}

	public virtual bool HasFPS()
	{
		return gotIntervals > 2;
	}

	public virtual void Update()
	{
		frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		double num = (double)realtimeSinceStartup - lastSample;
		lastSample = realtimeSinceStartup;
		timeleft = (float)((double)timeleft - num);
		accum = (float)((double)accum + 1.0 / num);
		if (!(timeleft > 0f))
		{
			fps = accum / (float)frames;
			guiText.text = fps.ToString("f2");
			timeleft = updateInterval;
			accum = 0f;
			frames = 0;
			gotIntervals++;
		}
	}

	public virtual void Main()
	{
	}
}
