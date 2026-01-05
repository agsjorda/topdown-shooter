using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

/// <summary>
/// Manages player weapon slots, switching, shooting, and reload mechanics.
/// </summary>
public class PlayerWeaponController : MonoBehaviour
{
    private const float REFERENCE_BULLET_SPEED = 20f;

    [Header("References")]
    private Player player;
    private UIManager uiManager; // Cache UIManager reference

    [Header("Weapon Settings")]
    [SerializeField] private Weapon_Data defaultWeaponData;
    [SerializeField] private int maxSlots = 4;
    [SerializeField] private WeaponSlot[] weaponSlots;
    [SerializeField] private int currentSlotIndex = 0;
    [SerializeField] private Weapon currentWeapon;

    [Header("Bullet Settings")]
    [SerializeField] private float bulletImpactForce = 100f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private GameObject weaponPickupPrefab;

    [Header("State")]
    private bool weaponReady = true;
    private bool isShooting;

    #region Properties
    public Weapon CurrentWeapon => currentWeapon;
    public int SlotCount => maxSlots;
    public bool IsWeaponReady => weaponReady;
    #endregion

    private void Start()
    {
        player = GetComponent<Player>();
        uiManager = Object.FindFirstObjectByType<UIManager>(); // Cache on Start
        InitializeWeaponSlots();
        AssignInputEvents();
        Invoke(nameof(EquipStartingWeapon), 0.1f);
    }

    private void Update()
    {
        if (isShooting && weaponReady)
            Shoot();
    }

    #region Initialization
    private void InitializeWeaponSlots()
    {
        weaponSlots = new WeaponSlot[maxSlots];
        for (int i = 0; i < maxSlots; i++) {
            weaponSlots[i] = new WeaponSlot { slotIndex = i };
        }
    }

    private void EquipStartingWeapon()
    {
        if (defaultWeaponData != null) {
            AddWeaponToSlot(new Weapon(defaultWeaponData), 0);
            EquipSlot(0);
        }
    }
    #endregion

    #region Slot Management
    public bool AddWeaponToSlot(Weapon weapon, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return false;
        if (weapon == null) return false;

        weaponSlots[slotIndex].AssignWeapon(weapon);
        return true;
    }

    public bool EquipSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || weaponSlots[slotIndex].IsEmpty)
            return false;

        // Set weapon not ready during equip
        SetWeaponReady(false);

        UnequipCurrentSlot();
        currentSlotIndex = slotIndex;
        currentWeapon = weaponSlots[slotIndex].weapon;
        weaponSlots[slotIndex].isEquipped = true;

        UpdateVisuals();

        // Play equip animation
        player.weaponVisuals.PlayWeaponEquipAnimation();

        return true;
    }

    private void UnequipCurrentSlot()
    {
        if (IsValidSlot(currentSlotIndex))
            weaponSlots[currentSlotIndex].isEquipped = false;
    }

    private bool IsValidSlot(int index) => index >= 0 && index < maxSlots;

    public void QuickSwitchWeapon()
    {
        for (int i = 1; i <= maxSlots; i++) {
            int nextSlot = (currentSlotIndex + i) % maxSlots;
            if (!weaponSlots[nextSlot].IsEmpty) {
                EquipSlot(nextSlot);
                return;
            }
        }
    }
    #endregion

    #region Weapon Pickup/Drop
    public void PickupWeapon(Weapon newWeapon)
    {
        // Check for existing weapon of same type
        Weapon existingWeapon = FindWeaponByType(newWeapon.weaponType);
        if (existingWeapon != null) {
            existingWeapon.totalReserveAmmo += newWeapon.bulletsInMagazine;
            return;
        }

        // Find empty slot
        int emptySlot = FindEmptySlot();
        if (emptySlot != -1) {
            AddWeaponToSlot(newWeapon, emptySlot);
            player.weaponVisuals.UpdateBackupVisuals();
            return;
        }

        // Send to inventory if slots full
        SendToInventory(newWeapon);
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < maxSlots; i++) {
            if (weaponSlots[i].IsEmpty)
                return i;
        }
        return -1;
    }

    private Weapon FindWeaponByType(WeaponType type)
    {
        foreach (var slot in weaponSlots) {
            if (!slot.IsEmpty && slot.weapon.weaponType == type)
                return slot.weapon;
        }
        return null;
    }

    /// <summary>
    /// Attempts to add a weapon to the player's inventory when weapon slots are full.
    /// </summary>
    private void SendToInventory(Weapon weapon)
    {
        var inventory = InventoryService.GetPlayerInventory();
        if (inventory != null && inventory.CanAddItem()) {
            inventory.AddItem(new InventoryItem(weapon.weaponData));
            Debug.Log($"Weapon {weapon.weaponData.itemName} added to inventory (weapon slots full)");
        } else {
            Debug.LogWarning($"Cannot add weapon to inventory - inventory is full or not found");
        }
    }

    public void DropCurrentWeapon()
    {
        if (weaponSlots[currentSlotIndex].IsEmpty || GetEquippedWeaponCount() <= 1)
            return;

        DropWeaponFromSlot(currentSlotIndex);

        // Equip first available weapon
        for (int i = 0; i < maxSlots; i++) {
            if (!weaponSlots[i].IsEmpty) {
                EquipSlot(i);
                break;
            }
        }
    }

    private void DropWeaponFromSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || weaponSlots[slotIndex].IsEmpty)
            return;

        Weapon weaponToDrop = weaponSlots[slotIndex].weapon;

        // Create pickup
        GameObject droppedWeapon = ObjectPool.instance.GetObject(weaponPickupPrefab);
        var pickup = droppedWeapon.GetComponent<Pickup_Weapon>();
        if (pickup != null)
            pickup.SetupPickupWeapon(weaponToDrop, transform.position + Vector3.up * 0.75f);

        weaponSlots[slotIndex].Clear();
    }

    private int GetEquippedWeaponCount()
    {
        int count = 0;
        foreach (var slot in weaponSlots) {
            if (!slot.IsEmpty) count++;
        }
        return count;
    }

    public bool HasOnlyOneWeapon() => GetEquippedWeaponCount() <= 1;
    #endregion

    #region Shooting
    private void Shoot()
    {
        // Don't allow shooting when inventory is open
        if (uiManager != null && uiManager.IsInventoryOpen()) {
            return;
        }

        if (currentWeapon == null || !currentWeapon.CanShoot() || !weaponReady)
            return;

        player.weaponVisuals.PlayFireAnimation();

        if (currentWeapon.shootType == ShootType.Single)
            isShooting = false;

        if (currentWeapon.BurstActivated()) {
            StartCoroutine(BurstFire());
            return;
        }

        FireSingleBullet();
        TriggerEnemyDodge();
    }

    private IEnumerator BurstFire()
    {
        SetWeaponReady(false);

        for (int i = 0; i < currentWeapon.bulletsPerShot; i++) {
            FireSingleBullet();
            yield return new WaitForSeconds(currentWeapon.burstFireDelay);
        }

        SetWeaponReady(true);
    }

    private void FireSingleBullet()
    {
        currentWeapon.bulletsInMagazine--;

        GameObject newBullet = ObjectPool.instance.GetObject(bulletPrefab);
        if (newBullet == null) return;

        Transform gunPoint = player.weaponVisuals.GetCurrentWeaponModel()?.gunPoint;
        if (gunPoint == null) return;

        newBullet.transform.position = gunPoint.position;
        newBullet.transform.rotation = Quaternion.LookRotation(gunPoint.forward);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();
        Bullet bulletScript = newBullet.GetComponent<Bullet>();

        if (bulletScript != null)
            bulletScript.BulletSetup(currentWeapon.gunDistance, bulletImpactForce);

        Vector3 bulletDirection = currentWeapon.ApplySpread(CalculateBulletDirection(gunPoint));

        rbNewBullet.mass = REFERENCE_BULLET_SPEED / bulletSpeed;
        rbNewBullet.linearVelocity = bulletDirection * bulletSpeed;
    }

    private Vector3 CalculateBulletDirection(Transform gunPoint)
    {
        if (player.aim == null) return gunPoint.forward;

        Transform aim = player.aim.Aim;
        Vector3 direction = (aim.position - gunPoint.position).normalized;

        if (!player.aim.CanAimPrecisely && player.aim.GetCurrentTarget() == null)
            direction.y = 0;

        return direction;
    }
    #endregion

    #region Reloading
    public void Reload()
    {
        if (currentWeapon == null || !currentWeapon.CanReload() || !weaponReady)
            return;

        SetWeaponReady(false);
        player.weaponVisuals.PlayReloadAnimation();
    }

    public void CompleteReload()
    {
        if (currentWeapon != null)
            currentWeapon.ReloadBullets();
        SetWeaponReady(true);
    }
    #endregion

    #region Utility Methods
    private void UpdateVisuals()
    {
        if (currentWeapon != null) {
            // First update weapon models
            player.weaponVisuals.UpdateWeaponVisuals(
                currentWeapon.weaponType,
                GetEquippedWeaponCount() > 1
            );

            // Update camera distance
            CameraManager.instance?.ChangeCameraDistance(currentWeapon.cameraDistance);
        }
    }

    public void SetWeaponReady(bool ready) => weaponReady = ready;

    public WeaponModel GetCurrentWeaponModel()
    {
        return currentWeapon != null ?
            player.weaponVisuals.GetWeaponModel(currentWeapon.weaponType, WeaponModelType.Primary) :
            null;
    }

    public List<Weapon> GetAllWeapons()
    {
        List<Weapon> weapons = new List<Weapon>();
        foreach (var slot in weaponSlots) {
            if (!slot.IsEmpty)
                weapons.Add(slot.weapon);
        }
        return weapons;
    }

    public Weapon WeaponInSlots(WeaponType weaponType)
    {
        return FindWeaponByType(weaponType);
    }

    public Transform GunPoint() => player.weaponVisuals.GetCurrentWeaponModel()?.gunPoint;

    public Vector3 BulletDirection()
    {
        Transform gunPoint = GunPoint();
        if (gunPoint == null) return Vector3.forward;
        return CalculateBulletDirection(gunPoint);
    }

    private void TriggerEnemyDodge()
    {
        Transform gunPoint = GunPoint();
        if (gunPoint == null) return;

        Vector3 rayOrigin = gunPoint.position;
        Vector3 rayDirection = CalculateBulletDirection(gunPoint);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, Mathf.Infinity)) {
            Enemy_Melee enemyMelee = hitInfo.collider.GetComponentInParent<Enemy_Melee>();
            enemyMelee?.ActivateDodgeRoll();
        }
    }
    #endregion

    #region Input Events
    private void AssignInputEvents()
    {
        PlayerControls controls = player.controls;

        // Shooting
        controls.Character.Fire.performed += ctx => isShooting = true;
        controls.Character.Fire.canceled += ctx => isShooting = false;

        // Quick slots (only enable if slot has weapon)
        controls.Character.EquipSlot1.performed += ctx => { if (!weaponSlots[0].IsEmpty) EquipSlot(0); };
        controls.Character.EquipSlot2.performed += ctx => { if (!weaponSlots[1].IsEmpty) EquipSlot(1); };
        controls.Character.EquipSlot3.performed += ctx => { if (!weaponSlots[2].IsEmpty) EquipSlot(2); };
        controls.Character.EquipSlot4.performed += ctx => { if (!weaponSlots[3].IsEmpty) EquipSlot(3); };
        controls.Character.EquipSlot5.performed += ctx => { if (maxSlots > 4 && !weaponSlots[4].IsEmpty) EquipSlot(4); };

        // Quick switch
        //controls.Character.QuickSwitchWeapon.performed += ctx => QuickSwitchWeapon();

        // Drop weapon
        controls.Character.DropCurrentWeapon.performed += ctx => DropCurrentWeapon();

        // Reload
        controls.Character.Reload.performed += ctx => Reload();

        // Toggle burst mode
        controls.Character.ToggleWeaponMode.performed += ctx =>
        {
            if (currentWeapon != null)
                currentWeapon.ToggleBurstMode();
        };
    }
    #endregion
}