using UnityEngine;

/// <summary>
/// Oscillating hazard. As a pendulum it swings like a chandelier; it can also
/// slide horizontally or vertically. Damages the player on contact.
/// </summary>
public class MovingHazard : MonoBehaviour, IHazard
{
    public enum HazardMode { Pendulum, Horizontal, Vertical }

    [Header("Motion")]
    public HazardMode mode = HazardMode.Pendulum;
    public float pendulumAngle = 45f;
    public float frequency = 1.5f;
    public float travelDistance = 2.5f;

    [Header("Damage")]
    public float damage = 40f;
    public float DamageAmount => damage;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void Update()
    {
        float t = Mathf.Sin(Time.time * frequency);
        switch (mode)
        {
            case HazardMode.Pendulum:
                transform.localRotation = startRot * Quaternion.Euler(0f, 0f, t * pendulumAngle);
                break;
            case HazardMode.Horizontal:
                transform.localPosition = startPos + Vector3.right * t * travelDistance;
                break;
            case HazardMode.Vertical:
                transform.localPosition = startPos + Vector3.up * t * travelDistance;
                break;
        }
    }
}
