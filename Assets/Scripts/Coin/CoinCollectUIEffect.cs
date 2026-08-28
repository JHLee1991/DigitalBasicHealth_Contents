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
        public float Elapsed;
    }

    [Header("References")]
    [Tooltip("코인을 촬영하는 Player 전용 Camera입니다.")]
    [SerializeField] private Camera _worldCamera;
    [Tooltip("생성되는 코인 이미지가 들어갈 UI 부모입니다.")]
    [SerializeField] private RectTransform _effectRoot;
    [Tooltip("코인 이미지가 최종적으로 모일 UI 위치입니다.")]
    [SerializeField] private RectTransform _collectTarget;
    [Tooltip("코인이 도착했을 때 Scale Tween을 재생할 대상입니다.")]
    [SerializeField] private CoinCollectingTarget _collectingTarget;
    [Tooltip("Project 창의 코인 Sprite를 넣습니다.")]
    [SerializeField] private Sprite _coinSprite;

    [Header("Icon")]
    [Min(1)] [SerializeField] private int _initialPoolSize = 20;
    [SerializeField] private Vector2 _iconSize = new Vector2(64f, 64f);

    [Header("Motion")]
    [Min(0.01f)] [SerializeField] private float _duration = 0.65f;          // 코인 이미지 하나가 움직이기 시작한 후 목표 지점에 도착하기까지 걸리는 시간
    [Min(0f)] [SerializeField] private float _staggerDelay = 0.04f;         // 여러 코인 이미지가 움직이기 시작하는 시간 간격
    [Min(0f)] [SerializeField] private float _burstRadius = 90f;            // 코인 이미지가 생성된 직후 충돌 위치 주변으로 얼마나 멀리 퍼지는지를 결정
    [Min(0f)] [SerializeField] private float _arcHeight = 80f;              // 코인이 흩어진 지점에서 ScoreTarget으로 이동할 때 곡선이 얼마나 위로 휘어지는지 결정
    [Range(0.05f, 0.8f)] [SerializeField] private float _burstRatio = 0.3f; // 전체 이동 시간 중에서 처음 퍼지는 동작에 사용할 비율
                                                                            // 기본값은 0.3이므로: 전체 시간의 30% → 주변으로 퍼짐, 전체 시간의 70% → ScoreTarget으로 이동

    private readonly Queue<Image> _imagePool = new Queue<Image>();
    private readonly Queue<Vector2> _pendingSpawnPositions =
        new Queue<Vector2>();
    private readonly Stack<IconMotion> _motionPool = new Stack<IconMotion>();
    private readonly List<IconMotion> _activeMotions = new List<IconMotion>();

    private Canvas _canvas;
    private Camera _uiCamera;
    private bool _isInitialized;
    private float _nextSpawnTime;

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

        if (_collectingTarget == null && _collectTarget != null)
        {
            _collectingTarget =
                _collectTarget.GetComponent<CoinCollectingTarget>();
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

        // 충돌 한 번당 이미지 생성 요청은 정확히 하나만 추가한다.
        // 거의 동시에 여러 Coin이 충돌하면 요청들이 Queue에 쌓이고,
        // Update에서 _staggerDelay 간격으로 하나씩 실제 생성된다.
        _pendingSpawnPositions.Enqueue(start);
    }

    private void SpawnIcon(Vector2 start)
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
        motion.Elapsed = 0f;
        _activeMotions.Add(motion);
    }

    private void Update()
    {
        SpawnPendingIcon();

        for (int i = _activeMotions.Count - 1; i >= 0; i--)
        {
            IconMotion motion = _activeMotions[i];
            motion.Elapsed += Time.unscaledDeltaTime;

            float normalizedTime = Mathf.Clamp01(motion.Elapsed / _duration);
            UpdateMotion(motion, normalizedTime);

            if (normalizedTime >= 1f)
            {
                ReleaseMotion(i, motion);
            }
        }
    }

    private void SpawnPendingIcon()
    {
        if (_pendingSpawnPositions.Count == 0)
        {
            return;
        }

        if (_staggerDelay <= 0f)
        {
            while (_pendingSpawnPositions.Count > 0)
            {
                SpawnIcon(_pendingSpawnPositions.Dequeue());
            }

            return;
        }

        if (Time.unscaledTime < _nextSpawnTime)
        {
            return;
        }

        SpawnIcon(_pendingSpawnPositions.Dequeue());
        _nextSpawnTime = Time.unscaledTime + _staggerDelay;
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
        _collectingTarget?.PlayScaleTween();

        motion.Image.gameObject.SetActive(false);
        _imagePool.Enqueue(motion.Image);
        motion.Image = null;
        _motionPool.Push(motion);
        _activeMotions.RemoveAt(index);
    }
}
