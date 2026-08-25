using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _player_1_Prefab;
    [SerializeField] private GameObject _player_2_Prefab;
    [SerializeField] private GameObject _player_3_Prefab;
    [SerializeField] private GameObject _player_4_Prefab;

    [SerializeField] private Transform _playerParentTransform;

    private const float INIT_Z_VALUE = 10f;

    private void Start()
    {
        SpawnPlayer(GlobalDefine.EGameSceneType.GameScene_1, GlobalDefine.EPlayerNumber.Player_1);
    }
    // TODO : 25-08-25 [Ace 이곳 나중에 고쳐야 함]
    private void SpawnPlayer(GlobalDefine.EGameSceneType eGameSceneType, GlobalDefine.EPlayerNumber ePlayerNumber)
    {
        switch (eGameSceneType)
        {
            case GlobalDefine.EGameSceneType.GameScene_1:
                Instantiate(_player_1_Prefab, _playerParentTransform).GetComponentInChildren<PlayerController>().Init(GlobalDefine.EPlayerNumber.Player_1, new Vector3(-2, 0, INIT_Z_VALUE));
                Instantiate(_player_2_Prefab, _playerParentTransform).GetComponentInChildren<PlayerController>().Init(GlobalDefine.EPlayerNumber.Player_2, new Vector3(0, 0, INIT_Z_VALUE));
                Instantiate(_player_3_Prefab, _playerParentTransform).GetComponentInChildren<PlayerController>().Init(GlobalDefine.EPlayerNumber.Player_3, new Vector3(2, 0, INIT_Z_VALUE));
                Instantiate(_player_4_Prefab, _playerParentTransform).GetComponentInChildren<PlayerController>().Init(GlobalDefine.EPlayerNumber.Player_4, new Vector3(4, 0, INIT_Z_VALUE));
                break;
            case GlobalDefine.EGameSceneType.GameScene_2:
                break;
            case GlobalDefine.EGameSceneType.GameScene_3:
                break;
            case GlobalDefine.EGameSceneType.GameScene_4:
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }
}
