# Game Architecture — Top-Down Shooter

> Read-through snapshot: June 2026. Paths are relative to the project root.

## Project setup

- **Unity 6** (6000.4.x), **URP**, **New Input System** (1.19), **UI Toolkit**, **Cinemachine 3.1**, **NavMesh/AI Navigation**, ML-Agents present in manifest.
- **Scenes** (`Assets/Scenes/`): `SampleScene.unity` (main), `SampleScenetest.unity`, `SampleSceneWithFOW.unity` (Fog-of-War variant).
- **No assembly definitions** in game code — everything compiles into `Assembly-CSharp` (the InventorySystem module is being extracted into its own asmdef; see `inventory-modularization-plan.md`).
- Namespaces are mostly global; only parts of the inventory system use `namespace InventorySystem`.
- Third-party: **Pixel-Perfect Fog Of War** (`Assets/FogOfWar/`, assembly `Pixel-PerfectFogOfWar`, namespace `FOW`) — also the modularity model for the inventory extraction.

## Player (`Assets/Scripts/Player/`)

`Player.cs` is a composition hub exposing subsystems as properties: `controls` (generated `PlayerControls` from `Assets/Input Manager/PlayerControls.inputactions`), `aim`, `movement`, `weapon`, `weaponVisuals`, `interaction`.

| Component | Responsibility |
|---|---|
| `PlayerMovement` | CharacterController walk/run, gravity, rotates toward mouse aim, drives locomotion animation |
| `PlayerAim` | Mouse raycast aiming, laser sight (LineRenderer), camera follow, precise-aim and target-lock toggles |
| `PlayerWeaponController` | 4-slot weapon array (`WeaponSlot[]`), fire/reload/burst, ammo, **overflow weapons go to the inventory** via `InventoryService` |
| `PlayerWeaponVisuals` | Shows/hides weapon models, animation layers per `HoldType`, left-hand IK |
| `PlayerInteraction` | Tracks nearby `Interactable`s, highlights closest, interacts on key press |
| `PlayerAnimationEvents` | Animation-event callbacks for reload/equip completion |

Input flow: `PlayerControls` callbacks → movement/fire/aim/interaction. The UI's inventory toggle is also read from `player.controls` (being decoupled — see plan Phase D).

## Weapons (`Assets/Scripts/Weapon/`, `Bullet.cs`)

- `Weapon` — runtime state (magazine, reserve ammo, fire rate, accumulating spread, burst mode) built from a `Weapon_Data` ScriptableObject.
- `WeaponModel` — prefab marker with `gunPoint`/`holdPoint` and enums (`WeaponType`, `WeaponModelType`, `HoldType`, `EquipType`, `HangType`).
- `Bullet` — pooled projectile; disables itself past `flyDistance`; on hit calls `Enemy.GetHit()` + `DeathImpact()`; returns to pool. Has a FogOfWar integration check (`FOW.FogOfWarHider`).

Firing path: `PlayerWeaponController.Shoot()` → ammo/fire-rate checks → `Weapon.ApplySpread()` → `ObjectPool.GetObject(bulletPrefab)` → rigidbody velocity.

## Enemies (`Assets/Scripts/Enemy/`)

State-machine architecture: `EnemyStateMachine` holds current `EnemyState` (Enter/Update/Exit pattern).

- `Enemy` (base): NavMeshAgent + Animator, `EnterBattleMode()` on player detection, `GetHit()` (spawns pooled `FloatingText` damage popups), `DeathImpact()` ragdoll impulse.
- **Melee** (`Enemy_Melee/`): Idle/Move/Chase/Attack/Recovery/Dead/Ability states; variants Regular/Shield/Dodge/AxeThrow; `Enemy_Shield` durability, `Enemy_Axe` throwable, dodge-roll triggered by player raycasts.
- **Range** (`Enemy_Range/`): Idle/Move/Battle states; fires pooled `Enemy_Bullet` volleys with cooldowns.
- Support: `Enemy_Visuals` (appearance variation), `Enemy_Ragdoll`, `Enemy_AnimationEvents`, `Enemy_WeaponModel`, `Enemy_PatrolPoint`, `Enemy_CorruptionCrystal`.

## Pickups & interaction (`Assets/Scripts/Pickups/`, `Interactable.cs`)

`Interactable` (abstract): trigger-radius detection, highlight, `Interaction()` on key press via `PlayerInteraction`.

| Pickup | Routing |
|---|---|
| `Pickup_Weapon` | → `PlayerWeaponController.PickupWeapon()`; if slots full → `SendToInventory(weaponData)` via `InventoryService` |
| `Pickup_Ammo` | → adds to `Weapon.totalReserveAmmo` of matching weapons (`AmmoBoxType` small/big) |
| `Pickup_Item` | → `InventoryService.GetPlayerInventory().AddItem(new InventoryItem(itemData))` |
| `Pickup_Armor` | → same as item, with `Armor_Data` |

These are the **only game→inventory call sites** (plus `UIManager`); they go exclusively through `InventoryService`/`InventoryItem`/data SOs.

## Supporting systems

- `ObjectPool` (singleton, `Assets/Scripts/Object Pool/`) — preallocates bullets, pickups, floating text; `GetObject`/`ReturnObject`.
- `CameraManager` (singleton) — Cinemachine camera distance, driven by weapon `cameraDistance`.
- `FloatingText` — pooled damage popups.
- `Target.cs` — marker for aim-target queries.
- UI: `Assets/UI/GameUi.uxml` + `Assets/UI/uss/GameUI.uss` (single UIDocument: HUD panel, health bar, minimap, **and** the inventory panel); `GameHUD_UI`/`HealthBar_UI` are UI Toolkit wrappers. The scene also has a uGUI canvas for minimap RawImage etc. (game-side only).

## Data (`Assets/Data/`)

ScriptableObject content: 5 weapon assets (`Weapon_*_D.asset`), 2 armor assets (`Armor_CowboyHat`, `Armor_Backpack`), `Health_Potion_Small`, enemy data under `Enemy/`.

## Patterns in use

Singletons (ObjectPool, CameraManager), Service Locator (`InventoryService`), State Machine (enemies), Object Pool, Composition (Player), Strategy + Factory (inventory drag/drop transactions), Observer (inventory change events, input callbacks), MVVM (inventory).

## Inventory system

See [inventory-system.md](inventory-system.md) for the deep dive and [inventory-modularization-plan.md](inventory-modularization-plan.md) for the extraction into `Assets/InventorySystem/`.
