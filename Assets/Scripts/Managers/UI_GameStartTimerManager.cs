using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_GameStartTimerManager : MonoBehaviour
{
    public static UI_GameStartTimerManager Instance;

    [SerializeField] private TMP_Text[] _gameStartTimerTexts;
    [SerializeField] private Image[] _gameStartImgs;

    public UnityEvent CoinCollectingGameStartEventHandler = new(); 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Assert(false, "매니저 Instance는 하나 뿐이어야 함");
            Destroy(Instance);
        }
    }
    private void Start()
    {
        Debug.Assert(_gameStartTimerTexts != null && _gameStartTimerTexts.Length == 4, "조건문 참고!! 게임 선조건임");
        Debug.Assert(_gameStartImgs != null && _gameStartImgs.Length == 4, "조건문 참고!! 게임 선조건임");
        Debug.Assert(CoinCollectingGameManager.Instance != null);
        CoinCollectingGameManager.Instance.CoinCollectingGameStartEventHandler.AddListener(OnCoinCollectingGameStarted);
    }

    private void OnDisable()
    {
        CoinCollectingGameManager.Instance.CoinCollectingGameStartEventHandler.RemoveListener(OnCoinCollectingGameStarted);
    }

    private void OnCoinCollectingGameStarted()
    {
        StopAllCoroutines();
        StartCoroutine(StartCoinCollectingGameTimer_Cor());
    }


    private IEnumerator StartCoinCollectingGameTimer_Cor()
    {
        for (int i = 5; i >= 1; --i)
        {
            yield return CoroutineManager.GetWaitForSec(1);

            for (int j = 0; j < 4; ++j)
            {
                _gameStartTimerTexts[j].text = $"{i}";
            }
        }

        yield return CoroutineManager.GetWaitForSec(1);
        for (int i = 0; i < 4; ++i)
        {
            _gameStartTimerTexts[i].gameObject.SetActive(false);
        }
        // _gameStartImg[i]들은 이미 활성화 상태가 False임.
        for (int i = 0; i < 4; ++i)
        {
            _gameStartImgs[i].gameObject.SetActive(true);
        }
        CoinCollectingGameStartEventHandler.Invoke();
    }
}
