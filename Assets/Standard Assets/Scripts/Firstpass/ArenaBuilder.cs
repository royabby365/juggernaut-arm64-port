using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds arena backgrounds from extracted textures (stored in Resources/__textures/).
/// Replaces placeholder colored geometry with real painted arena backdrops.
///
/// Each arena has 2x 1024x1024 background paintings + floor decals extracted
/// from the original Unity 4.x bundles via UnityPy.
/// </summary>
public static class ArenaBuilder
{
    // Map arena index -> background texture names extracted from OBB scene bundles
    private static readonly Dictionary<int, ArenaTextures> ArenaMap = new Dictionary<int, ArenaTextures>
    {
        { 1,  new ArenaTextures { Bg01 = "arena_01_bg_01", Bg02 = "arena_01_bg_02", Floor = "01_tile" } },
        { 2,  new ArenaTextures { Bg01 = "02_bg_01",       Bg02 = "02_bg_02",       Floor = "ground" } },
        { 3,  new ArenaTextures { Bg01 = "arena_03_bg_01", Bg02 = "arena_03_bg_02", Floor = "02_004" } },
        { 4,  new ArenaTextures { Bg01 = "03_bg_01",       Bg02 = "03_bg_02",       Floor = "03_bg_02" } },
        { 5,  new ArenaTextures { Bg01 = "05_bg_01",       Bg02 = "05_bg_02",       Floor = "ground" } },
        { 6,  new ArenaTextures { Bg01 = "arena_06_01",     Bg02 = "arena_06_02",    Floor = "06_tile" } },
        { 7,  new ArenaTextures { Bg01 = "07_bg_01",       Bg02 = "07_bg_02",       Floor = "ground" } },
        { 8,  new ArenaTextures { Bg01 = "arena_08_bg_01", Bg02 = "arena_08_bg_02", Floor = "ground_01" } },
        { 9,  new ArenaTextures { Bg01 = "09_bg_01",       Bg02 = "09_bg_02",       Floor = "ground" } },
        { 10, new ArenaTextures { Bg01 = "arena10_bg_01",  Bg02 = "arena10_bg_02",  Floor = "ground_01" } },
        { 11, new ArenaTextures { Bg01 = "11_bg_01",       Bg02 = "11_bg_02",       Floor = "ground" } },
    };

    private struct ArenaTextures
    {
        public string Bg01;
        public string Bg02;
        public string Floor;
    }

    /// <summary>
    /// Build the visual arena for the given level index.
    /// Creates a root GameObject with background planes, floor, and markers.
    /// </summary>
    public static GameObject Build(int arenaIndex, Transform parent = null)
    {
        var root = new GameObject($"Arena_{arenaIndex}");
        if (parent != null)
            root.transform.SetParent(parent);

        ArenaTextures tex;
        if (!ArenaMap.TryGetValue(arenaIndex, out tex))
        {
            // Fall back to a minimal placeholder
            AddFallbackArena(root);
            return root;
        }

        // Load textures from Resources/__textures/
        Texture2D bgTex1 = Resources.Load<Texture2D>($"__textures/{tex.Bg01}");
        Texture2D bgTex2 = Resources.Load<Texture2D>($"__textures/{tex.Bg02}");

        // Try to build the REAL baked arena geometry from Resources/__arena<N>
        // (extracted + bind-baked from the original OBB scene bundle). Falls
        // back to the painted quad background below if meshes are missing.
        bool realGeometry = BuildArenaGeometry(root, arenaIndex);

        if (!realGeometry)
        {
            // Create background — a large quad behind the arena
            if (bgTex1 != null)
            {
                var bg = CreateTexturedQuad($"Background_{arenaIndex}_1", bgTex1, 12f, 9f);
                bg.transform.SetParent(root.transform);
                bg.transform.position = new Vector3(0, 2.5f, -12); // Behind everything
                bg.transform.Rotate(0, 0, 0);
                // Flip for correct orientation (the texture is a painting seen front-on)
                bg.transform.localScale = new Vector3(12, 9, 1);
            }

            if (bgTex2 != null)
            {
                // Second background plane — often a different depth layer
                var bg2 = CreateTexturedQuad($"Background_{arenaIndex}_2", bgTex2, 10f, 7.5f);
                bg2.transform.SetParent(root.transform);
                bg2.transform.position = new Vector3(0, 2.5f, -11.5f);
                bg2.transform.localScale = new Vector3(10, 7.5f, 1);
            }

            // Floor
            Texture2D floorTex = Resources.Load<Texture2D>($"__textures/{tex.Floor}");
            if (floorTex != null)
            {
                var floor = CreateTexturedQuad("Floor", floorTex, 10f, 10f);
                floor.transform.SetParent(root.transform);
                floor.transform.position = new Vector3(0, -0.5f, 0);
                floor.transform.Rotate(-90, 0, 0); // Flat on ground
                floor.transform.localScale = new Vector3(10, 10, 1);
            }
        }

        // Directional light
        var lightGo = new GameObject("Directional Light");
        lightGo.transform.SetParent(root.transform);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.8f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Spawn the baked warrior at arena center (bind-pose meshes from __char)
        var warrior = CharacterBuilder.Build(CharacterBuilder.Variant.Male);
        if (warrior != null)
        {
            warrior.transform.SetParent(root.transform);
            warrior.transform.position = new Vector3(0f, 0f, 0f);
            var idle = warrior.AddComponent<CharacterIdle>();
            idle.Enabled = true;
        }

        // Spawn the female blue warrior as an opponent across the arena
        var foe = CharacterBuilder.Build(CharacterBuilder.Variant.Female, "Warrior_Blue_F_Pve1");
        if (foe != null)
        {
            foe.transform.SetParent(root.transform);
            foe.transform.position = new Vector3(3.2f, 0f, 2.5f);
            var idle2 = foe.AddComponent<CharacterIdle>();
            idle2.Enabled = true;
        }

        return root;
    }

    /// <summary>
    /// Build the arena from the baked bind-pose geometry (Resources/__arena&lt;N&gt;).
    /// Each part OBJ was baked at its world transform from the OBB scene bundle,
    /// so parts are simply parented at the origin. Returns false if no meshes.
    /// </summary>
    private static bool BuildArenaGeometry(GameObject root, int arenaIndex)
    {
        Shader shader = Shader.Find("Hidden/JuggernautPlaceholder");
        if (shader == null) return false;

        string folder = $"__arena{arenaIndex}";
        Mesh[] meshes = Resources.LoadAll<Mesh>(folder);
        if (meshes == null || meshes.Length == 0) return false;

        foreach (Mesh mesh in meshes)
        {
            string nm = mesh.name;
            var go = new GameObject(nm);
            go.transform.SetParent(root.transform);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            Texture2D tex = ResolveArenaTexture(nm, arenaIndex);
            var mat = new Material(shader);
            if (tex != null) mat.mainTexture = tex;
            else mat.color = new Color(0.55f, 0.58f, 0.62f);
            mr.sharedMaterial = mat;
        }
        Debug.Log($"[ArenaBuilder] real arena geometry loaded ({meshes.Length} parts) for index {arenaIndex}");
        return true;
    }

    private static Texture2D ResolveArenaTexture(string partName, int arenaIndex)
    {
        // Material convention (see scripts/bake_arena.py fallback map).
        // Match by role prefix because baked parts carry _NNN dedupe suffixes.
        // Floor/wall/background texture names vary per arena bundle.
        string texName = null;
        if (partName.StartsWith("Plane001")) texName = FloorTexture(arenaIndex);          // base floor
        else if (partName.StartsWith("floor")) texName = FloorTexture(arenaIndex);        // tiled floor
        else if (partName.StartsWith("Plane003")) texName = FloorDecalTexture(arenaIndex); // floor decals
        else if (partName.StartsWith("center")) texName = FloorDecalTexture(arenaIndex);
        else if (partName.StartsWith("background")) texName = BgTexture(arenaIndex);       // back wall
        else if (partName.StartsWith("Plane002")) texName = BgTexture2(arenaIndex);        // side wall
        else texName = FloorTexture(arenaIndex); // generic geometry (msh01*, etc.)
        return Resources.Load<Texture2D>($"__textures/{texName}");
    }

    private static string FloorTexture(int i)
    {
        // Real floor material texture names per arena (from bake rig.json)
        switch (i)
        {
            case 1: return "01_tile";
            case 2: return "02_2";
            case 3: return "02_003";
            case 4: return "90917_3_3378_111822_texsture_04";
            case 5: return "05_02";
            case 6: return "06_tile";
            case 7: return "07_2";
            case 8: return "08_02";
            case 9: return "09_1";
            case 10: return "10_001";
            default: return "01_tile";
        }
    }

    private static string FloorDecalTexture(int i)
    {
        switch (i)
        {
            case 1: return "arena_08_floor_decals";
            case 2: return "arena_04_floor_decals";
            case 3: return "arena_03_floor_decals";
            case 4: return "arena_04_floor_decals";
            case 5: return "arena_03_floor_decals";
            case 6: return "arena_06_floor_decals";
            case 7: return "ground_d";
            case 8: return "arena_08_floor_decals";
            case 9: return "ground_d";
            case 10: return "arena_10_floor_decals";
            default: return "arena_08_floor_decals";
        }
    }

    private static string BgTexture(int i)
    {
        switch (i)
        {
            case 1: return "arena_01_bg_01";
            case 2: return "02_bg_01";
            case 3: return "arena_03_bg_01";
            case 4: return "03_bg_01";
            case 5: return "05_bg_01";
            case 6: return "arena_06_01";
            case 7: return "07_bg_01";
            case 8: return "arena_08_bg_01";
            case 9: return "09_bg_01";
            case 10: return "arena10_bg_01";
            default: return "arena_01_bg_01";
        }
    }

    private static string BgTexture2(int i)
    {
        switch (i)
        {
            case 1: return "arena_01_bg_02";
            case 2: return "02_bg_02";
            case 3: return "arena_03_bg_02";
            case 4: return "03_bg_02";
            case 5: return "05_bg_02";
            case 6: return "arena_06_02";
            case 7: return "07_bg_02";
            case 8: return "arena_08_bg_02";
            case 9: return "09_bg_02";
            case 10: return "arena10_bg_02";
            default: return "arena_01_bg_02";
        }
    }

    private static GameObject CreateTexturedQuad(string name, Texture2D texture, float width, float height)
    {
        var go = new GameObject(name);
        var mf = go.AddComponent<MeshFilter>();
        var mesh = new Mesh();

        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, 0.5f, 0)
        };
        mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;

        var mr = go.AddComponent<MeshRenderer>();
        var shader = Shader.Find("Hidden/JuggernautPlaceholder");
        if (shader != null)
        {
            var mat = new Material(shader);
            mat.mainTexture = texture;
            mat.color = Color.white;
            mr.sharedMaterial = mat;
        }

        go.transform.localScale = new Vector3(width, height, 1);
        return go;
    }

    private static void AddFallbackArena(GameObject root)
    {
        // Simple colored quad fallback — same as placeholder but with the arena index
        var ground = CreateColoredQuad("Ground", new Color(0.4f, 0.45f, 0.5f));
        ground.transform.SetParent(root.transform);
        ground.transform.position = new Vector3(0, -0.5f, 0);
        ground.transform.Rotate(-90, 0, 0);
        ground.transform.localScale = new Vector3(10, 10, 1);

        var lightGo = new GameObject("Directional Light");
        lightGo.transform.SetParent(root.transform);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.8f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    private static GameObject CreateColoredQuad(string name, Color color)
    {
        var go = new GameObject(name);
        var mf = go.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f)
        };
        mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        var shader = Shader.Find("Hidden/JuggernautPlaceholder");
        if (shader != null)
        {
            var mat = new Material(shader);
            mat.color = color;
            mr.sharedMaterial = mat;
        }
        return go;
    }
}