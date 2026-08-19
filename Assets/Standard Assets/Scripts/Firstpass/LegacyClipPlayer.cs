using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays legacy (non-humanoid) animation clips exported from the original
/// Unity 4.x bundles (scripts/export_clips.py) on a skeleton built by
/// SkinnedRigBuilder. The clip JSON has per-bone-path keyframe curves:
///   [{ clip, fps, curves: { "bones/bone_pelvis/...": { pos: [[t,x,y,z]...], rot: [[t,x,y,z,w]...] } } }]
///
/// Bone paths match the skeleton GameObject hierarchy names (bones/...).
/// Curves are evaluated with linear interpolation + wrap.
///
/// Supports a list of clips; when cycleClips is on, it plays through them in
/// order (idle, step, attacks, damage, death...) for a combat showcase.
/// </summary>
public class LegacyClipPlayer : MonoBehaviour
{
    public string clipJsonPath = "__anim/warrior_clips_full";
    public string[] clipNames = { "idle" };
    public float speed = 1f;
    public bool playOnStart = true;
    public bool cycleClips = true;
    public float cycleHold = 2.5f; // seconds to play each clip before switching

    private class Curve
    {
        public float[] Times;
        public Vector3[] Poses;
        public Quaternion[] Rots;
    }

    private class ClipData
    {
        public string Name;
        public float Length;
        public Dictionary<string, Curve> Curves = new Dictionary<string, Curve>();
    }

    private readonly List<ClipData> _clips = new List<ClipData>();
    private Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();
    private int _clipIdx = -1;
    private float _t;
    private float _inClip;
    private bool _loaded;

    void Start()
    {
        BuildBoneMap(transform);

        TextAsset clipTa = Resources.Load<TextAsset>(clipJsonPath);
        if (clipTa == null)
        {
            Debug.LogWarning("[LegacyClipPlayer] clip JSON missing: " + clipJsonPath);
            return;
        }
        var clips = JSON.Parse(clipTa.text);
        var wanted = new HashSet<string>(clipNames);
        foreach (var clip in clips.Arr)
        {
            string nm = clip["clip"].str;
            if (!wanted.Contains(nm)) continue;
            var cd = new ClipData { Name = nm };
            var curves = clip["curves"];
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
                        if (c.Times[i] > cd.Length) cd.Length = c.Times[i];
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
                            c.Times = new float[n];
                            c.Times[i] = (float)k[0].f;
                        }
                    }
                }
                cd.Curves[path] = c;
            }
            _clips.Add(cd);
        }
        _loaded = _clips.Count > 0;
        if (_loaded)
        {
            _clipIdx = 0;
            Debug.Log($"[LegacyClipPlayer] loaded {_clips.Count} clips: {string.Join(", ", _clips.ConvertAll(c => c.Name).ToArray())}");
        }
        else
        {
            Debug.LogWarning("[LegacyClipPlayer] no matching clips found");
        }
    }

    private void BuildBoneMap(Transform node)
    {
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
        _inClip += Time.deltaTime;

        var clip = _clips[_clipIdx];

        // Advance to next clip when the current one finishes (or after hold)
        if (_inClip >= Mathf.Max(clip.Length, 0.5f) + (cycleClips ? 0f : 0f))
        {
            if (cycleClips)
            {
                _clipIdx = (_clipIdx + 1) % _clips.Count;
                _inClip = 0f;
                clip = _clips[_clipIdx];
                Debug.Log($"[LegacyClipPlayer] clip: {clip.Name}");
            }
            else if (_inClip >= Mathf.Max(clip.Length, 0.5f) + cycleHold)
            {
                _inClip = 0f;
            }
        }

        float t = _inClip % Mathf.Max(clip.Length, 0.001f);
        foreach (var kv in clip.Curves)
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
                if (seg <= 0) seg = clip.Length;
                float f = seg > 0 ? Mathf.Clamp01((t - c.Times[i0]) / seg) : 0f;
                bone.localPosition = Vector3.Lerp(c.Poses[i0], c.Poses[i1], f);
            }
            if (c.Rots != null && idx >= 0)
            {
                int i0 = idx, i1 = idx + 1;
                if (i1 >= c.Times.Length) { i1 = 0; }
                float seg = c.Times[i1] - c.Times[i0];
                if (seg <= 0) seg = clip.Length;
                float f = seg > 0 ? Mathf.Clamp01((t - c.Times[i0]) / seg) : 0f;
                bone.localRotation = Quaternion.Slerp(c.Rots[i0], c.Rots[i1], f);
            }
        }
    }

    private static int FindKey(float[] times, float t)
    {
        if (times == null || times.Length == 0) return -1;
        for (int i = times.Length - 1; i >= 0; i--)
        {
            if (times[i] <= t) return i;
        }
        return times.Length - 1;
    }
}
