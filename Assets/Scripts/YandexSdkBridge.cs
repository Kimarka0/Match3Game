using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class YandexSdkBridge : MonoBehaviour
{
    [Header("Optional refs")]
    [SerializeField] private AppRoot appRoot;

    
    private bool initRequested;
    private bool isInitialized;
    private bool initSucceeded;

    
    private bool pendingGameReady;
    private bool pendingGameplayStart;
    private bool pendingGameplayStop;
    private bool pendingShowInterstitial;

    private bool pendingShowRewarded;
    private Action pendingRewardOnReward;
    private Action pendingRewardOnClose;
    private Action pendingRewardOnError;

    private bool interstitialInProgress;
    private bool rewardedInProgress;
    private bool rewardedGranted; 

    private Action currentRewardOnReward;
    private Action currentRewardOnClose;
    private Action currentRewardOnError;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void YG_Init(string unityObjectName);
    [DllImport("__Internal")] private static extern void YG_GameReady();
    [DllImport("__Internal")] private static extern void YG_GameplayStart();
    [DllImport("__Internal")] private static extern void YG_GameplayStop();
    [DllImport("__Internal")] private static extern void YG_ShowInterstitial();
    [DllImport("__Internal")] private static extern void YG_ShowRewarded();
#endif

    private void Awake()
    {
        if (appRoot == null)
            appRoot = GetComponent<AppRoot>();
    }


    public void Init()
    {
        if (initRequested) return;
        initRequested = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        YG_Init(gameObject.name);
#else
        isInitialized = true;
        initSucceeded = true;
        FlushPendingCalls();
#endif
    }

    public void GameReady()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized) { pendingGameReady = true; return; }
        if (!initSucceeded) return;
        YG_GameReady();
#endif
    }

    public void GameplayStart()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized) { pendingGameplayStart = true; return; }
        if (!initSucceeded) return;
        YG_GameplayStart();
#endif
    }

    public void GameplayStop()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized) { pendingGameplayStop = true; return; }
        if (!initSucceeded) return;
        YG_GameplayStop();
#endif
    }

    public void ShowInterstitial()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized) { pendingShowInterstitial = true; return; }
        if (!initSucceeded) return;
        if (interstitialInProgress) return;

        interstitialInProgress = true;
        YG_ShowInterstitial();
#else
        
        OnInterstitialOpen("1");
        OnInterstitialClose("1");
#endif
    }

    public void ShowRewarded(Action onReward, Action onClose, Action onError)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized)
        {
            pendingShowRewarded = true;
            pendingRewardOnReward = onReward;
            pendingRewardOnClose  = onClose;
            pendingRewardOnError  = onError;
            return;
        }

        if (!initSucceeded)
        {
            onError?.Invoke();
            onClose?.Invoke();
            return;
        }

        if (rewardedInProgress) return;

        rewardedInProgress = true;
        rewardedGranted = false;

        currentRewardOnReward = onReward;
        currentRewardOnClose  = onClose;
        currentRewardOnError  = onError;

        YG_ShowRewarded();
#else
        rewardedInProgress = true;
        rewardedGranted = false;
        currentRewardOnReward = onReward;
        currentRewardOnClose  = onClose;
        currentRewardOnError  = onError;

        OnRewardedOpen("1");
        OnRewarded("1");
        OnRewardedClose("1");
#endif
    }

    public void OnSdkInitialized(string value)
    {
        isInitialized = true;
        initSucceeded = value == "1";

        if (!initSucceeded)
        {
            Debug.LogWarning("[YandexSdkBridge] SDK init failed (OnSdkInitialized=0). SDK calls will be ignored.");
            if (pendingShowRewarded)
            {
                var err = pendingRewardOnError;
                var cls = pendingRewardOnClose;
                ClearPendingRewarded();
                err?.Invoke();
                cls?.Invoke();
            }
            return;
        }

        FlushPendingCalls();
    }

    public void OnGameApiPause(string _)
    {
        Time.timeScale = 0f;
        if (appRoot != null && appRoot.Audio != null)
            appRoot.Audio.SetPlatformMuted(true);
    }

    public void OnGameApiResume(string _)
    {
        Time.timeScale = 1f;
        if (appRoot != null && appRoot.Audio != null)
            appRoot.Audio.SetPlatformMuted(false);
    }


    public void OnInterstitialOpen(string _)
    {
        if (appRoot != null && appRoot.Audio != null)
            appRoot.Audio.SetPlatformMuted(true);
    }

    public void OnInterstitialClose(string _)
    {
        interstitialInProgress = false;

        if (appRoot != null && appRoot.Audio != null)
            appRoot.Audio.SetPlatformMuted(false);
    }

    public void OnInterstitialError(string _)
    {
        interstitialInProgress = false;

        if (appRoot != null && appRoot.Audio != null)
            appRoot.Audio.SetPlatformMuted(false);

        Debug.LogWarning("[YandexSdkBridge] Interstitial error.");
    }


    public void OnRewardedOpen(string _)
    {
        if (appRoot != null && appRoot.Audio != null)
            appRoot.Audio.SetPlatformMuted(true);
    }

    public void OnRewarded(string _)
    {
        rewardedGranted = true;
        currentRewardOnReward?.Invoke();
    }

    public void OnRewardedClose(string _)
    {
        rewardedInProgress = false;

        if (appRoot != null && appRoot.Audio != null)
            appRoot.Audio.SetPlatformMuted(false);

        currentRewardOnClose?.Invoke();
        ClearCurrentRewarded();
    }

    public void OnRewardedError(string _)
    {
        rewardedInProgress = false;

        if (appRoot != null && appRoot.Audio != null)
            appRoot.Audio.SetPlatformMuted(false);

        currentRewardOnError?.Invoke();
        currentRewardOnClose?.Invoke();
        ClearCurrentRewarded();

        Debug.LogWarning("[YandexSdkBridge] Rewarded error.");
    }


    private void FlushPendingCalls()
    {
        if (pendingGameReady)
        {
            pendingGameReady = false;
            GameReady();
        }

        if (pendingGameplayStop)
        {
            pendingGameplayStop = false;
            GameplayStop();
        }

        if (pendingGameplayStart)
        {
            pendingGameplayStart = false;
            GameplayStart();
        }

        if (pendingShowInterstitial)
        {
            pendingShowInterstitial = false;
            ShowInterstitial();
        }

        if (pendingShowRewarded)
        {
            var r = pendingRewardOnReward;
            var c = pendingRewardOnClose;
            var e = pendingRewardOnError;
            ClearPendingRewarded();
            ShowRewarded(r, c, e);
        }
    }

    private void ClearCurrentRewarded()
    {
        currentRewardOnReward = null;
        currentRewardOnClose  = null;
        currentRewardOnError  = null;
        rewardedGranted = false;
    }

    private void ClearPendingRewarded()
    {
        pendingShowRewarded = false;
        pendingRewardOnReward = null;
        pendingRewardOnClose  = null;
        pendingRewardOnError  = null;
    }
}
