using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class FloorSpawner : DelayableMonoBehaviour
{
  public static FloorSpawner Instance { get; private set; }

  public class SpawnConfig
  {
    public bool enabled = true;
    public GameObject prefab;
    public Spawner spawner;
    public int overrideAttempts;
  }

  [System.Serializable]
  public class TimedSpawn : SpawnConfig
  {
    public AnimationCurve spawnProbability;
    public int spawnDuration;
    public float minSpawnInterval = 0f;
    public float maxSpawnInterval = -1f;

    [NonSerialized] public float lastSpawn;
  }

  [System.Serializable]
  public class ScheduledSpawn : SpawnConfig
  {
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
  private bool _navMeshReady = false;

  private Player _player;

  void Awake()
  {
    Instance = this;
  }

  void Start()
  {
    _player = FindFirstObjectByType<Player>();

    // Restart the timer
    Restart();
  }

  public void Restart()
  {
    _startTime = Time.time;
  }

  void Update()
  {
    float timeSinceStart = Time.time - _startTime;

    // Don't try spawning anything unless we have navmesh data
    if (!_navMeshReady) return;

    foreach (var spawnConfig in scheduledSpawns.Where(config => timeSinceStart >= config.time && !config.spawned && config.enabled))
    {
      int count = Random.Range(spawnConfig.minSpawnCount, spawnConfig.maxSpawnCount + 1);
      for (int i = 0; i < count; i++)
      {
        TrySpawnObjectOnFloor(spawnConfig);
      }

      // Set spawned regardless of success to avoid endless attempts -- for more flexible implementations, add a retry offset
      spawnConfig.spawned = true;
    }

    foreach (var spawnConfig in timedSpawns.Where(config => timeSinceStart <= config.spawnDuration && config.enabled))
    {
      // last spawn should at least be greater than the start time
      spawnConfig.lastSpawn = Mathf.Max(spawnConfig.lastSpawn, _startTime);

      // Spawn by probability
      float durationPercentage = timeSinceStart / spawnConfig.spawnDuration;
      float spawnProbability = spawnConfig.spawnProbability.Evaluate(durationPercentage);

      bool probabilitySpawn = (
        Random.value < (spawnProbability * Time.deltaTime) &&
        (Time.time - spawnConfig.lastSpawn) > spawnConfig.minSpawnInterval
      );

      // Spawn by max interval
      bool intervalSpawn = (
        !Mathf.Approximately(spawnProbability, 0f) &&
        spawnConfig.maxSpawnInterval > 0f &&
        (Time.time - spawnConfig.lastSpawn) > spawnConfig.maxSpawnInterval
      );

      if (intervalSpawn || probabilitySpawn)
      {
        bool spawned = TrySpawnObjectOnFloor(spawnConfig);
        if (spawned) spawnConfig.lastSpawn = Time.time;
      }
    }
  }

  private bool TrySpawnObjectOnFloor(SpawnConfig spawnConfig)
  {
    Vector3? location = GetValidSpawnLocation(spawnConfig);

    if (location.HasValue)
    {
      // Face player if available
      float yaw = 0;
      if (_player)
      {
        Vector3 direction = _player.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        Debug.Log($"[FloorSpawner] Spawning {spawnConfig.prefab.name} facing player with yaw {targetRotation.eulerAngles.y}");

        yaw = targetRotation.eulerAngles.y;
      }
      else
      {
        yaw = Random.Range(0f, 360f);
      }

      Quaternion rotation = Quaternion.Euler(Vector3.up * yaw);
      Debug.Log($"[FloorSpawner] Final rotation: {rotation.eulerAngles}");

      SpawnObject(spawnConfig, location.Value, rotation);
      return true; // Successfully spawned, no need to continue
    }

    return false;
  }

  private Vector3? GetValidSpawnLocation(SpawnConfig spawnConfig)
  {
    NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
    int attempts = spawnConfig.overrideAttempts == 0 ? _maxAttempts : spawnConfig.overrideAttempts;

    for (int attempt = 0; attempt < attempts; attempt++)
    {
      Vector3 randomPoint = GetRandomPoint(triangulation);

      if (
        Vector3.Distance(randomPoint, _player.transform.position) >= _minPlayerDistance
        && IsSpotUnobstructed(randomPoint, spawnConfig.prefab)
      ) return randomPoint;
    }

    Debug.LogWarning($"[FloorSpawner] Failed to find an unobstructed spot for {spawnConfig.prefab.name} after {attempts} attempts.");
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
        position,
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

  private void SpawnObject(SpawnConfig spawnConfig, Vector3 location, Quaternion rotation)
  {
    Debug.Log($"[FloowSpawner] Spawning {spawnConfig.prefab.name}");
    if (!spawnConfig.spawner) Instantiate(spawnConfig.prefab, location, rotation);
    else
    {
      Spawner spawnerObject = Instantiate(spawnConfig.spawner.gameObject).GetComponent<Spawner>();
      spawnerObject.prefab = spawnConfig.prefab;
      spawnerObject.targetPosition = location;
      spawnerObject.targetRotation = rotation;
    }
  }

  public void NavMeshReady()
  {
    _navMeshReady = true;
  }
}
