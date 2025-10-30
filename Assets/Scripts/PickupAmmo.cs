using UnityEngine;

public class PickupAmmo : Interactable
{
    public override void Interaction()
    {
        base.Interaction();
        Debug.Log("Picked up ammo from " + gameObject.name);
        Destroy(gameObject);
    }
}
