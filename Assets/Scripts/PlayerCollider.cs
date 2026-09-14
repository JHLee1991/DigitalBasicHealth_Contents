using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlayerCollider : MonoBehaviour
{
    private PlayerController _ownerPC;
    private CoinCollectUIEffect _coinCollectUIEffect;
    private int _coinLayer;

    private void Start()
    {
        _ownerPC = GetComponentInParent<PlayerController>();
        Debug.Assert(_ownerPC != null);
        _coinLayer = LayerMask.NameToLayer("Coin");
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

        Debug.Assert(_coinCollectUIEffect != null);
        if (_coinCollectUIEffect != null)
        {
            _coinCollectUIEffect.Play(collectWorldPosition);
        }
    }
}
