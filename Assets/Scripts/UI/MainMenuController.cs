using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Populates the main-menu high-score label from saved PlayerPrefs.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public Text highScoreText;

    void Start()
    {
        if (highScoreText != null)
            highScoreText.text = "Best Score: " + ScoreManager.GetSavedHighScore();
    }
}
