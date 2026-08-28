using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public class CoinCollectingTarget : MonoBehaviour
{
    [Header("Scale Tween")]
    [Min(1f)]
    [SerializeField] private float _punchScale = 1.4f;
    [Min(0.01f)]
    [SerializeField] private float _duration = 0.32f;
    [Range(0.5f, 1f)]
    [SerializeField] private float _squashY = 0.82f;
    [Range(1f, 1.3f)]
    [SerializeField] private float _reboundScale = 1.12f;

    private Vector3 _initialScale;
    private Sequence _scaleSequence;

    private void Awake()
    {
        _initialScale = transform.localScale;
        CreateScaleSequence();
    }

    public void PlayScaleTween()
    {
        if (_scaleSequence == null || !_scaleSequence.IsActive())
        {
            CreateScaleSequence();
        }

        // 캐시된 Sequence를 처음부터 다시 재생한다.
        // 재생 중 다시 호출되어도 새로운 Tween은 생성하지 않는다.
        _scaleSequence.Restart();
    }

    private void CreateScaleSequence()
    {
        _scaleSequence?.Kill();

        float popDuration = _duration * 0.28f;
        float squashDuration = _duration * 0.16f;
        float reboundDuration = _duration * 0.22f;
        float settleDuration = _duration * 0.34f;

        Vector3 popScale = _initialScale * _punchScale;
        Vector3 squashScale = new Vector3(
            _initialScale.x * (_punchScale * 1.08f),
            _initialScale.y * _squashY,
            _initialScale.z);
        Vector3 reboundScale = _initialScale * _reboundScale;

        _scaleSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .SetUpdate(true)
            .Append(
                transform
                    .DOScale(popScale, popDuration)
                    .SetEase(Ease.OutBack, 2.2f))
            .Append(
                transform
                    .DOScale(squashScale, squashDuration)
                    .SetEase(Ease.InQuad))
            .Append(
                transform
                    .DOScale(reboundScale, reboundDuration)
                    .SetEase(Ease.OutBack, 1.8f))
            .Append(
                transform
                    .DOScale(_initialScale, settleDuration)
                    .SetEase(Ease.OutQuad))
            .Pause();
    }

    private void OnDisable()
    {
        if (_scaleSequence != null && _scaleSequence.IsActive())
        {
            _scaleSequence.Rewind();
        }

        transform.localScale = _initialScale;
    }

    private void OnDestroy()
    {
        _scaleSequence?.Kill();
        _scaleSequence = null;
    }
}
