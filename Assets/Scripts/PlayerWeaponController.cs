using UnityEngine;

public class PlayerWeaponController : MonoBehaviour {
    private Player player;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Transform gunPoint;

    private void Start() {
        player = GetComponent<Player>();

        player.controls.Character.Fire.performed += ctx => Shoot();
    }
    private void Shoot() {
        GameObject newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.LookRotation(gunPoint.forward));

        newBullet.GetComponent<Rigidbody>().linearVelocity = gunPoint.forward * bulletSpeed;

        Destroy(newBullet, 5);

        GetComponentInChildren<Animator>().SetTrigger("Fire");
        Debug.Log("Pew Pew");
    }
}
