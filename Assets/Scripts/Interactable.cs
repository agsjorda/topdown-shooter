using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Material highlightMaterial;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private bool requireLookAt = false;
    [SerializeField] private float maxLookAngle = 45f;

    protected Renderer objectRenderer;
    protected Material defaultMaterial;
    protected Collider interactionCollider;

    #region Properties
    public float InteractionRadius => interactionRadius;
    public bool IsHighlighted { get; private set; }
    public bool CanBeInteracted { get; protected set; } = true;
    #endregion

    protected virtual void Awake()
    {
        InitializeComponents();
        CacheDefaultMaterial();
    }

    protected virtual void InitializeComponents()
    {
        // Try to get renderer
        if (objectRenderer == null)
            objectRenderer = GetComponentInChildren<Renderer>();

        // Try to get collider - add one if missing
        if (interactionCollider == null) {
            interactionCollider = GetComponent<Collider>();
            if (interactionCollider == null)
                interactionCollider = gameObject.AddComponent<SphereCollider>();
        }

        // Configure collider if it's a sphere
        if (interactionCollider is SphereCollider sphereCollider) {
            sphereCollider.radius = interactionRadius;
            sphereCollider.isTrigger = true;
        }
    }

    protected virtual void CacheDefaultMaterial()
    {
        if (objectRenderer != null && defaultMaterial == null)
            defaultMaterial = objectRenderer.sharedMaterial;
    }

    #region Interaction Core
    public abstract void Interaction();

    public virtual bool CanInteract(Transform playerTransform)
    {
        if (!CanBeInteracted || !gameObject.activeSelf)
            return false;

        // Check distance
        float distance = Vector3.Distance(playerTransform.position, transform.position);
        if (distance > interactionRadius)
            return false;

        // Check if player is looking at the object (optional)
        if (requireLookAt && playerTransform != null) {
            Vector3 directionToObject = (transform.position - playerTransform.position).normalized;
            Vector3 playerForward = playerTransform.forward;

            float angle = Vector3.Angle(playerForward, directionToObject);
            if (angle > maxLookAngle)
                return false;
        }

        return true;
    }
    #endregion

    #region Highlight Management
    public void Highlight(bool enable)
    {
        if (objectRenderer == null || highlightMaterial == null || defaultMaterial == null)
            return;

        IsHighlighted = enable;

        // Apply or remove highlight material
        if (enable)
            objectRenderer.material = highlightMaterial;
        else
            objectRenderer.material = defaultMaterial;
    }

    // For backward compatibility
    public void HighlightActive(bool active) => Highlight(active);

    protected void UpdateRenderer(Renderer newRenderer)
    {
        objectRenderer = newRenderer;
        if (objectRenderer != null && defaultMaterial == null)
            defaultMaterial = objectRenderer.sharedMaterial;
    }
    #endregion

    #region Trigger Events
    protected virtual void OnTriggerEnter(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction == null) return;

        playerInteraction.RegisterInteractable(this);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
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

        // Draw line of sight if required
        if (requireLookAt) {
            Gizmos.color = Color.cyan;
            Vector3 forward = transform.forward * interactionRadius * 0.5f;
            Gizmos.DrawLine(transform.position, transform.position + forward);
        }
    }

    protected virtual void OnValidate()
    {
        // Update sphere collider radius in editor
        if (interactionCollider is SphereCollider sphereCollider)
            sphereCollider.radius = interactionRadius;
    }
    #endregion
}