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
/// Controls (rendered by BattleUI on the arena root):
///   ATTACK  — sword swing, ~10-16 dmg          (player: hand_right, enemy: damage)
///   MAGIC   — arcane shot,   ~16-24 dmg       (player: magic_attack, enemy: damage_force)
///   BLOCK   — guard; incoming enemy attack reduced 70%  (player: block)
///   DODGE   — 60% chance to fully evade                   (player: dodge)
/// </summary>
public class CombatController : MonoBehaviour
{
    private const float MaxHP = 100f;

    // ---- Public accessors for BattleUI ----
    public float PlayerHP { get { return _playerHP; } }
    public float EnemyHP { get { return _enemyHP; } }
    public bool Busy { get { return _busy; } }
    public bool BattleOver { get { return _battleOver; } }
    public bool Victory { get { return _victory; } }
    public string Phase { get { return _phase; } }
    public bool IsReady { get { return _playerAnim != null && _playerAnim.IsReady; } }
    public void AdvanceArena() { AdvanceArenaImpl(); }
    public void PlayerAttack() { PlayerAttackImpl(); }
    public void PlayerMagic() { PlayerMagicImpl(); }
    public void PlayerBlock() { PlayerBlockImpl(); }
    public void PlayerDodge() { PlayerDodgeImpl(); }

    // ---- Internal implementation follows ----
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

    private int _arenaIndex = 1;        // which Arena_N is being played (advances on win)

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
        // The heavy arena build is done — hide the loading overlay.
        LoadingScreen.Hide();

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

    // ---- Turn actions (called by BattleUI) ------------------------------------

    private void PlayerAttackImpl()
    {
        if (_battleOver || _busy) return;
        StartCoroutine(Turn_Attack());
    }

    private void PlayerMagicImpl()
    {
        if (_battleOver || _busy) return;
        StartCoroutine(Turn_Magic());
    }

    private void PlayerBlockImpl()
    {
        if (_battleOver || _busy) return;
        _playerBlocking = true;
        _playerDodging = false;
        Play(_playerAnim, "block");
        _phase = "YOU BLOCK";
        StartCoroutine(WaitThenEnemyTurn(0.7f));
    }

    private void PlayerDodgeImpl()
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

    private void AdvanceArenaImpl()
    {
        _busy = false;
        if (_arenaRoot == null) return;
        if (_enemyGO != null) Destroy(_enemyGO);

        int next = _victory ? (_arenaIndex + 1) : _arenaIndex;
        if (next > 11) next = 1;
        _arenaIndex = next;

        Destroy(_arenaRoot);
        _arenaRoot = null;
        var fresh = ArenaBuilder.Build(_arenaIndex);
        if (fresh == null) { Debug.LogError("[Battle] arena rebuild failed"); return; }
        _arenaRoot = fresh;
        _phase = "ARENA " + _arenaIndex + " \u2014 FIGHT!";
        Debug.Log("[Battle] arena advanced to " + _arenaIndex);
    }
}