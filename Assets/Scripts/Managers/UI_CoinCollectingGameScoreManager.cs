using UnityEngine;

public class UI_CoinCollectingGameScoreManager : MonoBehaviour
{
    [SerializeField] private PlayerSpawnManager _playerSpawnManager;
    [SerializeField] private UI_PlayerScoreText[] _playerScoreTexts;

    private int _currIdx;
    private void Start()
    {
        _playerSpawnManager.PlayerSpawnedEventHandler.AddListener(InitPlayerScoreText);
    }

    private void OnDisable()
    {
        _playerSpawnManager.PlayerSpawnedEventHandler.RemoveListener(InitPlayerScoreText);
    }

    public void InitPlayerScoreText(PlayerController pc)
    {
        Debug.Assert(_currIdx < 4);
        _playerScoreTexts[_currIdx++].Init(pc);
    }
}
