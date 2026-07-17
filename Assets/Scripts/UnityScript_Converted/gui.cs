using System;
using Boo.Lang.Runtime;
using UnityEngine;

[Serializable]
public class gui : MonoBehaviour
{
	[NonSerialized]
	public static object console;

	[NonSerialized]
	public static object controlpanel;

	[NonSerialized]
	public static object cursor;

	[NonSerialized]
	public static object dialogs;

	[NonSerialized]
	public static object icons;

	[NonSerialized]
	public static object tooltip;

	public Font unicode_font;

	public bool setfont;

	public Texture cursor_default;

	public Texture cursor_link;

	public GUIStyle styleControlpanelList;

	public gui()
	{
		setfont = true;
	}

	public virtual void Start()
	{
		console = gameObject.AddComponent("gui_console");
		if (PlayerPrefs.HasKey("controlpanel"))
		{
			controlpanel = gameObject.AddComponent("gui_controlpanel");
		}
		cursor = gameObject.AddComponent("gui_cursor");
		dialogs = gameObject.AddComponent("gui_dialogs");
		icons = gameObject.AddComponent("gui_icons");
		tooltip = gameObject.AddComponent("gui_tooltip");
		cursor_link = (Texture)Resources.Load("cursor_hand");
	}

	public virtual void OnGUI()
	{
		if (setfont && (bool)unicode_font)
		{
			GUI.skin.font = unicode_font;
			setfont = false;
		}
	}

	public virtual void Update()
	{
		if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.J))
		{
			if (PlayerPrefs.HasKey("controlpanel"))
			{
				if (RuntimeServices.ToBool(controlpanel))
				{
					object obj = controlpanel;
					if (!(obj is UnityEngine.Object))
					{
						obj = RuntimeServices.Coerce(obj, typeof(UnityEngine.Object));
					}
					UnityEngine.Object.Destroy((UnityEngine.Object)obj);
				}
				PlayerPrefs.DeleteKey("controlpanel");
			}
			else
			{
				if (!RuntimeServices.ToBool(controlpanel))
				{
					controlpanel = gameObject.AddComponent("gui_controlpanel");
				}
				PlayerPrefs.SetString("controlpanel", "1");
			}
		}
		if (Input.GetKeyDown(KeyCode.PageUp))
		{
			QualitySettings.IncreaseLevel();
		}
		if (Input.GetKeyDown(KeyCode.PageDown))
		{
			QualitySettings.DecreaseLevel();
		}
	}

	public virtual void Main()
	{
	}
}
