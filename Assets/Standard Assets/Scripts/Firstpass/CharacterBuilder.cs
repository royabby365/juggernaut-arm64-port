using UnityEngine;

/// <summary>
/// Runtime assembler for the baked bind-pose warrior character.
///
/// The character parts were baked by scripts/bake_bindpose.py from the original
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
    private static readonly string[] Parts =
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

    /// <summary>Assemble the blue warrior. Returns null if meshes missing.</summary>
    public static GameObject Build(string rootName = "Warrior_Blue_Pve1")
    {
        Shader shader = Shader.Find("Hidden/JuggernautPlaceholder");
        if (shader == null)
        {
            Debug.LogWarning("[CharacterBuilder] JuggernautPlaceholder shader not found");
            return null;
        }

        var root = new GameObject(rootName);
        int loaded = 0;

        foreach (string part in Parts)
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

            Texture2D tex = Resources.Load<Texture2D>($"__textures/{part}_ds");
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

        // Attach the sword (blue_war_pve_1_sword, baked centered on grip at
        // origin, blade along Y) at the right hand. In bind pose the right
        // hand sits at x≈-0.53, y≈1.21, arm angled slightly out/down.
        AttachSword(root, shader);

        Debug.Log($"[CharacterBuilder] assembled {loaded}/{Parts.Length} parts");
        if (loaded == 0)
        {
            Object.Destroy(root);
            return null;
        }
        return root;
    }

    private static void AttachSword(GameObject root, Shader shader)
    {
        Mesh mesh = Resources.Load<Mesh>("__char/blue_war_pve_1_sword");
        if (mesh == null)
        {
            Debug.LogWarning("[CharacterBuilder] sword mesh missing");
            return;
        }
        Texture2D tex = Resources.Load<Texture2D>("__textures/blue_war_pve_1_sword_ds");
        var sword = new GameObject("Sword");
        sword.transform.SetParent(root.transform);
        sword.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = sword.AddComponent<MeshRenderer>();
        var mat = new Material(shader);
        if (tex != null) mat.mainTexture = tex;
        mr.sharedMaterial = mat;

        // Bind-pose right hand at x≈-0.53 y≈1.21; arm angles down/out slightly.
        // Place the grip (mesh origin) at the hand, blade angling down-forward.
        sword.transform.localPosition = new Vector3(-0.53f, 1.21f, 0.02f);
        sword.transform.localRotation = Quaternion.Euler(0f, 0f, -15f); // tilt blade forward
    }
}
