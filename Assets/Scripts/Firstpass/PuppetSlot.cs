using System;
using System.Collections;
using UnityEngine;
using Yarx.Collections;

public class PuppetSlot : SpriteButton
{
	public ServerData.Slot.TypeE Slot;

	public StarSlotButton StarSlot;

	private GameObject _highlightPrefab;

	private GameObject _higlite;

	private void Awake()
	{
		Init();
		SetUnselected();
		StarSlot.MySlot = this;
		_highlightPrefab = Util.Resource<GameObject>("_z_prefabs/combo_button_highlight");
	}

	public void TurnBgOff(bool off)
	{
		GetComponent<Renderer>().ShowOrHide(!off);
	}

	public override void SetSelected()
	{
		base.SetSelected();
		_higlite = (GameObject)UnityEngine.Object.Instantiate(_highlightPrefab);
		_higlite.transform.parent = base.transform;
		_higlite.transform.localPosition = new Vector3(52f, -52f, 0f);
		StartCoroutine("ShowHighlight", _higlite);
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
		if (_higlite != null)
		{
			GameObject higlite = _higlite;
			higlite.Eliminate();
			_higlite = null;
		}
	}

	private IEnumerator ShowHighlight(GameObject highlight)
	{
		float time = 0f;
		Vector3 scale = Vector3.one * 140f;
		while (time < 0.5f)
		{
			if (_higlite == null)
			{
				yield break;
			}
			highlight.transform.localScale = Vector3.Lerp(Vector3.zero, scale, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		if (_higlite != null)
		{
			highlight.transform.localScale = scale;
		}
		yield return new WaitForSeconds(1f);
		while (time < 1.5f)
		{
			if (_higlite == null)
			{
				yield break;
			}
			highlight.transform.localScale = Vector3.Lerp(scale, Vector3.zero, time / 1.5f);
			time += Time.deltaTime;
			yield return null;
		}
		if (_higlite != null)
		{
			highlight.transform.localScale = Vector3.zero;
		}
		SetUnselected();
	}

	public bool ActivateUpgrade(BagItemButton ib, bool activate)
	{
		if (ib != null && activate && ib.item.CurrentStars > ib.item.MaxStars)
		{
			int delta = ib.item.CurrentStars - ib.item.MaxStars;
			ib.item.CurrentStars = ib.item.MaxStars;
			ServerData.MoneyType.TypeE.Star.ChangePlayerFundsCount(delta);
			Globals.MainMenu.SaveGame();
		}
		if (ib == null || !activate || ib.item.MaxStars <= 0)
		{
			StarSlot.SetInactive();
			SetUnselected();
			return false;
		}
		StarSlot.SetActive();
		SetSelected();
		StarSlot.SetCount(ib.item.CurrentStars, ib.item.MaxStars);
		return true;
	}

	public void UpgradeStars()
	{
		if (HudMk1.Instance == null || !(BagInventory.Instance != null))
		{
			return;
		}
		BagItemButton putOnItemFromSlot = BagInventory.Instance.GetPutOnItemFromSlot(Slot);
		if (!(putOnItemFromSlot != null))
		{
			return;
		}
		int currentStars = putOnItemFromSlot.item.CurrentStars;
		int maxStars = putOnItemFromSlot.item.MaxStars;
		int num = maxStars - currentStars;
		if (num <= 0)
		{
			return;
		}
		if (SingletonT<ServerData>.I.PlayerParams.StarsCount <= 0)
		{
			Messenger<ServerData.PhrasesE, ServerData.PhrasesE, ServerData.PhrasesE, Action>.Invoke(Globals.MsgPopup2ButtonYesHandler, ServerData.PhrasesE.ButtonBuy, ServerData.PhrasesE.ButtonCancel, ServerData.PhrasesE.NeedMoreStars, delegate
			{
				Messenger.Invoke(Globals.MsgShopStarsInsufficient);
				HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Shop);
			});
			BagInventory.Instance.ActivateUpgrade(activate: false);
			return;
		}
		int num2 = Mathf.Min(num, SingletonT<ServerData>.I.PlayerParams.StarsCount);
		ServerData.MoneyType.TypeE.Star.ChangePlayerFundsCount(-num2);
		currentStars += num2;
		putOnItemFromSlot.item.CurrentStars = currentStars;
		putOnItemFromSlot.SetUpgradeProgress(currentStars, maxStars);
		StartCoroutine("SetCountCoro", System.Tuple.Create(currentStars, maxStars));
		Messenger<GameObject>.Invoke(Globals.MsgStarIncreased, StarSlot.Star.gameObject);
		if (currentStars >= maxStars)
		{
			putOnItemFromSlot.UpgradeItem();
		}
	}

	private IEnumerator SetCountCoro(System.Tuple<int, int> minmax)
	{
		yield return new WaitForSeconds(1.1f);
		StarSlot.SetCount(minmax.Item1, minmax.Item2);
		yield return new WaitForSeconds(1.1f);
		BagInventory.Instance.ActivateUpgrade(activate: false);
	}
}
