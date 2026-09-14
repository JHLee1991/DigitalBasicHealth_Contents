using System.Collections;
using UnityEngine;

public class CoinCollectingGameObjectSpawnManager : MonoBehaviour
{
    [Header("Coin")]
    [SerializeField] private CoinLineGroupObjectPool _coinLineGroupPool;
    [SerializeField] private float _initZPos = 20f;

    [Header("Obstacle")]
    [SerializeField] private ObstacleObjectPool _obstaclePool;
    [SerializeField] private Transform[] _obstacleParentTransforms;
    private const int SPAWN_COIN_TIME_INTERVAL = 5;

    private void OnGameStartTimerReachToZero()
    {
        Debug.Log("CoinSpawnManager 코인 스폰 시작!!");
        StopAllCoroutines();
        StartCoroutine(StartSpawnCoinGroup_Cor());
    }
    private void Start()
    {
        Debug.Assert(UI_CoinCollectingGameStartTimerManager.Instance != null);
        Debug.Assert(_coinLineGroupPool != null);
        Debug.Assert(_obstaclePool != null);
        Debug.Assert(_obstacleParentTransforms != null && _obstacleParentTransforms.Length == 4);
        UI_CoinCollectingGameStartTimerManager.Instance.CoinCollectingGameStartEventHandler.AddListener(OnGameStartTimerReachToZero);
    }

    private void OnDisable()
    {
        if (UI_CoinCollectingGameStartTimerManager.Instance != null)
        {
            UI_CoinCollectingGameStartTimerManager.Instance.CoinCollectingGameStartEventHandler.RemoveListener(OnGameStartTimerReachToZero);
        }
    }

    private IEnumerator StartSpawnCoinGroup_Cor()
    {
        while (true)
        {
            if (Random.Range(0, 2) == 0)
            {
                for (int i = 0; i < 4; ++i)
                {
                    SpawnObstacle(_obstacleParentTransforms[i]);
                }
            }
            else
            {
                SpawnCoinGroup();
            }
            yield return CoroutineManager.GetWaitForSec(SPAWN_COIN_TIME_INTERVAL);
        }
    }
    private void SpawnCoinGroup()
    {
        _coinLineGroupPool.SpawnCoinLineGroup((CoinLine.ECoinDirection)Random.Range(0, (int)CoinLine.ECoinDirection.Count), _initZPos);
    }

    private void SpawnObstacle(Transform parentTransform)
    {
        _obstaclePool.SpawnObstacle(parentTransform);
    }
}
