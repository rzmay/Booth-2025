using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuController : DelayableMonoBehaviour
{
    private static MenuController _Instance;

    [SerializeField] private float _healthBarLerpFactor = 1f;
    [SerializeField] private List<GameObject> _menus; // Expecting 4 -- title, gameplay, lose, win
    [SerializeField] private TMP_Text _gunDetectedName;
    [SerializeField] private TMP_Text _gunName;
    [SerializeField] private Image _gunCooldownMeter;
    [SerializeField] private Image _gunDamageMeter;
    [SerializeField] private Image _healthBarL;
    [SerializeField] private Image _healthBarR;
    [SerializeField] private Gradient _healthGradient;
    [SerializeField] private TMP_Text _scoreMeter;
    [SerializeField] private TMP_Text _comboMeter;
    [SerializeField] private ParticleSystem _comboSparks;
    [SerializeField] private TMP_Text _gameOverScoreMeter;
    [SerializeField] private TMP_Text _gameOverComboMeter;
    [SerializeField] private TMP_Text _gameOverPlayAgain;
    [SerializeField] private TMP_Text _victoryScoreMeter;
    [SerializeField] private TMP_Text _victoryComboMeter;
    [SerializeField] private TMP_Text _victoryPlayAgain;


    [SerializeField] private float _acceptRestartDelay;
    [SerializeField] private InputActionReference _restartAction;

    private bool _acceptRestart;


    // Saved to control charge meter graphics
    private float _chargeSpeed;

    // Save for smooth lerping
    private float _health;

    // For highest combo in game over screen
    private float _highestCombo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _Instance = this;
    }

    void Start()
    {
        _restartAction.action.performed += OnRestartAction;

        // Health starts full
        _health = 1f;
        _healthBar.fillAmount = 1f;

        // Set initial menu
        SetMenu(0);
    }

    void OnDestroy()
    {
        _restartAction.action.performed -= OnRestartAction;
    }

    // Update is called once per frame
    void Update()
    {
        // Lerp health meter
        if (!Mathf.Approximately(_healthBar.fillAmount, _health))
        {
            _healthBarL.fillAmount = Mathf.Lerp(_healthBarL.fillAmount, _health, _healthBarLerpFactor * Time.deltaTime);
            _healthBarR.fillAmount = Mathf.Lerp(_healthBarR.fillAmount, _health, _healthBarLerpFactor * Time.deltaTime);
            _healthBarL.color = _healthGradient.Evaluate(_healthBarL.fillAmount);
            _healthBarR.color = _healthGradient.Evaluate(_healthBarR.fillAmount);
        }
    }

    void _SetMenu(int index)
    {
        Debug.Log($"[MenuController] Set menu ${index}");
        for (int i = 0; i < _Instance._menus.Count; i++)
        {
            if (_menus[i].activeSelf != (i == index)) _menus[i].SetActive(i == index);
        }

        // If it's game over / victory accept restart input
        if (index == 2 || index == 3)
        {
            Delay(() =>
            {
                _acceptRestart = true;
                _victoryPlayAgain.gameObject.SetActive(true);
                _gameOverPlayAgain.gameObject.SetActive(true);
            }, _acceptRestartDelay);
        }
    }

    void _SetGun(string name, float damage, float cooldown, float chargeSpeed)
    {
        _gunDetectedName.gameObject.SetActive(true);
        _gunDetectedName.text = name;

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

    void _SetScore(int score, int combo, float comboTime)
    {
        // Set score meter
        _scoreMeter.text = score.ToString("0000");
        _gameOverScoreMeter.text = score.ToString("0000");
        _victoryScoreMeter.text = score.ToString("0000");

        // Set combo meter
        _comboMeter.text = combo.ToString("0000");

        // Set combo particles
        var emission = _comboSparks.emission;
        emission.rateOverTime = Mathf.Clamp(comboTime, 0, 30);

        // Store highest combo
        if (combo > _highestCombo)
        {
            _highestCombo = combo;
            _gameOverComboMeter.text = combo.ToString("0000");
            _victoryComboMeter.text = combo.ToString("0000");
        }
    }

    void _SetHealth(float health)
    {
        _health = health;
    }

    void OnRestartAction(InputAction.CallbackContext obj)
    {
        if (_acceptRestart) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    public static void SetScore(int score, int combo, float comboTime)
    {
        _Instance._SetScore(score, combo, comboTime);
    }

    public static void SetHealth(float health)
    {
        _Instance._SetHealth(health);
    }
}
