using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip _lobbyMusic;
    [SerializeField] private AudioClip _timerSFX;
    [SerializeField] private AudioClip _correctSFX;
    [SerializeField] private AudioClip _incorrectSFX;

    [Header("Volumes")]
    [Range(0f, 1f)] [SerializeField] private float _musicVolume    = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float _sfxVolume      = 1f;

    // Fuente para música/loops (lobby, timer)
    private AudioSource _musicSource;
    // Fuente para efectos puntuales (correcto / incorrecto)
    private AudioSource _sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource            = gameObject.AddComponent<AudioSource>();
        _musicSource.loop       = true;
        _musicSource.playOnAwake = false;
        _musicSource.volume     = _musicVolume;

        _sfxSource              = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop         = false;
        _sfxSource.playOnAwake  = false;
        _sfxSource.volume       = _sfxVolume;
    }

    // ── Lobby ────────────────────────────────────────────────────────────────
    public void PlayLobbyMusic()
    {
        if (_lobbyMusic == null) return;
        if (_musicSource.clip == _lobbyMusic && _musicSource.isPlaying) return;
        _musicSource.clip = _lobbyMusic;
        _musicSource.loop = true;
        _musicSource.volume = _musicVolume;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
    }

    // ── Timer ────────────────────────────────────────────────────────────────
    public void PlayTimerSFX()
    {
        if (_timerSFX == null) return;
        if (_musicSource.clip == _timerSFX && _musicSource.isPlaying) return;
        _musicSource.clip   = _timerSFX;
        _musicSource.loop   = true;
        _musicSource.volume = _musicVolume;
        _musicSource.Play();
    }

    public void StopTimerSFX() => StopMusic();

    // ── Respuestas ───────────────────────────────────────────────────────────
    public void PlayCorrect()
    {
        if (_correctSFX == null) return;
        _sfxSource.volume = _sfxVolume;
        _sfxSource.PlayOneShot(_correctSFX);
    }

    public void PlayIncorrect()
    {
        if (_incorrectSFX == null) return;
        _sfxSource.volume = _sfxVolume;
        _sfxSource.PlayOneShot(_incorrectSFX);
    }

    // ── Utilidades ───────────────────────────────────────────────────────────
    public void SetMusicVolume(float v)
    {
        _musicVolume = Mathf.Clamp01(v);
        _musicSource.volume = _musicVolume;
    }

    public void SetSFXVolume(float v)
    {
        _sfxVolume = Mathf.Clamp01(v);
        _sfxSource.volume = _sfxVolume;
    }
}
