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

        // Directional light
        var lightGo = new GameObject("Directional Light");
        lightGo.transform.SetParent(root.transform);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.8f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        return root;
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