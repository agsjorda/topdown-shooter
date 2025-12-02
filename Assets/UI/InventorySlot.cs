public class InventorySlot : BaseSlot
{
    public InventorySlot()
    {
    }

    public override void ClearItem()
    {
        base.ClearItem();
    }

    public override void SetCellMargin(float margin)
    {
        base.SetCellMargin(margin);
    }

    public override void SetItem(Inventory_Item item, int qty = 1)
    {
        base.SetItem(item, qty);
    }

    public override void SetSlotSize(int size)
    {
        base.SetSlotSize(size);
    }
}
