using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CoinCollectUIEffect : MonoBehaviour
{
    private sealed class IconMotion
    {
        public Image Image;
        public Vector2 Start;
        public Vector2 BurstEnd;
        public float Delay;
        public float Elapsed;
    }

    [Header("References")]
    [Tooltip("코인을 촬영하는 Player 전용 Camera입니다.")]
    [SerializeField] private Camera _worldCamera;
    [Tooltip("생성되는 코인 이미지가 들어갈 UI 부모입니다.")]
    [SerializeField] private RectTransform _effectRoot;
    [Tooltip("코인 이미지가 최종적으로 모일 UI 위치입니다.")]
    [SerializeField] private RectTransform _collectTarget;
    [Tooltip("Project 창의 코인 Sprite를 넣습니다.")]
    [SerializeField] private Sprite _coinSprite;

    [Header("Icon")]
    private int _iconCount = 1;
    [Min(1)] [SerializeField] private int _initialPoolSize = 20;
    [SerializeField] private Vector2 _iconSize = new Vector2(64f, 64f);

    [Header("Motion")]
    [Min(0.01f)] [SerializeField] private float _duration = 0.65f;
    [Min(0f)] [SerializeField] private float _staggerDelay = 0.04f;
    [Min(0f)] [SerializeField] private float _burstRadius = 90f;
    [Min(0f)] [SerializeField] private float _arcHeight = 80f;
    [Range(0.05f, 0.8f)] [SerializeField] private float _burstRatio = 0.3f;

    private readonly Queue<Image> _imagePool = new Queue<Image>();
    private readonly Stack<IconMotion> _motionPool = new Stack<IconMotion>();
    private readonly List<IconMotion> _activeMotions = new List<IconMotion>();

    private Canvas _canvas;
    private Camera _uiCamera;
    private bool _isInitialized;

    private void Awake()
    {
        if (_effectRoot == null)
        {
            _effectRoot = transform as RectTransform;
        }

        _canvas = _effectRoot != null
            ? _effectRoot.GetComponentInParent<Canvas>()
            : null;

        if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            _uiCamera = _canvas.worldCamera;
        }

        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        for (int i = 0; i < _initialPoolSize; i++)
        {
            _imagePool.Enqueue(CreateImage());
            _motionPool.Push(new IconMotion());
        }

        _isInitialized = true;
    }

    private bool ValidateReferences()
    {
        if (_worldCamera != null && _effectRoot != null &&
            _collectTarget != null && _coinSprite != null && _canvas != null)
        {
            return true;
        }

        Debug.LogError(
            $"[{nameof(CoinCollectUIEffect)}] Camera, Effect Root, " +
            "Collect Target, Coin Sprite, Canvas 설정을 확인하세요.", this);
        return false;
    }

    public void Play(Vector3 worldPosition)
    {
        if (!_isInitialized)
        {
            return;
        }

        Vector3 screenPosition = _worldCamera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z <= 0f ||
            !TryScreenToEffectPosition(screenPosition, out Vector2 start))
        {
            return;
        }

        for (int i = 0; i < _iconCount; i++)
        {
            SpawnIcon(start, i * _staggerDelay);
        }
    }

    private void SpawnIcon(Vector2 start, float delay)
    {
        Image image = GetImage();
        RectTransform iconTransform = image.rectTransform;

        Vector2 randomDirection = Random.insideUnitCircle;
        if (randomDirection.sqrMagnitude < 0.001f)
        {
            randomDirection = Vector2.up;
        }

        randomDirection.Normalize();
        float randomRadius = Random.Range(_burstRadius * 0.55f, _burstRadius);

        iconTransform.anchoredPosition = start;
        iconTransform.localScale = Vector3.one * 0.35f;
        image.gameObject.SetActive(true);
        iconTransform.SetAsLastSibling();

        IconMotion motion = _motionPool.Count > 0
            ? _motionPool.Pop()
            : new IconMotion();

        motion.Image = image;
        motion.Start = start;
        motion.BurstEnd = start + randomDirection * randomRadius;
        motion.Delay = delay;
        motion.Elapsed = 0f;
        _activeMotions.Add(motion);
    }

    private void Update()
    {
        for (int i = _activeMotions.Count - 1; i >= 0; i--)
        {
            IconMotion motion = _activeMotions[i];
            motion.Elapsed += Time.unscaledDeltaTime;

            float movementTime = motion.Elapsed - motion.Delay;
            if (movementTime < 0f)
            {
                continue;
            }

            float normalizedTime = Mathf.Clamp01(movementTime / _duration);
            UpdateMotion(motion, normalizedTime);

            if (normalizedTime >= 1f)
            {
                ReleaseMotion(i, motion);
            }
        }
    }

    private void UpdateMotion(IconMotion motion, float normalizedTime)
    {
        RectTransform iconTransform = motion.Image.rectTransform;

        if (normalizedTime < _burstRatio)
        {
            float burstTime = normalizedTime / _burstRatio;
            float easedTime = 1f - Mathf.Pow(1f - burstTime, 3f);
            iconTransform.anchoredPosition = Vector2.LerpUnclamped(
                motion.Start, motion.BurstEnd, easedTime);
            iconTransform.localScale =
                Vector3.one * Mathf.Lerp(0.35f, 1f, easedTime);
            return;
        }

        if (!TryGetTargetPosition(out Vector2 target))
        {
            return;
        }

        float collectTime =
            (normalizedTime - _burstRatio) / (1f - _burstRatio);
        float easedCollectTime = collectTime * collectTime * collectTime;
        Vector2 controlPoint =
            (motion.BurstEnd + target) * 0.5f + Vector2.up * _arcHeight;

        iconTransform.anchoredPosition = EvaluateQuadraticBezier(
            motion.BurstEnd, controlPoint, target, easedCollectTime);
        iconTransform.localScale =
            Vector3.one * Mathf.Lerp(1f, 0.55f, easedCollectTime);
    }

    private bool TryGetTargetPosition(out Vector2 targetPosition)
    {
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            _uiCamera, _collectTarget.position);
        return TryScreenToEffectPosition(screenPosition, out targetPosition);
    }

    private bool TryScreenToEffectPosition(
        Vector2 screenPosition, out Vector2 effectPosition)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _effectRoot, screenPosition, _uiCamera, out effectPosition);
    }

    private static Vector2 EvaluateQuadraticBezier(
        Vector2 start, Vector2 control, Vector2 end, float time)
    {
        float inverseTime = 1f - time;
        return
            inverseTime * inverseTime * start +
            2f * inverseTime * time * control +
            time * time * end;
    }

    private Image GetImage()
    {
        return _imagePool.Count > 0 ? _imagePool.Dequeue() : CreateImage();
    }

    private Image CreateImage()
    {
        GameObject imageObject = new GameObject(
            "CoinCollectIcon", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(_effectRoot, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = _coinSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.rectTransform.sizeDelta = _iconSize;
        imageObject.SetActive(false);
        return image;
    }

    private void ReleaseMotion(int index, IconMotion motion)
    {
        motion.Image.gameObject.SetActive(false);
        _imagePool.Enqueue(motion.Image);
        motion.Image = null;
        _motionPool.Push(motion);
        _activeMotions.RemoveAt(index);
    }
}
