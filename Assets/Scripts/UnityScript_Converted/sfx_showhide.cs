using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boo.Lang;
using UnityEngine;

[Serializable]
public class sfx_showhide : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Start_002470 : GenericGenerator<YieldInstruction>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<YieldInstruction>, IEnumerator
		{
			internal sfx_showhide _0024self__002471;

			public _0024(sfx_showhide self_)
			{
				_0024self__002471 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024self__002471.emitter = (ParticleSystem)_0024self__002471.GetComponent<ParticleSystem>();
					result = (Yield(2, new WaitForSeconds(0.05f)) ? 1 : 0);
					break;
				case 2:
					result = (Yield(3, _0024self__002471.StartCoroutine_Auto(_0024self__002471.Show())) ? 1 : 0);
					break;
				case 3:
					result = (Yield(4, new WaitForSeconds(_0024self__002471.autodestroy_time)) ? 1 : 0);
					break;
				case 4:
					result = (Yield(5, _0024self__002471.StartCoroutine_Auto(_0024self__002471.Destroy())) ? 1 : 0);
					break;
				case 5:
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal sfx_showhide _0024self__002472;

		public _0024Start_002470(sfx_showhide self_)
		{
			_0024self__002472 = self_;
		}

		public override IEnumerator<YieldInstruction> GetEnumerator()
		{
			return new _0024(_0024self__002472);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Show_002473 : GenericGenerator<object>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<object>, IEnumerator
		{
			internal float _0024default_minEmission_002474;

			internal float _0024default_maxEmission_002475;

			internal string _0024colortype_002476;

			internal float _0024default_alpha_002477;

			internal Color _0024mat_color_002478;

			internal float _0024default_intensity_002479;

			internal sfx_showhide _0024self__002480;

			public _0024(sfx_showhide self_)
			{
				_0024self__002480 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if ((bool)_0024self__002480.emitter)
					{
						_0024default_minEmission_002474 = _0024self__002480.emitter.emission.rateOverTimeMultiplier;
						_0024default_maxEmission_002475 = _0024self__002480.emitter.emission.rateOverTimeMultiplier;
						{
							var em = _0024self__002480.emitter.emission;
							em.rateOverTimeMultiplier = 0f;
						}
						{
							var em = _0024self__002480.emitter.emission;
							em.rateOverTimeMultiplier = 0f;
						}
						{
							var em = _0024self__002480.emitter.emission;
							em.enabled = true;
						}
						goto case 2;
					}
					if ((bool)_0024self__002480.GetComponent<MeshRenderer>() && typeof(MeshRenderer) != null && (bool)_0024self__002480.GetComponent<Renderer>().material)
					{
						_0024colortype_002476 = _0024self__002480.GetColorType(_0024self__002480.GetComponent<Renderer>().material);
						if (!string.IsNullOrEmpty(_0024colortype_002476))
						{
							_0024default_alpha_002477 = _0024self__002480.GetComponent<Renderer>().material.GetColor(_0024colortype_002476).a;
							_0024mat_color_002478 = _0024self__002480.GetComponent<Renderer>().material.GetColor(_0024colortype_002476);
							_0024mat_color_002478.a = 0f;
							_0024self__002480.GetComponent<Renderer>().material.SetColor(_0024colortype_002476, _0024mat_color_002478);
							_0024self__002480.GetComponent<Renderer>().enabled = true;
							goto case 3;
						}
					}
					goto IL_030d;
				case 2:
					if (_0024self__002480.emitter.emission.rateOverTimeMultiplier < _0024default_maxEmission_002475)
					{
						if (!(_0024self__002480.emitter.emission.rateOverTimeMultiplier >= _0024default_minEmission_002474))
						{
							{
								var em = _0024self__002480.emitter.emission;
								em.rateOverTimeMultiplier = _0024self__002480.emitter.emission.rateOverTimeMultiplier + 1f * Time.deltaTime * _0024self__002480.dtime;
							}
						}
						{
							var em = _0024self__002480.emitter.emission;
							em.rateOverTimeMultiplier = _0024self__002480.emitter.emission.rateOverTimeMultiplier + 15f * Time.deltaTime * _0024self__002480.dtime;
						}
						result = (YieldDefault(2) ? 1 : 0);
						break;
					}
					{
						var em = _0024self__002480.emitter.emission;
						em.rateOverTimeMultiplier = _0024default_maxEmission_002475;
					}
					{
						var em = _0024self__002480.emitter.emission;
						em.rateOverTimeMultiplier = _0024default_minEmission_002474;
					}
					goto IL_030d;
				case 3:
					if (_0024self__002480.GetComponent<Renderer>().material.GetColor(_0024colortype_002476).a < _0024default_alpha_002477)
					{
						_0024mat_color_002478.a += _0024self__002480.show_speed / 100f * Time.deltaTime * _0024self__002480.dtime;
						_0024self__002480.GetComponent<Renderer>().material.SetColor(_0024colortype_002476, _0024mat_color_002478);
						result = (YieldDefault(3) ? 1 : 0);
						break;
					}
					goto IL_030d;
				case 4:
					if (_0024self__002480.GetComponent<Light>().intensity < _0024default_intensity_002479)
					{
						_0024self__002480.GetComponent<Light>().intensity = _0024self__002480.GetComponent<Light>().intensity + _0024self__002480.show_speed / 20f * Time.deltaTime * _0024self__002480.dtime;
						result = (YieldDefault(4) ? 1 : 0);
						break;
					}
					goto IL_03dd;
				case 1:
					{
						result = 0;
						break;
					}
					IL_030d:
					if ((bool)_0024self__002480.GetComponent<Light>() && typeof(Light) != null)
					{
						_0024default_intensity_002479 = _0024self__002480.GetComponent<Light>().intensity;
						_0024self__002480.GetComponent<Light>().intensity = 0f;
						_0024self__002480.GetComponent<Light>().enabled = true;
						goto case 4;
					}
					goto IL_03dd;
					IL_03dd:
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal sfx_showhide _0024self__002481;

		public _0024Show_002473(sfx_showhide self_)
		{
			_0024self__002481 = self_;
		}

		public override IEnumerator<object> GetEnumerator()
		{
			return new _0024(_0024self__002481);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Destroy_002482 : GenericGenerator<object>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<object>, IEnumerator
		{
			internal string _0024colortype_002483;

			internal Color _0024mat_color_002484;

			internal sfx_showhide _0024self__002485;

			public _0024(sfx_showhide self_)
			{
				_0024self__002485 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if ((bool)_0024self__002485.emitter)
					{
						{
							var em = _0024self__002485.emitter.emission;
							em.enabled = false;
						}
						goto case 2;
					}
					if ((bool)_0024self__002485.GetComponent<MeshRenderer>() && typeof(MeshRenderer) != null && (bool)_0024self__002485.GetComponent<Renderer>().material)
					{
						_0024colortype_002483 = _0024self__002485.GetColorType(_0024self__002485.GetComponent<Renderer>().material);
						if (!string.IsNullOrEmpty(_0024colortype_002483))
						{
							_0024mat_color_002484 = _0024self__002485.GetComponent<Renderer>().material.GetColor(_0024colortype_002483);
							goto case 3;
						}
					}
					goto IL_019a;
				case 2:
					if (_0024self__002485.emitter.particleCount > 0)
					{
						result = (YieldDefault(2) ? 1 : 0);
						break;
					}
					goto IL_019a;
				case 3:
					if (_0024self__002485.GetComponent<Renderer>().material.GetColor(_0024colortype_002483).a > 0f)
					{
						_0024mat_color_002484.a -= _0024self__002485.hide_speed / 100f * Time.deltaTime * _0024self__002485.dtime;
						_0024self__002485.GetComponent<Renderer>().material.SetColor(_0024colortype_002483, _0024mat_color_002484);
						result = (YieldDefault(3) ? 1 : 0);
						break;
					}
					goto IL_019a;
				case 4:
					if (_0024self__002485.GetComponent<Light>().intensity > 0f)
					{
						_0024self__002485.GetComponent<Light>().intensity = _0024self__002485.GetComponent<Light>().intensity - _0024self__002485.hide_speed / 20f * Time.deltaTime * _0024self__002485.dtime;
						result = (YieldDefault(4) ? 1 : 0);
						break;
					}
					goto IL_022d;
				case 1:
					{
						result = 0;
						break;
					}
					IL_019a:
					if ((bool)_0024self__002485.GetComponent<Light>() && typeof(Light) != null)
					{
						goto case 4;
					}
					goto IL_022d;
					IL_022d:
					UnityEngine.Object.Destroy(_0024self__002485.gameObject);
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal sfx_showhide _0024self__002486;

		public _0024Destroy_002482(sfx_showhide self_)
		{
			_0024self__002486 = self_;
		}

		public override IEnumerator<object> GetEnumerator()
		{
			return new _0024(_0024self__002486);
		}
	}

	public float show_speed;

	public float hide_speed;

	public float autodestroy_time;

	private ParticleSystem emitter;

	private float dtime;

	public sfx_showhide()
	{
		show_speed = 1f;
		hide_speed = 1f;
		autodestroy_time = 1f;
		dtime = 40f;
	}

	public virtual void Awake()
	{
		if ((bool)(ParticleSystem)GetComponent<ParticleSystem>())
		{
			{ var em = ((ParticleSystem)GetComponent<ParticleSystem>()).emission; em.enabled = false; }
		}
		if ((bool)GetComponent<MeshRenderer>())
		{
			GetComponent<Renderer>().enabled = false;
		}
		if ((bool)GetComponent<Light>())
		{
			GetComponent<Light>().enabled = false;
		}
	}

	public virtual IEnumerator Start()
	{
		return new _0024Start_002470(this).GetEnumerator();
	}

	public virtual IEnumerator Show()
	{
		return new _0024Show_002473(this).GetEnumerator();
	}

	public virtual IEnumerator Destroy()
	{
		return new _0024Destroy_002482(this).GetEnumerator();
	}

	public virtual string GetColorType(Material mat)
	{
		string text = null;
		string result;
		if ((bool)mat)
		{
			if (mat.shader.name == "Transparent/Diffuse")
			{
				text = "_Color";
			}
			if (mat.shader.name == "Particles/Additive" || mat.shader.name == "Particles/Alpha Blended")
			{
				text = "_TintColor";
			}
			if (!string.IsNullOrEmpty(text) && mat.HasProperty(text))
			{
				result = text;
				goto IL_0085;
			}
		}
		result = text;
		goto IL_0085;
		IL_0085:
		return result;
	}

	public virtual void Main()
	{
	}
}
