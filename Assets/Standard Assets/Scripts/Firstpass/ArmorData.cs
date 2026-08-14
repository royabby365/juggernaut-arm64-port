using UnityEngine;

public class ArmorData : MonoBehaviour
{
	public class BodyPositionItem
	{
		public BodyPositions BodyPosition;

		public GameObject AddChildrenToBodyPosition;

		public Transform ReplaceEffect;
	}

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

	public string Flags = string.Empty;

	public bool IsBloodOn;

	public string ModelFile = string.Empty;
}
