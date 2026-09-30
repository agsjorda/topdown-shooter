# InventorySystem — How to Use

How to drop this module into a Unity 6 game and drive it from your own code. For the short checklist, see [QuickStart.md](QuickStart.md). For drag-and-drop internals, see [../Scripts/DragDrop/README.md](../Scripts/DragDrop/README.md).

The module is UI Toolkit only. It does not use uGUI, and it does not know about your player, camera, or input system. You author data as assets, build a panel in UXML, put six components on one GameObject, and call a small API.

## Requirements

- Unity 6000.x.
- UI Toolkit (`UIDocument`).
- The core assembly (`InventorySystem`) references engine modules only. It does not need the Input System.
- The demo scene uses the Input System. Delete `Demo/` if you do not want that dependency.

## Install

Copy `Assets/InventorySystem/` into the project, or import the `.unitypackage`. `autoReferenced` is on, so game scripts can `using InventorySystem;` without their own assembly definition.

You can delete:

- `Demo/` — sample scene, sample items, sample look.
- `Scripts/Extras/` — the generic `ItemPickup`. Delete it when the game already has pickups.

Do not delete `Resources/InventorySystem/`. The slot grid loads `InventoryCore.uss` from that path at runtime.

## What you do not attach to the player

Nothing from this module goes on the player prefab. The inventory is a scene object next to the UI. If it lived on the player, a respawn would destroy the inventory and leave the UI pointing at a dead object.

Your player script only calls into the inventory: toggle the panel, add a picked-up item, react when something is equipped.

## 1. Author data

All of these are created from `Create > Inventory System` in the Project window. There are no item or slot enums to edit.

### Item Category

One asset per inventory tab.

| Field | Use |
| --- | --- |
| `id` | Stable id. This is also the tab id. `"all"` is reserved for the show-everything tab. |
| `displayName` | Label on the tab button. |
| `tabIcon` | Texture on the tab button. |
| `tabIconTint` | Tint applied to that texture. |

Create one category with `id` set to `all` if you want a tab that shows every item. Other ids (`weapon`, `consumable`, `quest`) are yours.

### Equipment Slot Type

One asset per equipment slot the game has: weapon, head, ring, and so on.

| Field | Use |
| --- | --- |
| `id` | Stable id, for example `weapon`. |
| `displayName` | Name for tooling and logs. |
| `ussClassSuffix` | Adds the USS class `equipment-slot--{suffix}` on that slot. Falls back to `id` when empty. |

### Item Data

One asset per item, or a subclass when the item needs game stats (the shooter uses `Weapon_Data` and `Armor_Data`, both derived from `Item_DataSO`).

| Field | Use |
| --- | --- |
| `itemName` | Display name. |
| `icon` | Sprite shown in the slot and on the drag ghost. |
| `description` | Text for your own tooltip. The module does not render it. |
| `category` | Which tab this item appears on. |
| `compatibleSlots` | Equipment slot types this item may be equipped into. |
| `stackable` | When on, incoming copies merge into an existing stack of the same item. |
| `maxStack` | Cap for that stack. |

`itemId` is a hidden GUID generated for you. Stacking compares this id. Do not assign it by hand.

**Slot fit is declared on the item.** A slot does not list what it accepts. If `compatibleSlots` contains the Hand slot type, the item can go in a Hand slot. If the list is empty, the item cannot be equipped.

`stackable` defaults to on. Turn it off for weapons, armor, and anything that should occupy its own slot.

Stacking merges the whole incoming quantity or none of it. If a stack of 5 exists, `maxStack` is 10, and you add 6, the 6 does not split into the existing stack. It takes a new slot. If no slot is free, `AddItem` returns false.

### Inventory Theme (optional)

`Create > Inventory System > Inventory Theme`. Assign sprites in the Inspector, then drop the asset on `InventoryUIConfig.theme`.

| Field | Painted onto |
| --- | --- |
| `panelBackground` | The element named `inventory-panel` |
| `slotsContainerBackground` | The generated slot grid |
| `slotBackground` | Each empty inventory slot |
| `tabBackground` | Each tab, normal state |
| `tabActiveBackground` | Each tab, active state |
| `equipmentSlotBackgrounds` | The empty-state layer of the matching equipment slot type |
| `themeStyleSheet` | Extra USS added to the document root while this theme is active |

Each entry is a sprite, a tint, and a size mode (`Contain` by default). Use a Sprite so 9-slice borders from the texture import settings are kept.

Leave a sprite empty to keep whatever your stylesheet draws. An assigned sprite wins over the stylesheet for that one element. Swap themes at runtime with `InventoryUIConfig.SetTheme(theme)`.

## 2. Build the UI

Create a `UIDocument` and a UXML tree with these names. Equipment slot names are yours; the other three names are the defaults (the panel name is set on `InventoryUIController`).

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="project://database/Assets/YourGame/UI/InventoryPanel.uss" />
    <ui:VisualElement name="inventory-panel" class="inventory-panel">
        <InventorySystem.EquipmentSlotView name="equipSlotWeapon" class="equipment-slot-frame" />
        <InventorySystem.EquipmentSlotView name="equipSlotHead" class="equipment-slot-frame" />
        <ui:VisualElement name="tabButtonsContainer" class="tabs-row" />
        <ui:VisualElement name="tabContentContainer" class="slots-area" />
    </ui:VisualElement>
</ui:UXML>
```

Tabs and the slot grid are generated at runtime into `tabButtonsContainer` and `tabContentContainer`. Do not hand-place inventory slots. Do place every equipment slot yourself, anywhere in the panel, and give each a unique `name`.

`Demo/UI/InventoryPanel.uxml` is a working copy of this shape. The shooter's inventory lives inside `Assets/UI/GameUi.uxml` on the same document as the HUD; that is fine. One `UIDocument` can hold both.

`InventoryContainerComponent` is an optional element that clones `Resources/InventorySystem/InventoryContainer.uxml` (a tab row plus a content area). Use it when you do not want to write those two containers yourself. The demo and the shooter do not use it; they name the containers directly.

## 3. Style it

Two layers, on purpose:

1. **Functional USS** ships in `Resources/InventorySystem/InventoryCore.uss` and is added to the document root automatically. It only covers behavior: the grid wraps, the drag ghost follows the pointer, icons and stack counts show and hide, the panel can be hidden.
2. **Your USS** draws the look: slot color, borders, tab shape, equipment frames.

Do not put cosmetic rules in `InventoryCore.uss`. A game update of the module would overwrite them, and the module is meant to be dropped into projects that already have art.

Style these classes yourself. `Demo/UI/InventoryPanel.uss` is a full example.

| Class | What it is |
| --- | --- |
| `inventory-panel` | Your panel root. Name it whatever you like in UXML; this class is only a suggestion. |
| `inventorySlots` | One generated inventory slot. |
| `inventory-slots-container` | The grid that holds the slots. Padding, gap, and background go here. |
| `inventoryTab` | A generated tab button. |
| `inventoryTab--active` | The selected tab. |
| `equipment-slot` | Every equipment slot, after it is initialized. |
| `equipment-slot--{suffix}` | One slot type. `ussClassSuffix` of `weapon` becomes `equipment-slot--weapon`. |
| `has-item` | Present on a slot that currently shows an item. |

These classes are state. The module turns them on and off. You can restyle them, but do not set `display` on them or their children, or icons, counts, and equipped items disappear.

| Class | When it is on |
| --- | --- |
| `has-item` | The slot is showing an item. Reveals `.inventorySlots-icon` and `.equipment-icon`. |
| `inventorySlots--stacked` | Quantity is above 1. Reveals `.inventorySlots-qty`. |
| `inventoryTab--has-label` | The tab has a `displayName`. |
| `inventoryTab--active` | This tab is selected. Swaps the tab background layers. |
| `ui-hidden` | The panel (or any element using `UIBaseComponent`) is closed. |
| `inventorySlots--dragging` | The slot currently being dragged. |
| `inventorySlots--empty-highlight` | Empty slots while a drag is in progress. |
| `inventorySlots--drop-target` | The slot under a successful drop, for one frame. |
| `drag-ghost`, `drag-ghost--visible` | The icon that follows the pointer. |

Layers you can paint, including from the theme:

| Class | Contents |
| --- | --- |
| `equipment-slot-bg` | Empty-state texture of an equipment slot. Hidden while an item is equipped. |
| `equipment-icon` | The equipped item's sprite. Hidden while the slot is empty. |
| `inventoryTab-bg` | Tab background, normal. |
| `inventoryTab-bg--active` | Tab background, selected. |
| `inventoryTab-label` | The tab's text. |
| `inventorySlots-icon` | The item sprite in an inventory slot. |
| `inventorySlots-qty` | The stack count, bottom-right. |

Do not write `.equipment-icon { display: none }` in your stylesheet. Core USS shows that icon with `.equipment-slot.has-item > .equipment-icon`. A single-class `display: none` fights it. Put empty-slot art on `equipment-slot` or `equipment-slot--weapon` instead. When an item is equipped, `.equipment-slot.has-item` clears the slot's own background image so a silhouette does not sit under the item.

## 4. Add the components

Create an empty GameObject, for example `InventoryUI`. Put the `UIDocument` on it, assign the UXML and Panel Settings, then add:

| Component | Inspector |
| --- | --- |
| `InventoryModel` | `maxInventorySize`. This is the real capacity. `debugLogging` logs add, stack, and move. |
| `EquipmentController` | Nothing required. `debugLogging` logs equip and unequip. |
| `InventoryUIConfig` | See the table below. |
| `TabFilterManager` | Drag this GameObject into `uiConfig`. |
| `DragDropController` | `uiDocument` and `equipmentController` resolve from the same GameObject when left empty. |
| `InventoryUIController` | `document` resolves from the same GameObject. `inventoryPanelName` defaults to `inventory-panel`. `startHidden` hides the panel after the first frame. |

`InventoryUIConfig` fields:

| Field | Effect |
| --- | --- |
| `targetDocument` | The `UIDocument`. Auto-filled from the same GameObject. |
| `panelElementName` | Element that receives the theme's panel background. Default `inventory-panel`. |
| `slotSize` | Width and height of each generated slot, in pixels. |
| `slotCount` | How many slot views to generate. Keep this at least `maxInventorySize`. |
| `cellMargin` | Right and bottom margin of each slot, in pixels. |
| `autoCreateScrollWrapper` | Wraps the grid in a scroll view. Leave on. |
| `scrollWidthPx`, `scrollHeightPx` | Scroll view size. `0` lets the parent layout decide. |
| `verticalVisibility` | Scrollbar visibility. |
| `tabs` | Category assets, in tab order. Include the `all` category if you made one. |
| `tabWidthPx`, `tabHeightPx`, `tabIconWidthPx`, `tabIconHeightPx` | Tab and icon size. `0` leaves size to USS. |
| `equipmentSlotBindings` | Pairs a UXML element name (`equipSlotWeapon`) with an Equipment Slot Type asset. A slot that is not listed here is not interactive. |
| `equipmentController` | Auto-filled from the same GameObject. This is the equipment data the panel displays. |
| `theme` | Optional `InventoryThemeSO`. |
| `addCoreStyles` | Leave on so `InventoryCore.uss` is applied. |

`DragDropController` fields worth knowing:

| Field | Default | Effect |
| --- | --- | --- |
| `dragThreshold` | 5 | Pixels the pointer must move before a drag starts. Raise it if clicks become accidental drags. |
| `doubleClickWindow` | 0.3 | Seconds between clicks to count as a double-click. Uses unscaled time, so it works while paused. |
| `highlightEmptySlots` | on | Outlines empty inventory slots during a drag. |
| `debugMode` | off | Logs pointer, drag, and transaction decisions. |

`slotCount` and `maxInventorySize` are separate. The model refuses adds past `maxInventorySize`. The config only decides how many boxes are drawn. Set them to the same number.

There is one inventory and one equipment component per scene. `InventoryService` finds them with `FindFirstObjectByType` the first time something asks. A second inventory in the same scene is not addressed by the service. See Limits below.

## 5. Open and close the panel

The module does not read input. Call the controller from your input action.

```csharp
using InventorySystem;
using UnityEngine;

public class InventoryInput : MonoBehaviour
{
    [SerializeField] private InventoryUIController inventory;

    public void OnTogglePerformed()
    {
        inventory.Toggle();
    }
}
```

`Open()`, `Close()`, and `IsInventoryOpen` are the same API. `OnInventoryToggled` fires with `true` after a show and `false` after a hide. Use it to release the cursor, pause combat, or hide the HUD. The shooter does this in `UIManager`: the inventory action calls `Toggle()`, and `IsInventoryOpen()` blocks weapon fire.

The panel hides by adding the class `ui-hidden`. That rule is `display: none !important`, so an inline `display: flex` on the panel in UXML does not keep it on screen.

## 6. Give and take items

```csharp
using InventorySystem;

Item_DataSO potion = /* your asset */;
var inventory = InventoryService.GetPlayerInventory();

bool added = inventory.AddItem(new InventoryItem(potion, 3));
if (!added) {
    // Inventory is full, or the item asset is missing.
    // Leave the world object where it is.
}

InventoryItem slot = inventory.GetItemAt(0); // null when that slot is empty
inventory.RemoveItemAt(0);
```

`AddItem` returns false when the item is null or no slot can hold it. Check the return value before destroying a pickup.

`GetItemAt` accepts any index from `0` to `maxInventorySize - 1`. Indexes past the end of the internal list are empty slots, not errors.

`OnInventoryChanged` fires once per add, remove, move, or swap. `OnSlotChanged` fires with each index that changed. The grid already listens to `OnInventoryChanged` and redraws, so gameplay code uses these events for HUD counts and quests, not to refresh slots.

```csharp
inventory.OnInventoryChanged += RefreshQuestTracker;
inventory.OnSlotChanged += index => Debug.Log($"Slot {index} changed");
```

Unsubscribe in `OnDestroy`. The inventory object outlives a typical player, but the listener should not.

### World pickups

`InventorySystem.Extras.ItemPickup` on a GameObject with a trigger collider:

- Assign the item asset and a quantity.
- `pickupOnTriggerEnter` adds the item when a matching layer enters the trigger.
- The object is destroyed only after `AddItem` returns true. A full inventory leaves the pickup in the world.
- `destroyOnPickup` off deactivates the object instead.
- `OnPickedUp` and the UnityEvent `onPickedUp` fire after a successful add.
- Call `TryPickup()` from your own interact key and turn `pickupOnTriggerEnter` off.

The shooter's `Pickup_Item` and `Pickup_Armor` do the same thing against `InventoryService` without using `ItemPickup`. Either path is valid.

## 7. Equip from code

Dragging onto a slot, or double-clicking, equips through the same API you call yourself. The equipment slot view updates from `OnEquipmentChanged`, so a loadout applied in `Start` shows up in the panel.

```csharp
using InventorySystem;

[SerializeField] private Item_DataSO startingWeapon;
[SerializeField] private EquipmentSlotTypeSO weaponSlot;

void Start()
{
    var equipment = InventoryService.GetPlayerEquipment();
    var item = new InventoryItem(startingWeapon);

    if (!equipment.CanEquip(item, weaponSlot)) return;

    // Remove it from the bag first if it was added there.
    equipment.EquipItem(item, weaponSlot);

    equipment.OnEquipmentChanged += OnEquipmentChanged;
}

void OnEquipmentChanged(EquipmentSlotTypeSO slotType, InventoryItem equipped)
{
    if (slotType != weaponSlot) return;
    if (equipped == null) {
        HolsterWeapon();
        return;
    }
    var weapon = equipped.itemData as Weapon_Data;
    if (weapon != null) ShowWeapon(weapon);
}

void OnDestroy()
{
    var equipment = InventoryService.GetPlayerEquipment();
    if (equipment != null) equipment.OnEquipmentChanged -= OnEquipmentChanged;
}
```

`CanEquip` is true only when the item's `compatibleSlots` contains that slot type. `EquipItem` returns false and changes nothing when `CanEquip` fails. `UnequipSlot` clears the slot and passes `null` to the event. `GetEquippedItem` returns the current item, or null.

Replacing an occupied slot fires the event once, with the new item. It does not fire a separate unequip for the old one. If the old item should return to the bag, put it there yourself with `AddItem` or `SetItemAt`. Drag-and-drop already does that: the displaced item lands in the slot the new item came from.

`EquipItem` does not remove the item from the inventory. A drag does both (remove from the bag, then equip). A starting loadout that should not also sit in the bag should be equipped without calling `AddItem`.

## 8. What the pointer does

These are the built-in interactions. They are not rebound per action; the only input you wire is open and close.

| Gesture | Result |
| --- | --- |
| Drag an inventory slot onto another inventory slot | Move, or swap if the target has an item. |
| Drag an inventory item onto an equipment slot | Equip, if `compatibleSlots` allows it. The item that was equipped moves into the source slot. |
| Drag an equipped item onto an inventory slot | Unequip into that slot. If the slot is occupied, the items swap only when that item can go into the equipment slot. |
| Drag one equipment slot onto another | Move, or swap when both items fit the other slot. |
| Double-click an inventory item | Equip into the first equipment slot that accepts it. |
| Double-click an equipped item | Unequip into the first empty inventory slot. Does nothing when the bag is full. |
| Release a drag outside every slot | Cancel. The item stays where it was. |

A filtered tab hides items that are not in that category. It does not pack them into the first slots. Slot 5 is still slot 5. Hidden items cannot be dragged until you switch to a tab that shows them, including `all`.

## 9. Subclass an item

Keep stats on a subclass. Do not copy `Item_DataSO`.

```csharp
using InventorySystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon System/Weapon Data")]
public class Weapon_Data : Item_DataSO
{
    public int magazineCapacity;
    public float fireRate;
}
```

Create the asset from the subclass menu. Set `category` and `compatibleSlots` on the same asset. At runtime, cast `equipped.itemData as Weapon_Data` inside `OnEquipmentChanged`.

## 10. Register explicitly

`InventoryService.GetPlayerInventory()` and `GetPlayerEquipment()` find the scene objects on first use. Call the register methods when you construct the inventory yourself or when more than one exists and you need to choose.

```csharp
InventoryService.RegisterPlayerInventory(inventoryModel);
InventoryService.RegisterPlayerEquipment(equipmentController);
```

`Clear()` drops the cached references. Statics are also cleared automatically when entering Play Mode, including when Domain Reload is disabled.

The UI components resolve the shared `InventoryViewModel` from the service. Do not construct another `InventoryViewModel` for the same model. A second wrapper would split the change events and the grid would stop updating.

## Limits

- One player inventory and one equipment set per scene, reached through `InventoryService`. Chests, shops, and a second character need a later change; the service cannot point the panel at a different inventory.
- There is no save API. `itemId` is the key a save system should store, then look the `Item_DataSO` back up. `InventoryModel.itemList` and `EquipmentController.equippedItems` are the runtime state to write out.
- `description` is not shown. Tooltips are yours.
- Equipping does not, by itself, change character stats or spawn a mesh. Subscribe to `OnEquipmentChanged` and do that in the game.

## Troubleshooting

**The panel never appears.** The `UIDocument` needs Panel Settings. `InventoryUIController.inventoryPanelName` must match the UXML element name. `startHidden` leaves it closed until `Toggle()` or `Open()`.

**Tabs or slots are missing.** `tabButtonsContainer` and `tabContentContainer` must exist under the document. `InventoryUIConfig.tabs` needs at least one category asset. `slotCount` must be greater than zero.

**An equipment slot does nothing.** The UXML `name` and the binding's `elementName` must match, and the binding needs a Slot Type asset. The item needs that same asset in `compatibleSlots`.

**A weapon stacks with itself.** `stackable` is on by default. Turn it off on the item asset.

**The equipped icon never shows.** A stylesheet rule of `.equipment-icon { display: none }` overrides the core rule. Remove it. Also confirm the item has an `icon` sprite.

**Empty equipment slots lost their silhouette.** `.equipment-slot.has-item` clears the slot background while an item is in it. The silhouette belongs on `equipment-slot` or `equipment-slot--{suffix}`, not on a rule that stays active after `has-item` is added.

**Drag never starts.** `DragDropController` needs the `UIDocument` and an `InventoryModel` in the scene. Enable `debugMode` and look for `[DragDrop] Initialized`. Raising `dragThreshold` makes drags harder to start; lowering it makes them easier.

**AddItem returns false on an empty bag.** `maxInventorySize` is 0, the `Item_DataSO` reference is missing, or a stackable add found no legal stack and no free slot. `debugLogging` on `InventoryModel` prints the reason.

**Code equips an item and the slot stays empty.** The slot type asset passed to `EquipItem` must be the same asset listed in `equipmentSlotBindings`. A duplicate Slot Type asset with the same `id` is a different reference and will not match.
