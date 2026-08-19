using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays legacy (non-humanoid) animation clips exported from the original
/// Unity 4.x bundles (scripts/export_clips.py) on a skeleton built by
/// SkinnedRigBuilder. The clip JSON has per-bone-path keyframe curves:
///   { clip, fps, curves: { "bones/bone_pelvis/...": { pos: [[t,x,y,z]...], rot: [[t,x,y,z,w]...] } } }
///
/// Bone paths match the skeleton GameObject hierarchy names (bones/...).
/// Curves are evaluated with linear interpolation + wrap.
/// </summary>
public class LegacyClipPlayer : MonoBehaviour
{
    public string rigJsonPath = "__anim/warrior_rig";
    public string clipJsonPath = "__anim/warrior_clips";
    public string clipName = "idle";
    public float speed = 1f;
    public bool playOnStart = true;

    private class Curve
    {
        public float[] Times;
        public Vector3[] Poses;
        public Quaternion[] Rots;
    }

    private Dictionary<string, Curve> _curves = new Dictionary<string, Curve>();
    private Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();
    private float _clipLen;
    private float _t;
    private bool _loaded;

    void Start()
    {
        // Find the skeleton: bones were parented directly under this GO
        // (SkinnedRigBuilder sets the skeleton root as this component's GO child).
        BuildBoneMap(transform);

        TextAsset clipTa = Resources.Load<TextAsset>(clipJsonPath);
        if (clipTa == null)
        {
            Debug.LogWarning("[LegacyClipPlayer] clip JSON missing: " + clipJsonPath);
            return;
        }
        var clips = JSON.Parse(clipTa.text);
        foreach (var clip in clips.Arr)
        {
            if (clip["clip"].str != clipName) continue;
            var curves = clip["curves"];
            _clipLen = 0f;
            foreach (var path in curves.Keys)
            {
                var node = curves[path];
                var c = new Curve();
                if (node["pos"] != null)
                {
                    int n = node["pos"].Count;
                    c.Times = new float[n];
                    c.Poses = new Vector3[n];
                    for (int i = 0; i < n; i++)
                    {
                        var k = node["pos"][i];
                        c.Times[i] = (float)k[0].f;
                        c.Poses[i] = new Vector3((float)k[1].f, (float)k[2].f, (float)k[3].f);
                        if (c.Times[i] > _clipLen) _clipLen = c.Times[i];
                    }
                }
                if (node["rot"] != null)
                {
                    int n = node["rot"].Count;
                    c.Rots = new Quaternion[n];
                    for (int i = 0; i < n; i++)
                    {
                        var k = node["rot"][i];
                        c.Rots[i] = new Quaternion((float)k[1].f, (float)k[2].f, (float)k[3].f, (float)k[4].f);
                        if (c.Times == null)
                        {
                            // shouldn't happen (pos usually present) but guard
                            if (c.Times == null) { c.Times = new float[n]; }
                            c.Times[i] = (float)k[0].f;
                        }
                    }
                }
                _curves[path] = c;
            }
            break;
        }
        _loaded = _curves.Count > 0;
        if (_loaded)
            Debug.Log($"[LegacyClipPlayer] loaded '{clipName}' ({_curves.Count} bone curves, {_clipLen:F2}s)");
    }

    private void BuildBoneMap(Transform node)
    {
        // Register this node by its hierarchy path
        var chain = new List<string> { node.name };
        var parent = node.parent;
        while (parent != null)
        {
            chain.Add(parent.name);
            parent = parent.parent;
        }
        chain.Reverse();
        _bones["bones/" + string.Join("/", chain.ToArray())] = node;
        for (int i = 0; i < node.childCount; i++)
            BuildBoneMap(node.GetChild(i));
    }

    void Update()
    {
        if (!_loaded || !playOnStart) return;
        _t += Time.deltaTime * speed;
        float t = _t % _clipLen;
        foreach (var kv in _curves)
        {
            Transform bone;
            if (!_bones.TryGetValue(kv.Key, out bone)) continue;
            var c = kv.Value;
            int idx = FindKey(c.Times, t);
            if (c.Poses != null && idx >= 0)
            {
                int i0 = idx, i1 = idx + 1;
                if (i1 >= c.Times.Length) { i1 = 0; }
                float seg = c.Times[i1] - c.Times[i0];
                if (seg <= 0) seg = _clipLen;
                float f = seg > 0 ? Mathf.Clamp01((t - c.Times[i0]) / seg) : 0f;
                bone.localPosition = Vector3.Lerp(c.Poses[i0], c.Poses[i1], f);
            }
            if (c.Rots != null && idx >= 0)
            {
                int i0 = idx, i1 = idx + 1;
                if (i1 >= c.Times.Length) { i1 = 0; }
                float seg = c.Times[i1] - c.Times[i0];
                if (seg <= 0) seg = _clipLen;
                float f = seg > 0 ? Mathf.Clamp01((t - c.Times[i0]) / seg) : 0f;
                bone.localRotation = Quaternion.Slerp(c.Rots[i0], c.Rots[i1], f);
            }
        }
    }

    /// <summary>Index of the last keyframe at or before t.</summary>
    private static int FindKey(float[] times, float t)
    {
        for (int i = times.Length - 1; i >= 0; i--)
        {
            if (times[i] <= t) return i;
        }
        return times.Length - 1; // wrap: t before first key -> last key
    }
}
