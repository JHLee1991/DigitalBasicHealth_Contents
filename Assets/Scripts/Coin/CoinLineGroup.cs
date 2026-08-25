using UnityEngine;

public class CoinLineGroup : MonoBehaviour
{
    [SerializeField] private CoinLine[] _coinLines;
    public void Init(CoinLine.ECoinDirection eCoinDirection, float initZPos)
    {
        transform.localPosition = new Vector3(0, 0, initZPos);
        foreach (CoinLine coinLine in _coinLines)
        {
            coinLine.Init(eCoinDirection);
        }
    }

    public void HideCoinGroup()
    {
        foreach (CoinLine coinLine in _coinLines)
        {
            coinLine.Hide();
        }
    }
}
