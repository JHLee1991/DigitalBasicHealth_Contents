using UnityEngine;
using UnityEngine.Pool;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Obstacle : MonoBehaviour
{
    private ObjectPool<Obstacle> _pool;
    private const float X_MOVE_RANGE = 1f;
    private const float X_MOVE_SPEED = 1f;
    private const float Z_MOVE_SPEED = 3f;
    private const float HIDE_TIME_IN_SEC = 12f;
    private float _hideTimeTimer;
    private float _moveTimer;
    private bool _isTimerStarted;
    private Vector3 _initialLocalPosition;
    private bool _isCollidedWithPlayer;
    private int _playerLayer;
    public void SetPool(ObjectPool<Obstacle> pool)
    {
        Debug.Assert(pool != null);
        _pool = pool;
    }
    public void Init(Transform parentTransform)
    {
        Debug.Assert(parentTransform != null);
        _isTimerStarted = true;
        _hideTimeTimer = 0f;
        _moveTimer = 0f;
        _isCollidedWithPlayer = false;
        transform.SetParent(parentTransform);
        transform.localPosition = new Vector3(0, 0, 0);
        _initialLocalPosition = transform.localPosition;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != _playerLayer)
        {
            return;
        }
        if (!other.TryGetComponent<PlayerCollider>(out var pc))
        {
            return;
        }
        if (_isCollidedWithPlayer)
        {
            return;
        }
        Debug.Log("Obstacle이 Player와 충돌했습니다!!");
        _isCollidedWithPlayer = true;
        Vector3 collectWorldPosition = other.transform.position;
        EffectManager.Instance.PlayCoinHitParticle(collectWorldPosition);
    }

    private void Awake()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
    }
    private void Update()
    {
        if (!_isTimerStarted)
        {
            return;
        }

        UpdateMovement();
        UpdateHideTimer();
    }
    private void UpdateMovement()
    {
        _moveTimer += Time.deltaTime;

        float xOffset = Mathf.PingPong((_moveTimer * X_MOVE_SPEED) + X_MOVE_RANGE, X_MOVE_RANGE * 2f) - X_MOVE_RANGE;

        Vector3 currentPosition = transform.localPosition;
        currentPosition.x = _initialLocalPosition.x + xOffset;
        currentPosition.z -= Z_MOVE_SPEED * Time.deltaTime;
        transform.localPosition = currentPosition;
    }
    private void UpdateHideTimer()
    {
        _hideTimeTimer += Time.deltaTime;

        if (_hideTimeTimer >= HIDE_TIME_IN_SEC)
        {
            ReleaseToPool();
        }
    }

    private void ReleaseToPool()
    {
        _isTimerStarted = false;
        transform.localPosition = _initialLocalPosition;
        Debug.Assert(_pool != null);
        _pool.Release(this);
    }
}
