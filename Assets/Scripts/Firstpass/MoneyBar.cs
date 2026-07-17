using System;
using UnityEngine;
using Yarx;

public class MoneyBar : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText DiamonsCount;

	public SpriteText KeysCount;

	public SpriteText MoneyCount;

	public SpriteText SkullsCount;

	public SpriteText ScarabsCount;

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
		switch (type)
		{
		case ServerData.MoneyType.TypeE.Gold:
			MoneyCount.Text_ = text_;
			break;
		case ServerData.MoneyType.TypeE.Diamond:
			DiamonsCount.Text_ = text_;
			break;
		case ServerData.MoneyType.TypeE.Key:
			KeysCount.Text_ = text_;
			break;
		case ServerData.MoneyType.TypeE.Skull:
			SkullsCount.Text_ = text_;
			break;
		case ServerData.MoneyType.TypeE.Scarab:
			ScarabsCount.Text_ = text_;
			break;
		case ServerData.MoneyType.TypeE.Star:
			break;
		default:
			throw new ArgumentOutOfRangeException("type");
		}
	}
}
