using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject originalPrefab;

    // Override in custom pooled objects if they need cleanup
    public virtual void ResetState() { }
}
