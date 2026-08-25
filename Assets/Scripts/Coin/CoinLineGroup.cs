using UnityEngine;
using UnityEngine.Pool;

public class CoinLineGroup : MonoBehaviour
{
    [SerializeField] private CoinLine[] _coinLines;

    private ObjectPool<CoinLineGroup> _pool;

    private const float HIDE_TIME_IN_SEC = 10f;
    private float _hideTimeTimer;
    private bool _isTimerStarted;
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
        foreach (CoinLine coinLine in _coinLines)
        {
            coinLine.Init(eCoinDirection);
        }
    }

    private void Update()
    {
        if (_isTimerStarted)
        {
            _hideTimeTimer += Time.deltaTime;
            if (_hideTimeTimer >= HIDE_TIME_IN_SEC)
            {
                HideCoinGroup();
            }
        }
    }
    private void HideCoinGroup()
    {
        _isTimerStarted = false;
        foreach (CoinLine coinLine in _coinLines)
        {
            coinLine.Hide();
        }
        Debug.Assert(_pool != null);
        _pool.Release(this);
    }
}
