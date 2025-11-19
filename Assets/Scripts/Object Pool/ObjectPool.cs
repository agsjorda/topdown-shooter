using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField] private int poolSize = 10;

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary =
        new Dictionary<GameObject, Queue<GameObject>>();

    [Header("To Initialize")]
    [SerializeField] private GameObject weaponPickup;
    [SerializeField] private GameObject ammoPickup;
    [SerializeField] private GameObject floatingText;

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
        InitializeNewPool(weaponPickup);
        InitializeNewPool(ammoPickup);
        InitializeNewPool(floatingText);
    }

    // ───────────────────────────────
    // GET OBJECT FROM POOL
    // ───────────────────────────────
    public GameObject GetObject(GameObject prefab, bool autoActivate = true)
    {
        if (prefab == null) {
            Debug.LogWarning("ObjectPool.GetObject called with null prefab.");
            return null;
        }

        if (!poolDictionary.ContainsKey(prefab))
            InitializeNewPool(prefab);

        if (poolDictionary[prefab].Count == 0)
            CreateNewObject(prefab);

        GameObject objectToGet = poolDictionary[prefab].Dequeue();

        objectToGet.transform.parent = null;
        ResetPooledObjectState(objectToGet);

        if (autoActivate)
            objectToGet.SetActive(true);

        Debug.Log($"ObjectPool: Dequeued '{objectToGet.name}' for prefab '{prefab.name}'. Remaining = {poolDictionary[prefab].Count}");

        return objectToGet;
    }

    // ───────────────────────────────
    // SPAWN OBJECT AT SPECIFIC POSITION
    // ───────────────────────────────
    public GameObject SpawnFromPool(GameObject prefab, Transform parent, Vector3 localPosition, Quaternion localRotation, bool keepParented = false)
    {
        GameObject obj = GetObject(prefab, false); // get inactive

        if (parent != null) {
            obj.transform.SetParent(parent);
            obj.transform.localPosition = localPosition;
            obj.transform.localRotation = localRotation;
        } else {
            obj.transform.position = localPosition;
            obj.transform.rotation = localRotation;
        }

        obj.SetActive(true);

        if (!keepParented && parent != null)
            obj.transform.SetParent(null);

        return obj;
    }

    // ───────────────────────────────
    // RETURN OBJECT TO POOL
    // ───────────────────────────────
    public void ReturnObject(GameObject objectToReturn, float delay = .001f)
    {
        StartCoroutine(DelayReturn(delay, objectToReturn));
    }

    private IEnumerator DelayReturn(float delay, GameObject objectToReturn)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(objectToReturn);
    }

    private void ReturnToPool(GameObject objectToReturn)
    {
        var pooledComp = objectToReturn.GetComponent<PooledObject>();
        if (pooledComp == null || pooledComp.originalPrefab == null) {
            Debug.LogWarning($"ObjectPool: ReturnToPool received non-pooled object '{objectToReturn.name}', destroying.");
            Destroy(objectToReturn);
            return;
        }

        GameObject originalPrefab = pooledComp.originalPrefab;

        // reset physics
        var rb = objectToReturn.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        objectToReturn.SetActive(false);
        objectToReturn.transform.parent = transform;

        if (!poolDictionary.ContainsKey(originalPrefab))
            poolDictionary[originalPrefab] = new Queue<GameObject>();

        poolDictionary[originalPrefab].Enqueue(objectToReturn);

        Debug.Log($"ObjectPool: Returned '{objectToReturn.name}' to pool for '{originalPrefab.name}'. New size = {poolDictionary[originalPrefab].Count}");
    }

    // ───────────────────────────────
    // INITIALIZATION HELPERS
    // ───────────────────────────────
    // InitializeNewPool - add a log after creating the queue
    private void InitializeNewPool(GameObject prefab)
    {
        if (prefab == null) {
            Debug.LogWarning("InitializeNewPool called with null prefab, skipping.");
            return;
        }

        if (!poolDictionary.ContainsKey(prefab))
            poolDictionary[prefab] = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++) {
            CreateNewObject(prefab);
        }

        Debug.Log($"ObjectPool: Initialized pool for '{prefab.name}' with {poolDictionary[prefab].Count} instances.");
    }

    // CreateNewObject - log the instance creation
    private void CreateNewObject(GameObject prefab)
    {
        if (prefab == null) return;

        GameObject newObject = Instantiate(prefab, transform);
        var pooled = newObject.AddComponent<PooledObject>();
        pooled.originalPrefab = prefab;
        newObject.SetActive(false);

        if (!poolDictionary.ContainsKey(prefab))
            poolDictionary[prefab] = new Queue<GameObject>();

        poolDictionary[prefab].Enqueue(newObject);

        Debug.Log($"ObjectPool: Created pooled instance '{newObject.name}' for prefab '{prefab.name}'. Queue size = {poolDictionary[prefab].Count}");
    }

    // add this helper inside the ObjectPool class
    public int GetPoolCount(GameObject prefab)
    {
        if (prefab == null) return 0;
        return poolDictionary.ContainsKey(prefab) ? poolDictionary[prefab].Count : 0;
    }

    // ───────────────────────────────
    // RESET STATE OF OBJECT BEFORE USE
    // ───────────────────────────────
    private void ResetPooledObjectState(GameObject obj)
    {
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        var pooledComp = obj.GetComponent<PooledObject>();
        if (pooledComp != null) {
            // allow pooled scripts to define their own reset behavior
            pooledComp.ResetState();
        }
    }
}
