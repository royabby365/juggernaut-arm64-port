using System;
using UnityEngine;

public class Foot : SpriteButton
{
	public enum State
	{
		Start,
		Show,
		Input,
		InputShow,
		Win,
		Lose
	}

	public Transform FootPlain;

	public Transform FootRed;

	public Transform Green;

	public Transform Orange;

	public Transform Red;

	private void Awake()
	{
		Init();
	}

	public void SetState(State state)
	{
		DisableAll();
		switch (state)
		{
		case State.Start:
			Orange.ShowOrHide(show: true);
			break;
		case State.Show:
			FootPlain.ShowOrHide(show: true);
			Orange.ShowOrHide(show: true);
			break;
		case State.Input:
			Green.ShowOrHide(show: true);
			break;
		case State.InputShow:
			SingletonT<SoundManager>.I.PlayGlobalSound("click_strong_magic");
			Green.ShowOrHide(show: true);
			FootPlain.ShowOrHide(show: true);
			break;
		case State.Win:
			Green.ShowOrHide(show: true);
			FootPlain.ShowOrHide(show: true);
			break;
		case State.Lose:
			Red.ShowOrHide(show: true);
			FootRed.ShowOrHide(show: true);
			break;
		default:
			throw new ArgumentOutOfRangeException("state");
		}
	}

	private void DisableAll()
	{
		FootPlain.ShowOrHide(show: false);
		FootRed.ShowOrHide(show: false);
		Orange.ShowOrHide(show: false);
		Green.ShowOrHide(show: false);
		Red.ShowOrHide(show: false);
		SetInactive();
	}
}
