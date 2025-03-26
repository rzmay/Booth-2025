using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunManager : MonoBehaviour
{
  private static GunManager _Instance;

  [SerializeField]
  private List<GameObject> _guns = new();

  [SerializeField]
  private InputActionReference _switchAction;

  [SerializeField]
  private bool _allowSwitching = true;

  private int _gunIndex = 0;
  private GameObject _currentInstance;

  void Awake()
  {
    _Instance = this;
  }

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
    ContinuousGun continuousGun = gun.GetComponent<ContinuousGun>();

    if (standardGun)
    {
      MenuController.SetGun(standardGun.name, standardGun.damage, standardGun.rate);
    }
    else if (chargeGun)
    {
      MenuController.SetGun(chargeGun.name, chargeGun.externalDamage, chargeGun.rate, chargeGun.chargeRatio);
    }
    else if (continuousGun)
    {
      MenuController.SetGun(continuousGun.name, continuousGun.damage, 0);
    }
  }

  void DestroyCurrent()
  {
    Destroy(_currentInstance);
    _currentInstance = null;
  }

  public void SetGun(int index)
  {
    if (index < 0 || index >= _guns.Count) return;

    _gunIndex = index;

    Debug.Log($"Set gun: {_guns[_gunIndex].name}");
    InstantiateCurrent();
  }

  private void OnSwitchAction(InputAction.CallbackContext obj)
  {
    if (!_allowSwitching) return;

    Debug.Log($"[GunManager] Switching from {_gunIndex} to {(_gunIndex + 1) % _guns.Count} ({_gunIndex + 1} % {_guns.Count})");

    _gunIndex = (_gunIndex + 1) % _guns.Count;

    Debug.Log($"[GunManager] Switched gun: {_guns[_gunIndex].name}");
    InstantiateCurrent();
  }

  public static void DisableGuns()
  {
    _Instance.DestroyCurrent();
    _Instance._allowSwitching = false;
  }
}
