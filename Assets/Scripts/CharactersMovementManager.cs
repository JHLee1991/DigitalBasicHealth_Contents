using UnityEngine;

public class CharactersMovementManager : MonoBehaviour
{
    [SerializeField] private GameObject _charactersParent;
    [SerializeField] private float _movementSpeed = 5f;

    void Update()
    {
        Vector3 oriPos = _charactersParent.transform.position;
        _charactersParent.transform.position = new Vector3(oriPos.x, oriPos.y, oriPos.z + _movementSpeed * Time.deltaTime);
    }
}
