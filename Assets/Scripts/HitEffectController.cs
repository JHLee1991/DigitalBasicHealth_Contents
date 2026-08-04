using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class HitEffectController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;

    public bool IsPlayingParticle()
    {
        //EnsureComponent();
        return _particle.isPlaying;
    }
    public void PlayParticleAt(Vector3 pos)
    {
        transform.position = pos;
        //EnsureComponent();

        if (_particle.isPlaying)
        {
            _particle.Stop();
        }
        _particle.Play();
    }

    private void EnsureComponent()
    {
        if (_particle == null)
        {
            _particle = GetComponent<ParticleSystem>();
        }
    }
}
