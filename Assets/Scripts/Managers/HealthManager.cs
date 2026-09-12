using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple health: the player can take a fixed number of hits (default 2) before
/// dying. Every hazard counts as exactly one hit, regardless of its damage value,
/// so the bar always empties in maxHits hits. No lives / hearts.
/// </summary>
public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Config")]
    [Tooltip("How many hits the player can survive. The 2nd hit (by default) is Game Over.")]
    public int maxHits = 2;

    [Header("HUD (auto-wired by the builder)")]
    public Slider healthBar;

    public int HitsTaken { get; private set; }
    public bool IsDead => HitsTaken >= maxHits;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        HitsTaken = 0;
        RefreshUI();
    }

    /// <summary>Register one hit (damage amount ignored). Returns true if this was the killing hit.</summary>
    public bool TakeDamage(float amount)
    {
        if (IsDead) return true;

        HitsTaken++;
        RefreshUI();
        return HitsTaken >= maxHits;
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        if (HitsTaken > 0) HitsTaken--;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHits;
            healthBar.value = Mathf.Max(0, maxHits - HitsTaken);
        }
    }
}
