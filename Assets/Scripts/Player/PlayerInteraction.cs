using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public List<Interactable> interactables;

    private Interactable closestInteractable;


    private void Start()
    {
        Player player = GetComponent<Player>();

        player.controls.Character.Interaction.performed += ctx => InteractWithClosest();
    }

    private void InteractWithClosest()
    {
        closestInteractable?.Interaction();
    }

    public void UpdateClosestInteractable()
    {
        closestInteractable?.HighlightActive(false);

        closestInteractable = null;

        float closestDistance = float.MaxValue;

        foreach (Interactable interactable in interactables) {

            float distance = Vector3.Distance(transform.position, interactable.transform.position);

            if (distance < closestDistance) {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

        closestInteractable?.HighlightActive(true);
    }
}
