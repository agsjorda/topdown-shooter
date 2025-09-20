using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour {
    private Player player;

    private const float REFERENCE_BULLET_SPEED = 20f;
    //Default speed from which our mass fromula is derived

    [SerializeField] private Weapon currentWeapon;

    [Header("Bullet Details")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Transform gunPoint;

    [SerializeField] private Transform weaponHolder;

    [Header("Inventory")]
    [SerializeField] private int maxSlots = 2;
    [SerializeField] private List<Weapon> weaponSlots;

    private void Start() {
        player = GetComponent<Player>();
        AssignInputEvents();

        currentWeapon.bulletsInMagazine = currentWeapon.totalReserveAmmo;
    }


    #region Slots Management - Equip, Drop, Pickup
    private void EquipWeapon(int i) => currentWeapon = weaponSlots[i];

    public void PickupWeapon(Weapon newWeapon) {

        if (weaponSlots.Count >= maxSlots) {
            Debug.Log("Can't carry more than 2 weapons!");
            return;
        }
        weaponSlots.Add(newWeapon);
        currentWeapon = newWeapon;
    }

    private void DropWeapon() {
        if (weaponSlots.Count <= 1) {
            Debug.Log("Can't drop last weapon!");
            return;
        }

        weaponSlots.Remove(currentWeapon);

        currentWeapon = weaponSlots[0];
    }
    #endregion
    private void Shoot() {

        if (currentWeapon.CanShoot() == false)
            return;

        GameObject newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.LookRotation(gunPoint.forward));

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        rbNewBullet.mass = REFERENCE_BULLET_SPEED / bulletSpeed; // Adjust mass based on speed to keep momentum consistent
        rbNewBullet.linearVelocity = BulletDirection() * bulletSpeed;

        Destroy(newBullet, 5);

        GetComponentInChildren<Animator>().SetTrigger("Fire");
        Debug.Log("Pew Pew");
    }

    public Vector3 BulletDirection() {
        Transform aim = player.aim.Aim();

        Vector3 direction = (aim.position - gunPoint.position).normalized;

        if (player.aim.CanAimPrecisely() == false && player.aim.Target() == null)
            direction.y = 0; // Keep the bullet level

        //weaponHolder.LookAt(aim);
        //gunPoint.LookAt(aim); TODO: find a better place to put this

        return direction;
    }

    public Weapon CurrentWeapon() => currentWeapon;

    public Transform GunPoint() => gunPoint;


    #region Input Events
    private void AssignInputEvents() {
        PlayerControls controls = player.controls;

        controls.Character.Fire.performed += ctx => Shoot();

        controls.Character.EquipSlot1.performed += ctx => EquipWeapon(0);
        controls.Character.EquipSlot2.performed += ctx => EquipWeapon(1);
        controls.Character.DropCurrentWeapon.performed += ctx => DropWeapon();

        controls.Character.Reload.performed += ctx => {
            if (currentWeapon.CanReload()) {
                player.weaponVisuals.PlayReloadAnimation();
            }
        };
    }
    #endregion
}
