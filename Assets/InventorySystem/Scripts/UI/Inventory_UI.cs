using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Thin wrapper around the inventory panel element (show/hide/toggle via UIBaseComponent).
    /// Tab building, active-tab visuals, and filtering are owned by TabFilterManager.
    /// </summary>
    public class Inventory_UI : UIBaseComponent
    {
        public Inventory_UI(VisualElement root) : base(root)
        {
        }

        public override void Show() => base.Show();
        public override void Hide() => base.Hide();
    }
}
