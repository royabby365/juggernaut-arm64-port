using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TouchscreenCasts
{
	private ITouchscreen _touchscreen;

	private List<Cast> _casts;

	public bool Enabled { get; set; }

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<string> Casted;

	public TouchscreenCasts(Battle battle, ITouchscreen touchscreen, Func<GameObject> fx)
	{
		_casts = new List<Cast>
		{
			new LinesCast(Globals.MagicDarkness, fx, battle, (LinesCast.FollowTheDirection a, LinesCast.FollowTheDirection b) => Mathf.Abs(a.StartPosition.y - b.LastValidPosition.y) <= (float)Globals.CastDarknessEndYDeviation, new LinesCast.FollowTheDirection("darkness.up", new Vector2(0f, 1f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: false), new LinesCast.FollowTheDirection("darkness.right", new Vector2(1f, 0f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: false), new LinesCast.FollowTheDirection("darkness.down", new Vector2(0f, -1f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: false)),
			new LinesCast(Globals.MagicIce, fx, battle, (LinesCast.FollowTheDirection a, LinesCast.FollowTheDirection b) => true, new LinesCast.FollowTheDirection("ice.up-side", new Vector2(0.5f, 0.866f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: false), new LinesCast.FollowTheDirection("ice.down-side", new Vector2(0.5f, -0.866f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: false)),
			new LinesCast(Globals.MagicFire, fx, battle, (LinesCast.FollowTheDirection a, LinesCast.FollowTheDirection b) => true, new LinesCast.FollowTheDirection("fire.up", new Vector2(0f, 1f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: true), new LinesCast.FollowTheDirection("fire.right", new Vector2(1f, 0f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: true)),
			new LinesCast(Globals.MagicElectro, fx, battle, (LinesCast.FollowTheDirection a, LinesCast.FollowTheDirection b) => true, new LinesCast.FollowTheDirection("electro.down", new Vector2(0f, -1f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: true), new LinesCast.FollowTheDirection("electro.right", new Vector2(1f, 0f), Globals.CastAngleDeviation, Globals.CastPixelDeviation, checkAngle: true))
		};
		_touchscreen = touchscreen;
		_touchscreen.OnTouchStart += _touchscreen_OnTouchStart;
		_touchscreen.OnTouchMove += _touchscreen_OnTouchMove;
		_touchscreen.OnTouchEnd += _touchscreen_OnTouchEnd;
		foreach (Cast cast in _casts)
		{
			cast.Casted = (Action<Cast>)Delegate.Combine(cast.Casted, new Action<Cast>(CastedHandler));
		}
	}

	private void CastedHandler(Cast cast)
	{
		if (this.Casted != null)
		{
			this.Casted(cast.Name);
		}
		foreach (Cast cast2 in _casts)
		{
			cast2.Reset();
		}
	}

	private void _touchscreen_OnTouchEnd(Vector2 startPoint, Vector2 point, float time)
	{
		Player player = Globals.Player;
		List<Cast> casts = _casts;
		if (!(player != null))
		{
			return;
		}
		for (int i = 0; i < casts.Count; i++)
		{
			if (player.IsCastAllowed(i))
			{
				casts[i].End(point);
			}
			else
			{
				casts[i].Reset();
			}
		}
	}

	private void _touchscreen_OnTouchMove(Vector2 offset, Vector2 pos)
	{
		Player player = Globals.Player;
		List<Cast> casts = _casts;
		if (!(player != null))
		{
			return;
		}
		for (int i = 0; i < casts.Count; i++)
		{
			if (player.IsCastAllowed(i))
			{
				casts[i].Move(pos);
			}
			else
			{
				casts[i].Reset();
			}
		}
	}

	private void _touchscreen_OnTouchStart(Vector2 point)
	{
		Player player = Globals.Player;
		List<Cast> casts = _casts;
		if (!(player != null))
		{
			return;
		}
		for (int i = 0; i < casts.Count; i++)
		{
			if (player.IsCastAllowed(i))
			{
				casts[i].Start(point);
			}
			else
			{
				casts[i].Reset();
			}
		}
	}

	public void Update()
	{
		if (Enabled)
		{
		}
	}
}
