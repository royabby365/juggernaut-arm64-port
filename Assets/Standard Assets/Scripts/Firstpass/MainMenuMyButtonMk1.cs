using UnityEngine;
using Yarx;

public class MainMenuMyButtonMk1 : SpriteButton
{
	public Color darkTint = new Color(0.5f, 0.5f, 0.5f);

	public Color neutralTint = new Color32(128, 128, 128, byte.MaxValue);

	public Vector3 overScale = new Vector3(1.1f, 1.1f, 1f);

	public Sprite normal;

	public Sprite over;

	private CompositeDisposable _subscriptions;

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

	private void HideMyComButton()
	{
		base.gameObject.SetActiveRecursively(state: false);
	}

	private void ShowMyComButton(bool showSign)
	{
		base.gameObject.SetActiveRecursively(state: true);
	}

	public new void Init()
	{
		base.Init();
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

	public void Start()
	{
		if (_subscriptions == null)
		{
			_subscriptions = new CompositeDisposable();
			_subscriptions.Add(Messenger<bool>.AddListener("MyComEvent", OnMyComEvent));
			Debug.Log("Adman add listener");
		}
		UnityApi.InitMyComAdman();
		UnityApi.MyComAdmanStart();
	}

	public void OnDestroy()
	{
		UnityApi.MyComAdmanStop();
		Utils.DisposeAndSetNull(ref _subscriptions);
		Debug.Log("Adman remove listener");
	}

	private void OnMyComEvent(bool admanResult)
	{
		Debug.Log("OnMyComEvent");
		ShowMyComButton(admanResult);
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
		OnMyComClick();
	}

	private void OnMyComClick()
	{
		HideMyComButton();
		UnityApi.MyComAdmanShow();
	}
}
