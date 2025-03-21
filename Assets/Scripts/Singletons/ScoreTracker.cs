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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
        MenuController.SetScore(_score, _currentComboCount);
    }

    public static void Kill(int points)
    {
        _Instance._TrackPoints(points);
    }
}
