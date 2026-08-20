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

        // Spawn heroes (bind-pose meshes from __char): warrior center, others
        // positioned around the arena as opponents
        // The male warrior uses the FULL SKELETAL RIG (SkinnedRigBuilder) so it
        // animates with real bone curves from the original idle clip.
        GameObject skinned = null;
        try
        {
            skinned = SkinnedRigBuilder.Build("__anim/warrior_rig", "__anim/warrior_clips_full", "Warrior_Skinned");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[ArenaBuilder] skinned build threw: " + e.Message);
        }
        GameObject player = null;
        if (skinned != null)
        {
            skinned.transform.SetParent(root.transform);
            skinned.transform.position = new Vector3(0f, 0f, 0f);
            skinned.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // face camera
            player = skinned;
        }
        else
        {
            player = SpawnHero(root, CharacterBuilder.Variant.MaleWarrior, new Vector3(0f, 0f, 0f));
        }
        var foe = SpawnHero(root, CharacterBuilder.Variant.FemaleWarrior, new Vector3(3.2f, 0f, 2.5f));
        SpawnHero(root, CharacterBuilder.Variant.MaleAssassin, new Vector3(-3.2f, 0f, 2.5f));
        SpawnHero(root, CharacterBuilder.Variant.MaleMage, new Vector3(2.8f, 0f, -2.6f));
        // foe intentionally unused beyond spawning (see NOTE below)

        // ---- Register player/enemy for the camera state machine ----
        // BattleCameraController.LateUpdate re-inits when
        // Globals.PlayerGameObject != _player, so pointing it at our warrior
        // makes the camera track the player's bones (InitPlayer finds 'bones').
        if (player != null && Globals.PlayerGameObject != player)
        {
            player.name = Globals.PlayerName; // "__player"
            Globals.PlayerGameObject = player;
            Debug.Log("[ArenaBuilder] registered player for camera: " + player.name);
        }
        // NOTE: not registering Globals.Enemy — the Enemy component's StartImpl
        // expects a full Person rig and can throw; BattleCameraController handles
        // _enemy==null gracefully (UpdateBag/Fight guard on player only).

        // Ensure the battle camera root exists WITH a Camera component:
        // BattleCameraController finds GameObject.Find("camera") and calls
        // _camera.GetComponent<Camera>() — a root without Camera NREs.
        var camRoot = GameObject.Find(Globals.LocationGameObjectBattleCamera);
        if (camRoot == null)
        {
            camRoot = new GameObject(Globals.LocationGameObjectBattleCamera);
            camRoot.transform.position = new Vector3(4f, 2f, -5f);
        }
        if (camRoot.GetComponent<Camera>() == null)
        {
            var cam = camRoot.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.3f, 0.4f, 0.6f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            cam.fieldOfView = 60f;
            if (camRoot.GetComponent<AudioListener>() == null)
                camRoot.AddComponent<AudioListener>();
        }
        if (camRoot.GetComponent<StartBattleCamera>() == null && skinned != null &&
            GameObject.Find("arena_center") != null)
        {
            // Replace the debug orbit with a shoulder-ish static view.
            // Pull back far enough (radius ~6.5, height ~3) that the PLAYER and
            // ENEMY both fit in frame — the default 3.2/1.9 crops heads/edges.
            var sbc = camRoot.AddComponent<StartBattleCamera>();
            sbc.SetParams(6.5f, 3.0f, 0f); // speed 0 => no orbit, fixed offset
        }

        // ---- Turn-based battle controller ----
        // Root singleton to this arena, driving the real extracted clips via
        // player agency (attack/magic/block/dodge) against a skinned enemy.
        if (player != null)
        {
            CombatController.Instance.AttachArena(arenaIndex, root, player);
        }

        return root;
    }

    private static GameObject SpawnHero(GameObject root, CharacterBuilder.Variant variant, Vector3 pos)
    {
        var hero = CharacterBuilder.Build(variant);
        if (hero == null) return null;
        hero.transform.SetParent(root.transform);
        hero.transform.position = pos;
        var idle = hero.AddComponent<CharacterIdle>();
        idle.Enabled = true;
        // Slight variation so the scene isn't a mirror
        idle.SwaySpeed = 0.4f + (int)variant * 0.1f;
        return hero;
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