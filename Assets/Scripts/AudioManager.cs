using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool playMusicOnAwake = true;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip matchClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;

    private bool isMuted;
    private bool isPlatformMuted;

    private const string MuteKey = "audio_muted";

    public bool IsMuted => isMuted;

    private void Awake()
    {
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        LoadMute();
        ApplyMute();

        if (musicSource != null)
        {
            musicSource.loop = true;

            if (backgroundMusic != null && musicSource.clip != backgroundMusic)
                musicSource.clip = backgroundMusic;

            if (playMusicOnAwake)
                TryPlayMusic();
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        SaveMute();
        ApplyMute();
    }

    public void SetPlatformMuted(bool muted)
    {
        isPlatformMuted = muted;
        ApplyMute();
    }

    public void PlayClick() { EnsureMusicStarted(); PlayOneShot(clickClip); }
    public void PlayMatch() { EnsureMusicStarted(); PlayOneShot(matchClip); }
    public void PlayWin() { EnsureMusicStarted(); PlayOneShot(winClip); }
    public void PlayLose() { EnsureMusicStarted(); PlayOneShot(loseClip); }

    public void PlayMusic()
    {
        TryPlayMusic(forceRestart: false);
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    public void SetMusicClip(AudioClip clip, bool restart = true)
    {
        backgroundMusic = clip;
        if (musicSource == null) return;

        musicSource.clip = backgroundMusic;

        if (restart)
            TryPlayMusic(forceRestart: true);
    }


    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxSource == null) return;
        if (sfxSource.mute) return;

        sfxSource.PlayOneShot(clip);
    }

    private void EnsureMusicStarted()
    {
        if (!playMusicOnAwake) return;
        TryPlayMusic();
    }

    private void TryPlayMusic(bool forceRestart = false)
    {
        if (musicSource == null) return;
        if (musicSource.mute) return;

        if (musicSource.clip == null && backgroundMusic != null)
            musicSource.clip = backgroundMusic;

        if (musicSource.clip == null) return;

        if (forceRestart)
        {
            musicSource.Stop();
            musicSource.Play();
            return;
        }

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

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
        bool muted = isMuted || isPlatformMuted;

        if (sfxSource != null) sfxSource.mute = muted;
        if (musicSource != null) musicSource.mute = muted;
    }
}
