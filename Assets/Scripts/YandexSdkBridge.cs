using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class YandexSdkBridge : MonoBehaviour
{

    private bool isInitialized;

    private Action rewardedOnReward;
    private Action rewardedOnClose;
    private Action rewardedOnError;

    private bool rewardedWasGiven;

    [Header("References")]
    [SerializeField] private AppRoot appRoot;

    private void Awake()
    {
        if (appRoot == null) appRoot = GetComponent<AppRoot>();
    }


    public void Init()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        YG_Init(gameObject.name);
#else
        isInitialized = true;
#endif
    }

    public void GameReady()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized) return;
        YG_GameReady();
#endif
    }

    public void GameplayStart()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized) return;
        YG_GameplayStart();
#endif
    }

    public void GameplayStop()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized) return;
        YG_GameplayStop();
#endif
    }

    public void ShowInterstitial()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized) return;
        YG_ShowInterstitial();
#endif
    }

    
    public void ShowRewarded(Action onReward, Action onClose, Action onError)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized)
        {
            onError?.Invoke();
            return;
        }

        rewardedOnReward = onReward;
        rewardedOnClose = onClose;
        rewardedOnError = onError;

        rewardedWasGiven = false;

        YG_ShowRewarded();
#else
        
        onReward?.Invoke();
        onClose?.Invoke();
#endif
    }

    public void OnSdkInitialized(string value)
    {
        isInitialized = true;
    }


    public void OnRewardedOpen(string value)
    {
    }

    public void OnRewarded(string value)
    {
        rewardedWasGiven = true;
        rewardedOnReward?.Invoke();
    }

    public void OnRewardedClose(string value)
    {
        rewardedOnClose?.Invoke();
        ClearRewardedCallbacks();
    }

    public void OnRewardedError(string value)
    {
        rewardedOnError?.Invoke();
        ClearRewardedCallbacks();
    }

    private void ClearRewardedCallbacks()
    {
        rewardedOnReward = null;
        rewardedOnClose = null;
        rewardedOnError = null;
        rewardedWasGiven = false;
    }


    public void OnInterstitialOpen(string value) { }
    public void OnInterstitialClose(string value) { }
    public void OnInterstitialError(string value) { }


    public void OnGameApiPause(string value)
    {
        Time.timeScale = 0f;
        if (appRoot != null && appRoot.Audio != null) appRoot.Audio.SetPlatformMuted(true);
    }

    public void OnGameApiResume(string value)
    {
        Time.timeScale = 1f;
        if (appRoot != null && appRoot.Audio != null) appRoot.Audio.SetPlatformMuted(false);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void YG_Init(string unityObjectName);
    [DllImport("__Internal")] private static extern void YG_GameReady();
    [DllImport("__Internal")] private static extern void YG_GameplayStart();
    [DllImport("__Internal")] private static extern void YG_GameplayStop();
    [DllImport("__Internal")] private static extern void YG_ShowInterstitial();
    [DllImport("__Internal")] private static extern void YG_ShowRewarded();
#endif
}
