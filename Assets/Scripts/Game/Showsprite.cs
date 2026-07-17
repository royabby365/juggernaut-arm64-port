using UnityEngine;

public class Showsprite : MonoBehaviour
{
	private ShowspriteState _state;

	private float _alpha;

	private float _dir;

	private float _speed;

	private float _time;

	public float FadeInTime;

	public float FadeOutTime;

	public float ShowTime;

	public Texture2D FadeImage;

	public Rect NormalizedRect = new Rect(0f, 0f, 1f, 1f);

	private void Start()
	{
		_state = ShowspriteState.Disabled;
		Restart();
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		if (_state != ShowspriteState.Done && _state != ShowspriteState.Disabled)
		{
			GUI.depth = -1000;
			Draw();
		}
	}

	public void Restart()
	{
		_state = ShowspriteState.None;
		_alpha = 1f;
		_speed = 0.04f;
		_dir = 1f;
	}

	private void Draw()
	{
		if (_state == ShowspriteState.None)
		{
			FadeOut();
			DrawFade();
		}
		else if (_state == ShowspriteState.In || _state == ShowspriteState.Out)
		{
			DrawFade();
		}
		else if (_state == ShowspriteState.Normal)
		{
			DrawNormal();
		}
	}

	private void DrawNormal()
	{
		DrawImages();
		_time -= Time.deltaTime;
		if (_time < 0f)
		{
			FadeIn();
		}
	}

	private void DrawFade()
	{
		GuiA(1f);
		DrawImages();
		GuiA(_alpha);
		_alpha += _speed * _dir * Time.deltaTime;
		if (_state == ShowspriteState.Out)
		{
			if (_alpha > 1f)
			{
				GotoNormal();
			}
		}
		else if (_state == ShowspriteState.In && _alpha < 0f)
		{
			GotoDone();
		}
	}

	private void GotoDone()
	{
		_state = ShowspriteState.Disabled;
	}

	private void GotoNormal()
	{
		_state = ShowspriteState.Normal;
		_alpha = 1f;
		_time = ShowTime;
		GuiA(_alpha);
	}

	private void DrawImages()
	{
		if (_state == ShowspriteState.In || _state == ShowspriteState.Out || _state == ShowspriteState.Normal)
		{
			float a = GUI.color.a;
			GuiA(_alpha);
			DrawTexture2D(FadeImage);
			GuiA(a);
		}
	}

	private void DrawTexture2D(Texture2D tex)
	{
		Rect position = new Rect((float)Screen.width * NormalizedRect.x, (float)Screen.height * NormalizedRect.y, (float)Screen.width * NormalizedRect.width, (float)Screen.height * NormalizedRect.height);
		GUI.DrawTexture(position, FadeImage);
	}

	private void GuiA(float p)
	{
		Color color = GUI.color;
		color.a = p;
		GUI.color = color;
	}

	private void FadeIn()
	{
		_state = ShowspriteState.In;
		_alpha = 1f;
		_dir = -1f;
		_time = 0f;
		_speed = 1f / FadeInTime;
	}

	private void FadeOut()
	{
		_state = ShowspriteState.Out;
		_alpha = 0f;
		_dir = 1f;
		_time = 0f;
		_speed = 1f / FadeOutTime;
	}
}
