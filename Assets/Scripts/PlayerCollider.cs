using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlayerCollider : MonoBehaviour
{
    private LayerMask _coinLayer;

    void Start()
    {
        _coinLayer = LayerMask.NameToLayer("Coin");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _coinLayer)
        {
            EffectManager.Instance.PlayHitParticle(other.transform.position);
            Destroy(other.gameObject);
        }
    }
}
