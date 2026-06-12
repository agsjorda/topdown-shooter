# Inventory System — Deep Dive

> Snapshot of the system as found in `Assets/Scripts/InventorySystem/` (June 2026), before the modularization described in [inventory-modularization-plan.md](inventory-modularization-plan.md). File:line references point at the pre-move locations.

## Architecture: MVVM + Service Locator + Transaction drag/drop

```
Item_DataSO (SO data) ──wrapped by──> InventoryItem ──stored in──> InventoryModel (MonoBehaviour, IInventory)
                                                                        │  OnInventoryChanged
                                                   InventoryViewModel (plain C# validating facade)
                                                                        │
        UI Toolkit views: SlotView / EquipmentSlotView / tabs  <── TabFilterManager / InventoryUIConfig
                                                                        │
                    DragDropController (Mono) → DragDropService → TransactionFactory → 6 transactions
```

Game code reaches the system only through the static `InventoryService` locator.

## Core (`Core/`)

- **IInventory** — `AddItem`, `RemoveItemAt`, `SwapItems`, `MoveItem`, `GetItemAt`, `SetItemAt`, `CanAddItem`, `IsValidSlotIndex`.
- **InventoryModel** (MonoBehaviour, on the Player GO in SampleScene) — `List<InventoryItem>` where **null = empty slot**; `maxInventorySize = 24`; coarse `OnInventoryChanged` event; `SmartMoveItem` swap-or-move.
- **InventoryViewModel** (plain C#) — wraps the model, validates indices before delegating. Stateless.
- **IEquipmentSystem / EquipmentController** — `EquipItem/UnequipSlot/GetEquippedItem` keyed by `EquipmentSlotType`; `List<EquippedItem>` pairs.
- **InventoryService** (static, `namespace InventorySystem`) — registry with `FindFirstObjectByType` fallback; `RegisterPlayerInventory/Equipment`, `Clear()`, `GetPlayerInventoryViewModel()`.
- **DragDropController** (MonoBehaviour, 431 lines) — wires UIDocument pointer events per slot, owns `DragDropService`/`DoubleClickHandler`/`DragVisualHandler`, executes transactions as coroutines, initializes the 4 equipment slots by hard-coded UXML names.
- **WeaponSlot** — legacy serializable holding the game type `Weapon`; used by `PlayerWeaponController`'s 4-slot array, not by the inventory itself.

## Data (`Data/`)

- **Item_DataSO** (base SO): auto-GUID `itemId`, `itemName`, `icon`, `description`, `itemType` enum, `stackable`, `maxStack` (stacking is **not implemented** in the model).
- **Weapon_Data** : Item_DataSO + shooter stats (magazine, fire rate, burst, spread, `WeaponType`, `ShootType`, reload/equip speed, camera distance).
- **Armor_Data** : Item_DataSO + `ArmorType`, damage reduction, bonus health, durability, model prefab, tint.
- **Enemy_MeleeWeaponData** — enemy-only, not part of the inventory.

## Drag/drop (`DragDrop/`)

- **DragState** — current drag (source inventory/equipment slot, start position).
- **DragDropService** — drag lifecycle, drop-target detection by bounds, events (`OnDragStarted/Updated/Ended`, `OnTransactionReady`, `OnDebugLog`).
- **DragVisualHandler** — ghost icon + USS state classes (`drag-ghost--*`, `inventorySlots--*`).
- **DoubleClickHandler** — 0.3 s double-click window (uses `Time.unscaledTime`).
- **TransactionFactory** (static) routes source→target to one of six **DragDropTransaction** strategies (`CanExecute` / `Execute` coroutine / `GetTargetVisual`): InventoryToInventory, InventoryToEquipment, EquipmentToInventory, EquipmentToEquipment, QuickEquip, QuickUnequip.

## UI (`UI/`, all UI Toolkit)

- **UIManager** (MonoBehaviour) — initializes `GameHUD_UI` + `Inventory_UI` from one UIDocument; **couples to `Player`/`PlayerControls`** for the toggle inputs; manages minimap/preview cameras; `IsInventoryOpen()` consumed by `PlayerWeaponController` to block firing.
- **UIBaseComponent** — Show/Hide/Toggle base for panel wrappers (`Inventory_UI`, game-side `GameHUD_UI`/`HealthBar_UI`).
- **SlotView** (`[UxmlElement]`) — slot icon display, `SetItem/ClearItem`, virtual `CanAcceptItem` (accepts all).
- **EquipmentSlotView** : SlotView — typed slot; `CanAcceptItem` hard-codes Weapon/Armor matching via enum switch + `is Armor_Data` cast.
- **InventoryUIConfig** (MonoBehaviour) — slot factory (size/count/margin), scroll wrapper, tab descriptors; rebuilds slots on change; auto-creates a ViewModel.
- **TabFilterManager** (MonoBehaviour) — builds `InventoryTabElement`s, filters by hard-coded `ItemType`→tab-id mapping (`"all"/"weapon"/"armor"/...`).
- **InventoryScrollElement / InventoryTabElement / InventoryContainerComponent** — supporting `[UxmlElement]`s; the container loads `Assets/UI/uxml/InventoryContainer.uxml` by hard-coded path.

The shooter's panel markup lives in `Assets/UI/GameUi.uxml` (element `inventory-panel`, 4 `<EquipmentSlotView>` tags named `equipSlotWeapon/Headgear/Armor/Boots`, `tabButtonsContainer`, `tabContentContainer`), styled by `Assets/UI/uss/GameUI.uss`.

## Coupling audit

**Inventory → game (the only violations):**

- `UIManager` → `Player`, `PlayerControls` (FindFirstObjectByType + input subscription in `Update()` — re-subscribes every frame, a latent bug).
- Shared enums in `Assets/Scripts/Enum/ItemEnums.cs`: inventory-generic `ItemType`/`EquipmentSlotType`/`ArmorType` mixed with game-only `WeaponType`/`ShootType`/`AmmoBoxType`/`WeaponModelType`/`EquipType`/`HoldType`/`HangType`.
- `WeaponSlot` holds game type `Weapon` (belongs to the game, not the module).

**Game → inventory (the API surface):** `PlayerWeaponController` (overflow `SendToInventory`), `Pickup_Item`/`Pickup_Armor` (`InventoryService` + `InventoryItem`), `Pickup_Weapon` (`Weapon_Data` field), `GameHUD_UI`/`HealthBar_UI` (extend `UIBaseComponent`), `PlayerWeaponController` → `UIManager.IsInventoryOpen()`.

## Known defects & refactor targets (verified)

1. `InventoryModel.CanAddItem()` counts null (empty) slots as occupied → false "inventory full".
2. `InventoryModel.GetItemAt()` grows the list — getter with side effects.
3. `AddItem`/`RemoveItemAt` return void — pickups can consume items even when the add failed.
4. `InventoryService` statics survive disabled domain reload → stale references.
5. Multiple `InventoryViewModel`s wrap one model (`GetPlayerInventoryViewModel()` news one per call; `InventoryUIConfig` builds its own).
6. Per-slot pointer callbacks are orphaned when `InventoryUIConfig.RebuildUI()` recreates all slots — which `DragDropController.ExecuteTransaction` triggers after every transaction.
7. `InventoryUIConfig.Update()` polls 7 inspector fields every frame.
8. `IEquipmentSystem` has no change events; `EquipmentController.AddItemToSlot` is an admitted placeholder.
9. Stacking (`stackable`/`maxStack`) is declared in data but never implemented.
10. Magic strings throughout (USS classes, element names); `InventoryTabElement` keeps static mutable UI state; double-click window hard-coded.

All of these are scheduled in the modularization plan (Phase B tiers).
