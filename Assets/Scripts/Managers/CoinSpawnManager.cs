using System.Collections;
using UnityEngine;

public class CoinSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform _playersParentTransform;
    [SerializeField] private CoinLineGroupObjectPool _coinLineGroupPool;
    [SerializeField] private float _initZPos = 20f;
    private const int SPAWN_COIN_TIME_INTERVAL = 5;

    private void OnGameStartTimerReachToZero()
    {
        Debug.Log("CoinSpawnManager 코인 스폰 시작!!");
        StopAllCoroutines();
        StartCoroutine(StartSpawnCoinGroup_Cor());
    }
    private void Start()
    {
        Debug.Assert(UI_GameStartTimerManager.Instance != null);
        UI_GameStartTimerManager.Instance.CoinCollectingGameStartEventHandler.AddListener(OnGameStartTimerReachToZero);
    }

    private void OnDisable()
    {
        if (UI_GameStartTimerManager.Instance != null)
        {
            UI_GameStartTimerManager.Instance.CoinCollectingGameStartEventHandler.RemoveListener(OnGameStartTimerReachToZero);
        }
    }

    private IEnumerator StartSpawnCoinGroup_Cor()
    {
        while (true)
        {
            SpawnCoinGroup();
            yield return CoroutineManager.GetWaitForSec(SPAWN_COIN_TIME_INTERVAL);
        }
    }
    private void SpawnCoinGroup()
    {
        _coinLineGroupPool.SpawnCoinLineGroup((CoinLine.ECoinDirection)Random.Range(0, (int)CoinLine.ECoinDirection.Count), _initZPos);
    }
}
