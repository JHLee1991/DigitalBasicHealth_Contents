using UnityEngine;

public class CoinSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _coinGroupPrefab;
    [SerializeField] private Transform _coinGroupParentTransform;
    [SerializeField] private Transform _playersParentTransform;
    private float _spawnInterval = 15f;
    private float _nextSpawnZ = 0f;

    private void Update()
    {
        while (_playersParentTransform.position.z >= _nextSpawnZ)
        {
            _nextSpawnZ += _spawnInterval;
            SpawnCoinGroup(_nextSpawnZ);
        }
    }

    private void SpawnCoinGroup(float zPos)
    {
        CoinLineGroup coinGroup = Instantiate(_coinGroupPrefab, _coinGroupParentTransform).GetComponent<CoinLineGroup>();
        coinGroup.Init((CoinLine.ECoinDirection)Random.Range(0, (int)CoinLine.ECoinDirection.Count), zPos + _spawnInterval);
    }
}
