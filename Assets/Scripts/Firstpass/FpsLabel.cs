using System;
using UnityEngine;

public class FpsLabel : MonoBehaviour
{
	private SpriteText _text;

	private float _updateInterval = 1f;

	private float _accum;

	private float _frames;

	private float _timeLeft;

	private float _fps;

	private float _lastSample;

	private float _gotIntervals;

	private void Start()
	{
		_text = GetComponent<SpriteText>();
		_timeLeft = _updateInterval;
		_lastSample = Time.realtimeSinceStartup;
		base.transform.localPosition = new Vector3(-Camera2D.ScreenWidth / 2, Camera2D.ScreenHeight / 2, 50f);
	}

	private void Update()
	{
		if (Globals.BuildType == Globals.BuildTypeE.InnerRelease && !Globals.ForceFPS)
		{
			if (_text.Text != string.Empty)
			{
				_text.Text_ = string.Empty;
			}
			return;
		}
		_frames += 1f;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = realtimeSinceStartup - _lastSample;
		_lastSample = realtimeSinceStartup;
		_timeLeft -= num;
		_accum += 1f / num;
		if ((double)_timeLeft <= 0.0)
		{
			_fps = _accum / _frames;
			string text = _fps.ToString("f2");
			if (Globals.IsDebugGC)
			{
				text += " \nC#HEAP:{0} MONO:{1}/{2} ".Fmt((int)(GC.GetTotalMemory(forceFullCollection: true) / 1048576), UnityApi.GetMonoUsedHeap(), UnityApi.GetMonoHeap());
			}
			_text.Text_ = text;
			_timeLeft = _updateInterval;
			_accum = 0f;
			_frames = 0f;
			_gotIntervals += 1f;
		}
	}
}
