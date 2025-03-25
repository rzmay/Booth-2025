using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneProcessing : MonoBehaviour
{
  public bool smoothNormals = true;
  public bool removeCeiling = true;

  public void Process()
  {
    if (!isActiveAndEnabled) return;

    if (removeCeiling)
    {
      RemoveCeiling[] removeCeilings = FindObjectsByType<RemoveCeiling>(FindObjectsSortMode.None);
      foreach (RemoveCeiling r in removeCeilings)
      {
        r.RemoveMeshCeiling();
      }
    }

    if (smoothNormals)
    {
      SmoothNormals[] smoothNormals = FindObjectsByType<SmoothNormals>(FindObjectsSortMode.None);
      foreach (SmoothNormals s in smoothNormals)
      {
        s.SmoothMeshNormals();
      }
    }
  }
}
