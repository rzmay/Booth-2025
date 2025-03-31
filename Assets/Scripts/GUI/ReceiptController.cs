using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PrintReceipt))]
public class ReceiptController : DelayableMonoBehaviour
{
    private static ReceiptController _Instance;
    [SerializeField] private TMP_Text _gunName;
    [SerializeField] private TMP_Text _scoreMeter;
    [SerializeField] private TMP_Text _comboMeter;
    [SerializeField] private TMP_Text _aliensDefeated;
    [SerializeField] private TMP_Text _ufosDestroyed;
    [SerializeField] private TMP_Text _accuracy;
    [SerializeField] private CanvasGroup _savedGalaxy;
    [SerializeField] private RawImage _gunImage;
    [SerializeField] private RawImage _letterImage;
    [SerializeField] private List<Texture2D> _letterTextures;

    private PrintReceipt _printReceipt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _Instance = this;
    }

    void Start()
    {
        _printReceipt = GetComponent<PrintReceipt>();
    }

    void _SetGun(string name, Texture2D image)
    {
        _gunName.text = name;
        _gunImage.texture = image;
    }

    void _SetStats(int aliensDefeated, int ufosDestroyed, int accuracy, bool savedGalaxy)
    {
        _aliensDefeated.text = aliensDefeated.ToString();
        _ufosDestroyed.text = ufosDestroyed.ToString();
        _accuracy.text = Mathf.RoundToInt(accuracy).ToString();
        _savedGalaxy.alpha = savedGalaxy ? 1 : 0;
    }

    void _SetScore(int score, int combo)
    {
        // Set score meter
        _scoreMeter.text = score.ToString();
        _comboMeter.text = combo.ToString();

        // Calculate rank
        int rank = 0;
        if (score >= 5000) rank++;
        if (score >= 15000) rank++;
        if (score >= 30000) rank++;

        // Set rank letter
        _letterImage.texture = _letterTextures[rank];
    }

    public static void SetGun(string name, Texture2D image)
    {
        _Instance._SetGun(name, image);
    }

    public static void SetStats(int aliensDefeated, int ufosDestroyed, int accuracy, bool savedGalaxy)
    {
        _Instance._SetStats(aliensDefeated, ufosDestroyed, accuracy, savedGalaxy);
    }

    public static void SetScore(int score, int combo)
    {
        _Instance._SetScore(score, combo);
    }

    public static void Print()
    {
        _Instance._printReceipt.Print();
    }
}
