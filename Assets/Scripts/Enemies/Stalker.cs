using UnityEngine;

/// <summary>
/// "Mremireh" the stalker. Trails behind the player and runs his Run animation.
/// Instead of blinking on/off, he RUNS AWAY (back and to the side, out of frame)
/// to disappear and RUNS BACK in to reappear. He's revealed for a few seconds at
/// the start, again after each hit, and during death - where he runs onto the
/// player, lifts the (still visible) body, carries it off into the dark, then asks
/// the GameManager to show the end screen.
/// </summary>
public class Stalker : MonoBehaviour
{
    public static Stalker Instance { get; private set; }

    [Header("References (auto-wired by the builder)")]
    public Transform player;

    [Header("Follow")]
    public float followDistance = 2f;     // revealed: just behind the player
    public float followLerp = 4f;
    public float turnSpeed = 9f;

    [Header("Hide / reveal (movement based)")]
    public float hideSideOffset = 6f;     // how far to the side he retreats
    public float hideBackOffset = 9f;     // how far back (past the camera) he retreats
    public float visibleDuration = 4f;    // seconds revealed before retreating

    [Header("Death carry")]
    public float approachTime = 0.7f;                               // run onto the body
    public Vector3 carryLocalPos = new Vector3(0f, 1.1f, 0.55f);    // where the body sits in his arms
    public Vector3 carryLocalEuler = new Vector3(-75f, 0f, 0f);
    public float carryShowTime = 2f;       // seconds carried in view before receding
    public float carryWalkSpeed = 2.2f;    // how fast he carries the body away
    public float carrySequenceDuration = 5.5f;

    static readonly int HashCarry = Animator.StringToHash("Carry");

    enum Mode { Revealed, Hidden, Death }
    private Mode mode = Mode.Revealed;
    private Animator animator;
    private CameraFollow cam;
    private float visibleTimer;
    private float hideSign = 1f;

    private float deathTimer;
    private bool carryGrabbed;
    private bool cameraReleased;

    void Awake()
    {
        Instance = this;
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
        }
        cam = FindFirstObjectByType<CameraFollow>();
        if (player != null) transform.position = VisiblePos();
        mode = Mode.Revealed;
        visibleTimer = visibleDuration;
    }

    Vector3 VisiblePos()
    {
        return new Vector3(player.position.x, player.position.y, player.position.z - followDistance);
    }

    Vector3 HiddenPos()
    {
        return new Vector3(player.position.x + hideSign * hideSideOffset,
                           player.position.y,
                           player.position.z - hideBackOffset);
    }

    void Update()
    {
        if (player == null) return;
        if (mode == Mode.Death) { UpdateDeath(); return; }

        Vector3 target = (mode == Mode.Revealed) ? VisiblePos() : HiddenPos();
        transform.position = Vector3.Lerp(transform.position, target, followLerp * Time.deltaTime);
        FaceMoveDir(target);

        if (mode == Mode.Revealed)
        {
            visibleTimer -= Time.deltaTime;
            if (visibleTimer <= 0f)
            {
                mode = Mode.Hidden;     // run away
                hideSign = -hideSign;   // alternate which side he flees to
            }
        }
    }

    void FaceMoveDir(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.05f) dir = Vector3.forward;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), turnSpeed * Time.deltaTime);
    }

    /// <summary>Player took a (non-fatal) hit: run back into view.</summary>
    public void OnPlayerHit()
    {
        if (mode == Mode.Death) return;
        mode = Mode.Revealed;
        visibleTimer = visibleDuration;
    }

    /// <summary>Player died: run onto the body, lift it, carry it away, then end.</summary>
    public void BeginDeathCarry()
    {
        if (mode == Mode.Death) return;
        mode = Mode.Death;
        deathTimer = 0f;
        carryGrabbed = false;
        cameraReleased = false;
    }

    void UpdateDeath()
    {
        deathTimer += Time.deltaTime;

        // 1) run onto the body
        if (deathTimer < approachTime)
        {
            Vector3 target = new Vector3(player.position.x, player.position.y, player.position.z - 0.4f);
            transform.position = Vector3.Lerp(transform.position, target, 10f * Time.deltaTime);
            FaceMoveDir(target);
            return;
        }

        // 2) lift the body (kept visible, parented to him) + play carry animation
        if (!carryGrabbed)
        {
            carryGrabbed = true;
            if (animator != null) animator.SetTrigger(HashCarry);
            GrabBody();
        }

        // 3) after a beat, let the camera go and carry the body off into the dark
        if (deathTimer >= approachTime + carryShowTime)
        {
            if (!cameraReleased)
            {
                cameraReleased = true;
                if (cam != null) cam.target = null; // freeze camera so he recedes from view
            }
            transform.position += Vector3.forward * carryWalkSpeed * Time.deltaTime;
        }

        // 4) end screen
        if (deathTimer >= carrySequenceDuration)
        {
            enabled = false;
            if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
        }
    }

    void GrabBody()
    {
        if (player == null) return;

        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        var pAnim = player.GetComponentInChildren<Animator>();
        if (pAnim != null) pAnim.enabled = false; // freeze the body's pose

        player.SetParent(transform, true);
        player.localPosition = carryLocalPos;
        player.localRotation = Quaternion.Euler(carryLocalEuler);
        // body stays visible - he is carrying it
    }
}
