using UnityEngine;

public class PlayerItemController : MonoBehaviour
{
    private Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    public void PickupItem(Inventory_Item newItem)
    {
        Debug.Log("Picked up item: " + newItem.itemData.name);
    }
}
