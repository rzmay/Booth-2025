using UnityEngine;

public class LookAt : MonoBehaviour
{
  public Transform target; // The target your enemy is looking at
  public float rotationSpeed = 5f; // Speed of rotation interpolation
  public bool rotateX = true;
  public bool rotateY = true;
  public bool rotateZ = true;

  void Update()
  {
    SmoothLookAt();
  }

  private void SmoothLookAt()
  {
    Vector3 direction = (target?.position ?? transform.position + Vector3.forward) - transform.position;
    Quaternion targetRotation = Quaternion.LookRotation(direction);

    Vector3 eulerRotation = targetRotation.eulerAngles;
    Vector3 currentEuler = transform.rotation.eulerAngles;

    float x = rotateX ? eulerRotation.x : currentEuler.x;
    float y = rotateY ? eulerRotation.y : currentEuler.y;
    float z = rotateZ ? eulerRotation.z : currentEuler.z;

    Quaternion finalRotation = Quaternion.Euler(x, y, z);
    transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime * rotationSpeed);
  }
}
