using UnityEngine;

namespace InventorySystem
{
    /// <summary>
    /// Defines an item category as an authorable asset. Categories drive tab filtering:
    /// the id doubles as the tab id, and the icon/label/tint feed the tab button.
    /// Games add their own categories without touching inventory code.
    /// </summary>
    [CreateAssetMenu(fileName = "Category_", menuName = "Inventory System/Item Category")]
    public class ItemCategorySO : ScriptableObject
    {
        [Tooltip("Stable identifier, e.g. \"weapon\". Doubles as the tab id. The id \"all\" is reserved for the show-everything tab.")]
        public string id;

        [Tooltip("Label shown on the tab button")]
        public string displayName;

        [Header("Tab Button")]
        public Texture2D tabIcon;
        public Color tabIconTint = Color.white;
    }
}
