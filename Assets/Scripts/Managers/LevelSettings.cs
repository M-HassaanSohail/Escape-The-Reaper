using UnityEngine;

/// <summary>
/// Holds per-level tuning values. Placed once per level scene by the builder.
/// Ghosts read GhostChaseSpeed so the same Ghost prefab behaves differently per level.
/// </summary>
public class LevelSettings : MonoBehaviour
{
    public static LevelSettings Instance { get; private set; }

    [Tooltip("0 on Level 1 (ghosts only patrol). Higher on later levels.")]
    public float ghostChaseSpeed = 0f;

    void Awake()
    {
        Instance = this;
    }
}
