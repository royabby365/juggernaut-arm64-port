using UnityEngine;

public class MainMenuFbButtonMk1 : SpriteButton
{
	public Color darkTint = new Color(0.5f, 0.5f, 0.5f);

	public Color neutralTint = new Color32(128, 128, 128, byte.MaxValue);

	public Vector3 overScale = new Vector3(1.1f, 1.1f, 1f);

	public Sprite normal;

	public Sprite over;

	private bool PosFixes;

	private Vector3 _localPos;

	public override void SetActive()
	{
		base.SetActive();
		SetColor(neutralTint);
	}

	public override void SetInactive()
	{
		base.SetInactive();
		SetColor(darkTint);
	}

	private void Awake()
	{
		Init();
	}

	public new void Init()
	{
		base.Init();
		if (UnityApi.UseOK())
		{
			base.gameObject.SetActiveRecursively(state: false);
			return;
		}
		if (!UnityApi.UseGameClub() && !PosFixes)
		{
			Vector3 position = base.transform.position;
			position.x -= 40f;
			base.transform.position = position;
			PosFixes = true;
		}
		Sprite componentInChildren = base.transform.GetComponentInChildren<Sprite>();
		if (componentInChildren != null && _collider != null)
		{
			int width = componentInChildren.Width;
			int height = componentInChildren.Height;
			((BoxCollider)_collider).center = componentInChildren.GetCenter();
			_horizontalPadding /= base.transform.localScale.x;
			_verticalPadding /= base.transform.localScale.y;
			((BoxCollider)_collider).size = new Vector3((float)width + 2f * _horizontalPadding, (float)height + 2f * _verticalPadding, 0f);
		}
	}

	public override void Left()
	{
		base.Left();
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		normal.ShowOrHide(show: true);
		over.ShowOrHide(show: false);
	}

	public override void Entered()
	{
		base.Entered();
		base.transform.localScale = overScale;
		normal.ShowOrHide(show: false);
		over.ShowOrHide(show: true);
	}

	public override void Released()
	{
		base.Released();
		string url = ((!UnityApi.UseGameClub()) ? SingletonT<ServerData>.I.GameSettings.FacebookCommunityUrl : SingletonT<ServerData>.I.GameSettings.FacebookCommunityUrlNew);
		Application.OpenURL(url);
	}
}
