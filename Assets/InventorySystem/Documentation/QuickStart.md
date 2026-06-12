# InventorySystem — Quick Start

A self-contained, plug-and-play inventory + equipment + drag/drop system for Unity 6 (UI Toolkit only — no uGUI). Structured like an Asset Store package: copy the `Assets/InventorySystem/` folder (or import the `.unitypackage`) into any project. The `Demo/` folder is fully deletable; so is `Scripts/Extras/` if your game has its own pickups.

## Requirements

- Unity 6000.x. No package dependencies for the core (`InventorySystem` assembly references only engine modules).
- The demo additionally uses the **Input System** package (`InventorySystem.Demos` assembly).

## Try the demo

Open `InventorySystem/Demo/InventoryDemo.unity` and press Play:

- **WASD** moves the capsule; walk over the orbs to pick items up (Extras `ItemPickup`)
- **I** toggles the inventory panel
- **1/2/3** grant sample items from code (`DemoItemGiver`)
- Drag between slots, drag onto equipment slots, **double-click** to quick-equip/unequip
- Potions stack (`stackable` + `maxStack` on the item asset); the count renders in the slot corner

## Drop into your game

### 1. Author your data (no code)

- **Item categories** — `Create > Inventory System > Item Category`. The `id` doubles as the tab id (`"all"` is reserved for the show-everything tab); `displayName`/`tabIcon`/`tabIconTint` feed the tab button.
- **Equipment slot types** — `Create > Inventory System > Equipment Slot Type` (e.g. weapon, headgear, ring…). `ussClassSuffix` adds `equipment-slot--{suffix}` for styling.
- **Items** — `Create > Inventory System > Item Data` (or subclass `Item_DataSO` for game stats, like the shooter's `Weapon_Data`). Each item declares its `category` and the `compatibleSlots` it can be equipped into — **the item is the single source of truth for slot fit**. Empty `compatibleSlots` = not equippable.

### 2. Build the UI (UI Toolkit)

One `UIDocument` whose tree contains, by element name (names configurable on the components):

```text
inventory-panel                  ← panel root (InventoryUIController shows/hides this)
├── <InventorySystem.EquipmentSlotView name="equipSlot..."/>   ← any number, any names
├── tabButtonsContainer          ← tab buttons are generated here
└── tabContentContainer          ← slot grid + scroll wrapper are generated here
```

`Demo/UI/InventoryPanel.uxml` is a working reference. **Functional** styles only (drag states, ghost, stack-count label) ship in `Resources/InventorySystem/InventoryCore.uss` and are auto-applied — the module deliberately does NOT style the look of slots, equipment slots, or tabs, so it can never fight your game's art. Style these classes in your own stylesheet (the demo's `InventoryPanel.uss` shows a complete example): `inventorySlots`, `equipment-slot`, `equipment-slot--{suffix}`, `has-item`, `inventoryTab--active`, `inventory-slots-container` (padding/spacing/background of the grid); and optionally override the functional ones: `inventorySlots--dragging/--empty-highlight/--drop-target`, `inventorySlots-qty`, `drag-ghost(--visible/--over-empty/--over-occupied)`, and the grid's `flex-direction: row; flex-wrap: wrap` default on `inventory-slots-container`.

### 3. Add the components (one GameObject is fine)

| Component | Role |
| --- | --- |
| `InventoryModel` | The data: slots, add/remove/move/swap, stacking, change events |
| `EquipmentController` | Equipped items per slot type; fires `OnEquipmentChanged` |
| `InventoryUIConfig` | Slot grid layout, tab list (`ItemCategorySO`s), **equipmentSlotBindings** (element name → slot type asset) |
| `TabFilterManager` | Tab building + category filtering (assign its `uiConfig`) |
| `DragDropController` | Drag/drop + double-click transactions (assign `uiDocument`, `equipmentController`) |
| `InventoryUIController` | Panel open/close API + `OnInventoryToggled` event (assign `document`) |

Bind your own input: call `InventoryUIController.Toggle()` (see the shooter's `UIManager` or the demo's `DemoInventoryInput`).

### 4. Talk to it from game code

```csharp
using InventorySystem;

// add items (returns false when full — don't consume the world object then)
bool added = InventoryService.GetPlayerInventory().AddItem(new InventoryItem(itemData, quantity));

// react to changes
InventoryService.GetPlayerEquipment().OnEquipmentChanged += (slotType, item) => { /* show armor model, apply stats */ };
model.OnInventoryChanged += RefreshUI;        // coarse
model.OnSlotChanged += i => RefreshSlot(i);   // per-slot

// optional world pickups without writing code
// add InventorySystem.Extras.ItemPickup + a trigger collider to any object
```

`InventoryService` finds scene instances automatically, or call `RegisterPlayerInventory()/RegisterPlayerEquipment()` for explicit DI (e.g. multiple inventories).

## Export / import

- Export: select `Assets/InventorySystem`, right-click → **Export Package…** (no dependencies needed — the module is self-contained).
- Or copy the folder. Either way the `InventorySystem` and `InventorySystem.Demos` asmdefs come along, and `autoReferenced` means your game scripts can use it without their own asmdefs.

## Layout

```text
Assets/InventorySystem/
├── Scripts/            core runtime (asmdef: InventorySystem — compiler-enforced: no game references)
│   ├── Core/ Data/ DragDrop/ UI/ Utilities/
│   └── Extras/         optional ItemPickup (deletable)
├── Resources/InventorySystem/   InventoryContainer.uxml, InventoryCore.uss
├── Demo/               playable demo, own asmdef + namespace (deletable)
└── Documentation/      this file
```
