using UnityEngine;
using Yarx;

public class GotLevelBanner : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText GotLevel;

	public SpriteText GotPoints;

	public SpriteText GotNewItems;

	private string _gotLevelFmt;

	private string _gotPointsFmt;

	private void Awake()
	{
		_gotLevelFmt = SingletonT<ServerData>.I.GetPhrase(GotLevel.Phrase_);
		GotLevel.Phrase_ = ServerData.PhrasesE.Custom;
		_gotPointsFmt = SingletonT<ServerData>.I.GetPhrase(GotPoints.Phrase_);
		GotPoints.Phrase_ = ServerData.PhrasesE.Custom;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int, int, string>.AddListener(Globals.MsgPlayerLevelChanged, OnPlayerLevelChanged));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	public void Init()
	{
		InitTextsForLevel(SingletonT<ServerData>.I.PlayerParams.Level - 1, SingletonT<ServerData>.I.PlayerParams.Level);
	}

	private void OnPlayerLevelChanged(int old, int @new, string reason)
	{
		InitTextsForLevel(old, @new);
	}

	private void InitTextsForLevel(int old, int @new)
	{
		GotLevel.Text_ = _gotLevelFmt.Fmt(@new);
		int playerSkillPoints = Extensions.GetPlayerSkillPoints();
		GotPoints.Text_ = _gotPointsFmt.Fmt(playerSkillPoints);
	}
}
