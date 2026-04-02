using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioData", menuName = "Eduzo/Audio/GameAudioData")]
public class GameAudioData : ScriptableObject
{
    [Header("Music")]
    public AudioClip BackgroundMusic;

    [Header("SFX")]
    public AudioClip HonkClip;
    public AudioClip CorrectClip;
    public AudioClip WrongClip;
    public AudioClip WinClip;

    [Header("UI")]
    public AudioClip ButtonClickClip;
}