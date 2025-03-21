using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class FloorSpawner : DelayableMonoBehaviour
{
  public static FloorSpawner Instance { get; private set; }

  [System.Serializable]
  public class TimedSpawn
  {
    public GameObject prefab;
    public AnimationCurve spawnProbability;
    public int spawnDuration;
  }

  [System.Serializable]
  public class ScheduledSpawn
  {
    public GameObject prefab;
    public float time;
    public int minSpawnCount;
    public int maxSpawnCount;
    [NonSerialized] public bool spawned;
  }

  // Public fields for configuration
  public List<TimedSpawn> timedSpawns = new List<TimedSpawn>();
  public List<ScheduledSpawn> scheduledSpawns = new List<ScheduledSpawn>();
  [SerializeField] private int _maxAttempts = 50;
  [SerializeField] private LayerMask _obstructionMask = Physics.AllLayers;
  [SerializeField] private float _minPlayerDistance = 1f;

  private float _startTime;

  private Player _player;

  void Awake()
  {
    Instance = this;
  }

  void Start()
  {
    _player = FindFirstObjectByType<Player>();

    _startTime = Time.time;
  }

  public void Restart()
  {
    _startTime = Time.time;
  }

  void Update()
  {
    float timeSinceStart = Time.time - _startTime;

    foreach (var spawnConfig in scheduledSpawns.Where(config => timeSinceStart >= config.time && !config.spawned))
    {
      int count = Random.Range(spawnConfig.minSpawnCount, spawnConfig.maxSpawnCount + 1);
      for (int i = 0; i < count; i++)
      {
        TrySpawnObjectOnFloor(spawnConfig.prefab);
      }

      // Set spawned regardless of success to avoid endless attempts -- for more flexible implementations, add a retry offset
      spawnConfig.spawned = true;
    }

    foreach (var spawnConfig in timedSpawns)
    {
      float durationPercentage = timeSinceStart / spawnConfig.spawnDuration;
      if (Random.value < (spawnConfig.spawnProbability.Evaluate(durationPercentage) * Time.deltaTime))
      {
        TrySpawnObjectOnFloor(spawnConfig.prefab);
      }
    }
  }

  private bool TrySpawnObjectOnFloor(GameObject prefab)
  {
    Vector3? location = GetValidSpawnLocation(prefab);

    if (location.HasValue)
    {
      // Face player if available
      float yaw = 0;
      if (_player)
      {
        Vector3 direction = _player.transform.position - transform.position;
        Vector3 flatDirection = Vector3.ProjectOnPlane(direction, Vector3.up);

        yaw = Vector3.SignedAngle(transform.forward, flatDirection, Vector3.up);
      }
      else
      {
        yaw = Random.Range(0f, 360f);
      }

      Quaternion rotation = prefab.transform.rotation * Quaternion.Euler(Vector3.up * yaw);

      SpawnObject(prefab, location.Value, rotation);
      return true; // Successfully spawned, no need to continue
    }

    Debug.LogWarning($"[FloorSpawner] Failed to find an unobstructed spot for {prefab.name} after {_maxAttempts} attempts.");
    return false;
  }

  private Vector3? GetValidSpawnLocation(GameObject prefab)
  {
    NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

    for (int attempt = 0; attempt < _maxAttempts; attempt++)
    {
      Vector3 randomPoint = GetRandomPoint(triangulation);

      if (
        Vector3.Distance(randomPoint, _player.transform.position) >= _minPlayerDistance
        && IsSpotUnobstructed(randomPoint, prefab)
      ) return randomPoint;
    }

    return null; // No valid location found
  }

  public Vector3 GetRandomPoint(NavMeshTriangulation triangulation)
  {
    // Each triangle is defined by 3 consecutive indices.
    int triangleCount = triangulation.indices.Length / 3;

    // Pick a random triangle index.
    int randomTriangle = Random.Range(0, triangleCount);
    int index = randomTriangle * 3;

    // Get the triangle's vertex indices.
    int indexA = triangulation.indices[index];
    int indexB = triangulation.indices[index + 1];
    int indexC = triangulation.indices[index + 2];

    // Retrieve the actual vertices.
    Vector3 A = triangulation.vertices[indexA];
    Vector3 B = triangulation.vertices[indexB];
    Vector3 C = triangulation.vertices[indexC];

    // Generate random barycentric coordinates.
    float r1 = Random.value;
    float r2 = Random.value;

    // Ensure the point lies inside the triangle.
    if (r1 + r2 > 1f)
    {
      r1 = 1f - r1;
      r2 = 1f - r2;
    }

    // Return the random point inside the triangle.
    return A + r1 * (B - A) + r2 * (C - A);
  }

  public bool IsSpotUnobstructed(Vector3 position, GameObject prefab)
  {
    // Retrieve the BoxCollider from the prefab
    Collider prefabCollider = prefab.GetComponentInChildren<Collider>();

    if (prefabCollider == null) return false;

    Collider[] hitColliders = Physics.OverlapBox(
        prefabCollider.bounds.center,
        prefabCollider.bounds.extents,
        Quaternion.identity, // since bounds are axis-aligned
        _obstructionMask
    );

    // Check for obstructions
    foreach (Collider collider in hitColliders)
    {
      // Ignore colliding with the navmesh itself
      if (!Physics.GetIgnoreLayerCollision(prefab.layer, collider.gameObject.layer)) return false;
    }

    // No obstructions found
    return true;
  }

  private void SpawnObject(GameObject prefab, Vector3 location, Quaternion rotation)
  {
    Instantiate(prefab, location, rotation);
  }
}
