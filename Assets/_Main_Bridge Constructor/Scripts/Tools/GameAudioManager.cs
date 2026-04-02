using UnityEngine;
using UnityEngine.UI;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("Data")]
    public GameAudioData AudioData;

    [Header("Sources")]
    public AudioSource MusicSource;
    public AudioSource SFXSource;

    [Header("Volume")]
    [Range(0f, 1f)] public float MusicVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;

    [Header("Optional Sliders")]
    public Slider MusicSlider;
    public Slider SFXSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ApplyVolume();
        SetupSliders();
    }

    private void Start()
    {
        PlayMusic();
    }

    void SetupSliders()
    {
        if (MusicSlider != null)
        {
            MusicSlider.value = MusicVolume;
            MusicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (SFXSlider != null)
        {
            SFXSlider.value = SFXVolume;
            SFXSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    void ApplyVolume()
    {
        if (MusicSource != null)
            MusicSource.volume = MusicVolume;

        if (SFXSource != null)
            SFXSource.volume = SFXVolume;
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = value;
        ApplyVolume();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        ApplyVolume();
    }

    // ---------------- MUSIC ----------------

    public void PlayMusic()
    {
        if (AudioData == null || AudioData.BackgroundMusic == null)
            return;

        MusicSource.clip = AudioData.BackgroundMusic;
        MusicSource.loop = true;
        MusicSource.Play();
    }

    // ---------------- SFX ----------------

    public void PlayCorrect()
    {
        PlaySFX(AudioData.CorrectClip);
    }

    public void PlayWrong()
    {
        PlaySFX(AudioData.WrongClip);
    }

    public void PlayWin()
    {
        PlaySFX(AudioData.WinClip);
    }

    public void Honk()
    {
        PlaySFX(AudioData.HonkClip);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        SFXSource.Stop(); // 🔥 prevents overlap
        SFXSource.PlayOneShot(clip);
    }
}