using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class SoundManager : SingletonT<SoundManager>
{
	private enum WaitLoadingScreenModeE
	{
		None,
		Boss,
		Enemy
	}

	private class Data
	{
		public enum StateE
		{
			None,
			Current,
			Down
		}

		public AudioSource Source;

		public AudioClip Clip;

		public StateE State;

		public string UserId;

		public string Id;
	}

	private static readonly float MusikK = 0.5f;

	private static int counter = 1;

	private WaitLoadingScreenModeE _waitLoadingScreenHide;

	private Data[] _data = new Data[2]
	{
		new Data(),
		new Data()
	};

	private AudioClip _proxy = AudioClip.Create("_proxy", 1, 1, 44100, false);

	private int _lastBattleMusic = -1;

	private string _lastPlayedMusicId;

	private IDisposable _lastBattleMusicTimer;

	private int _lastClickPopupSoundId = 1;

	private int _lastMoneyPaySoundId = 1;

	private float _soundVolume = 1f;

	private Dictionary<string, ResourcesManager.AssetBundleData> _soundsBundles = new Dictionary<string, ResourcesManager.AssetBundleData>();

	public SoundManager()
	{
		Messenger.AddListener(Globals.MsgLoadingScreenHided, OnMsgLoadingScreenHided);
	}

	private Data GetFreeData()
	{
		if (_data[0].Clip == null)
		{
			return _data[0];
		}
		return _data[1];
	}

	private Data GetCurrentData(Data data)
	{
		if (_data[0] == data && _data[0].Clip != null)
		{
			return _data[1];
		}
		if (_data[1] == data && _data[1].Clip != null)
		{
			return _data[0];
		}
		return null;
	}

	private void StopMusicCoroutine(Data data)
	{
		if (data.State == Data.StateE.Down)
		{
			Globals.MainMenu.StopCoroutine("StopPlayMusic");
			StopData(data);
		}
		if (data.State == Data.StateE.Current)
		{
			Globals.MainMenu.StopCoroutine("StartPlayMusic");
		}
	}

	private void StopMusicCoroutines()
	{
		StopMusicCoroutine(_data[0]);
		StopMusicCoroutine(_data[1]);
	}

	private AudioClip GetGlobalSound(string name)
	{
		string path = "sounds/" + name;
		return Util.Resource<AudioClip>(path);
	}

	private AudioClip PlayMusic(bool forceChange, string userId, string path, string name, ActionD onCached)
	{
		if (!Globals.IsPlayMusic || Globals.DebugDontLoadAndPlayMusic)
		{
			onCached?.Invoke();
			return null;
		}
		if (!forceChange)
		{
			Data[] data = _data;
			foreach (Data data2 in data)
			{
				if (data2.UserId == userId && data2.State == Data.StateE.Current)
				{
					onCached?.Invoke();
					return data2.Clip;
				}
			}
		}
		StopMusicCoroutines();
		Data freeData = GetFreeData();
		freeData.Clip = _proxy;
		AudioClip globalSound = GetGlobalSound(name);
		if (globalSound == null)
		{
			Utils.Log("PlayMusic failed", userId, path, name);
			return null;
		}
		freeData.Clip = globalSound;
		freeData.UserId = userId;
		counter++;
		Globals.MainMenu.StartCoroutine(StartPlayMusic(userId + " " + counter, freeData));
		Data currentData = GetCurrentData(freeData);
		Utils.Log("music", name, globalSound, currentData.UserId, globalSound.length);
		if (currentData != null)
		{
			Globals.MainMenu.StartCoroutine(StopPlayMusic(currentData));
		}
		onCached?.Invoke();
		return globalSound;
	}

	private IEnumerator StartPlayMusic(string id, Data data)
	{
		if (data.Clip == null)
		{
			data.State = Data.StateE.None;
			yield break;
		}
		if (data.Source == null)
		{
			data.Source = Globals.MainMenu.gameObject.AddComponent<AudioSource>();
			data.Source.loop = true;
			data.Source.playOnAwake = false;
		}
		data.Source.clip = data.Clip;
		data.State = Data.StateE.Current;
		data.Id = id;
		data.Source.volume = 0f;
		float musicVolume = ((SingletonT<ServerData>.I.GameSettings == null) ? 1f : SingletonT<ServerData>.I.GameSettings.MusicVolume) * MusikK;
		float growTime = ((SingletonT<ServerData>.I.GameSettings == null) ? 2f : SingletonT<ServerData>.I.GameSettings.MusicChangeTime);
		float speed = musicVolume / growTime;
		_lastPlayedMusicId = id;
		data.Source.Play();
		while (data.Source.volume < musicVolume)
		{
			float n = data.Source.volume + Time.deltaTime / speed;
			growTime -= Time.deltaTime;
			if (n > musicVolume)
			{
				n = musicVolume;
			}
			data.Source.volume = n;
			yield return null;
		}
		data.State = Data.StateE.Current;
	}

	private IEnumerator StopPlayMusic(Data data)
	{
		if (data.Clip != null && data.Source != null)
		{
			data.State = Data.StateE.Down;
			float time = SingletonT<ServerData>.I.GameSettings.MusicChangeTime;
			float speed = SingletonT<ServerData>.I.GameSettings.MusicVolume * MusikK / time;
			while (data.Source.volume > 0f)
			{
				float n = data.Source.volume - Time.deltaTime / speed;
				time -= Time.deltaTime;
				if (n < 0f)
				{
					n = 0f;
				}
				data.Source.volume = n;
				yield return null;
			}
		}
		StopData(data);
	}

	private void StopData(Data data)
	{
		if (data.Clip != null && data.Source != null)
		{
			data.Source.Stop();
			data.Source.clip = null;
		}
		data.Clip = null;
		data.State = Data.StateE.None;
	}

	private void OnMsgLoadingScreenHided()
	{
		if (_waitLoadingScreenHide != WaitLoadingScreenModeE.None)
		{
			WaitLoadingScreenModeE waitLoadingScreenHide = _waitLoadingScreenHide;
			_waitLoadingScreenHide = WaitLoadingScreenModeE.None;
			switch (waitLoadingScreenHide)
			{
			case WaitLoadingScreenModeE.Enemy:
				PlayBattleMusicImpl(null, forceChange: false);
				break;
			case WaitLoadingScreenModeE.Boss:
				PlayBossMusicImpl();
				break;
			}
		}
	}

	public void PlayBattleMusic()
	{
		LoadingScreen loadingScreen = Utils.FindObjectOfTypeNoThrow<LoadingScreen>();
		if (loadingScreen != null && loadingScreen.IsVisible)
		{
			_waitLoadingScreenHide = WaitLoadingScreenModeE.Enemy;
		}
		else
		{
			PlayBattleMusicImpl(null, forceChange: false);
		}
	}

	private void PlayBattleMusicImpl(ActionD action, bool forceChange)
	{
		_lastBattleMusic = Utils.GetRandom(1, 5, _lastBattleMusic);
		AudioClip audioClip = PlayMusic(forceChange, "PlayBattleMusic", "sounds/inbattle" + _lastBattleMusic, "Jugger battle theme " + _lastBattleMusic + "96k", action);
		if (!(audioClip != null))
		{
			return;
		}
		Utils.Dispose(ref _lastBattleMusicTimer);
		_lastBattleMusicTimer = SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(audioClip.length, delegate
		{
			Utils.Log("TIMER MUSIC", _lastPlayedMusicId);
			if (_lastPlayedMusicId == "PlayBattleMusic")
			{
				PlayBattleMusicImpl(null, forceChange: true);
			}
		});
	}

	public void PlayLocationMusic(ActionD action)
	{
		Utils.Dispose(ref _lastBattleMusicTimer);
		PlayMusic(forceChange: false, "PlayLocationMusic", "sounds/location", "Jugger location new96k", action);
	}

	public void PlayInBattleBossMusic()
	{
		Utils.Dispose(ref _lastBattleMusicTimer);
		LoadingScreen loadingScreen = Utils.FindObjectOfTypeNoThrow<LoadingScreen>();
		if (loadingScreen != null && loadingScreen.IsVisible)
		{
			_waitLoadingScreenHide = WaitLoadingScreenModeE.Boss;
		}
		else
		{
			PlayBossMusicImpl();
		}
	}

	private void PlayBossMusicImpl()
	{
		PlayMusic(forceChange: false, "PlayInBattleBossMusic", "sounds/inbattle_boss", "Jugger battle BOSS96k", null);
	}

	public void PlayMenuMusic(ActionD action)
	{
		Utils.Dispose(ref _lastBattleMusicTimer);
		PlayMusic(forceChange: false, "PlayMenuMusic", "sounds/inmenu", "Jugger menu 96k", action);
	}

	public void ForcePlayMenuMusic(ActionD action)
	{
		Utils.Dispose(ref _lastBattleMusicTimer);
		PlayMusic(forceChange: true, "PlayMenuMusic", "sounds/inmenu", "Jugger menu 96k", action);
	}

	internal void PlaySound(string soundName)
	{
		PlaySound(Globals.Player, soundName, 0f, null, isParent: false, Vector3.zero);
	}

	internal void PlayGlobalSound(string soundName)
	{
		PlaySound(soundName);
	}

	internal void PlayGlobalSound(string soundName, bool fadeMusic)
	{
		if (fadeMusic)
		{
			Globals.MainMenu.StartCoroutine(PlaySoundWithMusicFade(0.4f, soundName));
		}
		else
		{
			PlaySound(Globals.Player, soundName, 0f, null, isParent: false, Vector3.zero);
		}
	}

	private IEnumerator PlaySoundWithMusicFade(float fadeTime, string soundName)
	{
		float time = 0f;
		float musicVolume = SingletonT<ServerData>.I.GameSettings.MusicVolume;
		while (time < fadeTime)
		{
			time += Time.deltaTime;
			SetCurrentMusicVolume(musicVolume * (1f - time / fadeTime));
			yield return null;
		}
		SetCurrentMusicVolume(0f);
		float soundLength = PlaySound(Globals.Player, soundName, 0f, null, isParent: false, Vector3.zero);
		yield return new WaitForSeconds(soundLength);
		time = 0f;
		while (time < fadeTime)
		{
			time += Time.deltaTime;
			SetCurrentMusicVolume(musicVolume * (time / fadeTime));
			yield return null;
		}
		SetCurrentMusicVolume(musicVolume);
	}

	internal void PlaySoundClickPopup()
	{
		PlayGlobalSound("click_" + Utils.Random(1, 5, ref _lastClickPopupSoundId) + "_low_volume");
	}

	internal void PlaySoundMoneyPay()
	{
		PlayGlobalSound("_money_" + Utils.Random(1, 3, ref _lastMoneyPaySoundId));
	}

	internal void PlayChestSound()
	{
		PlayGlobalSound("chest");
	}

	internal void PlayFailSound()
	{
		PlayGlobalSound("failure");
	}

	internal void PlaySuccessSound()
	{
		PlayGlobalSound("success");
	}

	private static string PersonModelName(Person person)
	{
		if (person == null)
		{
			return null;
		}
		string text = person.ModelName;
		PersonData component = person.GetComponent<PersonData>();
		if (component != null && component.UseAssetsOfOtherModel > 0)
		{
			text = component.UseAssetsOfOtherModel.ToString();
		}
		if (text.Contains("_"))
		{
			text = text.Remove(text.IndexOf("_"));
		}
		return text;
	}

	internal float PlaySound(Person person, string soundName, float delay, string posName, bool isParent, Vector3 offset)
	{
		try
		{
			if (!Globals.IsPlaySound || Globals.DebugDontLoadAndPlaySounds)
			{
				return 0f;
			}
			AudioClip sound = GetSound(PersonModelName(person), soundName);
			Transform transform = ((!(person != null)) ? null : person.BodyPart(posName));
			if (transform == null && person != null)
			{
				transform = person.transform;
			}
			if (transform == null)
			{
				AudioListener audioListener = Utils.FindObjectOfTypeNoThrow<AudioListener>();
				if (audioListener != null)
				{
					transform = audioListener.transform;
				}
			}
			if (sound == null || transform == null)
			{
				return 0f;
			}
			SingletonT<TimeEventsManager>.I.StartOneShotTimeEvent(delay, delegate
			{
				Vector3 pos = transform.TransformPoint(offset);
				PlayOnce(sound, pos, (!isParent) ? null : transform);
			});
			return sound.length;
		}
		catch (Exception)
		{
			return 0f;
		}
	}

	private void PlayOnce(AudioClip audioClip, Vector3 pos, Transform parent)
	{
		GameObject gameObject = new GameObject("one shot audio " + audioClip.name);
		gameObject.transform.position = pos;
		gameObject.transform.parent = parent;
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		Suicidal suicidal = gameObject.AddComponent<Suicidal>();
		suicidal.SuicideTime = audioClip.length;
		audioSource.volume = SingletonT<ServerData>.I.GameSettings.SoundsVolume;
		audioSource.clip = audioClip;
		audioSource.Play();
	}

	public void UnloadAllSounds()
	{
		foreach (KeyValuePair<string, ResourcesManager.AssetBundleData> soundsBundle in _soundsBundles)
		{
		}
	}

	internal void UnloadSounds(string personName)
	{
		ResourcesManager.AssetBundleData value = null;
		if (_soundsBundles.TryGetValue(personName, out value) && value != null)
		{
			SingletonT<ResourcesManager>.I.RemoveAssetBundle(value, PersonSoundsPath(personName), removeAll: true);
			_soundsBundles.Remove(personName);
		}
	}

	public void CacheGlobalSounds(MonoBehaviour caller)
	{
		CacheSounds(caller, "globals");
	}

	private string PersonSoundsPath(string personName)
	{
		return ResourcesManager.GetAssetBundlePath("sounds/" + personName);
	}

	internal void CacheSounds(MonoBehaviour caller, Person person)
	{
		CacheSounds(caller, PersonModelName(person));
	}

	private void CacheAllSounds(MonoBehaviour caller, string personName, ResourcesManager.AssetBundleData data)
	{
		data.Clips = new Dictionary<string, AudioClip>();
		UnityEngine.Object[] array = data.Bundle.LoadAllAssets(typeof(AudioClip));
		for (int i = 0; i < array.Length; i++)
		{
			AudioClip audioClip = (AudioClip)array[i];
			data.Clips.Add(audioClip.name, audioClip);
		}
		SingletonT<ResourcesManager>.I.UnloadAssetBundle(data.Bundle, destroyObjs: false);
		data.Reset();
	}

	internal void CacheSounds(MonoBehaviour caller, string personName)
	{
		if (!Globals.DebugDontLoadAndPlaySounds && !_soundsBundles.ContainsKey(personName))
		{
			SingletonT<ResourcesManager>.I.GetAssetBundleAsync(caller, PersonSoundsPath(personName), delegate(string _, ResourcesManager.AssetBundleData bundle, float time)
			{
				Utils.LogFrom("SoundManager", "LoadSounds", personName, time);
				_soundsBundles.Add(personName, bundle);
				CacheAllSounds(caller, personName, bundle);
			}, delegate
			{
				Utils.LogFrom("SoundManager", "LoadSounds no sound");
			});
		}
	}

	internal AudioClip GetSound(string personName, string soundName)
	{
		if (!soundName.StartsWith("local_"))
		{
			personName = "globals";
		}
		ResourcesManager.AssetBundleData value = null;
		AudioClip value2 = null;
		if (_soundsBundles.TryGetValue(personName, out value) && value != null && !Globals.DebugLoadSoundsABOnly && value.Clips != null)
		{
			value.Clips.TryGetValue(soundName, out value2);
		}
		return value2;
	}

	public void SetMusicVolume(float volume)
	{
		if (SingletonT<ServerData>.I.GameSettings != null)
		{
			SingletonT<ServerData>.I.GameSettings.MusicVolume = volume;
		}
		SetCurrentMusicVolume(volume);
	}

	internal void SetCurrentMusicVolume(float volume)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			Data data = _data[i];
			if (data != null && data.Source != null)
			{
				data.Source.volume = volume * MusikK;
				if (!data.Source.isPlaying)
				{
					data.Source.Play();
				}
			}
		}
	}

	internal void OnNewEnemy(Player player, Enemy enemy)
	{
		if (enemy == null)
		{
			OnNewEnemy(player);
			return;
		}
		PersonData component = enemy.GetComponent<PersonData>();
		if (!(component == null))
		{
			string text = PersonModelName(enemy);
			if (!(player != null) || !(player.ModelName == text))
			{
				UnloadSounds(text);
			}
		}
	}

	internal void OnNewEnemy(Player player)
	{
		if (player == null)
		{
			return;
		}
		List<string> list = new List<string>(_soundsBundles.Keys);
		foreach (string item in list)
		{
			if (item != player.ModelName && item != "globals")
			{
				UnloadSounds(item);
			}
		}
	}
}
