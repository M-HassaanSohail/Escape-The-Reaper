using UnityEngine;

/// <summary>
/// Ghost enemy with a simple state machine:
///   Patrol - floats side to side around its spawn point
///   Chase  - moves toward the player when in range (only if chaseSpeed > 0)
///   Dead   - plays die animation then is destroyed
/// Damages the player on contact.
/// </summary>
public class GhostEnemy : MonoBehaviour, IHazard
{
    public enum GhostState { Patrol, Chase, Dead }

    [Header("Patrol")]
    public float patrolAmplitude = 1.5f;
    public float patrolSpeed = 1.5f;
    public float bobAmplitude = 0.25f;
    public float bobSpeed = 2f;

    [Header("Chase")]
    [Tooltip("Set to 0 to disable chasing (Level 1).")]
    public float chaseSpeed = 0f;
    [Tooltip("If true, chaseSpeed is overridden by the scene's LevelSettings.")]
    public bool useLevelChaseSpeed = true;
    public float detectionRange = 6f;

    [Header("Damage")]
    public float damage = 50f;

    [Header("SFX (auto-wired by the builder)")]
    public AudioClip deathSFX;
    public AudioClip groanSFX;

    private static readonly int HashIsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int HashDie = Animator.StringToHash("Die");

    public float DamageAmount => damage;

    private GhostState state = GhostState.Patrol;
    private Vector3 origin;
    private float phase;
    private Animator animator;
    private Transform player;

    void Start()
    {
        origin = transform.position;
        phase = Random.value * Mathf.PI * 2f;
        animator = GetComponentInChildren<Animator>();

        if (useLevelChaseSpeed && LevelSettings.Instance != null)
            chaseSpeed = LevelSettings.Instance.ghostChaseSpeed;

        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }

    void Update()
    {
        if (state == GhostState.Dead) return;

        switch (state)
        {
            case GhostState.Patrol: Patrol(); break;
            case GhostState.Chase: Chase(); break;
        }
    }

    private void Patrol()
    {
        phase += Time.deltaTime * patrolSpeed;
        float x = origin.x + Mathf.Sin(phase) * patrolAmplitude;
        float y = origin.y + Mathf.Sin(phase * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(x, y, origin.z);

        if (chaseSpeed > 0f && player != null)
        {
            float dist = Mathf.Abs(player.position.z - transform.position.z);
            if (dist < detectionRange && player.position.z < transform.position.z + 1f)
            {
                state = GhostState.Chase;
                if (animator != null) animator.SetBool(HashIsAttacking, true);
                AudioManager.PlaySFXStatic(groanSFX);
            }
        }
    }

    private void Chase()
    {
        if (player == null) { state = GhostState.Patrol; return; }
        Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);

        // bob while chasing
        transform.position += Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude * Time.deltaTime;
    }

    /// <summary>Kill the ghost (e.g. if the player jumps on it from above).</summary>
    public void Die()
    {
        if (state == GhostState.Dead) return;
        state = GhostState.Dead;
        if (animator != null) animator.SetTrigger(HashDie);
        AudioManager.PlaySFXStatic(deathSFX);
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, 1.2f);
    }
}
