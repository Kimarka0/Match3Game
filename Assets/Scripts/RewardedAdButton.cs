using UnityEngine;
using UnityEngine.UI;

public class RewardedAdButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;

    [Header("Reward")]
    [SerializeField] private Board board;
    [SerializeField] private RewardType rewardType = RewardType.Shuffle;

    [Header("Limit")]
    [SerializeField] private bool limitOneTime = true;
    [SerializeField] private string limitKey = "rewarded_used";

    private bool isLocked;

    private void Awake()
    {
        if (button != null) button.onClick.AddListener(OnClicked);
    }

    private void Start()
    {
        UpdateInteractable();
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnClicked);
    }


    public void ResetLimit()
    {
        PlayerPrefs.SetInt(limitKey, 0);
        PlayerPrefs.Save();
        UpdateInteractable();
    }

    public void SetLimitKey(string newKey)
    {
        limitKey = newKey;
        UpdateInteractable();
    }


    private void UpdateInteractable()
    {
        if (button == null) return;

        if (isLocked)
        {
            button.interactable = false;
            return;
        }

        if (!limitOneTime)
        {
            button.interactable = true;
            return;
        }

        bool used = PlayerPrefs.GetInt(limitKey, 0) == 1;
        button.interactable = !used;
    }

    private void OnClicked()
    {
        if (isLocked) return;
        if (board == null) return;

        if (limitOneTime)
        {
            bool used = PlayerPrefs.GetInt(limitKey, 0) == 1;
            if (used) return;
        }

        AppRoot appRoot = AppRoot.Instance;
        if (appRoot != null && appRoot.Audio != null) appRoot.Audio.PlayClick();

        isLocked = true;
        UpdateInteractable();

        if (appRoot == null || appRoot.YandexSdk == null)
        {
            OnAdError();
            return;
        }

        appRoot.YandexSdk.ShowRewarded(
            onReward: () =>
            {
                GiveReward();
            },
            onClose: () =>
            {
                OnAdClosed();
            },
            onError: () =>
            {
                OnAdError();
            }
        );
    }

    private void GiveReward()
    {
        if (limitOneTime)
        {
            PlayerPrefs.SetInt(limitKey, 1);
            PlayerPrefs.Save();
        }

        if (rewardType == RewardType.Shuffle) board.ShuffleBoard();
        if (rewardType == RewardType.Rebuild) board.RebuildBoard();
    }

    private void OnAdClosed()
    {
        isLocked = false;
        UpdateInteractable();
    }

    private void OnAdError()
    {
        isLocked = false;
        UpdateInteractable();
    }

    private enum RewardType
    {
        Shuffle = 0,
        Rebuild = 1
    }
}
