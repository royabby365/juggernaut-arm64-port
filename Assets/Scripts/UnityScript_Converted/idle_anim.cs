using System;
using UnityEngine;

[Serializable]
public class idle_anim : MonoBehaviour
{
	public virtual void Start()
	{
		if ((bool)GetComponent<Animation>())
		{
			GetComponent<Animation>().wrapMode = WrapMode.Loop;
		}
	}

	public virtual void Update()
	{
		if ((bool)GetComponent<Animation>() && !GetComponent<Animation>().isPlaying)
		{
			GetComponent<Animation>().Play("idle");
		}
	}

	public virtual void Main()
	{
	}
}
