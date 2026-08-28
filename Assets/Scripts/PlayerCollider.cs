using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlayerCollider : MonoBehaviour
{
    [SerializeField] private CoinCollectUIEffect _coinCollectUIEffect;

    private PlayerController _ownerPC;
    private int _coinLayer;

    private void Start()
    {
        _ownerPC = GetComponentInParent<PlayerController>();
        Debug.Assert(_ownerPC != null);
        _coinLayer = LayerMask.NameToLayer("Coin");

        if (_coinCollectUIEffect == null)
        {
            _coinCollectUIEffect =
                GetComponentInChildren<CoinCollectUIEffect>(true);
        }
    }

    // Player가 동적으로 생성되고 UI가 Scene에 있을 때 SpawnManager에서 주입한다.
    public void SetCoinCollectUIEffect(CoinCollectUIEffect effect)
    {
        _coinCollectUIEffect = effect;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != _coinLayer)
        {
            return;
        }

        Coin coin = other.GetComponentInParent<Coin>();
        if (coin == null || !coin.TryCollect())
        {
            return;
        }

        Vector3 collectWorldPosition = other.transform.position;

        _ownerPC.AddCoinScore();
        EffectManager.Instance.PlayCoinHitParticle(collectWorldPosition);
        _coinCollectUIEffect?.Play(collectWorldPosition);
    }
}
