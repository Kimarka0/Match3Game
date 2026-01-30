using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // ============================================================
    // Inspector
    // ============================================================

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip matchClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;

    // ============================================================
    // Runtime
    // ============================================================

    private bool isMuted;
    private bool isPlatformMuted;

    private const string MuteKey = "audio_muted";

    public bool IsMuted => isMuted;

    private void Awake()
    {
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        LoadMute();
        ApplyMute();
    }

    // ============================================================
    // User API
    // ============================================================

    public void ToggleMute()
    {
        isMuted = !isMuted;
        SaveMute();
        ApplyMute();
    }

    // ============================================================
    // Platform API
    // ============================================================

    public void SetPlatformMuted(bool muted)
    {
        isPlatformMuted = muted;
        ApplyMute();
    }

    // ============================================================
    // Play
    // ============================================================

    public void PlayClick() { PlayOneShot(clickClip); }
    public void PlayMatch() { PlayOneShot(matchClip); }
    public void PlayWin() { PlayOneShot(winClip); }
    public void PlayLose() { PlayOneShot(loseClip); }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxSource == null) return;
        if (sfxSource.mute) return;

        sfxSource.PlayOneShot(clip);
    }

    // ============================================================
    // Persistence
    // ============================================================

    private void SaveMute()
    {
        PlayerPrefs.SetInt(MuteKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadMute()
    {
        isMuted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
    }

    private void ApplyMute()
    {
        if (sfxSource == null) return;
        sfxSource.mute = isMuted || isPlatformMuted;
    }
}
