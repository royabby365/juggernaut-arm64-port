using UnityEngine;

/// <summary>
/// Gentle idle animation for the baked bind-pose warrior: a slow vertical bob
/// (breathing) plus a subtle rotation sway. Runs on the character root so all
/// armor parts move together.
///
/// IL2CPP-safe: pure Transform math, no AnimationClip needed (the original 4.x
/// clips can't be imported yet).
/// </summary>
public class CharacterIdle : MonoBehaviour
{
    public bool Enabled = true;
    public float BobHeight = 0.03f;
    public float BobSpeed = 1.6f;
    public float SwayAngle = 2.5f;
    public float SwaySpeed = 0.5f;

    private Vector3 _basePos;
    private Quaternion _baseRot;
    private float _t;

    void Start()
    {
        _basePos = transform.position;
        _baseRot = transform.rotation;
    }

    void Update()
    {
        if (!Enabled) return;
        _t += Time.deltaTime;

        // Breathing bob
        float bob = Mathf.Sin(_t * BobSpeed) * BobHeight;
        transform.position = _basePos + new Vector3(0f, bob, 0f);

        // Slow sway
        float sway = Mathf.Sin(_t * SwaySpeed) * SwayAngle;
        transform.rotation = _baseRot * Quaternion.Euler(0f, sway, 0f);
    }
}
