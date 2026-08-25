using UnityEngine;
using UnityEngine.Pool;

public class CoinLineGroupObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject _coinLineGroupPrefab;
    [SerializeField] private Transform _coinLineGroupParentTransform;
    public const int MAX_COIN_LINE_GROUP_COUNT = 5;

    private ObjectPool<CoinLineGroup> _coinLineGroupPool;
    public void SpawnCoinLineGroup(CoinLine.ECoinDirection eCoinDirection, float zPos)
    {
        _coinLineGroupPool.Get().Init(eCoinDirection, zPos);
    }

    private void Awake()
    {
        EnsurePoolNotNull();
    }
    private void EnsurePoolNotNull()
    {
        if (_coinLineGroupPool == null)
        {
            _coinLineGroupPool = new ObjectPool<CoinLineGroup>(
                OnCreateTape,
                OnGetTape,
                OnReleaseToPool,
                OnDestroyTape,
                defaultCapacity: MAX_COIN_LINE_GROUP_COUNT,
                maxSize: MAX_COIN_LINE_GROUP_COUNT
            );
        }
    }

    #region Pool
    private CoinLineGroup OnCreateTape()
    {
        CoinLineGroup tc = Instantiate(_coinLineGroupPrefab, _coinLineGroupParentTransform, true).GetComponent<CoinLineGroup>();
        Debug.Assert(tc != null);
        if (tc != null)
        {
            EnsurePoolNotNull();
            tc.SetPool(_coinLineGroupPool);
            return tc;
        }
        return null;
    }


    private void OnGetTape(CoinLineGroup tc)
    {
        Debug.Assert(tc != null);
        tc.gameObject.SetActive(true);
    }
    private void OnReleaseToPool(CoinLineGroup tc)
    {
        tc.gameObject.SetActive(false);
    }

    private void OnDestroyTape(CoinLineGroup tc)
    {
        Destroy(tc.gameObject);
    }
    #endregion
}
