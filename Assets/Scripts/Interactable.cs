using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Material highlightMaterial;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private bool requireLookAt = false;
    [SerializeField, Range(0f, 180f)] private float maxLookAngle = 45f;

    protected Renderer objectRenderer;
    protected Material defaultSharedMaterial;
    protected Collider interactionCollider;

    // Cached for cheaper angle checks when requireLookAt is true
    private Transform _cachedTransform;
    private Transform _lastPlayerTransform;

    #region Properties
    public float InteractionRadius => interactionRadius;
    public bool IsHighlighted { get; private set; }
    public bool CanBeInteracted { get; protected set; } = true;
    #endregion

    protected virtual void Awake()
    {
        _cachedTransform = transform;
        InitializeComponents();
        CacheDefaultMaterial();
    }

    protected virtual void InitializeComponents()
    {
        // Cache renderer once
        if (objectRenderer == null)
            objectRenderer = GetComponentInChildren<Renderer>();

        // Ensure a trigger collider exists and matches the interaction radius
        if (interactionCollider == null) {
            interactionCollider = GetComponent<Collider>();
            if (interactionCollider == null)
                interactionCollider = gameObject.AddComponent<SphereCollider>();
        }

        // Configure collider if it's a sphere
        if (interactionCollider is SphereCollider sphereCollider) {
            sphereCollider.radius = interactionRadius;
            sphereCollider.isTrigger = true;
        } else {
            // Ensure non-sphere colliders still work as triggers
            interactionCollider.isTrigger = true;
        }
    }

    protected virtual void CacheDefaultMaterial()
    {
        // Store sharedMaterial to avoid unwanted instantiation; use .material only when highlighting
        if (objectRenderer != null && defaultSharedMaterial == null)
            defaultSharedMaterial = objectRenderer.sharedMaterial;
    }

    #region Interaction Core
    public abstract void Interaction();

    public virtual bool CanInteract(Transform playerTransform)
    {
        if (!CanBeInteracted || !_cachedTransform.gameObject.activeInHierarchy)
            return false;

        // Check distance first (early out)
        // Using sqrMagnitude to avoid sqrt for performance
        Vector3 delta = _cachedTransform.position - playerTransform.position;
        if (delta.sqrMagnitude > interactionRadius * interactionRadius)
            return false;

        // Optional line-of-sight (angle) gating
        if (requireLookAt && playerTransform != null) {
            // Cache player transform to avoid repeated property access
            _lastPlayerTransform = playerTransform;

            // Use dot product instead of Angle for cheaper check
            Vector3 playerForward = playerTransform.forward;
            Vector3 directionToObject = delta.normalized;
            float cosMax = Mathf.Cos(maxLookAngle * Mathf.Deg2Rad);
            float dot = Vector3.Dot(playerForward, directionToObject);
            if (dot < cosMax)
                return false;
        }

        return true;
    }
    #endregion

    #region Highlight Management
    public void Highlight(bool enable)
    {
        if (objectRenderer == null || highlightMaterial == null || defaultSharedMaterial == null)
            return;

        if (IsHighlighted == enable)
            return;

        IsHighlighted = enable;

        // Use .material only when assigning a different material to avoid copies when not needed
        if (enable) {
            objectRenderer.material = highlightMaterial;
        } else {
            // Restore shared material without creating a new instance
            objectRenderer.sharedMaterial = defaultSharedMaterial;
        }
    }

    // For backward compatibility
    public void HighlightActive(bool active) => Highlight(active);

    protected void UpdateRenderer(Renderer newRenderer)
    {
        objectRenderer = newRenderer;
        if (objectRenderer != null && defaultSharedMaterial == null)
            defaultSharedMaterial = objectRenderer.sharedMaterial;
    }
    #endregion

    #region Trigger Events
    protected virtual void OnTriggerEnter(Collider other)
    {
        var playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction == null) return;

        playerInteraction.RegisterInteractable(this);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        var playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction == null) return;

        playerInteraction.UnregisterInteractable(this);
        Highlight(false);
    }
    #endregion

    #region Editor & Debug
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw interaction radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        // Draw approximate forward direction
        if (requireLookAt) {
            Gizmos.color = Color.cyan;
            Vector3 forward = transform.forward * interactionRadius * 0.5f;
            Gizmos.DrawLine(transform.position, transform.position + forward);
        }
    }

    protected virtual void OnValidate()
    {
        // Keep collider radius synced in editor
        if (interactionCollider is SphereCollider sphereCollider) {
            sphereCollider.radius = interactionRadius;
            sphereCollider.isTrigger = true;
        }
    }
    #endregion
}