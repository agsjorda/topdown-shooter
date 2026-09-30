# Drag & Drop

Transactions are the only place item moves happen. Each one talks to `IInventory` and `IEquipmentSystem` — never to slot views.

`InventoryUIConfig` owns the slot views and paints equipment from `IEquipmentSystem.OnEquipmentChanged`. A transaction that equips or unequips does not call `SetItem` on an `EquipmentSlotView`.

## Flow

1. `DragDropController` captures the pointer (inventory events live on the slots container, equipment events on each `EquipmentSlotView`).
2. Past `dragThreshold`, `DragDropService.StartDrag` shows the ghost.
3. On release, `TransactionFactory` picks a transaction. `CanExecute` must pass, then `Execute` mutates the model.
4. Inventory slots refresh from `OnInventoryChanged` (`TabFilterManager`). Equipment slots refresh from `OnEquipmentChanged`.

## Transactions

| Source to target | Class | Rule |
| --- | --- | --- |
| Inventory to inventory | `InventoryToInventoryTransaction` | Move, or swap when both slots are occupied |
| Inventory to equipment | `InventoryToEquipmentTransaction` | `CanEquip`; the previously equipped item returns to the source slot |
| Equipment to inventory | `EquipmentToInventoryTransaction` | Unequip, or swap when the inventory slot's item `CanEquip`s into the source slot |
| Equipment to equipment | `EquipmentToEquipmentTransaction` | Move or swap; both directions must `CanEquip` |
| Double-click inventory | `QuickEquipTransaction` | First equipment slot `CanEquip` accepts |
| Double-click equipment | `QuickUnequipTransaction` | First empty inventory index |

`IEquipmentSystem.EquipItem` returns false and changes nothing when `CanEquip` fails. `CanEquip` reads `Item_DataSO.compatibleSlots`.

Add a new move by subclassing `DragDropTransaction` and branching in `TransactionFactory`. Do not grow `DragDropController`.
