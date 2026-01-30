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

    private void Awake()
    {
        if (appRoot == null) appRoot = AppRoot.Instance;

        if (board != null)
        {
            board.OnMoveMade += OnMoveMade;
            board.OnTilesCleared += OnTilesCleared;
        }

        if (objectives != null)
        {
            objectives.OnChainCompleted += OnObjectivesCompleted;
        }

        if (resultUI != null) resultUI.Hide();

        isActive = true;

        if (appRoot != null && appRoot.YandexSdk != null)
        {
            appRoot.YandexSdk.GameReady();
            appRoot.YandexSdk.GameplayStart();
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
        }
    }

    private void OnMoveMade()
    {
    }

    private void OnTilesCleared(Dictionary<TileType, int> clearedByType)
    {
        if (!isActive) return;

        if (objectives != null) objectives.AddCleared(clearedByType);
        if (appRoot != null && appRoot.Audio != null) appRoot.Audio.PlayMatch();
    }


    private void OnObjectivesCompleted()
    {
        if (!isActive) return;
        isActive = false;

        if (appRoot != null && appRoot.YandexSdk != null)
        {
            appRoot.YandexSdk.GameplayStop();
            appRoot.YandexSdk.ShowInterstitial();
        }

        if (appRoot != null && appRoot.Audio != null) appRoot.Audio.PlayWin();

        if (resultUI != null) resultUI.ShowWin();
    }
}
