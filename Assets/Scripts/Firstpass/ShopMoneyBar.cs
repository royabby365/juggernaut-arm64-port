using UnityEngine;
using Yarx;

public class ShopMoneyBar : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText DiamonsCount;

	public SpriteText MoneyCount;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.MoneyType.TypeE, string>.AddListener(Globals.MsgPlayerFundsChanged, FundsChanged));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
	}

	private void FundsChanged(ServerData.MoneyType.TypeE type, string reason)
	{
		string text_ = type.GetPlayerFundsCount().ToString();
		if (type == ServerData.MoneyType.TypeE.Gold)
		{
			MoneyCount.Text_ = text_;
		}
		if (type == ServerData.MoneyType.TypeE.Diamond)
		{
			DiamonsCount.Text_ = text_;
		}
	}
}
