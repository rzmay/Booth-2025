using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FloorSpawner : DelayableMonoBehaviour
{
  public static FloorSpawner Instance { get; private set; }

  [System.Serializable]
  public class TimedSpawn
  {
    public GameObject prefab;
    public float spawnProbability;
  }

  [System.Serializable]
  public class StartupSpawn
  {
    public GameObject prefab;
    public int minSpawnCount;
    public int maxSpawnCount;
  }

  // Public fields for configuration
  public List<TimedSpawn> TimedSpawns = new List<TimedSpawn>();
  public List<StartupSpawn> StartupSpawns = new List<StartupSpawn>();
  [SerializeField] private int _maxAttempts = 50;
  [SerializeField] private float _startupDelay = 0.5f;

  private List<ARPlane> _trackedPlanes = new List<ARPlane>();

  private bool _spawnedStartupObjects = false;

  void Awake()
  {
    Instance = this;
    Debug.Log("[Floor Spawner] awake");
  }

  void Update()
  {
    foreach (var spawnConfig in TimedSpawns)
    {
      if (Random.value < (spawnConfig.spawnProbability * Time.deltaTime))
      {
        TrySpawnObjectOnFloor(spawnConfig.prefab);
      }
    }
  }

  public void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
  {
    if (changes.added.Count > 0)
    {
      Debug.Log($"[Floor Spawner] {changes.added.Count} planes added");
      foreach (var plane in changes.added)
      {
        OnPlaneAdded(plane);
      }
    }
  }

  private void OnPlaneAdded(ARPlane plane)
  {
    if (plane.classifications == PlaneClassifications.Floor)
    {
      Debug.Log("[Floor Spawner] Floor added");
      _trackedPlanes.Add(plane);

      if (!_spawnedStartupObjects)
      {
        _spawnedStartupObjects = true;

        Debug.Log("[Floor Spawner] Hell yeah adding stuff");
        // Quick and nasty method of waiting for all planes & stuff to be instantiated
        Delay(delegate () { SpawnStartupObjects(); }, _startupDelay);
      }
    }
  }

  private void SpawnStartupObjects()
  {
    Debug.Log("[Floor Spawner] Attempting spawn startup objects");
    foreach (var spawnConfig in StartupSpawns)
    {
      int count = Random.Range(spawnConfig.minSpawnCount, spawnConfig.maxSpawnCount + 1);
      for (int i = 0; i < count; i++)
      {
        TrySpawnObjectOnFloor(spawnConfig.prefab);
      }
    }
  }

  private void TrySpawnObjectOnFloor(GameObject prefab)
  {
    // Early return if no floor planes exist
    if (_trackedPlanes.Count == 0)
    {
      Debug.LogWarning("No floor planes available for spawning.");
      return;
    }

    for (int attempt = 0; attempt < _maxAttempts; attempt++)
    {
      ARPlane randomPlane = _trackedPlanes[Random.Range(0, _trackedPlanes.Count)];
      Vector3? location = GetValidSpawnLocation(randomPlane, prefab, out Quaternion rotation);

      if (location.HasValue)
      {
        SpawnObject(prefab, location.Value, rotation);
        return; // Successfully spawned, no need to continue
      }
    }

    Debug.LogWarning($"Failed to find an unobstructed spot for {prefab.name} after {_maxAttempts} attempts.");
  }

  private Vector3? GetValidSpawnLocation(ARPlane plane, GameObject prefab, out Quaternion validRotation)
  {
    validRotation = prefab.transform.rotation;

    for (int attempt = 0; attempt < _maxAttempts; attempt++)
    {
      Vector3 randomPoint = GetRandomPointOnPlane(plane, prefab);

      // Generate a random Y-axis rotation
      float randomYaw = Random.Range(0f, 360f);
      Quaternion randomRotation = prefab.transform.rotation * Quaternion.Euler(Vector3.up * randomYaw);

      if (IsSpotUnobstructed(randomPoint, prefab, randomRotation))
      {
        validRotation = randomRotation;
        return randomPoint;
      }
    }

    return null; // No valid location found
  }

  private Vector3 GetRandomPointOnPlane(ARPlane plane, GameObject prefab)
  {
    var planeCenter = plane.center;
    var planeSize = plane.size;
    BoxCollider collider = prefab.GetComponent<BoxCollider>();

    float spaceBuffer = Mathf.Max(collider.size.x, collider.size.z);

    float randomX = Random.Range(-(planeSize.x - spaceBuffer) / 2, (planeSize.x - spaceBuffer) / 2);
    float randomZ = Random.Range(-(planeSize.y - spaceBuffer) / 2, (planeSize.y - spaceBuffer) / 2);

    return planeCenter + new Vector3(randomX, 0.1f, randomZ);
  }

  public bool IsSpotUnobstructed(Vector3 position, GameObject prefab, Quaternion rotation)
  {
    // Retrieve the BoxCollider from the prefab
    BoxCollider prefabCollider = prefab.GetComponent<BoxCollider>();

    if (prefabCollider == null)
    {
      Debug.LogWarning($"Prefab {prefab.name} does not have a BoxCollider!");
      return false;
    }

    // Calculate the world space half-extents
    Vector3 halfExtents = Vector3.Scale(prefabCollider.size / 2, prefab.transform.localScale); // Component-wise scaling

    // Calculate the world space center
    Vector3 worldCenter = position + rotation * Vector3.Scale(prefabCollider.center, prefab.transform.localScale);

    // Perform the overlap box check
    Collider[] hitColliders = Physics.OverlapBox(worldCenter, halfExtents, rotation);

    foreach (var hitCollider in hitColliders)
    {
      // Check if the hit collider belongs to any of the tracked floor planes
      ARPlane plane = hitCollider.GetComponent<ARPlane>();
      if (plane == null || !_trackedPlanes.Contains(plane)) return false;
    }

    // No obstructions found
    return true;
  }

  private void SpawnObject(GameObject prefab, Vector3 location, Quaternion rotation)
  {
    Instantiate(prefab, location, rotation);
  }
}
