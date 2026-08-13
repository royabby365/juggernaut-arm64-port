using UnityEngine;

public class PersonData : MonoBehaviour
{
	public string ScenariosIndex = string.Empty;

	public Vector3 TranslateAtScene = Vector3.zero;

	public EffectsSizes EffectsSize = EffectsSizes.Normal;

	public int MaxAnimationsPack = 2;

	public int UseAssetsOfOtherModel;

	public bool UseAssetsOfOtherModel_ScenariosLocal;

	public bool LoadLocalEffects;

	public bool LoadLocalSounds = true;

	public AnimationClip VictoryAnimationHummer;

	public AnimationClip VictoryAnimation2Handed;

	public AnimationClip VictoryAnimationGlave;

	public Vector3 CameraAddOffset;

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

	internal Transform GetPosByType(BodyPositions postype)
	{
		return postype switch
		{
			BodyPositions.none => base.transform, 
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

	internal Transform GetPosByType(ArmorTypes postype)
	{
		return postype switch
		{
			ArmorTypes.helm => PosHead, 
			ArmorTypes.hand_l => PosHandL, 
			ArmorTypes.hand_r => PosHandR, 
			ArmorTypes.weapon => PosWeapon, 
			ArmorTypes.shoulderstrap => PosSpineCenter, 
			ArmorTypes.belt => PosToeL, 
			ArmorTypes.pelvis => PosToeR, 
			ArmorTypes.torso => PosMiddle, 
			ArmorTypes.boots => PosHipR, 
			_ => null, 
		};
	}

	internal string GetPosNameByType(BodyPositions postype)
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

	internal GameObject GetBodyByArmorType(ArmorTypes armortype)
	{
		return armortype switch
		{
			ArmorTypes.npcbody => NPCBody, 
			ArmorTypes.helm => BodyHead, 
			ArmorTypes.torso => BodyTorso, 
			ArmorTypes.hand_l => BodyHandL, 
			ArmorTypes.hand_r => BodyHandR, 
			ArmorTypes.pelvis => BodyPelvis, 
			ArmorTypes.boots => BodyBoots, 
			_ => null, 
		};
	}

	internal Material GetMatByType(BodyTypes bodytype)
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

	internal Material GetMatByType(ArmorTypes bodytype)
	{
		return bodytype switch
		{
			ArmorTypes.npcbody => MatNPCBody, 
			ArmorTypes.torso => MatTorso, 
			ArmorTypes.hand_l => MatHandL, 
			ArmorTypes.hand_r => MatHandR, 
			ArmorTypes.pelvis => MatPelvis, 
			ArmorTypes.boots => MatBoots, 
			_ => null, 
		};
	}

	internal Material RendererMaterial(GameObject obj)
	{
		if (obj != null)
		{
			Renderer renderer = obj.GetComponent<Renderer>();
			if (renderer != null)
			{
				return GetComponent<Renderer>().material;
			}
		}
		return null;
	}

	internal Material GetMatByType2(BodyTypes bodytype)
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
}
