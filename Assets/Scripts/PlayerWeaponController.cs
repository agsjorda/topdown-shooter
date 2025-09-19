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

    private void Start() {
        player = GetComponent<Player>();
        player.controls.Character.Fire.performed += ctx => Shoot();

        currentWeapon.ammo = currentWeapon.maxAmmo;
    }
    private void Shoot() {

        currentWeapon.ammo--;

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

    public Transform GunPoint() => gunPoint;

}
