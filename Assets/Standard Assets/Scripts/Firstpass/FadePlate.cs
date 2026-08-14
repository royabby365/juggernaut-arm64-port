using UnityEngine;
using Yarx;

public class FadePlate : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText Text;

	public ServerData.PhrasesE Phrase
	{
		get
		{
			return Text.Phrase_;
		}
		set
		{
			Text.Phrase_ = value;
		}
	}
}
