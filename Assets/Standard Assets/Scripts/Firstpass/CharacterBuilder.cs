using UnityEngine;

/// <summary>
/// Runtime assembler for the baked bind-pose warrior characters.
///
/// Character parts were baked by scripts/bake_bindpose.py from the original
/// Unity 4.x skinned rig: each part OBJ (Assets/Resources/__char/) already
/// contains bind-pose vertex positions in a common frame, so assembling is just
/// parenting all parts at the origin. Textures are the extracted _ds PNGs in
/// Resources/__textures/.
///
/// IL2CPP-safe: uses the custom JuggernautPlaceholder shader (project shader,
/// always compiled), no CreatePrimitive, no Shader.Find("Standard").
/// </summary>
public static class CharacterBuilder
{
    // Male blue warrior: armor set pve_1, sword weapon
    private static readonly string[] MaleParts =
    {
        "blue_war_m_pve_1_boots",
        "blue_war_m_pve_1_pelvis",
        "blue_war_m_pve_1_belt",
        "blue_war_m_pve_1_torso",
        "blue_war_m_pve_1_shoulderstrap",
        "blue_war_m_pve_1_hand_l",
        "blue_war_m_pve_1_hand_r",
        "blue_war_m_pve_1_helm",
    };

    // Female blue warrior: armor set pve_1 + visible head under the helm
    private static readonly string[] FemaleParts =
    {
        "blue_war_f_pve_1_boots",
        "blue_war_f_pve_1_pelvis",
        "blue_war_f_pve_1_belt",
        "blue_war_f_pve_1_torso",
        "blue_war_f_pve_1_shoulderstrap",
        "blue_war_f_pve_1_hand_l",
        "blue_war_f_pve_1_hand_r",
        "f_0_head",
        "blue_war_f_pve_1_helm",
    };

    public enum Variant { Male, Female }

    /// <summary>Assemble a warrior. Returns null if meshes missing.</summary>
    public static GameObject Build(Variant variant = Variant.Male, string rootName = null)
    {
        string[] parts = variant == Variant.Male ? MaleParts : FemaleParts;
        if (rootName == null)
            rootName = variant == Variant.Male ? "Warrior_Blue_Pve1" : "Warrior_Blue_F_Pve1";

        Shader shader = Shader.Find("Hidden/JuggernautPlaceholder");
        if (shader == null)
        {
            Debug.LogWarning("[CharacterBuilder] JuggernautPlaceholder shader not found");
            return null;
        }

        var root = new GameObject(rootName);
        int loaded = 0;

        foreach (string part in parts)
        {
            // Resources.Load<Mesh> resolves the imported OBJ mesh by name
            Mesh mesh = Resources.Load<Mesh>($"__char/{part}");
            if (mesh == null)
            {
                Debug.LogWarning($"[CharacterBuilder] mesh missing: {part}");
                continue;
            }

            var go = new GameObject(part);
            go.transform.SetParent(root.transform);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();

            // Male textures cover the female strap/belt/hands (original game reuses them)
            Texture2D tex = Resources.Load<Texture2D>($"__textures/{part}_ds");
            if (tex == null && variant == Variant.Female)
            {
                tex = Resources.Load<Texture2D>($"__textures/{part.Replace("_f_", "_m_")}_ds");
            }
            var mat = new Material(shader);
            if (tex != null)
            {
                mat.mainTexture = tex;
            }
            else
            {
                mat.color = new Color(0.5f, 0.55f, 0.65f);
            }
            mr.sharedMaterial = mat;
            loaded++;
        }

        // The baked parts stand centered on the origin, feet at y≈0; face the
        // battle camera (-z) by flipping the A-pose which faces +z.
        root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // Attach the weapon at the right hand
        AttachWeapon(root, shader, variant);

        Debug.Log($"[CharacterBuilder] assembled {loaded}/{parts.Length} parts ({variant})");
        if (loaded == 0)
        {
            Object.Destroy(root);
            return null;
        }
        return root;
    }

    private static void AttachWeapon(GameObject root, Shader shader, Variant variant)
    {
        // Male: sword (baked centered on grip at origin, blade along Y).
        // Female: mace (weapons/2.unity3d - mace2_ds) - baked via bake_arena.py
        string meshName, texName, weaponName;
        if (variant == Variant.Male)
        {
            meshName = "blue_war_pve_1_sword";
            texName = "blue_war_pve_1_sword_ds";
            weaponName = "Sword";
        }
        else
        {
            meshName = "weapon2";   // mace from weapons/2.unity3d
            texName = "mace2_ds";
            weaponName = "Mace";
        }

        Mesh mesh = Resources.Load<Mesh>($"__char/{meshName}");
        if (mesh == null)
        {
            Debug.LogWarning($"[CharacterBuilder] {weaponName} mesh missing: {meshName}");
            return;
        }
        Texture2D tex = Resources.Load<Texture2D>($"__textures/{texName}");
        var weapon = new GameObject(weaponName);
        weapon.transform.SetParent(root.transform);
        weapon.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = weapon.AddComponent<MeshRenderer>();
        var mat = new Material(shader);
        if (tex != null) mat.mainTexture = tex;
        mr.sharedMaterial = mat;

        // Bind-pose right hand at x≈-0.53 y≈1.21 (male) / x≈-0.33 y≈1.12 (female);
        // place the grip at the hand, blade/mace angling down-forward.
        float handX = variant == Variant.Male ? -0.53f : -0.33f;
        float handY = variant == Variant.Male ? 1.21f : 1.12f;
        weapon.transform.localPosition = new Vector3(handX, handY, 0.02f);
        if (variant == Variant.Male)
        {
            // Sword: blade along Y, tilt blade forward
            weapon.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
        }
        else
        {
            // Mace: head extends along +z, so tilt it downward (head toward ground)
            weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f) * Quaternion.Euler(-35f, 90f, 0f);
        }
    }
}
