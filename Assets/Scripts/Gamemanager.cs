using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("rules")]
    [SerializeField] int lanternsRequired = 5;

    [Header("ui")]
    [SerializeField] TMP_Text lanternCounter;
    [SerializeField] TMP_Text messageText;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    [Header("world")]
    [SerializeField] ExitGate exitGate;
    [SerializeField] MonoBehaviour[] disableOnGameOver;

    [Header("audio")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip collectClip;
    [SerializeField] AudioClip gateOpenClip;
    [SerializeField] AudioClip winClip;
    [SerializeField] AudioClip[] loseClips;
    [SerializeField] bool layerLoseClips = true;

    int collected;
    bool gameOver;
    Coroutine messageRoutine;

    public bool AllCollected => collected >= lanternsRequired;
    public bool IsGameOver => gameOver;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (messageText) messageText.text = "";
        UpdateCounter();
        ShowMessage("Find the 5 lanterns.\nDon't. look. away.");
    }

    void Update()
    {
        if (gameOver && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            Restart();
    }


    public void CollectLantern()
    {
        if (gameOver) return;
        collected++;
        UpdateCounter();
        Play(collectClip);

        if (AllCollected)
        {
            if (exitGate) exitGate.Unlock();
            Play(gateOpenClip);
            ShowMessage("The gate is open.\nRun.");
        }
        else
        {
            int left = lanternsRequired - collected;
            ShowMessage(left == 1 ? "1 lantern left" : $"{left} lanterns left");
        }
    }

    public void Win()  => EndGame(winPanel, winClip);
    public void Lose()
    {
        if (layerLoseClips || loseClips.Length == 0)
            EndGame(losePanel, loseClips);
        else
            EndGame(losePanel, loseClips[Random.Range(0, loseClips.Length)]);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    void EndGame(GameObject panel, params AudioClip[] clips)
    {
        if (gameOver) return;
        gameOver = true;

        foreach (MonoBehaviour b in disableOnGameOver)
            if (b) b.enabled = false;

        if (panel) panel.SetActive(true);
        foreach (AudioClip clip in clips) Play(clip);

        Time.timeScale = 0f;              
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UpdateCounter()
    {
        if (lanternCounter) lanternCounter.text = $"Lanterns  {collected} / {lanternsRequired}";
    }

    void ShowMessage(string text)
    {
        if (!messageText) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(text));
    }

    IEnumerator MessageRoutine(string text)
    {
        messageText.text = text;
        messageText.alpha = 1f;
        yield return new WaitForSecondsRealtime(2.5f);

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime)
        {
            messageText.alpha = 1f - t;
            yield return null;
        }
        messageText.alpha = 0f;
    }

    void Play(AudioClip clip)
    {
        if (sfxSource && clip) sfxSource.PlayOneShot(clip);
    }
}