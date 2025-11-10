using UnityEngine;

public class Enemy_Ragdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollParent;

    private Collider[] ragdollColliders;
    private Rigidbody[] ragdollRigidbodies;

    private void Awake()
    {
        ragdollColliders = ragdollParent.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = ragdollParent.GetComponentsInChildren<Rigidbody>();

        RagdollActive(false);
    }

    public void RagdollActive(bool active)
    {
        foreach (Rigidbody rb in ragdollRigidbodies) {
            rb.isKinematic = !active;
        }
    }

    public void CollidersActive(bool active)
    {
        foreach (Collider col in ragdollColliders) {
            col.enabled = active;
        }
    }
}
