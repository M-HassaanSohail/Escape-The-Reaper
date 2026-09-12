using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns floor tile prefabs ahead of the player along +Z and destroys tiles that
/// fall behind. Tiles are picked from weighted pools, with harder pools gated by
/// how far the player has travelled (minDistance).
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TilePool
    {
        public string name = "Pool";
        public GameObject[] tiles;
        [Range(0f, 1f)] public float weight = 0.33f;
        [Tooltip("Player must have travelled at least this far before this pool can spawn.")]
        public float minDistance = 0f;
    }

    [Header("Layout")]
    public float tileLength = 20f;
    public int tilesAhead = 6;
    public int tilesTotal = 10;

    [Header("Pools")]
    public TilePool[] tilePools;

    [Header("References (auto-wired by the builder)")]
    public Transform playerTransform;

    private float spawnZ = 0f;
    private int safeTilesToStart = 2;
    private readonly List<GameObject> active = new List<GameObject>();

    void Start()
    {
        if (playerTransform == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) playerTransform = pc.transform;
        }

        // pre-spawn a runway: first couple are guaranteed safe
        for (int i = 0; i < tilesAhead; i++)
            SpawnTile(i < safeTilesToStart);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // spawn ahead
        if (playerTransform.position.z + (tilesAhead * tileLength) > spawnZ)
            SpawnTile(false);

        // recycle behind
        if (active.Count > 0)
        {
            GameObject first = active[0];
            if (first != null && first.transform.position.z < playerTransform.position.z - (tileLength * 2f))
            {
                active.RemoveAt(0);
                Destroy(first);
            }
            else if (first == null)
            {
                active.RemoveAt(0);
            }
        }
    }

    private void SpawnTile(bool forceSafe)
    {
        GameObject prefab = forceSafe ? GetSafePrefab() : PickWeightedPrefab();
        if (prefab == null) prefab = GetSafePrefab();
        if (prefab == null) { spawnZ += tileLength; return; }

        GameObject tile = Instantiate(prefab, new Vector3(0f, 0f, spawnZ), Quaternion.identity, transform);
        active.Add(tile);
        spawnZ += tileLength;

        // keep memory bounded
        while (active.Count > tilesTotal)
        {
            GameObject old = active[0];
            active.RemoveAt(0);
            if (old != null) Destroy(old);
        }
    }

    private GameObject GetSafePrefab()
    {
        if (tilePools == null || tilePools.Length == 0) return null;
        var pool = tilePools[0];
        if (pool.tiles == null || pool.tiles.Length == 0) return null;
        return pool.tiles[Random.Range(0, pool.tiles.Length)];
    }

    private GameObject PickWeightedPrefab()
    {
        float travelled = playerTransform != null ? playerTransform.position.z : 0f;

        // build the eligible set
        float totalWeight = 0f;
        List<TilePool> eligible = new List<TilePool>();
        foreach (var pool in tilePools)
        {
            if (pool.tiles == null || pool.tiles.Length == 0) continue;
            if (travelled < pool.minDistance) continue;
            eligible.Add(pool);
            totalWeight += pool.weight;
        }
        if (eligible.Count == 0 || totalWeight <= 0f) return GetSafePrefab();

        float r = Random.value * totalWeight;
        foreach (var pool in eligible)
        {
            r -= pool.weight;
            if (r <= 0f)
                return pool.tiles[Random.Range(0, pool.tiles.Length)];
        }
        return eligible[eligible.Count - 1].tiles[0];
    }
}
