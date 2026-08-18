using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudUI : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private AppRoot appRoot;

    [Header("Logic")]
    [SerializeField] private ObjectiveRunner objectives;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI goalTitleText;
    [SerializeField] private TextMeshProUGUI goalProgressText; 
    [SerializeField] private TextMeshProUGUI goalTimerText;     
    [SerializeField] private TextMeshProUGUI failsText;        
    [Header("Buttons")]
    [SerializeField] private Button soundButton;

    private void Awake()
    {
        if (soundButton != null) soundButton.onClick.AddListener(OnSoundClicked);
        if (objectives != null) objectives.OnStateChanged += Refresh;
    }

    private void Start()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;
        Refresh();
    }

    private void OnDestroy()
    {
        if (soundButton != null) soundButton.onClick.RemoveListener(OnSoundClicked);
        if (objectives != null) objectives.OnStateChanged -= Refresh;
    }

    private void Refresh()
    {
        if (objectives == null) return;

        int goalIndex = Mathf.Clamp(objectives.GoalsCompleted + 1, 1, objectives.GoalsToComplete);

        if (goalTitleText != null)
            goalTitleText.text = $"Цель {goalIndex}/{objectives.GoalsToComplete} {ShortType(objectives.CurrentType)}";

        if (goalProgressText != null)
            goalProgressText.text = $"Выполнено: {objectives.CurrentProgress}/{objectives.CurrentTarget}";

        if (goalTimerText != null)
        {
            int sec = Mathf.Max(0, Mathf.CeilToInt(objectives.TimeLeft));
            goalTimerText.text = $"{sec}";
        }

        if (failsText != null)
            failsText.text = $"Неудачи: {objectives.Fails}/{objectives.FailsAllowed}";
            Debug.Log($"[HUD] objectives={objectives?.name} goalsToComplete={objectives?.GoalsToComplete} id={objectives?.GetInstanceID()}");

    }

    private static string ShortType(TileType t)
{
    return t switch
    {
        TileType.Strawberry => "Клубника",
        TileType.Apple      => "Яблоко",
        TileType.Banana     => "Банан",
        TileType.Blueberry  => "Черника",
        TileType.Grape      => "Виноград",
        TileType.Orange     => "Апельсин",
        TileType.Pear       => "Груша",
        _ => t.ToString()
    };
}

    private void OnSoundClicked()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;
        if (appRoot == null || appRoot.Audio == null) return;

        appRoot.Audio.ToggleMute();
        appRoot.Audio.PlayClick();
    }
}
