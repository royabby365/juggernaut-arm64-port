using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Reconstructs a fully skinned, ANIMATED character from the original Unity 4.x
/// rig data exported by scripts/export_rig.py + scripts/export_clips.py.
///
/// The rig JSON contains:
///   skeleton : Transform hierarchy (parent + local pos/rot/scl per bone)
///   smr      : SkinnedMeshRenderer bone order (PPtrs -> Transform pids)
///   meshes   : verts/uvs/tris + per-vertex skin (4 bone idx + weights) +
///              bindposes (bone -> mesh space)
///
/// The clip JSON contains legacy animation curves (bone path -> pos/rot keys).
///
/// This produces a REAL skinned character: the armor parts deform with the
/// skeleton when the idle animation plays — not a static bind-pose shell.
/// </summary>
public static class SkinnedRigBuilder
{
    public static GameObject Build(string rigJsonPath, string clipJsonPath, string rootName = "Skinned_Warrior")
    {
        try
        {
            return BuildInternal(rigJsonPath, clipJsonPath, rootName);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[SkinnedRigBuilder] Build failed: " + e);
            return null;
        }
    }

    private static GameObject BuildInternal(string rigJsonPath, string clipJsonPath, string rootName)
    {
        TextAsset rigTa = Resources.Load<TextAsset>(rigJsonPath);
        if (rigTa == null)
        {
            Debug.LogWarning($"[SkinnedRigBuilder] rig JSON missing: {rigJsonPath}");
            return null;
        }

        var rig = JSON.Parse(rigTa.text);
        var root = new GameObject(rootName);

        // ---- 1. Build skeleton (Transform hierarchy) ----
        // Bones are named by their ORIGINAL GameObject names (from go_names) so
        // the animation clip paths (bones/bone_pelvis/bone_spine/...) match.
        var bones = new Dictionary<string, Transform>();
        var pathToBone = new Dictionary<string, Transform>();
        var goNames = rig["go_names"];
        var skeleton = rig["skeleton"];
        foreach (var kv in skeleton.Keys)
        {
            string boneName = "bone_" + kv;
            if (goNames != null && goNames[kv] != null)
                boneName = goNames[kv].str;
            if (string.IsNullOrEmpty(boneName)) boneName = "bone_" + kv;
            var b = new GameObject(boneName).transform;
            bones[kv] = b;
            b.SetParent(root.transform, false);
        }
        foreach (var kv in skeleton.Keys)
        {
            var node = skeleton[kv];
            string parent = node["parent"].str;
            if (parent != "0" && bones.ContainsKey(parent))
                bones[kv].SetParent(bones[parent], false);
            var p = node["pos"];
            var r = node["rot"];
            var s = node["scl"];
            bones[kv].localPosition = new Vector3(p[0].f, p[1].f, p[2].f);
            bones[kv].localRotation = new Quaternion(r[0].f, r[1].f, r[2].f, r[3].f);
            bones[kv].localScale = new Vector3(s[0].f, s[1].f, s[2].f);
        }
        // Map clip paths (bones/bone_pelvis/bone_spine/...) -> transforms by
        // walking each bone's parent chain.
        foreach (var kv in bones.Keys)
        {
            var t = bones[kv];
            var chain = new List<string> { t.name };
            var parent = t.parent;
            while (parent != null && parent != root.transform)
            {
                chain.Add(parent.name);
                parent = parent.parent;
            }
            chain.Reverse();
            pathToBone["bones/" + string.Join("/", chain.ToArray())] = t;
        }

        // ---- 2. Build skinned meshes ----
        Shader shader = Shader.Find("Hidden/JuggernautPlaceholder");
        var meshes = rig["meshes"];
        var smrs = rig["smr"];
        int built = 0;
        foreach (var smr in smrs.Nodes)
        {
            string meshKey = smr["mesh"].str;
            var md = meshes[meshKey];
            string meshName = md["name"].str;
            if (meshName == null) continue;

            // Mesh geometry
            var vertList = md["verts"];
            var uvList = md["uvs"];
            var triList = md["tris"];
            var skinList = md["skin"];
            var bindList = md["bindposes"];

            int nv = vertList.Count;
            var verts = new Vector3[nv];
            var uvs = new Vector2[nv];
            for (int i = 0; i < nv; i++)
            {
                verts[i] = new Vector3(vertList[i][0].f, vertList[i][1].f, vertList[i][2].f);
                if (i < uvList.Count)
                    uvs[i] = new Vector2(uvList[i][0].f, uvList[i][1].f);
            }
            var tris = new int[triList.Count];
            for (int i = 0; i < tris.Length; i++) tris[i] = triList[i].i;

            // Bone weights + bindposes
            int nb = bindList.Count;
            var bindposes = new Matrix4x4[nb];
            for (int b = 0; b < nb; b++)
            {
                var m = bindList[b];
                bindposes[b] = new Matrix4x4(
                    new Vector4(m[0].f, m[4].f, m[8].f, m[12].f),
                    new Vector4(m[1].f, m[5].f, m[9].f, m[13].f),
                    new Vector4(m[2].f, m[6].f, m[10].f, m[14].f),
                    new Vector4(m[3].f, m[7].f, m[11].f, m[15].f));
            }

            // Bone order from SMR (transform pids)
            var bonePids = smr["bones"];
            var boneTransforms = new Transform[bonePids.Count];
            for (int b = 0; b < bonePids.Count; b++)
            {
                string pid = bonePids[b].str;
                Transform t;
                if (bones.TryGetValue(pid, out t)) boneTransforms[b] = t;
            }

            var go = new GameObject(meshName);
            go.transform.SetParent(root.transform, false);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            var sm = go.AddComponent<SkinnedMeshRenderer>();

            var mesh = new Mesh();
            mesh.name = meshName;
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;

            // Bone weights (BoneWeight has up to 4 influences)
            var weights = new BoneWeight[nv];
            for (int i = 0; i < nv && i < skinList.Count; i++)
            {
                var s = skinList[i];
                var bw = new BoneWeight();
                bw.boneIndex0 = s[0].i; bw.weight0 = s[4].f;
                bw.boneIndex1 = s[1].i; bw.weight1 = s[5].f;
                bw.boneIndex2 = s[2].i; bw.weight2 = s[6].f;
                bw.boneIndex3 = s[3].i; bw.weight3 = s[7].f;
                weights[i] = bw;
            }
            mesh.boneWeights = weights;
            mesh.bindposes = bindposes;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            sm.sharedMesh = mesh;
            sm.bones = boneTransforms;
            if (smr["root"].str != "0" && bones.ContainsKey(smr["root"].str))
                sm.rootBone = bones[smr["root"].str];

            // Material
            var mat = new Material(shader != null ? shader : Shader.Find("Standard"));
            string texName = meshName + "_ds";
            Texture2D tex = Resources.Load<Texture2D>("__textures/" + texName);
            if (tex != null) mat.mainTexture = tex;
            mr.sharedMaterial = mat;
            built++;
        }

        // ---- 3. Idle animation driver ----
        if (clipJsonPath != null)
        {
            // Attach to the 'bones' skeleton root so clip paths (bones/...)
            // resolve from that node downward.
            Transform skelRoot = null;
            foreach (var kv in bones)
            {
                if (kv.Value.parent == root.transform)
                {
                    skelRoot = kv.Value;
                    break;
                }
            }
            var animGo = skelRoot != null ? skelRoot.gameObject : root;
            var anim = animGo.AddComponent<LegacyClipPlayer>();
            anim.rigJsonPath = rigJsonPath;
            anim.clipJsonPath = clipJsonPath;
            anim.clipName = "idle";
        }

        Debug.Log($"[SkinnedRigBuilder] built {built} skinned meshes, {bones.Count} bones");
        return root;
    }
}
