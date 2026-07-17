using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boo.Lang;
using UnityEngine;

[Serializable]
[AddComponentMenu("Parameters/Scene")]
public class scene_parameters : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024MainLightOn_002441 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal float _0024_002417_002442;

			internal Color _0024_002418_002443;

			internal float _0024_002419_002444;

			internal Color _0024_002420_002445;

			internal float _0024_002421_002446;

			internal Color _0024_002422_002447;

			internal float _0024speed_002448;

			internal float _0024wait_002449;

			internal scene_parameters _0024self__002450;

			public _0024(float speed, float wait, scene_parameters self_)
			{
				_0024speed_002448 = speed;
				_0024wait_002449 = wait;
				_0024self__002450 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					funcs.PrintTemp("MainLightOn " + _0024speed_002448 + " " + _0024wait_002449 + " " + _0024self__002450.mainligh_on);
					if (!(_0024wait_002449 <= 0f))
					{
						result = (Yield(2, new WaitForSeconds(_0024wait_002449)) ? 1 : 0);
						break;
					}
					goto case 2;
				case 2:
					if (!_0024self__002450.mainlight)
					{
						goto case 1;
					}
					_0024self__002450.mainligh_on = true;
					funcs.PrintTemp("  MainLightOn START " + _0024speed_002448 + " " + _0024wait_002449 + " ");
					goto case 3;
				case 3:
					if (!(_0024self__002450.mainlight.intensity >= _0024self__002450.mainlight_intensity) && _0024self__002450.mainligh_on)
					{
						_0024self__002450.mainlight.intensity = Mathf.Lerp(_0024self__002450.mainlight.intensity, _0024self__002450.mainlight_intensity, Time.deltaTime * _0024speed_002448);
						float num = (_0024_002417_002442 = Mathf.Lerp(RenderSettings.ambientLight.r, _0024self__002450.ambient.r, Time.deltaTime * _0024speed_002448));
						Color color = (_0024_002418_002443 = RenderSettings.ambientLight);
						float num2 = (_0024_002418_002443.r = _0024_002417_002442);
						Color color2 = (RenderSettings.ambientLight = _0024_002418_002443);
						float num3 = (_0024_002419_002444 = Mathf.Lerp(RenderSettings.ambientLight.g, _0024self__002450.ambient.g, Time.deltaTime * _0024speed_002448));
						Color color4 = (_0024_002420_002445 = RenderSettings.ambientLight);
						float num4 = (_0024_002420_002445.g = _0024_002419_002444);
						Color color5 = (RenderSettings.ambientLight = _0024_002420_002445);
						float num5 = (_0024_002421_002446 = Mathf.Lerp(RenderSettings.ambientLight.b, _0024self__002450.ambient.b, Time.deltaTime * _0024speed_002448));
						Color color7 = (_0024_002422_002447 = RenderSettings.ambientLight);
						float num6 = (_0024_002422_002447.b = _0024_002421_002446);
						Color color8 = (RenderSettings.ambientLight = _0024_002422_002447);
						result = (YieldDefault(3) ? 1 : 0);
						break;
					}
					funcs.PrintTemp("  MainLightOn END " + _0024speed_002448 + " " + _0024wait_002449 + " ");
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal float _0024speed_002451;

		internal float _0024wait_002452;

		internal scene_parameters _0024self__002453;

		public _0024MainLightOn_002441(float speed, float wait, scene_parameters self_)
		{
			_0024speed_002451 = speed;
			_0024wait_002452 = wait;
			_0024self__002453 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024speed_002451, _0024wait_002452, _0024self__002453);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024MainLightOff_002454 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal float _0024_002423_002455;

			internal Color _0024_002424_002456;

			internal float _0024_002425_002457;

			internal Color _0024_002426_002458;

			internal float _0024_002427_002459;

			internal Color _0024_002428_002460;

			internal float _0024speed_002461;

			internal float _0024wait_002462;

			internal scene_parameters _0024self__002463;

			public _0024(float speed, float wait, scene_parameters self_)
			{
				_0024speed_002461 = speed;
				_0024wait_002462 = wait;
				_0024self__002463 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					funcs.PrintTemp("MainLightOff " + _0024speed_002461 + " " + _0024wait_002462 + " " + _0024self__002463.mainligh_on);
					if (!(_0024wait_002462 <= 0f))
					{
						result = (Yield(2, new WaitForSeconds(_0024wait_002462)) ? 1 : 0);
						break;
					}
					goto case 2;
				case 2:
					if (!_0024self__002463.mainlight)
					{
						goto case 1;
					}
					_0024self__002463.mainligh_on = false;
					funcs.PrintTemp("  MainLightOff START " + _0024speed_002461 + " " + _0024wait_002462 + " ");
					goto case 3;
				case 3:
					if (!(_0024self__002463.mainlight.intensity <= 0f) && !_0024self__002463.mainligh_on)
					{
						_0024self__002463.mainlight.intensity = Mathf.Lerp(_0024self__002463.mainlight.intensity, 0f, Time.deltaTime * _0024speed_002461);
						float num = (_0024_002423_002455 = Mathf.Lerp(RenderSettings.ambientLight.r, _0024self__002463.ambient.r / 3f, Time.deltaTime * _0024speed_002461));
						Color color = (_0024_002424_002456 = RenderSettings.ambientLight);
						float num2 = (_0024_002424_002456.r = _0024_002423_002455);
						Color color2 = (RenderSettings.ambientLight = _0024_002424_002456);
						float num3 = (_0024_002425_002457 = Mathf.Lerp(RenderSettings.ambientLight.g, _0024self__002463.ambient.g / 3f, Time.deltaTime * _0024speed_002461));
						Color color4 = (_0024_002426_002458 = RenderSettings.ambientLight);
						float num4 = (_0024_002426_002458.g = _0024_002425_002457);
						Color color5 = (RenderSettings.ambientLight = _0024_002426_002458);
						float num5 = (_0024_002427_002459 = Mathf.Lerp(RenderSettings.ambientLight.b, _0024self__002463.ambient.b / 3f, Time.deltaTime * _0024speed_002461));
						Color color7 = (_0024_002428_002460 = RenderSettings.ambientLight);
						float num6 = (_0024_002428_002460.b = _0024_002427_002459);
						Color color8 = (RenderSettings.ambientLight = _0024_002428_002460);
						result = (YieldDefault(3) ? 1 : 0);
						break;
					}
					funcs.PrintTemp("  MainLightOff END " + _0024speed_002461 + " " + _0024wait_002462 + " ");
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal float _0024speed_002464;

		internal float _0024wait_002465;

		internal scene_parameters _0024self__002466;

		public _0024MainLightOff_002454(float speed, float wait, scene_parameters self_)
		{
			_0024speed_002464 = speed;
			_0024wait_002465 = wait;
			_0024self__002466 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024speed_002464, _0024wait_002465, _0024self__002466);
		}
	}

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
		ambient = new Color(0.5f, 0.5f, 0.5f);
		fogColor = new Color(0.5f, 0.5f, 0.5f);
		fogDensity = 0.1f;
		botsColor = new Color(0.5f, 0.5f, 0.5f);
		mainligh_on = true;
		glowIntensity = 1.05f;
	}

	public virtual void Start()
	{
		RenderSettings.ambientLight = ambient;
		RenderSettings.fog = fog;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogDensity = fogDensity;
		if ((bool)mainlight)
		{
			mainlight_intensity = mainlight.intensity;
		}
		UpdateGlowIntensity();
	}

	public virtual void UpdateGlowIntensity()
	{
		Camera main = Camera.main;
		if ((bool)main)
		{
		}
	}

	public virtual IEnumerator MainLightOn(float speed, float wait)
	{
		return new _0024MainLightOn_002441(speed, wait, this).GetEnumerator();
	}

	public virtual IEnumerator MainLightOff(float speed, float wait)
	{
		return new _0024MainLightOff_002454(speed, wait, this).GetEnumerator();
	}

	public virtual void Main()
	{
	}
}
