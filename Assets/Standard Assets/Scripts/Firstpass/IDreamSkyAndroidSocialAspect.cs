using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class IDreamSkyAndroidSocialAspect : SocialAspect
{
	private bool AuthenticationRunning;

	private bool Authenticated;

	private IDreamSky.OnAuthenticated OnAuthCallbacks;

	internal IDreamSkyAndroidSocialAspect(MonoBehaviour behaviour)
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
					IDreamSky.ShowLeaderboardUI(_);
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
					IDreamSky.ShowAddFriendsUI();
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
		IDreamSky.Authenticate(delegate
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

	private void WaitAuthentication(IDreamSky.OnAuthenticated cb)
	{
		if (AuthenticationRunning)
		{
			if (OnAuthCallbacks != null)
			{
				OnAuthCallbacks = (IDreamSky.OnAuthenticated)Delegate.Combine(OnAuthCallbacks, cb);
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
			if (Authenticated)
			{
				IDreamSky.LoadAchievements(OnAchievementsLoaded, delegate(Exception err)
				{
					Utils.LogForce("SYNC ACHIVS FAILED", err.ToString());
				});
			}
		});
	}

	private void OnAchievementsLoaded(IDreamSky.Achievement[] achivs)
	{
		Utils.LogForce("SYNC ACHIVS STARTED", achivs.Length);
		List<GameEvents.Event> list = new List<GameEvents.Event>();
		try
		{
			bool flag = false;
			GameEvents gameEvents = MainMenu.GameEvents;
			Utils.LogForce("SYNCACHIVS1", gameEvents, SingletonT<ServerData>.I._achievements.Count, achivs.Length);
			StringBuilder stringBuilder = new StringBuilder(achivs.Length * 128 + SingletonT<ServerData>.I._achievements.Count * 128);
			if (gameEvents != null)
			{
				foreach (KeyValuePair<int, ServerData.Achievement> achievement2 in SingletonT<ServerData>.I._achievements)
				{
					stringBuilder.AppendLine("SERVERDATA: {0} {1}".Fmt(achievement2.Value.Id.ToString(), achievement2.Value.Title));
				}
				foreach (IDreamSky.Achievement achievement in achivs)
				{
					bool flag2 = false;
					foreach (KeyValuePair<int, ServerData.Achievement> achievement3 in SingletonT<ServerData>.I._achievements)
					{
						if (!(GetAchivSocialId(achievement3.Value) == achievement.id))
						{
							continue;
						}
						flag2 = true;
						GameEvents.Event eventByAchievmentId = gameEvents.GetEventByAchievmentId(achievement3.Value.Id);
						if (eventByAchievmentId != null)
						{
							stringBuilder.AppendLine("SYNC ACHIV {0} {1} {2} {3} {4} {5}".Fmt(achievement.id, achievement3.Value.Id, eventByAchievmentId.ProgressDouble, eventByAchievmentId.Progress, eventByAchievmentId.MaxProgress, achievement.percentCompleted));
							if (eventByAchievmentId.ProgressDouble < achievement.percentCompleted)
							{
								if (eventByAchievmentId.SetPercentCompleted(achievement.percentCompleted))
								{
									stringBuilder.AppendLine("  changed");
									flag = true;
								}
							}
							else if (eventByAchievmentId.ProgressDouble > achievement.percentCompleted)
							{
								list.Add(eventByAchievmentId);
							}
						}
						else
						{
							stringBuilder.AppendLine("CANT FIND ACHIV in SERVER DATA {0} {1}".Fmt(achievement.id, achievement3.Value.Id));
						}
						break;
					}
					if (!flag2)
					{
						stringBuilder.AppendLine("CANT FIND ACHIV " + achievement.id);
					}
				}
			}
			stringBuilder.AppendLine("isChanged {0} {1}".Fmt(flag, list.Count));
			Utils.LogForce(stringBuilder.ToString());
			if (flag)
			{
				MainMenu.GameEvents.SaveProgress();
			}
		}
		catch (Exception ex)
		{
			Utils.LogForce("SyncAchiv FAILED", ex.Message);
		}
		Utils.LogForce("SYNC ACHIVS FINISHED");
		if (list.Count > 0)
		{
			foreach (GameEvents.Event item in list)
			{
				UpdateAchivStateToSocial(item);
			}
		}
		UpdateRatingAchivsScore();
	}

	public override string GetLeaderboardId()
	{
		return Globals.GetIDreamSkyLeaderboardId();
	}

	public override string GetLeaderboardCaveId()
	{
		return Globals.GetIDreamSkyLeaderboardCaveId();
	}

	private string GetAchivSocialId(ServerData.Achievement achiv)
	{
		return Globals.GetIDreamSkyAchievmentId(achiv.Id.ToString());
	}

	private void UpdateAchivStateToSocial(GameEvents.Event @event)
	{
		ServerData.Achievement achievement = @event.Achievement;
		if (achievement == null)
		{
			return;
		}
		string achivId = GetAchivSocialId(achievement);
		if (!string.IsNullOrEmpty(achivId))
		{
			IDreamSky.ReportProgress(achivId, @event.ProgressDouble, delegate
			{
				Utils.Log("**ACHIV PROGRESS SENT", achivId);
			}, delegate(Exception err)
			{
				Utils.Log("**ACHIVS PROGRESS FAILED", achivId, err.ToString());
			});
		}
	}

	private void UpdateRatingAchivsScore()
	{
		int score = MainMenu.GameEvents.AllFinishedAchivsPoints;
		int caveScore = 0;
		if (PlayerPrefs.HasKey("Match3Record"))
		{
			caveScore = PlayerPrefs.GetInt("Match3Record");
		}
		else
		{
			PlayerPrefs.SetInt("Match3Record", caveScore);
		}
		Utils.Log("SCORE", score, caveScore);
		if (score > 0)
		{
			IDreamSky.ReportScore(GetLeaderboardId(), score, delegate
			{
				Utils.Log("**ACHIVS SCORE PROGRESS SENT", score);
			}, delegate(Exception err)
			{
				Utils.Log("**ACHIVS SCORE PROGRESS FAILED", score, err.ToString());
			});
		}
		if (caveScore > 0)
		{
			IDreamSky.ReportScore(GetLeaderboardCaveId(), caveScore, delegate
			{
				Utils.Log("**ACHIVS CAVE SCORE PROGRESS SENT", caveScore);
			}, delegate(Exception err)
			{
				Utils.Log("**ACHIVS CAVE SCORE PROGRESS FAILED", caveScore, err.ToString());
			});
		}
	}

	private IEnumerator ProcessScores(IDreamSky.Score[] scores, bool saveMyScoreInPrefs, ActionD<ScoresInfo[]> action)
	{
		string meUserId = IDreamSky.GetLocalUserId();
		StringBuilder sb1 = new StringBuilder(1024);
		foreach (IDreamSky.Score sc in scores)
		{
			sb1.AppendLine("  {0} {1} {2} {3}".Fmt(sc.userId, sc.userName, sc.rank, sc.score));
		}
		Utils.LogForce("ALL RESULTS", sb1.ToString());
		int meIndex = 0;
		for (int j = 0; j < scores.Length; j++)
		{
			if (!(scores[j].userId == meUserId))
			{
				continue;
			}
			meIndex = j;
			long myScore = scores[j].score;
			if (!saveMyScoreInPrefs)
			{
				continue;
			}
			if (PlayerPrefs.HasKey("Match3Record"))
			{
				int savedScore = PlayerPrefs.GetInt("Match3Record");
				if (savedScore < myScore)
				{
					PlayerPrefs.SetInt("Match3Record", (int)myScore);
				}
			}
			else
			{
				PlayerPrefs.SetInt("Match3Record", (int)myScore);
			}
		}
		int mn = Math.Max(0, meIndex - 4);
		int mx = Math.Min(meIndex + 4, scores.Length - 1);
		while (mx - mn > 5)
		{
			if (mx - meIndex > meIndex - mn)
			{
				mx--;
			}
			else
			{
				mn++;
			}
		}
		List<ScoresInfo> r = new List<ScoresInfo>();
		for (int k = mn; k <= mx; k++)
		{
			IDreamSky.Score sc2 = scores[k];
			WWW www = new WWW(sc2.userImageUrl);
			yield return www;
			r.Add(new ScoresInfo(sc2.userId, sc2.userName, www.texture, sc2.score, sc2.userId == meUserId));
		}
		StringBuilder sb2 = new StringBuilder(1024);
		foreach (ScoresInfo sci in r)
		{
			sb2.AppendLine("  {0} {1} {2} {3} {4}".Fmt(sci.Id, sci.UserName, sci.Score, (!(sci.Image != null)) ? (-1) : sci.Image.width, (!(sci.Image != null)) ? (-1) : sci.Image.height));
		}
		Utils.LogForce("RESULT", sb2.ToString());
		action(r.ToArray());
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
				IDreamSky.LoadScores(GetLeaderboardId(), 1, delegate(IDreamSky.Score[] scores)
				{
					Utils.LogForce("GetFriendsScores scores loaded", scores.Length);
					Globals.MainMenu.StartCoroutine(ProcessScores(scores, saveMyScoreInPrefs: false, action));
				}, delegate(Exception err)
				{
					Utils.LogForce("GetFriendsScores. FAILED TO LOAD SCORES", err.ToString());
					onError();
				});
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
				IDreamSky.LoadScores(GetLeaderboardCaveId(), 1, delegate(IDreamSky.Score[] scores)
				{
					Utils.LogForce("GetFriendsCaveScores scores loaded", scores.Length);
					Globals.MainMenu.StartCoroutine(ProcessScores(scores, saveMyScoreInPrefs: true, action));
				}, delegate(Exception err)
				{
					Utils.LogForce("GetFriendsCaveScores. FAILED TO LOAD SCORES", err.ToString());
					onError();
				});
			}
		});
	}
}
