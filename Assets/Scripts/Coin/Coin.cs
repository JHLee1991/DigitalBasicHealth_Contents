using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Coin : MonoBehaviour
{
    [SerializeField] private GameObject _coinBody;
    [SerializeField] private ParticleSystem _shinyPaticle;

    private LayerMask _playerLayer;
    private bool _isCollidedWithPlayer;

    public void Init()
    {
        SetActiveObjects(true);
    }

    public void Hide()
    {
        SetActiveObjects(false);
    }

    private void Start()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isCollidedWithPlayer)
        {
            return;
        }
        if (other.gameObject.layer == _playerLayer)
        {
            SetActiveObjects(false);
        }
    }

    private void SetActiveObjects(bool isActive)
    {
        _coinBody.SetActive(isActive);
        if (isActive)
        {
            _isCollidedWithPlayer = false;
            _shinyPaticle.Play();
        }
        else
        {
            _isCollidedWithPlayer = true;
            _shinyPaticle.Stop();
        }
    }
}
