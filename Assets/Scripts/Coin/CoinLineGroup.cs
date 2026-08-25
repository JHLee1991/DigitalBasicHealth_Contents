using UnityEngine;
using UnityEngine.Pool;

public class CoinLineGroup : MonoBehaviour
{
    [SerializeField] private CoinLine[] _coinLines;

    private ObjectPool<CoinLineGroup> _pool;
    private const float _moveRange = 1f;
    private const float _moveSpeed = 1f;
    private const float HIDE_TIME_IN_SEC = 10f;
    private float _hideTimeTimer;
    private float _moveTimer;
    private bool _isTimerStarted;

    private Vector3 _initialLocalPosition;
    public void SetPool(ObjectPool<CoinLineGroup> pool)
    {
        Debug.Assert(pool != null);
        _pool = pool;
    }
    public void Init(CoinLine.ECoinDirection eCoinDirection, float initZPos)
    {
        _isTimerStarted = true;
        _hideTimeTimer = 0f;
        transform.localPosition = new Vector3(0, 0, initZPos);
        _initialLocalPosition = transform.localPosition;
        foreach (CoinLine coinLine in _coinLines)
        {
            coinLine.Init(eCoinDirection);
        }
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
        float xOffset = Mathf.PingPong((_moveTimer * _moveSpeed) + _moveRange, _moveRange * 2f) - _moveRange;
        transform.localPosition = _initialLocalPosition + Vector3.right * xOffset;
    }
    private void UpdateHideTimer()
    {
        _hideTimeTimer += Time.deltaTime;

        if (_hideTimeTimer >= HIDE_TIME_IN_SEC)
        {
            HideCoinGroup();
        }
    }

    private void HideCoinGroup()
    {
        _isTimerStarted = false;
        transform.localPosition = _initialLocalPosition;
        foreach (CoinLine coinLine in _coinLines)
        {
            coinLine.Hide();
        }
        Debug.Assert(_pool != null);
        _pool.Release(this);
    }
}
