using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GlowBillboard : MonoBehaviour
{
  void Update()
  {
    if (Application.isPlaying)
    {
      UpdateRotation(Camera.main);
    }
  }

  void OnDrawGizmos()
  {
    if (!enabled) return;

#if UNITY_EDITOR
    if (!Application.isPlaying)
    {
      SceneView sceneView = SceneView.lastActiveSceneView;
      if (sceneView != null && sceneView.camera != null) UpdateRotation(sceneView.camera);
    }
#endif
  }

  void UpdateRotation(Camera targetCamera)
  {
    if (targetCamera == null) return;

    // Step 1: Get the direction from the sprite to the camera in world space
    Vector3 directionToCamera = targetCamera.transform.position - transform.position;

    // Step 2: Get the parent's up vector (handles arbitrary parent rotations)
    Transform parent = transform.parent;
    Vector3 parentUp = parent != null ? parent.up : Vector3.up;

    // Step 3: Project the direction onto the parent's horizontal plane to ignore vertical differences
    Vector3 projectedDirection = Vector3.ProjectOnPlane(directionToCamera, parentUp).normalized;

    // If the projected direction is too small, skip rotation to avoid errors
    if (projectedDirection.sqrMagnitude < 0.001f)
      return;

    // Step 4: Calculate the desired world rotation to face the camera on the horizontal plane
    Quaternion desiredWorldRotation = Quaternion.LookRotation(projectedDirection, parentUp);

    // Step 5: Convert the desired world rotation to the child's local rotation relative to the parent
    Quaternion parentRotation = parent != null ? parent.rotation : Quaternion.identity;
    Quaternion localDesiredRotation = Quaternion.Inverse(parentRotation) * desiredWorldRotation;

    // Step 6: Extract only the Y-axis rotation from the local desired rotation
    Vector3 localEulerAngles = localDesiredRotation.eulerAngles;
    localEulerAngles.x = 0; // Lock X-axis rotation
    localEulerAngles.z = 0; // Lock Z-axis rotation

    // Step 7: Apply the rotation to the sprite's local Y-axis
    transform.localRotation = Quaternion.Euler(localEulerAngles);
  }
}
