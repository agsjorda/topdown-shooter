using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionCooldown = 0.5f;
    [SerializeField] private LayerMask interactionLayer = ~0;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionHintUI;
    [SerializeField] private UnityEngine.UI.Text interactionHintText;

    private List<Interactable> nearbyInteractables = new List<Interactable>();
    private Interactable closestInteractable;
    private Player player;
    private PlayerControls controls;
    private float lastInteractionTime;

    #region Properties
    public List<Interactable> Interactables => new List<Interactable>(nearbyInteractables);
    public Interactable ClosestInteractable => closestInteractable;
    #endregion

    private void Awake()
    {
        player = GetComponent<Player>();
        controls = player.controls;

        // Subscribe to input
        controls.Character.Interaction.performed += OnInteractionPerformed;
    }

    private void OnEnable()
    {
        if (controls != null)
            controls.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Disable();
    }

    private void OnDestroy()
    {
        if (controls != null)
            controls.Character.Interaction.performed -= OnInteractionPerformed;
    }

    private void Update()
    {
        UpdateClosestInteractable();
        UpdateInteractionHint();
    }

    #region Interactable Management
    public void RegisterInteractable(Interactable interactable)
    {
        if (interactable == null) return;

        if (!nearbyInteractables.Contains(interactable)) {
            nearbyInteractables.Add(interactable);
            UpdateClosestInteractable();
        }
    }

    public void UnregisterInteractable(Interactable interactable)
    {
        if (interactable == null) return;

        if (nearbyInteractables.Contains(interactable)) {
            nearbyInteractables.Remove(interactable);
            if (closestInteractable == interactable) {
                interactable.Highlight(false);
                closestInteractable = null;
            }
            UpdateClosestInteractable();
        }
    }

    public void UpdateClosestInteractable()
    {
        // Remove null references first
        nearbyInteractables.RemoveAll(item => item == null);

        // Unhighlight previous closest
        if (closestInteractable != null)
            closestInteractable.Highlight(false);

        closestInteractable = FindClosestValidInteractable();

        // Highlight new closest
        if (closestInteractable != null)
            closestInteractable.Highlight(true);
    }

    private Interactable FindClosestValidInteractable()
    {
        Interactable closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Interactable interactable in nearbyInteractables) {
            if (interactable == null || !interactable.CanInteract(transform))
                continue;

            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

    // Backward compatibility method
    public List<Interactable> GetInteractables() => new List<Interactable>(nearbyInteractables);
    #endregion

    #region Interaction
    private void OnInteractionPerformed(InputAction.CallbackContext context)
    {
        TryInteract();
    }

    private void TryInteract()
    {
        // Check cooldown
        if (Time.time < lastInteractionTime + interactionCooldown)
            return;

        if (closestInteractable != null) {
            closestInteractable.Interaction();
            lastInteractionTime = Time.time;

            // Update after interaction (item might be removed)
            UpdateClosestInteractable();
        }
    }

    // For backward compatibility
    private void InteractWithClosest() => TryInteract();
    #endregion

    #region UI Feedback
    private void UpdateInteractionHint()
    {
        if (interactionHintUI != null) {
            bool shouldShow = closestInteractable != null;
            interactionHintUI.SetActive(shouldShow);

            // Update hint text if available
            if (shouldShow && interactionHintText != null) {
                string interactableName = closestInteractable.gameObject.name;
                interactableName = interactableName.Replace("Pickup_", "").Replace(" - ", ": ");
                interactionHintText.text = $"Press E to interact with {interactableName}";
            }
        }
    }
    #endregion

    #region Debug & Gizmos
    private void OnDrawGizmosSelected()
    {
        // Draw lines to all nearby interactables
        Gizmos.color = Color.yellow;
        foreach (Interactable interactable in nearbyInteractables) {
            if (interactable != null)
                Gizmos.DrawLine(transform.position, interactable.transform.position);
        }

        // Highlight closest interactable
        if (closestInteractable != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, closestInteractable.transform.position);
            Gizmos.DrawWireSphere(closestInteractable.transform.position, 0.3f);
        }
    }
    #endregion
}