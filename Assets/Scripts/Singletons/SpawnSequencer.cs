using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnSequencer : MonoBehaviour
{
  public static SpawnSequencer Instance { get; private set; }

  [System.Serializable]
  public class Phase
  {
    public List<FloorSpawner.TimedSpawn> timedSpawns;
    public List<FloorSpawner.ScheduledSpawn> scheduledSpawns;
  }

  public int initialPhase = 0;
  public List<Phase> phases = new();

  void Awake()
  {
    Instance = this;
  }

  void Start()
  {
    SetPhase(initialPhase);
  }

  public static void SetPhase(int index)
  {
    Debug.Log($"[SpawnSequencer] Set phase {index}");
    FloorSpawner.Instance.timedSpawns = Instance.phases[index].timedSpawns;
    FloorSpawner.Instance.scheduledSpawns = Instance.phases[index].scheduledSpawns;

    // Restart the floor spawner's time
    FloorSpawner.Instance.Restart();
  }
}
