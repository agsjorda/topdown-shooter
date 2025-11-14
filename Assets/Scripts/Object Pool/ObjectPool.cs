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
    }

    // ───────────────────────────────
    // GET OBJECT FROM POOL
    // ───────────────────────────────
    public GameObject GetObject(GameObject prefab, bool autoActivate = true)
    {
        if (!poolDictionary.ContainsKey(prefab))
            InitializeNewPool(prefab);

        if (poolDictionary[prefab].Count == 0)
            CreateNewObject(prefab);

        GameObject objectToGet = poolDictionary[prefab].Dequeue();

        objectToGet.transform.parent = null;
        ResetPooledObjectState(objectToGet);

        if (autoActivate)
            objectToGet.SetActive(true);

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
        GameObject originalPrefab = objectToReturn.GetComponent<PooledObject>().originalPrefab;

        // reset physics
        var rb = objectToReturn.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false; // ensure it's ready for reuse
        }

        objectToReturn.SetActive(false);
        objectToReturn.transform.parent = transform;
        poolDictionary[originalPrefab].Enqueue(objectToReturn);
    }

    // ───────────────────────────────
    // INITIALIZATION HELPERS
    // ───────────────────────────────
    private void InitializeNewPool(GameObject prefab)
    {
        poolDictionary[prefab] = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++) {
            CreateNewObject(prefab);
        }
    }

    private void CreateNewObject(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab, transform);
        newObject.AddComponent<PooledObject>().originalPrefab = prefab;
        newObject.SetActive(false);
        poolDictionary[prefab].Enqueue(newObject);
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
