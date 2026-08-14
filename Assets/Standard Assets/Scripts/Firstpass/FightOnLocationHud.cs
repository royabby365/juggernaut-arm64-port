using System.Text;
using UnityEngine;
using Yarx;

public class FightOnLocationHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText ChapterInfo;

	public SpriteText MobInfo;

	public GameObject FightButton;

	internal AreaData.MobData MobData;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	internal void Init(AreaData.MobData mob)
	{
		MobData = mob;
		if (!string.IsNullOrEmpty(ChapterInfo.Text))
		{
			ChapterInfo.Text_ = string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(SingletonT<ServerData>.I.GetPhrase(ServerData.PhraseInBattleFightWith) + " " + mob.ServerInfo.Title);
		if (mob.Darkness || mob.Lighting || mob.Fire || mob.Ice)
		{
			stringBuilder.Append(" [ ");
			if (mob.Darkness)
			{
				stringBuilder.Append(Globals.CharIconDark);
			}
			if (mob.Lighting)
			{
				stringBuilder.Append(Globals.CharIconElectro);
			}
			if (mob.Fire)
			{
				stringBuilder.Append(Globals.CharIconFire);
			}
			if (mob.Ice)
			{
				stringBuilder.Append(Globals.CharIconIce);
			}
			stringBuilder.Append(" ]");
		}
		MobInfo.Text_ = stringBuilder.ToString();
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.FightOnLocation)
		{
			FightButton.SetActiveRecursivelyMk1(setActive: true);
		}
	}
}
