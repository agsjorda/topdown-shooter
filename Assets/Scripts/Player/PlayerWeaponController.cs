using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;

    private const float REFERENCE_BULLET_SPEED = 20f;
    //Default speed from which our mass fromula is derived

    [SerializeField] private Weapon currentWeapon;
    private bool weaponReady;
    private bool isShooting;

    [Header("Bullet Details")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

    [SerializeField] private Transform weaponHolder;

    [Header("Inventory")]
    [SerializeField] private int maxSlots = 2;
    [SerializeField] private List<Weapon> weaponSlots;

    private void Start()
    {
        player = GetComponent<Player>();
        AssignInputEvents();

        Invoke("EquipStartingWeapon", 0.1f);
    }

    private void Update()
    {
        if (isShooting)
            Shoot();

    }


    #region Slots Management - Equip, Drop, Pickup, Ready Weapon

    private void EquipStartingWeapon() => EquipWeapon(0);

    private void EquipWeapon(int i)
    {
        if (i >= weaponSlots.Count)
            return;

        SetWeaponReady(false);

        currentWeapon = weaponSlots[i];

        player.weaponVisuals.PlayWeaponEquipAnimation();

        CameraManager.instance.ChangeCameraDistance(currentWeapon.cameraDistance);
    }

    public void PickupWeapon(Weapon newWeapon)
    {

        if (weaponSlots.Count >= maxSlots) {
            Debug.Log("Can't carry more than 2 weapons!");
            return;
        }

        weaponSlots.Add(newWeapon);
        player.weaponVisuals.SwitchOnBackupWeaponModel();

    }

    private void DropWeapon()
    {
        if (hasHasOnlyOneWeapon()) {
            Debug.Log("Can't drop last weapon!");
            return;
        }

        weaponSlots.Remove(currentWeapon);

        EquipWeapon(0);
    }

    public void SetWeaponReady(bool ready) => weaponReady = ready;
    public bool IsWeaponReady() => weaponReady;
    #endregion

    private IEnumerator BurstFire()
    {
        SetWeaponReady(false);

        for (int i = 1; i <= currentWeapon.bulletsPerShot; i++) {
            FireSingleBullet();
            yield return new WaitForSeconds(currentWeapon.burstFireDelay);

            if (i >= currentWeapon.bulletsPerShot)
                SetWeaponReady(true);
        }
    }

    private void Shoot()
    {

        if (IsWeaponReady() == false)
            return;

        if (currentWeapon.CanShoot() == false)
            return;

        player.weaponVisuals.PlayFireAnimation();
        Debug.Log("Pew Pew");

        if (currentWeapon.shootType == ShootType.Single)
            isShooting = false; // Prevent continuous shooting for single-shot weapons

        if (currentWeapon.BurstActivated() == true) {
            StartCoroutine(BurstFire());
            return;
        }


        FireSingleBullet();
    }

    private void FireSingleBullet()
    {
        currentWeapon.bulletsInMagazine--;

        GameObject newBullet = ObjectPool.instance.GetBulletFromPool();

        newBullet.transform.position = GunPoint().position;
        newBullet.transform.rotation = Quaternion.LookRotation(GunPoint().forward);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.BulletSetup(currentWeapon.gunDistance);

        Vector3 bulletsDirection = currentWeapon.ApplySpread(BulletDirection());

        rbNewBullet.mass = REFERENCE_BULLET_SPEED / bulletSpeed; // Adjust mass based on speed to keep momentum consistent
        rbNewBullet.linearVelocity = bulletsDirection * bulletSpeed;
    }

    private void Reload()
    {
        SetWeaponReady(false);
        player.weaponVisuals.PlayReloadAnimation();
    }

    public Vector3 BulletDirection()
    {
        Transform aim = player.aim.Aim();

        Vector3 direction = (aim.position - GunPoint().position).normalized;

        if (player.aim.CanAimPrecisely() == false && player.aim.Target() == null)
            direction.y = 0; // Keep the bullet level



        return direction;
    }

    public bool hasHasOnlyOneWeapon() => weaponSlots.Count <= 1;
    public Weapon WeaponInSlots(WeaponType weaponType)
    {
        foreach (Weapon weapon in weaponSlots) {
            if (weapon.weaponType == weaponType) return weapon;
        }

        return null;
    }

    public Weapon CurrentWeapon() => currentWeapon;


    public Transform GunPoint() => player.weaponVisuals.CurrentWeaponModel().gunPoint;


    #region Input Events
    private void AssignInputEvents()
    {
        PlayerControls controls = player.controls;

        controls.Character.Fire.performed += ctx => isShooting = true;
        controls.Character.Fire.canceled += ctx => isShooting = false;

        controls.Character.EquipSlot1.performed += ctx => EquipWeapon(0);
        controls.Character.EquipSlot2.performed += ctx => EquipWeapon(1);
        controls.Character.EquipSlot3.performed += ctx => EquipWeapon(2);
        controls.Character.EquipSlot4.performed += ctx => EquipWeapon(3);
        controls.Character.EquipSlot5.performed += ctx => EquipWeapon(4);
        controls.Character.DropCurrentWeapon.performed += ctx => DropWeapon();

        controls.Character.Reload.performed += ctx =>
        {
            if (currentWeapon.CanReload() && IsWeaponReady()) {
                Reload();
            }
        };

        controls.Character.ToggleWeaponMode.performed += ctx => currentWeapon.ToggleBurstMode();

    }


    #endregion
}
