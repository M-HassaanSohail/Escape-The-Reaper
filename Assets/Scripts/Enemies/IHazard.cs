using UnityEngine;

/// <summary>
/// Anything that damages the player on contact implements this.
/// PlayerController reads DamageAmount in OnTriggerEnter.
/// </summary>
public interface IHazard
{
    float DamageAmount { get; }
}

/// <summary>
/// Marker placed on the level-exit trigger. Touching it completes the level.
/// </summary>
public class ExitGate : MonoBehaviour
{
}
