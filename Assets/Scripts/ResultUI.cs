using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI resultTitleText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Buttons")]
    [SerializeField] private Button restartButton;

    [Header("Services")]
    [SerializeField] private AppRoot appRoot;

    private void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        Hide();
    }

    private void OnDestroy()
    {
        if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartClicked);
    }

    public void ShowWin()
    {
        ShowInternal("Победа");
    }

    public void ShowLose()
    {
        ShowInternal("Поражение");
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    private void ShowInternal(string title)
    {
        if (resultTitleText != null) resultTitleText.text = title;
        if (root != null) root.SetActive(true);
    }

    private void OnRestartClicked()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;

        if (appRoot != null && appRoot.Audio != null)
        {
            appRoot.Audio.PlayMusic();
            appRoot.Audio.PlayClick();
        }

        SceneManager.LoadScene(gameSceneName);
    }
}
