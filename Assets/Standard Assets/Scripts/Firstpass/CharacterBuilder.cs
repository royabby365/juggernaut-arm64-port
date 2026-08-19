using UnityEngine;

/// <summary>
/// Runtime assembler for the baked bind-pose hero characters.
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
    public enum Variant
    {
        MaleWarrior,   // blue_war_m_pve_1 + sword
        FemaleWarrior, // blue_war_f_pve_1 + mace
        MaleAssassin,  // blue_asn_m_pve_1 + glaive
        MaleMage,      // blue_mag_m_pve_1 + hammer
    }

    private static string[] PartsFor(Variant v)
    {
        switch (v)
        {
            case Variant.MaleWarrior:
                return new[] { "blue_war_m_pve_1_boots", "blue_war_m_pve_1_pelvis", "blue_war_m_pve_1_belt",
                               "blue_war_m_pve_1_torso", "blue_war_m_pve_1_shoulderstrap",
                               "blue_war_m_pve_1_hand_l", "blue_war_m_pve_1_hand_r", "blue_war_m_pve_1_helm" };
            case Variant.FemaleWarrior:
                return new[] { "blue_war_f_pve_1_boots", "blue_war_f_pve_1_pelvis", "blue_war_f_pve_1_belt",
                               "blue_war_f_pve_1_torso", "blue_war_f_pve_1_shoulderstrap",
                               "blue_war_f_pve_1_hand_l", "blue_war_f_pve_1_hand_r",
                               "f_0_head", "blue_war_f_pve_1_helm" };
            case Variant.MaleAssassin:
                return new[] { "blue_asn_m_pve_1_boots", "blue_asn_m_pve_1_pelvis", "blue_asn_m_pve_1_belt",
                               "blue_asn_m_pve_1_torso", "blue_asn_m_pve_1_shoulderstrap",
                               "blue_asn_m_pve_1_hand_l", "blue_asn_m_pve_1_hand_r",
                               "m_0_head", "blue_asn_m_pve_1_helm" };
            case Variant.MaleMage:
                return new[] { "blue_mag_m_pve_1_boots", "blue_mag_m_pve_1_pelvis", "blue_mag_m_pve_1_belt",
                               "blue_mag_m_pve_1_torso", "blue_mag_m_pve_1_shoulderstrap",
                               "blue_mag_m_pve_1_hand_l", "blue_mag_m_pve_1_hand_r", "blue_mag_m_pve_1_helm" };
            default:
                return new string[0];
        }
    }

    private static string RootNameFor(Variant v)
    {
        switch (v)
        {
            case Variant.MaleWarrior: return "Warrior_Blue_Pve1";
            case Variant.FemaleWarrior: return "Warrior_Blue_F_Pve1";
            case Variant.MaleAssassin: return "Assassin_Blue_Pve1";
            case Variant.MaleMage: return "Mage_Blue_Pve1";
            default: return "Hero";
        }
    }

    /// <summary>Assemble a hero. Returns null if meshes missing.</summary>
    public static GameObject Build(Variant variant = Variant.MaleWarrior, string rootName = null)
    {
        string[] parts = PartsFor(variant);
        if (rootName == null)
            rootName = RootNameFor(variant);

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

            Texture2D tex = ResolvePartTexture(part, variant);
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

        AttachWeapon(root, shader, variant);

        Debug.Log($"[CharacterBuilder] assembled {loaded}/{parts.Length} parts ({variant})");
        if (loaded == 0)
        {
            Object.Destroy(root);
            return null;
        }
        return root;
    }

    private static Texture2D ResolvePartTexture(string part, Variant variant)
    {
        // Head parts use the character head atlas texture
        if (part == "m_0_head") return Resources.Load<Texture2D>("__textures/char_1_head");
        if (part == "f_0_head") return Resources.Load<Texture2D>("__textures/f_0_head_ds");

        Texture2D tex = Resources.Load<Texture2D>($"__textures/{part}_ds");
        if (tex == null)
        {
            // Female warrior strap/belt/hands reuse the male textures
            string alt = part.Replace("_f_", "_m_");
            if (alt != part)
                tex = Resources.Load<Texture2D>($"__textures/{alt}_ds");
        }
        return tex;
    }

    private static void AttachWeapon(GameObject root, Shader shader, Variant variant)
    {
        string meshName, texName, weaponName;
        switch (variant)
        {
            case Variant.MaleWarrior:
                meshName = "blue_war_pve_1_sword"; texName = "blue_war_pve_1_sword_ds";
                weaponName = "Sword"; break;
            case Variant.FemaleWarrior:
                meshName = "weapon2"; texName = "mace2_ds";
                weaponName = "Mace"; break;
            case Variant.MaleAssassin:
                meshName = "glaive"; texName = "blue_asn_pve_1_glaive_ds";
                weaponName = "Glaive"; break;
            case Variant.MaleMage:
                meshName = "hammer"; texName = "blue_mag_pve_1_hammer_ds";
                weaponName = "Hammer"; break;
            default:
                return;
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

        // Hand positions per variant (bind pose)
        float handX, handY;
        switch (variant)
        {
            case Variant.FemaleWarrior: handX = -0.33f; handY = 1.12f; break;
            default: handX = -0.53f; handY = 1.21f; break;
        }
        weapon.transform.localPosition = new Vector3(handX, handY, 0.02f);

        // Weapon orientation (all baked centered on grip at origin)
        switch (variant)
        {
            case Variant.MaleWarrior: // sword: blade along Y, tilt forward
                weapon.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
                break;
            case Variant.FemaleWarrior: // mace: head along +z, tilt down
                weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f) * Quaternion.Euler(-35f, 90f, 0f);
                break;
            case Variant.MaleAssassin: // glaive: pole along z, hold vertical-ish
                weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f) * Quaternion.Euler(15f, 0f, 0f);
                break;
            case Variant.MaleMage: // hammer: head along +z, tilt down-forward
                weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f) * Quaternion.Euler(-20f, 90f, 0f);
                break;
        }
    }
}
