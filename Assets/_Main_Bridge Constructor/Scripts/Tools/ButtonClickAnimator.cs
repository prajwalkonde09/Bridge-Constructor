using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonClickAnimator : MonoBehaviour, IPointerClickHandler
{
    [Header("Animation")]
    public float ScaleDown = 0.9f;
    public float Duration = 0.1f;

    private Vector3 originalScale;
    private bool isAnimating;
    private Coroutine animRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSound();
        PlayAnimation();
    }

    void PlayClickSound()
    {
        if (GameAudioManager.Instance == null || GameAudioManager.Instance.AudioData == null)
            return;

        var clip = GameAudioManager.Instance.AudioData.ButtonClickClip; // using correct clip as click (you can change later)

        if (clip == null)
            return;

        GameAudioManager.Instance.SFXSource.Stop(); // prevent overlap
        GameAudioManager.Instance.SFXSource.PlayOneShot(clip);
    }

    void PlayAnimation()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        if (!gameObject.activeInHierarchy)
            return;

        if (GameAudioManager.Instance != null)
        {
            animRoutine = GameAudioManager.Instance.StartCoroutine(Animate());
        }
    }

    IEnumerator Animate()
    {
        isAnimating = true;

        Vector3 targetScale = originalScale * ScaleDown;

        // Scale Down
        float t = 0f;
        while (t < Duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / Duration);
            yield return null;
        }

        // Scale Up
        t = 0f;
        while (t < Duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t / Duration);
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }
}