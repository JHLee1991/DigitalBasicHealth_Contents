using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [SerializeField] private GameObject _effectParent;
    private HitEffectController[] _effects;
    public void PlayHitParticle(Vector3 pos)
    {
        for (int i = 0; i < _effects.Length; ++i)
        {
            if (!_effects[i].IsPlayingParticle())
            {
                _effects[i].PlayParticleAt(new Vector3(pos.x, pos.y, pos.z));
                return;
            }
        }
        Debug.Assert(false, "Must Append Hit Particle!!");
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Debug.Assert(_effectParent != null);
        int effectCount = _effectParent.transform.childCount;
        _effects = new HitEffectController[effectCount];
        for (int i = 0; i < effectCount; ++i)
        {
            _effects[i] = _effectParent.transform.GetChild(i).GetComponent<HitEffectController>();
            Debug.Assert(_effects[i] != null);
        }
    }
}
