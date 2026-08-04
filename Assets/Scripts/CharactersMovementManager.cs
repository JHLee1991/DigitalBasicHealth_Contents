using UnityEngine;

public class CharactersMovementManager : MonoBehaviour
{
    [SerializeField] private GameObject _charactersParent;
    [SerializeField] private float _movementSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 oriPos = _charactersParent.transform.position;
        _charactersParent.transform.position = new Vector3(oriPos.x, oriPos.y, oriPos.z + _movementSpeed * Time.deltaTime);
    }


}
