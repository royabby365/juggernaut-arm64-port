using System.Collections.Generic;
using UnityEngine;

public class PersonArmor : MonoBehaviour
{
	internal class ArmorLoadEntry
	{
		internal ServerData.Slot.TypeE Slot;

		internal string SetName;

		internal bool IsHair;

		internal int HairColor;

		internal int Eyes;
	}

	private GameObject _weapon;

	public string SetName;

	internal Color HairsColor = Color.black;

	internal static Dictionary<int, Color> _hairsColors;

	private bool _parallelLoad;

	private List<GameObject> _bodyPartsToHide = new List<GameObject>();

	public GameObject Weapon
	{
		get
		{
			return _weapon;
		}
		set
		{
			if (_weapon != value)
			{
				_weapon = value;
				Messenger<GameObject>.Invoke(Globals.MsgPlayerWeaponChanged, value);
			}
		}
	}

	static PersonArmor()
	{
		_hairsColors = new Dictionary<int, Color>();
		AddHairColor(0, 170, 150, 135);
		AddHairColor(1, 199, 109, 109);
		AddHairColor(2, 135, 170, 145);
		AddHairColor(3, 135, 139, 170);
		AddHairColor(4, 170, 117, 117);
		AddHairColor(5, 167, 170, 117);
		AddHairColor(6, 117, 131, 170);
		AddHairColor(7, 57, 18, 18);
		AddHairColor(8, 59, 78, 47);
		AddHairColor(9, 128, 128, 128);
		AddHairColor(10, 92, 116, 92);
	}

	internal void PutAllPlayerArmor()
	{
		PutAllPlayerArmor(SingletonT<ServerData>.I.PlayerServerPersData.ModelId, null, noWeapon: false, null);
	}

	private static void AddHairColor(int i, int r, int g, int b)
	{
		_hairsColors.Add(i, new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, 1f));
	}

	internal void LoadHair(string modelId, string hairId, int colorId, ActionD<ServerData.Item> onLoad)
	{
		Utils.Log(" === LOADHAIR0", modelId, hairId, colorId);
		Transform transform = base.transform.root.FindChildByName("hairs", includeInactive: true);
		if (transform != null)
		{
			ResreshHairVisibility();
			if (onLoad != null)
			{
				onLoad(null);
			}
			return;
		}
		SingletonT<ResourcesManager>.I.LoadHair(Globals.MainMenu, modelId, hairId, delegate(GameObject go)
		{
			Transform transform2 = go.transform.FindChildByName("hairs", includeInactive: true);
			if (transform2 != null)
			{
				GameObject gameObject = AddSkinnedMeshClone(transform2.gameObject);
				if (gameObject != null)
				{
					Color value = _hairsColors[0];
					_hairsColors.TryGetValue(colorId, out value);
					HairsColor = value;
					Material[] materials = gameObject.renderer.materials;
					foreach (Material material in materials)
					{
						material.color = HairsColor;
					}
					ResreshHairVisibility();
				}
			}
			if (onLoad != null)
			{
				onLoad(null);
			}
		}, delegate(string _, string __)
		{
			Utils.Log("LOADHAIR FAILED", modelId, hairId, colorId, __, _);
		});
	}

	internal void PutAllArmorSet(string modelId, string setName, string weaponName, ActionD onLoadAll)
	{
		Utils.Log("PutAllArmorSet", modelId, setName, weaponName);
		object[] slots = new object[8]
		{
			ServerData.Slot.TypeE.Belt,
			ServerData.Slot.TypeE.Boots,
			ServerData.Slot.TypeE.HandLeft,
			ServerData.Slot.TypeE.HandRight,
			ServerData.Slot.TypeE.Helm,
			ServerData.Slot.TypeE.Pelvis,
			ServerData.Slot.TypeE.Shoulder,
			ServerData.Slot.TypeE.Torso
		};
		bool isMan = modelId != "2";
		int loadedCount = 0;
		if (string.IsNullOrEmpty(setName) && string.IsNullOrEmpty(weaponName))
		{
			if (onLoadAll != null)
			{
				onLoadAll();
			}
			return;
		}
		if (string.IsNullOrEmpty(setName))
		{
			if (!string.IsNullOrEmpty(weaponName))
			{
				SingletonT<ResourcesManager>.I.LoadArmorAsync(Globals.MainMenu, modelId, weaponName, ServerData.Slot.TypeE.Weapon, delegate(string abPath2, GameObject weaponGO)
				{
					CreateArmor(abPath2, weaponGO, isMan, weaponName, ServerData.Slot.TypeE.Weapon);
					if (onLoadAll != null)
					{
						onLoadAll();
					}
				}, delegate
				{
					Utils.Log("PutAllArmorSet FAILED weapon", modelId, weaponName);
					if (onLoadAll != null)
					{
						onLoadAll();
					}
				});
			}
			else if (++loadedCount == slots.Length && onLoadAll != null)
			{
				onLoadAll();
			}
			return;
		}
		SingletonT<ResourcesManager>.I.LoadArmorSet(Globals.MainMenu, modelId, setName, delegate(string abPath, ResourcesManager.AssetBundleData ab)
		{
			SingletonT<ResourcesManager>.I.LoadAsync(Globals.MainMenu, ab.Bundle, slots, (object _) => ((ServerData.Slot.TypeE)(int)_).PrefabName(setName), delegate(object obj, string name, GameObject go)
			{
				CreateArmor(abPath, go, isMan, setName, (ServerData.Slot.TypeE)(int)obj);
				SingletonT<ResourcesManager>.I.RemoveAssetBundle(ab, abPath);
			}, delegate
			{
				if (!string.IsNullOrEmpty(weaponName))
				{
					SingletonT<ResourcesManager>.I.LoadArmorAsync(Globals.MainMenu, modelId, weaponName, ServerData.Slot.TypeE.Weapon, delegate(string path, GameObject weaponGO)
					{
						CreateArmor(path, weaponGO, isMan, weaponName, ServerData.Slot.TypeE.Weapon);
						if (onLoadAll != null)
						{
							onLoadAll();
						}
					}, delegate
					{
						Utils.Log("PutAllArmorSet FAILED weapon", modelId, weaponName);
						if (onLoadAll != null)
						{
							onLoadAll();
						}
					});
				}
				else if (++loadedCount == slots.Length && onLoadAll != null)
				{
					onLoadAll();
				}
			});
		}, delegate
		{
			if (++loadedCount == slots.Length && onLoadAll != null)
			{
				onLoadAll();
			}
			Utils.Log("PutAllArmorSet FAILED", modelId, setName);
		});
	}

	private void RemoveAll(string playerId, string setName, List<ServerData.Slot.TypeE> slots, ActionD onRemoveAll)
	{
		if (_parallelLoad)
		{
			int count = 0;
			ActionD<ServerData.Item> onLoad = delegate
			{
				count++;
				if (count == slots.Count && onRemoveAll != null)
				{
					onRemoveAll();
				}
			};
			if (slots.Count != 0)
			{
				foreach (ServerData.Slot.TypeE slot in slots)
				{
					if (setName == null)
					{
						ChangeArmor(playerId, null, slot, onLoad);
					}
					else
					{
						ChangeArmor(playerId, null, setName, slot, onLoad);
					}
				}
				return;
			}
			if (onRemoveAll != null)
			{
				onRemoveAll();
			}
		}
		else
		{
			RemoveAllRec(slots.GetEnumerator(), playerId, setName, onRemoveAll);
		}
	}

	private void RemoveAllRec(IEnumerator<ServerData.Slot.TypeE> slotsEnum, string playerId, string setName, ActionD onRemoveAll)
	{
		if (!slotsEnum.MoveNext())
		{
			if (onRemoveAll != null)
			{
				onRemoveAll();
			}
			return;
		}
		ActionD<ServerData.Item> onLoad = delegate
		{
			RemoveAllRec(slotsEnum, playerId, setName, onRemoveAll);
		};
		ServerData.Slot.TypeE current = slotsEnum.Current;
		if (setName == null)
		{
			ChangeArmor(playerId, null, current, onLoad);
		}
		else
		{
			ChangeArmor(playerId, null, setName, current, onLoad);
		}
	}

	internal void PutAllPlayerArmor(string playerId, string setName, bool noWeapon, ActionD onLoad)
	{
		List<ServerData.Slot.TypeE> slots = new List<ServerData.Slot.TypeE>
		{
			ServerData.Slot.TypeE.Belt,
			ServerData.Slot.TypeE.Boots,
			ServerData.Slot.TypeE.HandLeft,
			ServerData.Slot.TypeE.HandRight,
			ServerData.Slot.TypeE.Helm,
			ServerData.Slot.TypeE.Pelvis,
			ServerData.Slot.TypeE.Shoulder,
			ServerData.Slot.TypeE.Torso
		};
		if (!noWeapon)
		{
			slots.Add(ServerData.Slot.TypeE.Weapon);
		}
		List<ServerData.Item> putOn = SingletonT<ServerData>.I.GetAllPutOn();
		if (_parallelLoad)
		{
			int changedCount = 0;
			ActionD<ServerData.Item> onLoad2 = delegate(ServerData.Item c)
			{
				changedCount++;
				if (c.Slot.SlotId == ServerData.Slot.TypeE.Weapon)
				{
					SingletonT<ServerData>.I.MyWeapon = c;
				}
				slots.Remove(c.Slot.SlotId);
				if (changedCount == putOn.Count)
				{
					RemoveAll(playerId, setName, slots, onLoad);
				}
			};
			if (putOn.Count > 0)
			{
				foreach (ServerData.Item item in putOn)
				{
					if (setName == null)
					{
						ChangeArmor(playerId, item, item.Slot.SlotId, onLoad2);
					}
					else
					{
						ChangeArmor(playerId, item, setName, item.Slot.SlotId, onLoad2);
					}
				}
				return;
			}
			RemoveAll(playerId, setName, slots, onLoad);
		}
		else
		{
			PutAllPlayerArmorRec(putOn.GetEnumerator(), playerId, setName, slots, onLoad);
		}
	}

	private void PutAllPlayerArmorRec(IEnumerator<ServerData.Item> putOnEnum, string playerId, string setName, List<ServerData.Slot.TypeE> slots, ActionD onLoad)
	{
		if (!putOnEnum.MoveNext())
		{
			RemoveAll(playerId, setName, slots, onLoad);
			return;
		}
		ActionD<ServerData.Item> onLoad2 = delegate(ServerData.Item item)
		{
			if (item.Slot.SlotId == ServerData.Slot.TypeE.Weapon)
			{
				SingletonT<ServerData>.I.MyWeapon = item;
			}
			slots.Remove(item.Slot.SlotId);
			PutAllPlayerArmorRec(putOnEnum, playerId, setName, slots, onLoad);
		};
		ServerData.Item current = putOnEnum.Current;
		if (setName == null)
		{
			ChangeArmor(playerId, current, current.Slot.SlotId, onLoad2);
		}
		else
		{
			ChangeArmor(playerId, current, setName, current.Slot.SlotId, onLoad2);
		}
	}

	internal void PutAllEnemyArmor(string playerId, List<ArmorLoadEntry> list, ActionD onLoad)
	{
		if (list.Count == 0)
		{
			if (onLoad != null)
			{
				onLoad();
			}
			return;
		}
		int count = 0;
		ActionD<ServerData.Item> onLoad2 = delegate
		{
			count++;
			if (count == list.Count)
			{
				ResreshHairVisibility();
				if (onLoad != null)
				{
					onLoad();
				}
			}
		};
		foreach (ArmorLoadEntry item in list)
		{
			if (item.Eyes != 0)
			{
				ChangeArmor(playerId, null, item.Eyes.ToString(), ServerData.Slot.TypeE.Eyes, onLoad2);
			}
			else if (!item.IsHair)
			{
				ChangeArmor(playerId, null, item.SetName, item.Slot, onLoad2);
			}
			else
			{
				LoadHair(playerId, item.SetName, item.HairColor, onLoad2);
			}
		}
	}

	internal void PutAllEnemyArmor(string playerId, string setName, bool noWeapon, ActionD onLoad)
	{
		List<ServerData.Slot.TypeE> list = new List<ServerData.Slot.TypeE>();
		list.Add(ServerData.Slot.TypeE.Belt);
		list.Add(ServerData.Slot.TypeE.Boots);
		list.Add(ServerData.Slot.TypeE.HandLeft);
		list.Add(ServerData.Slot.TypeE.HandRight);
		list.Add(ServerData.Slot.TypeE.Helm);
		list.Add(ServerData.Slot.TypeE.Pelvis);
		list.Add(ServerData.Slot.TypeE.Shoulder);
		list.Add(ServerData.Slot.TypeE.Torso);
		List<ServerData.Slot.TypeE> list2 = list;
		if (!noWeapon)
		{
			list2.Add(ServerData.Slot.TypeE.Weapon);
		}
		RemoveAll(playerId, setName, list2, onLoad);
	}

	internal void ChangeArmor(string person, ServerData.Item item, ServerData.Slot.TypeE slot, ActionD<ServerData.Item> onLoad)
	{
		if (item == null || (item != null && item.IsArmorOrWeapon))
		{
			ChangeArmor(person, item, (item == null) ? Globals.DefaultSetName(slot) : item.Get3DModel(), slot, onLoad);
		}
		else
		{
			onLoad?.Invoke(item);
		}
	}

	internal void ChangeArmor(string person, ServerData.Item item, string setName, ServerData.Slot.TypeE slot, ActionD<ServerData.Item> onLoad)
	{
		ServerData.Slot.TypeE slotId = ((item == null) ? slot : item.Slot.SlotId);
		if (slotId != ServerData.Slot.TypeE.Boots && slotId != ServerData.Slot.TypeE.Belt && slotId != ServerData.Slot.TypeE.HandLeft && slotId != ServerData.Slot.TypeE.HandRight && slotId != ServerData.Slot.TypeE.Helm && slotId != ServerData.Slot.TypeE.Pelvis && slotId != ServerData.Slot.TypeE.Shoulder && slotId != ServerData.Slot.TypeE.Torso && slotId != ServerData.Slot.TypeE.Eyes && slotId != ServerData.Slot.TypeE.Weapon)
		{
			if (onLoad != null)
			{
				onLoad(item);
			}
			return;
		}
		GameObject gameObject = ((slotId != ServerData.Slot.TypeE.Eyes) ? IsArmorPutOn(setName, slotId) : null);
		if (gameObject != null)
		{
			gameObject.SetActive(true);
			if (slotId == ServerData.Slot.TypeE.Helm)
			{
				Transform transform = base.transform.FindChildByName("head", includeInactive: true);
				if (transform != null)
				{
					transform.gameObject.SetActive(true);
				}
			}
			if (onLoad != null)
			{
				onLoad(item);
			}
			return;
		}
		SingletonT<ResourcesManager>.I.LoadArmor((!(Globals.MainMenu != null)) ? Globals.ABLoader : Globals.MainMenu, person, setName, slotId, delegate(string abPath, GameObject go)
		{
			CreateArmor(abPath, go, person != "2", setName, slotId);
			if (onLoad != null)
			{
				onLoad(item);
			}
		}, delegate(string _, string error)
		{
			Utils.Log("ChangeArmor FAILED", person, setName, slotId, _, error);
			if (onLoad != null)
			{
				onLoad(item);
			}
		});
	}

	public void HideBodyParts()
	{
		foreach (GameObject item in _bodyPartsToHide)
		{
			item.active = false;
		}
		Transform transform = base.transform.FindChildByName("head", includeInactive: true);
		if (transform != null)
		{
			Utils.SetAllRenderersActive(transform, value: true);
		}
	}

	private void Start()
	{
		PersonData componentInChildren = base.transform.root.GetComponentInChildren<PersonData>();
		bool flag = !(componentInChildren != null) || componentInChildren.ScenariosIndex != "2";
	}

	private void CreateArmor(string path, GameObject armor, bool isMan, string setName, ServerData.Slot.TypeE slotId)
	{
		if (armor != null)
		{
			Utils.TryDo(delegate
			{
				GameObject armorPrototype = (GameObject)Object.Instantiate(armor);
				Load(path, isMan, armorPrototype, setName, slotId);
			}, () => armor.name);
		}
	}

	public GameObject IsArmorPutOn(string setName, ServerData.Slot.TypeE slotId)
	{
		ArmoronPersData[] componentsInChildren = base.gameObject.GetComponentsInChildren<ArmoronPersData>(includeInactive: true);
		foreach (ArmoronPersData armoronPersData in componentsInChildren)
		{
			if (armoronPersData.Slot == slotId && armoronPersData.SetName == setName && armoronPersData.Slot != ServerData.Slot.TypeE.Eyes)
			{
				return armoronPersData.gameObject;
			}
		}
		return null;
	}

	private void RemoveEffects(PersonData charParams, ArmorData armorParams)
	{
		Transform posByType = charParams.GetPosByType(armorParams.BodyPosition);
		if (posByType != null && posByType.name.StartsWith("pos_"))
		{
			Utils.ForeachChild(posByType.transform, delegate(Transform _)
			{
				Object.Destroy(_.gameObject);
			});
		}
		posByType = charParams.GetPosByType(armorParams.ArmorType);
		if (posByType != null && posByType.name.StartsWith("pos_"))
		{
			Utils.ForeachChild(posByType.transform, delegate(Transform _)
			{
				Object.Destroy(_.gameObject);
			});
		}
	}

	private void Load(string path, bool isMan, GameObject armorPrototype, string setName, ServerData.Slot.TypeE slotId)
	{
		if (armorPrototype == null)
		{
			return;
		}
		ArmorData component = armorPrototype.GetComponent<ArmorData>();
		PersonData component2 = base.gameObject.GetComponent<PersonData>();
		Invs.Inv(component != null, "armorParams != null", armorPrototype.name);
		Invs.Inv(component2 != null, "charParams != null", armorPrototype.name);
		Material replaceBodyMaterial = component.ReplaceBodyMaterial;
		Material replaceBodyMaterial2 = component.ReplaceBodyMaterial2;
		if (replaceBodyMaterial != null && replaceBodyMaterial2 != null)
		{
			GameObject bodyByArmorType = component2.GetBodyByArmorType(component.ArmorType);
			if (bodyByArmorType != null)
			{
				bodyByArmorType.renderer.materials = new Material[2] { replaceBodyMaterial, replaceBodyMaterial2 };
			}
		}
		else if ((bool)replaceBodyMaterial)
		{
			GameObject bodyByArmorType2 = component2.GetBodyByArmorType(component.ArmorType);
			if (bodyByArmorType2 != null)
			{
				bodyByArmorType2.renderer.material = replaceBodyMaterial;
			}
		}
		GameObject gameObject = null;
		if (replaceBodyMaterial != null && component.Armor == null)
		{
			component.Armor = armorPrototype;
			gameObject = armorPrototype;
		}
		if (component.Armor == null)
		{
			return;
		}
		Transform posByType = component2.GetPosByType(component.BodyPosition);
		GameObject gameObject2 = null;
		GameObject bodyByArmorType3 = component2.GetBodyByArmorType(component.ArmorType);
		if (bodyByArmorType3 != null)
		{
			gameObject2 = bodyByArmorType3.gameObject;
		}
		bool flag = component.Armor.GetComponent<SkinnedMeshRenderer>() != null;
		if (flag)
		{
			gameObject = AddSkinnedMesh(component.Armor);
			gameObject.AddComponent<FromAssetBundle>().Path = path;
			GameObject addChildrenToBodyPosition = component.AddChildrenToBodyPosition;
			if ((bool)posByType && (bool)addChildrenToBodyPosition)
			{
				gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, base.transform.root.localScale);
				Transform transform = posByType.FindChildByName(addChildrenToBodyPosition.name);
				if (transform != null && transform.name == addChildrenToBodyPosition.name)
				{
					transform.transform.parent = null;
					transform.transform.position = new Vector3(1000f, 100000f, 0f);
					Object.Destroy(transform.gameObject);
				}
				addChildrenToBodyPosition.transform.parent = posByType;
				addChildrenToBodyPosition.transform.localPosition = Vector3.zero;
				addChildrenToBodyPosition.transform.localEulerAngles = Vector3.zero;
			}
		}
		else if (posByType != null)
		{
			gameObject = component.Armor;
			gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, base.transform.root.localScale);
			gameObject.transform.parent = posByType;
			gameObject.AddComponent<FromAssetBundle>().Path = path;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
		}
		ArmorData[] componentsInChildren = GetComponentsInChildren<ArmorData>();
		foreach (ArmorData armorData in componentsInChildren)
		{
			if (armorData.ArmorType == component.ArmorType && !armorData.gameObject.Equals(gameObject))
			{
				Object.Destroy(armorData.gameObject);
			}
		}
		Animation component3 = component.Armor.GetComponent<Animation>();
		if (component3 != null)
		{
			if (gameObject.GetComponent<Animation>() == null)
			{
				gameObject.AddComponent<Animation>();
			}
			if (component3.clip != null)
			{
				string newName = component3.clip.name;
				if (gameObject.animation.GetClip(newName) == null)
				{
					gameObject.animation.AddClip(component3.clip, newName);
					gameObject.animation.clip = component3.clip;
				}
				gameObject.animation.Play(newName);
			}
		}
		GameObject bodyByArmorType4 = component2.GetBodyByArmorType(component.ArmorType);
		if (component.AddSkinMaterial && bodyByArmorType4 != null && gameObject.renderer != null && bodyByArmorType4.renderer != null)
		{
			if (gameObject.renderer.materials.Length > 1 && gameObject.renderer.materials[1] != null)
			{
				Color color = gameObject.renderer.materials[1].color;
				gameObject.renderer.materials = new Material[2]
				{
					gameObject.renderer.material,
					bodyByArmorType4.renderer.material
				};
				gameObject.renderer.materials[1].color = color;
			}
			else
			{
				gameObject.renderer.materials = new Material[2]
				{
					gameObject.renderer.material,
					bodyByArmorType4.renderer.material
				};
			}
		}
		ServerData.PersData playerServerPersData = SingletonT<ServerData>.I.PlayerServerPersData;
		if (isMan && component.ArmorTranslate != Vector3.zero)
		{
			gameObject.transform.Translate(component.ArmorTranslate);
		}
		else if (!isMan && component.ArmorTranslateWoman != Vector3.zero)
		{
			gameObject.transform.Translate(component.ArmorTranslateWoman);
		}
		if (gameObject != null)
		{
			ArmorData armorData2 = gameObject.AddComponent<ArmorData>();
			armorData2.Armor = component.Armor;
			armorData2.ArmorType = component.ArmorType;
			armorData2.BodyPosition = component.BodyPosition;
			armorData2.AddChildrenToBodyPosition = component.AddChildrenToBodyPosition;
			armorData2.ArmorTranslate = component.ArmorTranslate;
			armorData2.ArmorTranslateWoman = component.ArmorTranslateWoman;
			armorData2.AddSkinMaterial = component.AddSkinMaterial;
			armorData2.ReplaceBodyMaterial = component.ReplaceBodyMaterial;
			armorData2.WeaponAnimationType = component.WeaponAnimationType;
			armorData2.HelmHairsOff = component.HelmHairsOff;
			armorData2.HelmFaceOff = component.HelmFaceOff;
			ArmoronPersData armoronPersData = gameObject.AddComponent<ArmoronPersData>();
			armoronPersData.SetName = setName;
			armoronPersData.Slot = slotId;
			if (component.ReplaceEffect != null)
			{
				Transform posByType2 = component2.GetPosByType(component.BodyPosition);
				if (posByType2 != null)
				{
					Transform transform2 = (Transform)Object.Instantiate(component.ReplaceEffect);
					transform2.transform.parent = posByType2;
					transform2.transform.localRotation = Quaternion.identity;
					transform2.transform.localPosition = Vector3.zero;
				}
			}
			if (slotId == ServerData.Slot.TypeE.Weapon)
			{
				Component componentInChildren = Utils.GetComponentInChildren(gameObject, "armor_fx");
				if (componentInChildren != null && Utils.GetValue(componentInChildren, "fxname") is string fxName)
				{
					ArmorFx armorFx = gameObject.AddComponent<ArmorFx>();
					armorFx.FxName = fxName;
				}
			}
			if (slotId == ServerData.Slot.TypeE.Weapon)
			{
				Weapon = gameObject;
			}
			if (slotId == ServerData.Slot.TypeE.Helm)
			{
				if (component.HelmHairsOff)
				{
					HideHair();
				}
				else
				{
					ShowHair();
				}
			}
		}
		if (gameObject2 != null && slotId != ServerData.Slot.TypeE.Helm)
		{
			gameObject2.active = false;
			if (gameObject2.GetComponent<ArmorData>() == null)
			{
				_bodyPartsToHide.Add(gameObject2);
			}
		}
		else if (slotId == ServerData.Slot.TypeE.Helm && gameObject2 != null)
		{
			gameObject2.active = true;
		}
		if (flag || (!(replaceBodyMaterial != null) && !(replaceBodyMaterial2 != null)))
		{
			Object.Destroy(armorPrototype);
		}
	}

	private GameObject AddSkinnedMesh(GameObject element)
	{
		SkinnedMeshRenderer component = element.GetComponent<SkinnedMeshRenderer>();
		if (component == null)
		{
			return null;
		}
		element.transform.position = Vector3.zero;
		element.transform.localScale = base.transform.root.localScale;
		element.transform.parent = base.transform;
		element.transform.localPosition = Vector3.zero;
		SkinnedMeshRenderer skinnedMeshRenderer = component;
		Transform[] bones = component.bones;
		skinnedMeshRenderer.bones = bones.Clone((Transform _) => base.transform.FindChildByName(_.name, includeInactive: true));
		skinnedMeshRenderer.updateWhenOffscreen = true;
		return element;
	}

	private GameObject AddSkinnedMeshClone(GameObject element)
	{
		SkinnedMeshRenderer component = element.GetComponent<SkinnedMeshRenderer>();
		if (component == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject(element.name);
		gameObject.transform.localScale = base.transform.root.localScale;
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
		Transform[] bones = component.bones;
		skinnedMeshRenderer.bones = bones.Clone((Transform _) => base.transform.FindChildByName(_.name, includeInactive: true));
		skinnedMeshRenderer.sharedMesh = component.sharedMesh;
		skinnedMeshRenderer.materials = component.materials;
		Material[] materials = skinnedMeshRenderer.materials;
		foreach (Material material in materials)
		{
			if (material != null && material.mainTexture != null)
			{
				Utils.DoNothing(material.mainTexture.name);
			}
		}
		skinnedMeshRenderer.receiveShadows = false;
		skinnedMeshRenderer.updateWhenOffscreen = true;
		return gameObject;
	}

	internal void ShowHair()
	{
		Transform transform = base.transform.root.FindChildByName("hairs", includeInactive: true);
		if (transform != null)
		{
			Material[] materials = transform.renderer.materials;
			foreach (Material material in materials)
			{
				material.color = HairsColor;
			}
			transform.ShowOrHide(show: true);
		}
	}

	internal void HideHair()
	{
		Transform transform = base.transform.root.FindChildByName("hairs", includeInactive: false);
		if (transform != null)
		{
			transform.ShowOrHide(show: false);
		}
	}

	internal void ResreshHairVisibility()
	{
		ArmorData[] componentsInChildren = GetComponentsInChildren<ArmorData>();
		foreach (ArmorData armorData in componentsInChildren)
		{
			if (armorData.ArmorType == ArmorTypes.helm)
			{
				if (armorData.HelmHairsOff)
				{
					HideHair();
				}
				else
				{
					ShowHair();
				}
				break;
			}
		}
	}
}
