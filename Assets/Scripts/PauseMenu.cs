using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] MonoBehaviour[] disableWhilePaused;
    [SerializeField] string mainMenuSceneName = "MainMenu";

    public static bool IsPaused { get; private set; }

    void Start()
    {
        SetPaused(false);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)
        {
            if (GameManager.Instance && GameManager.Instance.IsGameOver) return;
            SetPaused(!IsPaused);
        }
    }

    // ---------- button methods ----------

    public void Resume() => SetPaused(false);

    public void Restart()
    {
        ClearPauseState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        ClearPauseState();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- internals ----------

    void SetPaused(bool paused)
    {
        IsPaused = paused;
        if (pausePanel) pausePanel.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;

        foreach (MonoBehaviour b in disableWhilePaused)
            if (b) b.enabled = !paused;

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    void ClearPauseState()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
