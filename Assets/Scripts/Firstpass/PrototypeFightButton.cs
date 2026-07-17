using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Yarx;

public class PrototypeFightButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public Sprite ButtonSprite;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action Press;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action Release;

	private void OnPress()
	{
		this.Press?.Invoke();
	}

	private void OnRelease()
	{
		this.Release?.Invoke();
	}

	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		SetActive();
	}

	private void Update()
	{
	}

	public override void Released()
	{
		base.Released();
		OnRelease();
	}

	public override void Clicked()
	{
		base.Clicked();
		OnPress();
	}

	public override void Entered()
	{
		base.Entered();
		base.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
	}

	public override void Left()
	{
		base.Left();
		base.transform.localScale = Vector3.one;
	}
}
