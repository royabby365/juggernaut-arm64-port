using System;
using Boo.Lang.Runtime;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class flash : MonoBehaviour
{
	[NonSerialized]
	public static object controls;

	[NonSerialized]
	public static object icons;

	[NonSerialized]
	public static object messages;

	[NonSerialized]
	public static object commands;

	public float camera_k;

	public float camera_t;

	[NonSerialized]
	public static UnityScript.Lang.Array sendmessages = new object[0];

	public flash()
	{
		camera_k = 1f;
		camera_t = 0.3f;
	}

	public virtual void Start()
	{
		controls = gameObject.AddComponent(System.Type.GetType("flash_controls, Assembly-CSharp"));
		icons = gameObject.AddComponent(System.Type.GetType("flash_icons, Assembly-CSharp"));
		messages = gameObject.AddComponent(System.Type.GetType("flash_messages, Assembly-CSharp"));
		commands = gameObject.AddComponent(System.Type.GetType("flash_commands, Assembly-CSharp"));
	}

	public virtual void Update()
	{
		if (sendmessages.length != 0)
		{
			object obj = sendmessages[0];
			if (!(obj is string))
			{
				obj = RuntimeServices.Coerce(obj, typeof(string));
			}
			string text = (string)obj;
			Application.ExternalCall("unity_receive", text);
			sendmessages.shift();
		}
	}

	public static void Send(string msg)
	{
		sendmessages.Add(msg);
	}

	public static void Progress(string status)
	{
	}

	public virtual void QuitGame()
	{
		GameObject gameObject = new GameObject("core");
		gameObject.AddComponent(System.Type.GetType("flash_quit, Assembly-CSharp"));
	}

	public virtual bool IsPrintable(string msg)
	{
		string text = funcs.Part(msg, 0, string.Empty);
		return true;
	}

	public virtual void Main()
	{
	}
}
