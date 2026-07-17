using System;
using UnityEngine;

[Serializable]
public class asset_showsprite : MonoBehaviour
{
	public float fade_in_time;

	public float fade_out_time;

	public float show_time;

	public Texture2D fade_image;

	public Rect draw_rect;

	[NonSerialized]
	public static int state_disabled;

	[NonSerialized]
	public static int state_none = 1;

	[NonSerialized]
	public static int state_in = 2;

	[NonSerialized]
	public static int state_normal = 3;

	[NonSerialized]
	public static int state_out = 4;

	[NonSerialized]
	public static int state_done = 5;

	private int state;

	private float alpha;

	private float dir;

	private float speed;

	private float time;

	public virtual void Start()
	{
		state = state_disabled;
		Restart();
	}

	public virtual void Update()
	{
	}

	public virtual void Restart()
	{
		state = state_none;
		alpha = 1f;
		speed = 0.04f;
		dir = 1f;
	}

	public virtual void OnGUI()
	{
		if (state != state_done && state != state_disabled)
		{
			GUI.depth = -1000;
			Draw();
		}
	}

	public virtual void Draw()
	{
		if (state == state_none)
		{
			FadeOut();
			DrawFade();
		}
		else if (state == state_in || state == state_out)
		{
			DrawFade();
		}
		else if (state == state_normal)
		{
			DrawNormal();
		}
	}

	public virtual void DrawImages()
	{
		if (state == state_in || state == state_out || state == state_normal)
		{
			float a = GUI.color.a;
			GuiA(alpha);
			DrawTexture2D(fade_image);
			GuiA(a);
		}
	}

	public virtual void DrawTexture2D(Texture2D tex)
	{
		GUI.DrawTexture(draw_rect, tex);
	}

	public virtual void GuiA(float p)
	{
		Color color = GUI.color;
		color.a = p;
		GUI.color = color;
	}

	public virtual void GotoNormal()
	{
		state = state_normal;
		alpha = 1f;
		time = show_time;
		GuiA(alpha);
	}

	public virtual void GotoDone()
	{
		state = state_disabled;
	}

	public virtual void DrawNormal()
	{
		DrawImages();
		time -= Time.deltaTime;
		if (!(time >= 0f))
		{
			FadeIn();
		}
	}

	public virtual void DrawFade()
	{
		GuiA(1f);
		DrawImages();
		GuiA(alpha);
		alpha += speed * dir * Time.deltaTime;
		if (state == state_out)
		{
			if (!(alpha <= 1f))
			{
				GotoNormal();
			}
		}
		else if (state == state_in && !(alpha >= 0f))
		{
			GotoDone();
		}
	}

	public virtual void FadeIn()
	{
		state = state_in;
		alpha = 1f;
		dir = -1f;
		time = 0f;
		speed = 1f / fade_in_time;
	}

	public virtual void FadeOut()
	{
		state = state_out;
		alpha = 0f;
		dir = 1f;
		time = 0f;
		speed = 1f / fade_out_time;
	}

	public virtual void Main()
	{
	}
}
