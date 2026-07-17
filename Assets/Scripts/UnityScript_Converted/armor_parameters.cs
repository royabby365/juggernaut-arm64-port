using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Parameters/Armor")]
public class armor_parameters : MonoBehaviour
{
	public GameObject Armor;

	public ArmorTypes ArmorType;

	public BodyPositions BodyPosition;

	public GameObject AddChildrenToBodyPosition;

	public BodyPositionItem[] BodyPositions;

	public Vector3 ArmorTranslate;

	public Vector3 ArmorTranslateWoman;

	public bool AddSkinMaterial;

	public Material ReplaceBodyMaterial;

	public Material ReplaceBodyMaterial2;

	public Transform ReplaceEffect;

	public AnimationTypes WeaponAnimationType;

	public bool HelmHairsOff;

	public bool HelmFaceOff;

	public string Flags;

	public string ModelFile;

	public armor_parameters()
	{
		Flags = string.Empty;
		ModelFile = string.Empty;
	}

	public virtual void Main()
	{
	}
}
