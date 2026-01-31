using UnityEngine;

public class AppRoot : MonoBehaviour
{

    [Header("Services")]
    [SerializeField] private YandexSdkBridge yandexSdk;
    [SerializeField] private AudioManager audioManager;
    public static AppRoot Instance { get; private set; }

    public YandexSdkBridge YandexSdk => yandexSdk;
    public AudioManager Audio => audioManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (yandexSdk == null) yandexSdk = GetComponent<YandexSdkBridge>();
        if (audioManager == null) audioManager = GetComponent<AudioManager>();
    }
}

