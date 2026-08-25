using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GlobalDefine.EPlayerNumber EPlayerNumber { get; private set; }
    public void Init(GlobalDefine.EPlayerNumber ePlayerNumber, Vector3 localPos)
    {
        EPlayerNumber = ePlayerNumber;
        transform.localPosition = localPos;
        switch (ePlayerNumber)
        {
            case GlobalDefine.EPlayerNumber.Player_1:
                break;
            case GlobalDefine.EPlayerNumber.Player_2:
                break;
            case GlobalDefine.EPlayerNumber.Player_3:
                break;
            case GlobalDefine.EPlayerNumber.Player_4:
                break;
            default:
                Debug.Assert(false);
                break;
        }
        gameObject.name = $"Player_{((int)ePlayerNumber) + 1}";
    }
    public void AddCoinScore()
    {
    }
}
