using UnityEditor;
using UnityEngine;

/// <summary>Batchmode diagnostic: parse the rig JSON and report structure.</summary>
public static class RigTest
{
    public static void Run()
    {
        var rigTa = Resources.Load<TextAsset>("__anim/warrior_rig");
        if (rigTa == null) { Debug.LogError("[RigTest] rig JSON missing"); return; }
        var rig = JSON.Parse(rigTa.text);
        var skeleton = rig["skeleton"];
        var goNames = rig["go_names"];
        var smrs = rig["smr"];
        var meshes = rig["meshes"];
        Debug.Log($"[RigTest] skeleton={skeleton.Count} go_names={(goNames != null ? goNames.Count : -1)} smr={(smrs != null ? smrs.Count : -1)} meshes={(meshes != null ? meshes.Count : -1)}");

        // test access patterns used by SkinnedRigBuilder
        int boneCount = 0;
        foreach (var kv in skeleton.Keys)
        {
            string bn = "bone_" + kv;
            if (goNames != null && goNames[kv] != null) bn = goNames[kv].str;
            var node = skeleton[kv];
            var p = node["pos"]; var r = node["rot"]; var s = node["scl"];
            float x = p[0].f; float y = p[1].f; float z = p[2].f;
            boneCount++;
        }
        Debug.Log($"[RigTest] parsed {boneCount} bone transforms OK");

        foreach (var smr in smrs.Nodes)
        {
            string meshKey = smr["mesh"].str;
            var md = meshes[meshKey];
            string meshName = md["name"].str;
            var vertList = md["verts"]; var uvList = md["uvs"]; var triList = md["tris"];
            var skinList = md["skin"]; var bindList = md["bindposes"];
            int nv = vertList.Count;
            float vx = vertList[0][0].f;
            int t0 = triList[0].i;
            Debug.Log($"[RigTest] mesh {meshKey} {meshName} nv={nv} uvs={uvList.Count} tris={triList.Count} skin={skinList.Count} bind={bindList.Count} v0.x={vx} tri0={t0}");
        }
        Debug.Log("[RigTest] done");

        // Full build test in editor (needs an active scene for GameObjects)
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
            UnityEditor.SceneManagement.NewSceneMode.Single);
        var built = SkinnedRigBuilder.Build("__anim/warrior_rig", "__anim/warrior_clips", "Warrior_Test");
        if (built != null)
        {
            Debug.Log("[RigTest] FULL BUILD OK: " + built.transform.childCount + " children");
        }
        else
        {
            Debug.LogWarning("[RigTest] FULL BUILD returned null");
        }
    }
}
