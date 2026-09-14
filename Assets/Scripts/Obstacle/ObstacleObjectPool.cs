using UnityEngine;
using UnityEngine.Pool;

public class ObstacleObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private Transform _initialParentTransform;
    public const int MAX_OBSTACLE_COUNT = 8;

    private ObjectPool<Obstacle> _obstaclePool;
    public void SpawnObstacle(Transform parentTransform)
    {
        _obstaclePool.Get().Init(parentTransform);
    }
    private void Awake()
    {
        EnsurePoolNotNull();
    }
    private void EnsurePoolNotNull()
    {
        if (_obstaclePool == null)
        {
            _obstaclePool = new ObjectPool<Obstacle>(
                OnCreateTape,
                OnGetTape,
                OnReleaseToPool,
                OnDestroyTape,
                defaultCapacity: MAX_OBSTACLE_COUNT,
                maxSize: MAX_OBSTACLE_COUNT
            );
        }
    }

    #region Pool
    private Obstacle OnCreateTape()
    {
        Obstacle obstacle = Instantiate(_obstaclePrefab, _initialParentTransform, true).GetComponent<Obstacle>();
        Debug.Assert(obstacle != null);
        if (obstacle != null)
        {
            EnsurePoolNotNull();
            obstacle.SetPool(_obstaclePool);
            return obstacle;
        }
        return null;
    }

    private void OnGetTape(Obstacle obstacle)
    {
        Debug.Assert(obstacle != null);
        obstacle.gameObject.SetActive(true);
    }
    private void OnReleaseToPool(Obstacle obstacle)
    {
        obstacle.gameObject.SetActive(false);
    }

    private void OnDestroyTape(Obstacle obstacle)
    {
        Destroy(obstacle.gameObject);
    }
    #endregion
}
