using System;
using Boo.Lang.Runtime;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class character_armors : MonoBehaviour
{
	public GameObject Armor1;

	public GameObject Armor2;

	public GameObject Armor3;

	public GameObject Armor4;

	public GameObject Armor5;

	public GameObject Armor6;

	public GameObject Armor7;

	public GameObject Armor8;

	public virtual void Start()
	{
		CreateArmor(Armor1);
		CreateArmor(Armor2);
		CreateArmor(Armor3);
		CreateArmor(Armor4);
		CreateArmor(Armor5);
		CreateArmor(Armor6);
		CreateArmor(Armor7);
		CreateArmor(Armor8);
	}

	public virtual void CreateArmor(GameObject armor)
	{
		if ((bool)armor)
		{
			Load((GameObject)UnityEngine.Object.Instantiate(armor));
		}
	}

	public virtual void Load(GameObject @object)
	{
		if (!@object)
		{
			return;
		}
		GameObject gameObject = null;
		armor_parameters armor_parameters2 = (armor_parameters)@object.GetComponent<armor_parameters>();
		if (!armor_parameters2)
		{
			return;
		}
		Material replaceBodyMaterial = armor_parameters2.ReplaceBodyMaterial;
		object obj = null;
		character_parameters character_parameters2 = (character_parameters)GetComponent<character_parameters>();
		if ((bool)replaceBodyMaterial && RuntimeServices.ToBool(obj))
		{
			GameObject bodyByArmorType = character_parameters2.GetBodyByArmorType(armor_parameters2.ArmorType);
			if ((bool)bodyByArmorType)
			{
				Material[] array = new Material[2] { replaceBodyMaterial, null };
				object obj2 = obj;
				if (!(obj2 is Material))
				{
					obj2 = RuntimeServices.Coerce(obj2, typeof(Material));
				}
				array[1] = (Material)obj2;
				bodyByArmorType.GetComponent<Renderer>().materials = array;
			}
		}
		else if ((bool)replaceBodyMaterial)
		{
			GameObject bodyByArmorType = character_parameters2.GetBodyByArmorType(armor_parameters2.ArmorType);
			if ((bool)bodyByArmorType)
			{
				bodyByArmorType.GetComponent<Renderer>().material = replaceBodyMaterial;
			}
		}
		if ((bool)replaceBodyMaterial && !armor_parameters2.Armor)
		{
			armor_parameters2.Armor = @object;
			gameObject = @object;
		}
		if (!armor_parameters2.Armor)
		{
			return;
		}
		UnityScript.Lang.Array array2 = new object[0];
		Transform posByType = character_parameters2.GetPosByType(armor_parameters2.BodyPosition);
		if ((bool)(SkinnedMeshRenderer)armor_parameters2.Armor.GetComponent<SkinnedMeshRenderer>())
		{
			gameObject = AddSkinnedMesh(armor_parameters2.Armor);
			GameObject addChildrenToBodyPosition = armor_parameters2.AddChildrenToBodyPosition;
			if ((bool)posByType && (bool)addChildrenToBodyPosition)
			{
				addChildrenToBodyPosition.transform.localPosition = Vector3.zero;
				addChildrenToBodyPosition.transform.localEulerAngles = Vector3.zero;
			}
		}
		else if (armor_parameters2.BodyPosition != BodyPositions.none && (bool)posByType)
		{
			gameObject = armor_parameters2.Armor;
			gameObject.transform.parent = posByType;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
		}
		Animation animation = (Animation)armor_parameters2.Armor.GetComponent<Animation>();
		if ((bool)animation)
		{
			gameObject.AddComponent(typeof(Animation));
			gameObject.GetComponent<Animation>().AddClip(GetComponent<Animation>().clip, GetComponent<Animation>().clip.name);
			gameObject.GetComponent<Animation>().clip = GetComponent<Animation>().clip;
			gameObject.GetComponent<Animation>().Play(GetComponent<Animation>().clip.name);
		}
		GameObject bodyByArmorType2 = character_parameters2.GetBodyByArmorType(armor_parameters2.ArmorType);
		if (armor_parameters2.AddSkinMaterial && (bool)bodyByArmorType2)
		{
			Material[] sharedMaterials = new Material[2]
			{
				gameObject.GetComponent<Renderer>().material,
				bodyByArmorType2.GetComponent<Renderer>().material
			};
			gameObject.GetComponent<Renderer>().sharedMaterials = sharedMaterials;
		}
		AnimationTypes weaponAnimationType = armor_parameters2.WeaponAnimationType;
		if (armor_parameters2.ArmorTranslate != Vector3.zero)
		{
			gameObject.transform.Translate(armor_parameters2.ArmorTranslate);
		}
		ArmorTypes armorType = armor_parameters2.ArmorType;
		if ((bool)gameObject)
		{
			armor_parameters armor_parameters3 = (armor_parameters)gameObject.AddComponent(typeof(armor_parameters));
			armor_parameters3.Armor = armor_parameters2.Armor;
			armor_parameters3.ArmorType = armor_parameters2.ArmorType;
			armor_parameters3.BodyPosition = armor_parameters2.BodyPosition;
			armor_parameters3.AddChildrenToBodyPosition = armor_parameters2.AddChildrenToBodyPosition;
			armor_parameters3.ArmorTranslate = armor_parameters2.ArmorTranslate;
			armor_parameters3.ArmorTranslateWoman = armor_parameters2.ArmorTranslateWoman;
			armor_parameters3.AddSkinMaterial = armor_parameters2.AddSkinMaterial;
			armor_parameters3.ReplaceBodyMaterial = armor_parameters2.ReplaceBodyMaterial;
			armor_parameters3.WeaponAnimationType = armor_parameters2.WeaponAnimationType;
			armor_parameters3.HelmHairsOff = armor_parameters2.HelmHairsOff;
			armor_parameters3.HelmFaceOff = armor_parameters2.HelmFaceOff;
			SetNewArmor(armor_parameters2.ArmorType, string.Empty, gameObject);
		}
		UnityEngine.Object.Destroy(@object);
	}

	public virtual GameObject RecoverBodyByType(ArmorTypes ArmorType)
	{
		return null;
	}

	public virtual GameObject HideBodyByType(ArmorTypes ArmorType)
	{
		object result;
		if (ArmorType != ArmorTypes.npcbody)
		{
			character_parameters character_parameters2 = (character_parameters)GetComponent<character_parameters>();
			GameObject bodyByArmorType = character_parameters2.GetBodyByArmorType(ArmorType);
			if ((bool)bodyByArmorType)
			{
				bodyByArmorType.GetComponent<Renderer>().enabled = false;
				result = bodyByArmorType;
			}
			else
			{
				result = null;
			}
		}
		else
		{
			result = null;
		}
		return (GameObject)result;
	}

	public virtual void SetNewArmor(object armortype, string filename, GameObject armor)
	{
	}

	public virtual GameObject AddSkinnedMesh(GameObject element)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)element.GetComponent<SkinnedMeshRenderer>();
		object result;
		if ((bool)skinnedMeshRenderer)
		{
			GameObject gameObject = new GameObject(element.name);
			gameObject.transform.parent = transform;
			gameObject.transform.localPosition = Vector3.zero;
			SkinnedMeshRenderer skinnedMeshRenderer2 = (SkinnedMeshRenderer)gameObject.AddComponent(typeof(SkinnedMeshRenderer));
			Transform[] array = new Transform[skinnedMeshRenderer.bones.Length];
			for (int i = 0; i < skinnedMeshRenderer.bones.Length; i++)
			{
				array[i] = funcs.FindChildByName(transform, skinnedMeshRenderer.bones[i].name);
			}
			skinnedMeshRenderer2.bones = array;
			skinnedMeshRenderer2.sharedMesh = skinnedMeshRenderer.sharedMesh;
			skinnedMeshRenderer2.materials = skinnedMeshRenderer.materials;
			skinnedMeshRenderer2.receiveShadows = false;
			skinnedMeshRenderer2.updateWhenOffscreen = true;
			result = gameObject;
		}
		else
		{
			result = null;
		}
		return (GameObject)result;
	}

	public virtual void Main()
	{
	}
}
