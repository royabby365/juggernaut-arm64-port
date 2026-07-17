using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Parameters/Character")]
public class character_parameters : MonoBehaviour
{
	public Vector3 TranslateAtScene;

	public EffectsSizes EffectsSize;

	public int MaxAnimationsPack;

	public int UseAssetsOfOtherModel;

	public string UseAssetsOfOtherModelText;

	public bool UseAssetsOfOtherModel_ScenariosLocal;

	public string UseSoundsOfOtherModelText;

	public bool LoadLocalEffects;

	public bool LoadLocalSounds;

	public bool DontHideBody;

	public GameObject Bones;

	public float CameraBonesOffset;

	public float CameraAddDistance;

	public float CameraAddHeight;

	public float CameraEnemyBonesOffset;

	public float CameraEnemyAddDistance;

	public float CameraEnemyAddHeight;

	public GameObject NPCBody;

	public GameObject BodyHead;

	public GameObject BodyTorso;

	public GameObject BodyHandL;

	public GameObject BodyHandR;

	public GameObject BodyPelvis;

	public GameObject BodyBoots;

	public Material MatNPCBody;

	public Material MatHead;

	public Material MatTorso;

	public Material MatHandL;

	public Material MatHandR;

	public Material MatPelvis;

	public Material MatBoots;

	public Transform PosHead;

	public Transform PosEyes;

	public Transform PosHandL;

	public Transform PosHandR;

	public Transform PosWeapon;

	public Transform PosSpineCenter;

	public Transform PosToeL;

	public Transform PosToeR;

	public Transform PosMiddle;

	public Transform PosHipL;

	public Transform PosHipR;

	public Transform PosShoulderL;

	public Transform PosShoulderR;

	public character_parameters()
	{
		TranslateAtScene = new Vector3(0f, 0f, 0f);
		EffectsSize = EffectsSizes.Normal;
		MaxAnimationsPack = 2;
		UseAssetsOfOtherModelText = string.Empty;
		UseSoundsOfOtherModelText = string.Empty;
		LoadLocalSounds = true;
	}

	public virtual Transform GetPosByType(BodyPositions postype)
	{
		return postype switch
		{
			BodyPositions.none => transform, 
			BodyPositions.Head => PosHead, 
			BodyPositions.Eyes => PosEyes, 
			BodyPositions.HandL => PosHandL, 
			BodyPositions.HandR => PosHandR, 
			BodyPositions.Weapon => PosWeapon, 
			BodyPositions.SpineCenter => PosSpineCenter, 
			BodyPositions.ToeL => PosToeL, 
			BodyPositions.ToeR => PosToeR, 
			BodyPositions.Middle => PosMiddle, 
			BodyPositions.HipR => PosHipR, 
			BodyPositions.HipL => PosHipL, 
			BodyPositions.ShoulderR => PosShoulderR, 
			BodyPositions.ShoulderL => PosShoulderL, 
			_ => null, 
		};
	}

	public virtual string GetPosNameByType(BodyPositions postype)
	{
		return postype switch
		{
			BodyPositions.none => "bottom", 
			BodyPositions.Head => "pos_head", 
			BodyPositions.Eyes => "pos_eyes", 
			BodyPositions.HandL => "pos_hand_l", 
			BodyPositions.HandR => "pos_hand_r", 
			BodyPositions.Weapon => "pos_weapon", 
			BodyPositions.SpineCenter => "pos_spinecenter", 
			BodyPositions.ToeL => "pos_toe_l", 
			BodyPositions.ToeR => "pos_toe_r", 
			BodyPositions.Middle => "pos_middle", 
			BodyPositions.HipR => "pos_hip_r", 
			BodyPositions.HipL => "pos_hip_l", 
			BodyPositions.ShoulderR => "pos_shoulder_r", 
			BodyPositions.ShoulderL => "pos_shoulder_l", 
			_ => null, 
		};
	}

	public virtual GameObject GetBodyByArmorType(ArmorTypes armortype)
	{
		return armortype switch
		{
			ArmorTypes.npcbody => NPCBody, 
			ArmorTypes.torso => BodyTorso, 
			ArmorTypes.hand_l => BodyHandL, 
			ArmorTypes.hand_r => BodyHandR, 
			ArmorTypes.pelvis => BodyPelvis, 
			ArmorTypes.boots => BodyBoots, 
			_ => null, 
		};
	}

	public virtual Material GetMatByType(BodyTypes bodytype)
	{
		return bodytype switch
		{
			BodyTypes.npcbody => MatNPCBody, 
			BodyTypes.head => MatHead, 
			BodyTypes.torso => MatTorso, 
			BodyTypes.hand_l => MatHandL, 
			BodyTypes.hand_r => MatHandR, 
			BodyTypes.pelvis => MatPelvis, 
			BodyTypes.boots => MatBoots, 
			_ => null, 
		};
	}

	public virtual Material RendererMaterial(GameObject obj)
	{
		object result;
		if ((bool)obj)
		{
			Renderer renderer = obj.renderer;
			result = ((!renderer) ? null : GetComponent<Renderer>()material);
		}
		else
		{
			result = null;
		}
		return (Material)result;
	}

	public virtual Material GetMatByType2(BodyTypes bodytype)
	{
		return bodytype switch
		{
			BodyTypes.npcbody => RendererMaterial(NPCBody), 
			BodyTypes.head => RendererMaterial(BodyHead), 
			BodyTypes.torso => RendererMaterial(BodyTorso), 
			BodyTypes.hand_l => RendererMaterial(BodyHandL), 
			BodyTypes.hand_r => RendererMaterial(BodyHandR), 
			BodyTypes.pelvis => RendererMaterial(BodyPelvis), 
			BodyTypes.boots => RendererMaterial(BodyBoots), 
			_ => null, 
		};
	}

	public virtual void Main()
	{
	}
}
