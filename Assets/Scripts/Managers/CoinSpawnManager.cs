using UnityEngine;

public class CoinSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform _playersParentTransform;
    [SerializeField] private CoinLineGroupObjectPool _coinLineGroupPool;
    private const float SPAWN_INTERVAL_Z_VALUE = 15f;
    private float _nextSpawnZ = -8f;
    private void Update()
    {
        while (_playersParentTransform.position.z >= _nextSpawnZ)
        {
            _nextSpawnZ += SPAWN_INTERVAL_Z_VALUE;
            SpawnCoinGroup(_nextSpawnZ);
        }
    }

    private void SpawnCoinGroup(float zPos)
    {
        _coinLineGroupPool.SpawnCoinLineGroup((CoinLine.ECoinDirection)Random.Range(0, (int)CoinLine.ECoinDirection.Count), zPos + SPAWN_INTERVAL_Z_VALUE);
    }
}
