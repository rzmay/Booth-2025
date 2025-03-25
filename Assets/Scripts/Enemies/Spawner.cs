using System.Collections.Generic;
using UnityEngine;

public class Spawner : DelayableMonoBehaviour
{
  [System.NonSerialized] public GameObject prefab;
  [System.NonSerialized] public Vector3 targetPosition;
  [System.NonSerialized] public Quaternion targetRotation;
}
