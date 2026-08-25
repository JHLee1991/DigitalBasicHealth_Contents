using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlayerCollider : MonoBehaviour
{
    private PlayerController _ownerPC;
    private LayerMask _coinLayer;

    void Start()
    {
        _ownerPC = GetComponentInParent<PlayerController>();
        Debug.Assert(_ownerPC != null);
        _coinLayer = LayerMask.NameToLayer("Coin");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _coinLayer)
        {
            if (other.GetComponentInParent<Coin>() != null)
            {
                // TODO : 26-08-25 [Ace 이곳 점수 중복 판정 될 수 있음. 나중에 체크해야함]
                _ownerPC.AddCoinScore();
                EffectManager.Instance.PlayCoinHitParticle(other.transform.position);
            }
        }
    }
}
