using System;
using UnityEngine;

[Serializable]
public class maneken_parameters : MonoBehaviour
{
	public float[] DamagesStartTime;

	public string AnimationName;

	public bool IsSkinned;

	public maneken_parameters()
	{
		AnimationName = string.Empty;
		IsSkinned = true;
	}

	public virtual void Main()
	{
	}
}
