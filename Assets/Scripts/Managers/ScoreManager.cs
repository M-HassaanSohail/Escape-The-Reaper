using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks current score (coins + distance) and persists the high score with PlayerPrefs.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private const string HighScoreKey = "HighScore";

    [Header("HUD (auto-wired by the builder)")]
    public Text scoreText;
    public Text highScoreText;
    public Text finalScoreText;

    [Header("Config")]
    [Tooltip("Score gained per metre travelled forward.")]
    public float distancePointsPerUnit = 1f;

    public int Score { get; private set; }
    public int HighScore { get; private set; }

    private float distanceAccumulator;

    void Awake()
    {
        Instance = this;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    void Start()
    {
        RefreshUI();
    }

    public void AddScore(int amount)
    {
        Score += amount;
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
        RefreshUI();
    }

    /// <summary>Called by the player each frame with how far it moved this frame.</summary>
    public void AddDistance(float deltaUnits)
    {
        distanceAccumulator += deltaUnits * distancePointsPerUnit;
        if (distanceAccumulator >= 1f)
        {
            int whole = Mathf.FloorToInt(distanceAccumulator);
            distanceAccumulator -= whole;
            AddScore(whole);
        }
    }

    public void PushFinalScore()
    {
        if (finalScoreText != null)
            finalScoreText.text = "Score: " + Score + "\nBest: " + HighScore;
    }

    public static int GetSavedHighScore()
    {
        return PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void RefreshUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + Score;
        if (highScoreText != null) highScoreText.text = "Best: " + HighScore;
    }
}
