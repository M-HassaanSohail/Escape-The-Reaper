using UnityEngine;

/// <summary>
/// Lane-based endless-runner player. Auto-runs forward, switches between 3 lanes,
/// jumps and slides, drives the Animator, and reacts to coins / hazards / ghosts.
/// Uses a CharacterController (direct movement, no physics jitter).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 8f;
    public float maxForwardSpeed = 12f;
    public float speedRampPerSecond = 0.15f;
    public float laneDistance = 3.5f;
    public float laneChangeSpeed = 12f;
    public float jumpForce = 14f;
    public float gravity = -40f;

    [Header("Slide")]
    public float slideDuration = 0.9f;
    public float slideColliderHeight = 0.9f;

    [Header("Damage")]
    [Tooltip("Seconds of invulnerability after a hit so one hazard can't count twice.")]
    public float invulnerableDuration = 0.8f;
    private float lastHitTime = -99f;

    [Header("SFX (auto-wired by the builder)")]
    public AudioClip jumpSFX;
    public AudioClip damageSFX;
    public AudioClip deathSFX;
    public AudioClip slideSound;
    public AudioClip coinSFX;

    // animator parameter hashes (faster than strings)
    private static readonly int HashIsRunning = Animator.StringToHash("isRunning");
    private static readonly int HashIsSliding = Animator.StringToHash("isSliding");
    private static readonly int HashIsDead = Animator.StringToHash("isDead");
    private static readonly int HashJump = Animator.StringToHash("Jump");
    private static readonly int HashSpeedMul = Animator.StringToHash("speedMultiplier");

    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;
    private MobileInput mobileInput;

    private int currentLane = 1;            // 0 = left, 1 = middle, 2 = right
    private float verticalVel;
    private bool isSliding;
    private float slideTimer;
    private bool isDead;

    private float defaultHeight;
    private Vector3 defaultCenter;
    private Vector3 lastPosition;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        mobileInput = GetComponent<MobileInput>();

        defaultHeight = controller.height;
        defaultCenter = controller.center;
        lastPosition = transform.position;
    }

    void Start()
    {
        if (animator != null) animator.SetBool(HashIsRunning, true);
    }

    void Update()
    {
        if (isDead) { ApplyGravityOnly(); return; }

        HandleLaneInput();
        HandleJumpInput();
        HandleSlideInput();
        RampSpeed();
        MoveCharacter();
        UpdateScoreDistance();
    }

    // ---------------- input ----------------
    private void HandleLaneInput()
    {
        bool left = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        bool right = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        if (mobileInput != null) { left |= mobileInput.SwipeLeft; right |= mobileInput.SwipeRight; }

        if (left) currentLane = Mathf.Max(0, currentLane - 1);
        if (right) currentLane = Mathf.Min(2, currentLane + 1);
    }

    private void HandleJumpInput()
    {
        bool jump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        if (mobileInput != null) jump |= mobileInput.SwipeUp;

        if (jump && controller.isGrounded && !isSliding)
        {
            verticalVel = jumpForce;
            if (animator != null) animator.SetTrigger(HashJump);
            PlayClip(jumpSFX);
        }
    }

    private void HandleSlideInput()
    {
        bool slide = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftControl);
        if (mobileInput != null) slide |= mobileInput.SwipeDown;

        if (slide && controller.isGrounded && !isSliding)
            StartSlide();

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f) EndSlide();
        }
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        controller.height = slideColliderHeight;
        controller.center = new Vector3(defaultCenter.x, slideColliderHeight * 0.5f, defaultCenter.z);
        if (animator != null) animator.SetBool(HashIsSliding, true);
        PlayClip(slideSound);
    }

    private void EndSlide()
    {
        isSliding = false;
        controller.height = defaultHeight;
        controller.center = defaultCenter;
        if (animator != null) animator.SetBool(HashIsSliding, false);
    }

    // ---------------- movement ----------------
    private void RampSpeed()
    {
        if (forwardSpeed < maxForwardSpeed)
            forwardSpeed = Mathf.Min(maxForwardSpeed, forwardSpeed + speedRampPerSecond * Time.deltaTime);

        if (animator != null && maxForwardSpeed > 0f)
            animator.SetFloat(HashSpeedMul, Mathf.Clamp(forwardSpeed / 8f, 0.6f, 2f));
    }

    private void MoveCharacter()
    {
        // gravity / jump
        if (controller.isGrounded && verticalVel < 0f)
            verticalVel = -2f;
        verticalVel += gravity * Time.deltaTime;

        // target lane x
        float targetX = (currentLane - 1) * laneDistance;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        float deltaX = newX - transform.position.x;

        Vector3 motion = new Vector3(deltaX, verticalVel * Time.deltaTime, forwardSpeed * Time.deltaTime);
        controller.Move(motion);
    }

    private void ApplyGravityOnly()
    {
        if (controller.isGrounded && verticalVel < 0f) verticalVel = -2f;
        verticalVel += gravity * Time.deltaTime;
        controller.Move(new Vector3(0f, verticalVel * Time.deltaTime, 0f));
    }

    private void UpdateScoreDistance()
    {
        float dz = transform.position.z - lastPosition.z;
        if (dz > 0f && ScoreManager.Instance != null)
            ScoreManager.Instance.AddDistance(dz);
        lastPosition = transform.position;
    }

    // ---------------- collisions ----------------
    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        Coin coin = other.GetComponent<Coin>();
        if (coin != null) { coin.Collect(this); return; }

        ExitGate gate = other.GetComponent<ExitGate>();
        if (gate != null) { if (GameManager.Instance != null) GameManager.Instance.TriggerLevelComplete(); return; }

        IHazard hazard = other.GetComponent<IHazard>();
        if (hazard != null) { TakeDamage(hazard.DamageAmount); return; }
    }

    public void OnCoinCollected()
    {
        PlayClip(coinSFX);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        if (Time.time - lastHitTime < invulnerableDuration) return; // brief i-frames
        lastHitTime = Time.time;

        bool killed = false;
        if (HealthManager.Instance != null)
            killed = HealthManager.Instance.TakeDamage(amount);

        if (killed)
        {
            Die();
        }
        else
        {
            PlayClip(damageSFX);
            if (Stalker.Instance != null) Stalker.Instance.OnPlayerHit(); // reaper reappears
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        forwardSpeed = 0f;
        if (animator != null)
        {
            animator.SetBool(HashIsRunning, false);
            animator.SetBool(HashIsSliding, false);
            animator.SetBool(HashIsDead, true);
        }
        PlayClip(deathSFX);

        if (GameManager.Instance != null) GameManager.Instance.OnPlayerDying(); // stop the level timer

        if (Stalker.Instance != null)
            Stalker.Instance.BeginDeathCarry();          // reaper carries the body, then shows the end
        else if (GameManager.Instance != null)
            Invoke(nameof(ShowGameOver), 1.4f);          // fallback if no stalker present
    }

    private void ShowGameOver()
    {
        if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource != null) audioSource.PlayOneShot(clip);
        else AudioManager.PlaySFXStatic(clip);
    }
}
