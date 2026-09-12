using UnityEngine;

/// <summary>
/// Static spike trap. Damages the player on contact (trigger collider).
/// </summary>
public class SpikeTrap : MonoBehaviour, IHazard
{
    public float damage = 35f;
    public float DamageAmount => damage;
}
