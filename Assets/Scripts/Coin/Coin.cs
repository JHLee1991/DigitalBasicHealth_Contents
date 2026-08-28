using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Coin : MonoBehaviour
{
    [SerializeField] private GameObject _coinBody;
    [SerializeField] private ParticleSystem _shinyPaticle;

    private bool _isCollidedWithPlayer;

    public void Init()
    {
        SetActiveObjects(true);
    }

    public void Hide()
    {
        SetActiveObjects(false);
    }

    public bool TryCollect()
    {
        if (_isCollidedWithPlayer)
        {
            return false;
        }

        SetActiveObjects(false);
        return true;
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
