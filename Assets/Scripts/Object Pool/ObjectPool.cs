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
    [SerializeField] private GameObject armorPickup;
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
        Debug.Log("[ObjectPool] Initializing pools...");

        if (weaponPickup != null)
            InitializeNewPool(weaponPickup);
        else
            Debug.LogError("[ObjectPool] weaponPickup prefab is NULL!");

        if (armorPickup != null)
            InitializeNewPool(armorPickup);
        else
            Debug.LogError("[ObjectPool] armorPickup prefab is NULL!");

        if (ammoPickup != null)
            InitializeNewPool(ammoPickup);
        else
            Debug.LogError("[ObjectPool] ammoPickup prefab is NULL!");

        if (floatingText != null)
            InitializeNewPool(floatingText);
        else
            Debug.LogError("[ObjectPool] floatingText prefab is NULL!");

        Debug.Log("[ObjectPool] Pool initialization complete.");
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

        // FIX: Use SetParent instead of .parent
        objectToGet.transform.SetParent(null, true); // worldPositionStays = true when unparenting
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
            // FIX: Use SetParent with worldPositionStays = false for UI elements
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localRotation = localRotation;
        } else {
            obj.transform.position = localPosition;
            obj.transform.rotation = localRotation;
        }

        obj.SetActive(true);

        if (!keepParented && parent != null)
            // FIX: Use SetParent with worldPositionStays = true when unparenting
            obj.transform.SetParent(null, true);

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
        // FIX: Use SetParent with worldPositionStays = false for UI elements
        objectToReturn.transform.SetParent(transform, false);

        if (!poolDictionary.ContainsKey(originalPrefab))
            poolDictionary[originalPrefab] = new Queue<GameObject>();

        poolDictionary[originalPrefab].Enqueue(objectToReturn);

        Debug.Log($"ObjectPool: Returned '{objectToReturn.name}' to pool for '{originalPrefab.name}'. New size = {poolDictionary[originalPrefab].Count}");
    }

    // ───────────────────────────────
    // INITIALIZATION HELPERS
    // ───────────────────────────────
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

    private void CreateNewObject(GameObject prefab)
    {
        if (prefab == null) return;

        Debug.Log($"[ObjectPool] Creating new object for prefab: {prefab.name}");

        GameObject newObject = null;

        try {
            newObject = Instantiate(prefab, transform);
        } catch (System.Exception e) {
            Debug.LogError($"[ObjectPool] Failed to instantiate prefab '{prefab.name}': {e.Message}");
            return;
        }

        // Check for missing scripts
        var components = newObject.GetComponents<Component>();
        bool hasMissingScript = false;
        foreach (var comp in components) {
            if (comp == null) {
                hasMissingScript = true;
                Debug.LogError($"[ObjectPool] ⚠️ Missing script detected on prefab '{prefab.name}'! Check the prefab in the Project window and remove any missing script references.");
            }
        }

        // If there are missing scripts, destroy the instance and skip pooling
        if (hasMissingScript) {
            Debug.LogWarning($"[ObjectPool] Skipping pool creation for '{prefab.name}' due to missing scripts. Fix the prefab first!");
            if (newObject != null) Destroy(newObject);
            return;
        }

        var pooled = newObject.AddComponent<PooledObject>();
        pooled.originalPrefab = prefab;
        newObject.SetActive(false);

        if (!poolDictionary.ContainsKey(prefab))
            poolDictionary[prefab] = new Queue<GameObject>();

        poolDictionary[prefab].Enqueue(newObject);

        //Debug.Log($"ObjectPool: Created pooled instance '{newObject.name}' for prefab '{prefab.name}'. Queue size = {poolDictionary[prefab].Count}");
    }

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