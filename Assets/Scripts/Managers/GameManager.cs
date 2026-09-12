using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game-state controller. One per scene.
/// In the MainMenu scene it runs in "menu mode" (only shows the main menu panel).
/// In level scenes it runs the gameplay loop: HUD, pause, level timer, game-over and level-complete flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver, LevelComplete, Dying }
    public GameState State { get; private set; }

    [Header("UI Panels (auto-wired by the builder)")]
    public GameObject mainMenuPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;

    [Header("Level Settings")]
    [Tooltip("How long (seconds) the player must survive to clear the level.")]
    public float levelDuration = 120f;
    [Tooltip("Leave empty to auto-detect the next scene by build index.")]
    public string nextSceneName = "";

    [Header("Mode")]
    [Tooltip("If true, this GameManager only drives the main menu.")]
    public bool isMainMenu = false;

    private float timeRemaining;
    private bool isGameplayScene;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        isMainMenu = isMainMenu || sceneName == "MainMenu";
        isGameplayScene = !isMainMenu;

        Time.timeScale = 1f;
        timeRemaining = levelDuration;

        if (isMainMenu)
        {
            State = GameState.MainMenu;
            ShowOnly(mainMenuPanel);
        }
        else
        {
            State = GameState.Playing;
            ShowOnly(hudPanel);
        }
    }

    void Update()
    {
        if (!isGameplayScene) return;

        if (State == GameState.Playing)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                TriggerLevelComplete();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                PauseGame();
        }
        else if (State == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ResumeGame();
        }
    }

    public float GetTimeRemaining() => timeRemaining;

    /// <summary>Player has died: stop the level timer/pause input but keep time running
    /// so the stalker's carry animation can play. The end screen is shown later.</summary>
    public void OnPlayerDying()
    {
        if (State == GameState.Playing) State = GameState.Dying;
    }

    // ---------- Panel helpers ----------
    private void ShowOnly(GameObject panel)
    {
        SetActiveSafe(mainMenuPanel, panel == mainMenuPanel);
        SetActiveSafe(hudPanel, panel == hudPanel);
        SetActiveSafe(pausePanel, panel == pausePanel);
        SetActiveSafe(gameOverPanel, panel == gameOverPanel);
        SetActiveSafe(levelCompletePanel, panel == levelCompletePanel);
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active) go.SetActive(active);
    }

    // ---------- Button hooks ----------
    public void OnPlayButton()
    {
        AudioManager.PlayButtonClickStatic();
        LoadSceneByIndex(1); // Level1 is build index 1
    }

    public void PlayLevel1() { AudioManager.PlayButtonClickStatic(); LoadSceneByIndex(1); }
    public void PlayLevel2() { AudioManager.PlayButtonClickStatic(); LoadSceneByIndex(2); }
    public void PlayLevel3() { AudioManager.PlayButtonClickStatic(); LoadSceneByIndex(3); }

    public void QuitGame()
    {
        AudioManager.PlayButtonClickStatic();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        // keep HUD visible behind the pause panel
        SetActiveSafe(pausePanel, true);
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;
        AudioManager.PlayButtonClickStatic();
        State = GameState.Playing;
        Time.timeScale = 1f;
        SetActiveSafe(pausePanel, false);
    }

    public void RestartLevel()
    {
        AudioManager.PlayButtonClickStatic();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        AudioManager.PlayButtonClickStatic();
        Time.timeScale = 1f;
        LoadSceneByIndex(0);
    }

    public void NextLevel()
    {
        AudioManager.PlayButtonClickStatic();
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            LoadSceneByIndex(0); // finished last level -> back to menu
    }

    public void TriggerGameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;
        Time.timeScale = 0f;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.PushFinalScore();
        ShowOnly(gameOverPanel);
    }

    public void TriggerLevelComplete()
    {
        if (State == GameState.LevelComplete) return;
        State = GameState.LevelComplete;
        Time.timeScale = 0f;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.PushFinalScore();
        ShowOnly(levelCompletePanel);
    }

    private void LoadSceneByIndex(int index)
    {
        Time.timeScale = 1f;
        if (index < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(index);
    }
}
