using System;
using UnityEngine;

internal class TrailEffect
{
	private TrailRenderer _fxTrail;

	private readonly Func<GameObject> _fxPrototype;

	private bool _enabled;

	internal bool Started => _fxTrail != null;

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			if (_fxTrail != null && !value)
			{
				Utils.DestroyGameObject(ref _fxTrail);
			}
		}
	}

	internal TrailEffect(Func<GameObject> fxPrototype)
	{
		_fxPrototype = fxPrototype;
	}

	internal void Start(Vector3 wp)
	{
		if (_enabled)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(_fxPrototype(), wp, Quaternion.identity);
			_fxTrail = gameObject.renderer as TrailRenderer;
		}
	}

	internal void Move(Vector3 wp)
	{
		if (_enabled && _fxTrail != null)
		{
			_fxTrail.transform.position = wp;
		}
	}

	internal void Destroy()
	{
		Utils.DestroyGameObject(ref _fxTrail);
	}
}
