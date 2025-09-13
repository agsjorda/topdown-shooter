using UnityEngine;

public class PlayerWeaponController : MonoBehaviour {
    private Player player;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Transform gunPoint;


    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aim;

    private void Start() {
        player = GetComponent<Player>();

        player.controls.Character.Fire.performed += ctx => Shoot();
    }
    private void Shoot() {


        GameObject newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.LookRotation(gunPoint.forward));

        newBullet.GetComponent<Rigidbody>().linearVelocity = BulletDirection() * bulletSpeed;

        Destroy(newBullet, 5);

        GetComponentInChildren<Animator>().SetTrigger("Fire");
        Debug.Log("Pew Pew");
    }

    private Vector3 BulletDirection() {
        Vector3 direction = (aim.position - gunPoint.position).normalized;

        if (player.aim.CanAimPrecisely() == false)
            direction.y = 0; // Keep the bullet level

        weaponHolder.LookAt(aim);
        gunPoint.LookAt(aim);

        return direction;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawLine(weaponHolder.position, weaponHolder.position + weaponHolder.forward * 25);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(gunPoint.position, gunPoint.position + BulletDirection() * 25);
    }
}
