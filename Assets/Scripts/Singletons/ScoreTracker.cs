using System;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    private static ScoreTracker _Instance;

    public float comboTime = 5f;
    public float comboMultiplier = 2f;

    private float _currentComboTime = 0f;
    private int _currentComboCount = 0;
    private int _score;

    // Accuracy tracking
    private int _shotsFired;
    private int _shotsHit;

    // UFO and Alien tracking
    private int _aliensDefeated;
    private int _ufosDestroyed;

    void Awake()
    {
        _Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        // Decrement time from current combo time
        _currentComboTime = Mathf.Max(0, _currentComboTime - Time.deltaTime);

        // Reset combo count if combo is over
        if (_currentComboTime <= 0f) _currentComboCount = 0;

        // Set menu
        MenuController.SetScore(_score, _currentComboCount, _currentComboTime);
    }

    void _TrackPoints(int points)
    {
        // Add score -- the more combo time remaining the better
        _score += Mathf.RoundToInt(points * (_currentComboTime * comboMultiplier));

        // Add combo time
        _currentComboTime += comboTime;

        // Increment combo count
        _currentComboCount += 1;

        // Set Menu
        MenuController.SetScore(_score, _currentComboCount, _currentComboTime);
    }

    public static void TrackAliens()
    {
        _Instance._aliensDefeated += 1;
    }

    public static void TrackUFOs()
    {
        _Instance._ufosDestroyed += 1;
    }

    public static void TrackShotsFired()
    {
        _Instance._shotsFired += 1;
    }

    public static void TrackShotsHit()
    {
        _Instance._shotsHit += 1;
    }

    public static void Kill(int points)
    {
        _Instance._TrackPoints(points);
    }

    public static void Print(bool won)
    {
        ReceiptController.SetStats(
            _Instance._aliensDefeated,
            _Instance._ufosDestroyed,
            (_Instance._shotsHit * 100) / _Instance._shotsFired,
            won
        );

        ReceiptController.Print();
    }
}
