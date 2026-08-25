using UnityEngine;

public class CoinLine : MonoBehaviour
{
    [SerializeField] private Coin[] _coins;

    private const float COIN_SPACING = 0.25f;
    public enum ECoinDirection
    {
        Horizontal,
        Vertical,
        Diagonal_LeftUp,
        Diagonal_RightUp,
        Count
    }
    public void Init(ECoinDirection eCoinDirection)
    {
        Vector3 direction;
        switch (eCoinDirection)
        {
            case ECoinDirection.Horizontal:
                direction = new Vector3(1f, 0f, 0f);
                break;

            case ECoinDirection.Vertical:
                direction = new Vector3(0f, 1f, 0f);
                break;

            case ECoinDirection.Diagonal_LeftUp:
                direction = new Vector3(1f, -1f, 0f);
                break;

            case ECoinDirection.Diagonal_RightUp:
                direction = new Vector3(-1f, -1f, 0f);
                break;

            default:
                Debug.Assert(false);
                return;
        }

        float centerIndex = (_coins.Length - 1) * 0.5f;

        for (int i = 0; i < _coins.Length; i++)
        {
            float offset = (i - centerIndex) * COIN_SPACING;
            _coins[i].transform.localPosition = direction * offset;
            _coins[i].Init();
        }
    }

    public void Hide()
    {
        for (int i = 0; i < _coins.Length; i++)
        {
            _coins[i].Hide();
        }
    }

}
