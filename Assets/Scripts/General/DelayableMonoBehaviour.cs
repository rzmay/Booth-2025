using UnityEngine;
using System;
using System.Collections;


public abstract class DelayableMonoBehaviour : MonoBehaviour
{
  // Method to initialize the config (optional, but good practice)
  public void Delay(Action action, float delay)
  {
    StartCoroutine(_Delay(action, delay));
  }

  public IEnumerator _Delay(Action action, float delay)
  {
    yield return new WaitForSeconds(delay);

    action.Invoke();
  }
}
