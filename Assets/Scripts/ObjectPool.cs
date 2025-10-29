using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {

    public static ObjectPool instance;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> bulletPool;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        bulletPool = new Queue<GameObject>();
        CreateInitialPool();
    }

    public GameObject GetBulletFromPool()
    {

        if (bulletPool.Count == 0)
            CreateNewBullet();

        GameObject bulletToGet = bulletPool.Dequeue();
        bulletToGet.SetActive(true);
        bulletToGet.transform.parent = null; // Detach from pool parent

        return bulletToGet;
    }

    public void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
        bullet.transform.parent = transform; // Reattach to pool parent
    }

    private void CreateInitialPool()
    {
        for (int i = 0; i < poolSize; i++) {
            CreateNewBullet();

        }
    }

    private void CreateNewBullet()
    {
        GameObject newBullet = CreateGameObject(bulletPrefab, transform, false, bulletPool);
    }

    private GameObject CreateGameObject(GameObject prefab, Transform parent = null, bool setActive = false, Queue<GameObject> targetPool = null)
    {
        GameObject newObj = Instantiate(prefab, parent != null ? parent : transform);
        newObj.SetActive(setActive);
        targetPool?.Enqueue(newObj);
        return newObj;
    }
}
