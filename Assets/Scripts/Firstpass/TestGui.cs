using UnityEngine;

public class TestGui : SpriteGui
{
	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
		foreach (SpriteButton value in _buttons.Values)
		{
			value.SetActive();
			if (Globals.IsDebugBuild)
			{
				Debug.Log("@@ " + value.name);
			}
		}
		base.Release += ProcessButtons;
	}

	private void Update()
	{
		ProcessRayCast();
	}

	private void ProcessButtons(SpriteButton button)
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log($"[PRESSED: {button.name}]");
		}
	}
}
