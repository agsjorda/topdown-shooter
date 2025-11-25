using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public int quantity = 1;
    public int maxStackSize = 1;
}

public enum ItemType
{
    Weapon,
    Armor,
    Miscellaneous
}

[System.Serializable]
public class InventoryCategory
{
    public ItemType categoryType;
    public string displayName;
    public Sprite tabIcon;
    public int slotCount = 10;
}