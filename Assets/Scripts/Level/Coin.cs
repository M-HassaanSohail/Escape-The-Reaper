using UnityEngine;

/// <summary>
/// Spinning collectible. On contact with the player it awards score, plays a
/// chime, optionally spawns a particle, and destroys itself.
/// </summary>
public class Coin : MonoBehaviour
{
    public int value = 10;
    public float spinSpeed = 120f;
    public GameObject collectParticle;

    private bool collected;

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    public void Collect(PlayerController player)
    {
        if (collected) return;
        collected = true;

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(value);
        if (player != null) player.OnCoinCollected();

        if (collectParticle != null)
            Instantiate(collectParticle, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
