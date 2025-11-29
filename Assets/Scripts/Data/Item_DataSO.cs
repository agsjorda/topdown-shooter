using UnityEngine;


[CreateAssetMenu(fileName = "Item Data", menuName = "Item data/Item")]
public class Item_DataSO : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
    public ItemType itemType;
}
