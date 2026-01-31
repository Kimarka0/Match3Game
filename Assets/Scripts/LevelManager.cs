using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private AppRoot appRoot;

    [Header("Core")]
    [SerializeField] private Board board;
    [SerializeField] private ObjectiveRunner objectives;

    [Header("UI")]
    [SerializeField] private ResultUI resultUI;

    private bool isActive;

    private AppRoot Root
    {
        get
        {
            if (appRoot == null) appRoot = AppRoot.Instance;
            return appRoot;
        }
    }

    private void Awake()
    {
        appRoot = AppRoot.Instance;

        if (board != null)
        {
            board.OnMoveMade += OnMoveMade;
            board.OnTilesCleared += OnTilesCleared;
        }

        if (objectives != null)
        {
            objectives.OnChainCompleted += OnObjectivesCompleted;
            objectives.OnFailed += OnObjectivesFailed;
        }

        if (resultUI != null) resultUI.Hide();

        isActive = true;

        if (Root != null && Root.YandexSdk != null)
        {
            Root.YandexSdk.GameReady();
            Root.YandexSdk.GameplayStart();
        }
    }

    private void OnDestroy()
    {
        if (board != null)
        {
            board.OnMoveMade -= OnMoveMade;
            board.OnTilesCleared -= OnTilesCleared;
        }

        if (objectives != null)
        {
            objectives.OnChainCompleted -= OnObjectivesCompleted;
            objectives.OnFailed -= OnObjectivesFailed;
        }
    }

    private void OnMoveMade() { }

    private void OnTilesCleared(Dictionary<TileType, int> clearedByType)
    {
        if (!isActive) return;

        if (objectives != null) objectives.AddCleared(clearedByType);

        if (Root != null && Root.Audio != null)
            Root.Audio.PlayMatch();
    }

    private void OnObjectivesCompleted()
    {
        if (!isActive) return;
        isActive = false;

        if (Root != null && Root.YandexSdk != null)
        {
            Root.YandexSdk.GameplayStop();
            Root.YandexSdk.ShowInterstitial();
        }

        if (resultUI != null) resultUI.ShowWin();
    }

    private void OnObjectivesFailed()
    {
        if (!isActive) return;
        isActive = false;

        if (Root != null && Root.YandexSdk != null)
            Root.YandexSdk.GameplayStop();

        if (resultUI != null) resultUI.ShowLose();
    }
}
