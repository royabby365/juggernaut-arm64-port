using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Self-contained TURN-BASED BATTLE controller for the Juggernaut arm64 port.
///
/// The original Battle.cs / Enemy.cs / Person.cs are wired to asset bundles and
/// server data that cannot run in this offline port. This controller drives the
/// REAL extracted animation clips (hand_right attack, damage, block, dodge,
/// magic_attack, death) on the skinned rigs built by SkinnedRigBuilder.
///
/// It is a DontDestroyOnLoad singleton so arena victory can rebuild the arena
/// (ArenaBuilder.Build) and re-attach to the fresh player without losing state.
///
/// Controls (OnGUI, landscape):
///   ATTACK  — sword swing, ~10-16 dmg          (player: hand_right, enemy: damage)
///   MAGIC   — arcane shot,   ~16-24 dmg       (player: magic_attack, enemy: damage_force)
///   BLOCK   — guard; incoming enemy attack reduced 70%  (player: block)
///   DODGE   — 60% chance to fully evade                   (player: dodge)
/// </summary>
public class CombatController : MonoBehaviour
{
    private const float MaxHP = 100f;

    // ---- Battle state ----
    private GameObject _arenaRoot;      // current Arena_N root (rebuilt on victory)
    private GameObject _playerGO;      // player (for enemy LookAt + placement)
    private GameObject _enemyGO;
    private LegacyClipPlayer _playerAnim;
    private LegacyClipPlayer _enemyAnim;

    private float _playerHP = MaxHP;
    private float _enemyHP = MaxHP;

    private bool _busy;                 // a turn is mid-animation
    private bool _playerBlocking;
    private bool _playerDodging;
    private bool _battleOver;
    private bool _victory;

    private string _phase = "FIGHT!";

    // ---- Singleton access -----------------------------------------------------
    private static CombatController _instance;
    public static CombatController Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("CombatController");
                _instance = go.AddComponent<CombatController>();
                DontDestroyOnLoad(go);
                Debug.Log("[Battle] singleton created");
            }
            return _instance;
        }
    }

    /// <summary>
    /// Called after ArenaBuilder.Build creates a fresh arena + skinned player.
    /// Roots the battle to that arena, spawns a skinned enemy, and re-uses the
    /// extracted animation clips driven by gameplay instead of the showcase.
    /// </summary>
    public void AttachArena(int arenaIndex, GameObject arenaRoot, GameObject player)
    {
        if (_enemyGO != null) Destroy(_enemyGO);

        _arenaIndex = arenaIndex;
        _arenaRoot = arenaRoot;
        _playerGO = player;
        _playerAnim = player != null ? player.GetComponentInChildren<LegacyClipPlayer>() : null;

        _playerHP = MaxHP;
        _enemyHP = MaxHP;
        _busy = false;
        _playerBlocking = false;
        _playerDodging = false;
        _battleOver = false;
        _victory = false;
        _phase = "FIGHT!";

        if (_playerAnim != null)
        {
            // Kill the showcase auto-cycle; keep it breathing in idle.
            _playerAnim.cycleClips = false;
            _playerAnim.playOnStart = true; // loops idle
        }
        else
        {
            Debug.LogWarning("[Battle] player has no LegacyClipPlayer");
        }

        SpawnEnemy();
        Debug.Log("[Battle] attached. you " + _playerHP + ", enemy " + _enemyHP);
    }

    private void SpawnEnemy()
    {
        if (_arenaRoot == null) return;
        GameObject enemy = null;
        try
        {
            enemy = SkinnedRigBuilder.Build("__anim/warrior_rig", "__anim/warrior_clips_full", "Warrior_Enemy");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Battle] enemy rig build threw: " + e.Message);
        }
        if (enemy == null)
        {
            Debug.LogError("[Battle] failed to spawn enemy rig");
            return;
        }

        _enemyGO = enemy;
        enemy.transform.SetParent(_arenaRoot.transform);
        // Place enemy to the player's left, in front of the arena, facing the player.
        // (Do NOT put it at z=-1.8 dead-center — that sits between the camera and
        //  the player and occludes the player entirely.)
        Vector3 playerPos = _playerGO != null ? _playerGO.transform.position : Vector3.zero;
        // Put the enemy on the CAMERA side of the player (camera is at z=-3.2) and
        // offset to the player's left, so BOTH fighters fit in the frontal frame.
        // (x=-2.6,z=+1.4 pushed it behind the player / off-frame.)
        enemy.transform.position = playerPos + new Vector3(-2.0f, 0f, -1.2f);
        enemy.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        // Face the enemy toward the player (LookAt makes +z point at target).
        if (_playerGO != null)
        {
            var look = _playerGO.transform.position;
            look.y = enemy.transform.position.y;
            enemy.transform.LookAt(look);
            // SkinnedModel faces +z by default; keep the wobble axis upright.
            enemy.transform.Rotate(0f, 0f, 0f);
        }

        _enemyAnim = enemy.GetComponentInChildren<LegacyClipPlayer>();
        if (_enemyAnim != null)
        {
            _enemyAnim.cycleClips = false;
            _enemyAnim.playOnStart = true;
        }
        else
        {
            Debug.LogWarning("[Battle] enemy has no LegacyClipPlayer");
        }

        // Tint enemy materials so the two rigs are visually distinct.
        Color tint = new Color(0.82f, 0.2f, 0.24f);
        foreach (var smr in enemy.GetComponentsInChildren<SkinnedMeshRenderer>())
            if (smr.sharedMaterial != null) smr.sharedMaterial.color = tint;
        foreach (var mr in enemy.GetComponentsInChildren<MeshRenderer>())
            if (mr.sharedMaterial != null) mr.sharedMaterial.color = tint;

        Debug.Log("[Battle] enemy rig spawned");
    }

    // ---- Turn actions (called by OnGUI) --------------------------------------

    private void PlayerAttack()
    {
        if (_battleOver || _busy) return;
        StartCoroutine(Turn_Attack());
    }

    private void PlayerMagic()
    {
        if (_battleOver || _busy) return;
        StartCoroutine(Turn_Magic());
    }

    private void PlayerBlock()
    {
        if (_battleOver || _busy) return;
        _playerBlocking = true;
        _playerDodging = false;
        Play(_playerAnim, "block");
        _phase = "YOU BLOCK";
        StartCoroutine(WaitThenEnemyTurn(0.7f));
    }

    private void PlayerDodge()
    {
        if (_battleOver || _busy) return;
        _playerDodging = true;
        _playerBlocking = false;
        Play(_playerAnim, "dodge");
        _phase = "YOU DODGE";
        StartCoroutine(WaitThenEnemyTurn(0.7f));
    }

    private IEnumerator Turn_Attack()
    {
        _busy = true; _phase = "YOU ATTACK";
        Play(_playerAnim, "hand_right");
        yield return new WaitForSeconds(0.4f);
        if (!_battleOver) Play(_enemyAnim, "damage");
        float dmg = Random.Range(10f, 16f);
        _enemyHP = Mathf.Max(0, _enemyHP - dmg);
        _phase = "HIT! -" + Mathf.RoundToInt(dmg);
        yield return new WaitForSeconds(0.4f);
        CheckEnemyDown();
        if (!_battleOver) yield return WaitThenEnemyTurn(0.5f);
        _busy = false;
    }

    private IEnumerator Turn_Magic()
    {
        _busy = true; _phase = "YOU CAST";
        Play(_playerAnim, "magic_attack");
        yield return new WaitForSeconds(0.6f);
        if (!_battleOver) Play(_enemyAnim, "damage");
        float dmg = Random.Range(16f, 24f);
        _enemyHP = Mathf.Max(0, _enemyHP - dmg);
        _phase = "ARCANE -" + Mathf.RoundToInt(dmg);
        yield return new WaitForSeconds(0.55f);
        CheckEnemyDown(); // enemy may be down from the magic hit
        if (!_battleOver) yield return WaitThenEnemyTurn(0.6f);
        _busy = false;
    }

    private IEnumerator WaitThenEnemyTurn(float pre)
    {
        _busy = true;
        yield return new WaitForSeconds(pre);
        yield return EnemyTurn();
        _busy = false;
    }

    private IEnumerator EnemyTurn()
    {
        if (_battleOver) yield break;

        // Enemy picks attack or magic.
        bool magic = Random.value < 0.45f;
        Play(_enemyAnim, magic ? "magic_attack" : "hand_left");
        _phase = magic ? "ENEMY CASTS" : "ENEMY ATTACKS";
        yield return new WaitForSeconds(0.55f);

        // Damage the player, respecting block / dodge.
        float dmg;
        if (_playerDodging && Random.value < 0.6f)
        {
            dmg = 0f; _phase = "DODGED!";
            Play(_playerAnim, "dodge");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            dmg = magic ? Random.Range(14f, 22f) : Random.Range(10f, 16f);
            if (_playerBlocking) dmg *= 0.3f;
            _playerHP = Mathf.Max(0, _playerHP - dmg);
            if (!_battleOver) Play(_playerAnim, "damage");
            _phase = "YOU TAKE -" + Mathf.RoundToInt(dmg);
            yield return new WaitForSeconds(0.45f);
        }

        _playerBlocking = false;
        _playerDodging = false;

        if (_playerHP <= 0)
        {
            _victory = false; _battleOver = true;
            Play(_playerAnim, "death");
            _phase = "DEFEAT";
            Debug.Log("[Battle] player defeated");
        }
    }

    private void CheckEnemyDown()
    {
        if (_enemyHP <= 0)
        {
            _victory = true; _battleOver = true;
            Play(_enemyAnim, "death");
            _phase = "VICTORY";
            Debug.Log("[Battle] enemy defeated");
        }
    }

    private void Play(LegacyClipPlayer anim, string clip)
    {
        if (anim == null) return;
        anim.Play(clip);
    }

    private int _arenaIndex = 1;       // which Arena_N is being played (advances on win)

    private void AdvanceArena()
    {
        _busy = false;
        if (_arenaRoot == null) return;
        if (_enemyGO != null) Destroy(_enemyGO);

        int next = _victory ? (_arenaIndex + 1) : _arenaIndex;
        if (next > 11) next = 1; // the 11 extracted arenas (Arena_1 .. Arena_11)
        _arenaIndex = next;

        Destroy(_arenaRoot); // tear down old arena (player & geometry)
        _arenaRoot = null;
        // NOTE: ArenaBuilder.Build itself calls Instance.AttachArena(arenaIndex,...)
        // which re-roots to the fresh arena, re-registers Globals.PlayerGameObject,
        // and spawns the enemy. So do NOT call AttachArena here again (that caused
        // a redundant double-attach / double enemy spawn).
        var fresh = ArenaBuilder.Build(_arenaIndex);
        if (fresh == null) { Debug.LogError("[Battle] arena rebuild failed"); return; }
        // _arenaRoot/_playerAnim refreshed by the Build's internal AttachArena.
        _arenaRoot = fresh;
        _phase = "ARENA " + _arenaIndex + " — FIGHT!";
        Debug.Log("[Battle] arena advanced to " + _arenaIndex);
    }

    // ---- OnGUI (landscape layout, scaled) -----------------------------------
    void OnGUI()
    {
        float w = Screen.width, h = Screen.height;
        float s = Mathf.Max(0.5f, Mathf.Min(w / 1920f, h / 1080f));

        DrawHPBars(s, w, h);

        float bannerY = 120f * s;
        float cw = 700f * s;
        var st = new GUIStyle(GUI.skin.label);
        st.fontSize = Mathf.RoundToInt(56 * s);
        st.alignment = TextAnchor.MiddleCenter;
        GUI.color = _battleOver
            ? (_victory ? new Color(0.2f, 0.85f, 0.35f) : new Color(0.9f, 0.25f, 0.25f))
            : Color.white;
        string banner = _battleOver ? (_victory ? "VICTORY" : "DEFEAT") : _phase;
        GUI.Label(new Rect(w / 2 - cw / 2, bannerY, cw, 70 * s), banner, st);
        GUI.color = Color.white;

        if (_battleOver)
        {
            var btn = new GUIStyle(GUI.skin.button);
            btn.fontSize = Mathf.RoundToInt(32 * s);
            float bw = 360 * s, bh = 84 * s;
            if (GUI.Button(new Rect(w / 2 - bw / 2, 460 * s, bw, bh),
                           _victory ? "NEXT ARENA" : "RETRY", btn))
                AdvanceArena();
        }
        else if (!_busy && _playerAnim != null && _playerAnim.IsReady)
        {
            DrawActionButtons(s, w, h);
        }
    }

    private void DrawActionButtons(float s, float w, float h)
    {
        float bw = 220 * s, bh = 78 * s, gap = 26 * s;
        float y = h - bh - 48 * s;
        float total = 4 * bw + 3 * gap;
        float x0 = (w - total) / 2f;
        var st = new GUIStyle(GUI.skin.button);
        st.fontSize = Mathf.RoundToInt(30 * s);
        st.alignment = TextAnchor.MiddleCenter;

        if (GUI.Button(new Rect(x0, y, bw, bh), "ATTACK", st)) PlayerAttack();
        if (GUI.Button(new Rect(x0 + bw + gap, y, bw, bh), "MAGIC", st)) PlayerMagic();
        if (GUI.Button(new Rect(x0 + 2 * (bw + gap), y, bw, bh), "BLOCK", st)) PlayerBlock();
        if (GUI.Button(new Rect(x0 + 3 * (bw + gap), y, bw, bh), "DODGE", st)) PlayerDodge();
    }

    private void DrawHPBars(float s, float w, float h)
    {
        float barW = 480f * s, barH = 30f * s, y = 60f * s;

        // Player (left)
        GUI.Label(new Rect(50 * s, y - 24 * s, 300 * s, 22 * s),
                  "YOU  " + Mathf.CeilToInt(_playerHP) + "/" + MaxHP,
                  new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(20 * s) });
        GUI.backgroundColor = Color.black;
        GUI.Box(new Rect(50 * s, y, barW, barH), "");
        float pp = Mathf.Clamp01(_playerHP / MaxHP);
        GUI.backgroundColor = _playerHP > 35 ? new Color(0.25f, 0.8f, 0.3f) : new Color(0.9f, 0.25f, 0.2f);
        GUI.Box(new Rect(50 * s, y, barW * pp, barH), "");

        // Enemy (right)
        float ex = w - 50 * s - barW;
        var es = new GUIStyle(GUI.skin.label);
        es.fontSize = Mathf.RoundToInt(20 * s);
        es.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(ex, y - 24 * s, barW, 20 * s),
                  "ENEMY  " + Mathf.CeilToInt(_enemyHP) + "/" + MaxHP, es);
        GUI.backgroundColor = Color.black;
        GUI.Box(new Rect(ex, y, barW, barH), "");
        float ep = Mathf.Clamp01(_enemyHP / MaxHP);
        GUI.backgroundColor = _enemyHP > 35 ? new Color(0.8f, 0.35f, 0.25f) : new Color(0.9f, 0.8f, 0.2f);
        GUI.Box(new Rect(ex, y, barW * ep, barH), "");

        GUI.backgroundColor = Color.white;
    }
}