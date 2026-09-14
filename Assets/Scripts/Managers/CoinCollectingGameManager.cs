using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CoinCollectingGameManager : MonoBehaviour
{
    public static CoinCollectingGameManager Instance;

    public UnityEvent CoinCollectingGameStartEventHandler = new();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        Debug.Assert(gameObject.activeSelf, "CoinCollectingGameManager 게임 오브젝트는 언제나 activeSelf == true 여야 함");
        StartCoroutine(StartCoinCollectingGameAfter1Frame_Cor());
    }


    private void OnDisable()
    {
        CoinCollectingGameStartEventHandler.RemoveAllListeners();
    }

    private IEnumerator StartCoinCollectingGameAfter1Frame_Cor()
    {
        // 26-09-14 14:04 [Ace 이벤트 구독 타이밍 이슈 해결 위해서 한 프레임 후에 게임 시작하는 로직으로 수정]
        yield return null;
        CoinCollectingGameStartEventHandler.Invoke();
    }

}
