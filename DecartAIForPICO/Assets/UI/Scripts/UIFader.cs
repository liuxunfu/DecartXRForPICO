using UnityEngine;
using DG.Tweening;
using System;
using Sequence = DG.Tweening.Sequence;

[RequireComponent(typeof(CanvasGroup))]
public class UIFader : MonoBehaviour
{
    const float DEFAULT_DURATION = 0.2f;

    private CanvasGroup _canvasGroup;
    private Sequence _sequence;
    

    public void Awake()
    {
        if (_canvasGroup != null) return;
        _canvasGroup = GetComponent<CanvasGroup>();
        UpdateInteractable(_canvasGroup.alpha);
    }


    private void UpdateInteractable(float alpha)
    {
        var isInteractable = alpha > 0.0001f;
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
    }


    public void SetAlpha(float alpha)
    {
        if (_sequence != null)
        {
            _sequence.Kill();
            _sequence = null;
        }

        if (_canvasGroup.alpha == alpha) return;

        _canvasGroup.alpha = alpha;
        UpdateInteractable(alpha);
    }
    public float GetAlpha() => _canvasGroup.alpha;
    public void Hide() => SetAlpha(0);
    public void Show() => SetAlpha(1);
    public void FadeIn(float delay) => FadeIn(null, delay);
    public void FadeIn(Action callback = null, float delay = 0f)
        => FadeTo(1, callback, DEFAULT_DURATION, delay);
    public void FadeOut(float delay) => FadeOut(null, delay);
    public void FadeOut(Action callback = null, float delay = 0f)
        => FadeTo(0, callback, DEFAULT_DURATION, delay);
    public void FadeTo(float alpha, float duration)
        => FadeTo(alpha, null, duration);
    
    public void FadeTo(float alpha, Action callback = null, float duration = DEFAULT_DURATION, float delay = 0)
    {
        if (_canvasGroup == null || _canvasGroup.gameObject == null)
            return;
        UpdateInteractable(alpha);

        _sequence?.Kill();

        _sequence = DOTween.Sequence();
        _sequence.AppendInterval(delay)
            .Append(_canvasGroup.DOFade(alpha, duration).From(_canvasGroup.alpha))
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                callback?.Invoke();
                _sequence = null;
            });

    }
}
