using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpawnManager : MonoBehaviour
{

    public UnityEvent<PlayerController> PlayerSpawnedEventHandler = new();

    [SerializeField] private GameObject _player_1_Prefab;
    [SerializeField] private GameObject _player_2_Prefab;
    [SerializeField] private GameObject _player_3_Prefab;
    [SerializeField] private GameObject _player_4_Prefab;

    [SerializeField] private Transform _playerParentTransform;

    private const float INIT_Z_VALUE = 10f;

    private void Start()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnPlayerAfter1Frame_Cor());
    }
    private void OnDestroy()
    {
        PlayerSpawnedEventHandler.RemoveAllListeners();
    }

    // 26-09-14 16:15 [Ace 이벤트 구독 타이밍 이슈를 방지하기 위해 아예 1 프레임 쉬고 SpawnPlayer 호출하도록 함]
    private IEnumerator SpawnPlayerAfter1Frame_Cor()
    {
        yield return null;
        SpawnPlayer(GlobalDefine.EGameSceneType.GameScene_1, GlobalDefine.EPlayerNumber.Player_1);
    }


    // TODO : 25-08-25 [Ace 이곳 나중에 고쳐야 함. 다 실제 데이터 연동해서 동적로딩 방식으로 해야함. 지금은 테스트용으로 이렇게 내비둠]
    private void SpawnPlayer(GlobalDefine.EGameSceneType eGameSceneType, GlobalDefine.EPlayerNumber ePlayerNumber)
    {
        switch (eGameSceneType)
        {
            case GlobalDefine.EGameSceneType.GameScene_1:
                PlayerController spawnPC = null;
                spawnPC = Instantiate(_player_1_Prefab, _playerParentTransform).GetComponentInChildren<PlayerController>();
                spawnPC.Init(GlobalDefine.EPlayerNumber.Player_1, new Vector3(-15, 0, INIT_Z_VALUE));
                PlayerSpawnedEventHandler.Invoke(spawnPC);
                spawnPC = Instantiate(_player_2_Prefab, _playerParentTransform).GetComponentInChildren<PlayerController>();
                spawnPC.Init(GlobalDefine.EPlayerNumber.Player_2, new Vector3(-9, 0, INIT_Z_VALUE));
                PlayerSpawnedEventHandler.Invoke(spawnPC);
                spawnPC = Instantiate(_player_3_Prefab, _playerParentTransform).GetComponentInChildren<PlayerController>();
                spawnPC.Init(GlobalDefine.EPlayerNumber.Player_3, new Vector3(-3, 0, INIT_Z_VALUE));
                PlayerSpawnedEventHandler.Invoke(spawnPC);
                spawnPC = Instantiate(_player_4_Prefab, _playerParentTransform).GetComponentInChildren<PlayerController>();
                spawnPC.Init(GlobalDefine.EPlayerNumber.Player_4, new Vector3(3, 0, INIT_Z_VALUE));
                PlayerSpawnedEventHandler.Invoke(spawnPC);
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
