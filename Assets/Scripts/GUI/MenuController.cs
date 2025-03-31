using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuController : DelayableMonoBehaviour
{
    private static MenuController _Instance;

    [SerializeField] private float _healthBarLerpFactor = 5f;
    [SerializeField] private float _menuLerpFactor = 5f;

    [SerializeField] private List<GameObject> _menus; // Expecting 4 -- title, gameplay, lose, win
    [SerializeField] private TMP_Text _gunDetectedName;
    [SerializeField] private TMP_Text _gunName;
    [SerializeField] private MenuBar _gunCooldownMeter;
    [SerializeField] private Image _gunCooldownMeterImage;
    [SerializeField] private Gradient _gunCooldownGradient;
    [SerializeField] private MenuBar _gunDamageMeter;
    [SerializeField] private TMP_Text _gunCooldownLabel;
    [SerializeField] private Image _healthBarR;
    [SerializeField] private Image _healthBarL;
    [SerializeField] private Gradient _healthBarGradient;
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

    private List<bool> _menuActive;

    private bool _acceptRestart;


    // Saved to control charge meter graphics
    private float _chargeSpeed;

    // Save for smooth lerping
    private float _health;

    // For highest combo in game over screen
    private int _highestCombo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _Instance = this;
    }

    void Start()
    {
        _restartAction.action.performed += OnRestartAction;
        _menuActive = new(new bool[_menus.Count]);

        // Health starts full
        _health = 1f;
        _healthBarL.fillAmount = 1f;
        _healthBarR.fillAmount = 1f;

        // Set health bar color
        _healthBarL.color = _healthBarGradient.Evaluate(_healthBarL.fillAmount);
        _healthBarR.color = _healthBarGradient.Evaluate(_healthBarR.fillAmount);

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
        if (!Mathf.Approximately(_healthBarL.fillAmount, _health))
        {
            _healthBarL.fillAmount = Mathf.Lerp(_healthBarL.fillAmount, _health, _healthBarLerpFactor * Time.deltaTime);
            _healthBarR.fillAmount = Mathf.Lerp(_healthBarR.fillAmount, _health, _healthBarLerpFactor * Time.deltaTime);
            _healthBarL.color = _healthBarGradient.Evaluate(_healthBarL.fillAmount);
            _healthBarR.color = _healthBarGradient.Evaluate(_healthBarR.fillAmount);
        }
        else
        {
            _healthBarL.fillAmount = _health;
            _healthBarR.fillAmount = _health;
        }

        // Lerp menu opacity
        for (int i = 0; i < _menus.Count; i++)
        {
            CanvasGroup group = _menus[i].GetComponent<CanvasGroup>();
            if (!group) continue;

            float targetAlpha = _menuActive[i] ? 1f : 0f;
            if (!Mathf.Approximately(group.alpha, targetAlpha))
            {
                group.alpha = Mathf.Lerp(group.alpha, targetAlpha, _menuLerpFactor * Time.deltaTime);
            }
            else
            {
                group.alpha = targetAlpha;
            }
        }
    }

    void _SetMenu(int index)
    {
        for (int i = 0; i < _Instance._menus.Count; i++)
        {
            _menuActive[i] = i == index;
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

    void _SetGun(string name, Texture2D image, float damage, float cooldown, float chargeSpeed)
    {
        _gunDetectedName.gameObject.SetActive(true);
        _gunDetectedName.text = name;

        _gunName.text = name;
        _chargeSpeed = chargeSpeed;


        // If it's not a charge gun, have the charge meter full
        if (chargeSpeed == 0) _gunDamageMeter.fill = 1f;

        // Continuous guns should not have a cooldown meter or label
        _gunCooldownMeter.gameObject.SetActive(cooldown != 0);
        _gunCooldownLabel.gameObject.SetActive(cooldown != 0);

        // Set bar widths
        _gunDamageMeter.size = damage;
        _gunCooldownMeter.size = cooldown;

        // Gun cooldown meter should always start full
        _gunCooldownMeter.fill = 1f;
        _gunCooldownMeterImage.color = _gunCooldownGradient.Evaluate(_gunCooldownMeter.fill);

        // Set receipt controller
        ReceiptController.SetGun(name, image);
    }

    void _SetCharge(float charge)
    {
        // Asymptotically towards full
        float fill = 1f - (1f / Mathf.Pow(charge + 1f, _chargeSpeed));
        _gunDamageMeter.fill = fill;
    }

    void _SetCooldown(float cooldown)
    {
        // Linearl fill
        _gunCooldownMeter.fill = 1f - cooldown;
        _gunCooldownMeterImage.color = _gunCooldownGradient.Evaluate(_gunCooldownMeter.fill);
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

        // Set receipt controller
        ReceiptController.SetScore(score, _highestCombo);
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

    public static void SetGun(string name, Texture2D image, float damage, float cooldown, float chargeSpeed = 0f)
    {
        _Instance._SetGun(name, image, damage, cooldown, chargeSpeed);
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
