using UnityEngine;

public class CoinCollectingEffectManager : MonoBehaviour
{
    [SerializeField] private PlayerSpawnManager _playerSpawnManager;
    [SerializeField] private CoinCollectUIEffect[] _coinCollectUIEffects;

    private void Awake()
    {
        _playerSpawnManager.PlayerSpawnedEventHandler.AddListener(OnPlayerSpawned);
    }
    private void OnDisable()
    {
        _playerSpawnManager.PlayerSpawnedEventHandler.RemoveListener(OnPlayerSpawned);
    }
    private void OnPlayerSpawned(PlayerController pc)
    {
        Debug.Assert(pc != null);
        Debug.Assert(pc.PlayerColliders != null && _coinCollectUIEffects != null);

        Debug.Log("CoinCollectingEffectManager.OnPlayerSpawned!");
        for (int i = 0; i < pc.PlayerColliders.Length; ++i)
        {
            pc.PlayerColliders[i].SetCoinCollectUIEffect(_coinCollectUIEffects[(int)pc.EPlayerNumber]);
        }
    }
}
