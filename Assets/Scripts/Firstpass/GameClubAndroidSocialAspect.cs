using System;
using UnityEngine;

public class GameClubAndroidSocialAspect : SocialAspect
{
	private bool AuthenticationRunning;

	private bool Authenticated;

	private GameClub.OnAuthenticated OnAuthCallbacks;

	internal GameClubAndroidSocialAspect(MonoBehaviour behaviour)
	{
		Utils.Log("SOCIAL CREATE");
		Messenger<GameEvents.Event, string>.AddListener(Globals.MsgGameEventProgressChanged, delegate(GameEvents.Event @event, string reason)
		{
			Utils.Log("Globals.MsgGameEventProgressChanged");
			if (!(reason != "ProgressChanged"))
			{
				WaitAuthentication(delegate
				{
					if (Authenticated)
					{
						UpdateAchivStateToSocial(@event);
						UpdateRatingAchivsScore();
					}
				});
			}
		});
		Messenger<string>.AddListener(Globals.MsgOpenRatings, delegate(string _)
		{
			Utils.Log("Globals.MsgOpenRatings");
			WaitAuthentication(delegate
			{
				if (Authenticated)
				{
					Utils.LogForce("ShowRatingUI", _);
					GameClub.ShowLeaderboardUI(_);
				}
			});
		});
		Messenger.AddListener(Globals.MsgAddFriends, delegate
		{
			Utils.Log("Globals.MsgAddFriends");
			WaitAuthentication(delegate
			{
				if (Authenticated)
				{
					Utils.Log("ShowAddFriendsUI");
					GameClub.ShowAddFriendsUI();
				}
			});
		});
	}

	public override void ProcessAuthentication(ActionD<bool> action)
	{
		RunAuthentication();
		WaitAuthentication(delegate
		{
			action(Authenticated);
		});
	}

	private void RunAuthentication()
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log("RunAuthentication");
		}
		AuthenticationRunning = true;
		GameClub.Authenticate(delegate
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("Authenticated");
			}
			OnAuthenticationFinished(authenticated: true);
		}, delegate(Exception err)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("Failed to authenticate: " + err.ToString());
			}
			OnAuthenticationFinished(authenticated: false);
		});
	}

	private void OnAuthenticationFinished(bool authenticated)
	{
		AuthenticationRunning = false;
		Authenticated = authenticated;
		if (OnAuthCallbacks != null)
		{
			OnAuthCallbacks();
		}
		OnAuthCallbacks = null;
	}

	private void WaitAuthentication(GameClub.OnAuthenticated cb)
	{
		if (AuthenticationRunning)
		{
			if (OnAuthCallbacks != null)
			{
				OnAuthCallbacks = (GameClub.OnAuthenticated)Delegate.Combine(OnAuthCallbacks, cb);
			}
			else
			{
				OnAuthCallbacks = cb;
			}
		}
		else
		{
			cb();
		}
	}

	public override void SyncWithExternal()
	{
		Utils.LogForce("SyncWithExternal");
		WaitAuthentication(delegate
		{
			if (!Authenticated)
			{
			}
		});
	}

	public override string GetLeaderboardId()
	{
		return string.Empty;
	}

	public override string GetLeaderboardCaveId()
	{
		return string.Empty;
	}

	private string GetAchivSocialId(ServerData.Achievement achiv)
	{
		return string.Empty;
	}

	private void UpdateAchivStateToSocial(GameEvents.Event @event)
	{
		ServerData.Achievement achievement = @event.Achievement;
		if (achievement != null)
		{
			string achivSocialId = GetAchivSocialId(achievement);
			if (!string.IsNullOrEmpty(achivSocialId))
			{
			}
		}
	}

	private void UpdateRatingAchivsScore()
	{
		int allFinishedAchivsPoints = MainMenu.GameEvents.AllFinishedAchivsPoints;
		int num = 0;
		if (PlayerPrefs.HasKey("Match3Record"))
		{
			num = PlayerPrefs.GetInt("Match3Record");
		}
		else
		{
			PlayerPrefs.SetInt("Match3Record", num);
		}
		Utils.Log("SCORE", allFinishedAchivsPoints, num);
		if (allFinishedAchivsPoints > 0)
		{
		}
		if (num <= 0)
		{
		}
	}

	public override void GetFriendsScores(ActionD<ScoresInfo[]> action, ActionD onError)
	{
		WaitAuthentication(delegate
		{
			if (!Authenticated)
			{
				Utils.LogForce("GetFriendsScores. FAILED TO CONNECT");
				onError();
			}
			else
			{
				Utils.LogForce("GetFriendsScores connected");
			}
		});
	}

	public override void GetFriendsCaveScores(ActionD<ScoresInfo[]> action, ActionD onError)
	{
		WaitAuthentication(delegate
		{
			if (!Authenticated)
			{
				Utils.LogForce("GetFriendsCaveScores. FAILED TO CONNECT");
				onError();
			}
			else
			{
				Utils.LogForce("GetFriendsCaveScores connected");
			}
		});
	}
}
