using UnityEngine;

namespace InventorySystem
{
    [System.Serializable]
    public class InventoryItem
    {
        public Item_DataSO itemData;

        [Min(1)] public int quantity = 1;

        public InventoryItem(Item_DataSO data, int quantity = 1)
        {
            this.itemData = data;
            this.quantity = Mathf.Max(1, quantity);
        }
    }
}
