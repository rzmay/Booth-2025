using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private static MenuController _Instance;

    [SerializeField] private float _healthBarLerpFactor = 1f;
    [SerializeField] private List<GameObject> _menus; // Expecting 4 -- title, gameplay, lose, win
    [SerializeField] private TMP_Text _gunName;
    [SerializeField] private Image _gunCooldownMeter;
    [SerializeField] private Image _gunDamageMeter;
    [SerializeField] private Image _healthBar;
    [SerializeField] private TMP_Text _scoreMeter;
    [SerializeField] private TMP_Text _comboMeter;
    [SerializeField] private TMP_Text _gameOverScoreMeter;
    [SerializeField] private TMP_Text _gameOverComboMeter;


    // Saved to control charge meter graphics
    private float _chargeSpeed;

    // Save for smooth lerping
    private float _health;

    // For highest combo in game over screen
    private float _highestCombo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _Instance = this;

        // Set initial menu
        SetMenu(0);
    }

    // Update is called once per frame
    void Update()
    {
        // Lerp health meter
        if (Mathf.Approximately(_healthBar.fillAmount, _health))
        {
            _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _health, _healthBarLerpFactor * Time.deltaTime);
        }
    }

    void _SetMenu(int index)
    {
        for (int i = 0; i < _Instance._menus.Count; i++)
        {
            _Instance._menus[i].SetActive(i == index);
        }
    }

    void _SetGun(string name, float damage, float cooldown, float chargeSpeed)
    {
        _gunName.text = name;
        _chargeSpeed = chargeSpeed;

        _gunDamageMeter.rectTransform.sizeDelta = new Vector2(damage, 1);
        _gunCooldownMeter.rectTransform.sizeDelta = new Vector2(cooldown, 1);

        // If it's not a charge gun, have the charge meter full
        if (chargeSpeed == 0) _gunDamageMeter.fillAmount = 1f;

        // Gun cooldown meter should always start full
        _gunCooldownMeter.fillAmount = 1f;
    }

    void _SetCharge(float charge)
    {
        // Asymptotically towards full
        _gunDamageMeter.fillAmount = 1f - (1f / Mathf.Pow(charge + 1f, _chargeSpeed));
    }

    void _SetCooldown(float cooldown)
    {
        // Linearl fill
        _gunCooldownMeter.fillAmount = 1f - cooldown;
    }

    void _SetScore(int score, int combo)
    {
        // Set score meter
        _scoreMeter.text = score.ToString("0000");
        _gameOverScoreMeter.text = score.ToString("0000");

        // Set combo meter
        _comboMeter.text = combo.ToString();
        if (combo > _highestCombo)
        {
            _highestCombo = combo;
            _gameOverComboMeter.text = combo.ToString();
        }
    }

    void _SetHealth(float health)
    {
        _health = health;
    }

    public static void SetMenu(int index)
    {
        _Instance._SetMenu(index);
    }

    public static void SetGun(string name, float damage, float cooldown, float chargeSpeed = 0f)
    {
        _Instance._SetGun(name, damage, cooldown, chargeSpeed);
    }

    public static void SetCharge(float charge)
    {
        _Instance._SetCharge(charge);
    }

    public static void SetCooldown(float cooldown)
    {
        _Instance._SetCooldown(cooldown);
    }

    public static void SetScore(int score, int combo)
    {
        _Instance._SetScore(score, combo);
    }

    public static void SetHealth(float health)
    {
        _Instance._SetHealth(health);
    }
}
