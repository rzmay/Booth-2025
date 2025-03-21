using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunManager : MonoBehaviour
{
  [SerializeField]
  private List<GameObject> _guns = new();

  [SerializeField]
  private InputActionReference _switchAction;

  [SerializeField]
  private bool _allowSwitching = true;

  private int _gunIndex = 0;
  private GameObject _currentInstance;

  void Start()
  {
    if (_switchAction) _switchAction.action.performed += OnSwitchAction;

    InstantiateCurrent();
  }

  void OnDestroy()
  {
    if (_switchAction) _switchAction.action.performed -= OnSwitchAction;
    DestroyCurrent();
  }

  void InstantiateCurrent()
  {
    // Make sure there's no other instance
    if (_currentInstance != null) DestroyCurrent();

    // Instantiate the gun
    GameObject gun = _guns[_gunIndex];
    _currentInstance = Instantiate(_guns[_gunIndex], transform, false);

    // Set the menu
    StandardGun standardGun = gun.GetComponent<StandardGun>();
    ChargeGun chargeGun = gun.GetComponent<ChargeGun>();
    if (standardGun)
    {
      MenuController.SetGun(standardGun.name, standardGun.damage, standardGun.rate);
    }
    else if (chargeGun)
    {
      MenuController.SetGun(chargeGun.name, chargeGun.externalDamage, chargeGun.rate, chargeGun.chargeRatio);
    }
  }

  void DestroyCurrent()
  {
    Destroy(_currentInstance);
    _currentInstance = null;
  }

  public void SetGun(int index)
  {
    if (index > 0 && index < _guns.Count) _gunIndex = index;

    Debug.Log($"Set gun: {_guns[_gunIndex].name}");
    InstantiateCurrent();
  }

  private void OnSwitchAction(InputAction.CallbackContext obj)
  {
    if (!_allowSwitching) return;

    _gunIndex = (_gunIndex + 1) % _guns.Count;

    Debug.Log($"Switched gun: {_guns[_gunIndex].name}");
    InstantiateCurrent();
  }
}
