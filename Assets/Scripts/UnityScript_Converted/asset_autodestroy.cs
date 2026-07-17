using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boo.Lang;
using UnityEngine;

[Serializable]
public class asset_autodestroy : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Start_002467 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal asset_autodestroy _0024self__002468;

			public _0024(asset_autodestroy self_)
			{
				_0024self__002468 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					result = (Yield(2, new WaitForSeconds(_0024self__002468.timer)) ? 1 : 0);
					break;
				case 2:
					UnityEngine.Object.Destroy(_0024self__002468.gameObject);
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal asset_autodestroy _0024self__002469;

		public _0024Start_002467(asset_autodestroy self_)
		{
			_0024self__002469 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024self__002469);
		}
	}

	public float timer;

	public asset_autodestroy()
	{
		timer = 1f;
	}

	public virtual IEnumerator Start()
	{
		return new _0024Start_002467(this).GetEnumerator();
	}

	public virtual void Main()
	{
	}
}
