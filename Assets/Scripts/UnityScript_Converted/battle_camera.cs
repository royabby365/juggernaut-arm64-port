using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boo.Lang;
using Boo.Lang.Runtime;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class battle_camera : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Start_002491 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal GameObject _0024newobj_002492;

			internal battle_camera _0024self__002493;

			public _0024(battle_camera self_)
			{
				_0024self__002493 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024self__002493.cam = GameObject.Find("camera");
					_0024self__002493.cam_transform = _0024self__002493.cam.transform;
					_0024newobj_002492 = new GameObject("camera_target_pos");
					_0024self__002493.target_pos = _0024newobj_002492.transform;
					_0024newobj_002492 = new GameObject("camera_target_look");
					_0024self__002493.target_look = _0024newobj_002492.transform;
					_0024self__002493.target_dummy = new GameObject("camera_target_dummy");
					_0024self__002493.arenacenter = GameObject.Find("arena_center");
					_0024self__002493.arenacenter_y = _0024self__002493.arenacenter.transform.position.y + 0.8f;
					_0024self__002493.cam_transform.position = _0024self__002493.arenacenter.transform.position;
					_0024self__002493.cam_transform.rotation = _0024self__002493.arenacenter.transform.rotation;
					result = (Yield(2, new WaitForSeconds(0.01f)) ? 1 : 0);
					break;
				case 2:
					_0024self__002493.cam_transform.Translate(_0024self__002493.start_distance, 2f, 0f);
					_0024self__002493.update_angle = true;
					_0024self__002493.update_position = true;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal battle_camera _0024self__002494;

		public _0024Start_002491(battle_camera self_)
		{
			_0024self__002494 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024self__002494);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024GameOverMode_002495 : GenericGenerator<YieldInstruction>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<YieldInstruction>, IEnumerator
		{
			internal GameObject _0024target_002496;

			internal Vector3 _0024playerup_002497;

			internal Transform _0024targetTransform_002498;

			internal battle_camera _0024self__002499;

			public _0024(battle_camera self_)
			{
				_0024self__002499 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					result = (Yield(2, new WaitForSeconds(1.5f)) ? 1 : 0);
					break;
				case 2:
					_0024target_002496 = new GameObject("MoveToTarget");
					_0024target_002496.transform.position = _0024self__002499.target_look.position;
					_0024self__002499.update_angle = false;
					_0024self__002499.update_position = false;
					_0024playerup_002497 = _0024self__002499.player_transform.position + new Vector3(0f, 1f, 0f);
					_0024targetTransform_002498 = _0024target_002496.transform;
					goto case 3;
				case 3:
					if (Vector3.Distance(_0024targetTransform_002498.position, _0024playerup_002497) > 0.1f)
					{
						_0024targetTransform_002498.LookAt(_0024playerup_002497);
						_0024targetTransform_002498.Translate(0f, 0f, Time.deltaTime * 5f);
						_0024self__002499.cam_transform.LookAt(_0024target_002496.transform);
						_0024self__002499.cam_transform.Translate(0f, 0f, Time.deltaTime * 2f);
						result = (YieldDefault(3) ? 1 : 0);
					}
					else
					{
						result = (Yield(4, _0024self__002499.StartCoroutine_Auto(_0024self__002499.MoveTo2(_0024self__002499.player_transform, _0024self__002499.enemy_transform, "player", "playerup", 1.5f, new Vector3(-3.5f, 0f, 2f), 0f))) ? 1 : 0);
					}
					break;
				case 4:
					_0024self__002499.gameover = true;
					UnityEngine.Object.Destroy(_0024target_002496);
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal battle_camera _0024self__0024100;

		public _0024GameOverMode_002495(battle_camera self_)
		{
			_0024self__0024100 = self_;
		}

		public override IEnumerator<YieldInstruction> GetEnumerator()
		{
			return new _0024(_0024self__0024100);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Vibration_0024101 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal float _0024t_0024102;

			internal int _0024i_0024103;

			internal float _0024power_0024104;

			internal int _0024count_0024105;

			internal float _0024increase_0024106;

			internal float _0024timeout_0024107;

			internal battle_camera _0024self__0024108;

			public _0024(float power, int count, float increase, float timeout, battle_camera self_)
			{
				_0024power_0024104 = power;
				_0024count_0024105 = count;
				_0024increase_0024106 = increase;
				_0024timeout_0024107 = timeout;
				_0024self__0024108 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if (!_0024self__0024108.in_vibration)
					{
						_0024self__0024108.in_vibration = true;
						if (_0024self__0024108.mode != 1)
						{
							_0024self__0024108.vibration_timeout = _0024timeout_0024107;
							_0024t_0024102 = Time.time;
							_0024self__0024108.vibration_start_time = Time.time;
							_0024power_0024104 /= 10f;
							_0024self__0024108.vibration_sign = 1f;
							_0024i_0024103 = 0;
							goto IL_01a7;
						}
					}
					goto case 1;
				case 2:
					_0024self__0024108.vibration += _0024power_0024104;
					_0024self__0024108.vibration_start_time = Time.time;
					result = (Yield(3, new WaitForSeconds(_0024timeout_0024107)) ? 1 : 0);
					break;
				case 3:
					_0024self__0024108.vibration_start_time = Time.time;
					_0024self__0024108.vibration_back_value = _0024self__0024108.vibration;
					_0024self__0024108.vibration_sign = -1f;
					_0024self__0024108.vibration -= _0024power_0024104;
					_0024i_0024103++;
					goto IL_01a7;
				case 1:
					{
						result = 0;
						break;
					}
					IL_01a7:
					if (_0024i_0024103 < _0024count_0024105)
					{
						_0024self__0024108.smooth.x = _0024self__0024108.smooth.y;
						_0024power_0024104 += _0024increase_0024106 / 10f;
						_0024self__0024108.vibration_start_time = Time.time;
						result = (Yield(2, new WaitForSeconds(_0024timeout_0024107)) ? 1 : 0);
						break;
					}
					_0024self__0024108.in_vibration = false;
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal float _0024power_0024109;

		internal int _0024count_0024110;

		internal float _0024increase_0024111;

		internal float _0024timeout_0024112;

		internal battle_camera _0024self__0024113;

		public _0024Vibration_0024101(float power, int count, float increase, float timeout, battle_camera self_)
		{
			_0024power_0024109 = power;
			_0024count_0024110 = count;
			_0024increase_0024111 = increase;
			_0024timeout_0024112 = timeout;
			_0024self__0024113 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024power_0024109, _0024count_0024110, _0024increase_0024111, _0024timeout_0024112, _0024self__0024113);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024SetOnPosition_0024114 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal Vector3 _0024oldPos_0024115;

			internal Quaternion _0024oldRot_0024116;

			internal Transform _0024bodypart_0024117;

			internal string _0024bodypart_name_0024118;

			internal string _0024isparent_0024119;

			internal string _0024lookat_0024120;

			internal Vector3 _0024pos_0024121;

			internal float _0024timeout_0024122;

			internal string _0024slowmo_0024123;

			internal battle_camera _0024self__0024124;

			public _0024(string bodypart_name, string isparent, string lookat, Vector3 pos, float timeout, string slowmo, battle_camera self_)
			{
				_0024bodypart_name_0024118 = bodypart_name;
				_0024isparent_0024119 = isparent;
				_0024lookat_0024120 = lookat;
				_0024pos_0024121 = pos;
				_0024timeout_0024122 = timeout;
				_0024slowmo_0024123 = slowmo;
				_0024self__0024124 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024oldPos_0024115 = _0024self__0024124.cam_transform.position;
					_0024oldRot_0024116 = _0024self__0024124.cam_transform.rotation;
					_0024self__0024124.update_angle = false;
					_0024self__0024124.update_position = false;
					_0024self__0024124.cam_transform.position = _0024self__0024124.player_pos;
					_0024bodypart_0024117 = funcs.FindChildByName(_0024self__0024124.player_transform, _0024bodypart_name_0024118);
					if ((bool)_0024bodypart_0024117)
					{
						_0024self__0024124.cam_transform.position = _0024bodypart_0024117.transform.position;
						if (_0024isparent_0024119 == "1")
						{
							_0024self__0024124.cam_transform.parent = _0024bodypart_0024117.transform;
						}
					}
					_0024self__0024124.cam_transform.Translate(_0024pos_0024121);
					if (_0024lookat_0024120 == "center" || _0024lookat_0024120 == "0")
					{
						_0024self__0024124.cam_transform.LookAt(_0024self__0024124.target_look);
					}
					if ((bool)_0024bodypart_0024117 && _0024lookat_0024120 == "0")
					{
						_0024self__0024124.cam_transform.rotation = _0024bodypart_0024117.transform.rotation;
					}
					if (_0024lookat_0024120 == "enemy")
					{
						_0024self__0024124.cam_transform.LookAt(_0024self__0024124.enemy_pos);
					}
					if (_0024lookat_0024120 == "auto")
					{
						_0024self__0024124.update_angle = true;
					}
					if (_0024slowmo_0024123 == "1")
					{
						_0024self__0024124.StartCoroutine_Auto(_0024self__0024124.Slowmo(_0024timeout_0024122));
					}
					result = (Yield(2, new WaitForSeconds(_0024timeout_0024122)) ? 1 : 0);
					break;
				case 2:
					if (_0024isparent_0024119 == "1")
					{
						_0024self__0024124.cam_transform.parent = null;
					}
					_0024self__0024124.cam_transform.position = _0024oldPos_0024115;
					_0024self__0024124.cam_transform.rotation = _0024oldRot_0024116;
					_0024self__0024124.update_angle = true;
					_0024self__0024124.update_position = true;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal string _0024bodypart_name_0024125;

		internal string _0024isparent_0024126;

		internal string _0024lookat_0024127;

		internal Vector3 _0024pos_0024128;

		internal float _0024timeout_0024129;

		internal string _0024slowmo_0024130;

		internal battle_camera _0024self__0024131;

		public _0024SetOnPosition_0024114(string bodypart_name, string isparent, string lookat, Vector3 pos, float timeout, string slowmo, battle_camera self_)
		{
			_0024bodypart_name_0024125 = bodypart_name;
			_0024isparent_0024126 = isparent;
			_0024lookat_0024127 = lookat;
			_0024pos_0024128 = pos;
			_0024timeout_0024129 = timeout;
			_0024slowmo_0024130 = slowmo;
			_0024self__0024131 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024bodypart_name_0024125, _0024isparent_0024126, _0024lookat_0024127, _0024pos_0024128, _0024timeout_0024129, _0024slowmo_0024130, _0024self__0024131);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024MoveTo3_0024132 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal Vector3 _0024lookatpos_0024133;

			internal GameObject _0024target_0024134;

			internal Transform _0024bodypart_0024135;

			internal Vector3 _0024targetpos_0024136;

			internal Transform _0024playertransform_0024137;

			internal Transform _0024enemytransform_0024138;

			internal string _0024targetname_0024139;

			internal string _0024lookatname_0024140;

			internal float _0024speed_0024141;

			internal Vector3 _0024translate_0024142;

			internal float _0024timeout_0024143;

			internal battle_camera _0024self__0024144;

			public _0024(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float speed, Vector3 translate, float timeout, battle_camera self_)
			{
				_0024playertransform_0024137 = playertransform;
				_0024enemytransform_0024138 = enemytransform;
				_0024targetname_0024139 = targetname;
				_0024lookatname_0024140 = lookatname;
				_0024speed_0024141 = speed;
				_0024translate_0024142 = translate;
				_0024timeout_0024143 = timeout;
				_0024self__0024144 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024self__0024144.update_angle = false;
					_0024self__0024144.update_position = false;
					_0024lookatpos_0024133 = default(Vector3);
					_0024target_0024134 = new GameObject("MoveToTarget");
					if (_0024lookatname_0024140 == "center" || _0024lookatname_0024140 == "0")
					{
						_0024lookatpos_0024133 = _0024self__0024144.target_look.position;
					}
					else if (_0024lookatname_0024140 == "player")
					{
						_0024lookatpos_0024133 = _0024playertransform_0024137.position;
					}
					else if (_0024lookatname_0024140 == "playerup")
					{
						_0024lookatpos_0024133 = _0024playertransform_0024137.position + new Vector3(0f, 1f, 0f);
					}
					else if (_0024lookatname_0024140 == "enemy")
					{
						_0024lookatpos_0024133 = _0024enemytransform_0024138.position;
					}
					else if (_0024lookatname_0024140 == "auto")
					{
						_0024self__0024144.update_angle = true;
					}
					_0024bodypart_0024135 = funcs.FindChildByName(_0024self__0024144.player_transform, _0024targetname_0024139);
					_0024targetpos_0024136 = default(Vector3);
					if (_0024targetname_0024139 == "0" && (bool)_0024bodypart_0024135)
					{
						_0024targetpos_0024136 = _0024bodypart_0024135.transform.position;
					}
					else if (_0024targetname_0024139 == "center")
					{
						_0024targetpos_0024136 = _0024self__0024144.target_look.position;
					}
					else if (_0024targetname_0024139 == "player")
					{
						_0024targetpos_0024136 = _0024playertransform_0024137.position;
					}
					else if (_0024targetname_0024139 == "enemy")
					{
						_0024targetpos_0024136 = _0024enemytransform_0024138.position;
					}
					_0024target_0024134.transform.position = _0024targetpos_0024136;
					_0024target_0024134.transform.LookAt(_0024lookatpos_0024133);
					_0024target_0024134.transform.Translate(_0024translate_0024142);
					_0024targetpos_0024136 = _0024target_0024134.transform.position;
					_0024target_0024134.transform.position = _0024self__0024144.cam_transform.position;
					goto case 2;
				case 2:
					if (Vector3.Distance(_0024self__0024144.cam_transform.position, _0024targetpos_0024136) > 0.1f)
					{
						_0024target_0024134.transform.LookAt(_0024targetpos_0024136);
						_0024target_0024134.transform.Translate(0f, 0f, Time.deltaTime * _0024speed_0024141);
						_0024self__0024144.cam_transform.position = _0024target_0024134.transform.position;
						if (_0024lookatpos_0024133 != Vector3.zero)
						{
							_0024self__0024144.cam_transform.LookAt(_0024lookatpos_0024133);
						}
						result = (YieldDefault(2) ? 1 : 0);
						break;
					}
					UnityEngine.Object.Destroy(_0024target_0024134);
					if (_0024timeout_0024143 != 0f)
					{
						result = (Yield(3, new WaitForSeconds(_0024timeout_0024143)) ? 1 : 0);
						break;
					}
					goto case 3;
				case 3:
					_0024self__0024144.update_angle = true;
					_0024self__0024144.update_position = true;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal Transform _0024playertransform_0024145;

		internal Transform _0024enemytransform_0024146;

		internal string _0024targetname_0024147;

		internal string _0024lookatname_0024148;

		internal float _0024speed_0024149;

		internal Vector3 _0024translate_0024150;

		internal float _0024timeout_0024151;

		internal battle_camera _0024self__0024152;

		public _0024MoveTo3_0024132(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float speed, Vector3 translate, float timeout, battle_camera self_)
		{
			_0024playertransform_0024145 = playertransform;
			_0024enemytransform_0024146 = enemytransform;
			_0024targetname_0024147 = targetname;
			_0024lookatname_0024148 = lookatname;
			_0024speed_0024149 = speed;
			_0024translate_0024150 = translate;
			_0024timeout_0024151 = timeout;
			_0024self__0024152 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024playertransform_0024145, _0024enemytransform_0024146, _0024targetname_0024147, _0024lookatname_0024148, _0024speed_0024149, _0024translate_0024150, _0024timeout_0024151, _0024self__0024152);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024MoveTo2_0024153 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal Vector3 _0024lookatpos_0024154;

			internal GameObject _0024target_0024155;

			internal Transform _0024bodypart_0024156;

			internal Vector3 _0024targetpos_0024157;

			internal Vector3 _0024startPos_0024158;

			internal float _0024ttime_0024159;

			internal float _0024dist_0024160;

			internal float _0024speed_0024161;

			internal float _0024k_0024162;

			internal Transform _0024playertransform_0024163;

			internal Transform _0024enemytransform_0024164;

			internal string _0024targetname_0024165;

			internal string _0024lookatname_0024166;

			internal float _0024moveTime_0024167;

			internal Vector3 _0024translate_0024168;

			internal float _0024timeout_0024169;

			internal battle_camera _0024self__0024170;

			public _0024(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float moveTime, Vector3 translate, float timeout, battle_camera self_)
			{
				_0024playertransform_0024163 = playertransform;
				_0024enemytransform_0024164 = enemytransform;
				_0024targetname_0024165 = targetname;
				_0024lookatname_0024166 = lookatname;
				_0024moveTime_0024167 = moveTime;
				_0024translate_0024168 = translate;
				_0024timeout_0024169 = timeout;
				_0024self__0024170 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024self__0024170.update_angle = false;
					_0024self__0024170.update_position = false;
					_0024lookatpos_0024154 = default(Vector3);
					_0024target_0024155 = new GameObject("MoveToTarget");
					if (_0024lookatname_0024166 == "center" || _0024lookatname_0024166 == "0")
					{
						_0024lookatpos_0024154 = _0024self__0024170.target_look.position;
					}
					else if (_0024lookatname_0024166 == "player")
					{
						_0024lookatpos_0024154 = _0024playertransform_0024163.position;
					}
					else if (_0024lookatname_0024166 == "playerup")
					{
						_0024lookatpos_0024154 = _0024playertransform_0024163.position + new Vector3(0f, 1f, 0f);
					}
					else if (_0024lookatname_0024166 == "enemy")
					{
						_0024lookatpos_0024154 = _0024enemytransform_0024164.position;
					}
					else if (_0024lookatname_0024166 == "auto")
					{
						_0024self__0024170.update_angle = true;
					}
					_0024bodypart_0024156 = funcs.FindChildByName(_0024self__0024170.player_transform, _0024targetname_0024165);
					_0024targetpos_0024157 = default(Vector3);
					if (_0024targetname_0024165 == "0" && (bool)_0024bodypart_0024156)
					{
						_0024targetpos_0024157 = _0024bodypart_0024156.transform.position;
					}
					else if (_0024targetname_0024165 == "center")
					{
						_0024targetpos_0024157 = _0024self__0024170.target_look.position;
					}
					else if (_0024targetname_0024165 == "player")
					{
						_0024targetpos_0024157 = _0024playertransform_0024163.position;
					}
					else if (_0024targetname_0024165 == "enemy")
					{
						_0024targetpos_0024157 = _0024enemytransform_0024164.position;
					}
					_0024target_0024155.transform.position = _0024targetpos_0024157;
					_0024target_0024155.transform.LookAt(_0024lookatpos_0024154);
					_0024target_0024155.transform.Translate(_0024translate_0024168);
					_0024targetpos_0024157 = _0024target_0024155.transform.position;
					_0024target_0024155.transform.position = _0024self__0024170.cam_transform.position;
					_0024startPos_0024158 = _0024self__0024170.cam_transform.position;
					_0024ttime_0024159 = _0024moveTime_0024167;
					goto case 2;
				case 2:
					if (_0024ttime_0024159 > 0f)
					{
						_0024dist_0024160 = Vector3.Distance(_0024self__0024170.cam_transform.position, _0024targetpos_0024157);
						if (_0024dist_0024160 >= 0.1f)
						{
							_0024speed_0024161 = _0024dist_0024160 / _0024ttime_0024159;
							_0024k_0024162 = 1f - _0024ttime_0024159 / _0024moveTime_0024167;
							_0024target_0024155.transform.LookAt(_0024targetpos_0024157);
							_0024target_0024155.transform.Translate(0f, 0f, Time.deltaTime * _0024speed_0024161);
							_0024ttime_0024159 -= Time.deltaTime;
							_0024self__0024170.cam_transform.position = _0024target_0024155.transform.position;
							if (_0024lookatpos_0024154 != Vector3.zero)
							{
								_0024self__0024170.cam_transform.LookAt(_0024lookatpos_0024154);
							}
							result = (YieldDefault(2) ? 1 : 0);
							break;
						}
					}
					UnityEngine.Object.Destroy(_0024target_0024155);
					if (_0024timeout_0024169 != 0f)
					{
						result = (Yield(3, new WaitForSeconds(_0024timeout_0024169)) ? 1 : 0);
						break;
					}
					goto case 3;
				case 3:
					_0024self__0024170.update_angle = true;
					_0024self__0024170.update_position = true;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal Transform _0024playertransform_0024171;

		internal Transform _0024enemytransform_0024172;

		internal string _0024targetname_0024173;

		internal string _0024lookatname_0024174;

		internal float _0024moveTime_0024175;

		internal Vector3 _0024translate_0024176;

		internal float _0024timeout_0024177;

		internal battle_camera _0024self__0024178;

		public _0024MoveTo2_0024153(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float moveTime, Vector3 translate, float timeout, battle_camera self_)
		{
			_0024playertransform_0024171 = playertransform;
			_0024enemytransform_0024172 = enemytransform;
			_0024targetname_0024173 = targetname;
			_0024lookatname_0024174 = lookatname;
			_0024moveTime_0024175 = moveTime;
			_0024translate_0024176 = translate;
			_0024timeout_0024177 = timeout;
			_0024self__0024178 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024playertransform_0024171, _0024enemytransform_0024172, _0024targetname_0024173, _0024lookatname_0024174, _0024moveTime_0024175, _0024translate_0024176, _0024timeout_0024177, _0024self__0024178);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024MoveTo_0024179 : GenericGenerator<object>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<object>, IEnumerator
		{
			internal Vector3 _0024lookatpos_0024180;

			internal GameObject _0024target_0024181;

			internal Transform _0024lookAtTransform_0024182;

			internal Transform _0024bodypart_0024183;

			internal Vector3 _0024targetpos_0024184;

			internal float _0024ttime_0024185;

			internal float _0024timeStart_0024186;

			internal Transform _0024targetTransform_0024187;

			internal float _0024maxTime_0024188;

			internal GameObject _0024obj_0024189;

			internal Vector3 _0024start_0024190;

			internal Vector3 _0024startLookAtPos_0024191;

			internal float _0024nx_0024192;

			internal float _0024ny_0024193;

			internal float _0024nz_0024194;

			internal Vector3 _0024startPos_0024195;

			internal float _0024dist_0024196;

			internal float _0024speed_0024197;

			internal float _0024k_0024198;

			internal Transform _0024playertransform_0024199;

			internal Transform _0024enemytransform_0024200;

			internal string _0024targetname_0024201;

			internal string _0024lookatname_0024202;

			internal float _0024moveTime_0024203;

			internal Vector3 _0024translate_0024204;

			internal float _0024timeout_0024205;

			internal battle_camera _0024self__0024206;

			public _0024(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float moveTime, Vector3 translate, float timeout, battle_camera self_)
			{
				_0024playertransform_0024199 = playertransform;
				_0024enemytransform_0024200 = enemytransform;
				_0024targetname_0024201 = targetname;
				_0024lookatname_0024202 = lookatname;
				_0024moveTime_0024203 = moveTime;
				_0024translate_0024204 = translate;
				_0024timeout_0024205 = timeout;
				_0024self__0024206 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024self__0024206.update_angle = false;
					_0024self__0024206.update_position = false;
					_0024lookatpos_0024180 = default(Vector3);
					_0024target_0024181 = new GameObject("MoveToTarget");
					_0024lookAtTransform_0024182 = null;
					if (_0024lookatname_0024202 == "center" || _0024lookatname_0024202 == "0")
					{
						_0024lookatpos_0024180 = _0024self__0024206.target_look.position;
						_0024lookAtTransform_0024182 = _0024self__0024206.target_look;
					}
					else if (_0024lookatname_0024202 == "player")
					{
						_0024lookatpos_0024180 = _0024playertransform_0024199.position;
						_0024lookAtTransform_0024182 = _0024playertransform_0024199;
					}
					else if (_0024lookatname_0024202 == "playerup")
					{
						_0024lookatpos_0024180 = _0024playertransform_0024199.position + new Vector3(0f, 1f, 0f);
						_0024lookAtTransform_0024182 = _0024playertransform_0024199;
					}
					else if (_0024lookatname_0024202 == "enemy")
					{
						_0024lookatpos_0024180 = _0024enemytransform_0024200.position;
						_0024lookAtTransform_0024182 = _0024enemytransform_0024200;
					}
					else if (_0024lookatname_0024202 == "auto")
					{
						_0024self__0024206.update_angle = true;
					}
					_0024bodypart_0024183 = funcs.FindChildByName(_0024self__0024206.player_transform, _0024targetname_0024201);
					_0024targetpos_0024184 = default(Vector3);
					if (_0024targetname_0024201 == "0" && (bool)_0024bodypart_0024183)
					{
						_0024targetpos_0024184 = _0024bodypart_0024183.transform.position;
					}
					else if (_0024targetname_0024201 == "center")
					{
						_0024targetpos_0024184 = _0024self__0024206.target_look.position;
					}
					else if (_0024targetname_0024201 == "player")
					{
						_0024targetpos_0024184 = _0024playertransform_0024199.position;
					}
					else if (_0024targetname_0024201 == "enemy")
					{
						_0024targetpos_0024184 = _0024enemytransform_0024200.position;
					}
					_0024target_0024181.transform.position = _0024targetpos_0024184;
					_0024target_0024181.transform.LookAt(_0024lookatpos_0024180);
					_0024target_0024181.transform.Translate(_0024translate_0024204);
					_0024targetpos_0024184 = _0024target_0024181.transform.position;
					_0024target_0024181.transform.position = _0024self__0024206.cam_transform.position;
					_0024ttime_0024185 = 0f;
					_0024timeStart_0024186 = Time.time;
					_0024self__0024206.vsign = -1f;
					if (!easing.IsNull())
					{
						_0024targetTransform_0024187 = _0024target_0024181.transform;
						_0024maxTime_0024188 = _0024moveTime_0024203;
						_0024obj_0024189 = new GameObject("_view");
						_0024start_0024190 = _0024targetTransform_0024187.position;
						_0024startLookAtPos_0024191 = _0024lookatpos_0024180;
						goto case 2;
					}
					_0024startPos_0024195 = _0024self__0024206.cam_transform.position;
					_0024ttime_0024185 = _0024moveTime_0024203;
					goto case 3;
				case 2:
					_0024target_0024181.transform.LookAt(_0024targetpos_0024184);
					_0024nx_0024192 = easing.CalcEase(_0024start_0024190.x, _0024targetpos_0024184.x - _0024start_0024190.x, _0024ttime_0024185, _0024maxTime_0024188);
					_0024ny_0024193 = easing.CalcEase(_0024start_0024190.y, _0024targetpos_0024184.y - _0024start_0024190.y, _0024ttime_0024185, _0024maxTime_0024188);
					_0024nz_0024194 = easing.CalcEase(_0024start_0024190.z, _0024targetpos_0024184.z - _0024start_0024190.z, _0024ttime_0024185, _0024maxTime_0024188);
					_0024lookatpos_0024180 = _0024self__0024206.Vibrate(_0024startLookAtPos_0024191);
					_0024targetTransform_0024187.position = new Vector3(_0024nx_0024192, _0024ny_0024193, _0024nz_0024194);
					_0024ttime_0024185 += Time.deltaTime;
					_0024self__0024206.cam_transform.position = _0024targetTransform_0024187.position;
					if (_0024lookatpos_0024180 != Vector3.zero)
					{
						_0024self__0024206.cam_transform.LookAt(_0024lookatpos_0024180);
					}
					_0024lookAtTransform_0024182.position = _0024lookatpos_0024180;
					if (_0024ttime_0024185 <= _0024maxTime_0024188)
					{
						result = (YieldDefault(2) ? 1 : 0);
						break;
					}
					_0024lookAtTransform_0024182.position = _0024startLookAtPos_0024191;
					goto IL_0639;
				case 3:
					if (_0024ttime_0024185 > 0f)
					{
						_0024dist_0024196 = Vector3.Distance(_0024self__0024206.cam_transform.position, _0024targetpos_0024184);
						if (_0024dist_0024196 >= 0.1f)
						{
							_0024speed_0024197 = _0024dist_0024196 / _0024ttime_0024185;
							_0024k_0024198 = 1f - _0024ttime_0024185 / _0024moveTime_0024203;
							_0024target_0024181.transform.LookAt(_0024targetpos_0024184);
							_0024target_0024181.transform.Translate(0f, 0f, Time.deltaTime * _0024speed_0024197);
							_0024ttime_0024185 -= Time.deltaTime;
							_0024self__0024206.cam_transform.position = _0024target_0024181.transform.position;
							if (_0024lookatpos_0024180 != Vector3.zero)
							{
								_0024self__0024206.cam_transform.LookAt(_0024lookatpos_0024180);
							}
							result = (YieldDefault(3) ? 1 : 0);
							break;
						}
					}
					goto IL_0639;
				case 4:
					if (_0024timeout_0024205 > 0f)
					{
						if (_0024self__0024206.in_vibration)
						{
							_0024lookatpos_0024180 = _0024self__0024206.Vibrate(_0024startLookAtPos_0024191);
							if (_0024lookatpos_0024180 != Vector3.zero)
							{
								_0024self__0024206.cam_transform.LookAt(_0024lookatpos_0024180);
							}
						}
						_0024timeout_0024205 -= Time.deltaTime;
						result = (YieldDefault(4) ? 1 : 0);
						break;
					}
					goto IL_06d3;
				case 5:
					if (_0024self__0024206.wait_move_to)
					{
						if (_0024self__0024206.in_vibration)
						{
							_0024lookatpos_0024180 = _0024self__0024206.Vibrate(_0024startLookAtPos_0024191);
							if (_0024lookatpos_0024180 != Vector3.zero)
							{
								_0024self__0024206.cam_transform.LookAt(_0024lookatpos_0024180);
							}
						}
						result = (YieldDefault(5) ? 1 : 0);
						break;
					}
					goto IL_075f;
				case 1:
					{
						result = 0;
						break;
					}
					IL_075f:
					_0024self__0024206.update_angle = true;
					_0024self__0024206.update_position = true;
					YieldDefault(1);
					goto case 1;
					IL_06d3:
					if (!(_0024timeout_0024205 >= 0f))
					{
						_0024self__0024206.wait_move_to = true;
						goto case 5;
					}
					goto IL_075f;
					IL_0639:
					UnityEngine.Object.Destroy(_0024target_0024181);
					if (!(_0024timeout_0024205 <= 0f))
					{
						goto case 4;
					}
					goto IL_06d3;
				}
				return (byte)result != 0;
			}
		}

		internal Transform _0024playertransform_0024207;

		internal Transform _0024enemytransform_0024208;

		internal string _0024targetname_0024209;

		internal string _0024lookatname_0024210;

		internal float _0024moveTime_0024211;

		internal Vector3 _0024translate_0024212;

		internal float _0024timeout_0024213;

		internal battle_camera _0024self__0024214;

		public _0024MoveTo_0024179(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float moveTime, Vector3 translate, float timeout, battle_camera self_)
		{
			_0024playertransform_0024207 = playertransform;
			_0024enemytransform_0024208 = enemytransform;
			_0024targetname_0024209 = targetname;
			_0024lookatname_0024210 = lookatname;
			_0024moveTime_0024211 = moveTime;
			_0024translate_0024212 = translate;
			_0024timeout_0024213 = timeout;
			_0024self__0024214 = self_;
		}

		public override IEnumerator<object> GetEnumerator()
		{
			return new _0024(_0024playertransform_0024207, _0024enemytransform_0024208, _0024targetname_0024209, _0024lookatname_0024210, _0024moveTime_0024211, _0024translate_0024212, _0024timeout_0024213, _0024self__0024214);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Slowmo2_0024215 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal float _0024inStepsCount_0024216;

			internal float _0024inScaleStep_0024217;

			internal float _0024inStepsCountCurrent_0024218;

			internal float _0024timeout_0024219;

			internal float _0024speed_0024220;

			internal float _0024intime_0024221;

			public _0024(float timeout, float speed, float intime)
			{
				_0024timeout_0024219 = timeout;
				_0024speed_0024220 = speed;
				_0024intime_0024221 = intime;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					Time.timeScale = 1f;
					_0024speed_0024220 = 1f / _0024speed_0024220;
					_0024inStepsCount_0024216 = _0024intime_0024221 / 0.05f;
					_0024inScaleStep_0024217 = (1f - _0024speed_0024220) / _0024inStepsCount_0024216;
					_0024inStepsCountCurrent_0024218 = _0024inStepsCount_0024216;
					goto IL_00ad;
				case 2:
					_0024inStepsCountCurrent_0024218 -= 1f;
					goto IL_00ad;
				case 3:
					_0024inStepsCountCurrent_0024218 = _0024inStepsCount_0024216;
					goto IL_013e;
				case 4:
					_0024inStepsCountCurrent_0024218 -= 1f;
					goto IL_013e;
				case 1:
					{
						result = 0;
						break;
					}
					IL_00ad:
					if (_0024inStepsCountCurrent_0024218 > 0f)
					{
						Time.timeScale -= _0024inScaleStep_0024217;
						result = (Yield(2, new WaitForSeconds(0.05f)) ? 1 : 0);
						break;
					}
					Time.timeScale = _0024speed_0024220;
					if (!(_0024timeout_0024219 < _0024intime_0024221))
					{
						result = (Yield(3, new WaitForSeconds(_0024timeout_0024219 - _0024intime_0024221)) ? 1 : 0);
						break;
					}
					goto case 3;
					IL_013e:
					if (_0024inStepsCountCurrent_0024218 > 0f)
					{
						Time.timeScale += _0024inScaleStep_0024217;
						result = (Yield(4, new WaitForSeconds(0.05f)) ? 1 : 0);
						break;
					}
					Time.timeScale = 1f;
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal float _0024timeout_0024222;

		internal float _0024speed_0024223;

		internal float _0024intime_0024224;

		public _0024Slowmo2_0024215(float timeout, float speed, float intime)
		{
			_0024timeout_0024222 = timeout;
			_0024speed_0024223 = speed;
			_0024intime_0024224 = intime;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024timeout_0024222, _0024speed_0024223, _0024intime_0024224);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Slowmo_0024225 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal float _0024timeout_0024226;

			public _0024(float timeout)
			{
				_0024timeout_0024226 = timeout;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if (Time.timeScale > 0.2f)
					{
						Time.timeScale -= 0.1f;
						result = (Yield(2, new WaitForSeconds(0.05f)) ? 1 : 0);
						break;
					}
					Time.timeScale = 0.2f;
					if (!(_0024timeout_0024226 < 0.8f))
					{
						result = (Yield(3, new WaitForSeconds(_0024timeout_0024226 - 0.8f)) ? 1 : 0);
						break;
					}
					goto case 3;
				case 3:
				case 4:
					if (Time.timeScale < 1f)
					{
						Time.timeScale += 0.1f;
						result = (Yield(4, new WaitForSeconds(0.05f)) ? 1 : 0);
						break;
					}
					Time.timeScale = 1f;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal float _0024timeout_0024227;

		public _0024Slowmo_0024225(float timeout)
		{
			_0024timeout_0024227 = timeout;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024timeout_0024227);
		}
	}

	public int mode;

	public Vector3 mouselook_angle;

	public Vector3 mouselook_last_angle;

	public Vector3 mouselook_last_pos;

	public bool thirdlook;

	public float distance;

	public float vibration;

	public Vector3 addOffset;

	private bool wait_move_to;

	private bool gameover;

	private float thirdlook_timer;

	private float start_distance;

	private bool update_angle;

	private bool update_position;

	private Vector3 smooth;

	private Vector2 distance_range;

	private GameObject cam;

	private Transform cam_transform;

	public bool ignore_CameraEnemyBonesOffset;

	public GameObject player;

	public GameObject enemy;

	private character_parameters player_params;

	private character_parameters enemy_params;

	private Vector3 player_pos;

	private Vector3 enemy_pos;

	private Transform player_bones;

	private Transform enemy_bones;

	private Transform player_transform;

	private Transform enemy_transform;

	private Vector3 player_bone_cam_offset;

	private Transform player_bone_cam_transform;

	private Transform enemy_bone_cam_transform;

	private float characters_distance;

	private float characters_distance_velocity;

	private Transform target_look;

	private Transform target_pos;

	private GameObject target_dummy;

	private GameObject arenacenter;

	private float arenacenter_y;

	private bool lock_move;

	private UnityScript.Lang.Array lock_data;

	public bool in_game_over;

	private bool startfly;

	private float addAngle;

	public bool _seted;

	public float prev_add_height;

	public float vibration_timeout;

	public float vibration_start_time;

	public float vibration_sign;

	public float vibration_back_value;

	public bool in_vibration;

	public Transform MoveToArg1;

	public Transform MoveToArg2;

	public string MoveToArg3;

	public string MoveToArg4;

	public float MoveToArg5;

	public Vector3 MoveToArg6;

	public float MoveToArg7;

	public float vsign;

	public Vector3 enPosLook;

	public Vector3 plPosLook;

	public battle_camera()
	{
		distance = 2f;
		addOffset = new Vector3(0f, 1f, 0f);
		start_distance = -20f;
		smooth = new Vector3(25f, 1f, 30f);
		distance_range = new Vector2(6f, 11.5f);
		player_bone_cam_offset = new Vector3(0f, 0f, 0f);
		startfly = true;
		addAngle = 60f;
		vibration_sign = 1f;
		vsign = -1f;
		enPosLook = new Vector3(0f, 0f, 0f);
		plPosLook = new Vector3(0f, 0f, 0f);
	}

	public virtual IEnumerator Start()
	{
		return new _0024Start_002491(this).GetEnumerator();
	}

	public virtual void OnDisable()
	{
		if ((bool)target_pos)
		{
			UnityEngine.Object.Destroy(target_pos.gameObject);
			target_pos = null;
		}
		if ((bool)target_look)
		{
			UnityEngine.Object.Destroy(target_look.gameObject);
			target_look = null;
		}
		if ((bool)target_dummy)
		{
			UnityEngine.Object.Destroy(target_dummy.gameObject);
			target_dummy = null;
		}
	}

	public virtual IEnumerator GameOverMode()
	{
		return new _0024GameOverMode_002495(this).GetEnumerator();
	}

	public virtual void SetPlayer(GameObject inPlayer)
	{
		player = inPlayer;
		player_bones = inPlayer.transform.Find("bones");
		player_transform = inPlayer.transform;
		player_bone_cam_transform = funcs.FindChildByName(player_transform, "bone_cam");
		player_params = (character_parameters)player.GetComponent<character_parameters>();
	}

	public virtual void SetEnemy(GameObject inEnemy)
	{
		enemy = inEnemy;
		enemy_bones = inEnemy.transform.Find("bones");
		enemy_transform = inEnemy.transform;
		enemy_bone_cam_transform = funcs.FindChildByName(enemy_transform, "bone_cam");
		enemy_params = (character_parameters)enemy.GetComponent<character_parameters>();
	}

	public virtual void SetAddAngle(float angle)
	{
		addAngle = angle;
	}

	public virtual void SetDistance(float v)
	{
		distance = v;
	}

	public virtual void SetPlayerBoneCamOffset(Vector3 v)
	{
		player_bone_cam_offset = v;
	}

	public virtual void SetAddOffset(Vector3 offset)
	{
		addOffset = offset;
	}

	public virtual void LateUpdate()
	{
		if (gameover)
		{
			cam_transform.RotateAround(player_pos, Vector3.up, -20f * Time.deltaTime);
			return;
		}
		bool mouseButtonDown = Input.GetMouseButtonDown(1);
		bool mouseButtonUp = Input.GetMouseButtonUp(1);
		bool flag = false;
		if (mouseButtonDown)
		{
		}
		if (flag)
		{
			return;
		}
		if (!lock_move && mouseButtonDown)
		{
			mouselook_last_pos = cam_transform.position;
			mouselook_last_angle = cam_transform.eulerAngles;
			thirdlook_timer += funcs.RealDeltaTime();
		}
		if (Input.GetMouseButton(1))
		{
		}
		if (mouseButtonUp)
		{
		}
		mode = @params.CAMERA_MODE;
		if (!lock_move && (bool)player && (bool)enemy)
		{
			if (startfly)
			{
				if (!(smooth.x <= smooth.y))
				{
					smooth.x -= funcs.RealDeltaTime() * 10f;
				}
				else
				{
					startfly = false;
				}
			}
			else if (!funcs.IsScenariosPlaying())
			{
				if (!(smooth.x >= smooth.z))
				{
					smooth.x += funcs.RealDeltaTime() * 10f;
				}
			}
			else
			{
				smooth.x = smooth.y;
			}
			UpdateCharactersDistance();
			if (update_position)
			{
				UpdateCameraPosition();
			}
			if (update_angle)
			{
				UpdateCameraAngle();
			}
		}
		else if (!lock_move && (bool)player && !enemy)
		{
			Transform transform = target_dummy.transform;
			transform.position = player_transform.position;
			transform.eulerAngles = player_transform.eulerAngles;
			transform.Translate(0f, 0.6f, @params.CHARACTERS_DISTANCE * 2f);
			enemy = target_dummy;
			enemy_bones = enemy.transform;
			enemy_transform = enemy.transform;
		}
	}

	public virtual void UpdateCharactersDistance()
	{
		if (!player_transform && !player_transform)
		{
			return;
		}
		if (player_pos == Vector3.zero)
		{
			player_pos = player_transform.position;
		}
		if (enemy_pos == Vector3.zero)
		{
			enemy_pos = enemy_transform.position;
		}
		Vector3 to;
		Vector3 to2;
		if (mode == 0)
		{
			to = player_bones.position;
			Transform transform = player_bone_cam_transform;
			if ((bool)player_bone_cam_transform)
			{
				to = new Vector3(transform.position.x, to.y, transform.position.z);
			}
			to2 = enemy_bones.position;
			transform = enemy_bone_cam_transform;
			if ((bool)enemy_bone_cam_transform)
			{
				to2 = new Vector3(transform.position.x, to2.y, transform.position.z);
			}
		}
		else
		{
			to = player_transform.position;
			to2 = enemy_transform.position;
		}
		player_pos = Vector3.Lerp(player_pos, to, 2f);
		enemy_pos = Vector3.Lerp(enemy_pos, to2, 2f);
		if (@params.CAMERA_MODE != 1)
		{
			if (!(player_pos.y >= arenacenter_y))
			{
				player_pos.y = arenacenter_y;
			}
			if (!(enemy_pos.y >= arenacenter_y))
			{
				enemy_pos.y = arenacenter_y;
			}
		}
		Vector3 position = player_transform.position;
		player_transform.position = player_pos + player_bone_cam_offset;
		plPosLook = player_transform.TransformPoint(new Vector3(0f, 0f, (!player_params) ? 0f : player_params.CameraBonesOffset));
		player_transform.position = position;
		position = enemy_transform.position;
		enemy_transform.position = enemy_pos;
		character_parameters character_parameters2;
		enPosLook = enemy_transform.TransformPoint(new Vector3(0f, 0f, (!RuntimeServices.ToBool((!(character_parameters2 = enemy_params)) ? character_parameters2 : ((object)(!ignore_CameraEnemyBonesOffset)))) ? 0f : enemy_params.CameraEnemyBonesOffset));
		enemy_transform.position = position;
		float num = Vector3.Distance(plPosLook, enPosLook);
		if (!(num >= 0.1f))
		{
			num = 0.1f;
		}
		if (RuntimeServices.EqualityOperator(characters_distance, Vector3.zero))
		{
			characters_distance = num;
		}
		characters_distance = Mathf.Lerp(characters_distance, num, funcs.RealDeltaTime() * 15f);
	}

	public virtual void UpdateCameraPosition()
	{
		if (thirdlook)
		{
			if (mode == 1)
			{
				target_pos.position = player_transform.position;
				target_pos.rotation = player_transform.rotation;
				target_pos.Translate(2f, 2f, -3f);
			}
			else
			{
				target_pos.position = player_bones.position;
				target_pos.rotation = player_transform.rotation;
				target_pos.Translate(2f, 1f, -3f);
			}
		}
		else
		{
			target_pos.position = player_transform.position;
			target_pos.LookAt(enemy_transform.position);
			float num = characters_distance * distance + Mathf.Max((!player_params) ? 0f : player_params.CameraAddDistance, (!enemy_params) ? 0f : enemy_params.CameraEnemyAddDistance);
			if (!(num >= distance_range.x))
			{
				num = distance_range.x;
			}
			if (!(num <= distance_range.y))
			{
				num = distance_range.y;
			}
			target_pos.Translate(num, 1f - vibration, characters_distance / 2f);
		}
		if (!_seted && (bool)cam_transform && (bool)arenacenter && (bool)arenacenter.transform)
		{
			cam_transform.position = target_pos.position + addOffset;
			cam_transform.rotation = target_pos.rotation;
			cam_transform.RotateAround(arenacenter.transform.position, new Vector3(0f, 1f, 0f), addAngle);
		}
	}

	public virtual void SetCamAtTarget()
	{
		if ((bool)player_transform)
		{
			target_pos.position = player_transform.position;
			target_pos.LookAt(enemy_transform.position);
			float num = characters_distance * distance;
			if (!(num >= distance_range.x))
			{
				num = distance_range.x;
			}
			if (!(num <= distance_range.y))
			{
				num = distance_range.y;
			}
			target_pos.Translate(num, 1f - vibration, characters_distance / 2f);
			cam_transform.position = target_pos.position;
		}
	}

	public virtual void UpdateCameraAngle()
	{
		float num = ((mode != 1) ? 0.5f : 1.5f);
		Vector3 position = target_look.position;
		target_look.position = player_pos;
		target_look.LookAt(enemy_pos);
		target_look.position = target_look.TransformPoint(new Vector3(0f, 0f, (!player_params) ? 0f : player_params.CameraBonesOffset));
		target_look.LookAt(enemy_pos);
		float num2 = characters_distance / 1.9f;
		if (!(num2 >= 0.1f))
		{
			num2 = 0.1f;
		}
		target_look.Translate(0f, num - vibration, num2);
		if (position == Vector3.zero)
		{
			position = target_look.position;
		}
		target_look.position = Vector3.Lerp(position, target_look.position, funcs.RealDeltaTime() * 8f * 20f / smooth.x);
		float num3 = ((!player_params) ? 0f : player_params.CameraAddHeight);
		if ((bool)enemy_params && enemy_params.CameraEnemyAddHeight != 0f)
		{
			num3 = ((num3 != 0f) ? ((enemy_params.CameraEnemyAddHeight + num3) / 2f) : enemy_params.CameraEnemyAddHeight);
		}
		cam_transform.LookAt(funcs.AddY(target_look.position, num3));
	}

	public virtual IEnumerator Vibration(float power, int count, float increase, float timeout)
	{
		return new _0024Vibration_0024101(power, count, increase, timeout, this).GetEnumerator();
	}

	public virtual IEnumerator SetOnPosition(string bodypart_name, string isparent, string lookat, Vector3 pos, float timeout, string slowmo)
	{
		return new _0024SetOnPosition_0024114(bodypart_name, isparent, lookat, pos, timeout, slowmo, this).GetEnumerator();
	}

	public virtual IEnumerator MoveTo3(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float speed, Vector3 translate, float timeout)
	{
		return new _0024MoveTo3_0024132(playertransform, enemytransform, targetname, lookatname, speed, translate, timeout, this).GetEnumerator();
	}

	public virtual IEnumerator MoveTo2(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float moveTime, Vector3 translate, float timeout)
	{
		return new _0024MoveTo2_0024153(playertransform, enemytransform, targetname, lookatname, moveTime, translate, timeout, this).GetEnumerator();
	}

	public virtual void EndMoveTo()
	{
		wait_move_to = false;
		_seted = true;
	}

	public virtual void MoveToArray()
	{
		Transform moveToArg = MoveToArg1;
		Transform moveToArg2 = MoveToArg2;
		MoveToArg1 = null;
		MoveToArg2 = null;
		StartCoroutine_Auto(MoveTo(moveToArg, moveToArg2, MoveToArg3, MoveToArg4, MoveToArg5, MoveToArg6, MoveToArg7));
	}

	public virtual IEnumerator MoveTo(Transform playertransform, Transform enemytransform, string targetname, string lookatname, float moveTime, Vector3 translate, float timeout)
	{
		return new _0024MoveTo_0024179(playertransform, enemytransform, targetname, lookatname, moveTime, translate, timeout, this).GetEnumerator();
	}

	public virtual Vector3 Vibrate(Vector3 initPos)
	{
		float num = (Time.time - vibration_start_time) / vibration_timeout;
		if (!(num <= 1f))
		{
			num = 1f;
		}
		float num2 = Mathf.Lerp(0f, (vibration_sign <= 0f) ? vibration_back_value : vibration, (vsign <= 0f) ? (1f - num) : num);
		if (num == 1f)
		{
			vsign = 0f - vsign;
		}
		return new Vector3(initPos.x, initPos.y + num2, initPos.z);
	}

	public virtual IEnumerator Slowmo2(float timeout, float speed, float intime)
	{
		return new _0024Slowmo2_0024215(timeout, speed, intime).GetEnumerator();
	}

	public virtual IEnumerator Slowmo(float timeout)
	{
		return new _0024Slowmo_0024225(timeout).GetEnumerator();
	}

	public virtual void Main()
	{
	}
}
