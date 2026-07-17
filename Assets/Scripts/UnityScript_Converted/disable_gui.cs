using System;
using UnityEngine;

[Serializable]
public class disable_gui : MonoBehaviour
{
	public virtual void Start()
	{
		GUI.enabled = false;
	}

	public virtual void Main()
	{
	}
}
