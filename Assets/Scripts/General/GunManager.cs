using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GunManager : MonoBehaviour
{
  [SerializeField]
  private List<GameObject> _guns = new();

  [SerializeField]
  private InputActionReference _switchAction;

  private int _gunIndex = 0;
  private GameObject _currentInstance;

  void Start()
  {
    _switchAction.action.performed += OnSwitchAction;

    InstantiateCurrent();
  }

  void OnDestroy()
  {
    _switchAction.action.performed -= OnSwitchAction;
    DestroyCurrent();
  }

  void InstantiateCurrent()
  {
    // Make sure there's no other instance
    if (_currentInstance != null) DestroyCurrent();

    // Instantiate the gun
    _currentInstance = Instantiate(_guns[_gunIndex], transform, false);
  }

  void DestroyCurrent()
  {
    Destroy(_currentInstance);
    _currentInstance = null;
  }

  private void OnSwitchAction(InputAction.CallbackContext obj)
  {
    _gunIndex = (_gunIndex + 1) % _guns.Count;

    Debug.Log($"Switched gun: {_guns[_gunIndex].name}");
    InstantiateCurrent();
  }
}
