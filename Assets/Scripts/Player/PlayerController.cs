using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GlobalDefine.EPlayerNumber EPlayerNumber { get; private set; }
    public void Init(GlobalDefine.EPlayerNumber ePlayerNumber, Vector3 pos)
    {
        EPlayerNumber = ePlayerNumber;
        transform.position = pos;
    }
    public void AddCoinScore()
    {
        Debug.Log("Player Get Score!!!!");
    }
}
