using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    // ============================================================
    // Inspector
    // ============================================================

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button soundButton;

    [Header("Services")]
    [SerializeField] private AppRoot appRoot;

    private void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (soundButton != null) soundButton.onClick.AddListener(OnSoundClicked);
    }

    private void Start()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;

        if (appRoot != null && appRoot.YandexSdk != null)
        {
            appRoot.YandexSdk.Init();
            appRoot.YandexSdk.GameReady();
        }
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
        if (soundButton != null) soundButton.onClick.RemoveListener(OnSoundClicked);
    }

    private void OnPlayClicked()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;
        if (appRoot != null && appRoot.Audio != null) appRoot.Audio.PlayClick();

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnSoundClicked()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;
        if (appRoot == null || appRoot.Audio == null) return;

        appRoot.Audio.ToggleMute();
        appRoot.Audio.PlayClick();
    }
}
