using UnityEngine;
using Yarx;

public class ElfPopupHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText MessageText;

	private void Start()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += Instance_Release;
			HudMk1.Instance.DragEndWithButton += Instance_Release;
		}
	}

	private void Instance_Release(SpriteButton obj)
	{
		if (!(HudMk1.Instance == null))
		{
			switch (obj.name)
			{
			case "button_elf_attack":
				HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.ElfPopup);
				Messenger.Invoke(Globals.MsgAttackElf);
				break;
			case "button_elf_cancel":
				HudMk1.Instance.AddOrRemoveGui(add: false, GuiRoot.GuiType.ElfPopup);
				break;
			}
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.PhrasesE>.AddListener(Globals.MsgShowElf, OnMsgShowElf));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void OnMsgShowElf(ServerData.PhrasesE phrase)
	{
		MessageText.Phrase_ = phrase;
		SingletonT<SoundManager>.I.PlayGlobalSound("Jug_achievment");
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.AddOrRemoveGui(add: true, GuiRoot.GuiType.ElfPopup);
		}
	}
}
