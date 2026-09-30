using UnityEngine;

/// <summary>
/// Implemented by item data that supplies its own world model for pickups.
/// Pickup_Item swaps its authored visual for this prefab when one is assigned.
/// </summary>
public interface IPickupModelSource
{
    GameObject PickupModelPrefab { get; }
    Color PickupModelTint { get; }
}
