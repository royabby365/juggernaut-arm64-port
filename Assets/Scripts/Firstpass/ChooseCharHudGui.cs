using UnityEngine;
using Yarx;

public class ChooseCharHudGui : SpriteGui
{
	private CompositeDisposable _subscriptions;

	public static ChooseCharHudGui Instance;

	private void Awake()
	{
		if (Instance != null && Globals.IsDebugBuild)
		{
			Debug.LogWarning("=============== ChooseCharGui NOT a singleton! =================");
		}
		Instance = this;
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
		foreach (SpriteButton value in _buttons.Values)
		{
			value.SetActive();
		}
	}

	private void Update()
	{
		if (base.enabled)
		{
			ProcessRayCast();
		}
	}
}
