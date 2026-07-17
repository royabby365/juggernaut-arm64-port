using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class ExtraChapterHud : MonoBehaviour
{
	private Stack<GameObject> _icons = new Stack<GameObject>();

	private Stack<GameObject> _animatedIcons = new Stack<GameObject>();

	private CompositeDisposable _subscriptions;

	private string _openButtonTextFormat;

	private ServerData.Location _location;

	private GameObject _lootItem;

	private GameObject _highlightPrefab;

	public Transform IconsRoot;

	public SpriteText TextChapterName;

	public SpriteText TextOpenCondition;

	public SpriteText TextOpenButton;

	public GameObject OpenButton;

	public GameObject IconPrefab;

	public GameObject AnimatedIconPrefab;

	public GameObject LootItemPrefab;

	private void Awake()
	{
		_highlightPrefab = Util.Resource<GameObject>("_z_prefabs/combo_button_highlight");
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += ProcessButtons;
			HudMk1.Instance.DragEndWithButton += ProcessButtons;
		}
	}

	private void OnDisable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release -= ProcessButtons;
			HudMk1.Instance.DragEndWithButton -= ProcessButtons;
		}
		_subscriptions.Dispose();
	}

	public void Init(ServerData.Location location, int prevProgress)
	{
		if (_openButtonTextFormat.IsNullOrEmpty())
		{
			_openButtonTextFormat = SingletonT<ServerData>.I.GetPhrase(TextOpenButton.Phrase_);
			TextOpenButton.Phrase_ = ServerData.PhrasesE.Custom;
		}
		while (_animatedIcons.Count > 0)
		{
			GameObject obj = _animatedIcons.Pop();
			UnityEngine.Object.Destroy(obj);
		}
		_location = location;
		OpenCondition openCondition = location.Logic.OpenCondition;
		if (!openCondition.CheckProgress)
		{
			openCondition.CheckProgress = true;
			Globals.MainMenu.SaveGame();
		}
		TextChapterName.Text_ = location.Title;
		TextOpenCondition.Text_ = location.Logic.OpenCondition.GetInfo(location.OpenInfo);
		OpenButton.SetActiveRecursivelyMk1(location.Logic.OpenCondition.Progress < location.Logic.OpenCondition.MaxProgress);
		int maxProgress = openCondition.MaxProgress;
		while (_icons.Count < maxProgress)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(IconPrefab);
			gameObject.transform.parent = IconsRoot;
			_icons.Push(gameObject);
		}
		while (_icons.Count > maxProgress)
		{
			UnityEngine.Object.Destroy(_icons.Pop());
		}
		int num = 0;
		int num2 = 100;
		int num3 = -(maxProgress * num2) / 2 + num2 / 2;
		int num4 = Mathf.Min(openCondition.Progress, prevProgress);
		foreach (GameObject icon in _icons)
		{
			icon.transform.localPosition = new Vector3(num3 + num * num2, 0f, 0f);
			if (num >= num4)
			{
				icon.GetComponent<Sprite>().SpriteName_ = "extra_chapter_mob_bw";
			}
			else
			{
				icon.GetComponent<Sprite>().SpriteName_ = "extra_chapter_mob";
			}
			num++;
		}
		foreach (KeyValuePair<ServerData.MoneyType, int> item in location.OpenPrice)
		{
			if (item.Key.Type == ServerData.MoneyType.TypeE.Diamond)
			{
				TextOpenButton.Text_ = string.Format(_openButtonTextFormat, item.Value);
				break;
			}
		}
		if (location.Bonus.Drop != null && location.Bonus.Drop.Count > 0)
		{
			if (_lootItem == null)
			{
				_lootItem = (GameObject)UnityEngine.Object.Instantiate(LootItemPrefab);
				_lootItem.transform.parent = IconsRoot.transform.parent;
				_lootItem.transform.localPosition = new Vector3(0f, -42f, -100f);
			}
			ExtraItemPreview componentInChildren = _lootItem.GetComponentInChildren<ExtraItemPreview>();
			componentInChildren.SetLoot(location.Bonus.Drop[0]);
		}
		else if (_lootItem != null)
		{
			UnityEngine.Object.Destroy(_lootItem);
		}
		if (openCondition.Progress > openCondition.MaxProgress)
		{
			return;
		}
		if (prevProgress < openCondition.Progress)
		{
			for (int i = prevProgress; i < openCondition.Progress; i++)
			{
				GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(AnimatedIconPrefab);
				gameObject2.transform.parent = IconsRoot;
				gameObject2.transform.localPosition = new Vector3(num3 + i * num2, 0f, 0f);
				_animatedIcons.Push(gameObject2);
			}
			StartCoroutine(AnimateIcons());
		}
		num = 0;
		foreach (GameObject icon2 in _icons)
		{
			if (num < openCondition.MaxProgress && num >= openCondition.Progress)
			{
				GameObject gameObject3 = (GameObject)UnityEngine.Object.Instantiate(_highlightPrefab);
				gameObject3.transform.parent = icon2.transform;
				gameObject3.transform.localPosition = default(Vector3);
				StartCoroutine(ShowHighlight(gameObject3));
			}
			num++;
		}
	}

	private IEnumerator ShowHighlight(GameObject highlight)
	{
		float time = 0f;
		Vector3 scale = Vector3.one * 150f;
		while (time < 0.5f)
		{
			highlight.transform.localScale = Vector3.Lerp(Vector3.zero, scale, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		highlight.transform.localScale = scale;
		yield return new WaitForSeconds(2f);
		time = 0f;
		while (time < 0.5f)
		{
			highlight.transform.localScale = Vector3.Lerp(scale, Vector3.zero, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		highlight.transform.localScale = Vector3.zero;
		UnityEngine.Object.Destroy(highlight);
	}

	private IEnumerator AnimateIcons()
	{
		GuiRoot guiRoot = base.transform.root.GetComponentInChildren<GuiRoot>();
		float time = 0f;
		float duration = 1.5f;
		Vector3 startScale = default(Vector3);
		foreach (GameObject item in _animatedIcons)
		{
			item.transform.localScale = startScale;
			Animation anim = item.GetComponentInChildren<Animation>();
			anim.Stop();
			anim["Take 001"].time = 0f;
		}
		yield return new WaitForSeconds(0.5f);
		foreach (GameObject item2 in _animatedIcons)
		{
			Animation anim2 = item2.GetComponentInChildren<Animation>();
			anim2.Play();
		}
		while (time < duration)
		{
			foreach (GameObject item3 in _animatedIcons)
			{
				item3.transform.localScale = Vector3.Lerp(startScale, Vector3.one, time / duration);
			}
			time += Time.deltaTime;
			yield return null;
		}
		foreach (GameObject item4 in _animatedIcons)
		{
			item4.transform.localScale = Vector3.one;
		}
	}

	private void ProcessButtons(SpriteButton button)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		HudMk1.GuiDesc currentGui = HudMk1.Instance.CurrentGui;
		if (currentGui.Type != GuiRoot.GuiType.ExtraChapterInfo && currentGui.Type != GuiRoot.GuiType.ExtraChapterCongratulations)
		{
			return;
		}
		switch (button.name)
		{
		case "button_extra_chapter_close":
			if (currentGui.Type == GuiRoot.GuiType.ExtraChapterInfo && _location.Logic != null && _location.Logic.OpenCondition != null && _location.Logic.OpenCondition.Done)
			{
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.ExtraChapterCongratulations);
				break;
			}
			if (currentGui.Type == GuiRoot.GuiType.ExtraChapterInfo)
			{
				HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.ExtraChapterInfo);
			}
			else if (currentGui.Type == GuiRoot.GuiType.ExtraChapterCongratulations)
			{
				HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.ExtraChapterCongratulations);
			}
			Messenger.Invoke(Globals.MsgGuiExitExtraChapter);
			break;
		case "button_extra_chapter_buy":
			if (Globals.IsDebugBuild)
			{
				Debug.Log("===== button_extra_chapter_buy");
			}
			if (SingletonT<ServerData>.I.BuyLocationOpenCondition(_location))
			{
				Globals.MainMenu.SaveGame();
				if (Globals.Battle != null && Globals.Battle.BattleGui != null && Globals.Battle.BattleGui.BattleResultContinueWasClicked)
				{
					Globals.MainMenu.AddOpenCondition(MainMenu.EventTypeE.ExtraChapterCongrats, 0);
					Messenger.Invoke(Globals.MsgGuiExitExtraChapter);
				}
				else
				{
					HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.ExtraChapterCongratulations);
				}
			}
			else
			{
				Messenger<ServerData.PhrasesE, ServerData.PhrasesE, ServerData.PhrasesE, Action>.Invoke(Globals.MsgPopup2ButtonYesHandler, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, ServerData.PhrasesE.InsufficientFunds, delegate
				{
					HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Bank);
				});
			}
			break;
		}
	}
}
