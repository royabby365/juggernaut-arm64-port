using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boo.Lang;
using Boo.Lang.Runtime;
using UnityEngine;

[Serializable]
public class sfx_react : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Start_002487 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal Transform _0024child_002488;

			internal sfx_react _0024self__002489;

			public _0024(sfx_react self_)
			{
				_0024self__002489 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024child_002488 = _0024self__002489.transform.Find("image");
					if (!_0024child_002488)
					{
						_0024child_002488 = _0024self__002489.transform.Find("text");
					}
					_0024child_002488.animation.Play();
					_0024child_002488.renderer.enabled = false;
					result = (Yield(2, new WaitForSeconds(0.1f)) ? 1 : 0);
					break;
				case 2:
					_0024self__002489.textmesh = (TextMesh)_0024self__002489.transform.GetComponentInChildren(typeof(TextMesh));
					if ((bool)_0024self__002489.textmesh)
					{
						_0024self__002489.textmesh.text = _0024self__002489.text;
						_0024self__002489.icon = _0024self__002489.textmesh.transform.Find("icon");
						if (RuntimeServices.ToBool(_0024self__002489.icon))
						{
							_0024self__002489.textobj = _0024self__002489.transform.Find("text");
						}
					}
					_0024self__002489.transform.Translate(0f, 0.5f, 1f);
					if ((bool)_0024self__002489.transform.parent)
					{
						_0024self__002489.old_parent_transform = _0024self__002489.transform.parent.transform;
					}
					_0024self__002489.transform.parent = null;
					_0024child_002488.renderer.enabled = true;
					_0024self__002489.start_time = Time.time;
					if (_0024self__002489.enableAutodestroy)
					{
						result = (Yield(3, new WaitForSeconds(_0024self__002489.autodestroy)) ? 1 : 0);
						break;
					}
					goto IL_020e;
				case 3:
					UnityEngine.Object.Destroy(_0024self__002489.gameObject);
					goto IL_020e;
				case 1:
					{
						result = 0;
						break;
					}
					IL_020e:
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal sfx_react _0024self__002490;

		public _0024Start_002487(sfx_react self_)
		{
			_0024self__002490 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024self__002490);
		}
	}

	public string text;

	public float autodestroy;

	public bool enableAutodestroy;

	public TextMesh textmesh;

	public object icon;

	public object textobj;

	public Transform old_parent_transform;

	public bool do_translate;

	public float offset;

	public float start_time;

	public float tan;

	public object baf;

	public sfx_react()
	{
		text = string.Empty;
		autodestroy = 3f;
		enableAutodestroy = true;
	}

	public virtual void Restart()
	{
		Debug.Log("restart");
		StartCoroutine_Auto(Start());
	}

	public virtual IEnumerator Start()
	{
		return new _0024Start_002487(this).GetEnumerator();
	}

	public virtual void SetDeviationAngle(float angle)
	{
		tan = Mathf.Tan(angle * ((float)Math.PI / 180f));
	}

	public virtual void LateUpdate()
	{
		if ((bool)textmesh && (bool)old_parent_transform)
		{
			float x = old_parent_transform.position.x;
			Vector3 position = transform.position;
			float num = (position.x = x);
			Vector3 vector = (transform.position = position);
			float z = old_parent_transform.position.z;
			Vector3 position2 = transform.position;
			float num2 = (position2.z = z);
			Vector3 vector3 = (transform.position = position2);
			if (do_translate && tan != 0f)
			{
				offset = tan * textmesh.transform.localPosition.y;
				transform.root.Translate(offset, 0f, 0f, transform);
			}
		}
	}

	public virtual void Main()
	{
	}
}
