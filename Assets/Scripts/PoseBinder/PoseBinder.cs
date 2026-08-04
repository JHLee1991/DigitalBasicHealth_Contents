using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class PoseBinder : MonoBehaviour
{
    private SkinnedMeshRenderer _smr;

    void Awake()
    {
        _smr = GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
