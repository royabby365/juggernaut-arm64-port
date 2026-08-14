using System;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCharHud : MonoBehaviour
{
	private class PersonData
	{
		internal GameObject GO;

		internal ServerData.PersData PersData;

		internal ResourcesManager.AssetBundleData AssetData;

		internal ResourcesManager.AssetBundleData ArmorAssetData;
	}

	private const int CELL_COUNT = 3;

	private List<PersonData> _persons = new List<PersonData>();

	private int _selectedPersonIndex = 2;

	private int _prevSelectedPersonIndex;

	private int _personCount;

	private bool _isPersonsLoaded;

	public float _cellWidth = 2f;

	private bool _isStartVideoPlaying = true;

	private bool _isMousePressed;

	private bool _hasDrag;

	private float _time = float.MaxValue;

	private float _scrollSpeed;

	private float _prevInertiaCurveValue;

	private float _pathLength;

	private bool _isFirstTime = true;

	public StatProgressBar[] Stats;

	public GameObject IconDark;

	public GameObject IconIce;

	public GameObject IconFire;

	public GameObject IconLighting;

	private GameObject[] _icons = new GameObject[4];

	public SpriteText PreferenceName;

	public SpriteText PersName;

	public SpriteText PersDescription;

	public SpriteText PersFeature;

	public GameObject CharStatsRoot;

	public GameObject LeftDevider;

	public GameObject RightDevider;

	public Camera Camera3D;

	public GameObject CharsRoot;

	public Vector3 CharacterInitPosition;

	public Vector3 CharacterInitAngles;

	public float MoveTime = 1.5f;

	public AnimationCurve InertiaCurve;

	public float MaxScrollSpeed = 0.24f;

	public bool IsPagingEnabled = true;

	public Transform Sounds;

	private bool _loadJugs;

	private void Start()
	{
		if (_loadJugs)
		{
			CharacterInitAngles = new Vector3(0f, 135f, 0f);
		}
	}

	private void OnEnable()
	{
		SpriteGui instance = HudMk1.Instance;
		if (!(instance == null))
		{
			instance.ReleaseWithMousePosition += ChooseCharHud_ReleaseWithMousePosition;
			instance.MoveBegin += ChooseCharHud_MoveBegin;
			instance.Move += ChooseCharHud_Move;
			instance.MoveEnd += ChooseCharHud_MoveEnd;
		}
	}

	private void OnDisable()
	{
		SpriteGui instance = HudMk1.Instance;
		if (!(instance == null))
		{
			instance.ReleaseWithMousePosition -= ChooseCharHud_ReleaseWithMousePosition;
			instance.MoveBegin -= ChooseCharHud_MoveBegin;
			instance.Move -= ChooseCharHud_Move;
			instance.MoveEnd -= ChooseCharHud_MoveEnd;
		}
	}

	internal void Show()
	{
		Globals.GameScreen = Globals.GameScreenE.SelectPlayer;
		_icons[0] = IconDark;
		_icons[1] = IconFire;
		_icons[2] = IconIce;
		_icons[3] = IconLighting;
		LoadPersons();
	}

	private void Update()
	{
		if (!_isStartVideoPlaying && !(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.ChooseChar)
		{
			float num = Camera2D.ScreenWidth / 2;
			float num2 = Camera2D.ScreenWidth / 3;
			LeftDevider.transform.localPosition = new Vector3(0f - num + num2, LeftDevider.transform.localPosition.y, LeftDevider.transform.localPosition.z);
			RightDevider.transform.localPosition = new Vector3(0f - num + num2 * 2f, LeftDevider.transform.localPosition.y, LeftDevider.transform.localPosition.z);
			CharStatsRoot.transform.localPosition = new Vector3(CharStatsRoot.transform.localPosition.x, (float)(-Camera2D.ScreenHeight) / 2f + 127f, CharStatsRoot.transform.localPosition.z);
			if (_isPersonsLoaded)
			{
				UpdateScroll();
			}
		}
	}

	private void ChooseCharHud_MoveBegin(Vector3 obj)
	{
		if (_isPersonsLoaded && !(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.ChooseChar)
		{
			_isMousePressed = true;
			_scrollSpeed = 0f;
			_prevSelectedPersonIndex = _selectedPersonIndex;
		}
	}

	private void ChooseCharHud_Move(Vector3 arg1, Vector3 arg2)
	{
		if (!_isPersonsLoaded || HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.ChooseChar)
		{
			return;
		}
		float num = arg2.x - arg1.x;
		if (!_isMousePressed)
		{
			return;
		}
		_scrollSpeed = num * (Camera3D.orthographicSize / ((float)Camera2D.ScreenHeight / 2f));
		foreach (PersonData person in _persons)
		{
			person.GO.transform.localPosition = new Vector3(person.GO.transform.localPosition.x + _scrollSpeed, person.GO.transform.localPosition.y, person.GO.transform.localPosition.z);
		}
		Swap();
	}

	private void ChooseCharHud_MoveEnd(Vector3 obj)
	{
		if (_isPersonsLoaded && !(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.ChooseChar)
		{
			_isMousePressed = false;
			_scrollSpeed = Mathf.Clamp(_scrollSpeed, 0f - MaxScrollSpeed, MaxScrollSpeed);
			_prevInertiaCurveValue = 0f;
			CalculatePathAndTime();
		}
	}

	public void DestroyPersons()
	{
		List<PersonData> persons = _persons;
		_persons = new List<PersonData>();
		foreach (PersonData item in persons)
		{
			try
			{
				if (item.AssetData != null && item.AssetData.Bundle != null)
				{
					SingletonT<ResourcesManager>.I.UnloadAssetBundle(item.AssetData.Bundle, destroyObjs: true);
				}
			}
			catch (Exception)
			{
			}
			if (item.ArmorAssetData != null && item.ArmorAssetData.Bundle != null)
			{
				SingletonT<ResourcesManager>.I.UnloadAssetBundle(item.ArmorAssetData.Bundle, destroyObjs: true);
			}
			UnityEngine.Object.Destroy(item.GO);
		}
	}

	private void ChooseCharHud_ReleaseWithMousePosition(SpriteButton button, Vector2 mousePos)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.ChooseChar)
		{
			return;
		}
		switch (button.name)
		{
		case "choose_active_char":
			Globals.ShowLoadingScreen(delegate
			{
				DestroyPersons();
				Globals.MainMenu.ChooseActivePlayerClicked();
			});
			break;
		case "2d_scene":
		{
			float num = (float)Camera2D.ScreenWidth / 3f;
			if (mousePos.x < num)
			{
				MoveRight();
			}
			else if (mousePos.x > num * 2f)
			{
				MoveLeft();
			}
			break;
		}
		case "chars_scroll_left":
			MoveLeft();
			break;
		case "chars_scroll_right":
			MoveRight();
			break;
		}
	}

	private void MoveLeft()
	{
		if (!(_time < InertiaCurve.keys[InertiaCurve.keys.Length - 1].time))
		{
			_time = 0f;
			_prevInertiaCurveValue = 0f;
			_pathLength = _cellWidth;
			_scrollSpeed = -1f;
		}
	}

	private void MoveRight()
	{
		if (!(_time < InertiaCurve.keys[InertiaCurve.keys.Length - 1].time))
		{
			_time = 0f;
			_prevInertiaCurveValue = 0f;
			_pathLength = _cellWidth;
			_scrollSpeed = 1f;
		}
	}

	private void UpdateSelection(int _selectedPersonIndex)
	{
		Utils.Log("UPDATESELECTION", _selectedPersonIndex, _persons.Count);
		int num = 0;
		Color32 color = new Color32(35, 35, 35, byte.MaxValue);
		Color32 color2 = new Color32(128, 128, 128, byte.MaxValue);
		foreach (PersonData person in _persons)
		{
			Renderer[] componentsInChildren = person.GO.GetComponentsInChildren<Renderer>();
			if (num == _selectedPersonIndex)
			{
				_persons[num].GO.GetComponent<Animation>()["idle2"].wrapMode = WrapMode.Loop;
				_persons[num].GO.GetComponent<Animation>().Play("idle2");
				Renderer[] array = componentsInChildren;
				foreach (Renderer renderer in array)
				{
					GetComponent<Renderer>().material.color = color2;
				}
				PresentPersStats(_persons[_selectedPersonIndex].PersData.Id);
				if (!_isFirstTime)
				{
					PlaySelectionSound(_persons[num].PersData.Id);
				}
			}
			else
			{
				_persons[num].GO.GetComponent<Animation>()["idle"].wrapMode = WrapMode.Loop;
				_persons[num].GO.GetComponent<Animation>().Play("idle");
				Renderer[] array2 = componentsInChildren;
				foreach (Renderer renderer2 in array2)
				{
					renderer2.material.color = color;
				}
			}
			num++;
		}
		SingletonT<ServerData>.I.PlayerServerPersData = _persons[_selectedPersonIndex].PersData;
		_isFirstTime = false;
	}

	private void PlaySelectionSound(int persId)
	{
		if (Sounds != null)
		{
			Transform transform = Sounds.FindChildByName(persId.ToString());
			if (transform != null)
			{
				GameObject gameObject = Sounds.FindChildByName(persId.ToString()).gameObject;
				AudioSource componentInChildren = gameObject.GetComponentInChildren<AudioSource>();
				componentInChildren.volume = SingletonT<ServerData>.I.GameSettings.SoundsVolume;
				componentInChildren.Play();
			}
		}
	}

	private void UpdateScroll()
	{
		int num = 0;
		float time = InertiaCurve.keys[InertiaCurve.keys.Length - 1].time;
		if (_isMousePressed || !(_time <= time))
		{
			return;
		}
		_time += Time.deltaTime;
		float num2 = InertiaCurve.Evaluate(_time);
		if (_time > time)
		{
			num2 = InertiaCurve.keys[InertiaCurve.keys.Length - 1].value;
			foreach (PersonData person in _persons)
			{
				if ((person.GO.transform.localPosition.x / _cellWidth).RoundToInt() == 0)
				{
					_selectedPersonIndex = num;
					break;
				}
				num++;
			}
			if (_prevSelectedPersonIndex != _selectedPersonIndex)
			{
				UpdateSelection(_selectedPersonIndex);
				_prevSelectedPersonIndex = _selectedPersonIndex;
			}
		}
		float moveDelta = (num2 - _prevInertiaCurveValue) * _pathLength;
		UpdatePositions(moveDelta);
		_prevInertiaCurveValue = num2;
	}

	private void UpdatePositions(float moveDelta)
	{
		if (_scrollSpeed > 0f)
		{
			foreach (PersonData person in _persons)
			{
				person.GO.transform.localPosition = new Vector3(person.GO.transform.localPosition.x + moveDelta, person.GO.transform.localPosition.y, person.GO.transform.localPosition.z);
			}
		}
		else
		{
			foreach (PersonData person2 in _persons)
			{
				person2.GO.transform.localPosition = new Vector3(person2.GO.transform.localPosition.x - moveDelta, person2.GO.transform.localPosition.y, person2.GO.transform.localPosition.z);
			}
		}
		Swap();
	}

	private void Swap()
	{
		int num = 0;
		int count = _persons.Count;
		int num2 = count / 2;
		foreach (PersonData person in _persons)
		{
			int num3 = (person.GO.transform.localPosition.x / _cellWidth).RoundToInt();
			if (_scrollSpeed < 0f)
			{
				if (num3 < -num2)
				{
					person.GO.transform.localPosition = new Vector3(person.GO.transform.localPosition.x + (float)count * _cellWidth, person.GO.transform.localPosition.y, person.GO.transform.localPosition.z);
				}
			}
			else if (_scrollSpeed > 0f && num3 > num2)
			{
				person.GO.transform.localPosition = new Vector3(person.GO.transform.localPosition.x - (float)count * _cellWidth, person.GO.transform.localPosition.y, person.GO.transform.localPosition.z);
			}
			num++;
		}
	}

	private void CalculatePathAndTime()
	{
		_time = 0f;
		float num = 5f * _cellWidth;
		float num2 = num / 2f;
		float num3 = _cellWidth / 2f;
		float num4 = (_persons[_selectedPersonIndex].GO.transform.localPosition.x + num2 + num3) % _cellWidth;
		if (Mathf.Abs(_scrollSpeed) > 0f)
		{
			int targetFrameRate = Application.targetFrameRate;
			_pathLength = Mathf.Abs(_scrollSpeed * (float)targetFrameRate / 2f * InertiaCurve.keys[InertiaCurve.keys.Length - 1].time);
			float num5 = _pathLength % _cellWidth;
			_pathLength -= num5;
			_pathLength += ((!(num5 > num3)) ? 0f : _cellWidth);
			if (IsPagingEnabled)
			{
				_pathLength %= _cellWidth;
			}
			if (_scrollSpeed > 0f)
			{
				if (num4 < num3)
				{
					_pathLength -= num4;
				}
				else
				{
					_pathLength += _cellWidth - num4;
				}
			}
			else if (num4 < num3)
			{
				_pathLength += num4;
			}
			else
			{
				_pathLength -= _cellWidth - num4;
			}
			if (_pathLength < 0f)
			{
				_pathLength += _cellWidth;
			}
		}
		else
		{
			if (num4 < num3)
			{
				_pathLength = num4;
				_scrollSpeed = -1f;
			}
			else
			{
				_pathLength = _cellWidth - num4;
				_scrollSpeed = 1f;
			}
			_time = InertiaCurve.keys[InertiaCurve.keys.Length - 1].time * (1f - _pathLength / num3);
		}
	}

	private void MakePerson(string id, GameObject go, ServerData.PersData pers)
	{
		CreatePerson(id, go, pers, noHairAndWeapon: true);
	}

	private void LoadPersons()
	{
		List<ServerData.PersData> startPersons = SingletonT<ServerData>.I.StartPersData;
		_personCount = startPersons.Count;
		Utils.Log("ChooseCharHud _personCount: ", _personCount);
		if (!_loadJugs)
		{
			if ((double)UnityApi.GetGameVersion() > 2.0)
			{
				List<ServerData.PersData> persons = new List<ServerData.PersData>(startPersons);
				SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "1", "_select_model_blue", delegate(string _, GameObject go)
				{
					int index = persons.FindIndex((ServerData.PersData x) => x.IsMan && x.IsClassBlue);
					MakePerson("1", go, persons[index]);
					persons.RemoveAt(index);
				});
				SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "1", "_select_model_red", delegate(string _, GameObject go)
				{
					int index = persons.FindIndex((ServerData.PersData x) => x.IsMan && x.IsClassRed);
					MakePerson("1", go, persons[index]);
					persons.RemoveAt(index);
				});
				SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "1", "_select_model_green", delegate(string _, GameObject go)
				{
					int index = persons.FindIndex((ServerData.PersData x) => x.IsMan && x.IsClassGreen);
					MakePerson("1", go, persons[index]);
					persons.RemoveAt(index);
				});
				SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "2", "_select_model_blue", delegate(string _, GameObject go)
				{
					int index = persons.FindIndex((ServerData.PersData x) => !x.IsMan && x.IsClassBlue);
					MakePerson("2", go, persons[index]);
					persons.RemoveAt(index);
				});
				SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "2", "_select_model_red", delegate(string _, GameObject go)
				{
					int index = persons.FindIndex((ServerData.PersData x) => !x.IsMan && x.IsClassRed);
					MakePerson("2", go, persons[index]);
					persons.RemoveAt(index);
				});
				SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "2", "_select_model_green", delegate(string _, GameObject go)
				{
					int index = persons.FindIndex((ServerData.PersData x) => !x.IsMan && x.IsClassGreen);
					MakePerson("2", go, persons[index]);
					persons.RemoveAt(index);
				});
				return;
			}
			SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "1", "_select", delegate(string _, GameObject go)
			{
				foreach (ServerData.PersData item in startPersons)
				{
					if (item.IsMan)
					{
						CreatePerson("1", go, item);
					}
				}
			});
			SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "2", "_select", delegate(string _, GameObject go)
			{
				foreach (ServerData.PersData item2 in startPersons)
				{
					if (!item2.IsMan)
					{
						CreatePerson("2", go, item2);
					}
				}
			});
			return;
		}
		List<ServerData.PersData> persons2 = new List<ServerData.PersData>(startPersons);
		SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "3_1", string.Empty, delegate(string _, GameObject go)
		{
			int index = persons2.FindIndex((ServerData.PersData x) => x.IsMan);
			MakePerson("3_1", go, persons2[index]);
			persons2.RemoveAt(index);
		});
		SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "3_3", string.Empty, delegate(string _, GameObject go)
		{
			int index = persons2.FindIndex((ServerData.PersData x) => x.IsMan);
			MakePerson("3_3", go, persons2[index]);
			persons2.RemoveAt(index);
			index = persons2.FindIndex((ServerData.PersData x) => x.IsMan);
			MakePerson("3_3", go, persons2[index]);
			persons2.RemoveAt(index);
		});
		SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "4_1", string.Empty, delegate(string _, GameObject go)
		{
			int index = persons2.FindIndex((ServerData.PersData x) => !x.IsMan);
			MakePerson("4_1", go, persons2[index]);
			persons2.RemoveAt(index);
		});
		SingletonT<ResourcesManager>.I.GetPersonPrototypeAndRemoveAssetBundle(this, "4_3", string.Empty, delegate(string _, GameObject go)
		{
			int index = persons2.FindIndex((ServerData.PersData x) => !x.IsMan);
			MakePerson("4_3", go, persons2[index]);
			persons2.RemoveAt(index);
		});
	}

	private void CreatePerson(string modelId, GameObject prototype, ServerData.PersData pers)
	{
		CreatePerson(modelId, prototype, pers, noHairAndWeapon: false);
	}

	private void CreatePerson(string modelId, GameObject prototype, ServerData.PersData pers, bool noHairAndWeapon)
	{
		GameObject person = (GameObject)UnityEngine.Object.Instantiate(prototype);
		int num = _personCount - 1;
		person.transform.parent = CharsRoot.transform;
		person.transform.localPosition = CharacterInitPosition + new Vector3((float)num * _cellWidth, 0f, 0f);
		person.transform.localRotation = Quaternion.Euler(CharacterInitAngles.x, CharacterInitAngles.y, CharacterInitAngles.z);
		person.active = true;
		PersonData personData = new PersonData();
		PersonArmor component = person.GetComponent<PersonArmor>();
		if (component != null)
		{
			person.GetComponent<PersonArmor>().PutAllArmorSet(modelId, pers.SelectSet, (!noHairAndWeapon) ? pers.SelectWeapon : null, delegate
			{
				SetupAnimations(person, pers.SelectWeapon);
				if (pers.SelectHair != null && !noHairAndWeapon)
				{
					bool flag = false;
					ArmorData[] componentsInChildren = person.GetComponentsInChildren<ArmorData>();
					foreach (ArmorData armorData in componentsInChildren)
					{
						if (armorData.HelmHairsOff && armorData.ArmorType == ArmorTypes.helm)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						person.GetComponent<PersonArmor>().LoadHair(modelId, pers.SelectHair, pers.SelectHairColor, delegate
						{
						});
					}
				}
				ArmorFx componentInChildren = person.GetComponentInChildren<ArmorFx>();
				if (componentInChildren != null)
				{
					componentInChildren.enabled = false;
				}
				if (--_personCount == 0)
				{
					PersonsLoaded();
				}
			});
		}
		personData.GO = person;
		personData.PersData = pers;
		_persons.Add(personData);
		if (component == null && --_personCount == 0)
		{
			PersonsLoaded();
		}
	}

	private void SetLayerRecursively(Transform root, int layer)
	{
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(root);
		while (stack.Count != 0)
		{
			Transform transform = stack.Pop();
			transform.gameObject.layer = layer;
			foreach (Transform item in transform)
			{
				stack.Push(item);
			}
		}
	}

	private void SetupAnimations(GameObject person, string setName)
	{
		PersonArmor component = person.GetComponent<PersonArmor>();
		ArmorData armorData = null;
		ArmorData[] componentsInChildren = person.GetComponentsInChildren<ArmorData>();
		foreach (ArmorData armorData2 in componentsInChildren)
		{
			if (armorData2.ArmorType == ArmorTypes.weapon)
			{
				armorData = armorData2;
				break;
			}
		}
		if (!(armorData != null))
		{
			return;
		}
		AnimationTypes weaponAnimationType = armorData.WeaponAnimationType;
		WeaponIdleData[] components = person.GetComponents<WeaponIdleData>();
		foreach (WeaponIdleData weaponIdleData in components)
		{
			if (weaponIdleData.Type == weaponAnimationType)
			{
				person.GetComponent<Animation>().AddClip(weaponIdleData.IdleAnimation, "idle");
				person.GetComponent<Animation>().AddClip(weaponIdleData.WinAnimation, "idle2");
				person.GetComponent<Animation>().wrapMode = WrapMode.Loop;
				person.GetComponent<Animation>().playAutomatically = true;
				break;
			}
		}
	}

	private void PersonsLoaded()
	{
		foreach (PersonData person in _persons)
		{
			SetLayerRecursively(person.GO.transform, Camera3D.gameObject.layer);
		}
		ResetPositions();
		Messenger.Invoke(Globals.MsgSelectScreenDataLoaded);
		Utils.Log("**** START PLAY INTRO VIDEO");
		SpriteGui.DontReleaseButtons = true;
		UnityApi.PlayMovie("1", delegate
		{
			SpriteGui.DontReleaseButtons = false;
			_isStartVideoPlaying = false;
			_isPersonsLoaded = true;
			UpdateSelection(_selectedPersonIndex);
			Globals.HideLoadingScreen();
		});
	}

	private void ResetPositions()
	{
		for (int i = 0; i < _persons.Count; i++)
		{
			Vector3 localPosition = CharacterInitPosition + new Vector3((float)(i - 2) * _cellWidth, 0f, 0f);
			_persons[i].GO.transform.localPosition = localPosition;
			string text = ((i != _selectedPersonIndex) ? "idle" : "idle2");
			if (i == _selectedPersonIndex)
			{
				PresentPersStats(_persons[_selectedPersonIndex].PersData.Id);
			}
			_persons[i].GO.GetComponent<Animation>()[text].wrapMode = WrapMode.Loop;
			_persons[i].GO.GetComponent<Animation>().Play(text);
		}
		SingletonT<ServerData>.I.PlayerServerPersData = _persons[2].PersData;
	}

	private void PresentPersStats(int persIdx)
	{
		ServerData.PersData persDataByServerId = SingletonT<ServerData>.I.GetPersDataByServerId(persIdx);
		PersName.Text_ = persDataByServerId.Title;
		PersDescription.Text_ = persDataByServerId.Description;
		PersFeature.Text_ = $"{SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.ChooseCharFeature)} {persDataByServerId.Feature}";
		for (int i = 0; i < persDataByServerId.Skills.Length; i++)
		{
			StatProgressBar statProgressBar = Stats[i];
			if (!(statProgressBar == null))
			{
				Stats[i].SetProgress(persDataByServerId.Skills[i].Skill.Title, persDataByServerId.Skills[i].Max, GetMaxSkill(i));
			}
		}
		switch (persDataByServerId.StartSpell.SkillType)
		{
		case ServerData.Skill.TypeE.MagicIce:
			SelectIcon(IconIce);
			break;
		case ServerData.Skill.TypeE.MagicFire:
			SelectIcon(IconFire);
			break;
		case ServerData.Skill.TypeE.MagicDark:
			SelectIcon(IconDark);
			break;
		case ServerData.Skill.TypeE.MagicElectro:
			SelectIcon(IconLighting);
			break;
		default:
			SelectIcon(IconFire);
			break;
		}
		if (PreferenceName != null)
		{
			PreferenceName.Text_ = persDataByServerId.StartSpell.Title;
		}
	}

	private int GetMaxSkill(int idx)
	{
		int num = 1;
		foreach (PersonData person in _persons)
		{
			if (person.PersData.Skills[idx].Max >= num)
			{
				num = person.PersData.Skills[idx].Max;
			}
		}
		return num;
	}

	private void SelectIcon(GameObject icon)
	{
		GameObject[] icons = _icons;
		foreach (GameObject gameObject in icons)
		{
			if (gameObject == icon)
			{
				gameObject.SetActiveRecursivelyMk1(setActive: true);
			}
			else
			{
				gameObject.SetActiveRecursivelyMk1(setActive: false);
			}
		}
	}
}
