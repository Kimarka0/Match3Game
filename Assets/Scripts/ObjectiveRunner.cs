using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveRunner : MonoBehaviour
{
    [Header("Rules")]
    [SerializeField] private int goalsToComplete = 5;
    [SerializeField] private float timePerGoal = 60f;

    [Header("Lose Condition")]
    [SerializeField] private int failsAllowed = 3;

    [Header("Collect Goal Settings")]
    [SerializeField] private TileType[] allowedTypes =
    {
        TileType.Strawberry,
        TileType.Apple,
        TileType.Banana,
        TileType.Blueberry,
        TileType.Grape,
        TileType.Orange,
        TileType.Pear
    };

    [SerializeField] private int minCollectCount;
    [SerializeField] private int maxCollectCount;

    public int GoalsCompleted { get; private set; }
    public int GoalsToComplete => goalsToComplete;

    public TileType CurrentType { get; private set; }
    public int CurrentProgress { get; private set; }
    public int CurrentTarget { get; private set; }

    public float TimeLeft { get; private set; }
    public float TimeLimit => timePerGoal;

    public bool IsFinished { get; private set; }

    public int Fails { get; private set; }
    public int FailsAllowed => failsAllowed;

    public event Action OnStateChanged;
    public event Action OnChainCompleted;
    public event Action OnFailed;

    private System.Random rnd;

    private void Awake()
    {
        rnd = new System.Random();
        StartNewGoal(resetChain: true);
    }

    private void Update()
    {
        if (IsFinished) return;

        TimeLeft -= Time.deltaTime;

        if (TimeLeft <= 0f)
        {
            Fails++;

            if (Fails > failsAllowed)
            {
                IsFinished = true;
                OnStateChanged?.Invoke();
                OnFailed?.Invoke();
                return;
            }

            StartNewGoal(resetChain: false);
            return;
        }

        OnStateChanged?.Invoke();
    }

    public void AddCleared(Dictionary<TileType, int> clearedByType)
    {
        if (IsFinished) return;
        if (clearedByType == null) return;

        if (clearedByType.TryGetValue(CurrentType, out int add))
        {
            CurrentProgress += add;

            if (CurrentProgress >= CurrentTarget)
            {
                GoalsCompleted++;

                if (GoalsCompleted >= goalsToComplete)
                {
                    IsFinished = true;
                    OnStateChanged?.Invoke();
                    OnChainCompleted?.Invoke();
                    return;
                }

                StartNewGoal(resetChain: false);
                return;
            }

            OnStateChanged?.Invoke();
        }
    }

    public void RestartChain()
    {
        StartNewGoal(resetChain: true);
    }

    private void StartNewGoal(bool resetChain)
    {
        if (resetChain)
        {
            GoalsCompleted = 0;
            Fails = 0;
            IsFinished = false;
        }

        TimeLeft = timePerGoal;

        CurrentType = PickRandomType();
        CurrentTarget = PickRandomTarget();
        CurrentProgress = 0;

        OnStateChanged?.Invoke();
    }

    private TileType PickRandomType()
    {
        if (allowedTypes == null || allowedTypes.Length == 0)
            return TileType.Strawberry;

        int index = rnd.Next(0, allowedTypes.Length);
        return allowedTypes[index];
    }

    private int PickRandomTarget()
    {
        if (maxCollectCount < minCollectCount)
            return minCollectCount;

        return rnd.Next(minCollectCount, maxCollectCount + 1);
    }
}
