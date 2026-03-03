using UnityEngine;
using UnityEngine.UI;
using System;

public class ScoreManager : MonoBehaviour
{
    public int score;
    public float secondsAlive;

    public Text scoreText; // optional (legacy UI). For TMP, swap type.

    public event Action<int> OnScoreChanged;

    public void Add(int amount)
    {
        score += amount;
        UpdateUI();
        OnScoreChanged?.Invoke(score);
    }

    void Update()
    {
        secondsAlive += Time.deltaTime;
        // 1 point per second alive
        score = Mathf.Max(score, Mathf.FloorToInt(secondsAlive));
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = score.ToString();
    }
}
