# Inventory System Modularization Plan

Extract the inventory system into a reusable, FogOfWar-style module at `Assets/InventorySystem/`, plug-and-play for other games.

## Status

- [x] Phase 0 — Documentation (`game-architecture.md`, `inventory-system.md`, this plan)
- [x] Phase A — Module skeleton, file moves, asmdef, namespaces *(compile gate passed 2026-06-13; play-mode suite pending user session — note: an `Item_DataSO.ArmorKind` virtual + `Armor_Data` override was added as a temporary shim so the module never references game-side `Armor_Data`; dies in Phase C)*
- [x] Phase B — Robustness refactor pass (Tier 1 + 2) *(compile gate passed 2026-06-13; also fixed: VM bypassing model in RemoveItemAt, IndexOutOfRange on unallocated valid indices, never-removed `--drop-target` USS class, weapon pickup vanishing when overflow fails — `PickupWeapon` now returns bool. Debug.Log gating + double-click window exposure landed early. Play-mode suite pending user session)*
- [x] Phase C — ScriptableObject type system *(compile+import gate passed 2026-06-13; LegacyItemEnums + ArmorKind shim + Armor_Data.armorType deleted; 9 type assets authored in Assets/Data/InventoryTypes; 8 item assets re-authored; scene tabs + equipmentSlotBindings rewired via YAML; stacking landed — InventoryItem.quantity, whole-stack merge, qty label on SlotView)*
- [x] Phase D — UIManager split *(gate passed 2026-06-13; module InventoryUIController added to scene fileID 474115399 and wired into slimmed game UIManager; per-frame input re-subscription fixed — subscribe in Start, unsubscribe in OnDestroy)*
- [x] Phase E — UXML/USS packaging *(gate passed 2026-06-13; InventoryContainer.uxml moved to Resources/InventorySystem with GUID preserved; InventoryCore.uss auto-applied by InventoryUIConfig — game's GameUI.uss overrides it for visual parity)*
- [x] Phase F — Extras pickup, playable demo, QuickStart docs, `.unitypackage` export *(gate passed 2026-06-13; Demo/InventoryDemo.unity + demo data/UI/theme hand-authored; InventorySystem.Demos asmdef compiles; QuickStart.md written; package exported to project root)*

**Remaining (requires a human in the editor):** run the play-mode verification suite below in SampleScene and the demo checks in InventoryDemo.unity. All six compile/import gates passed in batch mode.

**Play-test regression (2026-06-13, FIXED — pending retest):** first play-test found drag/drop, double-click equip, and tab clicks all dead after the first slot click. Root cause: the Phase B event delegation moved Move/Up handlers to the slots container but still called `CapturePointer` on the SlotView — UI Toolkit delivers a captured pointer's events *exclusively* to the capturing element, so the container's handlers were starved AND the capture was never released (the old per-slot Up handler did the releasing), hijacking all panel pointer input after one click. Fix: inventory-originated presses now capture the **slots container** (where the handlers live); the container Up handler releases the capture; `Cleanup()` defensively releases any stale mouse capture. Also wired the never-assigned `uiConfig.TabFilterManager` back-reference in `TabFilterManager.Awake`. Re-export the .unitypackage after retest.

## Decisions

- **FogOfWar-style packaging**: self-contained `Assets/InventorySystem/` (not UPM), asmdef hierarchy, separable `Demo/`, distributed as `.unitypackage`.
- **Name**: assembly `InventorySystem`; namespaces `InventorySystem` / `InventorySystem.Extras` / `InventorySystem.Demos` (+ `InventorySystem.EditorTools` reserved).
- **ScriptableObject-driven types**: `ItemCategorySO` (tabs/filtering) + `EquipmentSlotTypeSO` (slot validation) replace the `ItemType`/`EquipmentSlotType`/`ArmorType` enums. Items declare `compatibleSlots`; slots have no accepted-lists (single source of truth).
- **Strict UI Toolkit**: nothing under `Assets/InventorySystem/` may use uGUI.
- **Pickups**: module ships optional `InventorySystem.Extras.ItemPickup`; the shooter's `Interactable`-based pickups stay game-side unchanged. `Pickup_Weapon`/`Pickup_Ammo` are shooter concepts and stay regardless.
- **Playable demo scene** (`Demo/InventoryDemo.unity`), deletable without breaking the core.
- No save/load. Shooter must behave identically after each phase.

## Target layout

```
Assets/InventorySystem/
├── Scripts/
│   ├── InventorySystem.asmdef          {"name":"InventorySystem","autoReferenced":true}
│   ├── Core/        IInventory, InventoryModel, InventoryViewModel, InventoryItem,
│   │                IEquipmentSystem, EquipmentController, InventoryService, DragDropController
│   ├── Data/        Item_DataSO, ItemCategorySO (new), EquipmentSlotTypeSO (new)
│   ├── DragDrop/    DragState, DragDropService, DragVisualHandler, DoubleClickHandler,
│   │                DragDropTransaction, TransactionFactory, Transactions/ (6 classes)
│   ├── UI/          SlotView, EquipmentSlotView, InventoryUIConfig, TabFilterManager,
│   │                InventoryTabElement, InventoryScrollElement, InventoryContainerComponent,
│   │                Inventory_UI, UIBaseComponent, InventoryUIController (new)
│   ├── Utilities/   VisualElementExtensions, EnumerableExtensions, RectExtensions
│   ├── Extras/      ItemPickup.cs (new, optional drop-in)
│   └── Editor/      (reserved for InventorySystem.Editor.asmdef when needed)
├── Resources/InventorySystem/          InventoryContainer.uxml (moved), InventoryCore.uss (new)
├── Demo/                               own asmdef + InventorySystem.Demos namespace, deletable
│   ├── InventoryDemo.unity, Scripts/, Data/ (sample SOs + icons), UI/ (panel UXML/USS, PanelSettings)
└── Documentation/QuickStart.md
```

## Phase A — Skeleton, moves, asmdef, namespaces

1. Unity closed. Create skeleton + `Scripts/InventorySystem.asmdef` (`autoReferenced: true` so Assembly-CSharp sees it).
2. `git mv` **with .meta files** (renames, not add/delete):
   - `Assets/Scripts/InventorySystem/**` → `Assets/InventorySystem/Scripts/...` **except** game-side: `Weapon_Data.cs`/`Armor_Data.cs`/`Enemy_MeleeWeaponData.cs` → `Assets/Scripts/Data/`; `WeaponSlot.cs` → `Assets/Scripts/Weapon/`; `UIManager.cs` → `Assets/Scripts/UI/`.
   - `VisualElementExtensions/EnumerableExtensions/RectExtensions` → `Scripts/Utilities/` (used only by inventory code).
3. Split `ItemEnums.cs`: `ItemType`/`EquipmentSlotType`/`ArmorType` → module `Scripts/Data/LegacyItemEnums.cs` (global namespace, deleted in Phase C); game file (same GUID) keeps the 7 game enums.
4. Namespace pass: all module runtime files → `namespace InventorySystem` (flatten sub-namespaces). Game-side `using InventorySystem;` added to `GameHUD_UI.cs`, `HealthBar_UI.cs`, relocated `UIManager.cs`. PlayerWeaponController/Pickup_* already import it.
5. **Same commit:** `GameUi.uxml` lines 32-41 — `<EquipmentSlotView>` → `<InventorySystem.EquipmentSlotView>` (namespacing changes the UXML tag; missing this silently breaks equipment slots).
6. Open Unity, resave SampleScene. **Gate:** verification suite identical.

## Phase B — Robustness refactor pass

**Tier 1 (correctness):**
1. `CanAddItem()` counts nulls as occupied → free ⇔ `FindFirstEmptySlot() >= 0`.
2. `GetItemAt` mutates (grows) the list → pure getter; `EnsureSize` only in setters.
3. `IInventory` mutators return `bool`; pickups consume the world object only on success.
4. `InventoryService`: `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` reset for disabled domain reload.
5. One cached `InventoryViewModel` in `InventoryService`; `InventoryUIConfig`/`DragDropController` consume it.

**Tier 2 (lifecycle):**
6. Slot pointer events via **event delegation** on the slots container (bubbling + `evt.target` resolution) — per-slot callbacks are orphaned today because `RebuildUI()` recreates all slots after every transaction.
7. Refresh slot contents instead of full rebuild per transaction; drop `InventoryUIConfig.Update()` polling.
8. `IEquipmentSystem.OnEquipmentChanged` event; delete placeholder `AddItemToSlot`.
9. `InventoryModel.OnSlotChanged(int)` alongside coarse `OnInventoryChanged`.

**Tier 3 (folded into later phases):** Debug.Log gating (C) · USS/element-name constants (E) · double-click window exposed (C) · remove `InventoryTabElement` statics (E) · **stacking** — `InventoryItem.quantity` honoring `stackable`/`maxStack` (end of C) · optional plain-C# core + EditMode tests (deferred).

**Gate:** suite identical + fill-free-refill works, full-inventory pickup stays on ground, drag/drop survives many consecutive transactions.

## Phase C — ScriptableObject type system

- New `ItemCategorySO { id, displayName, tabIcon, tabIconTint }` (id doubles as tab id; `Category_All` special-cased by existing `"all"` string).
- New `EquipmentSlotTypeSO { id, displayName, ussClassSuffix }`.
- `Item_DataSO`: `itemType` enum → `category` + `compatibleSlots` + `CanEquipTo(slot)`.
- `IEquipmentSystem`/`EquipmentController`/`SlotView.EquipmentType`/`EquipmentSlotView`/`DoubleClickHandler`: enum → SO. `EquipmentSlotView.CanAcceptItem` collapses to `itemData.CanEquipTo(SlotType)`.
- `InventoryUIConfig`: `TabDescriptor` deleted; `tabs: List<ItemCategorySO>`; new `equipmentSlotBindings: List<{slotType, elementName}>`; `DragDropController` iterates bindings (no hard-coded slot names).
- `TabFilterManager.GetTabIdForItem` → `item?.itemData?.category?.id`.
- Game: `Armor_Data` drops `armorType`; `Pickup_Armor` switch replaced by `itemName`-based naming.
- Delete `LegacyItemEnums.cs`.
- **Assets:** `Assets/Data/InventoryTypes/` — 5 categories (icons/tint copied from scene tab descriptors), 4 slot types. Re-author 8 item assets (weapons→Weapon slot, hat→Headgear, backpack→Vest, potion→none).
- **Scene:** repopulate `tabs`, fill `equipmentSlotBindings` (`equipSlotArmor` maps to **Vest**, matching today).

**Gate:** slot-type enforcement per item, tab filtering per category.

## Phase D — UIManager split

- Module `InventoryUIController` (UIDocument + panel name; `Toggle/Open/Close`, `IsInventoryOpen`, `OnInventoryToggled` event; no Player/input/cameras — compiler-enforced).
- Game `UIManager` keeps Player/HUD/cameras, delegates inventory calls, subscribes to the event; **fix per-frame input re-subscription** (subscribe in `Start`, unsubscribe in `OnDestroy`).
- Scene: add controller component, wire references. `PlayerWeaponController` unchanged.

**Gate:** toggle fires once per press; HUD/cameras behave; firing blocked while open.

## Phase E — UXML/USS packaging

- Shooter panel stays in `GameUi.uxml`; module documents the **element-name contract** and ships a working panel in `Demo/UI/`.
- `InventoryContainer.uxml` → `Resources/InventorySystem/`; `InventoryContainerComponent` loads via `[UxmlAttribute]` template → `Resources.Load`.
- New `InventoryCore.uss` (functional classes only); `InventoryUIConfig.coreStyles` auto-added if absent. Shooter's `GameUI.uss` keeps visual parity.

**Gate:** suite + visual parity.

## Phase F — Extras pickup, demo, docs, export

- `Extras/ItemPickup.cs`: `Item_DataSO item; int quantity`; trigger mode (LayerMask) + `bool TryPickup()`; fires `OnPickedUp` (C# + UnityEvent); zero game references.
- Demo scene: rig GO with the 6 components + UIDocument (`Demo/UI/InventoryPanel.uxml`, DemoPanelSettings); demo scripts (`DemoInventoryInput` via `Keyboard.current`, `DemoItemGiver`); ground items via Extras `ItemPickup`; stacked potions demo.
- `Documentation/QuickStart.md`: drop-in steps, element-name + USS contract, API surface, export/import.
- Deletability gates: removing `Demo/` compiles; removing `Scripts/Extras/` compiles.
- Export `InventorySystem.unitypackage`; import into a throwaway Unity 6 URP project — demo works standalone.

## Verification suite (Play mode, SampleScene — run after every phase)

1. Pistol auto-equips; firing + reload work.
2. Pick up potion, hat, backpack, weapons (all three pickup paths).
3. 5th weapon overflows to inventory.
4. Toggle inventory: HUD hides, minimap off, preview on; firing blocked; close restores.
5. Drag/drop all 4 directions + cancel + ghost + highlights.
6. Double-click quick-equip/unequip.
7. Tab filtering; "all" shows everything.
8. (B+) fill/free/refill; full-inventory pickup persists; repeated transactions keep working.
9. (F) demo scene full pass; Demo/ and Extras/ deletable.

## Risks

- **GUID/meta preservation** — move with Unity closed, `git status` must show renames. Critical GUIDs: UIManager `5f05e113…`, InventoryUIConfig `185bffaf…`, DragDropController `c2accd89…`, EquipmentController `a4ba8a39…`, TabFilterManager `a0c7c246…`, InventoryModel `a9d10b6f…`, Item_DataSO `df2aeb16…`, Weapon_Data `6679e192…`, Armor_Data `5d4c4419…`.
- Namespaced `[UxmlElement]` ⇒ UXML tag change must land with Phase A.
- Serialized `tabs`/slot-type fields reset on type change ⇒ re-authored in Phase C.
- Phase B changes failure-path behavior deliberately ⇒ gate tests target it.
- Demos asmdef references `Unity.InputSystem`; core asmdef references nothing beyond engine modules.
