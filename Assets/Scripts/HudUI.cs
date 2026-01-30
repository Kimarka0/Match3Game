using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HudUI : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Services")]
    [SerializeField] private AppRoot appRoot;

    [Header("Logic")]
    [SerializeField] private ObjectiveRunner objectives;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI goalTitleText;
    [SerializeField] private TextMeshProUGUI goalProgressText;
    [SerializeField] private TextMeshProUGUI goalTimerText;

    [Header("Buttons")]
    [SerializeField] private Button menuButton;
    [SerializeField] private Button soundButton;

    private void Awake()
    {
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);
        if (soundButton != null) soundButton.onClick.AddListener(OnSoundClicked);

        if (objectives != null)
            objectives.OnStateChanged += Refresh;
    }

    private void Start()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;
        Refresh();
    }

    private void OnDestroy()
    {
        if (menuButton != null) menuButton.onClick.RemoveListener(OnMenuClicked);
        if (soundButton != null) soundButton.onClick.RemoveListener(OnSoundClicked);

        if (objectives != null)
            objectives.OnStateChanged -= Refresh;
    }

    private void Refresh()
    {
        if (objectives == null) return;

        int goalIndex = Mathf.Clamp(objectives.GoalsCompleted + 1, 1, objectives.GoalsToComplete);

        if (goalTitleText != null)
            goalTitleText.text = $"Goal {goalIndex}/{objectives.GoalsToComplete}: Collect {objectives.CurrentType}";

        if (goalProgressText != null)
            goalProgressText.text = $"Progress: {objectives.CurrentProgress}/{objectives.CurrentTarget}";

        if (goalTimerText != null)
        {
            int sec = Mathf.Max(0, Mathf.CeilToInt(objectives.TimeLeft));
            goalTimerText.text = $"Time: {sec}s";
        }
    }

    private void OnMenuClicked()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;

        if (appRoot != null && appRoot.Audio != null) appRoot.Audio.PlayClick();
        if (appRoot != null && appRoot.YandexSdk != null) appRoot.YandexSdk.GameplayStop();

        SceneManager.LoadScene(menuSceneName);
    }

    private void OnSoundClicked()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;
        if (appRoot == null || appRoot.Audio == null) return;

        appRoot.Audio.ToggleMute();
        appRoot.Audio.PlayClick();
    }
}
